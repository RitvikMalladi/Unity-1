//──────────────────────────────────────────────────────────────
// CarInputAdapter.cs
// GAME APK side — attach to the Player car GameObject.
//
// The single authoritative caller of EasyCarController.Move().
// Nothing else in the project should call Move() directly.
//
// Input source priority (highest wins):
//   1. RemoteInputManager  — if connected
//   2. Fallback source     — any ICarInputSource assigned in the
//                            Inspector (e.g. a local AI wrapper)
//   3. Zero input          — car coasts to a stop
//
// EasyCarController, Car_AI, and InputSystem are NEVER modified.
// InputSystem and Car_AI are disabled when remote input is active
// to prevent conflicting writes to throttleInput / steerInput.
//──────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ALIyerEdon;

namespace ALIyerEdon.RemoteInput
{
    public class CarInputAdapter : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Input Sources")]
        [Tooltip("Leave empty – resolved automatically from RemoteInputManager.Instance at runtime.")]
        public RemoteInputManager remoteManager;

        [Tooltip("Optional fallback when remote is not connected (e.g. a local AI wrapper).")]
        public MonoBehaviour fallbackSource;       // must implement ICarInputSource

        [Header("Behaviour")]
        [Tooltip("When true the adapter is always active even if RemoteInputManager is absent.")]
        public bool alwaysActive = true;

        [Tooltip("When remote connects/disconnects, disable/enable InputSystem and Car_AI automatically.")]
        public bool manageLocalInputComponents = true;

        [Header("HUD (optional)")]
        [Tooltip("Text element updated each frame with connection status.")]
        public Text statusText;

        // ── Private ──────────────────────────────────────────────
        EasyCarController _controller;
        InputSystem       _localInput;
        Car_AI            _carAI;
        ICarInputSource   _fallback;

        bool _remoteWasActive;

        // ────────────────────────────────────────────────────────
        IEnumerator Start()
        {
            _controller = GetComponent<EasyCarController>();

            if (_controller == null)
            {
                Debug.LogError("[CarInputAdapter] No EasyCarController found on this GameObject. " +
                               "Disabling adapter.");
                enabled = false;
                yield break;
            }

            _localInput = GetComponent<InputSystem>() ?? FindFirstObjectByType<InputSystem>();
            _carAI      = GetComponent<Car_AI>();

            // Resolve fallback ICarInputSource
            if (fallbackSource != null)
            {
                _fallback = fallbackSource as ICarInputSource;
                if (_fallback == null)
                    Debug.LogWarning("[CarInputAdapter] fallbackSource does not implement " +
                                     "ICarInputSource – it will be ignored.");
            }

            // Resolve RemoteInputManager — wait a frame in case it Start()s after us
            yield return null;
            if (remoteManager == null)
                remoteManager = RemoteInputManager.Instance;

            if (remoteManager == null && alwaysActive)
                Debug.LogWarning("[CarInputAdapter] No RemoteInputManager found in scene. " +
                                 "Adapter will use fallback / zero input only.");

            // Subscribe to connect/disconnect events
            RemoteInputManager.OnControllerConnected    += OnRemoteConnected;
            RemoteInputManager.OnControllerDisconnected += OnRemoteDisconnected;

            // If already connected when this adapter starts:
            if (remoteManager != null && remoteManager.IsConnected)
            {
                OnRemoteConnected(remoteManager.RemoteIP);
            }
        }

        void OnDestroy()
        {
            RemoteInputManager.OnControllerConnected    -= OnRemoteConnected;
            RemoteInputManager.OnControllerDisconnected -= OnRemoteDisconnected;
        }

