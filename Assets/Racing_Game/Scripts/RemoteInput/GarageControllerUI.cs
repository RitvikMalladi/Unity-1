//──────────────────────────────────────────────────────────────
// GarageControllerUI.cs  —  CONTROLLER APK  (Portrait 800×1200)
//
//  ┌───────────────────────────────────┐
//  │  🏎  GARAGE REMOTE CONTROL        │  title bar
//  │  ● Connected  192.168.1.x:5556    │  status bar
//  ├───────────────────────────────────┤
//  │                                   │
//  │   ◄ PREVIOUS CAR    NEXT CAR ►    │  car browse
//  │                                   │
//  │   ✓  SELECT THIS CAR / PLAY       │  big green
//  │                                   │
//  │   $ BUY THIS CAR   ✕ CANCEL       │  buy / cancel
//  │                                   │
//  ├───────────────────────────────────┤
//  │  SELECT LEVEL                     │
//  │   ◄    Level 1 — City Sprint  ►   │  level browse
//  │                                   │
//  ├───────────────────────────────────┤
//  │  Game Device IP:                  │
//  │  [_______________]  [CONNECT]     │
//  │  ● status text                    │
//  └───────────────────────────────────┘
//──────────────────────────────────────────────────────────────

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ALIyerEdon.RemoteInput
{
    [AddComponentMenu("RemoteInput/Garage Controller UI")]
    public class GarageControllerUI : MonoBehaviour
    {
        [Header("Network")]
        public string targetIP   = "192.168.1.100";
        public int    targetPort = 5556;

        [Header("Level Names (shown on controller)")]
        public string[] levelNames = {
            "City Sprint", "Desert Storm", "Mountain Pass",
            "Night Circuit", "Off-Road Challenge", "Championship"
        };

        // ── Private ───────────────────────────────────────────
        UdpClient  _udp;
        IPEndPoint _ep;
        int        _localLevelID;
        int        _localModeID;

        static readonly string[] ModeNames = { "SPORT", "TRUCK", "F1", "OFFROAD" };

        // UI refs
        TMP_InputField _ipField;
        TMP_Text       _statusText;
        TMP_Text       _levelLabel;
        TMP_Text       _modeLabel;
        TMP_Text       _connDot;
        GameObject     _canvasRoot;

        bool  _hasAck;
        float _lastAckTime;

        /// <summary>Shows or hides this screen (used when switching to/from the driving screen).</summary>
        public void SetVisible(bool visible) => _canvasRoot?.SetActive(visible);

        void OpenDrive()
        {
            var drive = FindFirstObjectByType<RemoteControllerUI>();
            if (drive != null) drive.SetVisible(true);
            SetVisible(false);
        }

        // Colours
        static readonly Color C_BG       = new Color(0.06f, 0.07f, 0.10f, 1f);
        static readonly Color C_TITLE    = new Color(0.10f, 0.11f, 0.17f, 1f);
        static readonly Color C_STATUS   = new Color(0.12f, 0.14f, 0.20f, 1f);
        static readonly Color C_CARD     = new Color(0.11f, 0.13f, 0.19f, 1f);
        static readonly Color C_DIVIDER  = new Color(0.18f, 0.20f, 0.28f, 1f);
        static readonly Color C_SELECT   = new Color(0.08f, 0.60f, 0.18f, 1f);
        static readonly Color C_BUY      = new Color(0.78f, 0.52f, 0.04f, 1f);
        static readonly Color C_BACK     = new Color(0.50f, 0.14f, 0.14f, 1f);
        static readonly Color C_NAV      = new Color(0.18f, 0.25f, 0.52f, 1f);
        static readonly Color C_CONNECT  = new Color(0.15f, 0.58f, 0.20f, 1f);
        static readonly Color C_INPUT    = new Color(0.14f, 0.16f, 0.24f, 1f);
        static readonly Color C_WHITE    = Color.white;
        static readonly Color C_GREY     = new Color(0.60f, 0.63f, 0.72f, 1f);
        static readonly Color C_CONN_OK  = new Color(0.12f, 0.88f, 0.30f, 1f);
        static readonly Color C_CONN_NO  = new Color(0.85f, 0.62f, 0.08f, 1f);
        static readonly Color C_SECTION_LBL = new Color(0.45f, 0.50f, 0.68f, 1f);
        static readonly Color C_HINT        = new Color(0.48f, 0.52f, 0.62f, 1f);

        // ── Lifecycle ─────────────────────────────────────────
        void Start()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            _localLevelID = PlayerPrefs.GetInt("LevelID", 0);
            _localModeID  = Mathf.Clamp(PlayerPrefs.GetInt("GameMode", 0), 0, ModeNames.Length - 1);
            targetIP = PlayerPrefs.GetString("RemoteTargetIP", targetIP);

            OpenSocket();
            BuildUI();
        }

        void OnDestroy()         => CloseSocket();
        void OnApplicationQuit() => CloseSocket();

        void Update()
        {
            if (_hasAck)
            {
                if (Time.time - _lastAckTime > 4f)
                {
                    _hasAck = false;
                    SetStatus($"Ready  →  {targetIP}:{targetPort}", false);
                }
                else
                {
                    SetStatus($"Connected  →  {targetIP}:{targetPort}", true);
                }
            }
        }

        // ── Send ──────────────────────────────────────────────
        void Send(GarageCommand cmd, byte param = 0)
        {
            // Resync the endpoint if the IP field was edited without tapping CONNECT.
            if (_ipField != null)
            {
                string current = _ipField.text.Trim();
                if (!string.IsNullOrEmpty(current) && current != targetIP)
                    OnIPChanged(current);
            }

            if (_udp == null || _ep == null) return;
            byte[] b = new GarageCommandData { command = cmd, param = param }.Serialize();
            try   { _udp.Send(b, b.Length, _ep); }
            catch (Exception e) { Debug.LogWarning($"[GarageControllerUI] {e.Message}"); }
        }

        // ── Socket ────────────────────────────────────────────
        void OpenSocket()
        {
            try
            {
                _udp = new UdpClient();
                _ep  = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
                _hasAck = false;
                SetStatus($"Ready  →  {targetIP}:{targetPort}", false);
                BeginReceive();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GarageControllerUI] {e.Message}");
                SetStatus("⚠  Invalid IP address", false);
            }
        }

        void BeginReceive()
        {
            if (_udp == null) return;
            try
            {
                _udp.BeginReceive(OnReceive, null);
            }
            catch { }
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (_udp == null) return;
                IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udp.EndReceive(ar, ref from);
                if (data != null && data.Length > 0)
                {
                    _hasAck = true;
                    _lastAckTime = Time.time;
                }
                BeginReceive();
            }
            catch { }
        }

        void CloseSocket()
        {
            try { _udp?.Close(); } catch { }
            _udp = null;
        }

        void OnIPChanged(string ip)
        {
            ip = ip.Trim();
            if (string.IsNullOrEmpty(ip)) return;
            if (ip.Contains(":"))
                ip = ip.Split(':')[0].Trim();

            targetIP = ip;
            if (_ipField != null && _ipField.text != targetIP)
                _ipField.text = targetIP;

            PlayerPrefs.SetString("RemoteTargetIP", targetIP);
            CloseSocket();
            OpenSocket();

            var driveSender = GetComponent<UDPInputSender>() ?? FindFirstObjectByType<UDPInputSender>();
            if (driveSender != null && driveSender.targetIP != targetIP)
            {
                driveSender.SetTargetIP(targetIP);
            }
        }

        public void SetTargetIP(string ip)
        {
            ip = ip.Trim();
            if (string.IsNullOrEmpty(ip)) return;
            if (ip.Contains(":"))
                ip = ip.Split(':')[0].Trim();

            if (ip == targetIP) return;
            targetIP = ip;
            if (_ipField != null) _ipField.text = targetIP;
            CloseSocket();
            OpenSocket();
        }

        public void Connect()
        {
            if (_ipField != null) OnIPChanged(_ipField.text);
        }

        void SetStatus(string msg, bool ok)
        {
            if (_statusText != null) _statusText.text = msg;
            if (_connDot    != null)
            {
                _connDot.text  = ok ? "●" : "○";
                _connDot.color = ok ? C_CONN_OK : C_CONN_NO;
            }
        }

        // ── Level browsing ────────────────────────────────────
        public void NextLevel()
        {
            if (_localLevelID < levelNames.Length - 1) _localLevelID++;
            RefreshLevelLabel();
            Send(GarageCommand.SelectLevel, (byte)_localLevelID);
        }

        public void PrevLevel()
        {
            if (_localLevelID > 0) _localLevelID--;
            RefreshLevelLabel();
            Send(GarageCommand.SelectLevel, (byte)_localLevelID);
        }

        void RefreshLevelLabel()
        {
            if (_levelLabel != null)
                _levelLabel.text = $"Level {_localLevelID + 1}" +
                    (_localLevelID < levelNames.Length
                        ? $"\n{levelNames[_localLevelID]}" : "");
        }

        // ── Game mode browsing ────────────────────────────────
        public void NextMode()
        {
            if (_localModeID < ModeNames.Length - 1) _localModeID++;
            RefreshModeLabel();
            Send(GarageCommand.SelectMode, (byte)_localModeID);
        }

        public void PrevMode()
        {
            if (_localModeID > 0) _localModeID--;
            RefreshModeLabel();
            Send(GarageCommand.SelectMode, (byte)_localModeID);
        }

        void RefreshModeLabel()
        {
            if (_modeLabel != null) _modeLabel.text = ModeNames[_localModeID];
        }

        // ── Build UI ──────────────────────────────────────────
        // All positions are normalised anchors (0..1 of the parent group), so nothing
        // can ever land outside the canvas regardless of screen aspect ratio.
        void BuildUI()
        {
            var cGO    = new GameObject("GarageCanvas");
            _canvasRoot = cGO;
            var canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = cGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
            cGO.AddComponent<GraphicRaycaster>();
            var root = cGO.transform;

            Panel(root, "BG", C_BG, 0f, 0f, 1f, 1f);

            // ── Title bar  y: 0.945 → 1.000 ────────────────────
            var titleBar = Grp(root, "TitleBar", 0f, 0.945f, 1f, 1f);
            Panel(titleBar.transform, "BarBG", C_TITLE, 0f, 0f, 1f, 1f);
            Txt(titleBar.transform, "TitleText", "🏎 GARAGE REMOTE",
                0.03f, 0f, 0.62f, 1f, 24, FontStyles.Bold, C_WHITE,
                TextAlignmentOptions.MidlineLeft);
            Btn(titleBar.transform, "DriveBtn", "DRIVE ▸",
                0.65f, 0.18f, 0.97f, 0.82f, C_NAV, 18, OpenDrive);

            // ── Status bar  y: 0.895 → 0.945 ───────────────────
            var statusBar = Grp(root, "StatusBar", 0f, 0.895f, 1f, 0.945f);
            Panel(statusBar.transform, "BarBG", C_STATUS, 0f, 0f, 1f, 1f);
            _connDot = Txt(statusBar.transform, "Dot", "○",
                0.02f, 0f, 0.10f, 1f, 22, FontStyles.Bold, C_CONN_NO,
                TextAlignmentOptions.Center);
            _statusText = Txt(statusBar.transform, "StatusTxt",
                $"Tap CONNECT  →  {targetIP}:{targetPort}",
                0.11f, 0f, 0.97f, 1f, 16, FontStyles.Normal, C_GREY,
                TextAlignmentOptions.MidlineLeft);

            // ── Game mode group  y: 0.775 → 0.895 ──────────────
            var modeGrp = Grp(root, "ModeGroup", 0f, 0.775f, 1f, 0.895f);
            Txt(modeGrp.transform, "ModeSectionLbl", "GAME MODE",
                0.05f, 0.78f, 0.95f, 1.00f, 15, FontStyles.Bold, C_SECTION_LBL,
                TextAlignmentOptions.MidlineLeft);

            Btn(modeGrp.transform, "PrevMode", "◄", 0.05f, 0.10f, 0.28f, 0.72f,
                C_NAV, 30, PrevMode);

            var modeCard = Panel(modeGrp.transform, "ModeCard", C_CARD, 0.30f, 0.10f, 0.70f, 0.72f);
            _modeLabel = Txt(modeCard.transform, "ModeLbl", ModeNames[_localModeID],
                0.05f, 0.05f, 0.95f, 0.95f, 20, FontStyles.Bold, C_WHITE,
                TextAlignmentOptions.Center);

            Btn(modeGrp.transform, "NextMode", "►", 0.72f, 0.10f, 0.95f, 0.72f,
                C_NAV, 30, NextMode);

            Panel(root, "Div0", C_DIVIDER, 0.05f, 0.767f, 0.95f, 0.773f);

            // ── Car selection group  y: 0.470 → 0.765 ──────────
            var carGrp = Grp(root, "CarGroup", 0f, 0.470f, 1f, 0.765f);
            Txt(carGrp.transform, "CarSectionLbl", "CAR SELECTION",
                0.05f, 0.90f, 0.95f, 1.00f, 15, FontStyles.Bold, C_SECTION_LBL,
                TextAlignmentOptions.MidlineLeft);

            Btn(carGrp.transform, "PrevCar", "◄  PREVIOUS", 0.05f, 0.68f, 0.48f, 0.86f,
                C_NAV, 22, () => Send(GarageCommand.PrevCar));
            Btn(carGrp.transform, "NextCar", "NEXT  ►", 0.52f, 0.68f, 0.95f, 0.86f,
                C_NAV, 22, () => Send(GarageCommand.NextCar));

            Txt(carGrp.transform, "Hint1", "Browse cars with the buttons above",
                0.05f, 0.58f, 0.95f, 0.66f, 13, FontStyles.Italic, C_HINT,
                TextAlignmentOptions.Center);

            Btn(carGrp.transform, "SelectCar", "✓  SELECT THIS CAR / PLAY",
                0.05f, 0.32f, 0.95f, 0.54f, C_SELECT, 22,
                () => Send(GarageCommand.SelectCar));

            Btn(carGrp.transform, "BuyCar", "💰  BUY THIS CAR",
                0.05f, 0.04f, 0.48f, 0.28f, C_BUY, 18,
                () => Send(GarageCommand.BuyCar));
            Btn(carGrp.transform, "Back", "✕  CANCEL",
                0.52f, 0.04f, 0.95f, 0.28f, C_BACK, 18,
                () => Send(GarageCommand.Back));

            Panel(root, "Div1", C_DIVIDER, 0.05f, 0.462f, 0.95f, 0.468f);

            // ── Level selection group  y: 0.290 → 0.460 ────────
            var lvlGrp = Grp(root, "LevelGroup", 0f, 0.290f, 1f, 0.460f);
            Txt(lvlGrp.transform, "LevelSectionLbl", "LEVEL SELECTION",
                0.05f, 0.82f, 0.95f, 1.00f, 15, FontStyles.Bold, C_SECTION_LBL,
                TextAlignmentOptions.MidlineLeft);

            Btn(lvlGrp.transform, "PrevLvl", "◄", 0.05f, 0.15f, 0.28f, 0.75f,
                C_NAV, 30, PrevLevel);

            var lvlCard = Panel(lvlGrp.transform, "LvlCard", C_CARD, 0.30f, 0.15f, 0.70f, 0.75f);
            _levelLabel = Txt(lvlCard.transform, "LvlLbl",
                $"Level {_localLevelID + 1}\n{(levelNames.Length > 0 ? levelNames[Mathf.Min(_localLevelID, levelNames.Length - 1)] : "")}",
                0.05f, 0.05f, 0.95f, 0.95f, 18, FontStyles.Bold, C_WHITE,
                TextAlignmentOptions.Center);

            Btn(lvlGrp.transform, "NextLvl", "►", 0.72f, 0.15f, 0.95f, 0.75f,
                C_NAV, 30, NextLevel);

            Panel(root, "Div2", C_DIVIDER, 0.05f, 0.282f, 0.95f, 0.288f);

            // ── Connection group  y: 0.100 → 0.280 ─────────────
            var connGrp = Grp(root, "ConnGroup", 0f, 0.100f, 1f, 0.280f);
            Txt(connGrp.transform, "ConnSectionLbl", "CONNECTION",
                0.05f, 0.82f, 0.95f, 1.00f, 15, FontStyles.Bold, C_SECTION_LBL,
                TextAlignmentOptions.MidlineLeft);
            Txt(connGrp.transform, "IPHint", "Type the IP shown on the game device screen",
                0.05f, 0.58f, 0.95f, 0.78f, 13, FontStyles.Italic, C_HINT,
                TextAlignmentOptions.Center);

            _ipField = InputFld(connGrp.transform,
                PlayerPrefs.GetString("RemoteTargetIP", targetIP),
                0.05f, 0.05f, 0.62f, 0.48f);
            _ipField.onEndEdit.AddListener(OnIPChanged);

            Btn(connGrp.transform, "Connect", "CONNECT",
                0.66f, 0.05f, 0.95f, 0.48f, C_CONNECT, 20, Connect);
        }

        // ── Widget helpers ────────────────────────────────────
        // All helpers take normalised (0..1) anchors relative to their parent,
        // so nesting groups inside groups keeps every widget within bounds.

        static RectTransform RT(GameObject g) => g.GetComponent<RectTransform>();

        static GameObject GO(Transform p, string n, params System.Type[] t)
        {
            var go = new GameObject(n, t);
            go.transform.SetParent(p, false);
            return go;
        }

        // Bare group container (no visuals) — anchors its children as a sub-region.
        static GameObject Grp(Transform p, string n, float x0, float y0, float x1, float y1)
        {
            var go = GO(p, n, typeof(RectTransform));
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        static GameObject Panel(Transform p, string n, Color c, float x0, float y0, float x1, float y1)
        {
            var go = GO(p, n, typeof(RectTransform), typeof(Image));
            go.GetComponent<Image>().color = c;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        static TMP_Text Txt(Transform p, string n, string txt,
                            float x0, float y0, float x1, float y1,
                            int fs, FontStyles style, Color c, TextAlignmentOptions align)
        {
            var go = GO(p, n, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<TMP_Text>();
            t.text = txt;
            t.fontSize = fs;
            t.fontStyle = style;
            t.color = c;
            t.alignment = align;
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(8, fs - 10);
            t.fontSizeMax = fs;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        static void Btn(Transform p, string n, string lbl,
                        float x0, float y0, float x1, float y1,
                        Color c, int fs, Action cb)
        {
            var go = GO(p, n, typeof(RectTransform), typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = c;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var lt = Txt(go.transform, "Lbl", lbl, 0f, 0f, 1f, 1f,
                         fs, FontStyles.Bold, C_WHITE, TextAlignmentOptions.Center);
            lt.raycastTarget = false;

            go.GetComponent<Button>().onClick.AddListener(() => cb?.Invoke());
        }

        static TMP_InputField InputFld(Transform p, string defaultVal,
                                       float x0, float y0, float x1, float y1)
        {
            var go = GO(p, "IPInputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.GetComponent<Image>().color = C_INPUT;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var ph  = Txt(go.transform, "PH", "192.168.x.x", 0f, 0f, 1f, 1f,
                          20, FontStyles.Italic, new Color(0.42f, 0.45f, 0.56f),
                          TextAlignmentOptions.MidlineLeft);
            var txt = Txt(go.transform, "Txt", "", 0f, 0f, 1f, 1f,
                          20, FontStyles.Normal, C_WHITE, TextAlignmentOptions.MidlineLeft);

            var f = go.GetComponent<TMP_InputField>();
            f.textComponent = (TMP_Text)txt;
            f.placeholder   = (TMP_Text)ph;
            f.text          = defaultVal;
            // DecimalNumber only allows a single '.' — an IP address needs three, so use Standard.
            f.contentType    = TMP_InputField.ContentType.Standard;
            f.characterLimit = 30;
            return f;
        }
    }
}
