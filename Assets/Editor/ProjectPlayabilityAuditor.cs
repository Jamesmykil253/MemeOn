#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemeArena.EditorTools
{
    public static class ProjectPlayabilityAuditor
    {
        [MenuItem("Tools/MemeArena/Audit Playability (Active Scene)...")]
        public static void AuditActiveScene()
        {
            var report = new System.Text.StringBuilder();
            int fixes = 0;

            // 1) Ensure core prefabs/UI exist on disk
            if (!AssetExists("Assets/Resources/NetworkPrefabs/Player.prefab") ||
                !AssetExists("Assets/Resources/NetworkPrefabs/Enemy.prefab") ||
                !AssetExists("Assets/Resources/NetworkPrefabs/Projectile.prefab") ||
                !AssetExists("Assets/Resources/NetworkPrefabs/MeleeHitbox.prefab"))
            {
                TryInvokeStatic("MemeArena.EditorTools.BuildCorePrefabs", "BuildAll");
                report.AppendLine("Built core network prefabs.");
                fixes++;
            }
            if (!AssetExists("Assets/Prefabs/UI/UI.prefab"))
            {
                TryInvokeStatic("MemeArena.EditorTools.BuildUIPrefab", "BuildUIPrefabMenu");
                report.AppendLine("Built UI.prefab.");
                fixes++;
            }

            // 2) Ensure prefabs laid out canonically
            TryInvokeStatic("MemeArena.EditorTools.PrefabLayoutAuditor", "AutoSortAllPrefabs");

            // 3) Validate active scene essentials
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Audit Playability", "Active scene is not valid.", "OK");
                return;
            }

            // Create or find runtime root
            var root = GameObject.Find("_BootstrapRuntime") ?? new GameObject("_BootstrapRuntime");
            Undo.RegisterCreatedObjectUndo(root, "Create _BootstrapRuntime");

            // NetworkManager
            var nmFound = UnityEngine.Object.FindFirstObjectByType<Unity.Netcode.NetworkManager>(FindObjectsInactive.Exclude);
            var nmGo = nmFound != null ? nmFound.gameObject : null;
            if (nmGo == null)
            {
                nmGo = new GameObject("NetworkManager");
                nmGo.AddComponent<Unity.Netcode.NetworkManager>();
                // Try adding UnityTransport if available
                AddIfTypeExists(nmGo, "Unity.Netcode.Transports.UTP.UnityTransport");
                Undo.RegisterCreatedObjectUndo(nmGo, "Create NetworkManager");
                report.AppendLine("Added NetworkManager (and UnityTransport if available).");
                fixes++;
            }

            // NetworkGameManager + registrar
            var ngm = UnityEngine.Object.FindFirstObjectByType<MemeArena.Networking.NetworkGameManager>(FindObjectsInactive.Exclude);
            if (ngm == null)
            {
                var go = new GameObject("NetworkGame");
                ngm = go.AddComponent<MemeArena.Networking.NetworkGameManager>();
                var registrar = go.AddComponent<MemeArena.Networking.NetworkPrefabsRegistrar>();
                ngm.registrar = registrar;
                Undo.RegisterCreatedObjectUndo(go, "Create NetworkGameManager");
                report.AppendLine("Added NetworkGameManager + NetworkPrefabsRegistrar.");
                fixes++;
            }

            // SceneLoaderOnHost
            var loader = UnityEngine.Object.FindFirstObjectByType<MemeArena.Networking.SceneLoaderOnHost>(FindObjectsInactive.Exclude);
            if (loader == null)
            {
                loader = ngm != null ? ngm.gameObject.AddComponent<MemeArena.Networking.SceneLoaderOnHost>()
                                      : new GameObject("SceneLoader").AddComponent<MemeArena.Networking.SceneLoaderOnHost>();
                loader.gameplaySceneName = FindGameplaySceneName() ?? loader.gameplaySceneName;
                Undo.RegisterCreatedObjectUndo(loader.gameObject, "Create SceneLoaderOnHost");
                report.AppendLine($"Added SceneLoaderOnHost (gameplay='{loader.gameplaySceneName}').");
                fixes++;
            }

            // Main Camera with CameraBootstrap + controller
            var firstCam = UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
            var cam = Camera.main != null ? Camera.main.gameObject : firstCam != null ? firstCam.gameObject : null;
            if (cam == null)
            {
                cam = new GameObject("Main Camera");
                cam.tag = "MainCamera";
                cam.AddComponent<Camera>();
                cam.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(cam, "Create Main Camera");
                report.AppendLine("Created Main Camera + AudioListener.");
                fixes++;
            }
            AddIfTypeExists(cam, "MemeArena.CameraSystem.UniteCameraController");
            AddIfTypeExists(cam, "MemeArena.CameraSystem.CameraBootstrap");

            // HUD in scene
            bool hudPresent = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                                     .Any(c => c.GetComponent<MemeArena.HUD.PlayerHUDBinder>() != null);
            if (!hudPresent)
            {
                var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UI.prefab");
                if (hud != null)
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(hud, scene);
                    inst.name = "UI";
                    Undo.RegisterCreatedObjectUndo(inst, "Instantiate UI");
                    report.AppendLine("Instantiated UI.prefab in scene.");
                    fixes++;
                }
            }

            if (fixes > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            // Final notification
            if (report.Length == 0) report.AppendLine("No changes were required. Scene looks ready to play.");
            EditorUtility.DisplayDialog("Audit Playability", report.ToString(), "OK");
        }

        private static string FindGameplaySceneName()
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            string firstGameplay = null;
            string firstOther = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("Gameplay", StringComparison.OrdinalIgnoreCase))
                    return name;
                if (firstOther == null) firstOther = name;
            }
            return firstGameplay ?? firstOther; // prefer gameplay-like, else any
        }

        private static bool AssetExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null;
        }

        private static void AddIfTypeExists(GameObject go, string fullTypeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t != null && typeof(Component).IsAssignableFrom(t))
            {
                if (go.GetComponent(t) == null)
                {
                    go.AddComponent(t);
                }
            }
        }

        private static bool TryInvokeStatic(string fullTypeName, string methodName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t == null) return false;
            var mi = t.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (mi == null) return false;
            try { mi.Invoke(null, null); return true; } catch { return false; }
        }
    }
}
#endif
