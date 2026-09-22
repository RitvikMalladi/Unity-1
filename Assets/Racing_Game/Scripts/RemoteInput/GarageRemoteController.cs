//──────────────────────────────────────────────────────────────
// GarageRemoteController.cs  —  GAME APK side
//
// Place on any persistent GameObject in the Garage scene.
// Listens for GarageCommandData packets on a separate UDP port
// (default 5556) and calls the existing CarSelect / LevelSelect
// public methods — zero changes to existing scripts.
//
// Also responds to keyboard shortcuts so you can test without
// a phone (arrow keys + Enter + Escape).
//──────────────────────────────────────────────────────────────

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ALIyerEdon;

namespace ALIyerEdon.RemoteInput
{
    [AddComponentMenu("RemoteInput/Garage Remote Controller")]
    public class GarageRemoteController : MonoBehaviour
    {
        [Header("Network")]
        [Tooltip("Port for garage commands. Different from the driving port (5555).")]
        public int  commandPort   = 5556;
        [Tooltip("Seconds without a packet before connection indicator goes grey.")]
        public float timeoutSecs  = 3f;

        [Header("Scene References")]
        [Tooltip("Auto-found if left null.")]
        public CarSelect   carSelect;
        [Tooltip("Auto-found if left null.")]
        public LevelSelect levelSelect;

        [Header("HUD")]
        [Tooltip("Optional text element showing remote IP and connection state.")]
        public Text statusText;
        [Tooltip("Show local IP:port so user knows what to type on the phone.")]
        public bool showIPOnStart = true;
        [Tooltip("Auto-create a small on-screen IP overlay when statusText is left empty.")]
        public bool autoCreateHUD = true;

        [Header("Debug")]
        public bool verboseLog = false;

        // ── Public state ──────────────────────────────────────
        public bool   IsConnected { get; private set; }
        public string RemoteIP    { get; private set; } = "–";

        // ── Private ───────────────────────────────────────────
        UdpClient  _udp;
        Thread     _thread;
        volatile bool _running;

        GarageCommandData _pending;
        readonly object   _lock       = new object();
        bool              _hasCommand;
        float             _lastPacket;

        // ── Lifecycle ─────────────────────────────────────────
        void Start()
        {
            // Auto-find scene components
            if (carSelect  == null) carSelect  = FindFirstObjectByType<CarSelect>();
            if (levelSelect == null) levelSelect = FindFirstObjectByType<LevelSelect>();

            if (statusText == null && autoCreateHUD)
                statusText = CreateStatusHUD();

            OpenSocket();

            if (showIPOnStart)
            {
                string ip = UDPInputReceiver.LocalIPAddress();
                Debug.Log($"[GarageRemoteController] Listening on {ip}:{commandPort}");
                UpdateStatus($"Game IP: {ip}");
            }
        }

        void OnDestroy()
        {
            CloseSocket();
        }

        void OnApplicationQuit()
        {
            CloseSocket();
        }

        // ── Main thread update ────────────────────────────────
        void Update()
        {
            // Consume UDP command
            GarageCommandData cmd = default;
            bool hasCmd = false;

            lock (_lock)
            {
                if (_hasCommand)
                {
                    cmd         = _pending;
                    _hasCommand = false;
                    hasCmd      = true;
                    _lastPacket = Time.time;

                    if (!IsConnected)
                        RemoteInputBootstrapper.EnableRemote(); // phone is in control — use it for the race too

                    IsConnected = true;
                }
            }

            if (hasCmd)
                ExecuteCommand(cmd);

            // Keyboard fallback (for testing without phone)
            HandleKeyboard();

            // Timeout
            if (IsConnected && Time.time - _lastPacket > timeoutSecs)
            {
                IsConnected = false;
                UpdateStatus($"Game IP: {UDPInputReceiver.LocalIPAddress()}");
            }
        }

