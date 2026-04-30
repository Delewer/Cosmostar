using System.IO;
using Cosmostar.Core.Design;
using Cosmostar.Runtime.App;
using Cosmostar.Runtime.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmostar.Editor
{
    public static class VerticalSliceProjectSeeder
    {
        private const string CatalogDirectory = "Assets/Resources/Cosmostar";
        private const string CatalogPath = CatalogDirectory + "/VerticalSliceCatalog.asset";
        private const string SceneDirectory = "Assets/Scenes";
        private const string ScenePath = SceneDirectory + "/Boot.unity";

        [InitializeOnLoadMethod]
        private static void EnsureSeededOnLoad()
        {
            EditorApplication.delayCall += EnsureSeeded;
        }

        [MenuItem("Tools/Cosmostar/Seed Vertical Slice")]
        public static void EnsureSeeded()
        {
            EnsureFolders();
            EnsureCatalogAsset();
            EnsureSceneAsset();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(CatalogDirectory))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Cosmostar");
            }

            if (!AssetDatabase.IsValidFolder(SceneDirectory))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void EnsureCatalogAsset()
        {
            if (File.Exists(CatalogPath))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<VerticalSliceCatalogAsset>();
            asset.Catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            AssetDatabase.CreateAsset(asset, CatalogPath);
            EditorUtility.SetDirty(asset);
        }

        private static void EnsureSceneAsset()
        {
            if (File.Exists(ScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.backgroundColor = Color.black;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.orthographic = true;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 100f;

            var appObject = new GameObject("CosmostarApp");
            appObject.AddComponent<CosmostarApp>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(ScenePath));
        }
    }
}
