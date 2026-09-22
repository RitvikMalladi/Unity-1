//──────────────────────────────────────────────────────────────
// UDPInputReceiver.cs  —  GAME APK side
//
// Opens a UDP socket on a background thread and decodes
// RemoteInputData packets.  The main thread reads the latest
// decoded values each frame via LatestInput / IsConnected.
//
// v2 additions:
//   • Packet-rate diagnostics  (PacketsPerSecond, PacketLoss)
//   • Per-packet arrival timestamp  (LastPacketTime)
//   • Round-trip latency estimate   (EstimatedLatencyMs)
//   • Auto-reconnect on socket error (configurable attempts)
//   • Reconnect counter             (ReconnectCount)
//   • Last socket error string      (LastError)
//   • ConnectionState enum          (Disconnected/Connecting/Connected/Reconnecting)
//──────────────────────────────────────────────────────────────

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace ALIyerEdon.RemoteInput
{
    public enum ConnectionState { Disconnected, Connecting, Connected, Reconnecting }

    public class UDPInputReceiver : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Network")]
        [Tooltip("UDP port this device listens on.")]
        public int   listenPort     = 5555;
        [Tooltip("Seconds without a packet before the connection is declared lost.")]
        public float timeoutSeconds = 2f;

        [Header("Reconnect")]
        [Tooltip("Automatically reopen the socket after an error.")]
        public bool  autoReconnect        = true;
        [Tooltip("Seconds to wait between reconnect attempts.")]
        [Range(0.5f, 10f)]
        public float reconnectDelay       = 2f;
        [Tooltip("Maximum reconnect attempts (0 = unlimited).")]
        public int   maxReconnectAttempts = 0;

        [Header("Diagnostics")]
        [Tooltip("Log each received packet to the console.")]
        public bool  showDebugLog = false;

        // ── Public state ──────────────────────────────────────
        public RemoteInputData LatestInput     { get; private set; }
        public bool            IsConnected     { get; private set; }
        public string          RemoteIP        { get; private set; } = "–";
        public ConnectionState State           { get; private set; } = ConnectionState.Disconnected;

        // Diagnostics
        public float PacketsPerSecond  { get; private set; }   // smoothed PPS
        public float PacketLoss        { get; private set; }   // 0-1 (0=none, 1=all lost)
        public float EstimatedLatencyMs{ get; private set; }   // inter-arrival jitter estimate
        public int   ReconnectCount    { get; private set; }
        public float LastPacketTime    { get; private set; }   // Time.time of last good packet
        public string LastError        { get; private set; } = "";

        // ── Private ───────────────────────────────────────────
        UdpClient      _udpClient;
        Thread         _receiveThread;
        volatile bool  _running;
        volatile bool  _reconnectPending;

        // Thread-safe pending data
        RemoteInputData _pending;
        readonly object _lock       = new object();
        bool            _hasNewData;

        // Diagnostics accumulators (main thread)
        int   _packetsThisSecond;
        float _diagTimer;
        float _prevPacketTime;
        float _jitterAcc;
        int   _jitterSamples;

        // Expected packet interval for loss estimation
        float _expectedInterval = 1f / 30f;   // updated when first packets arrive

        // Reconnect state
        float _reconnectTimer;
        int   _reconnectAttempts;

        // ── Local IP helper ───────────────────────────────────
        public static string LocalIPAddress()
        {
            try
            {
                using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                s.Connect("8.8.8.8", 65530);
                string ip = ((IPEndPoint)s.LocalEndPoint).Address.ToString();
                if (!string.IsNullOrEmpty(ip) && ip != "0.0.0.0" && ip != "127.0.0.1")
                    return ip;
            }
            catch { }

            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;

                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ua.Address))
                        {
                            return ua.Address.ToString();
                        }
                    }
                }
            }
            catch { }

            return "127.0.0.1";
        }

        // ── Lifecycle ─────────────────────────────────────────
        void OnEnable()  => StartReceiving();
        void OnDisable() => StopReceiving();
        void OnApplicationQuit() => StopReceiving();

        // ── Main thread update ────────────────────────────────
        void Update()
        {
            float now = Time.time;

            // Consume pending packet from network thread
            bool gotPacket = false;
            lock (_lock)
            {
                if (_hasNewData)
                {
                    LatestInput      = _pending;
                    _hasNewData      = false;
                    gotPacket        = true;
                }
            }

            if (gotPacket)
            {
                // Jitter = deviation from expected interval
                if (_prevPacketTime > 0f)
                {
                    float arrival    = now - _prevPacketTime;
                    float deviation  = Mathf.Abs(arrival - _expectedInterval) * 1000f; // ms
                    _jitterAcc      += deviation;
                    _jitterSamples++;
                    // Smooth latency estimate
                    EstimatedLatencyMs = Mathf.Lerp(EstimatedLatencyMs, deviation, 0.1f);
                    // Refine expected interval
                    _expectedInterval = Mathf.Lerp(_expectedInterval, arrival, 0.05f);
                }
                _prevPacketTime  = now;
                LastPacketTime   = now;
                IsConnected      = true;
                State            = ConnectionState.Connected;
                _packetsThisSecond++;
            }

            // PPS & packet-loss update (every 1 second)
            _diagTimer += Time.deltaTime;
            if (_diagTimer >= 1f)
            {
                PacketsPerSecond = _packetsThisSecond / _diagTimer;

                // Loss: expected = 1/_expectedInterval per second
                float expected = _diagTimer / _expectedInterval;
                PacketLoss = IsConnected
                    ? Mathf.Clamp01(1f - (_packetsThisSecond / expected))
                    : 1f;

                _packetsThisSecond = 0;
                _diagTimer         = 0f;
            }

            // Timeout
            if (IsConnected && now - LastPacketTime > timeoutSeconds)
            {
                IsConnected = false;
                LatestInput = default;
                State       = ConnectionState.Disconnected;
                if (showDebugLog)
                    Debug.Log("[UDPInputReceiver] Timeout — input zeroed.");
            }

            // Auto-reconnect polling
            if (_reconnectPending && autoReconnect)
            {
                _reconnectTimer -= Time.deltaTime;
                if (_reconnectTimer <= 0f)
                {
                    _reconnectPending = false;
                    AttemptReconnect();
                }
            }
        }

        // ── Socket lifecycle ──────────────────────────────────
        void StartReceiving()
        {
            if (_running) return;
            State = ConnectionState.Connecting;

            try
            {
                _udpClient = new UdpClient(listenPort);
                _udpClient.Client.ReceiveTimeout = 500;
                _running = true;

                _receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name         = "UDPInputReceiverThread"
                };
                _receiveThread.Start();

                LastPacketTime     = Time.time;
                _prevPacketTime    = 0f;
                _reconnectAttempts = 0;

                if (showDebugLog)
                    Debug.Log($"[UDPInputReceiver] Listening :{listenPort}  IP:{LocalIPAddress()}");
            }
            catch (Exception e)
            {
                LastError = e.Message;
                State     = ConnectionState.Disconnected;
                Debug.LogError($"[UDPInputReceiver] Socket open failed: {e.Message}");
                ScheduleReconnect();
            }
        }

        void StopReceiving()
        {
            _running = false;
            try { _udpClient?.Close(); } catch { /* ignore */ }

            if (_receiveThread != null && _receiveThread.IsAlive)
                _receiveThread.Join(1000);

            _receiveThread = null;
            IsConnected    = false;
            State          = ConnectionState.Disconnected;
        }

        void AttemptReconnect()
        {
            if (maxReconnectAttempts > 0 && _reconnectAttempts >= maxReconnectAttempts)
            {
                Debug.LogWarning("[UDPInputReceiver] Max reconnect attempts reached.");
                return;
            }

            _reconnectAttempts++;
            ReconnectCount++;
            State = ConnectionState.Reconnecting;

            if (showDebugLog)
                Debug.Log($"[UDPInputReceiver] Reconnect attempt #{_reconnectAttempts}");

            StopReceiving();
            StartReceiving();
        }

        void ScheduleReconnect()
        {
            if (!autoReconnect) return;
            _reconnectPending = true;
            _reconnectTimer   = reconnectDelay;
        }

        // ── Background receive loop ───────────────────────────
        void ReceiveLoop()
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            while (_running)
            {
                try
                {
                    byte[] data = _udpClient.Receive(ref remote);

                    if (RemoteInputData.TryDeserialize(data, data.Length,
                            out RemoteInputData parsed))
                    {
                        lock (_lock)
                        {
                            _pending    = parsed;
                            _hasNewData = true;
                        }
                        RemoteIP = remote.Address.ToString();

                        if (showDebugLog)
                            Debug.Log($"[UDPInputReceiver] " +
                                      $"M:{parsed.motor:F2} S:{parsed.steer:F2} " +
                                      $"HB:{parsed.handBrake} N:{parsed.nitro}");
                    }
                }
                catch (SocketException sex)
                {
                    if (sex.SocketErrorCode == SocketError.TimedOut)
                        continue;   // normal — just loop again

                    if (_running)
                    {
                        LastError = sex.Message;
                        Debug.LogWarning($"[UDPInputReceiver] Socket error: {sex.Message}");
                        _running = false;   // exit loop; main thread will reconnect
                        ScheduleReconnect();
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;   // socket was closed intentionally
                }
                catch (Exception e)
                {
                    if (_running)
                    {
                        LastError = e.Message;
                        Debug.LogError($"[UDPInputReceiver] {e.Message}");
                    }
                }
            }
        }
    }
}
