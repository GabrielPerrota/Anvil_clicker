using System;
using UnityEditor;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// One-click (or one-command) project bootstrap. Every step is idempotent; the scene is only
    /// created when missing (rebuild it with Scenes/Build Workshop Scene).
    /// Batch: unity run . -- -executeMethod AnvilClicker.Editor.AnvilClickerSetup.RunAllBatch
    /// </summary>
    public static class AnvilClickerSetup
    {
        [MenuItem(AnvilClickerPaths.MenuRoot + "Setup/Run All", true)]
        static bool CanRunAll() => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem(AnvilClickerPaths.MenuRoot + "Setup/Run All", priority = 0)]
        public static void RunAll()
        {
            ProjectSetup.CreateFolders();
            ProjectSetup.ConfigureProject();
            PlaceholderArtGenerator.Generate();
            StationPrefabFactory.Generate();
            SampleDataGenerator.Generate();
            GameTools.ValidateDatabase();
            WorkshopSceneBuilder.BuildIfMissing();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Anvil Clicker] Setup complete.");
        }

        /// <summary>Entry point for -executeMethod. Exits with code 1 on any failure.</summary>
        public static void RunAllBatch() => RunBatch(RunAll);

        /// <summary>Entry point for -executeMethod: forces a rebuild of the Workshop scene.</summary>
        public static void RebuildWorkshopSceneBatch() => RunBatch(() =>
        {
            RunAll();
            WorkshopSceneBuilder.Build();
        });

        static void RunBatch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
