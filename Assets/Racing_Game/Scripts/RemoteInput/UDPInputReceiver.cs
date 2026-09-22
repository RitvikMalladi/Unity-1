//──────────────────────────────────────────────────────────────
// UDPInputReceiver.cs  —  GAME APK / WebGL side
//
// NOTE: despite the class name (kept unchanged so existing scene
// references don't break), this no longer opens a raw UDP socket
// — WebGL builds cannot do that. It joins a "room" on the relay
// server (see RelayLink.cs / RelayServer/) using a short room code
// and receives "drive" JSON messages over a WebSocket.
//
// The main thread reads the latest decoded values each frame via
// LatestInput / IsConnected, same as before.
//
// Diagnostics kept from the UDP version:
//   • Packet-rate diagnostics  (PacketsPerSecond, PacketLoss)
//   • Per-packet arrival timestamp  (LastPacketTime)
//   • Inter-arrival jitter estimate (EstimatedLatencyMs)
//   • Reconnect counter             (ReconnectCount)
//   • ConnectionState enum          (Disconnected/Connecting/Connected/Reconnecting)
//──────────────────────────────────────────────────────────────

using UnityEngine;

namespace ALIyerEdon.RemoteInput
{
    public enum ConnectionState { Disconnected, Connecting, Connected, Reconnecting }

    public class UDPInputReceiver : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Network")]
        [Tooltip("Room code the controller phone must enter. Leave blank to auto-generate one per device (recommended).")]
        public string roomCode = "";
        [Tooltip("Seconds without a packet before the connection is declared lost.")]
        public float timeoutSeconds = 2f;

        [Header("Reconnect")]
        [Tooltip("Automatically reopen the relay connection after an error.")]
        public bool  autoReconnect        = true;
        [Tooltip("Seconds to wait between reconnect attempts.")]
        [Range(0.5f, 10f)]
        public float reconnectDelay       = 2f;
        [Tooltip("Maximum reconnect attempts (0 = unlimited). Currently unused — the relay link retries forever.")]
        public int   maxReconnectAttempts = 0;

        [Header("Diagnostics")]
        [Tooltip("Log each received packet to the console.")]
        public bool  showDebugLog = false;

        // ── Public state ──────────────────────────────────────
        public RemoteInputData LatestInput     { get; private set; }
        public bool            IsConnected     { get; private set; }
        public string          RemoteIP        { get; private set; } = "–";   // now holds the room code once linked
        public ConnectionState State           { get; private set; } = ConnectionState.Disconnected;

        // Diagnostics
        public float PacketsPerSecond  { get; private set; }
        public float PacketLoss        { get; private set; }
        public float EstimatedLatencyMs{ get; private set; }
        public int   ReconnectCount    { get; private set; }
        public float LastPacketTime    { get; private set; }
        public string LastError        { get; private set; } = "";

        // ── Private ───────────────────────────────────────────
        RelayLink _link;

        RemoteInputData _pending;
        bool            _hasNewData;

        int   _packetsThisSecond;
        float _diagTimer;
        float _prevPacketTime;
        float _expectedInterval = 1f / 30f;

        // ── Static helper ─────────────────────────────────────
        /// <summary>
        /// One room code per physical device, persisted so it stays the
        /// same across the Garage screen and every race — the phone user
        /// only has to read/type it once. Call this instead of hardcoding
        /// a room code if you want it auto-generated.
        /// </summary>
        public static string GetOrCreateSessionRoomCode()
        {
            string code = PlayerPrefs.GetString("RelayRoomCode", "");
            if (string.IsNullOrEmpty(code))
            {
                code = Random.Range(1000, 10000).ToString();
                PlayerPrefs.SetString("RelayRoomCode", code);
            }
            return code;
        }

        // ── Lifecycle ─────────────────────────────────────────
        void OnEnable()  => StartReceiving();
        void OnDisable() => StopReceiving();
        void OnApplicationQuit() => StopReceiving();

