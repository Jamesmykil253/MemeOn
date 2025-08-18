using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MemeArena.Utilities
{
    /// <summary>
    /// Simple runtime checks to reduce manual setup: warns when key components
    /// are missing and auto-wires obvious references.
    /// </summary>
    public class RuntimeBootstrapValidator : MonoBehaviour
    {
        [Header("Checks")] public bool checkNetworkManager = true;
        public bool checkPlayerPrefab = true;
        public bool checkCameraTarget = true;
    [Tooltip("Warn and auto-disable deprecated components (PlayerController, EnemyWorldUI).")]
    public bool checkDeprecatedComponents = true;
    [Tooltip("Ensure PlayerUIBinder exists on HUD Canvas to auto-bind local player UI at runtime.")]
    public bool ensurePlayerUIBinder = true;

        void Start()
        {
            var nm = NetworkManager.Singleton;
            if (checkNetworkManager && !nm)
            {
                Debug.LogError("RuntimeBootstrapValidator: NetworkManager.Singleton not found. Add NetworkManager + UnityTransport in Bootstrap scene.");
            }
            if (nm)
            {
                if (checkPlayerPrefab && nm.NetworkConfig.PlayerPrefab == null)
                {
                    Debug.LogWarning("RuntimeBootstrapValidator: Player Prefab is not assigned in NetworkManager. Auto-assignment may occur via NetworkPrefabsRegistrar if configured.");
                }
            }

            if (checkCameraTarget)
            {
                var cam = Camera.main;
                if (cam)
                {
                    var bootstrap = cam.GetComponent<MemeArena.CameraSystem.CameraBootstrap>();
                    if (!bootstrap)
                    {
                        bootstrap = cam.gameObject.AddComponent<MemeArena.CameraSystem.CameraBootstrap>();
                        Debug.Log("RuntimeBootstrapValidator: Added CameraBootstrap to Main Camera.");
                    }

                    // Disable TMP Example CameraController which uses legacy Input
                    var tmpCamCtrl = cam.GetComponent("TMPro.Examples.CameraController");
                    if (tmpCamCtrl != null)
                    {
                        var comp = tmpCamCtrl as Behaviour;
                        if (comp != null && comp.enabled)
                        {
                            comp.enabled = false;
                            Debug.LogWarning("RuntimeBootstrapValidator: Disabled TextMeshPro Examples CameraController (legacy Input) to prevent Input System exceptions.");
                        }
                    }
                }
            }

            // Warn if multiple scenes are loaded (can cause duplicate objects if both Bootstrap and Gameplay are open additively)
            if (SceneManager.sceneCount > 1)
            {
                Debug.LogWarning($"RuntimeBootstrapValidator: {SceneManager.sceneCount} scenes loaded. For NGO flow, keep only Bootstrap loaded; it will load gameplay over the network.");
            }

            // Enforce single AudioListener: keep main camera's, disable others
            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (listeners != null && listeners.Length > 1)
            {
                var main = Camera.main != null ? Camera.main.GetComponent<AudioListener>() : null;
                foreach (var l in listeners)
                {
                    if (main != null && l == main) continue;
                    if (l.isActiveAndEnabled)
                    {
                        l.enabled = false;
                    }
                }
                Debug.LogWarning("RuntimeBootstrapValidator: Disabled extra AudioListeners to ensure only one is active.");
            }

            // Ensure PlayerUIBinder is present on HUD canvases so HUD auto-binds to local player
            if (ensurePlayerUIBinder)
            {
                TryEnsurePlayerUIBinder();
            }

            // Check for PlayerMovement objects that are not marked as PlayerObject (usually scene-placed players)
            var movers = FindObjectsByType<MemeArena.Players.PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int nonPlayerObjects = 0;
            foreach (var mv in movers)
            {
                var no = mv.GetComponent<NetworkObject>();
                if (no != null && !no.IsPlayerObject) nonPlayerObjects++;
            }
            if (nonPlayerObjects > 0)
            {
                Debug.LogWarning($"RuntimeBootstrapValidator: Found {nonPlayerObjects} PlayerMovement instance(s) that are not PlayerObject. Remove scene-placed players and rely on NetworkManager Player Prefab to avoid duplicates.");
            }

            // Deprecated component scan: disable to avoid conflicts (reflection-based to avoid CS0618 warnings)
            if (checkDeprecatedComponents)
            {
                HandleDeprecated("MemeArena.Players.PlayerController", disable:true, warn:$"RuntimeBootstrapValidator: Disabled deprecated PlayerController on {{0}}. Use PlayerMovement.");
                HandleDeprecated("MemeArena.UI.EnemyWorldUI", disable:false, warn:$"RuntimeBootstrapValidator: Deprecated EnemyWorldUI found on {{0}}. Prefer EnemyUnitUI + HealthBarUI/LevelUI.");
                HandleDeprecated("MemeArena.Items.LegacyPlayerInventory", disable:false, warn:$"RuntimeBootstrapValidator: Deprecated LegacyPlayerInventory found on {{0}}. Replace with Players.PlayerInventory.");
            }
        }

        private static Type FindTypeByFullName(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == fullName);
        }

        private static void HandleDeprecated(string typeFullName, bool disable, string warn)
        {
            var t = FindTypeByFullName(typeFullName);
            if (t == null) return;
            // Find active scene objects of this type
            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var mb in allBehaviours)
            {
                if (mb == null) continue;
                var mbType = mb.GetType();
                if (mbType == t || mbType.IsSubclassOf(t))
                {
                    if (!string.IsNullOrEmpty(warn)) Debug.LogWarning(string.Format(warn, mb.name));
                    if (disable)
                    {
                        mb.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Adds MemeArena.UI.PlayerUIBinder to likely HUD canvases if missing, using reflection to avoid hard deps.
        /// </summary>
        private static void TryEnsurePlayerUIBinder()
        {
            // Prefer canonical binder; fall back to deprecated alias if needed
            var binderType = FindTypeByFullName("MemeArena.HUD.PlayerHUDBinder")
                             ?? FindTypeByFullName("MemeArena.UI.PlayerUIBinder");
            if (binderType == null) return;

            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (canvases == null || canvases.Length == 0) return;

            // Types that indicate this canvas is a HUD
            var markerTypeNames = new string[]
            {
                "MemeArena.UI.HealthBarUI",
                "MemeArena.UI.XPBarUI",
                "MemeArena.UI.LevelUI",
                "MemeArena.UI.CoinCounterUI",
                "MemeArena.UI.BoostedAttackUI"
            };
            var markerTypes = new List<Type>();
            foreach (var tn in markerTypeNames)
            {
                var t = FindTypeByFullName(tn);
                if (t != null) markerTypes.Add(t);
            }

            foreach (var canvas in canvases)
            {
                if (canvas == null) continue;
                // Skip if already has binder
                if (canvas.GetComponent(binderType) != null) continue;

                bool looksLikeHud = false;
                if (!string.IsNullOrEmpty(canvas.name) && canvas.name.IndexOf("HUD", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    looksLikeHud = true;
                }
                else
                {
                    try
                    {
                        var behaviours = canvas.GetComponentsInChildren<MonoBehaviour>(true);
                        foreach (var mb in behaviours)
                        {
                            var t = mb != null ? mb.GetType() : null;
                            if (t == null) continue;
                            foreach (var mt in markerTypes)
                            {
                                if (mt.IsAssignableFrom(t)) { looksLikeHud = true; break; }
                            }
                            if (looksLikeHud) break;
                        }
                    }
                    catch { /* ignore and leave looksLikeHud false */ }
                }

                if (looksLikeHud)
                {
                    canvas.gameObject.AddComponent(binderType);
                    Debug.Log($"RuntimeBootstrapValidator: Added PlayerUIBinder to Canvas '{canvas.name}'.");
                }
            }
        }
    }
}
