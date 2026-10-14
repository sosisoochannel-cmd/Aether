#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AetherProjectSetup
{
    private const string ScenePath = "Assets/Aether/Scenes/AetherBoot.unity";

    [MenuItem("Aether/Setup/Create or Repair Boot Scene")]
    public static void CreateOrRepairBootScene()
    {
        Directory.CreateDirectory("Assets/Aether/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Aether Game Bootstrap");
        root.AddComponent<AetherGameBootstrap>();
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.transform.position = new Vector3(0f, 1.5f, -10f);
        cameraObject.AddComponent<AudioListener>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Aether boot scene created. This is a graybox prototype, not a final art build.");
    }
}
#endif
