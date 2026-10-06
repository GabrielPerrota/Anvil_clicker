using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Runtime
{
    /// <summary>Tells whether a screen position is on top of UI that should swallow world clicks.</summary>
    public static class UiPointerBlocker
    {
        /// <summary>USS class for UI areas (panels, modals) that must swallow clicks.</summary>
        public const string BlocksWorldInputClass = "blocks-world-input";

        public static bool IsOverBlockingUi(UIDocument document, Vector2 screenPosition)
        {
            var panel = document != null ? document.rootVisualElement?.panel : null;
            if (panel == null) return false;

            // Input System screen space starts at the bottom; UI Toolkit panels start at the top.
            var panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            for (var element = panel.Pick(panelPosition); element != null; element = element.parent)
            {
                if (element.ClassListContains(BlocksWorldInputClass) && element.resolvedStyle.display != DisplayStyle.None) return true;
            }

            return false;
        }
    }
}
