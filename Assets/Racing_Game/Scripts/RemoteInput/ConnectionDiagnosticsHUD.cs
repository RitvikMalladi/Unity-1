//──────────────────────────────────────────────────────────────
// ConnectionDiagnosticsHUD.cs  —  GAME APK side
//
// Attach to any GameObject in a race scene.
// Renders a real-time diagnostics overlay in the Game view
// (IMGUI — no prefab or Canvas setup required).
//
// Displays:
//   • Connection state with colour-coded indicator
//   • Remote controller IP
//   • Packets per second (PPS)
//   • Estimated packet loss %
//   • Inter-arrival jitter (latency estimate) in ms
//   • Reconnect count + last error
//   • Live motor / steer / handbrake / nitro values
//   • Uptime since last connect
//
// Toggle visibility with the `showHUD` field or by pressing
// the configurable toggle key (default: F2).
//──────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.InputSystem;

namespace ALIyerEdon.RemoteInput
{
    [AddComponentMenu("RemoteInput/Connection Diagnostics HUD")]
    public class ConnectionDiagnosticsHUD : MonoBehaviour
    {
        [Header("Display")]
        public bool    showHUD      = true;
        [Tooltip("Keyboard key that toggles the HUD on/off.")]
        public Key     toggleKey    = Key.F2;
        [Tooltip("Screen corner. 0=top-left, 1=top-right, 2=bottom-left, 3=bottom-right.")]
        [Range(0, 3)]
        public int     corner       = 0;
        [Tooltip("Pixel margin from the chosen corner.")]
        public Vector2 margin       = new Vector2(14f, 14f);
        [Range(0.6f, 2f)]
        public float   scale        = 1f;

        [Header("Source")]
        [Tooltip("Leave null — resolved automatically from RemoteInputManager.Instance.")]
        public RemoteInputManager manager;

        // ── Colours ───────────────────────────────────────────
        static readonly Color ColConnected    = new Color(0.10f, 0.90f, 0.25f);
        static readonly Color ColReconnecting = new Color(1.00f, 0.70f, 0.10f);
        static readonly Color ColDisconnected = new Color(0.90f, 0.20f, 0.15f);
        static readonly Color ColPanel        = new Color(0.02f, 0.02f, 0.05f, 0.82f);
        static readonly Color ColBar          = new Color(0.15f, 0.15f, 0.20f, 1f);
        static readonly Color ColBarFill      = new Color(0.20f, 0.65f, 1.00f, 1f);
        static readonly Color ColLoss         = new Color(1.00f, 0.35f, 0.20f, 1f);

        // ── Layout constants ──────────────────────────────────
        const float W        = 310f;
        const float ROW_H    = 20f;
        const float PAD      = 10f;
        const float BAR_H    = 10f;
        const float TOTAL_H  = PAD * 2 + ROW_H * 12 + BAR_H * 2 + 8f;

        // ── Cached styles ─────────────────────────────────────
        GUIStyle _panel, _label, _bold, _small;
        bool     _stylesReady;

        // ── Uptime tracking ───────────────────────────────────
        float _connectTime   = -1f;
        float _disconnectTime= -1f;

        // ─────────────────────────────────────────────────────
        void Start()
        {
            // Subscribe to events for uptime tracking
            RemoteInputManager.OnControllerConnected    += OnConnect;
            RemoteInputManager.OnControllerDisconnected += OnDisconnect;
        }

        void OnDestroy()
        {
            RemoteInputManager.OnControllerConnected    -= OnConnect;
            RemoteInputManager.OnControllerDisconnected -= OnDisconnect;
        }

        void OnConnect(string ip)    => _connectTime    = Time.time;
        void OnDisconnect()          => _disconnectTime = Time.time;

        void Update()
        {
            // Lazy-resolve manager
            if (manager == null)
                manager = RemoteInputManager.Instance;

            // Toggle key
            if (Keyboard.current != null &&
                Keyboard.current[toggleKey].wasPressedThisFrame)
                showHUD = !showHUD;
        }

        // ── IMGUI rendering ───────────────────────────────────
        void OnGUI()
        {
            if (!showHUD || manager == null) return;

            EnsureStyles();

            // Compute top-left corner position
            float x, y;
            float sw = Screen.width,  sh = Screen.height;
            float pw = W * scale,     ph = TOTAL_H * scale;

            switch (corner)
            {
                case 1:  x = sw - pw - margin.x;  y = margin.y;           break;
                case 2:  x = margin.x;             y = sh - ph - margin.y; break;
                case 3:  x = sw - pw - margin.x;  y = sh - ph - margin.y; break;
                default: x = margin.x;             y = margin.y;           break;
            }

            // Apply scale
            var prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(x, y, 0),
                                        Quaternion.identity,
                                        new Vector3(scale, scale, 1f));

            DrawPanel();

            GUI.matrix = prevMatrix;
        }

