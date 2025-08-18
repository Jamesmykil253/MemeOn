#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MemeArena.EditorTools
{
    public static class BuildSettingsAuditor
    {
        [MenuItem("Tools/MemeArena/Audit/Build Settings")] 
        public static void Audit()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            bool changed = false;

            bool Has(string name) => scenes.Any(s => s.path.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            void AddIfExistsInProject(string nameContains)
            {
                var guids = AssetDatabase.FindAssets("t:Scene");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (!scenes.Any(s => s.path == path))
                        {
                            scenes.Add(new EditorBuildSettingsScene(path, true));
                            changed = true;
                        }
                        return;
                    }
                }
            }

            // Ensure a bootstrap-like scene exists
            if (!Has("Bootstrap")) AddIfExistsInProject("Bootstrap");
            // Ensure at least one gameplay scene exists
            if (!Has("Gameplay")) AddIfExistsInProject("Gameplay");

            if (changed)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
                EditorUtility.DisplayDialog("Build Settings Audit", "Build Settings updated (added Bootstrap and/or Gameplay scene).", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Build Settings Audit", "Build Settings look OK.", "OK");
            }
        }
    }
}
#endif
