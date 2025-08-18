#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MemeArena.EditorTools
{
    public static class FilesystemUnifier
    {
        [MenuItem("Tools/MemeArena/Setup/Unify Filesystem (One-Click)")]
        public static void UnifyMenu()
        {
            var report = UnifyHeadless();
            EditorUtility.DisplayDialog(
                "Filesystem Unifier",
                report,
                "OK");
        }

        // Does not show dialogs; returns a short report string
        public static string UnifyHeadless()
        {
            var sb = new StringBuilder();
            try
            {
                // 1) Ensure project folders exist
                TryInvoke(() => ProjectFolderSetup.CreateFolders(), sb, "Create Folders");

                // 2) Auto-sort prefabs into canonical locations
                int moved = 0;
                TryInvoke(() => moved = PrefabLayoutAuditor.AutoSortAllPrefabsHeadless(), sb, $"Auto-Sort Prefabs -> moved {moved}");

                // 3) Clean duplicate prefabs (safe, unreferenced exact duplicates only)
                (int deleted, int conflicts) dup = (0, 0);
                TryInvoke(() => dup = PrefabLayoutAuditor.CleanPrefabDuplicatesHeadless(), sb, $"Clean Duplicates -> deleted {dup.deleted}, conflicts {dup.conflicts}");

                // 4) Build missing core/UI prefabs if any are required
                TryInvoke(() => PrefabLayoutAuditor.Audit(), sb, "Audit Canonical Prefabs");

                // 5) Audit active scene silently for EventSystem, HUD, NetworkManager
                TryInvoke(() => DevelopmentAuditor.SilentActiveSceneAudit(), sb, "Audit Active Scene");

                // 6) Write a filesystem snapshot for traceability
                TryInvoke(() => WriteFilesystemSnapshot(), sb, "Write Filesystem Snapshot");

                // 7) Save open scenes
                TryInvoke(() => EditorSceneManager.SaveOpenScenes(), sb, "Save Scenes");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR: {ex.Message}");
            }
            return sb.ToString();
        }

        private static void TryInvoke(Action action, StringBuilder log, string label)
        {
            try { action(); log.AppendLine($"✔ {label}"); }
            catch (Exception ex) { log.AppendLine($"✖ {label}: {ex.Message}"); }
        }

        private static void WriteFilesystemSnapshot()
        {
            var root = "Assets";
            var sb = new StringBuilder();
            sb.AppendLine($"Snapshot at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            DumpTree(root, sb, "");
            var outPath = Path.Combine("Assets", "FILESYSTEM_SNAPSHOT.txt");
            File.WriteAllText(outPath, sb.ToString());
            AssetDatabase.ImportAsset(outPath);
        }

        private static void DumpTree(string dir, StringBuilder sb, string indent)
        {
            var subDirs = AssetDatabase.GetSubFolders(dir).OrderBy(d => d).ToArray();
            foreach (var d in subDirs)
            {
                sb.AppendLine($"{indent}{Path.GetFileName(d)}/");
                DumpTree(d, sb, indent + "    ");
            }
            var guids = AssetDatabase.FindAssets(string.Empty, new[] { dir });
            var files = guids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => !string.IsNullOrEmpty(p) && !p.EndsWith("/"))
                .Where(p => Path.GetDirectoryName(p).Replace('\\','/') == dir.Replace('\\','/'))
                .OrderBy(p => p)
                .ToArray();
            foreach (var f in files)
            {
                sb.AppendLine($"{indent}{Path.GetFileName(f)}");
            }
        }
    }
}
#endif
