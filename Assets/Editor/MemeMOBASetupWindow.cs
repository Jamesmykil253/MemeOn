// Meme Online Battle Arena - MemeMOBASetupWindow
#if false
using UnityEditor;
using UnityEngine;
using System.IO;

public class MemeMOBASetupWindow : EditorWindow
{
    [MenuItem("Meme MOBA/Setup/ Open Setup Window")]
    public static void Open(){ GetWindow<MemeMOBASetupWindow>("Meme MOBA Setup").Show(); }

    private void OnGUI()
    {
        if (GUILayout.Button("Create Folder Structure", GUILayout.Height(26)))
        {
            ProjectFolderSetup.CreateFolders();
            EditorUtility.DisplayDialog("Meme MOBA", "Folder structure ensured.", "OK");
        }
    }
}
#endif
