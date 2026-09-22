//──────────────────────────────────────────────────────────────
// UDPInputSender.cs
// CONTROLLER APK side (RemoteController scene).
//
// NOTE: despite the class name (kept unchanged so existing scene
// references don't break), this no longer sends raw UDP packets.
// It joins a "room" on the relay server (see RelayLink.cs /
// RelayServer/) using a short room code instead of the game
// device's local IP address, and sends drive input as JSON over
// a WebSocket. This lets the controller reach a WebGL-hosted game
// over the open internet, not just a shared Wi-Fi network — and
// browsers cannot open a raw UDP socket at all.
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
        [Tooltip("Room code shown on the game's screen. Both devices must use the same code.")]
        public string roomCode = "";
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
        RelayLink _link;

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

            // Restore saved room code
            if (ipInputField != null)
            {
                string saved = PlayerPrefs.GetString("RemoteRoomCode", roomCode);
                ipInputField.text = saved;
                roomCode = saved;
                ipInputField.onEndEdit.AddListener(OnRoomCodeChanged);
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

            ConnectRelay();
        }

        void OnDestroy()         => _link?.Disconnect();
        void OnApplicationQuit() => _link?.Disconnect();

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
                string peer = _link != null && _link.PeerConnected ? "linked" : "waiting";
                debugText.text =
                    $"M:{_motorValue:F2}  S:{_steerValue:F2}  [{modeStr}]\n" +
                    $"HB:{_handBrakeHeld}  N:{_nitroHeld}  → room {roomCode} ({peer})";
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
        float SampleGyro()
        {
            if (!GyroAvailable || AttitudeSensor.current == null) return 0f;

            Quaternion raw        = AttitudeSensor.current.attitude.ReadValue();
            Quaternion worldSpace = GyroToWorld(raw);
            Quaternion relative   = Quaternion.Inverse(_gyroCalibrationOffset) * worldSpace;

            float rollDeg = relative.eulerAngles.z;
            if (rollDeg > 180f) rollDeg -= 360f;

            RawGyroAngle = rollDeg;

            float absRoll = Mathf.Abs(rollDeg);
            if (absRoll < gyroDeadzone) rollDeg = 0f;

            float target = Mathf.Clamp(rollDeg / gyroMaxAngle, -1f, 1f);
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
            // Resync the room if the code field was edited without tapping CONNECT.
            if (ipInputField != null)
            {
                string current = ipInputField.text.Trim();
                if (!string.IsNullOrEmpty(current) && current != roomCode)
                    OnRoomCodeChanged(current);
            }

            if (_link == null) return;

            var msg = new RemoteInputData
            {
                t         = "drive",
                motor     = _motorValue,
                steer     = _steerValue,
                handBrake = _handBrakeHeld,
                nitro     = _nitroHeld
            };
            _link.SendJson(JsonUtility.ToJson(msg));
        }

        // ── Relay connection ──────────────────────────────────
        void ConnectRelay()
        {
            if (_link == null)
                _link = GetComponent<RelayLink>() ?? gameObject.AddComponent<RelayLink>();

            if (string.IsNullOrEmpty(roomCode))
            {
                if (statusText != null) statusText.text = "Enter a room code";
                return;
            }

            _link.Connect(roomCode, "controller");
            if (statusText != null)
                statusText.text = $"Connecting → room {roomCode}";
        }

        void OnRoomCodeChanged(string newCode)
        {
            newCode = newCode.Trim();
            if (string.IsNullOrEmpty(newCode)) return;

            roomCode = newCode;
            if (ipInputField != null && ipInputField.text != roomCode)
                ipInputField.text = roomCode;

            PlayerPrefs.SetString("RemoteRoomCode", roomCode);
            ConnectRelay();

            var garage = GetComponent<GarageControllerUI>() ?? FindFirstObjectByType<GarageControllerUI>();
            if (garage != null && garage.roomCode != roomCode)
            {
                garage.SetRoomCode(roomCode);
            }
        }

        public void SetRoomCode(string newCode)
        {
            newCode = newCode.Trim();
            if (string.IsNullOrEmpty(newCode) || newCode == roomCode) return;

            roomCode = newCode;
            if (ipInputField != null) ipInputField.text = roomCode;
            ConnectRelay();
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
            if (ipInputField != null) OnRoomCodeChanged(ipInputField.text);
        }
    }
}
