using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Game Balance", fileName = "GameBalance")]
    public sealed class GameBalanceConfig : ScriptableObject, IGameBalance
    {
        [Header("Anvil")]
        [SerializeField] double baseClickPower = 1;
        [Tooltip("Probability in [0, 1] that a strike is critical.")]
        [SerializeField] double critChance = 0.05;
        [SerializeField] double critMultiplier = 5;

        [Header("New game")]
        [SerializeField] WeaponTypeDefinition startingWeapon;

        [Header("Apprentices")]
        [Tooltip("Owned counts (ascending) at which one apprentice type's production doubles.")]
        [SerializeField] List<int> apprenticeMilestones = new List<int> { 25, 50, 100, 200 };

        [Header("Offline progress")]
        [SerializeField] double offlineCapHours = 8;
        [Tooltip("Fraction in [0, 1] of production earned while away.")]
        [SerializeField] double offlineEfficiency = 0.5;

        [Header("Saving")]
        [SerializeField] double autosaveIntervalSeconds = 30;

        public double BaseClickPower => baseClickPower;
        public double CritChance => critChance;
        public double CritMultiplier => critMultiplier;
        public string StartingWeaponId => startingWeapon != null ? startingWeapon.Id : null;
        public WeaponTypeDefinition StartingWeapon => startingWeapon;
        public IReadOnlyList<int> ApprenticeMilestones => apprenticeMilestones;
        public double OfflineCapHours => offlineCapHours;
        public double OfflineEfficiency => offlineEfficiency;
        public double AutosaveIntervalSeconds => autosaveIntervalSeconds;

        void OnValidate()
        {
            if (baseClickPower < 0) baseClickPower = 0;
            if (critChance < 0) critChance = 0;
            if (critChance > 1) critChance = 1;
            if (critMultiplier < 1) critMultiplier = 1;
            if (offlineCapHours < 0) offlineCapHours = 0;
            if (offlineEfficiency < 0) offlineEfficiency = 0;
            if (offlineEfficiency > 1) offlineEfficiency = 1;
            if (autosaveIntervalSeconds < 5) autosaveIntervalSeconds = 5;
        }
    }
}
