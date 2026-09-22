//──────────────────────────────────────────────────────────────
// RemoteInputBootstrapper.cs
// GAME APK side — place on ANY persistent GameObject in a race
// scene (e.g. an empty "RemoteInputManager" GameObject).
//
// Responsibilities:
//   • Creates / configures RemoteInputManager (if not already in
//     the scene) — always, regardless of how the race was entered.
//   • Waits for the player car to spawn, then injects
//     CarInputAdapter onto it.
//   • Disables InputSystem and Car_AI only once a phone actually
//     connects (see CarInputAdapter.OnRemoteConnected) — local
//     play is unaffected until then.
//──────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ALIyerEdon;

namespace ALIyerEdon.RemoteInput
{
    public class RemoteInputBootstrapper : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────
        [Header("Remote Input Settings")]
        [Tooltip("Room code the controller phone must enter. Leave blank to auto-generate one per device (recommended).")]
        public string roomCode       = "";
        [Tooltip("Seconds of silence before input is zeroed.")]
        public float timeoutSeconds  = 2f;
        [Tooltip("Always activate, ignoring PlayerPrefs. Useful during development.")]
        public bool  alwaysUseRemote = false;

        [Header("HUD (optional)")]
        [Tooltip("Assign an existing Text element. Leave null to auto-create one.")]
        public Text  statusText;
        [Tooltip("Auto-create a minimal room-code/status overlay when statusText is null.")]
        public bool  autoCreateHUD   = true;

        // ── Static helpers ────────────────────────────────────────
        public static void EnableRemote()    => PlayerPrefs.SetInt("RemoteInput", 1);
        public static void DisableRemote()   => PlayerPrefs.SetInt("RemoteInput", 0);
        public static bool IsRemoteEnabled   => PlayerPrefs.GetInt("RemoteInput", 0) == 1;

        // ── Boot sequence ─────────────────────────────────────────
        IEnumerator Start()
        {
            // Always set up the receiver + adapter. This used to require
            // RemoteInputBootstrapper.EnableRemote() to have been called first
            // (from a Garage remote command), but that made remote driving
            // silently do nothing if the player skipped straight to the driving
            // screen without touching a Garage button first. CarInputAdapter
            // only actually overrides local input once a phone packet arrives
            // (see RemoteInputManager.IsActive), so leaving this always-on is
            // harmless for local play and makes remote driving connect on its own.

            // Step 1 — ensure RemoteInputManager exists
            RemoteInputManager manager = EnsureManager();

            // Step 2 — wait for the player car (spawned at runtime from prefab)
            yield return new WaitForEndOfFrame();

            GameObject playerGO = null;
            for (int i = 0; i < 60; i++)
            {
                playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) break;
                yield return null;
            }

            if (playerGO == null)
            {
                Debug.LogError("[RemoteInputBootstrapper] No GameObject tagged 'Player' found. " +
                               "Remote input inactive.");
                yield break;
            }

            // Step 3 — inject CarInputAdapter onto the player car
            InjectAdapter(playerGO, manager);

            Debug.Log($"[RemoteInputBootstrapper] Ready. " +
                      $"Room: {manager.roomCode}  " +
                      $"Player: '{playerGO.name}'");
        }

        // ── Ensure RemoteInputManager exists on this GameObject ───
        RemoteInputManager EnsureManager()
        {
            // Prefer an existing one in the scene (e.g. from DontDestroyOnLoad)
            var existing = RemoteInputManager.Instance;
            if (existing != null) return existing;

            // Add one to this GameObject
            var mgr = GetComponent<RemoteInputManager>();
            if (mgr == null)
                mgr = gameObject.AddComponent<RemoteInputManager>();

            mgr.roomCode       = roomCode;
            mgr.timeoutSeconds = timeoutSeconds;
            return mgr;
        }

        // ── Inject CarInputAdapter onto the player car ────────────
        void InjectAdapter(GameObject playerGO, RemoteInputManager manager)
        {
            // Note: Local input (InputSystem) remains active and functional until a remote
            // packet actually arrives over the relay. CarInputAdapter handles disabling
            // local input in OnRemoteConnected and re-enabling it in OnRemoteDisconnected.

            // CarInputAdapter coordinates remote vs local input
            var adapter = playerGO.GetComponent<CarInputAdapter>();
            if (adapter == null)
                adapter = playerGO.AddComponent<CarInputAdapter>();

            adapter.remoteManager              = manager;
            adapter.manageLocalInputComponents = true;

            // Resolve HUD text
            Text hud = statusText ?? (autoCreateHUD ? CreateStatusHUD(manager.roomCode) : null);
            adapter.statusText = hud;
        }

        // ── Auto-create a minimal status overlay ─────────────────
        Text CreateStatusHUD(string room)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasT;

            if (canvas != null)
            {
                canvasT = canvas.transform;
            }
            else
            {
                var cgo = new GameObject("RemoteInput_Canvas");
                var c   = cgo.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                cgo.AddComponent<CanvasScaler>();
                cgo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasT = cgo.transform;
            }

            var panel = new GameObject("RI_StatusPanel",
                            typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(canvasT, false);
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.5f);

            var rt       = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(400, 48);

            var txtGO = new GameObject("RI_StatusText", typeof(RectTransform), typeof(Text));
            txtGO.transform.SetParent(panel.transform, false);
            var trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(6, 2);
            trt.offsetMax = new Vector2(-6, -2);

            var txt       = txtGO.GetComponent<Text>();
            txt.fontSize  = 18;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.text      = $"Room: {room}";

            return txt;
        }
    }
}
