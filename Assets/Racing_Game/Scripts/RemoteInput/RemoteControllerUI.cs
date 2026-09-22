//──────────────────────────────────────────────────────────────
// RemoteControllerUI.cs  — CONTROLLER APK  (Landscape 1920x1080)
//
// All layout uses normalised anchors (0–1) so it works at
// any screen resolution. Built-in font is always assigned so
// text is always visible.
//
// Screen zones (normalised Y = 0 bottom, 1 top):
//
//   Y 0.91–1.00  Top bar      (IP, Connect, status)
//   Y 0.08–0.90  Main area
//     X 0.00–0.54  Left half  (steering + handbrake + nitro + mode)
//     X 0.56–1.00  Right half (throttle + brake)
//   Y 0.00–0.08  Bottom bar  (debug text)
//──────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace ALIyerEdon.RemoteInput
{
    [RequireComponent(typeof(UDPInputSender))]
    public class RemoteControllerUI : MonoBehaviour
    {
        // ── Colours ───────────────────────────────────────────
        static readonly Color C_BG        = new Color(0.06f, 0.07f, 0.10f, 1f);
        static readonly Color C_TOPBAR    = new Color(0.10f, 0.11f, 0.16f, 1f);
        static readonly Color C_THROTTLE  = new Color(0.07f, 0.58f, 0.15f, 1f);
        static readonly Color C_BRAKE     = new Color(0.68f, 0.10f, 0.08f, 1f);
        static readonly Color C_HANDBRAKE = new Color(0.76f, 0.48f, 0.04f, 1f);
        static readonly Color C_NITRO     = new Color(0.12f, 0.36f, 0.86f, 1f);
        static readonly Color C_JOYBG     = new Color(0.13f, 0.15f, 0.22f, 0.95f);
        static readonly Color C_JOYRING   = new Color(0.22f, 0.26f, 0.40f, 1f);
        static readonly Color C_JOYHANDLE = new Color(0.36f, 0.54f, 0.88f, 1f);
        static readonly Color C_INPUT     = new Color(0.14f, 0.16f, 0.24f, 1f);
        static readonly Color C_MODE_ON   = new Color(0.18f, 0.48f, 0.84f, 1f);
        static readonly Color C_MODE_OFF  = new Color(0.18f, 0.20f, 0.28f, 1f);
        static readonly Color C_CALIBRATE = new Color(0.48f, 0.15f, 0.76f, 1f);
        static readonly Color C_GYRO_NA   = new Color(0.32f, 0.13f, 0.13f, 1f);
        static readonly Color C_CONNECT   = new Color(0.13f, 0.55f, 0.18f, 1f);
        static readonly Color C_DIVIDER   = new Color(0.20f, 0.22f, 0.30f, 1f);
        static readonly Color C_CONN_OK   = new Color(0.10f, 0.86f, 0.26f, 1f);
        static readonly Color C_CONN_WAIT = new Color(0.84f, 0.60f, 0.08f, 1f);
        static readonly Color C_WHITE     = Color.white;
        static readonly Color C_GREY      = new Color(0.58f, 0.60f, 0.70f, 1f);
        static readonly Color C_SECTION   = new Color(0.42f, 0.46f, 0.62f, 1f);

        // ── Font — assign in Inspector, or auto-loaded ────────
        [Header("Font (assign any TTF font from your project)")]
        [Tooltip("Assign any font from Assets/Racing_Game/Font/ in the Inspector.")]
        public Font uiFont;

        [Header("Controller Artwork")]
        public Sprite steeringWheelSprite;
        public Sprite brakeSprite;
        public Sprite handbrakeSprite;
        public Sprite nitroSprite;

        Font _font;

        // UI / runtime references used by the generated layout
        UDPInputSender _s;
        Button[] _modeBtns;
        GameObject _joystickZone;
        GameObject _gyroPanel;
        TMP_Text _gyroValLabel;
        TMP_Text _connDot;
        Button _calibBtn;
        GameObject _canvasRoot;

        /// <summary>Shows or hides this screen (used when switching to/from the garage screen).</summary>
        public void SetVisible(bool visible)
        {
            _canvasRoot?.SetActive(visible);
            if (_s != null) _s.SetDrivingActive(visible);
        }

        void OpenGarage()
        {
            var garage = FindFirstObjectByType<GarageControllerUI>();
            if (garage != null) garage.SetVisible(true);
            SetVisible(false);
        }

        // ── Lifecycle ─────────────────────────────────────────
        void Awake()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Get built-in font — always available, guarantees visible text
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _s = GetComponent<UDPInputSender>();
            BuildUI(_s);
        }

        void Start()
        {
            // The phone controller starts in State 1 (Garage Controller)
            SetVisible(false);
        }

        void Update()
        {
            if (_s == null || (_canvasRoot != null && !_canvasRoot.activeInHierarchy)) return;
            RefreshModeButtons();
            RefreshGyroPanel();
            RefreshConnDot();
        }

        // ─────────────────────────────────────────────────────
        // BUILD UI
        // ─────────────────────────────────────────────────────
        void BuildUI(UDPInputSender s)
        {
            // Canvas
            var cGO    = new GameObject("Canvas_Ctrl");
            _canvasRoot = cGO;
            var canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var sc = cGO.AddComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920f, 1080f);
            sc.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            sc.matchWidthOrHeight  = 1f;
            cGO.AddComponent<GraphicRaycaster>();
            var root = cGO.transform;

            // Full background
            MkImg(root, "BG", C_BG, 0f, 0f, 1f, 1f, false);

            // Balanced safe-area layout for wide mobile screens.
            const float topBarH = 0.09f;
            const float topInset = 0.02f;
            const float leftX0 = 0.03f;
            const float leftX1 = 0.52f;
            const float rightX0 = 0.55f;
            const float rightX1 = 0.97f;
            const float panelY0 = 0.24f;
            const float panelY1 = 0.82f;

            // ─────────────────────────────────────────────────
            // TOP BAR  Y: 0.91 → 1.00
            // ─────────────────────────────────────────────────
            MkImg(root, "TopBar", C_TOPBAR, 0f, 1f - topBarH, 1f, 1f, false);

            // Title
            MkTxt(root, "Title", "CAR RACING REMOTE",
                  0.00f, 0.93f, 0.20f, 1.00f, 20, FontStyle.Bold, C_WHITE,
                  TextAnchor.MiddleCenter);

            // "Room:" label
            MkTxt(root, "IPLbl", "Room:",
                  0.20f, 0.92f, 0.30f, 1.00f, 17, FontStyle.Normal, C_GREY,
                  TextAnchor.MiddleRight);

            // Room code input field
            s.ipInputField = MkInputField(root,
                PlayerPrefs.GetString("RemoteRoomCode", ""),
                0.31f, 0.93f, 0.53f, 0.99f);
            s.ipInputField.onEndEdit.AddListener(_ => s.Connect());

            // Connect button
            var cb = MkBtn(root, "ConnBtn", "CONNECT",
                           0.54f, 0.92f, 0.64f, 1.00f, C_CONNECT, 18);
            cb.onClick.AddListener(s.Connect);

            // Connection dot
            _connDot = MkTxt(root, "ConnDot", "○",
                             0.66f, 0.92f, 0.70f, 1.00f, 26, FontStyle.Bold,
                             C_CONN_WAIT, TextAnchor.MiddleCenter);

            // Status text
            s.statusText = MkTxt(root, "StatusTxt", "Tap CONNECT",
                                 0.70f, 0.92f, 0.87f, 1.00f, 15, FontStyle.Normal,
                                 C_GREY, TextAnchor.MiddleLeft);

            // Calibrate button (hidden until Gyro mode)
            _calibBtn = MkBtn(root, "CalibBtn", "CALIBRATE",
                              0.88f, 0.92f, 0.99f, 1.00f, C_CALIBRATE, 16);
            _calibBtn.onClick.AddListener(s.CalibrateGyro);
            _calibBtn.gameObject.SetActive(false);

            // ─────────────────────────────────────────────────
            // VERTICAL DIVIDER  X: 0.545
            // ─────────────────────────────────────────────────
            MkImg(root, "Divider", C_DIVIDER, 0.540f, 0.10f, 0.548f, 0.90f, false);

            // ─────────────────────────────────────────────────
            // LEFT HALF
            // ─────────────────────────────────────────────────

            // Section label
            MkTxt(root, "LblSteer", "STEERING",
                  leftX0, 0.84f, leftX1, 0.90f, 14, FontStyle.Bold,
                  C_SECTION, TextAnchor.MiddleCenter);

            // ── Joystick zone  Y: 0.28 → 0.83
            _joystickZone = MkImg(root, "JoyBG", C_JOYBG,
                                  leftX0, panelY0, leftX1, panelY1);

            // Drag to steer — uses the UI event system (works for touch and mouse),
            // routed through the New Input System's InputSystemUIInputModule.
            var joyTrig = _joystickZone.AddComponent<EventTrigger>();
            AddTrig(joyTrig, EventTriggerType.PointerDown,
                d => s.OnJoystickDown(((PointerEventData)d).position));
            AddTrig(joyTrig, EventTriggerType.Drag,
                d => s.OnJoystickDrag(((PointerEventData)d).position));
            AddTrig(joyTrig, EventTriggerType.PointerUp,
                d => s.OnJoystickUp());

            // Ring inside joystick
            var ring = MkImg(_joystickZone.transform, "JoyRing", C_JOYRING,
                  0.05f, 0.05f, 0.95f, 0.95f);
            ring.GetComponent<Image>().raycastTarget = false;

            AddSpriteLayer(_joystickZone.transform, "WheelArtwork", steeringWheelSprite,
                       0.12f, 0.12f, 0.88f, 0.88f, true);

            // Handle (centred, fixed size)
            var handleGO = new GameObject("JoyHandle",
                               typeof(RectTransform), typeof(Image));
            handleGO.transform.SetParent(_joystickZone.transform, false);
            var handleImg = handleGO.GetComponent<Image>();
            handleImg.color = C_JOYHANDLE;
            handleImg.raycastTarget = false;
            var hRT = handleGO.GetComponent<RectTransform>();
            hRT.anchorMin = hRT.anchorMax = new Vector2(0.5f, 0.5f);
            hRT.anchoredPosition = Vector2.zero;
            hRT.sizeDelta = new Vector2(90f, 90f);

            s.joystickBackground = _joystickZone.GetComponent<RectTransform>();
            s.joystickHandle     = hRT;

            // "drag to steer" hint
            var hint = MkTxt(_joystickZone.transform, "JoyHint", "drag to steer",
                  0f, 0f, 1f, 0.15f, 13, FontStyle.Italic,
                  C_GREY, TextAnchor.MiddleCenter);
            hint.raycastTarget = false;

            // ── Gyro panel (same slot, hidden) ────────────────
            _gyroPanel = MkImg(root, "GyroPanel",
                               new Color(0.10f, 0.12f, 0.18f, 0.97f),
                               leftX0, panelY0, leftX1, panelY1);
            _gyroPanel.SetActive(false);

            MkTxt(_gyroPanel.transform, "GyroPanelTitle", "TILT / GYRO STEERING",
                  0f, 0.74f, 1f, 0.94f, 16, FontStyle.Bold,
                  C_GREY, TextAnchor.MiddleCenter);

            var trackGO = MkImg(_gyroPanel.transform, "Track",
                                new Color(0.20f, 0.22f, 0.30f),
                                0.05f, 0.46f, 0.95f, 0.56f);

            // Pip inside track
            var pipGO = new GameObject("Pip", typeof(RectTransform), typeof(Image));
            pipGO.transform.SetParent(trackGO.transform, false);
            pipGO.GetComponent<Image>().color = C_JOYHANDLE;
            var pipRT = pipGO.GetComponent<RectTransform>();
            pipRT.anchorMin = pipRT.anchorMax = new Vector2(0.5f, 0.5f);
            pipRT.anchoredPosition = Vector2.zero;
            pipRT.sizeDelta = new Vector2(18f, 28f);

            _gyroValLabel = MkTxt(_gyroPanel.transform, "GyroVal",
                                  "0.0 deg  |  steer: 0.00",
                                  0f, 0.24f, 1f, 0.46f, 17, FontStyle.Bold,
                                  C_WHITE, TextAnchor.MiddleCenter);

            MkTxt(_gyroPanel.transform, "GyroHint",
                  "Press CALIBRATE while holding phone level",
                  0f, 0.04f, 1f, 0.22f, 12, FontStyle.Italic,
                  C_GREY, TextAnchor.MiddleCenter);

            // ── HAND BRAKE
                 MkHold(root, "HandBrake", "HAND\nBRAKE",
                   0.02f, 0.13f, 0.26f, 0.27f,
                     C_HANDBRAKE, 22, s.HandBrakePress, s.HandBrakeRelease,
                     handbrakeSprite);

            // ── NITRO
                 MkHold(root, "Nitro", "NITRO",
                   0.28f, 0.13f, 0.50f, 0.27f,
                     C_NITRO, 26, s.NitroPress, s.NitroRelease,
                     nitroSprite);

            // ── Mode selector  Y: 0.08 → 0.13
            BuildModeSelector(root, s);

            // ─────────────────────────────────────────────────
            // RIGHT HALF
            // ─────────────────────────────────────────────────

            // Section labels
            MkTxt(root, "LblGo", "ACCELERATE",
                  rightX0, 0.84f, rightX1, 0.90f, 14, FontStyle.Bold,
                  new Color(0.28f, 0.80f, 0.35f), TextAnchor.MiddleCenter);

            MkTxt(root, "LblBrake", "BRAKE",
                  rightX0, 0.08f, rightX1, 0.13f, 14, FontStyle.Bold,
                  new Color(0.90f, 0.35f, 0.25f), TextAnchor.MiddleCenter);

            // ── THROTTLE
            MkHold(root, "Throttle", "GO",
                   rightX0, panelY0, rightX1, panelY1,
                   C_THROTTLE, 60, s.ThrottlePress, s.ThrottleRelease);

            // ── BRAKE
                 MkHold(root, "Brake", "BRAKE",
                   rightX0, 0.13f, rightX1, 0.27f,
                     C_BRAKE, 40, s.BrakePress, s.BrakeRelease,
                     brakeSprite);

            // ─────────────────────────────────────────────────
            // DEBUG TEXT  Y: 0.01 → 0.07
            // ─────────────────────────────────────────────────
            s.debugText = MkTxt(root, "DbgTxt", "",
                                0.10f, 0.01f, 0.90f, 0.07f, 12,
                                FontStyle.Normal, C_GREY, TextAnchor.MiddleCenter);

            // GARAGE toggle — switches back to the car/level picker screen
            var garageBtn = MkBtn(root, "GarageBtn", "GARAGE",
                                  0.90f, 0.01f, 0.99f, 0.07f, C_MODE_ON, 13);
            garageBtn.onClick.AddListener(OpenGarage);

            ApplyMode(s.steerMode);
        }

        // ── Mode selector  Y: 0.08 → 0.13  X: 0 → 0.54 ──────
        void BuildModeSelector(Transform root, UDPInputSender s)
        {
            MkTxt(root, "ModeSectionLbl", "STEER MODE",
                  0.01f, 0.08f, 0.10f, 0.13f, 11, FontStyle.Bold,
                  C_SECTION, TextAnchor.MiddleCenter);

            // Three equal buttons across X 0.11 → 0.53
            float[] xs = { 0.11f, 0.23f, 0.35f };
            float   xe = 0.12f;  // width of each
            string[] names = { "JOYSTICK", "GYRO", "TILT" };
            string[] subs  = { "drag",    "rotate","tilt" };
            _modeBtns = new Button[3];

            for (int i = 0; i < 3; i++)
            {
                int  idx    = i;
                bool gyroNA = (i == 1 && !SystemInfo.supportsGyroscope);

                float x0 = xs[i];
                float x1 = x0 + xe;

                var go = MkImg(root, "Mode" + i,
                               gyroNA ? C_GYRO_NA : C_MODE_OFF,
                               x0, 0.08f, x1, 0.13f);

                // Main label (upper half of button)
                MkTxt(go.transform, "Lbl",
                      gyroNA ? "GYRO\nN/A" : names[i],
                      0f, 0.42f, 1f, 1f,
                      12, FontStyle.Bold,
                      gyroNA ? new Color(0.58f, 0.38f, 0.38f) : C_WHITE,
                      TextAnchor.MiddleCenter);

                // Sub label (lower half)
                MkTxt(go.transform, "Sub", subs[i],
                      0f, 0f, 1f, 0.42f,
                      10, FontStyle.Normal, C_GREY,
                      TextAnchor.MiddleCenter);

                var btn = go.AddComponent<Button>();
                if (!gyroNA)
                    btn.onClick.AddListener(() =>
                    {
                        s.SetSteerMode(idx);
                        ApplyMode((SteerMode)idx);
                    });
                else
                    btn.interactable = false;

                _modeBtns[i] = btn;
            }
        }

        // ── Apply steering mode ───────────────────────────────
        void ApplyMode(SteerMode mode)
        {
            bool joy  = mode == SteerMode.Joystick;
            bool gyro = mode == SteerMode.Gyroscope;

            if (_joystickZone != null) _joystickZone.SetActive(joy);
            if (_gyroPanel    != null) _gyroPanel.SetActive(!joy);
            if (_calibBtn     != null) _calibBtn.gameObject.SetActive(gyro);

            if (_gyroPanel != null)
            {
                var t = _gyroPanel.transform.Find("GyroPanelTitle")
                            ?.GetComponent<TMP_Text>();
                if (t != null)
                    t.text = gyro ? "GYROSCOPE STEERING" : "TILT STEERING";
            }
        }

        // ── Per-frame updates ─────────────────────────────────
        void RefreshModeButtons()
        {
            if (_modeBtns == null) return;
            int active = (int)_s.steerMode;
            for (int i = 0; i < _modeBtns.Length; i++)
            {
                if (_modeBtns[i] == null || !_modeBtns[i].interactable) continue;
                var img = _modeBtns[i].GetComponent<Image>();
                if (img != null)
                    img.color = (i == active) ? C_MODE_ON : C_MODE_OFF;
            }
        }

        void RefreshGyroPanel()
        {
            if (_gyroPanel == null || !_gyroPanel.activeSelf) return;
            if (_gyroValLabel == null) return;

            float angle = _s.RawGyroAngle;
            float steer = _s.SmoothedSteer;
            _gyroValLabel.text = string.Format(
                "{0:+0.0;-0.0;0.0} deg  |  steer: {1:+0.00;-0.00;0.00}",
                angle, steer);

            var track = _gyroPanel.transform.Find("Track");
            if (track != null)
            {
                var pip = track.Find("Pip")?.GetComponent<RectTransform>();
                if (pip != null)
                {
                    float half = track.GetComponent<RectTransform>().rect.width * 0.45f;
                    pip.anchoredPosition = new Vector2(steer * half, 0f);
                }
            }
        }

        void RefreshConnDot()
        {
            if (_connDot == null || _s?.statusText == null) return;
            bool ok = _s.statusText.text.Contains("Ready") ||
                      _s.statusText.text.Contains("→");
            _connDot.color = ok ? C_CONN_OK : C_CONN_WAIT;
            _connDot.text  = ok ? "●" : "○";
        }

        // ─────────────────────────────────────────────────────
        // WIDGET HELPERS
        // All use normalised stretch anchors.
        // x0,y0 = anchorMin   x1,y1 = anchorMax
        // offset = zero so elements fill exactly their anchor rect
        // ─────────────────────────────────────────────────────

        static RectTransform RT(GameObject g) => g.GetComponent<RectTransform>();

        static GameObject NewGO(Transform p, string n, params System.Type[] t)
        {
            var go = new GameObject(n, t);
            go.transform.SetParent(p, false);
            return go;
        }

        // Image panel — fills the anchor rect
        static GameObject MkImg(Transform p, string n, Color c,
                                  float x0, float y0, float x1, float y1,
                                  bool raycastTarget = true)
        {
            var go = NewGO(p, n, typeof(RectTransform), typeof(Image));
            var img = go.GetComponent<Image>();
            img.color = c;
            img.raycastTarget = raycastTarget;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        // Text label — fills the anchor rect
        TMP_Text MkTxt(Transform p, string n, string txt,
                    float x0, float y0, float x1, float y1,
                    int fs, FontStyle style, Color c, TextAnchor align,
                    float padL = 6f, float padR = 6f)
        {
            var go = NewGO(p, n, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = new Vector2(padL,  2f);
            rt.offsetMax = new Vector2(-padR, -2f);
            var t = go.GetComponent<TMP_Text>();
            t.text = txt;
            t.fontSize = fs;
            t.fontStyle = style == FontStyle.Bold ? FontStyles.Bold :
                          style == FontStyle.Italic ? FontStyles.Italic : FontStyles.Normal;
            t.color = c;
            t.alignment = ToTmpAlignment(align);
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(8, fs - 8);
            t.fontSizeMax = fs;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.raycastTarget = false;
            return t;
        }

        // Click button
        Button MkBtn(Transform p, string n, string lbl,
                      float x0, float y0, float x1, float y1,
                      Color c, int fs = 20)
        {
            var go = NewGO(p, n, typeof(RectTransform), typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = c;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            MkBtnLabel(go.transform, lbl, fs);
            return go.GetComponent<Button>();
        }

        // Hold button (PointerDown / PointerUp)
        void MkHold(Transform p, string n, string lbl,
                     float x0, float y0, float x1, float y1,
                     Color c, int fs,
                     UnityAction onDown, UnityAction onUp,
                     Sprite sprite = null)
        {
            var go = NewGO(p, n, typeof(RectTransform), typeof(Image), typeof(EventTrigger));
            var img = go.GetComponent<Image>();
            img.color = c;
            img.raycastTarget = true;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            AddSpriteLayer(go.transform, "Artwork", sprite,
                           0.12f, 0.12f, 0.88f, 0.88f, true);
            MkBtnLabel(go.transform, lbl, fs);

            var trig = go.GetComponent<EventTrigger>();
            AddTrig(trig, EventTriggerType.PointerDown, _ => onDown());
            AddTrig(trig, EventTriggerType.PointerUp,   _ => onUp());
        }

        static void ApplySprite(GameObject go, Sprite sprite, bool preserveAspect)
        {
            if (sprite == null) return;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = preserveAspect;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
        }

        static void AddSpriteLayer(Transform parent, string name, Sprite sprite,
                                   float x0, float y0, float x1, float y1,
                                   bool preserveAspect)
        {
            if (sprite == null) return;

            var go = NewGO(parent, name, typeof(RectTransform), typeof(Image));
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            ApplySprite(go, sprite, preserveAspect);
        }

        // Label that fills its parent button
        void MkBtnLabel(Transform p, string txt, int fs)
        {
            var go = NewGO(p, "Lbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = RT(go);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 4f); rt.offsetMax = new Vector2(-6f, -4f);
            var t = go.GetComponent<TMP_Text>();
            t.text = txt;
            t.fontSize = fs;
            t.fontStyle = FontStyles.Bold;
            t.color = C_WHITE;
            t.alignment = TextAlignmentOptions.Center;
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(8, fs - 10);
            t.fontSizeMax = fs;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.raycastTarget = false;
        }

        // InputField
        TMP_InputField MkInputField(Transform p, string defaultVal,
                                 float x0, float y0, float x1, float y1)
        {
            var go = NewGO(p, "IPField",
                           typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.GetComponent<Image>().color = C_INPUT;
            var rt = RT(go);
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Placeholder text
            var ph  = NewGO(go.transform, "PH", typeof(RectTransform), typeof(TextMeshProUGUI));
            FillChild(ph);
            var phT = ph.GetComponent<TMP_Text>();
            phT.text      = "1234";
            phT.fontStyle = FontStyles.Italic;
            phT.fontSize  = 18;
            phT.color     = new Color(0.42f, 0.44f, 0.54f);
            phT.alignment = TextAlignmentOptions.MidlineLeft;
            phT.enableAutoSizing = true;
            phT.fontSizeMin = 12;
            phT.fontSizeMax = 18;

            // Actual text
            var tx  = NewGO(go.transform, "Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
            FillChild(tx);
            var txT = tx.GetComponent<TMP_Text>();
            txT.fontSize  = 19;
            txT.color     = C_WHITE;
            txT.alignment = TextAlignmentOptions.MidlineLeft;
            txT.enableAutoSizing = true;
            txT.fontSizeMin = 12;
            txT.fontSizeMax = 19;

            var f = go.GetComponent<TMP_InputField>();
            f.textComponent = txT;
            f.placeholder   = phT;
            f.text          = defaultVal;
            f.contentType   = TMP_InputField.ContentType.Standard;
            f.characterLimit = 12;
            return f;
        }

        static TextAlignmentOptions ToTmpAlignment(TextAnchor align)
        {
            switch (align)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        static void FillChild(GameObject go)
        {
            var rt = RT(go);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 1f); rt.offsetMax = new Vector2(-6f, -1f);
        }

        static void AddTrig(EventTrigger trig, EventTriggerType type,
                             UnityAction<BaseEventData> cb)
        {
            var e = new EventTrigger.Entry { eventID = type };
            e.callback.AddListener(cb);
            trig.triggers.Add(e);
        }
    }
}