        void DrawPanel()
        {
            // Background
            DrawBox(new Rect(0, 0, W, TOTAL_H), ColPanel);

            float cx = PAD;
            float cy = PAD;

            // ── Title row ─────────────────────────────────────
            ConnectionState state = manager.State;
            Color stateCol = state == ConnectionState.Connected    ? ColConnected
                           : state == ConnectionState.Reconnecting ? ColReconnecting
                           : ColDisconnected;

            string stateIcon = state == ConnectionState.Connected    ? "●"
                             : state == ConnectionState.Reconnecting ? "↺"
                             : state == ConnectionState.Connecting   ? "…"
                             : "○";

            ColorLabel(new Rect(cx, cy, 20, ROW_H), stateIcon, stateCol, _bold);
            GUI.Label(new Rect(cx + 22, cy, W - 30, ROW_H),
                      $"REMOTE INPUT  [{state}]", _bold);
            cy += ROW_H + 2;

            // ── Room ──────────────────────────────────────────
            Row(ref cy, "Peer",      manager.IsConnected ? manager.RemoteIP : "–");
            Row(ref cy, "Room Code", manager.LocalIP);

            cy += 4;

            // ── Uptime / downtime ─────────────────────────────
            if (manager.IsConnected && _connectTime > 0f)
                Row(ref cy, "Uptime",    FormatDuration(Time.time - _connectTime));
            else if (!manager.IsConnected && _disconnectTime > 0f)
                Row(ref cy, "Down for",  FormatDuration(Time.time - _disconnectTime));
            else
                Row(ref cy, "Status",    "Never connected");

            cy += 4;

            // ── Packet rate ───────────────────────────────────
            float pps  = manager.PacketsPerSecond;
            float loss = manager.PacketLoss;
            float lat  = manager.EstimatedLatencyMs;

            Row(ref cy, "Packets/s",  $"{pps:F1}  (expected ~{1f / 0.033f:F0})");

            // PPS bar
            float ppsMax = 60f;
            DrawBar(new Rect(PAD, cy, W - PAD * 2, BAR_H),
                    Mathf.Clamp01(pps / ppsMax), ColBarFill);
            cy += BAR_H + 4;

            // Loss row + bar
            Color lossCol = loss > 0.1f ? ColLoss : ColBarFill;
            ColorLabel(new Rect(cx, cy, 100, ROW_H), "Packet Loss", Color.white, _label);
            ColorLabel(new Rect(cx + 105, cy, W - 110, ROW_H),
                       $"{loss * 100f:F1}%", lossCol, _bold);
            cy += ROW_H + 2;

            DrawBar(new Rect(PAD, cy, W - PAD * 2, BAR_H), loss, ColLoss);
            cy += BAR_H + 4;

            // Jitter / latency
            Color jCol = lat > 20f ? ColLoss : ColConnected;
            ColorLabel(new Rect(cx, cy, 100, ROW_H), "Jitter", Color.white, _label);
            ColorLabel(new Rect(cx + 105, cy, W - 110, ROW_H),
                       $"{lat:F1} ms", jCol, _bold);
            cy += ROW_H + 4;

            // ── Reconnect ─────────────────────────────────────
            Row(ref cy, "Reconnects", manager.ReconnectCount.ToString());

            if (manager.LastError.Length > 0)
            {
                ColorLabel(new Rect(cx, cy, W - PAD, ROW_H),
                    $"Err: {manager.LastError}", ColLoss, _small);
                cy += ROW_H;
            }

            cy += 4;

            // ── Live inputs ───────────────────────────────────
            var raw = manager.RawInput;
            Row(ref cy, "Motor",  $"{manager.Motor:+0.00;-0.00;0.00}  raw:{raw.motor:+0.00;-0.00;0.00}");
            Row(ref cy, "Steer",  $"{manager.Steer:+0.00;-0.00;0.00}  raw:{raw.steer:+0.00;-0.00;0.00}");

            string extras = "";
            if (raw.handBrake) extras += " [HB]";
            if (raw.nitro)     extras += " [⚡]";
            if (extras.Length > 0)
            {
                ColorLabel(new Rect(cx, cy, W - PAD, ROW_H), extras, ColConnected, _bold);
                cy += ROW_H;
            }

            // ── Toggle hint ───────────────────────────────────
            GUI.Label(new Rect(cx, TOTAL_H - ROW_H - 4, W - PAD, ROW_H),
                      $"{toggleKey} toggles this overlay", _small);
        }

        // ── Helpers ───────────────────────────────────────────

        void Row(ref float cy, string key, string val)
        {
            GUI.Label(new Rect(PAD,       cy, 105,       ROW_H), key, _label);
            GUI.Label(new Rect(PAD + 105, cy, W - 110,  ROW_H), val, _bold);
            cy += ROW_H + 1;
        }

        static void ColorLabel(Rect r, string text, Color color, GUIStyle style)
        {
            var prev = GUI.contentColor;
            GUI.contentColor = color;
            GUI.Label(r, text, style);
            GUI.contentColor = prev;
        }

        static void DrawBox(Rect r, Color c)
        {
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        static void DrawBar(Rect r, float fill, Color fillColor)
        {
            DrawBox(r, ColBar);
            if (fill > 0f)
                DrawBox(new Rect(r.x, r.y, r.width * Mathf.Clamp01(fill), r.height), fillColor);
        }

        static string FormatDuration(float seconds)
        {
            int m = (int)(seconds / 60);
            int s = (int)(seconds % 60);
            return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _label = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft
            };
            _label.normal.textColor = new Color(0.75f, 0.75f, 0.80f);

            _bold = new GUIStyle(_label)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = 11
            };
            _bold.normal.textColor = Color.white;

            _small = new GUIStyle(_label) { fontSize = 10 };
            _small.normal.textColor = new Color(0.55f, 0.55f, 0.60f);

            _panel = new GUIStyle(GUI.skin.box);
        }
    }
}
