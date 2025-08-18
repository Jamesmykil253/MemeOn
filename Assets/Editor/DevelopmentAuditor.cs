#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MemeArena.EditorTools
{
    /// <summary>
    /// Centralizes lightweight, automatic audits during development and build.
    /// - Optional auto-audit on entering Play Mode (no dialogs, safe fixes).
    /// - Build-time checks for common pitfalls (duplicate EventSystems, TMP example CameraController).
    /// - Single-task menu items to run existing tools.
    /// </summary>
    [InitializeOnLoad]
    public static class DevelopmentAuditor
    {
        private const string AutoAuditOnPlayKey = "MemeArena.AutoAuditOnPlay";
        private const string AutoFixEventSystemsOnPlayKey = "MemeArena.AutoFixEventSystemsOnPlay";
    private const string AutoCleanReferencedPrefabsOnPlayKey = "MemeArena.AutoCleanReferencedPrefabsOnPlay";

        static DevelopmentAuditor()
        {
            if (!EditorPrefs.HasKey(AutoAuditOnPlayKey)) EditorPrefs.SetBool(AutoAuditOnPlayKey, true);
                if (!EditorPrefs.HasKey(AutoFixEventSystemsOnPlayKey)) EditorPrefs.SetBool(AutoFixEventSystemsOnPlayKey, true);
                if (!EditorPrefs.HasKey(AutoCleanReferencedPrefabsOnPlayKey)) EditorPrefs.SetBool(AutoCleanReferencedPrefabsOnPlayKey, false);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(AutoAuditOnPlayKey, true))
            {
                try
                {
                    SilentActiveSceneAudit();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"DevelopmentAuditor: silent audit failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Runs a silent, non-intrusive audit and light auto-fixes:
        /// - Ensure exactly one EventSystem (keeps the first, destroys extras; can create one if missing).
        /// - Disable TMP Example CameraController that reads legacy Input.
        /// </summary>
        [MenuItem("Tools/MemeArena/Audit/Active Scene (Silent)")]
        public static void SilentActiveSceneAudit()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            // Proactively clean broken MonoBehaviours to avoid missing-script spam
                if (EditorPrefs.GetBool(AutoCleanReferencedPrefabsOnPlayKey, false))
                    CleanMissingScriptsInPrefabsReferencedByScene(scene);
                CleanMissingScriptsInSceneInstances(scene);

            if (EditorPrefs.GetBool(AutoFixEventSystemsOnPlayKey, true))
                EnsureSingleEventSystem();

            DisableTMPExampleCameraController();
            EnsureSingleNetworkManager();
            EnsureSingleHUD(scene);
        }

        [MenuItem("Tools/MemeArena/Autopilot/Auto Audit On Play")]
        private static void ToggleAutoAuditOnPlay()
        {
            var v = !EditorPrefs.GetBool(AutoAuditOnPlayKey, true);
            EditorPrefs.SetBool(AutoAuditOnPlayKey, v);
        }

        private static void CleanMissingScriptsInPrefabsReferencedByScene(UnityEngine.SceneManagement.Scene scene)
        {
            try
            {
                var scenePath = scene.path;
                if (string.IsNullOrEmpty(scenePath)) return;
                var deps = AssetDatabase.GetDependencies(scenePath, true);
                int fixedCount = 0;
                foreach (var dep in deps)
                {
                    if (!dep.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                    // Only bother with UI-ish prefabs or those likely to include Canvas
                    // We'll still clean generically; the check is just for early skip if needed later
                    fixedCount += TryCleanMissingScriptsInPrefab(dep) ? 1 : 0;
                }
                if (fixedCount > 0)
                {
                    Debug.Log($"DevelopmentAuditor: Cleaned missing scripts in {fixedCount} prefab(s) referenced by scene '{scene.name}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DevelopmentAuditor: Prefab cleanup failed: {ex.Message}");
            }
        }

        private static void CleanMissingScriptsInSceneInstances(UnityEngine.SceneManagement.Scene scene)
        {
            try
            {
                int removed = 0;
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    removed += RemoveMissingScriptsRecursive(root);
                }
                if (removed > 0)
                {
                    Debug.Log($"DevelopmentAuditor: Removed {removed} missing script component(s) from scene instances.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DevelopmentAuditor: Scene instance cleanup failed: {ex.Message}");
            }
        }

        [MenuItem("Tools/MemeArena/Autopilot/Auto Audit On Play", true)]
        private static bool ToggleAutoAuditOnPlayValidate()
        {
            Menu.SetChecked("Tools/MemeArena/Autopilot/Auto Audit On Play", EditorPrefs.GetBool(AutoAuditOnPlayKey, true));
            return true;
        }

        [MenuItem("Tools/MemeArena/Autopilot/Fix EventSystems On Play")]
        private static void ToggleFixEventSystemsOnPlay()
        {
            var v = !EditorPrefs.GetBool(AutoFixEventSystemsOnPlayKey, true);
            EditorPrefs.SetBool(AutoFixEventSystemsOnPlayKey, v);
        }

        [MenuItem("Tools/MemeArena/Autopilot/Fix EventSystems On Play", true)]
        private static bool ToggleFixEventSystemsOnPlayValidate()
        {
            Menu.SetChecked("Tools/MemeArena/Autopilot/Fix EventSystems On Play", EditorPrefs.GetBool(AutoFixEventSystemsOnPlayKey, true));
            return true;
        }

            [MenuItem("Tools/MemeArena/Autopilot/Clean Referenced Prefabs On Play")]
            private static void ToggleCleanReferencedPrefabsOnPlay()
            {
                var v = !EditorPrefs.GetBool(AutoCleanReferencedPrefabsOnPlayKey, false);
                EditorPrefs.SetBool(AutoCleanReferencedPrefabsOnPlayKey, v);
            }

            [MenuItem("Tools/MemeArena/Autopilot/Clean Referenced Prefabs On Play", true)]
            private static bool ToggleCleanReferencedPrefabsOnPlayValidate()
            {
                Menu.SetChecked("Tools/MemeArena/Autopilot/Clean Referenced Prefabs On Play", EditorPrefs.GetBool(AutoCleanReferencedPrefabsOnPlayKey, false));
                return true;
            }

        // Single-task convenience menu items that call existing tools
        [MenuItem("Tools/MemeArena/Build/Create Core Prefabs")]
        private static void CreateCorePrefabs() => SafeInvoke(() => BuildCorePrefabs.BuildAll());

        [MenuItem("Tools/MemeArena/Build/Create UI Prefab")] 
        private static void CreateUIPrefab() => SafeInvoke(() => BuildUIPrefab.BuildUIPrefabMenu());

        [MenuItem("Tools/MemeArena/Audit/Prefabs Layout")] 
        private static void AuditPrefabsLayout() => SafeInvoke(() => PrefabLayoutAuditor.AutoSortAllPrefabs());

    [MenuItem("Tools/MemeArena/Audit/Deprecated Components")] 
    private static void AuditDeprecated() => SafeInvoke(() => DeprecatedScanner.ScanProject());

        [MenuItem("Tools/MemeArena/Audit/Active Scene (Interactive)")] 
        private static void AuditActiveSceneInteractive() => SafeInvoke(() => ProjectPlayabilityAuditor.AuditActiveScene());

    [MenuItem("Tools/MemeArena/Setup/Create Project Folders")] 
    private static void SetupFolders() => SafeInvoke(() => ProjectFolderSetup.CreateFolders());

        private static void SafeInvoke(Action a)
        {
            try { a(); } catch (Exception ex) { Debug.LogError($"DevelopmentAuditor: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"); }
        }

        private static void EnsureSingleEventSystem()
        {
            var systems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (systems == null || systems.Length == 0)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                // Add our persistent wrapper so it survives scene loads
                AddIfTypeExists(go, "MemeArena.UI.PersistentEventSystem");
                // Prefer Input System module if available; fallback to StandaloneInputModule
                var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputSystemModuleType != null)
                {
                    go.AddComponent(inputSystemModuleType);
                }
                else
                {
                    go.AddComponent<StandaloneInputModule>();
                }
                Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
                Debug.Log("DevelopmentAuditor: Created EventSystem.");
            }
            else if (systems.Length > 1)
            {
                // Keep the first enabled one, disable/destroy the rest
                var keep = systems.FirstOrDefault(s => s.isActiveAndEnabled) ?? systems[0];
                AddIfTypeExists(keep.gameObject, "MemeArena.UI.PersistentEventSystem");
                foreach (var es in systems)
                {
                    if (es == keep) continue;
                    Undo.DestroyObjectImmediate(es.gameObject);
                }
                Debug.LogWarning($"DevelopmentAuditor: Found {systems.Length} EventSystems; kept one and removed the rest.");
            }
        }

        private static void DisableTMPExampleCameraController()
        {
            // Avoid hard assembly dependency: find MonoBehaviours matching the full name and disable them
            var allMB = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in allMB)
            {
                if (mb == null) continue;
                var t = mb.GetType();
                if (t != null && t.FullName == "TMPro.Examples.CameraController")
                {
                    if (mb.enabled)
                    {
                        mb.enabled = false;
                        Debug.LogWarning("DevelopmentAuditor: Disabled TMP Examples CameraController (uses legacy Input). Use UniteCameraController instead.");
                    }
                }
            }
        }

        private static void EnsureSingleNetworkManager()
        {
            var nms = UnityEngine.Object.FindObjectsByType<Unity.Netcode.NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (nms == null || nms.Length == 0)
            {
                var nmGo = new GameObject("NetworkManager");
                nmGo.AddComponent<Unity.Netcode.NetworkManager>();
                // Add Unity Transport if available
                var utp = Type.GetType("Unity.Netcode.Transports.UTP.UnityTransport, Unity.Netcode.Transports.UTP");
                if (utp != null) nmGo.AddComponent(utp);
                Undo.RegisterCreatedObjectUndo(nmGo, "Create NetworkManager");
                Debug.Log("DevelopmentAuditor: Created NetworkManager.");
            }
            else if (nms.Length > 1)
            {
                var keep = nms.FirstOrDefault(m => m.isActiveAndEnabled) ?? nms[0];
                foreach (var nm in nms)
                {
                    if (nm == keep) continue;
                    Undo.DestroyObjectImmediate(nm.gameObject);
                }
                Debug.LogWarning($"DevelopmentAuditor: Found {nms.Length} NetworkManagers; kept one and removed the rest.");
            }
        }

        private static void EnsureSingleHUD(UnityEngine.SceneManagement.Scene scene)
        {
            // Detect canvases that host the canonical binder OR the migrator alias
            bool IsBinderOrAlias(MonoBehaviour mb)
            {
                if (mb == null) return false;
                var fn = mb.GetType().FullName;
                return fn == "MemeArena.HUD.PlayerHUDBinder" || fn == "MemeArena.UI.PlayerUIBinder";
            }

            var binders = UnityEngine.Object
                .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(IsBinderOrAlias)
                .ToArray();

            if (binders.Length == 0)
            {
                // Try to instantiate UI.prefab
                var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UI.prefab");
                if (hud != null)
                {
                    // Strip EventSystem inside the prefab to avoid duplicates; PersistentEventSystem will own it globally
                    TryStripComponentFromPrefab("Assets/Prefabs/UI/UI.prefab", typeof(EventSystem));
                    // Clean missing scripts in the prefab asset before instantiation to avoid warnings
                    TryCleanMissingScriptsInPrefab("Assets/Prefabs/UI/UI.prefab");

                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(hud, scene);
                    inst.name = "UI";

                    // Clean any missing scripts on the instance as a last line of defense
                    RemoveMissingScriptsRecursive(inst);

                    // Ensure the binder exists on the instance; add reflectively if missing
                    if (!HasComponentByFullNameInChildren(inst, "MemeArena.HUD.PlayerHUDBinder") &&
                        !HasComponentByFullNameInChildren(inst, "MemeArena.UI.PlayerUIBinder"))
                    {
                        AddIfTypeExists(inst, "MemeArena.HUD.PlayerHUDBinder");
                    }

                    Undo.RegisterCreatedObjectUndo(inst, "Instantiate HUD UI");
                    Debug.Log("DevelopmentAuditor: Instantiated HUD UI.prefab.");
                }
            }
            else if (binders.Length > 1)
            {
                // Keep the first active one; remove others at the root canvas level
                var keep = binders.FirstOrDefault(b => (b as Behaviour)?.isActiveAndEnabled == true) ?? binders[0];
                foreach (var b in binders)
                {
                    if (b == keep) continue;
                    // Destroy the top-level canvas hosting this binder
                    var canvas = b.GetComponentInParent<Canvas>(true)?.gameObject ?? b.gameObject;
                    Undo.DestroyObjectImmediate(canvas);
                }
                Debug.LogWarning($"DevelopmentAuditor: Found {binders.Length} HUD binders; kept one and removed the rest.");
            }
        }

        // Helpers
        private static bool TryStripComponentFromPrefab(string assetPath, Type componentType)
        {
            try
            {
                var root = PrefabUtility.LoadPrefabContents(assetPath);
                if (root == null) return false;
                var targets = root.GetComponentsInChildren(componentType, true);
                foreach (var c in targets)
                {
                    if (c is Component comp)
                    {
                        UnityEngine.Object.DestroyImmediate(comp, true);
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                PrefabUtility.UnloadPrefabContents(root);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DevelopmentAuditor: Failed to strip {componentType.Name} from '{assetPath}': {ex.Message}");
                return false;
            }
        }
        private static bool TryCleanMissingScriptsInPrefab(string assetPath)
        {
            try
            {
                var contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
                int removed = RemoveMissingScriptsRecursive(contentsRoot);
                bool changed = removed > 0;
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                if (changed)
                {
                    Debug.Log($"DevelopmentAuditor: Cleaned {removed} missing script component(s) in '{assetPath}'.");
                }
                return changed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DevelopmentAuditor: Failed cleaning missing scripts in '{assetPath}': {ex.Message}");
                return false;
            }
        }

        private static int RemoveMissingScriptsRecursive(GameObject root)
        {
            if (root == null) return 0;
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            var t = root.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                removed += RemoveMissingScriptsRecursive(t.GetChild(i).gameObject);
            }
            return removed;
        }

        private static bool HasComponentByFullNameInChildren(GameObject root, string fullTypeName)
        {
            if (root == null) return false;
            var comps = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in comps)
            {
                if (mb == null) continue;
                if (mb.GetType().FullName == fullTypeName) return true;
            }
            return false;
        }

        private static Component AddIfTypeExists(GameObject go, string fullTypeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t == null) return null;
            return go.AddComponent(t);
        }
    }

    /// <summary>
    /// Build-time processing: lightweight checks & fixes while building.
    /// </summary>
    public class BuildAuditProcessor : IPreprocessBuildWithReport, IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Build settings sanity: at least 1 enabled scene
            var enabledScenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (enabledScenes.Length == 0)
            {
                throw new BuildFailedException("No scenes are enabled in Build Settings.");
            }
            // Warn if no gameplay-like scene
            if (!enabledScenes.Any(s => s.path.IndexOf("Gameplay", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Debug.LogWarning("BuildAudit: No scene with name containing 'Gameplay' found in Build Settings.");
            }
        }

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            // Enforce single EventSystem and disable TMP CameraController in the scene being built
            try
            {
                DevelopmentAuditor.SilentActiveSceneAudit();
                    if (!scene.IsValid() || !scene.isLoaded) return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"BuildAudit: scene '{scene.name}' audit hit: {ex.Message}");
            }
        }
    }
}
#endif
