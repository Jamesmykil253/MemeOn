// Meme Online Battle Arena - ProjectFolderSetup
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class ProjectFolderSetup
{
    [MenuItem("Meme MOBA/Setup Project Folders")]
    public static void CreateFolders()
    {
        string[] folders = new string[]
        {
            // Core
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/Core",
            "Assets/Scripts/AI",
            "Assets/Scripts/Combat",
            "Assets/Scripts/Debug",
            "Assets/Art",
            "Assets/Resources",
            "Assets/Settings",
            "Assets/Editor",
            "Assets/Tests",

            // Canonical prefab locations used by auditors/builders
            "Assets/Resources/NetworkPrefabs",
            "Assets/Prefabs",
            "Assets/Prefabs/Players",
            "Assets/Prefabs/AI",
            "Assets/Prefabs/Projectiles",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/UI/World",
            "Assets/Prefabs/Misc",
        };
        foreach (var f in folders) EnsureFolder(f);
        AssetDatabase.SaveAssets();
        Debug.Log("Meme MOBA: Folder structure ensured.");
    }
    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
