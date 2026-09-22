//──────────────────────────────────────────────────────────────
// RemoteInputManager.cs  —  GAME APK side, scene singleton
//
// Responsibilities
//   • Owns UDPInputReceiver lifecycle
//   • Smooths raw input (motor, steer)
//   • Drives Nitro.cs via Apply/Release_Nitro()
//   • Implements ICarInputSource (feeds CarInputAdapter)
//   • Fires events: Connected / Disconnected / Reconnected
//   • Exposes full diagnostics from UDPInputReceiver:
//       State, PacketsPerSecond, PacketLoss,
//       EstimatedLatencyMs, ReconnectCount
//   • GetStatusString() / GetDiagnosticsString() for HUDs
//──────────────────────────────────────────────────────────────

using System;
using UnityEngine;

namespace ALIyerEdon.RemoteInput
{
    public class RemoteInputManager : MonoBehaviour, ICarInputSource
    {
        // ── Singleton ─────────────────────────────────────────
        public static RemoteInputManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────
        [Header("Network")]
        [Tooltip("Room code the controller phone must enter. Leave blank to auto-generate one per device (recommended).")]
        public string roomCode      = "";
        [Tooltip("Seconds of silence before connection is declared lost.")]
        public float timeoutSeconds = 2f;
        [Tooltip("Keep this manager alive across scene loads.")]
        public bool  persistAcrossScenes = false;

        [Header("Reconnect")]
        [Tooltip("Automatically reopen the relay connection after an error.")]
        public bool  autoReconnect        = true;
        [Tooltip("Seconds between reconnect attempts.")]
        [Range(0.5f, 10f)]
        public float reconnectDelay       = 2f;
        [Tooltip("Maximum reconnect attempts (0 = unlimited).")]
        public int   maxReconnectAttempts = 0;

        [Header("Input Smoothing")]
        [Range(1f, 30f)]
        public float motorSmoothing = 10f;
        [Range(1f, 30f)]
        public float steerSmoothing = 14f;

        [Header("Debug")]
        public bool verboseLog = false;

        // ── Events ────────────────────────────────────────────
        /// <summary>Fired (main thread) when a controller first sends a packet.</summary>
        public static event Action<string> OnControllerConnected;

        /// <summary>Fired (main thread) when the connection times out.</summary>
        public static event Action         OnControllerDisconnected;

        /// <summary>Fired (main thread) each time the socket is successfully reopened.</summary>
        public static event Action<int>    OnReconnected;   // arg = reconnect attempt #

        // ── ICarInputSource ───────────────────────────────────
        public float Motor     { get; private set; }
        public float Steer     { get; private set; }
        public bool  HandBrake { get; private set; }
        public bool  IsActive  => IsConnected;

        // ── Connection state ──────────────────────────────────
        public bool            IsConnected { get; private set; }
        public string          RemoteIP    { get; private set; } = "–";
        public ConnectionState State       => _receiver != null
                                             ? _receiver.State
                                             : ConnectionState.Disconnected;

        // ── Diagnostics (pass-through from UDPInputReceiver) ──
        public float PacketsPerSecond   => _receiver?.PacketsPerSecond   ?? 0f;
        public float PacketLoss         => _receiver?.PacketLoss         ?? 0f;
        public float EstimatedLatencyMs => _receiver?.EstimatedLatencyMs ?? 0f;
        public int   ReconnectCount     => _receiver?.ReconnectCount     ?? 0;
        public string LastError         => _receiver?.LastError          ?? "";

        // ── Extra public state ────────────────────────────────
        public RemoteInputData RawInput  { get; private set; }
        public bool            Nitro     { get; private set; }
        public string          LocalIP   => _receiver != null ? _receiver.roomCode : roomCode;

        // ── Private ───────────────────────────────────────────
        UDPInputReceiver _receiver;
        bool             _wasConnected;
        bool             _nitroPrev;
        int              _prevReconnectCount;
        ALIyerEdon.Nitro _nitroComponent;

        // ── Lifecycle ─────────────────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        }

