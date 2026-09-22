//──────────────────────────────────────────────────────────────
// UDPInputSender.cs
// CONTROLLER APK side (RemoteController scene).
//
// Steering modes (set via SetSteerMode):
//   Joystick     – touch-drag analog zone (default)
//   Gyroscope    – device rotation rate, calibrated on demand
//   Accelerometer – gravity-tilt (legacy, kept for devices
//                   without a gyro)
//
// Other inputs:
//   Throttle / Brake / HandBrake / Nitro – hold buttons
//──────────────────────────────────────────────────────────────

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace ALIyerEdon.RemoteInput
{
    public enum SteerMode { Joystick, Gyroscope, Accelerometer }

    public class UDPInputSender : MonoBehaviour
    {
        // ── Network ───────────────────────────────────────────
        [Header("Network")]
        public string targetIP   = "192.168.1.100";
        public int    targetPort = 5555;
        [Range(10, 60)]
        public int    sendRate   = 30;
        [Tooltip("True only when the phone has transitioned to the driving screen.")]
        public bool   isDrivingActive = false;

        // ── UI references (wired by RemoteControllerUI) ───────
        [Header("UI References")]
        public TMP_InputField ipInputField;
        public TMP_Text       statusText;
        public TMP_Text       debugText;

        // ── Steering ──────────────────────────────────────────
        [Header("Steering")]
        public SteerMode steerMode = SteerMode.Joystick;

        // Joystick
        [Header("Joystick")]
        public RectTransform joystickBackground;
        public RectTransform joystickHandle;

        // Gyroscope
        [Header("Gyroscope")]
        [Tooltip("Maximum roll angle (degrees) that maps to full steer.")]
        [Range(10f, 60f)]
        public float gyroMaxAngle      = 30f;

        [Tooltip("Angles smaller than this are treated as zero (prevents drift).")]
        [Range(0f, 5f)]
        public float gyroDeadzone      = 2f;

        [Tooltip("Low-pass filter strength. Higher = smoother but slower response.")]
        [Range(0f, 0.95f)]
        public float gyroSmoothing     = 0.15f;

        // Accelerometer (legacy)
        [Header("Accelerometer (legacy)")]
        public bool  useAccelerometer  = false;   // kept for ToggleAccelerometer compat
        [Range(1f, 5f)]
        public float accelSensitivity  = 2.5f;
        [Range(0f, 0.3f)]
        public float accelDeadzone     = 0.15f;

        // ── Runtime state (read-only, used by UI overlay) ─────
        [HideInInspector] public float RawGyroAngle;      // degrees, post-calibration
        [HideInInspector] public float SmoothedSteer;     // -1…+1 output
        [HideInInspector] public bool  GyroAvailable;

        // ── Private ───────────────────────────────────────────
        UdpClient  _udpClient;
        IPEndPoint _endPoint;

        bool _throttleHeld;
        bool _brakeHeld;
        bool _handBrakeHeld;
        bool _nitroHeld;

        // Joystick
        bool    _joystickActive;
        Vector2 _joystickOrigin;
        float   _joystickRadius;

        // Gyroscope
        Quaternion _gyroCalibrationOffset = Quaternion.identity;
        bool       _gyroCalibrated;
        float      _gyroSteerSmoothed;

        float _steerValue;
        float _motorValue;
        float _sendInterval;
        float _sendTimer;

        // ── Lifecycle ─────────────────────────────────────────
        void Start()
        {
            _sendInterval = 1f / sendRate;

            // Restore saved IP
            if (ipInputField != null)
            {
                string saved = PlayerPrefs.GetString("RemoteTargetIP", targetIP);
                ipInputField.text = saved;
                targetIP = saved;
                ipInputField.onEndEdit.AddListener(OnIPChanged);
            }

            // Restore saved mode
            steerMode = (SteerMode)PlayerPrefs.GetInt("SteerMode", (int)SteerMode.Joystick);

            // Check gyro availability (New Input System — legacy UnityEngine.Input is
            // disabled by this project's Active Input Handling setting).
            GyroAvailable = AttitudeSensor.current != null;
            if (GyroAvailable)
            {
                UnityEngine.InputSystem.InputSystem.EnableDevice(AttitudeSensor.current);
                // Auto-calibrate on first launch
                CalibrateGyro();
            }
            else if (steerMode == SteerMode.Gyroscope)
            {
                // Fallback to accelerometer if device has no gyro
                steerMode = SteerMode.Accelerometer;
                Debug.LogWarning("[UDPInputSender] Gyroscope not available — falling back to Accelerometer.");
            }
            if (Accelerometer.current != null)
                UnityEngine.InputSystem.InputSystem.EnableDevice(Accelerometer.current);

            OpenSocket();
        }

        void OnDestroy()         => CloseSocket();
        void OnApplicationQuit() => CloseSocket();

        // ── Update ────────────────────────────────────────────
        void Update()
        {
            if (!isDrivingActive) return;

            BuildInputValues();

            _sendTimer += Time.deltaTime;
            if (_sendTimer >= _sendInterval)
            {
                _sendTimer = 0f;
                SendPacket();
            }

            SmoothedSteer = _steerValue;

            if (debugText != null)
            {
                string modeStr = steerMode == SteerMode.Gyroscope
                    ? $"GYRO {RawGyroAngle:F1}°"
                    : steerMode.ToString();
                debugText.text =
                    $"M:{_motorValue:F2}  S:{_steerValue:F2}  [{modeStr}]\n" +
                    $"HB:{_handBrakeHeld}  N:{_nitroHeld}  → {targetIP}:{targetPort}";
            }
        }

        // ── Build motor / steer ───────────────────────────────
        void BuildInputValues()
        {
            // Motor
            _motorValue = _throttleHeld ? 1f : _brakeHeld ? -1f : 0f;

            // Steer — Joystick mode is updated by pointer events (OnJoystickDown/
            // Drag/Up), Gyroscope/Accelerometer are sampled here every frame.
            switch (steerMode)
            {
                case SteerMode.Gyroscope:
                    _steerValue = SampleGyro();
                    break;
                case SteerMode.Accelerometer:
                    _steerValue = SampleAccelerometer();
                    break;
            }
        }

        // ── Gyroscope steering ────────────────────────────────
        //
        // Strategy: read the device's current attitude (absolute
        // orientation), apply the calibration offset so that
        // "neutral" is whatever the phone was pointing at when
        // CalibrateGyro() was called, then extract the roll
        // component (tilt left/right) and map it to -1…+1.
        //
        // The New Input System's AttitudeSensor gives the same attitude
        // Quaternion the legacy Input.gyro.attitude used to, in the same
        // right-handed space where:
        //   x = roll (tilt left/right when held in landscape)
        //   y = yaw
        //   z = pitch
        // We re-map to world-space roll using the calibration
        // inverse so the output is always relative to neutral.
        float SampleGyro()
        {
            if (!GyroAvailable || AttitudeSensor.current == null) return 0f;

            // Convert Unity gyro space → world Quaternion
            Quaternion raw        = AttitudeSensor.current.attitude.ReadValue();
            Quaternion worldSpace = GyroToWorld(raw);

            // Apply calibration: remove neutral orientation
            Quaternion relative   = Quaternion.Inverse(_gyroCalibrationOffset) * worldSpace;

            // Extract roll angle (rotation around the forward/z axis in landscape)
            // eulerAngles.z gives us the roll in 0-360°; remap to -180…+180
            float rollDeg = relative.eulerAngles.z;
            if (rollDeg > 180f) rollDeg -= 360f;

            RawGyroAngle = rollDeg;

            // Apply deadzone
            float absRoll = Mathf.Abs(rollDeg);
            if (absRoll < gyroDeadzone) rollDeg = 0f;

            // Map to -1…+1
            float target = Mathf.Clamp(rollDeg / gyroMaxAngle, -1f, 1f);

            // Low-pass smooth
            _gyroSteerSmoothed = Mathf.Lerp(target, _gyroSteerSmoothed, gyroSmoothing);

            return _gyroSteerSmoothed;
        }

        // Unity gyro returns a right-handed Quaternion; convert to
        // Unity's left-handed world space (same transform every app uses)
        static Quaternion GyroToWorld(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        // ── Accelerometer (legacy fallback) ───────────────────
        float SampleAccelerometer()
        {
            if (Accelerometer.current == null) return 0f;
            float tilt = Accelerometer.current.acceleration.ReadValue().x;
            if (Mathf.Abs(tilt) < accelDeadzone) tilt = 0f;
            return Mathf.Clamp(tilt * accelSensitivity, -1f, 1f);
        }

        // ── Joystick pointer/touch tracking ───────────────────
        // Wired from RemoteControllerUI via an EventTrigger on the joystick
        // zone (PointerDown/Drag/PointerUp), so it works with touch AND mouse
        // through the UI event system instead of polling the legacy Input class.
        public void OnJoystickDown(Vector2 screenPos)
        {
            if (joystickBackground == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickBackground, screenPos, null, out Vector2 lp))
            {
                _joystickActive = true;
                _joystickOrigin = lp;
            }
        }

        public void OnJoystickDrag(Vector2 screenPos)
        {
            if (!_joystickActive || joystickBackground == null) return;

            if (_joystickRadius <= 0f)
                _joystickRadius = joystickBackground.rect.width * 0.5f;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickBackground, screenPos, null, out Vector2 lp))
                return;

            Vector2 delta   = lp - _joystickOrigin;
            float   maxDrag = _joystickRadius * 0.85f;
            Vector2 clamped = Vector2.ClampMagnitude(delta, maxDrag);

            if (joystickHandle != null)
                joystickHandle.anchoredPosition = clamped;

            _steerValue = Mathf.Clamp(delta.x / maxDrag, -1f, 1f);
        }

        public void OnJoystickUp()
        {
            _joystickActive = false;
            _steerValue     = 0f;
            if (joystickHandle != null)
                joystickHandle.anchoredPosition = Vector2.zero;
        }

        // ── Send packet ───────────────────────────────────────
        void SendPacket()
        {
            // Resync the endpoint if the IP field was edited without tapping CONNECT.
            if (ipInputField != null)
            {
                string current = ipInputField.text.Trim();
                if (!string.IsNullOrEmpty(current) && current != targetIP)
                    OnIPChanged(current);
            }

            if (_udpClient == null || _endPoint == null) return;

            byte[] bytes = new RemoteInputData
            {
                motor     = _motorValue,
                steer     = _steerValue,
                handBrake = _handBrakeHeld,
                nitro     = _nitroHeld
            }.Serialize();

            try   { _udpClient.Send(bytes, bytes.Length, _endPoint); }
            catch (Exception e) { Debug.LogWarning($"[UDPInputSender] {e.Message}"); }
        }

        // ── Socket ────────────────────────────────────────────
        void OpenSocket()
        {
            try
            {
                _udpClient = new UdpClient();
                _udpClient.EnableBroadcast = false;
                _endPoint = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
                if (statusText != null)
                    statusText.text = $"Ready → {targetIP}:{targetPort}";
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDPInputSender] {e.Message}");
                if (statusText != null) statusText.text = "Error – check IP";
            }
        }

        void CloseSocket()
        {
            try { _udpClient?.Close(); } catch { /* ignore */ }
            _udpClient = null;
        }

        void OnIPChanged(string newIP)
        {
            newIP = newIP.Trim();
            if (string.IsNullOrEmpty(newIP)) return;
            if (newIP.Contains(":"))
                newIP = newIP.Split(':')[0].Trim();

            targetIP = newIP;
            if (ipInputField != null && ipInputField.text != targetIP)
                ipInputField.text = targetIP;

            PlayerPrefs.SetString("RemoteTargetIP", targetIP);
            CloseSocket();
            OpenSocket();

            var garage = GetComponent<GarageControllerUI>() ?? FindFirstObjectByType<GarageControllerUI>();
            if (garage != null && garage.targetIP != targetIP)
            {
                garage.SetTargetIP(targetIP);
            }
        }

        public void SetTargetIP(string newIP)
        {
            newIP = newIP.Trim();
            if (string.IsNullOrEmpty(newIP)) return;
            if (newIP.Contains(":"))
                newIP = newIP.Split(':')[0].Trim();

            if (newIP == targetIP) return;
            targetIP = newIP;
            if (ipInputField != null) ipInputField.text = targetIP;
            CloseSocket();
            OpenSocket();
        }

        public void SetDrivingActive(bool active)
        {
            isDrivingActive = active;
            if (!active)
            {
                _throttleHeld = _brakeHeld = _handBrakeHeld = _nitroHeld = false;
                _steerValue = 0f;
                _motorValue = 0f;
                if (joystickHandle != null)
                    joystickHandle.anchoredPosition = Vector2.zero;
            }
        }

        // ── Public API ────────────────────────────────────────

        /// <summary>
        /// Sets the current steering mode and persists the choice.
        /// Called by the UI mode-selector buttons.
        /// </summary>
        public void SetSteerMode(int mode)
        {
            steerMode = (SteerMode)mode;
            PlayerPrefs.SetInt("SteerMode", mode);

            if (steerMode == SteerMode.Gyroscope)
            {
                if (GyroAvailable)
                {
                    if (AttitudeSensor.current != null)
                        UnityEngine.InputSystem.InputSystem.EnableDevice(AttitudeSensor.current);
                    CalibrateGyro();
                }
                else
                {
                    // Silently downgrade — the UI shows "N/A" for gyro
                    steerMode = SteerMode.Accelerometer;
                    Debug.LogWarning("[UDPInputSender] Gyro not available.");
                }
            }

            // Hide joystick zone when not in joystick mode
            if (joystickBackground != null)
                joystickBackground.gameObject.SetActive(steerMode == SteerMode.Joystick);

            if (statusText != null)
                statusText.text = $"Steer: {steerMode}";
        }

        /// <summary>
        /// Records the current device orientation as the neutral/zero point.
        /// Call this whenever the driver is holding the phone in their
        /// preferred driving position.
        /// </summary>
        public void CalibrateGyro()
        {
            if (!GyroAvailable || AttitudeSensor.current == null) return;
            _gyroCalibrationOffset = GyroToWorld(AttitudeSensor.current.attitude.ReadValue());
            _gyroSteerSmoothed     = 0f;
            _gyroCalibrated        = true;
            PlayerPrefs.SetString("GyroCal",
                $"{_gyroCalibrationOffset.x},{_gyroCalibrationOffset.y}" +
                $",{_gyroCalibrationOffset.z},{_gyroCalibrationOffset.w}");
            Debug.Log("[UDPInputSender] Gyro calibrated.");
        }

        // Convenience wrapper for UI Toggle (backward-compat)
        public void ToggleAccelerometer(bool value)
        {
            SetSteerMode(value ? (int)SteerMode.Accelerometer : (int)SteerMode.Joystick);
        }

        // Button callbacks
        public void ThrottlePress()    => _throttleHeld   = true;
        public void ThrottleRelease()  => _throttleHeld   = false;
        public void BrakePress()       => _brakeHeld      = true;
        public void BrakeRelease()     => _brakeHeld      = false;
        public void HandBrakePress()   => _handBrakeHeld  = true;
        public void HandBrakeRelease() => _handBrakeHeld  = false;
        public void NitroPress()       => _nitroHeld      = true;
        public void NitroRelease()     => _nitroHeld      = false;

        // Legacy discrete steer (kept for backward compat)
        bool _steerLeftHeld, _steerRightHeld;
        public void SteerLeftPress()    => _steerLeftHeld  = true;
        public void SteerLeftRelease()  => _steerLeftHeld  = false;
        public void SteerRightPress()   => _steerRightHeld = true;
        public void SteerRightRelease() => _steerRightHeld = false;

        public void Connect()
        {
            if (ipInputField != null) OnIPChanged(ipInputField.text);
        }
    }
}
