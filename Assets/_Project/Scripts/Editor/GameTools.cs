using System;
using AnvilClicker.Core;
using AnvilClicker.Data;
using AnvilClicker.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnvilClicker.Editor
{
    /// <summary>Data validation, save file management and Play-mode cheats.</summary>
    internal static class GameTools
    {
        const string DataMenu = AnvilClickerPaths.MenuRoot + "Data/";
        const string SaveMenu = AnvilClickerPaths.MenuRoot + "Save/";
        const string DebugMenu = AnvilClickerPaths.MenuRoot + "Debug/";

        // --- Data ----------------------------------------------------------------------------------

        [MenuItem(DataMenu + "Validate Database", priority = 41)]
        public static void ValidateDatabase()
        {
            var database = AssetDatabase.LoadAssetAtPath<GameDatabase>(AnvilClickerPaths.GameDatabase);
            var errors = GameDatabaseValidator.Validate(database);

            if (errors.Count == 0)
            {
                Debug.Log("[Anvil Clicker] Database is valid.");
                return;
            }

            foreach (var error in errors) Debug.LogError($"[Anvil Clicker] {error}", database);
            if (Application.isBatchMode) throw new InvalidOperationException($"Database has {errors.Count} error(s).");
        }

        // --- Save ----------------------------------------------------------------------------------

        static SaveSystem CreateSaveSystem() => new SaveSystem(SaveSystem.DefaultDirectory, new SaveSerializer(), new SystemClock());

        [MenuItem(SaveMenu + "Open Save Folder", priority = 80)]
        public static void OpenSaveFolder() => EditorUtility.RevealInFinder(SaveSystem.DefaultDirectory);

        [MenuItem(SaveMenu + "Delete Save", true)]
        static bool CanDeleteSave() => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem(SaveMenu + "Delete Save", priority = 81)]
        public static void DeleteSave()
        {
            var saves = CreateSaveSystem();
            if (!saves.HasSave)
            {
                Debug.Log("[Anvil Clicker] There is no save to delete.");
                return;
            }

            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Apagar save", $"Apagar o progresso salvo em\n{saves.SavePath}?", "Apagar", "Cancelar"))
                return;

            saves.Delete();
            Debug.Log("[Anvil Clicker] Save deleted.");
        }

        // --- Debug (Play mode only) ----------------------------------------------------------------

        [MenuItem(DebugMenu + "Add 1K Gold", true)]
        [MenuItem(DebugMenu + "Add 1M Gold", true)]
        [MenuItem(DebugMenu + "Simulate 1h Offline", true)]
        [MenuItem(DebugMenu + "Save Now", true)]
        static bool IsPlaying() => EditorApplication.isPlaying;

        [MenuItem(DebugMenu + "Add 1K Gold", priority = 100)]
        public static void AddThousandGold() => WithContext(context => context.Wallet.Add(1_000));

        [MenuItem(DebugMenu + "Add 1M Gold", priority = 101)]
        public static void AddMillionGold() => WithContext(context => context.Wallet.Add(1_000_000));

        [MenuItem(DebugMenu + "Simulate 1h Offline", priority = 102)]
        public static void SimulateOfflineHour() => WithContext(context => context.ApplyOfflineProgress(TimeSpan.FromHours(1)));

        [MenuItem(DebugMenu + "Save Now", priority = 103)]
        public static void SaveNow()
        {
            var loop = Object.FindFirstObjectByType<GameLoop>();
            if (loop == null) Debug.LogWarning("[Anvil Clicker] No GameLoop in the scene.");
            else loop.SaveNow();
        }

        static void WithContext(Action<GameContext> action)
        {
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null || bootstrap.Context == null)
            {
                Debug.LogWarning("[Anvil Clicker] No running game found (enter Play mode in the Workshop scene).");
                return;
            }

            action(bootstrap.Context);
        }
    }
}