        // ── Every frame: choose source, call Move() ──────────────
        void Update()
        {
            if (_controller == null) return;

            // Re-resolve manager if it arrived late (e.g. DontDestroyOnLoad)
            if (remoteManager == null)
                remoteManager = RemoteInputManager.Instance;

            if (_localInput == null)
                _localInput = GetComponent<InputSystem>() ?? FindFirstObjectByType<InputSystem>();

            ICarInputSource source = ChooseSource();

            if (source != null)
            {
                // Ensure local input is disabled when remote is driving so it doesn't overwrite Move() with zeros
                if (manageLocalInputComponents && _localInput != null)
                {
                    if (_localInput.enabled)
                        _localInput.enabled = false;
                    _localInput.canControl = false;
                    if (_localInput.joystick  != null && _localInput.joystick.activeSelf)
                        _localInput.joystick.SetActive(false);
                    if (_localInput.arrowKeys != null && _localInput.arrowKeys.activeSelf)
                        _localInput.arrowKeys.SetActive(false);
                }

                // If the race is sitting at the start screen and waiting, kick it off
                var raceMgr = FindFirstObjectByType<Race_Manager>();
                if (raceMgr != null && !raceMgr.raceStarted)
                {
                    raceMgr.StartRace_Button();
                }

                // Disengage clutch so motor torque and steering are applied
                if (_controller.Clutch)
                {
                    _controller.Clutch = false;
                }

                float motor     = source.Motor;
                float steer     = source.Steer;
                bool  handBrake = source.HandBrake;
                _controller.Move(motor, steer, handBrake);
            }

            UpdateHUD(source);
        }

        // ── Source selection logic ────────────────────────────────
        ICarInputSource ChooseSource()
        {
            // Priority 1: remote manager when connected
            if (remoteManager != null && remoteManager.IsActive)
                return remoteManager;

            // Priority 2: explicit fallback source
            if (_fallback != null && _fallback.IsActive)
                return _fallback;

            // Priority 3: null (allow local InputSystem to control the car)
            return null;
        }

        // ── React to remote connect / disconnect ─────────────────
        void OnRemoteConnected(string ip)
        {
            // The race waits for a "tap to start" trigger that only exists for local
            // touchscreen play; the phone has no way to reach it, so kick it off here.
            FindFirstObjectByType<Race_Manager>()?.StartRace_Button();

            if (_controller != null)
                _controller.Clutch = false;

            if (!manageLocalInputComponents) return;

            if (_localInput == null)
                _localInput = GetComponent<InputSystem>() ?? FindFirstObjectByType<InputSystem>();

            // Disable local input so it doesn't fight remote
            if (_localInput != null)
            {
                _localInput.enabled = false;
                _localInput.canControl = false;
                if (_localInput.joystick  != null) _localInput.joystick.SetActive(false);
                if (_localInput.arrowKeys != null) _localInput.arrowKeys.SetActive(false);
            }
            if (_carAI != null) _carAI.enabled = false;

            _remoteWasActive = true;
            Debug.Log($"[CarInputAdapter] Remote connected ({ip}) — local input disabled.");
        }

        void OnRemoteDisconnected()
        {
            if (!manageLocalInputComponents) return;
            if (!_remoteWasActive) return;

            if (_localInput == null)
                _localInput = GetComponent<InputSystem>() ?? FindFirstObjectByType<InputSystem>();

            // Re-enable local input when remote drops
            if (_localInput != null)
            {
                _localInput.enabled = true;
                _localInput.canControl = true;
                if (_localInput.controlType == InputType.Mobile)
                {
                    if (PlayerPrefs.GetInt("ControlType") == 0 && _localInput.arrowKeys != null)
                        _localInput.arrowKeys.SetActive(true);
                    else if (PlayerPrefs.GetInt("ControlType") == 1 && _localInput.joystick != null)
                        _localInput.joystick.SetActive(true);
                }
            }
            // Do NOT re-enable Car_AI — the player car should not go autonomous
            // when remote drops; it should coast. Re-enable manually if desired.

            _remoteWasActive = false;
            Debug.Log("[CarInputAdapter] Remote disconnected — local InputSystem re-enabled.");
        }

        // ── HUD update ────────────────────────────────────────────
        void UpdateHUD(ICarInputSource activeSource)
        {
            if (statusText == null) return;

            if (remoteManager != null)
                statusText.text = remoteManager.GetStatusString();
            else if (activeSource != null)
                statusText.text = $"Local  M:{activeSource.Motor:F2}  S:{activeSource.Steer:F2}";
            else
                statusText.text = "No input";
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>
        /// Force-set an input source at runtime (e.g. from a network lobby).
        /// Pass null to clear the override and return to priority logic.
        /// </summary>
        public void SetRemoteManager(RemoteInputManager manager)
        {
            remoteManager = manager;
        }

        /// <summary>Current active source name, for diagnostics.</summary>
        public string ActiveSourceName
        {
            get
            {
                var src = ChooseSource();
                if (src == null) return "None";
                if (src is RemoteInputManager) return "Remote";
                return src.GetType().Name;
            }
        }
    }
}
