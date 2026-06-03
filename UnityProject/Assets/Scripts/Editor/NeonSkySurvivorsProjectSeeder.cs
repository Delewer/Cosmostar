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

        private const string KeystorePath = "../../KeyStore/neon-sky-release.keystore";
        private const string KeystoreAlias = "neon-sky-release";
        private const string KeystorePass = "neonsky2024release";

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

        [MenuItem("Tools/Neon Sky Survivors/Build Android Release AAB")]
        public static void BuildAndroidRelease()
        {
            EnsureProject();
            ApplyReleaseSettings();

            EditorUserBuildSettings.buildAppBundle = true;

            var buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds", "Android"));
            Directory.CreateDirectory(buildDirectory);

            var buildPath = Path.Combine(buildDirectory, "NeonSkySurvivors-Release.aab");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            Require(report.summary.result == BuildResult.Succeeded, "Android release AAB build failed: " + report.summary.result);
            Require(File.Exists(buildPath), "Android release AAB build did not produce a file at " + buildPath);

            Debug.Log("Neon Sky Survivors Android release AAB build passed: " + buildPath);
        }

        private static void ApplyReleaseSettings()
        {
            PlayerSettings.Android.keystoreName = Path.GetFullPath(Path.Combine(Application.dataPath, KeystorePath));
            PlayerSettings.Android.keystorePass = KeystorePass;
            PlayerSettings.Android.keyaliasName = KeystoreAlias;
            PlayerSettings.Android.keyaliasPass = KeystorePass;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        }

        private static void EnsurePlayerSettings()
        {
            PlayerSettings.companyName = "NeonSkySurvivors";
            PlayerSettings.productName = "Neon Sky Survivors";
            PlayerSettings.applicationIdentifier = "com.neonsky.survivors";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Icon: set the 512x512 adaptive icon if the texture asset exists
            var iconPath512 = "Assets/Icons/icon_512.png";
            var icon512 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath512);
            if (icon512 != null)
            {
                var icons = PlayerSettings.GetIcons(BuildTargetGroup.Android, IconKind.Application);
                if (icons.Length > 0)
                {
                    for (var i = 0; i < icons.Length; i++) icons[i] = icon512;
                    PlayerSettings.SetIcons(BuildTargetGroup.Android, icons, IconKind.Application);
                }
            }

            // Splash screen: set background and logo if assets exist
            var splashPath = "Assets/Icons/splash.png";
            var splash = AssetDatabase.LoadAssetAtPath<Sprite>(splashPath);
            if (splash != null)
            {
                PlayerSettings.SplashScreen.show = true;
                PlayerSettings.SplashScreen.showUnityLogo = false;
                PlayerSettings.SplashScreen.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
            }
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
