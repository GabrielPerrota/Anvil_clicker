using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Weapon Type", fileName = "Weapon_")]
    public sealed class WeaponTypeDefinition : ScriptableObject, IWeaponDefinition
    {
        [Tooltip("Stable id saved in player data. Never rename it.")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField] double forgePointsRequired = 10;
        [SerializeField] double baseValue = 5;
        [SerializeField] Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public double ForgePointsRequired => forgePointsRequired;
        public double BaseValue => baseValue;
        public Sprite Icon => icon;

        void OnValidate()
        {
            if (forgePointsRequired <= 0) forgePointsRequired = 1;
            if (baseValue < 0) baseValue = 0;
        }
    }
}
