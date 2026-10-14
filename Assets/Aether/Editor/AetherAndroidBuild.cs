#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AetherAndroidBuild
{
    private const string ScenePath = "Assets/Aether/Scenes/AetherBoot.unity";
    private const string OutputPath = "Builds/Android/Aether.apk";

    public static void Build()
    {
        Directory.CreateDirectory("Assets/Aether/Scenes");
        Directory.CreateDirectory("Builds/Android");

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
        PlayerSettings.companyName = "Varellon";
        PlayerSettings.productName = "Aether";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.varellon.aether");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle = false;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = OutputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Aether Android build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors).");
        Debug.Log("Aether APK built: " + Path.GetFullPath(OutputPath) + " (" + report.summary.totalSize + " bytes)");
    }
}
#endif
