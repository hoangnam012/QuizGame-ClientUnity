#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SetupNewRoomUIEditor
{
    [InitializeOnLoadMethod]
    public static void AutoRunOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlaying)
            {
                RunSetupInternal(true);
            }
        };
    }

    [MenuItem("Tools/Rebuild NewRoom Scene UI")]
    public static void ForceRebuild()
    {
        GenerateCleanUISprites.GenerateAll();
        RunSetupInternal(true);
    }

    private static void RunSetupInternal(bool openSceneIfNotActive)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "NewRoomScene")
        {
            if (openSceneIfNotActive)
            {
                currentScene = EditorSceneManager.OpenScene("Assets/Scenes/NewRoomScene.unity");
            }
            else
            {
                return;
            }
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        RoomUIController uiCtrl = Object.FindAnyObjectByType<RoomUIController>();
        if (uiCtrl == null)
        {
            GameObject setupObj = GameObject.Find("RoomSetup");
            if (setupObj == null) setupObj = new GameObject("RoomSetup");
            uiCtrl = setupObj.AddComponent<RoomUIController>();
        }

        uiCtrl.InitializeUI();
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);
        Debug.Log("[SetupNewRoomUIEditor] Successfully rebuilt and saved NewRoomScene UI!");
    }
}
#endif
