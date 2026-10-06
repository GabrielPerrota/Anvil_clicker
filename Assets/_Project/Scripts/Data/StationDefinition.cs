using UnityEngine;

namespace AnvilClicker.Data
{
    public enum StationKind
    {
        Anvil,
        UpgradeDesk,
        Forge,
        Counter,
        Messenger,
        Storage
    }

    /// <summary>A place in the workshop the blacksmith can interact with.</summary>
    [CreateAssetMenu(menuName = "Anvil Clicker/Station", fileName = "Station_")]
    public sealed class StationDefinition : ScriptableObject
    {
        [Tooltip("Stable id saved in player data. Never rename it.")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField] StationKind kind;

        [Header("Interaction")]
        [Tooltip("{0} = station name. Shown when the blacksmith is close enough.")]
        [SerializeField] string promptFormat = "E — {0}";
        [Tooltip("Distance, in world units, from the station's centre at which it can be used.")]
        [SerializeField] float interactionRadius = 1.6f;

        [Header("World")]
        [SerializeField] GameObject prefab;
        [Tooltip("Cells around the station that the blacksmith cannot walk through (offsets from its own cell).")]
        [SerializeField] Vector2Int[] blockedOffsets = { Vector2Int.zero };

        [Header("Placeholder panel (stations without a real screen yet)")]
        [SerializeField, TextArea(2, 4)] string panelText;

        public string Id => id;
        public string DisplayName => displayName;
        public StationKind Kind => kind;
        public string PromptFormat => promptFormat;
        public float InteractionRadius => interactionRadius;
        public GameObject Prefab => prefab;
        public Vector2Int[] BlockedOffsets => blockedOffsets;
        public string PanelText => panelText;

        public string Prompt => string.Format(promptFormat, displayName);

        void OnValidate()
        {
            if (interactionRadius < 0.5f) interactionRadius = 0.5f;
            if (blockedOffsets == null) blockedOffsets = System.Array.Empty<Vector2Int>();
        }
    }
}
