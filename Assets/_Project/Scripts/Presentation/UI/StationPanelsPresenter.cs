using AnvilClicker.Data;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Opens the right panel for the station the blacksmith is using: the shop at the upgrade desk, a
    /// "coming soon" card at stations that have no real screen yet. Also shows the "E — ..." prompt.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class StationPanelsPresenter : MonoBehaviour
    {
        const string HiddenClass = "hidden";

        [SerializeField] InteractionController interaction;
        [SerializeField] ForgeMode forgeMode;

        [Header("Texts")]
        [SerializeField] string forgeModeHint = "Clique na bigorna ou aperte Espaço · E ou ESC para sair";
        [SerializeField] string defaultHint = "WASD andar · E usar · Scroll zoom";

        VisualElement _shopPanel;
        VisualElement _infoPanel;
        VisualElement _forgePanel;
        Label _prompt;
        Label _hint;
        Label _infoTitle;
        Label _infoText;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _shopPanel = root.Q<VisualElement>("shop-panel");
            _infoPanel = root.Q<VisualElement>("info-panel");
            _forgePanel = root.Q<VisualElement>("forge-panel");
            _prompt = root.Q<Label>("station-prompt");
            _hint = root.Q<Label>("hint-label");
            _infoTitle = root.Q<Label>("info-title");
            _infoText = root.Q<Label>("info-text");

            BindClose(root, "shop-close");
            BindClose(root, "info-close");

            interaction.NearbyChanged += OnNearbyChanged;
            interaction.StationOpened += OnOpened;
            interaction.StationClosed += OnClosed;
            forgeMode.ActiveChanged += OnForgeModeChanged;

            Hide(_shopPanel);
            Hide(_infoPanel);
            OnNearbyChanged(interaction.Nearby);
            OnForgeModeChanged(forgeMode.IsActive);
        }

        void OnDestroy()
        {
            if (interaction != null)
            {
                interaction.NearbyChanged -= OnNearbyChanged;
                interaction.StationOpened -= OnOpened;
                interaction.StationClosed -= OnClosed;
            }

            if (forgeMode != null) forgeMode.ActiveChanged -= OnForgeModeChanged;
        }

        void BindClose(VisualElement root, string name)
        {
            var button = root.Q<Button>(name);
            if (button == null) return;
            button.focusable = false;
            button.clicked += interaction.CloseOpen;
        }

        void OnNearbyChanged(Station station) => RefreshPrompt();

        void OnForgeModeChanged(bool active)
        {
            _forgePanel.EnableInClassList(HiddenClass, !active);
            _hint.text = active ? forgeModeHint : defaultHint;
            RefreshPrompt();
        }

        void RefreshPrompt()
        {
            var station = interaction.Nearby;
            var show = station != null && !forgeMode.IsActive && interaction.Open == null;
            _prompt.EnableInClassList(HiddenClass, !show);
            if (show) _prompt.text = station.Definition.Prompt;
        }

        void OnOpened(Station station)
        {
            if (station.Kind == StationKind.UpgradeDesk)
            {
                Show(_shopPanel);
            }
            else
            {
                _infoTitle.text = station.Definition.DisplayName;
                _infoText.text = station.Definition.PanelText;
                Show(_infoPanel);
            }

            RefreshPrompt();
        }

        void OnClosed(Station station)
        {
            Hide(_shopPanel);
            Hide(_infoPanel);
            RefreshPrompt();
        }

        static void Show(VisualElement element) => element.RemoveFromClassList(HiddenClass);
        static void Hide(VisualElement element) => element.AddToClassList(HiddenClass);
    }
}
