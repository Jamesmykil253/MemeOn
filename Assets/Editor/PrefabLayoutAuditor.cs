#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MemeArena.EditorTools
{
    public static class PrefabLayoutAuditor
    {
        private class CanonicalPrefab
        {
            public string name;              // e.g., Player.prefab
            public string canonicalPath;     // full asset path under Assets
            public bool required;            // if missing, we can build
        }

        private static readonly CanonicalPrefab[] Canonical = new[]
        {
            new CanonicalPrefab{ name = "Player.prefab", canonicalPath = "Assets/Resources/NetworkPrefabs/Player.prefab", required = true },
            new CanonicalPrefab{ name = "Enemy.prefab", canonicalPath = "Assets/Resources/NetworkPrefabs/Enemy.prefab", required = true },
            new CanonicalPrefab{ name = "Projectile.prefab", canonicalPath = "Assets/Resources/NetworkPrefabs/Projectile.prefab", required = true },
            new CanonicalPrefab{ name = "MeleeHitbox.prefab", canonicalPath = "Assets/Resources/NetworkPrefabs/MeleeHitbox.prefab", required = true },
            new CanonicalPrefab{ name = "UI.prefab", canonicalPath = "Assets/Prefabs/UI/UI.prefab", required = false },
        };

        [MenuItem("Tools/MemeArena/Audit Prefab Layout...")]
        public static void Audit()
        {
            var issues = new List<string>();
            var moves = new List<(string from, string to)>();
            var missing = new List<CanonicalPrefab>();

            foreach (var cp in Canonical)
            {
                var existingAtCanonical = AssetDatabase.LoadAssetAtPath<GameObject>(cp.canonicalPath);
                var foundByName = FindPrefabsByName(cp.name).ToArray();

                if (existingAtCanonical != null)
                {
                    // Good; note duplicates elsewhere
                    foreach (var (path, _) in foundByName)
                    {
                        if (!PathsEqual(path, cp.canonicalPath))
                        {
                            issues.Add($"Duplicate '{cp.name}' found at {path}. Consider removing or renaming.");
                        }
                    }
                    continue;
                }

                if (foundByName.Length == 0)
                {
                    issues.Add($"Missing '{cp.name}' at {cp.canonicalPath}.");
                    missing.Add(cp);
                }
                else
                {
                    // Candidate to move first occurrence
                    var (path, _) = foundByName[0];
                    issues.Add($"'{cp.name}' located at {path} (expected {cp.canonicalPath}). Will move.");
                    moves.Add((path, cp.canonicalPath));
                }
            }

            var report = issues.Count == 0 ? "All canonical prefabs are in the correct folders." : string.Join("\n", issues);
            int choice = EditorUtility.DisplayDialogComplex(
                "Prefab Layout Audit",
                report,
                "Fix Layout",
                "Build Missing",
                "Close");

            if (choice == 0)
            {
                FixLayout(moves);
            }
            else if (choice == 1)
            {
                BuildMissing(missing);
            }
        }

        [MenuItem("Tools/MemeArena/Auto-Sort All Prefabs...")]
        public static void AutoSortAllPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var moves = new List<(string from, string to)>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue; // skip Packages, etc.
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                var target = ComputeDesignatedPath(go, path);
                if (string.IsNullOrEmpty(target)) continue;
                if (!PathsEqual(path, target))
                {
                    moves.Add((path, target));
                }
            }

            if (moves.Count == 0)
            {
                EditorUtility.DisplayDialog("Auto-Sort Prefabs", "All prefabs are already in designated folders.", "OK");
                return;
            }

            var preview = string.Join("\n", moves.Take(20).Select(m => $"{m.from} -> {m.to}"));
            if (moves.Count > 20) preview += $"\n...and {moves.Count - 20} more";
            if (!EditorUtility.DisplayDialog("Auto-Sort Prefabs", preview, "Apply", "Cancel")) return;

            FixLayout(moves);
        }

        private static string ComputeDesignatedPath(GameObject prefab, string currentPath)
        {
            bool hasNetworkObject = prefab.GetComponentInChildren<Unity.Netcode.NetworkObject>(true) != null;
            bool isPlayer = HasTypeInChildren(prefab, "MemeArena.Players.PlayerMovement");
            bool isEnemy = HasTypeInChildren(prefab, "MemeArena.AI.AIController");
            bool isProjectile = HasTypeInChildren(prefab, "MemeArena.Combat.ProjectileServer");
            bool isMelee = HasTypeInChildren(prefab, "MemeArena.Combat.MeleeWeaponServer");
            bool isHUD = HasTypeInChildren(prefab, "MemeArena.HUD.PlayerHUDBinder");
            bool hasCanvas = prefab.GetComponentInChildren<Canvas>(true) != null;
            bool isWorldUIEnemy = HasTypeInChildren(prefab, "MemeArena.UI.EnemyUnitUI") && AnyWorldSpaceCanvas(prefab);

            string name = Path.GetFileName(currentPath);

            if (hasNetworkObject)
            {
                var dir = "Assets/Resources/NetworkPrefabs";
                if (isPlayer) return Path.Combine(dir, "Player.prefab").Replace('\\','/');
                if (isEnemy) return Path.Combine(dir, "Enemy.prefab").Replace('\\','/');
                if (isProjectile) return Path.Combine(dir, "Projectile.prefab").Replace('\\','/');
                if (isMelee) return Path.Combine(dir, "MeleeHitbox.prefab").Replace('\\','/');
                return Path.Combine(dir, name).Replace('\\','/');
            }

            if (hasCanvas)
            {
                if (isHUD)
                {
                    return "Assets/Prefabs/UI/UI.prefab";
                }
                if (isWorldUIEnemy)
                {
                    return Path.Combine("Assets/Prefabs/UI/World", name).Replace('\\','/');
                }
                return Path.Combine("Assets/Prefabs/UI", name).Replace('\\','/');
            }

            // Uncategorized prefabs → Prefabs/Misc (avoid moving if already under Assets/Prefabs)
            if (currentPath.StartsWith("Assets/Prefabs/", StringComparison.OrdinalIgnoreCase)) return currentPath; 
            return Path.Combine("Assets/Prefabs/Misc", name).Replace('\\','/');
        }

        private static bool HasTypeInChildren(GameObject go, string fullTypeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(x => x.FullName == fullTypeName);
            if (t == null) return false;
            var comps = go.GetComponentsInChildren<Component>(true);
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (t.IsAssignableFrom(c.GetType())) return true;
            }
            return false;
        }

        private static bool AnyWorldSpaceCanvas(GameObject go)
        {
            var canvases = go.GetComponentsInChildren<Canvas>(true);
            return canvases != null && canvases.Any(c => c.renderMode == RenderMode.WorldSpace);
        }

        private static void FixLayout(List<(string from, string to)> moves)
        {
            foreach (var (from, to) in moves)
            {
                EnsureDirectory(Path.GetDirectoryName(to));
                if (PathsEqual(from, to)) continue;

                // If destination exists, create a unique name to avoid overwrite
                var finalTo = to;
                if (File.Exists(to))
                {
                    finalTo = UniquePath(to);
                }

                var err = AssetDatabase.MoveAsset(from, finalTo);
                if (!string.IsNullOrEmpty(err))
                {
                    Debug.LogWarning($"Move failed {from} -> {finalTo}: {err}. Attempting copy.");
                    if (AssetDatabase.CopyAsset(from, finalTo))
                    {
                        Debug.Log($"Copied {from} -> {finalTo}");
                    }
                    else
                    {
                        Debug.LogError($"Copy failed {from} -> {finalTo}");
                    }
                }
                else
                {
                    Debug.Log($"Moved {from} -> {finalTo}");
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Prefab Layout Audit", "Layout fix complete.", "OK");
        }

        private static void BuildMissing(List<CanonicalPrefab> missing)
        {
            bool needCore = missing.Any(m => m.canonicalPath.StartsWith("Assets/Resources/NetworkPrefabs"));
            bool needUI = missing.Any(m => m.canonicalPath.StartsWith("Assets/Prefabs/UI"));

            if (needCore)
            {
                if (!TryInvokeStatic("MemeArena.EditorTools.BuildCorePrefabs", "BuildAll"))
                {
                    Debug.LogError("BuildCorePrefabs not found; ensure Assets/Editor/BuildCorePrefabs.cs exists.");
                }
            }
            if (needUI)
            {
                if (!TryInvokeStatic("MemeArena.EditorTools.BuildUIPrefab", "BuildUIPrefabMenu"))
                {
                    Debug.LogError("BuildUIPrefab not found; ensure Assets/Editor/BuildUIPrefab.cs exists.");
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
            try { mi.Invoke(null, null); return true; } catch (Exception e) { Debug.LogError($"{fullTypeName}.{methodName} invocation failed: {e.Message}"); return false; }
        }

        private static IEnumerable<(string path, GameObject go)> FindPrefabsByName(string fileName)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (go != null) yield return (path, go);
                }
            }
        }

        private static bool PathsEqual(string a, string b)
        {
            return string.Equals(a.Replace('\\','/'), b.Replace('\\','/'), StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        private static string UniquePath(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            int i = 1;
            string candidate;
            do { candidate = Path.Combine(dir, $"{name}-{i}{ext}"); i++; }
            while (File.Exists(candidate));
            return candidate.Replace('\\','/');
        }
    }
}
#endif
