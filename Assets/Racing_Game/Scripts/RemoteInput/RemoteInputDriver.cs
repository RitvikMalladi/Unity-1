//──────────────────────────────────────────────────────────────
// RemoteInputDriver.cs
//
// SUPERSEDED by CarInputAdapter + RemoteInputManager.
// Kept for backward compatibility with any existing scene
// references.  New code should use CarInputAdapter instead.
//──────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using ALIyerEdon;
using ALIyerEdon.RemoteInput;

namespace ALIyerEdon.RemoteInput
{
    public class RemoteInputDriver : MonoBehaviour
    {
        [Header("Optional HUD")]
        [Tooltip("Assign a UI Text / TMP_Text component to show connection status & local IP.")]
        public Text statusText;            // swap for TMP_Text if you use TextMeshPro

        [Header("Smoothing")]
        [Tooltip("Smoothing applied to motor input to avoid jerky acceleration.")]
        [Range(0f, 20f)]
        public float motorSmoothing  = 8f;

        [Tooltip("Smoothing applied to steer input.")]
        [Range(0f, 20f)]
        public float steerSmoothing  = 12f;

        // ── Private ─────────────────────────────────────────────
        EasyCarController  _controller;
        UDPInputReceiver   _receiver;

        float _smoothMotor;
        float _smoothSteer;

        // ────────────────────────────────────────────────────────
        void Start()
        {
            _controller = GetComponent<EasyCarController>();
            _receiver   = GetComponent<UDPInputReceiver>();

            // Disable local input sources so they don't conflict
            var localInput = GetComponent<InputSystem>();
            if (localInput != null)
            {
                localInput.enabled = false;
                Debug.Log("[RemoteInputDriver] InputSystem disabled – using remote input.");
            }

            var ai = GetComponent<Car_AI>();
            if (ai != null)
            {
                ai.enabled = false;
                Debug.Log("[RemoteInputDriver] Car_AI disabled – using remote input.");
            }

            // Show the local IP so the user knows what to type on the controller phone
            string localIP = UDPInputReceiver.LocalIPAddress();
            Debug.Log($"[RemoteInputDriver] Game device IP: {localIP}  Port: {_receiver.listenPort}");

            if (statusText != null)
                statusText.text = $"IP: {localIP}:{_receiver.listenPort}\nWaiting for controller…";
        }

        // ── Every frame: pull latest packet → smooth → Move() ───
        void Update()
        {
            // Guard: components are added dynamically so receiver may not be set yet
            if (_receiver == null) _receiver = GetComponent<UDPInputReceiver>();
            if (_controller == null) _controller = GetComponent<EasyCarController>();
            if (_receiver == null || _controller == null) return;

            RemoteInputData input = _receiver.LatestInput;

            // Smooth motor and steer to avoid physics jitter from dropped packets
            float dt = Time.deltaTime;
            _smoothMotor = Mathf.Lerp(_smoothMotor, input.motor, dt * motorSmoothing);
            _smoothSteer = Mathf.Lerp(_smoothSteer, input.steer, dt * steerSmoothing);

            // Hand brake is boolean – no smoothing needed
            _controller.Move(_smoothMotor, _smoothSteer, input.handBrake);

            // Update HUD
            if (statusText != null)
            {
                if (_receiver.IsConnected)
                    statusText.text = $"Controller: {_receiver.RemoteIP}\n" +
                                      $"Motor: {_smoothMotor:F2}  Steer: {_smoothSteer:F2}";
                else
                    statusText.text = $"IP: {UDPInputReceiver.LocalIPAddress()}:{_receiver.listenPort}\n" +
                                      "Waiting for controller…";
            }
        }
    }
}
