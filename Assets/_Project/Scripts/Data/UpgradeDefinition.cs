using System;
using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Upgrade", fileName = "Upgrade_")]
    public sealed class UpgradeDefinition : ScriptableObject, IUpgradeDefinition
    {
        [Serializable]
        public struct EffectEntry
        {
            public ModifierType type;
            [Tooltip("Flat: amount per level. Multiplier: fraction per level (0.1 = +10%).")]
            public double valuePerLevel;
        }

        [Tooltip("Stable id saved in player data. Never rename it.")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField, TextArea(2, 4)] string description;
        [SerializeField] UpgradeCategory category;

        [Header("Cost")]
        [SerializeField] double baseCost = 10;
        [Tooltip("Cost multiplier per level owned.")]
        [SerializeField] double costGrowth = 1.15;
        [Tooltip("0 = unlimited.")]
        [SerializeField] int maxLevel;
        [SerializeField] double unlockAtLifetimeGold;

        [Header("Effects")]
        [SerializeField] List<EffectEntry> effects = new List<EffectEntry>();

        UpgradeEffect[] _effects;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public UpgradeCategory Category => category;
        public double BaseCost => baseCost;
        public double CostGrowth => costGrowth;
        public int MaxLevel => maxLevel;
        public double UnlockAtLifetimeGold => unlockAtLifetimeGold;

        public IReadOnlyList<UpgradeEffect> Effects => _effects ??= BuildEffects();

        UpgradeEffect[] BuildEffects()
        {
            var result = new UpgradeEffect[effects.Count];
            for (var i = 0; i < effects.Count; i++) result[i] = new UpgradeEffect(effects[i].type, effects[i].valuePerLevel);
            return result;
        }

        void OnValidate()
        {
            if (baseCost <= 0) baseCost = 1;
            if (costGrowth < 1) costGrowth = 1;
            if (maxLevel < 0) maxLevel = 0;
            if (unlockAtLifetimeGold < 0) unlockAtLifetimeGold = 0;
            _effects = null;
        }
    }
}