        // ── Command dispatch ──────────────────────────────────
        void ExecuteCommand(GarageCommandData cmd)
        {
            if (verboseLog)
                Debug.Log($"[GarageRemoteController] Command: {cmd.command}  param:{cmd.param}");

            // The active CarSelect/LevelSelect belong to whichever mode panel is
            // currently shown — re-resolve if a mode switch left the cached one inactive.
            if (carSelect == null || !carSelect.gameObject.activeInHierarchy)
                carSelect = FindFirstObjectByType<CarSelect>();
            if (levelSelect == null || !levelSelect.gameObject.activeInHierarchy)
                levelSelect = FindFirstObjectByType<LevelSelect>();

            switch (cmd.command)
            {
                case GarageCommand.NextCar:
                    carSelect?.NextCar();
                    break;

                case GarageCommand.PrevCar:
                    carSelect?.PrevCar();
                    break;

                case GarageCommand.SelectCar:
                    carSelect?.SelectCar();
                    break;

                case GarageCommand.BuyCar:
                    // No local confirmation popup is visible on the phone, so BuyCar
                    // finalizes the purchase directly instead of just opening the popup.
                    carSelect?.BuyCar();
                    break;

                case GarageCommand.SelectMode:
                    // Reuse the existing tab-switch button so PlayerPrefs, camera and
                    // the Sport/Truck/F1/Offroad panel visibility all update identically
                    // to a local tap — no duplicated logic to keep in sync.
                    FindModeButton(cmd.param)?.onClick.Invoke();
                    carSelect   = FindFirstObjectByType<CarSelect>();
                    levelSelect = FindFirstObjectByType<LevelSelect>();
                    break;

                case GarageCommand.SelectLevel:
                    levelSelect?.SelectLevel(cmd.param);
                    break;

                case GarageCommand.NextLevel:
                    // LevelSelect has no built-in next; simulate by incrementing
                    int nextID = Mathf.Min(
                        (levelSelect != null ? levelSelect.levelNames.Length - 1 : 5),
                        PlayerPrefs.GetInt("LevelID", 0) + 1);
                    levelSelect?.SelectLevel(nextID);
                    break;

                case GarageCommand.PrevLevel:
                    int prevID = Mathf.Max(0, PlayerPrefs.GetInt("LevelID", 0) - 1);
                    levelSelect?.SelectLevel(prevID);
                    break;

                case GarageCommand.Confirm:
                    carSelect?.BuyCar();
                    break;

                case GarageCommand.Back:
                    // Simulate Escape — close any open popup by pressing
                    // the back button. Concrete action depends on scene state;
                    // here we close the purchase UI if visible.
                    if (carSelect != null && carSelect.purchaseUI != null)
                        carSelect.purchaseUI.SetActive(false);
                    break;
            }

            // Update HUD
            if (IsConnected)
                UpdateStatus($"● {RemoteIP}  [{cmd.command}]");
        }

        // ── Game mode tab buttons ("Select_Sport"/"Select_Truck"/"Select_F1"/"Select_Offroad") ──
        static Button FindModeButton(byte modeId)
        {
            string name = modeId switch
            {
                0 => "Select_Sport",
                1 => "Select_Truck",
                2 => "Select_F1",
                3 => "Select_Offroad",
                _ => null
            };
            if (name == null) return null;

            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var b in buttons)
                if (b.gameObject.name == name) return b;
            return null;
        }

        // ── Keyboard fallback ─────────────────────────────────
        void HandleKeyboard()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                carSelect?.NextCar();

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                carSelect?.PrevCar();

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                carSelect?.SelectCar();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (carSelect?.purchaseUI != null)
                    carSelect.purchaseUI.SetActive(false);
            }
        }

        // ── UDP socket ────────────────────────────────────────
        void OpenSocket()
        {
            try
            {
                _udp = new UdpClient(commandPort);
                _udp.Client.ReceiveTimeout = 500;
                _running = true;
                _lastPacket = Time.time;

                _thread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "GarageCommandThread"
                };
                _thread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GarageRemoteController] Socket failed: {e.Message}");
            }
        }

        void CloseSocket()
        {
            _running = false;
            try { _udp?.Close(); } catch { /* ignore */ }
            if (_thread != null && _thread.IsAlive)
                _thread.Join(800);
            _thread = null;
        }

        void ReceiveLoop()
        {
            var remote = new IPEndPoint(IPAddress.Any, 0);
            while (_running)
            {
                try
                {
                    byte[] data = _udp.Receive(ref remote);
                    if (GarageCommandData.TryDeserialize(data, data.Length,
                            out GarageCommandData parsed))
                    {
                        lock (_lock)
                        {
                            _pending    = parsed;
                            _hasCommand = true;
                        }
                        RemoteIP = remote.Address.ToString();

                        // Send 1-byte ACK back to controller phone so it can confirm CONNECTED
                        try
                        {
                            byte[] ack = new byte[] { 0x01 };
                            _udp.Send(ack, ack.Length, remote);
                        }
                        catch { /* non-fatal */ }
                    }
                }
                catch (SocketException sex)
                {
                    if (sex.SocketErrorCode != SocketError.TimedOut && _running)
                        Debug.LogWarning($"[GarageRemoteController] {sex.Message}");
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    if (_running) Debug.LogError($"[GarageRemoteController] {e.Message}");
                }
            }
        }

        void UpdateStatus(string msg)
        {
            if (statusText != null) statusText.text = msg;
        }

        // ── Auto-created on-screen IP overlay ─────────────────
        // Mobile builds have no accessible console, so this label is the only
        // way to read the device's IP:port needed to connect the phone.
        Text CreateStatusHUD()
        {
            var cGO    = new GameObject("GarageRemote_HUD_Canvas");
            var canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("IP_Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(cGO.transform, false);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(480, 44);

            var txtGO = new GameObject("IP_Text", typeof(RectTransform), typeof(Text));
            txtGO.transform.SetParent(panel.transform, false);
            var trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 2);
            trt.offsetMax = new Vector2(-10, -2);

            var txt = txtGO.GetComponent<Text>();
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            return txt;
        }
    }
}
