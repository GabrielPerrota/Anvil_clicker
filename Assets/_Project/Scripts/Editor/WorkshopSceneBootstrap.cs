using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// The game has a single scene for now, so the editor always plays it: pressing Play from any open
    /// scene starts Workshop, and an empty "Untitled" scene is swapped for Workshop when the editor loads.
    /// </summary>
    [InitializeOnLoad]
    internal static class WorkshopSceneBootstrap
    {
        const string OpenedThisSessionKey = "AnvilClicker.WorkshopOpenedThisSession";

        static WorkshopSceneBootstrap()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += Apply;
        }

        static void Apply()
        {
            var workshop = AssetDatabase.LoadAssetAtPath<SceneAsset>(AnvilClickerPaths.WorkshopScene);
            if (workshop == null) return;

            EditorSceneManager.playModeStartScene = workshop;

            if (SessionState.GetBool(OpenedThisSessionKey, false)) return;
            SessionState.SetBool(OpenedThisSessionKey, true);

            var active = EditorSceneManager.GetActiveScene();
            var isEmptyUntitled = string.IsNullOrEmpty(active.path) && !active.isDirty && EditorSceneManager.sceneCount == 1;
            if (isEmptyUntitled && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorSceneManager.OpenScene(AnvilClickerPaths.WorkshopScene);
        }
    }
}