        // ── Main thread update ────────────────────────────────
        void Update()
        {
            float now = Time.time;

            if (_hasNewData)
            {
                LatestInput = _pending;
                _hasNewData = false;

                if (_prevPacketTime > 0f)
                {
                    float arrival    = now - _prevPacketTime;
                    float deviation  = Mathf.Abs(arrival - _expectedInterval) * 1000f;
                    EstimatedLatencyMs = Mathf.Lerp(EstimatedLatencyMs, deviation, 0.1f);
                    _expectedInterval = Mathf.Lerp(_expectedInterval, arrival, 0.05f);
                }
                _prevPacketTime = now;
                LastPacketTime  = now;
                IsConnected     = true;
                State           = ConnectionState.Connected;
                _packetsThisSecond++;
            }

            _diagTimer += Time.deltaTime;
            if (_diagTimer >= 1f)
            {
                PacketsPerSecond = _packetsThisSecond / _diagTimer;

                float expected = _diagTimer / _expectedInterval;
                PacketLoss = IsConnected
                    ? Mathf.Clamp01(1f - (_packetsThisSecond / expected))
                    : 1f;

                _packetsThisSecond = 0;
                _diagTimer         = 0f;
            }

            if (IsConnected && now - LastPacketTime > timeoutSeconds)
            {
                IsConnected = false;
                LatestInput = default;
                State       = ConnectionState.Disconnected;
                if (showDebugLog)
                    Debug.Log("[UDPInputReceiver] Timeout — input zeroed.");
            }

            // Reflect the relay link's own connection state while no drive
            // packet has arrived yet (e.g. controller connected but idle).
            if (_link != null && State == ConnectionState.Disconnected && !_link.IsOpen)
                State = ConnectionState.Connecting;
        }

        // ── Relay lifecycle ────────────────────────────────────
        void StartReceiving()
        {
            if (string.IsNullOrEmpty(roomCode))
                roomCode = GetOrCreateSessionRoomCode();

            State = ConnectionState.Connecting;

            _link = GetComponent<RelayLink>() ?? gameObject.AddComponent<RelayLink>();
            _link.OnMessage    += HandleMessage;
            _link.OnPeerJoined += HandlePeerJoined;
            _link.OnPeerLeft   += HandlePeerLeft;
            _link.Connect(roomCode, "game");

            RemoteIP        = roomCode;
            LastPacketTime  = Time.time;
            _prevPacketTime = 0f;

            if (showDebugLog)
                Debug.Log($"[UDPInputReceiver] Joining room {roomCode} as 'game'.");
        }

        void StopReceiving()
        {
            if (_link != null)
            {
                _link.OnMessage    -= HandleMessage;
                _link.OnPeerJoined -= HandlePeerJoined;
                _link.OnPeerLeft   -= HandlePeerLeft;
                _link.Disconnect();
            }
            IsConnected = false;
            State       = ConnectionState.Disconnected;
        }

        void HandlePeerJoined()
        {
            if (showDebugLog) Debug.Log("[UDPInputReceiver] Controller linked.");
        }

        void HandlePeerLeft()
        {
            ReconnectCount++;
            if (showDebugLog) Debug.Log("[UDPInputReceiver] Controller unlinked.");
        }

        [System.Serializable]
        struct DriveEnvelope { public string t; public float motor; public float steer; public bool handBrake; public bool nitro; }

        void HandleMessage(string json)
        {
            var env = JsonUtility.FromJson<DriveEnvelope>(json);
            if (env.t != "drive") return;

            _pending = new RemoteInputData
            {
                t         = "drive",
                motor     = Mathf.Clamp(env.motor, -1f, 1f),
                steer     = Mathf.Clamp(env.steer, -1f, 1f),
                handBrake = env.handBrake,
                nitro     = env.nitro
            };
            _hasNewData = true;

            if (showDebugLog)
                Debug.Log($"[UDPInputReceiver] M:{_pending.motor:F2} S:{_pending.steer:F2} " +
                          $"HB:{_pending.handBrake} N:{_pending.nitro}");
        }
    }
}
