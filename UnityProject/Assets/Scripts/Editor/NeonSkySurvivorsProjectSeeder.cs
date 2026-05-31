using System.IO;
using NeonSkySurvivors.Runtime.App;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSkySurvivors.Editor
{
    public static class NeonSkySurvivorsProjectSeeder
    {
        private const string SceneDirectory = "Assets/Scenes";
        private const string ScenePath = SceneDirectory + "/Boot.unity";

        [InitializeOnLoadMethod]
        private static void EnsureProjectOnLoad()
        {
            EditorApplication.delayCall += EnsureProject;
        }

        [MenuItem("Tools/Neon Sky Survivors/Seed Mobile Boot Scene")]
        public static void EnsureProject()
        {
            EnsurePlayerSettings();
            EnsureBootScene();
            EnsureBuildSettings();
        }

        [MenuItem("Tools/Neon Sky Survivors/Verify Mobile Boot Scene")]
        public static void VerifyMobileBootScene()
        {
            EnsureProject();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Require(scene.IsValid() && scene.isLoaded, "Boot scene could not be loaded.");
            Require(UnityEngine.Object.FindAnyObjectByType<NeonSkySurvivorsApp>() != null, "Boot scene is missing NeonSkySurvivorsApp.");
            Require(Camera.main != null, "Boot scene is missing a camera tagged MainCamera.");
            Require(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait, "Default orientation must be Portrait.");
            Require(PlayerSettings.allowedAutorotateToPortrait, "Portrait autorotation must be enabled.");
            Require(!PlayerSettings.allowedAutorotateToLandscapeLeft && !PlayerSettings.allowedAutorotateToLandscapeRight, "Landscape autorotation must be disabled.");
            Require(EditorBuildSettings.scenes.Length == 1 && EditorBuildSettings.scenes[0].path == ScenePath && EditorBuildSettings.scenes[0].enabled, "Build settings must contain only the enabled Boot scene.");

            Debug.Log("Neon Sky Survivors mobile Boot scene verification passed.");
        }

        [MenuItem("Tools/Neon Sky Survivors/Build Android Smoke APK")]
        public static void BuildAndroidSmokeTest()
        {
            EnsureProject();

            EditorUserBuildSettings.buildAppBundle = false;

            var buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds", "Android"));
            Directory.CreateDirectory(buildDirectory);

            var buildPath = Path.Combine(buildDirectory, "NeonSkySurvivors-Smoke.apk");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });

            Require(report.summary.result == BuildResult.Succeeded, "Android smoke build failed: " + report.summary.result);
            Require(File.Exists(buildPath), "Android smoke build did not produce an APK at " + buildPath);

            Debug.Log("Neon Sky Survivors Android smoke APK build passed: " + buildPath);
        }

        private static void EnsurePlayerSettings()
        {
            PlayerSettings.companyName = "NeonSkySurvivors";
            PlayerSettings.productName = "Neon Sky Survivors";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static void EnsureBootScene()
        {
            if (File.Exists(ScenePath))
            {
                return;
            }

            if (!Directory.Exists(SceneDirectory))
            {
                Directory.CreateDirectory(SceneDirectory);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Boot";

            var cameraObject = new GameObject("Neon Mobile Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5.35f;
            camera.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var appObject = new GameObject("NeonSkySurvivorsApp");
            appObject.AddComponent<NeonSkySurvivorsApp>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException(message);
            }
        }
    }
}
