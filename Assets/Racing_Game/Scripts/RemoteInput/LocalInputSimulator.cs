//──────────────────────────────────────────────────────────────
// LocalInputSimulator.cs
// Simulates Android phone controller input using keyboard,
// gamepad, or mouse – no phone required.
//
// Implements ICarInputSource so CarInputAdapter treats it as a
// first-class input source.  Assign it to CarInputAdapter's
// "Fallback Source" field and it activates automatically
// whenever the remote (UDP) controller is not connected.
//
// Controls (keyboard):
//   W / Up Arrow    – throttle
//   S / Down Arrow  – brake / reverse
//   A / Left Arrow  – steer left
//   D / Right Arrow – steer right
//   Space           – hand brake
//   Left Shift      – boost (sets motor to 1 instantly)
//
// Controls (gamepad – mirrors existing InputSystem.cs):
//   Right Trigger   – throttle
//   Left Trigger    – brake
//   Left Stick X    – steer
//   Button East (B) – hand brake
//
// On-screen HUD (Game view):
//   • Steering arc with indicator needle
//   • Throttle bar  (green)
//   • Brake bar     (red)
//   • Speed readout
//   • Source label  (LOCAL SIM / REMOTE)
//   • Toggle button to enable/disable the simulator
//──────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.InputSystem;
using ALIyerEdon;

namespace ALIyerEdon.RemoteInput
{
    [AddComponentMenu("RemoteInput/Local Input Simulator")]
    public class LocalInputSimulator : MonoBehaviour, ICarInputSource
    {
        // ── Inspector ─────────────────────────────────────────────
        [Header("Simulator Settings")]
        [Tooltip("Enable/disable without removing the component.")]
        public bool simulatorEnabled = true;

        [Tooltip("Rate at which steer value ramps up when a key is held.")]
        [Range(1f, 10f)]
        public float steerSpeed     = 4f;

        [Tooltip("Rate at which steer returns to center when no key held.")]
        [Range(1f, 10f)]
        public float steerReturn    = 6f;

        [Tooltip("Rate at which throttle ramps up/down.")]
        [Range(1f, 20f)]
        public float throttleSpeed  = 8f;

        [Header("HUD")]
        [Tooltip("Show the on-screen simulator overlay in the Game view.")]
        public bool showHUD         = true;

        [Tooltip("HUD screen-space position (0,0 = bottom-left).")]
        public Vector2 hudPosition  = new Vector2(20f, 20f);

        [Tooltip("Overall HUD scale.")]
        [Range(0.5f, 2f)]
        public float hudScale       = 1f;

        // ── ICarInputSource ───────────────────────────────────────
        public float Motor     { get; private set; }
        public float Steer     { get; private set; }
        public bool  HandBrake { get; private set; }
        public bool  IsActive  => simulatorEnabled;

        /// <summary>Nitro button state (Left Shift or Gamepad South).</summary>
        public bool  Nitro     { get; private set; }

        // ── Private state ─────────────────────────────────────────
        float _rawMotor;
        float _rawSteer;

        // Reference to the car for speed readout
        EasyCarController _car;
        ALIyerEdon.Nitro  _nitroComponent;
        bool              _nitroPrev;

        // HUD skin cached
        GUIStyle _boxStyle;
        GUIStyle _labelStyle;
        GUIStyle _buttonStyle;
        bool     _stylesBuilt;

        // Arc geometry constants
        const float ARC_RADIUS   = 54f;
        const float ARC_HALF_DEG = 75f;   // arc spans ±75° from center

        // ─────────────────────────────────────────────────────────
        void Start()
        {
            // Find the car on the same GameObject or in the scene
            _car = GetComponent<EasyCarController>();
            if (_car == null)
                _car = FindFirstObjectByType<EasyCarController>();
        }

        // ── Input sampling ────────────────────────────────────────
        void Update()
        {
            if (!simulatorEnabled)
            {
                Motor     = 0f;
                Steer     = 0f;
                HandBrake = false;
                Nitro     = false;
                return;
            }

            float dt = Time.deltaTime;
            SampleThrottle(dt);
            SampleSteer(dt);
            SampleHandBrake();
            DriveNitroComponent();
        }

        // ── Drive the scene's Nitro component ─────────────────────
        void DriveNitroComponent()
        {
            if (_nitroComponent == null)
                _nitroComponent = FindFirstObjectByType<ALIyerEdon.Nitro>();
            if (_nitroComponent == null) return;

            if (Nitro && !_nitroPrev)   _nitroComponent.Apply_Nitro();
            else if (!Nitro && _nitroPrev) _nitroComponent.Release_Nitro();
            _nitroPrev = Nitro;
        }

