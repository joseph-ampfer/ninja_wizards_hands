#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class NetworkManagerSceneTools
{
    [MenuItem("Tools/Network/Disable NetworkManager GameObjects In All Scenes")]
    public static void DisableAllNetworkManagerGameObjectsInAllScenes()
    {
        string originalPath = SceneManager.GetActiveScene().path;

        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.StartsWith("Packages/")) continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Disable NetworkManagers",
                        path,
                        (float)i / guids.Length))
                    break;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var managers = Object.FindObjectsByType<NetworkManager>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                foreach (var nm in managers)
                {
                    if (nm == null) continue;
                    Undo.RecordObject(nm.gameObject, "Disable NetworkManager GameObject");
                    if (nm.gameObject.activeSelf)
                    {
                        nm.gameObject.SetActive(false);
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }

                EditorSceneManager.SaveScene(scene);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (!string.IsNullOrEmpty(originalPath))
                EditorSceneManager.OpenScene(originalPath);
        }
    }
}
#endif