using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnvilClicker.Editor
{
    /// <summary>Folder structure and project-wide settings. Safe to run any number of times.</summary>
    internal static class ProjectSetup
    {
        /// <summary>Back to front. "Default" already exists and stays between Floor and World.</summary>
        static readonly string[] SortingLayersBelowDefault = { "Floor" };
        static readonly string[] SortingLayersAboveDefault = { "World", "FX" };

        const int TransparencySortModeCustomAxis = 3; // UnityEngine.TransparencySortMode.CustomAxis

        [MenuItem(AnvilClickerPaths.MenuRoot + "Setup/Create Folders", priority = 1)]
        public static void CreateFolders()
        {
            foreach (var folder in AnvilClickerPaths.AllFolders)
            {
                EditorAssetUtility.EnsureFolder(folder);

                // Unity ignores dot-files, so this only keeps empty folders alive in git.
                var keep = Path.Combine(folder, ".gitkeep");
                if (!File.Exists(keep)) File.WriteAllText(keep, string.Empty);
            }

            AssetDatabase.Refresh();
            Debug.Log("[Anvil Clicker] Folders ready.");
        }

        [MenuItem(AnvilClickerPaths.MenuRoot + "Setup/Configure Project", priority = 2)]
        public static void ConfigureProject()
        {
            PlayerSettings.companyName = "GabrielPerrota";
            PlayerSettings.productName = "Anvil Clicker";
            PlayerSettings.runInBackground = true; // idle game: keep producing while unfocused
            PlayerSettings.resizableWindow = true;
            EditorSettings.serializationMode = SerializationMode.ForceText;

            // Uncompressed WebGL output can be served by any static file server; compress when publishing.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;

            // Always reload the domain on Play: with it disabled, the Input System keeps stale state
            // monitors between sessions ("Binding index out of range" + NullReferenceException).
            EditorSettings.enterPlayModeOptionsEnabled = false;

            ConfigureSortingLayers();
            ConfigureRenderer2D();
            ConfigureInputActions();

            AssetDatabase.SaveAssets();
            Debug.Log("[Anvil Clicker] Project configured.");
        }

        static void ConfigureSortingLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.Require("m_SortingLayers");

            for (var i = SortingLayersBelowDefault.Length - 1; i >= 0; i--)
                AddSortingLayer(layers, SortingLayersBelowDefault[i], insertAt: 0);

            foreach (var name in SortingLayersAboveDefault)
                AddSortingLayer(layers, name, insertAt: layers.arraySize);

            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddSortingLayer(SerializedProperty layers, string name, int insertAt)
        {
            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name) return;
            }

            layers.InsertArrayElementAtIndex(insertAt);
            var layer = layers.GetArrayElementAtIndex(insertAt);
            layer.FindPropertyRelative("name").stringValue = name;
            layer.FindPropertyRelative("uniqueID").longValue = StableId(name);
            var locked = layer.FindPropertyRelative("locked");
            if (locked != null) locked.boolValue = false;
        }

        /// <summary>Deterministic non-zero id so regenerating the project never reshuffles layer ids.</summary>
        static uint StableId(string name)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var c in name) hash = (hash ^ c) * 16777619u;
                return hash == 0 ? 1u : hash;
            }
        }

        static void ConfigureRenderer2D()
        {
            var renderer = EditorAssetUtility.LoadRequired<ScriptableObject>(AnvilClickerPaths.Renderer2D);

            // Isometric depth: whatever is lower on screen is drawn in front.
            EditorAssetUtility.Edit(renderer, so =>
            {
                so.Require("m_TransparencySortMode").intValue = TransparencySortModeCustomAxis;
                so.Require("m_TransparencySortAxis").vector3Value = new Vector3(0f, 1f, 0f);
            });
        }

        static void ConfigureInputActions()
        {
            var actions = EditorAssetUtility.LoadRequired<InputActionAsset>(AnvilClickerPaths.InputActions);
            if (InputSystem.actions != actions) InputSystem.actions = actions;
        }
    }
}
