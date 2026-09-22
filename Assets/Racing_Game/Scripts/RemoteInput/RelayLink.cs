//──────────────────────────────────────────────────────────────
// RelayLink.cs
// Thin wrapper around the NativeWebSocket package. Both the
// Controller app and the Game app use one of these to join a
// "room" on the relay server (RelayServer/server.js) instead of
// opening a direct UDP socket — which browsers cannot do, and
// which no longer requires both devices to share a Wi-Fi network.
//
// Protocol (all messages are JSON text frames):
//   → {"t":"join","room":"1234","role":"controller"|"game"}
//   ← {"t":"peer_joined"}   — the other side of the room connected
//   ← {"t":"peer_left"}     — the other side disconnected
//   any other message is relayed verbatim to the other peer.
//
// If a second connection joins with the same room+role as one
// already present (e.g. the game app moving from the Garage scene
// to a race scene), the relay server drops the stale connection
// and keeps the new one — see RelayServer/server.js.
//──────────────────────────────────────────────────────────────

using System;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

namespace ALIyerEdon.RemoteInput
{
    public class RelayLink : MonoBehaviour
    {
        public string RoomCode      { get; private set; } = "";
        public string Role          { get; private set; } = "";
        public bool   IsOpen        => _ws != null && _ws.State == WebSocketState.Open;
        public bool   PeerConnected { get; private set; }

        public event Action<string> OnMessage;
        public event Action         OnPeerJoined;
        public event Action         OnPeerLeft;

        WebSocket _ws;
        bool      _wantConnected;
        bool      _connecting;
        float     _reconnectTimer;
        const float ReconnectDelay = 2f;

        [Serializable] struct JoinMsg  { public string t; public string room; public string role; }
        [Serializable] struct TypeOnly { public string t; }

        public void Connect(string room, string role)
        {
            RoomCode = room;
            Role     = role;
            _wantConnected = true;
            _ = OpenSocket();
        }

        public void Disconnect()
        {
            _wantConnected = false;
            PeerConnected  = false;
            _ = CloseSocket();
        }

        public void SendJson(string json)
        {
            if (IsOpen) _ = _ws.SendText(json);
        }

        async Task OpenSocket()
        {
            if (_connecting) return;
            _connecting = true;

            try
            {
                await CloseSocket();

                if (string.IsNullOrEmpty(RoomCode)) return;

                _ws = new WebSocket(RelayConfig.ServerUrl);

                _ws.OnOpen += () =>
                {
                    var join = JsonUtility.ToJson(new JoinMsg { t = "join", room = RoomCode, role = Role });
                    _ws.SendText(join);
                };

                _ws.OnMessage += bytes =>
                {
                    string json = Encoding.UTF8.GetString(bytes);
                    var typeOnly = JsonUtility.FromJson<TypeOnly>(json);
                    switch (typeOnly.t)
                    {
                        case "peer_joined":
                            PeerConnected = true;
                            OnPeerJoined?.Invoke();
                            break;
                        case "peer_left":
                            PeerConnected = false;
                            OnPeerLeft?.Invoke();
                            break;
                        default:
                            OnMessage?.Invoke(json);
                            break;
                    }
                };

                _ws.OnError += e => Debug.LogWarning($"[RelayLink] {e}");

                _ws.OnClose += _ =>
                {
                    PeerConnected = false;
                    if (_wantConnected) _reconnectTimer = ReconnectDelay;
                };

                await _ws.Connect();
            }
            finally
            {
                _connecting = false;
            }
        }

        async Task CloseSocket()
        {
            if (_ws == null) return;
            _ws.OnOpen = null; _ws.OnMessage = null; _ws.OnError = null; _ws.OnClose = null;
            try { await _ws.Close(); } catch { /* ignore */ }
            _ws = null;
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _ws?.DispatchMessageQueue();
#endif
            if (_wantConnected && !_connecting && (_ws == null || _ws.State == WebSocketState.Closed))
            {
                _reconnectTimer -= Time.deltaTime;
                if (_reconnectTimer <= 0f)
                {
                    _reconnectTimer = ReconnectDelay;
                    _ = OpenSocket();
                }
            }
        }

        void OnDestroy()         => _ = CloseSocket();
        void OnApplicationQuit() => _ = CloseSocket();
    }
}
