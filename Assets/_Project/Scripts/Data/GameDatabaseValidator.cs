using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    /// <summary>
    /// Catches data mistakes before they reach the game: missing references, duplicate ids, impossible numbers.
    /// Used by the editor menu and by an EditMode test that runs against the real project database.
    /// </summary>
    public static class GameDatabaseValidator
    {
        public static IReadOnlyList<string> Validate(GameDatabase database)
        {
            var errors = new List<string>();
            if (database == null)
            {
                errors.Add("Database is missing.");
                return errors;
            }

            var ids = new HashSet<string>();
            ValidateBalance(database.Balance, database, errors);

            foreach (var weapon in database.Weapons)
            {
                if (!CheckEntry(weapon, "Weapon", ids, errors, out var label)) continue;
                if (!Positive(weapon.ForgePointsRequired)) errors.Add($"{label}: forge points required must be > 0.");
                if (!NonNegative(weapon.BaseValue)) errors.Add($"{label}: base value must be >= 0.");
            }

            foreach (var upgrade in database.UpgradeAssets)
            {
                if (!CheckEntry(upgrade, "Upgrade", ids, errors, out var label)) continue;
                CheckCost(label, upgrade.BaseCost, upgrade.CostGrowth, upgrade.UnlockAtLifetimeGold, errors);
                if (upgrade.MaxLevel < 0) errors.Add($"{label}: max level must be >= 0.");
                if (upgrade.Effects.Count == 0) errors.Add($"{label}: has no effects.");
                foreach (var effect in upgrade.Effects)
                {
                    if (!Finite(effect.ValuePerLevel)) errors.Add($"{label}: effect {effect.Type} has a non-finite value.");
                }
            }

            foreach (var apprentice in database.ApprenticeAssets)
            {
                if (!CheckEntry(apprentice, "Apprentice", ids, errors, out var label)) continue;
                CheckCost(label, apprentice.BaseCost, apprentice.CostGrowth, apprentice.UnlockAtLifetimeGold, errors);
                if (!Positive(apprentice.BaseForgePointsPerSecond)) errors.Add($"{label}: forge points per second must be > 0.");
            }

            return errors;
        }

        static void ValidateBalance(GameBalanceConfig balance, GameDatabase database, List<string> errors)
        {
            if (balance == null)
            {
                errors.Add("Database has no GameBalanceConfig.");
                return;
            }

            if (balance.StartingWeapon == null) errors.Add("Balance: starting weapon is not set.");
            else if (!database.TryGetWeapon(balance.StartingWeaponId, out _)) errors.Add("Balance: starting weapon is not listed in the database.");

            if (!NonNegative(balance.BaseClickPower)) errors.Add("Balance: base click power must be >= 0.");
            if (balance.CritChance < 0 || balance.CritChance > 1) errors.Add("Balance: crit chance must be in [0, 1].");
            if (!(balance.CritMultiplier >= 1)) errors.Add("Balance: crit multiplier must be >= 1.");
            if (!Positive(balance.OfflineCapHours)) errors.Add("Balance: offline cap must be > 0 hours.");
            if (balance.OfflineEfficiency < 0 || balance.OfflineEfficiency > 1) errors.Add("Balance: offline efficiency must be in [0, 1].");
            if (!Positive(balance.AutosaveIntervalSeconds)) errors.Add("Balance: autosave interval must be > 0.");

            var previous = 0;
            foreach (var milestone in balance.ApprenticeMilestones)
            {
                if (milestone <= previous) errors.Add("Balance: apprentice milestones must be positive and ascending.");
                previous = milestone;
            }
        }

        static bool CheckEntry<T>(T entry, string kind, HashSet<string> ids, List<string> errors, out string label) where T : Object
        {
            label = kind;
            if (entry == null)
            {
                errors.Add($"{kind} list contains an empty slot.");
                return false;
            }

            var id = entry is IWeaponDefinition w ? w.Id
                : entry is IUpgradeDefinition u ? u.Id
                : entry is IApprenticeDefinition a ? a.Id
                : null;

            label = $"{kind} '{entry.name}'";
            if (string.IsNullOrWhiteSpace(id)) errors.Add($"{label}: id is empty.");
            else if (!ids.Add(id)) errors.Add($"{label}: id '{id}' is used more than once.");
            return true;
        }

        static void CheckCost(string label, double baseCost, double growth, double unlock, List<string> errors)
        {
            if (!Positive(baseCost)) errors.Add($"{label}: base cost must be > 0.");
            if (!(growth >= 1) || !Finite(growth)) errors.Add($"{label}: cost growth must be >= 1.");
            if (!NonNegative(unlock)) errors.Add($"{label}: unlock threshold must be >= 0.");
        }

        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static bool Positive(double value) => Finite(value) && value > 0;
        static bool NonNegative(double value) => Finite(value) && value >= 0;
    }
}