        void OnEnable()  => EnsureReceiver();
        void OnDisable() { if (_receiver) _receiver.enabled = false; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        // ── Update ────────────────────────────────────────────
        void Update()
        {
            if (_receiver == null) return;

            RawInput    = _receiver.LatestInput;
            RemoteIP    = _receiver.RemoteIP;
            IsConnected = _receiver.IsConnected;

            float dt = Time.deltaTime;
            Motor     = Mathf.Lerp(Motor, RawInput.motor, dt * motorSmoothing);
            Steer     = Mathf.Lerp(Steer, RawInput.steer, dt * steerSmoothing);
            HandBrake = RawInput.handBrake;
            Nitro     = RawInput.nitro;

            DriveNitroComponent();
            FireConnectionEvents();
            DetectReconnect();
        }

        // ── Connection event dispatch ─────────────────────────
        void FireConnectionEvents()
        {
            if (IsConnected && !_wasConnected)
            {
                _wasConnected = true;
                if (verboseLog)
                    Debug.Log($"[RemoteInputManager] Connected from {RemoteIP}");
                OnControllerConnected?.Invoke(RemoteIP);
            }
            else if (!IsConnected && _wasConnected)
            {
                _wasConnected = false;
                Motor = Steer = 0f;
                HandBrake = Nitro = false;
                if (_nitroComponent != null) _nitroComponent.Release_Nitro();
                if (verboseLog)
                    Debug.Log("[RemoteInputManager] Disconnected.");
                OnControllerDisconnected?.Invoke();
            }
        }

        void DetectReconnect()
        {
            int current = ReconnectCount;
            if (current > _prevReconnectCount)
            {
                if (verboseLog)
                    Debug.Log($"[RemoteInputManager] Reconnected (attempt #{current})");
                OnReconnected?.Invoke(current);
                _prevReconnectCount = current;
            }
        }

        // ── Nitro passthrough ─────────────────────────────────
        void DriveNitroComponent()
        {
            if (_nitroComponent == null)
                _nitroComponent = FindFirstObjectByType<ALIyerEdon.Nitro>();
            if (_nitroComponent == null) return;

            if  (Nitro && !_nitroPrev) _nitroComponent.Apply_Nitro();
            else if (!Nitro &&  _nitroPrev) _nitroComponent.Release_Nitro();
            _nitroPrev = Nitro;
        }

        // ── Receiver setup ────────────────────────────────────
        void EnsureReceiver()
        {
            _receiver = GetComponent<UDPInputReceiver>();
            if (_receiver == null)
                _receiver = gameObject.AddComponent<UDPInputReceiver>();

            _receiver.roomCode            = roomCode;
            _receiver.timeoutSeconds      = timeoutSeconds;
            _receiver.autoReconnect       = autoReconnect;
            _receiver.reconnectDelay      = reconnectDelay;
            _receiver.maxReconnectAttempts= maxReconnectAttempts;
            _receiver.showDebugLog        = verboseLog;
            _receiver.enabled             = true;

            roomCode = _receiver.roomCode; // pick up the auto-generated code, if any
        }

        // ── Public API ────────────────────────────────────────

        /// <summary>Change the room code at runtime and rejoin.</summary>
        public void ChangeRoomCode(string newCode)
        {
            roomCode = newCode;
            if (_receiver != null)
            {
                _receiver.enabled  = false;
                _receiver.roomCode = newCode;
                _receiver.enabled  = true;
            }
        }

        /// <summary>One-line status for a HUD label.</summary>
        public string GetStatusString()
        {
            if (IsConnected)
                return $"● {RemoteIP}  M:{Motor:F2}  S:{Steer:F2}" +
                       (Nitro ? "  ⚡" : "");
            string stateStr = State == ConnectionState.Reconnecting ? "Reconnecting…" : "Waiting…";
            return $"{stateStr}  Room:{LocalIP}";
        }

        /// <summary>Multi-line diagnostics for a developer overlay.</summary>
        public string GetDiagnosticsString()
        {
            string stateColor;
            switch (State)
            {
                case ConnectionState.Connected:    stateColor = "●"; break;
                case ConnectionState.Reconnecting: stateColor = "↺"; break;
                case ConnectionState.Connecting:   stateColor = "…"; break;
                default:                           stateColor = "○"; break;
            }

            return
                $"{stateColor} {State}  |  {RemoteIP}\n" +
                $"PPS: {PacketsPerSecond:F1}  " +
                $"Loss: {PacketLoss * 100f:F1}%  " +
                $"Jitter: {EstimatedLatencyMs:F1}ms\n" +
                $"Reconnects: {ReconnectCount}" +
                (LastError.Length > 0 ? $"  Err: {LastError}" : "");
        }
    }
}
