using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    [CreateAssetMenu(menuName = "Anvil Clicker/Game Balance", fileName = "GameBalance")]
    public sealed class GameBalanceConfig : ScriptableObject, IForgeBalance
    {
        [Header("Anvil")]
        [SerializeField] double baseClickPower = 1;
        [Tooltip("Probability in [0, 1] that a strike is critical.")]
        [SerializeField] double critChance = 0.05;
        [SerializeField] double critMultiplier = 5;

        [Header("New game")]
        [SerializeField] WeaponTypeDefinition startingWeapon;

        public double BaseClickPower => baseClickPower;
        public double CritChance => critChance;
        public double CritMultiplier => critMultiplier;
        public string StartingWeaponId => startingWeapon != null ? startingWeapon.Id : null;

        void OnValidate()
        {
            if (baseClickPower < 0) baseClickPower = 0;
            if (critChance < 0) critChance = 0;
            if (critChance > 1) critChance = 1;
            if (critMultiplier < 1) critMultiplier = 1;
        }
    }
}
