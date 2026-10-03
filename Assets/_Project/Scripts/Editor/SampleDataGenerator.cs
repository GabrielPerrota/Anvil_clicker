using AnvilClicker.Data;
using UnityEditor;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Creates the definition assets with the starting values from docs/GDD.md.
    /// Values are written only when an asset is first created, so balance tuned in the
    /// Inspector survives re-runs; missing references are always repaired.
    /// </summary>
    internal static class SampleDataGenerator
    {
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

            var database = EditorAssetUtility.LoadOrCreate<GameDatabase>(AnvilClickerPaths.GameDatabase);
            EditorAssetUtility.Edit(database, so =>
            {
                var balanceRef = so.Require("balance");
                if (balanceRef.objectReferenceValue == null) balanceRef.objectReferenceValue = balance;

                AddIfMissing(so.Require("weapons"), dagger);
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[Anvil Clicker] Sample data ready.");
        }

        static void AddIfMissing(SerializedProperty list, Object item)
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
