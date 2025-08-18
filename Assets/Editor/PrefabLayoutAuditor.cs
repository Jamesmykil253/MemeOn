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

        [MenuItem("Tools/MemeArena/Clean Prefab Duplicates...")]
        public static void CleanPrefabDuplicatesMenu()
        {
            // 1) Gather all prefabs grouped by filename
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var nameToPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                var name = Path.GetFileName(path);
                if (!nameToPaths.TryGetValue(name, out var list))
                {
                    list = new List<string>();
                    nameToPaths[name] = list;
                }
                list.Add(path);
            }

            // 2) Build duplicate sets and decide keepers
            var duplicateSets = nameToPaths
                .Where(kvp => kvp.Value.Count > 1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (duplicateSets.Count == 0)
            {
                EditorUtility.DisplayDialog("Clean Prefab Duplicates", "No duplicate prefab filenames found.", "OK");
                return;
            }

            // Helpers for keeper selection
            string GetCanonicalPathFor(string filename)
            {
                var cp = Canonical.FirstOrDefault(c => c.name.Equals(filename, StringComparison.OrdinalIgnoreCase));
                return cp?.canonicalPath;
            }

            string ChooseKeeper(string filename, List<string> paths)
            {
                var canonical = GetCanonicalPathFor(filename);
                if (!string.IsNullOrEmpty(canonical) && paths.Any(p => PathsEqual(p, canonical)))
                {
                    return paths.First(p => PathsEqual(p, canonical));
                }

                // Prefer the path that already matches its designated location
                foreach (var p in paths)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (go == null) continue;
                    var target = ComputeDesignatedPath(go, p);
                    if (!string.IsNullOrEmpty(target) && PathsEqual(p, target)) return p;
                }

                // Prefer Resources/NetworkPrefabs, then Prefabs/
                var prefer = paths.FirstOrDefault(p => p.StartsWith("Assets/Resources/NetworkPrefabs/", StringComparison.OrdinalIgnoreCase))
                             ?? paths.FirstOrDefault(p => p.StartsWith("Assets/Prefabs/", StringComparison.OrdinalIgnoreCase))
                             ?? paths[0];
                return prefer;
            }

            string HashFile(string path)
            {
                try
                {
                    var full = Path.GetFullPath(path);
                    using (var stream = File.OpenRead(full))
                    {
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var hash = md5.ComputeHash(stream);
                            return BitConverter.ToString(hash).Replace("-", string.Empty);
                        }
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }

            // 3) Partition duplicates into exact duplicates (same hash) vs conflicts
            var toCheckReferences = new List<string>();
            var exactDupesByKeeper = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var conflicts = new List<(string filename, List<string> paths)>();

            foreach (var kvp in duplicateSets)
            {
                var filename = kvp.Key;
                var paths = kvp.Value;
                var keeper = ChooseKeeper(filename, paths);
                var keeperHash = HashFile(keeper);
                var exactDupes = new List<string>();
                var nonMatching = new List<string>();

                foreach (var p in paths)
                {
                    if (PathsEqual(p, keeper)) continue;
                    var h = HashFile(p);
                    if (!string.IsNullOrEmpty(keeperHash) && keeperHash == h)
                    {
                        exactDupes.Add(p);
                    }
                    else
                    {
                        nonMatching.Add(p);
                    }
                }

                if (exactDupes.Count > 0)
                {
                    exactDupesByKeeper[keeper] = exactDupes;
                    toCheckReferences.AddRange(exactDupes);
                }

                if (nonMatching.Count > 0)
                {
                    conflicts.Add((filename, new List<string> { keeper }.Concat(nonMatching).ToList()));
                }
            }

            // 4) Build reverse-reference map for exact duplicates using AssetDatabase.GetDependencies
            var candidateSet = new HashSet<string>(toCheckReferences, StringComparer.OrdinalIgnoreCase);
            var referencing = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in candidateSet) referencing[c] = new List<string>();

            // Scan all scenes and prefabs (primary places that reference prefabs)
            var scanGuids = AssetDatabase.FindAssets("t:Scene t:Prefab");
            foreach (var sg in scanGuids)
            {
                var ap = AssetDatabase.GUIDToAssetPath(sg);
                if (string.IsNullOrEmpty(ap)) continue;
                if (!ap.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                var deps = AssetDatabase.GetDependencies(ap, true);
                foreach (var d in deps)
                {
                    if (candidateSet.Contains(d))
                    {
                        referencing[d].Add(ap);
                    }
                }
            }

            // 5) Prepare report
            var deletable = referencing
                .Where(kvp => kvp.Value.Count == 0)
                .Select(kvp => kvp.Key)
                .OrderBy(p => p)
                .ToList();

            var keptPreview = string.Join("\n", exactDupesByKeeper.Select(k => $"KEEP: {k.Key}\n  Duplicates: {string.Join(", ", k.Value)}"));
            var conflictPreview = conflicts.Count == 0 ? "(none)" : string.Join("\n\n", conflicts.Select(c => $"NAME: {c.filename}\n  Variants: {string.Join("\n    ", c.paths)}"));
            var deletablePreview = deletable.Count == 0 ? "(none)" : string.Join("\n", deletable);

            var summary =
                $"Duplicate filename groups: {duplicateSets.Count}\n" +
                $"Exact duplicate sets: {exactDupesByKeeper.Count}\n" +
                $"Deletable (unreferenced) duplicates: {deletable.Count}\n\n" +
                "— Keepers and their exact duplicates —\n" + keptPreview + "\n\n" +
                "— Unreferenced exact duplicates (safe to delete) —\n" + deletablePreview + "\n\n" +
                "— Conflicting variants (different content; review manually) —\n" + conflictPreview + "\n\n" +
                "Proceed to delete only the unreferenced exact duplicates?";

            int choice = EditorUtility.DisplayDialogComplex(
                "Clean Prefab Duplicates",
                summary,
                "Delete Unreferenced",
                "Preview Only",
                "Cancel");

            if (choice == 2) return; // Cancel
            if (choice == 1) return; // Preview only

            // 6) Delete the unreferenced exact duplicates
            int deleted = 0;
            foreach (var p in deletable)
            {
                if (AssetDatabase.DeleteAsset(p))
                {
                    Debug.Log($"Deleted duplicate prefab: {p}");
                    deleted++;
                }
                else
                {
                    Debug.LogWarning($"Failed to delete duplicate prefab: {p}");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Clean Prefab Duplicates", $"Deleted {deleted} duplicate prefab(s).", "OK");
        }

        // Headless duplicate cleaner: deletes only unreferenced exact duplicates and returns counts
        public static (int deleted, int conflicts) CleanPrefabDuplicatesHeadless()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var nameToPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                var name = System.IO.Path.GetFileName(path);
                if (!nameToPaths.TryGetValue(name, out var list))
                {
                    list = new List<string>();
                    nameToPaths[name] = list;
                }
                list.Add(path);
            }

            string GetCanonicalPathFor(string filename)
            {
                var cp = Canonical.FirstOrDefault(c => c.name.Equals(filename, StringComparison.OrdinalIgnoreCase));
                return cp?.canonicalPath;
            }

            string ChooseKeeper(string filename, List<string> paths)
            {
                var canonical = GetCanonicalPathFor(filename);
                if (!string.IsNullOrEmpty(canonical) && paths.Any(p => PathsEqual(p, canonical)))
                {
                    return paths.First(p => PathsEqual(p, canonical));
                }
                foreach (var p in paths)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (go == null) continue;
                    var target = ComputeDesignatedPath(go, p);
                    if (!string.IsNullOrEmpty(target) && PathsEqual(p, target)) return p;
                }
                return paths.FirstOrDefault(p => p.StartsWith("Assets/Resources/NetworkPrefabs/", StringComparison.OrdinalIgnoreCase))
                       ?? paths.FirstOrDefault(p => p.StartsWith("Assets/Prefabs/", StringComparison.OrdinalIgnoreCase))
                       ?? paths[0];
            }

            string HashFile(string path)
            {
                try
                {
                    using (var stream = System.IO.File.OpenRead(System.IO.Path.GetFullPath(path)))
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", string.Empty);
                    }
                }
                catch { return string.Empty; }
            }

            var duplicateSets = nameToPaths.Where(kvp => kvp.Value.Count > 1).ToDictionary(k => k.Key, v => v.Value);
            var toCheck = new List<string>();
            int conflictCount = 0;

            foreach (var kvp in duplicateSets)
            {
                var keeper = ChooseKeeper(kvp.Key, kvp.Value);
                var keeperHash = HashFile(keeper);
                int nonMatching = 0;
                foreach (var p in kvp.Value)
                {
                    if (PathsEqual(p, keeper)) continue;
                    var h = HashFile(p);
                    if (string.IsNullOrEmpty(keeperHash) || keeperHash != h) nonMatching++;
                    else toCheck.Add(p);
                }
                if (nonMatching > 0) conflictCount++;
            }

            var candidateSet = new HashSet<string>(toCheck, StringComparer.OrdinalIgnoreCase);
            var referencing = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in candidateSet) referencing[c] = new List<string>();
            var scanGuids = AssetDatabase.FindAssets("t:Scene t:Prefab");
            foreach (var sg in scanGuids)
            {
                var ap = AssetDatabase.GUIDToAssetPath(sg);
                if (string.IsNullOrEmpty(ap)) continue;
                if (!ap.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                var deps = AssetDatabase.GetDependencies(ap, true);
                foreach (var d in deps) if (candidateSet.Contains(d)) referencing[d].Add(ap);
            }

            int deleted = 0;
            foreach (var p in referencing.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key))
            {
                if (AssetDatabase.DeleteAsset(p)) deleted++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return (deleted, conflictCount);
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

        // Headless variant used by Filesystem Unifier (no dialogs)
        public static int AutoSortAllPrefabsHeadless()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var moves = new List<(string from, string to)>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                var target = ComputeDesignatedPath(go, path);
                if (string.IsNullOrEmpty(target)) continue;
                if (!PathsEqual(path, target)) moves.Add((path, target));
            }
            if (moves.Count > 0) FixLayout(moves);
            return moves.Count;
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
