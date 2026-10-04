using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Apprentice", fileName = "Apprentice_")]
    public sealed class ApprenticeDefinition : ScriptableObject, IApprenticeDefinition
    {
        [Tooltip("Stable id saved in player data. Never rename it.")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField] double baseForgePointsPerSecond = 1;

        [Header("Cost")]
        [SerializeField] double baseCost = 15;
        [Tooltip("Cost multiplier per unit owned.")]
        [SerializeField] double costGrowth = 1.15;
        [SerializeField] double unlockAtLifetimeGold;

        [SerializeField] Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public double BaseForgePointsPerSecond => baseForgePointsPerSecond;
        public double BaseCost => baseCost;
        public double CostGrowth => costGrowth;
        public double UnlockAtLifetimeGold => unlockAtLifetimeGold;
        public Sprite Icon => icon;

        void OnValidate()
        {
            if (baseForgePointsPerSecond < 0) baseForgePointsPerSecond = 0;
            if (baseCost <= 0) baseCost = 1;
            if (costGrowth < 1) costGrowth = 1;
            if (unlockAtLifetimeGold < 0) unlockAtLifetimeGold = 0;
        }
    }
}
