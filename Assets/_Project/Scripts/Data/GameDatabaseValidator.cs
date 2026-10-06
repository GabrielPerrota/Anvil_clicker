using System.Collections.Generic;
using System.Linq;
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


            ValidateRooms(database, ids, errors);

            return errors;
        }

        static void ValidateRooms(GameDatabase database, HashSet<string> ids, List<string> errors)
        {
            var stationIds = new HashSet<string>();
            foreach (var station in database.StationAssets)
            {
                if (station == null)
                {
                    errors.Add("Station list contains an empty slot.");
                    continue;
                }

                var label = $"Station '{station.name}'";
                if (string.IsNullOrWhiteSpace(station.Id)) errors.Add($"{label}: id is empty.");
                else if (!stationIds.Add(station.Id)) errors.Add($"{label}: id '{station.Id}' is used more than once.");
                if (station.Prefab == null) errors.Add($"{label}: prefab is not set.");
                if (station.InteractionRadius <= 0) errors.Add($"{label}: interaction radius must be > 0.");
            }

            var hasFreeRoom = false;
            foreach (var room in database.RoomAssets)
            {
                if (!CheckEntry(room, "Room", ids, errors, out var label)) continue;
                if (!NonNegative(room.Cost)) errors.Add($"{label}: cost must be >= 0.");
                if (!NonNegative(room.UnlockAtLifetimeGold)) errors.Add($"{label}: unlock threshold must be >= 0.");
                if (room.Area.width < 1 || room.Area.height < 1) errors.Add($"{label}: area is empty.");
                if (room.Cost <= 0) hasFreeRoom = true;

                foreach (var door in room.DoorCells)
                {
                    if (room.Area.Contains(door)) errors.Add($"{label}: door cell {door} is inside the room; doors sit just outside the area.");
                }

                var used = new HashSet<Vector2Int>();
                foreach (var placement in room.Stations)
                {
                    if (placement.station == null)
                    {
                        errors.Add($"{label}: has a placement without a station.");
                        continue;
                    }

                    if (!database.StationAssets.Contains(placement.station)) errors.Add($"{label}: station '{placement.station.name}' is not listed in the database.");
                    if (!room.Area.Contains(placement.cell)) errors.Add($"{label}: station '{placement.station.name}' is outside the room area.");
                    if (!used.Add(placement.cell)) errors.Add($"{label}: two stations share the cell {placement.cell}.");
                }
            }

            if (database.RoomAssets.Count > 0 && !hasFreeRoom) errors.Add("No room is free: the player would start without a workshop.");
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
                : entry is IRoomDefinition r ? r.Id
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
