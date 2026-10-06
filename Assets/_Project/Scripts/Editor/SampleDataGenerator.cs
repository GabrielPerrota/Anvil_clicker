using System.Collections.Generic;
using AnvilClicker.Core;
using AnvilClicker.Data;
using UnityEditor;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Creates the definition assets with the starting values from docs/GDD.md.
    /// Values are written only when an asset is first created, so balance tuned in the
    /// Inspector survives re-runs; missing references and list entries are always repaired.
    /// </summary>
    internal static class SampleDataGenerator
    {
        readonly struct UpgradeSeed
        {
            public readonly string File, Id, Name, Description;
            public readonly UpgradeCategory Category;
            public readonly double BaseCost, Growth, Unlock;
            public readonly int MaxLevel;
            public readonly ModifierType Effect;
            public readonly double Value;

            public UpgradeSeed(string file, string id, string name, string description, UpgradeCategory category,
                double baseCost, double growth, int maxLevel, double unlock, ModifierType effect, double value)
            {
                File = file; Id = id; Name = name; Description = description; Category = category;
                BaseCost = baseCost; Growth = growth; MaxLevel = maxLevel; Unlock = unlock; Effect = effect; Value = value;
            }
        }

        readonly struct ApprenticeSeed
        {
            public readonly string File, Id, Name;
            public readonly double ForgePointsPerSecond, BaseCost, Growth, Unlock;

            public ApprenticeSeed(string file, string id, string name, double forgePointsPerSecond, double baseCost, double growth, double unlock)
            {
                File = file; Id = id; Name = name; ForgePointsPerSecond = forgePointsPerSecond; BaseCost = baseCost; Growth = growth; Unlock = unlock;
            }
        }

        // Shop order follows this list.
        static readonly UpgradeSeed[] Upgrades =
        {
            new UpgradeSeed("HammerGrip", "upgrade.hammer_grip", "Cabo Reforçado", "+1 PF por golpe a cada nível.",
                UpgradeCategory.Hammer, 15, 1.15, 0, 0, ModifierType.ClickPowerFlat, 1),
            new UpgradeSeed("Bellows", "upgrade.bellows", "Foles", "+10% de PF (golpes e aprendizes) por nível.",
                UpgradeCategory.Forge, 100, 1.18, 0, 50, ModifierType.ForgeSpeedMultiplier, 0.10),
            new UpgradeSeed("MerchantTongue", "upgrade.merchant_tongue", "Lábia de Mercador", "+10% de ouro por arma vendida, por nível.",
                UpgradeCategory.Commerce, 300, 1.25, 0, 150, ModifierType.SellValueMultiplier, 0.10),
            new UpgradeSeed("SteelHammer", "upgrade.steel_hammer", "Martelo de Aço", "Dobra o poder do golpe.",
                UpgradeCategory.Hammer, 400, 1, 1, 150, ModifierType.ClickPowerMultiplier, 1.0),
            new UpgradeSeed("MasterEye", "upgrade.master_eye", "Olho do Mestre", "+1% de chance de golpe crítico por nível.",
                UpgradeCategory.Precision, 250, 1.4, 15, 200, ModifierType.CritChanceFlat, 0.01),
            new UpgradeSeed("ApprenticeTraining", "upgrade.apprentice_training", "Treinamento dos Aprendizes", "Dobra a produção dos aprendizes.",
                UpgradeCategory.Workforce, 1_500, 1, 1, 500, ModifierType.PassiveMultiplier, 1.0),
            new UpgradeSeed("TrueStrike", "upgrade.true_strike", "Golpe Certeiro", "+1× de multiplicador no golpe crítico por nível.",
                UpgradeCategory.Precision, 1_000, 1.6, 10, 600, ModifierType.CritMultiplierFlat, 1),
            new UpgradeSeed("StoneCoal", "upgrade.stone_coal", "Carvão de Pedra", "A forja fica mais quente: +50% de PF.",
                UpgradeCategory.Forge, 2_500, 1, 1, 1_000, ModifierType.ForgeSpeedMultiplier, 0.5),
            new UpgradeSeed("MasterHammer", "upgrade.master_hammer", "Martelo do Mestre", "Triplica o poder do golpe.",
                UpgradeCategory.Hammer, 12_000, 1, 1, 5_000, ModifierType.ClickPowerMultiplier, 2.0),
        };

        static readonly ApprenticeSeed[] Apprentices =
        {
            new ApprenticeSeed("Apprentice", "apprentice.apprentice", "Aprendiz", 1, 15, 1.15, 0),
            new ApprenticeSeed("Journeyman", "apprentice.journeyman", "Ferreiro Jornaleiro", 8, 100, 1.15, 50),
            new ApprenticeSeed("Veteran", "apprentice.veteran", "Ferreiro Veterano", 47, 1_100, 1.15, 500),
            new ApprenticeSeed("Master", "apprentice.master", "Mestre Ferreiro", 260, 12_000, 1.15, 5_000),
        };

        [MenuItem(AnvilClickerPaths.MenuRoot + "Data/Generate Sample Data", priority = 40)]
        public static void Generate()
        {
            var dagger = EditorAssetUtility.LoadOrCreate<WeaponTypeDefinition>(AnvilClickerPaths.IronDagger, out var daggerCreated);
            if (daggerCreated)
            {
                EditorAssetUtility.Edit(dagger, so =>
                {
                    so.Require("id").stringValue = "weapon.iron_dagger";
                    so.Require("displayName").stringValue = "Adaga de Ferro";
                    so.Require("forgePointsRequired").doubleValue = 10;
                    so.Require("baseValue").doubleValue = 5;
                });
            }

            var balance = EditorAssetUtility.LoadOrCreate<GameBalanceConfig>(AnvilClickerPaths.GameBalance, out var balanceCreated);
            EditorAssetUtility.Edit(balance, so =>
            {
                if (balanceCreated)
                {
                    so.Require("baseClickPower").doubleValue = 1;
                    so.Require("critChance").doubleValue = 0.05;
                    so.Require("critMultiplier").doubleValue = 5;
                }

                var startingWeapon = so.Require("startingWeapon");
                if (startingWeapon.objectReferenceValue == null) startingWeapon.objectReferenceValue = dagger;
            });

            var upgrades = new List<UpgradeDefinition>();
            foreach (var seed in Upgrades) upgrades.Add(CreateUpgrade(seed));

            var apprentices = new List<ApprenticeDefinition>();
            foreach (var seed in Apprentices) apprentices.Add(CreateApprentice(seed));

            var stations = WorldDataSeeds.CreateStations();
            var rooms = WorldDataSeeds.CreateRooms(stations);

            var database = EditorAssetUtility.LoadOrCreate<GameDatabase>(AnvilClickerPaths.GameDatabase);
            EditorAssetUtility.Edit(database, so =>
            {
                var balanceRef = so.Require("balance");
                if (balanceRef.objectReferenceValue == null) balanceRef.objectReferenceValue = balance;

                AddIfMissing(so.Require("weapons"), dagger);
                foreach (var upgrade in upgrades) AddIfMissing(so.Require("upgrades"), upgrade);
                foreach (var apprentice in apprentices) AddIfMissing(so.Require("apprentices"), apprentice);
                foreach (var station in stations.Values) AddIfMissing(so.Require("stations"), station);
                foreach (var room in rooms) AddIfMissing(so.Require("rooms"), room);
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[Anvil Clicker] Sample data ready.");
        }

        static UpgradeDefinition CreateUpgrade(UpgradeSeed seed)
        {
            var path = $"{AnvilClickerPaths.Upgrades}/Upgrade_{seed.File}.asset";
            var upgrade = EditorAssetUtility.LoadOrCreate<UpgradeDefinition>(path, out var created);
            if (!created) return upgrade;

            EditorAssetUtility.Edit(upgrade, so =>
            {
                so.Require("id").stringValue = seed.Id;
                so.Require("displayName").stringValue = seed.Name;
                so.Require("description").stringValue = seed.Description;
                so.Require("category").enumValueIndex = (int)seed.Category;
                so.Require("baseCost").doubleValue = seed.BaseCost;
                so.Require("costGrowth").doubleValue = seed.Growth;
                so.Require("maxLevel").intValue = seed.MaxLevel;
                so.Require("unlockAtLifetimeGold").doubleValue = seed.Unlock;

                var effects = so.Require("effects");
                effects.ClearArray();
                effects.InsertArrayElementAtIndex(0);
                var effect = effects.GetArrayElementAtIndex(0);
                effect.FindPropertyRelative("type").enumValueIndex = (int)seed.Effect;
                effect.FindPropertyRelative("valuePerLevel").doubleValue = seed.Value;
            });
            return upgrade;
        }

        static ApprenticeDefinition CreateApprentice(ApprenticeSeed seed)
        {
            var path = $"{AnvilClickerPaths.Apprentices}/Apprentice_{seed.File}.asset";
            var apprentice = EditorAssetUtility.LoadOrCreate<ApprenticeDefinition>(path, out var created);
            if (!created) return apprentice;

            EditorAssetUtility.Edit(apprentice, so =>
            {
                so.Require("id").stringValue = seed.Id;
                so.Require("displayName").stringValue = seed.Name;
                so.Require("baseForgePointsPerSecond").doubleValue = seed.ForgePointsPerSecond;
                so.Require("baseCost").doubleValue = seed.BaseCost;
                so.Require("costGrowth").doubleValue = seed.Growth;
                so.Require("unlockAtLifetimeGold").doubleValue = seed.Unlock;
            });
            return apprentice;
        }

        internal static void AddIfMissing(SerializedProperty list, Object item)
        {
            for (var i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == item) return;
            }

            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
        }
    }
}