        void SampleThrottle(float dt)
        {
            float target = 0f;

            // Keyboard
            bool fwd = Keyboard.current != null &&
                       (Keyboard.current.wKey.isPressed ||
                        Keyboard.current.upArrowKey.isPressed);

            bool rev = Keyboard.current != null &&
                       (Keyboard.current.sKey.isPressed ||
                        Keyboard.current.downArrowKey.isPressed);

            // Gamepad
            if (Gamepad.current != null)
            {
                float gt = Gamepad.current.rightTrigger.ReadValue();
                float gb = Gamepad.current.leftTrigger.ReadValue();
                if (gt > 0.05f) target =  gt;
                else if (gb > 0.05f) target = -gb;
            }
            else
            {
                if (fwd) target =  1f;
                else if (rev) target = -1f;
            }

            _rawMotor = Mathf.MoveTowards(_rawMotor, target, dt * throttleSpeed);
            Motor = _rawMotor;
        }

        void SampleSteer(float dt)
        {
            float steerInput = 0f;

            if (Gamepad.current != null)
            {
                steerInput = Gamepad.current.leftStick.ReadValue().x;
            }
            else if (Keyboard.current != null)
            {
                bool left  = Keyboard.current.aKey.isPressed ||
                             Keyboard.current.leftArrowKey.isPressed;
                bool right = Keyboard.current.dKey.isPressed ||
                             Keyboard.current.rightArrowKey.isPressed;

                if      (left  && !right) steerInput = -1f;
                else if (right && !left)  steerInput =  1f;
            }

            if (Mathf.Abs(steerInput) > 0.01f)
                _rawSteer = Mathf.MoveTowards(_rawSteer, steerInput, dt * steerSpeed);
            else
                _rawSteer = Mathf.MoveTowards(_rawSteer, 0f, dt * steerReturn);

            Steer = _rawSteer;
        }

        void SampleHandBrake()
        {
            bool kbSpace = Keyboard.current != null &&
                           Keyboard.current.spaceKey.isPressed;
            bool gpEast  = Gamepad.current  != null &&
                           Gamepad.current.buttonEast.isPressed;
            HandBrake = kbSpace || gpEast;

            // Nitro: Left Shift (keyboard) or Gamepad South (A button)
            bool kbShift  = Keyboard.current != null &&
                            Keyboard.current.leftShiftKey.isPressed;
            bool gpSouth  = Gamepad.current  != null &&
                            Gamepad.current.buttonSouth.isPressed;
            Nitro = kbShift || gpSouth;
        }

        // ── On-screen HUD (IMGUI) ─────────────────────────────────
        void OnGUI()
        {
            if (!showHUD) return;

            BuildStyles();

            float s = hudScale;
            float x = hudPosition.x;
            float y = Screen.height - hudPosition.y - 220f * s;  // anchor bottom-left

            GUI.matrix = Matrix4x4.TRS(
                new Vector3(x, y, 0),
                Quaternion.identity,
                new Vector3(s, s, 1f));

            DrawHUD();

            GUI.matrix = Matrix4x4.identity;
        }

        void DrawHUD()
        {
            float panelW = 300f;
            float panelH = 220f;

            // Background panel
            GUI.Box(new Rect(0, 0, panelW, panelH), GUIContent.none, _boxStyle);

            // ── Title / toggle ────────────────────────────────────
            string titleLabel = simulatorEnabled
                ? "◉  LOCAL SIMULATOR  [active]"
                : "○  LOCAL SIMULATOR  [off]";
            if (GUI.Button(new Rect(10, 8, panelW - 20, 22), titleLabel, _buttonStyle))
                simulatorEnabled = !simulatorEnabled;

            // ── Speed readout ─────────────────────────────────────
            float speed = _car != null ? _car.currentSpeed : 0f;
            GUI.Label(new Rect(panelW - 80, 8, 70, 22),
                      $"{speed:F0} km/h", _labelStyle);

            // ── Throttle bar (green) ──────────────────────────────
            DrawBar(new Rect(10, 38, 80, 14), Motor > 0 ? Motor : 0f,
                    new Color(0.1f, 0.85f, 0.2f), "THROTTLE");

            // ── Brake bar (red) ───────────────────────────────────
            DrawBar(new Rect(10, 60, 80, 14), Motor < 0 ? -Motor : 0f,
                    new Color(0.9f, 0.15f, 0.1f), "BRAKE");

            // ── Hand-brake indicator ──────────────────────────────
            Color hbColor = HandBrake
                ? new Color(1f, 0.6f, 0f)
                : new Color(0.3f, 0.3f, 0.3f);
            DrawColoredBox(new Rect(100, 38, 60, 36), hbColor, "HAND\nBRAKE");

            // ── Nitro indicator ───────────────────────────────────
            Color nitroColor = Nitro
                ? new Color(0.1f, 0.45f, 1.0f)
                : new Color(0.3f, 0.3f, 0.3f);
            DrawColoredBox(new Rect(168, 38, 60, 36), nitroColor, "⚡\nNITRO");

            // ── Steering arc ──────────────────────────────────────
            DrawSteeringArc(new Vector2(150f, 145f), ARC_RADIUS, Steer);

            // ── Key legend ────────────────────────────────────────
            DrawKeyLegend(new Vector2(10, 90));
        }

