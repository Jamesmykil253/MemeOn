#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scans all scenes and prefabs in the project for deprecated components and offers auto-fix.
/// </summary>
public static class DeprecatedScanner
{
    private static readonly string[] DeprecatedTypes = new[]
    {
        "MemeArena.Players.PlayerController",
        "MemeArena.UI.EnemyWorldUI",
    "MemeArena.Items.LegacyPlayerInventory",
    // Duplicate HUD binder kept for back-compat; migrate to canonical binder
    "MemeArena.UI.PlayerUIBinder",
    // Legacy alias for AI controller (namespace is MemeArena.AI)
    "MemeArena.AI.EnemyController"
    };

    [MenuItem("Tools/MemeArena/Scan Deprecated Components...")]
    public static void ScanProject()
    {
    var report = new List<string>();
    var found = new List<(UnityEngine.Object obj, Component comp, string typeName)>();

        // Capture initially loaded scenes to avoid closing them
        var initiallyLoaded = new HashSet<string>();
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.IsValid()) initiallyLoaded.Add(s.path);
        }

        // Scan project scenes under Assets only (skip Packages to avoid read-only errors)
        var scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => !string.IsNullOrEmpty(p) && p.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();

        foreach (var scenePath in scenePaths)
        {
            try
            {
                var existing = EditorSceneManager.GetSceneByPath(scenePath);
                bool wasLoaded = existing.IsValid() && existing.isLoaded;
                Scene scene;
                if (!wasLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }
                else
                {
                    scene = existing;
                }

                foreach (var go in scene.GetRootGameObjects())
                {
                    ScanGameObject(go, found);
                }

                // Close only scenes we opened; never close initially loaded scenes
                if (!wasLoaded && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DeprecatedScanner: Skipped scene '{scenePath}' ({e.GetType().Name}: {e.Message})");
            }
        }

    // Scan prefabs
    var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var gos = prefab.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject);
            foreach (var go in gos)
            {
        ScanGameObject(go, found);
            }
        }

        foreach (var (obj, comp, typeName) in found)
        {
            report.Add($"{typeName} -> {GetObjectFullPath(comp)}");
        }

        if (report.Count == 0)
        {
            EditorUtility.DisplayDialog("Deprecated Scanner", "No deprecated components found.", "OK");
            return;
        }

        var msg = "Found deprecated components:\n\n" + string.Join("\n", report.Take(50)) + (report.Count > 50 ? $"\n...and {report.Count - 50} more" : "");
        if (EditorUtility.DisplayDialog("Deprecated Scanner", msg, "Auto-Fix (Disable/Replace)", "Cancel"))
        {
            AutoFix(found);
        }
    }

    private static void ScanGameObject(GameObject go, List<(UnityEngine.Object, Component, string)> found)
    {
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue; // missing script
            var t = c.GetType();
            var full = t.FullName;
            if (DeprecatedTypes.Contains(full))
            {
                found.Add((go, c, full));
            }
        }
    }

    private static void AutoFix(List<(UnityEngine.Object obj, Component comp, string typeName)> found)
    {
        int disabled = 0, replaced = 0;
        foreach (var (obj, comp, typeName) in found)
        {
            if (typeName == "MemeArena.Players.PlayerController")
            {
                var mb = comp as MonoBehaviour;
                if (mb != null) mb.enabled = false;
                disabled++;
            }
            else if (typeName == "MemeArena.UI.EnemyWorldUI")
            {
                // Prefer modular replacement if possible: add EnemyUnitUI and try to set refs
                var go = comp.gameObject;
                AddIfTypeExists(go, "MemeArena.UI.EnemyUnitUI");
                // Keep the old component disabled (don’t delete to avoid breaking serialized data unexpectedly)
                var mb = comp as MonoBehaviour; if (mb != null) mb.enabled = false;
                replaced++;
            }
            else if (typeName == "MemeArena.Items.LegacyPlayerInventory")
            {
                var root = (comp as Component).gameObject;
                AddIfTypeExists(root, "MemeArena.Players.PlayerInventory");
                // Keep legacy disabled
                var mb = comp as MonoBehaviour; if (mb != null) mb.enabled = false;
                replaced++;
            }
            else if (typeName == "MemeArena.UI.PlayerUIBinder")
            {
                var go = comp.gameObject;
                // Add canonical binder if missing
                AddIfTypeExists(go, "MemeArena.HUD.PlayerHUDBinder");
                // Disable deprecated alias to avoid double binding
                var mb = comp as MonoBehaviour; if (mb != null) mb.enabled = false;
                replaced++;
            }
            else if (typeName == "MemeArena.AI.EnemyController")
            {
                var go = comp.gameObject;
                // Add canonical AIController if missing
                AddIfTypeExists(go, "MemeArena.AI.AIController");
                // Disable legacy alias
                var mb = comp as MonoBehaviour; if (mb != null) mb.enabled = false;
                replaced++;
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Deprecated Scanner", $"Disabled: {disabled}\nReplaced: {replaced}", "OK");
    }

    private static string GetObjectFullPath(Component comp)
    {
        if (comp == null) return "<null>";
        var go = comp.gameObject;
        var path = GetHierarchyPath(go.transform);
        if (go.scene.IsValid())
        {
            return $"{go.scene.path}:{path} ({comp.GetType().Name})";
        }
        var assetPath = AssetDatabase.GetAssetPath(go);
        if (string.IsNullOrEmpty(assetPath)) assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go));
        if (string.IsNullOrEmpty(assetPath)) assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
        if (string.IsNullOrEmpty(assetPath)) assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
        if (string.IsNullOrEmpty(assetPath)) assetPath = AssetDatabase.GetAssetPath(go);
        return $"{assetPath}:{path} ({comp.GetType().Name})";
    }

    private static string GetHierarchyPath(Transform t)
    {
        if (t == null) return "<null>";
        var names = new System.Collections.Generic.List<string>();
        while (t != null)
        {
            names.Add(t.name);
            t = t.parent;
        }
        names.Reverse();
        return string.Join("/", names);
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
}
#endif
