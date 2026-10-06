using System;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Room", fileName = "Room_")]
    public sealed class RoomDefinition : ScriptableObject, IRoomDefinition
    {
        [Serializable]
        public struct StationPlacement
        {
            public StationDefinition station;
            [Tooltip("Isometric grid cell the station stands on.")]
            public Vector2Int cell;
        }

        [Tooltip("Stable id saved in player data. Never rename it.")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField, TextArea(2, 4)] string description;

        [Header("Cost")]
        [Tooltip("0 = part of the workshop from the start.")]
        [SerializeField] double cost;
        [SerializeField] double unlockAtLifetimeGold;

        [Header("Layout (isometric grid cells)")]
        [SerializeField] RectInt area = new RectInt(0, 0, 6, 6);
        [SerializeField] StationPlacement[] stations = Array.Empty<StationPlacement>();

        [Tooltip("Cells just outside the area that become an opening to the rest of the workshop once the room is built. Boarded up until then.")]
        [SerializeField] Vector2Int[] doorCells = Array.Empty<Vector2Int>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public double Cost => cost;
        public double UnlockAtLifetimeGold => unlockAtLifetimeGold;
        public RectInt Area => area;
        public StationPlacement[] Stations => stations;
        public Vector2Int[] DoorCells => doorCells;

        void OnValidate()
        {
            if (cost < 0) cost = 0;
            if (unlockAtLifetimeGold < 0) unlockAtLifetimeGold = 0;
            if (area.width < 1) area.width = 1;
            if (area.height < 1) area.height = 1;
        }
    }
}
