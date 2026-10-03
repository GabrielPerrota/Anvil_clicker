using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    /// <summary>Single entry point to every definition asset. Looked up by stable id.</summary>
    [CreateAssetMenu(menuName = "Anvil Clicker/Game Database", fileName = "GameDatabase")]
    public sealed class GameDatabase : ScriptableObject, IWeaponCatalog
    {
        [SerializeField] GameBalanceConfig balance;
        [SerializeField] List<WeaponTypeDefinition> weapons = new List<WeaponTypeDefinition>();

        Dictionary<string, WeaponTypeDefinition> _weaponsById;

        public GameBalanceConfig Balance => balance;

        public IReadOnlyList<WeaponTypeDefinition> Weapons => weapons;

        public bool TryGetWeapon(string id, out IWeaponDefinition weapon)
        {
            weapon = null;
            if (string.IsNullOrEmpty(id)) return false;

            _weaponsById ??= BuildIndex();
            if (!_weaponsById.TryGetValue(id, out var definition)) return false;

            weapon = definition;
            return true;
        }

        Dictionary<string, WeaponTypeDefinition> BuildIndex()
        {
            var index = new Dictionary<string, WeaponTypeDefinition>();
            foreach (var weapon in weapons)
            {
                if (weapon == null || string.IsNullOrEmpty(weapon.Id)) continue;
                if (!index.TryAdd(weapon.Id, weapon))
                    Debug.LogError($"Duplicate weapon id '{weapon.Id}' in {name}.", this);
            }
            return index;
        }

        void OnValidate() => _weaponsById = null;
    }
}