        // ── Steering arc drawn with GL lines ─────────────────────
        void DrawSteeringArc(Vector2 center, float radius, float steerVal)
        {
            // Arc background  (grey)
            DrawArcSegment(center, radius, -ARC_HALF_DEG, ARC_HALF_DEG,
                           new Color(0.25f, 0.25f, 0.25f), 8f);

            // Active arc (colored by direction)
            if (Mathf.Abs(steerVal) > 0.01f)
            {
                Color arcColor = steerVal < 0
                    ? new Color(0.2f, 0.5f, 1f)    // left = blue
                    : new Color(1f, 0.5f, 0.1f);   // right = orange

                float fromDeg = 0f;
                float toDeg   = steerVal * ARC_HALF_DEG;
                if (toDeg < fromDeg) { float tmp = fromDeg; fromDeg = toDeg; toDeg = tmp; }

                DrawArcSegment(center, radius, fromDeg, toDeg, arcColor, 8f);
            }

            // Needle
            float needleDeg = steerVal * ARC_HALF_DEG;
            float rad       = needleDeg * Mathf.Deg2Rad;
            Vector2 tip = center + new Vector2(
                Mathf.Sin(rad)  * radius,
               -Mathf.Cos(rad) * radius);

            // Draw needle as a GUI line approximation via a thin box
            DrawLine(center, tip, Color.white, 2f);

            // Center dot
            DrawColoredBox(new Rect(center.x - 4, center.y - 4, 8, 8),
                           Color.white, "");

            // Labels
            GUI.Label(new Rect(center.x - 30, center.y + radius + 4, 60, 16),
                      $"S: {steerVal:F2}", _labelStyle);
        }

        // Draw a progress bar with label
        void DrawBar(Rect rect, float value, Color fillColor, string label)
        {
            // Background
            DrawColoredBox(rect, new Color(0.15f, 0.15f, 0.15f), "");

            // Fill
            if (value > 0f)
            {
                var fill = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height);
                DrawColoredBox(fill, fillColor, "");
            }

            // Label above bar
            GUI.Label(new Rect(rect.x, rect.y - 14, rect.width, 14), label, _labelStyle);

            // Value text on bar
            GUI.Label(new Rect(rect.x + rect.width + 4, rect.y, 36, rect.height),
                      $"{value:F2}", _labelStyle);
        }

        void DrawKeyLegend(Vector2 origin)
        {
            var small = new GUIStyle(_labelStyle);
            small.fontSize = 10;

            string[] lines =
            {
                "W/↑  Throttle      S/↓  Brake",
                "A/←  Steer Left    D/→  Steer Right",
                "Space  Hand-brake  Shift  Nitro"
            };

            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(origin.x, origin.y + i * 13, 280, 13), lines[i], small);
        }

        // ── Primitive drawing helpers ─────────────────────────────

        void DrawColoredBox(Rect rect, Color color, string label)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prev;
            if (!string.IsNullOrEmpty(label))
                GUI.Label(rect, label, _labelStyle);
        }

        void DrawLine(Vector2 a, Vector2 b, Color color, float width)
        {
            // Approximate a line with a rotated box
            Vector2 dir  = (b - a);
            float   len  = dir.magnitude;
            float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var pivot = a + dir * 0.5f;
            var m     = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            DrawColoredBox(
                new Rect(pivot.x - len * 0.5f, pivot.y - width * 0.5f, len, width),
                color, "");
            GUI.matrix = m;
        }

        void DrawArcSegment(Vector2 center, float radius,
                            float fromDeg, float toDeg,
                            Color color, float thickness)
        {
            int steps = Mathf.Max(2, Mathf.RoundToInt(Mathf.Abs(toDeg - fromDeg) / 5f));
            float step = (toDeg - fromDeg) / steps;

            for (int i = 0; i < steps; i++)
            {
                float a0 = (fromDeg + i * step)       * Mathf.Deg2Rad;
                float a1 = (fromDeg + (i + 1f) * step) * Mathf.Deg2Rad;

                Vector2 p0 = center + new Vector2( Mathf.Sin(a0), -Mathf.Cos(a0)) * radius;
                Vector2 p1 = center + new Vector2( Mathf.Sin(a1), -Mathf.Cos(a1)) * radius;

                DrawLine(p0, p1, color, thickness);
            }
        }

        // ── Style builder ─────────────────────────────────────────
        void BuildStyles()
        {
            if (_stylesBuilt) return;
            _stylesBuilt = true;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.08f, 0.88f));
            _boxStyle.border = new RectOffset(4, 4, 4, 4);

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.fontSize = 11;
            _labelStyle.alignment = TextAnchor.MiddleCenter;

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.normal.textColor  = new Color(0.9f, 0.9f, 0.2f);
            _buttonStyle.hover.textColor   = Color.white;
            _buttonStyle.fontSize = 12;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.15f, 1f));
        }

        static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
