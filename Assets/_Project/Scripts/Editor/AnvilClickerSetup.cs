using System;
using UnityEditor;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// One-click (or one-command) project bootstrap. Every step is idempotent.
    /// Batch: unity run . -- -executeMethod AnvilClicker.Editor.AnvilClickerSetup.RunAllBatch
    /// </summary>
    public static class AnvilClickerSetup
    {
        [MenuItem(AnvilClickerPaths.MenuRoot + "Setup/Run All", priority = 0)]
        public static void RunAll()
        {
            ProjectSetup.CreateFolders();
            ProjectSetup.ConfigureProject();
            PlaceholderArtGenerator.Generate();
            SampleDataGenerator.Generate();
            WorkshopSceneBuilder.Build();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Anvil Clicker] Setup complete.");
        }

        /// <summary>Entry point for -executeMethod. Exits with code 1 on any failure.</summary>
        public static void RunAllBatch()
        {
            try
            {
                RunAll();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
