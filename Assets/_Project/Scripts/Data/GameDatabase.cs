using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Data
{
    /// <summary>Single entry point to every definition asset. List order is the display order in the shop.</summary>
    [CreateAssetMenu(menuName = "Anvil Clicker/Game Database", fileName = "GameDatabase")]
    public sealed class GameDatabase : ScriptableObject, IGameCatalog
    {
        [SerializeField] GameBalanceConfig balance;
        [SerializeField] List<WeaponTypeDefinition> weapons = new List<WeaponTypeDefinition>();
        [SerializeField] List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
        [SerializeField] List<ApprenticeDefinition> apprentices = new List<ApprenticeDefinition>();
        [SerializeField] List<RoomDefinition> rooms = new List<RoomDefinition>();
        [SerializeField] List<StationDefinition> stations = new List<StationDefinition>();

        Dictionary<string, WeaponTypeDefinition> _weaponsById;
        IUpgradeDefinition[] _upgrades;
        IApprenticeDefinition[] _apprentices;
        IRoomDefinition[] _rooms;

        public GameBalanceConfig Balance => balance;

        public IReadOnlyList<WeaponTypeDefinition> Weapons => weapons;
        public IReadOnlyList<UpgradeDefinition> UpgradeAssets => upgrades;
        public IReadOnlyList<ApprenticeDefinition> ApprenticeAssets => apprentices;
        public IReadOnlyList<RoomDefinition> RoomAssets => rooms;
        public IReadOnlyList<StationDefinition> StationAssets => stations;

        IReadOnlyList<IUpgradeDefinition> IGameCatalog.Upgrades => _upgrades ??= NonNull<UpgradeDefinition, IUpgradeDefinition>(upgrades);
        IReadOnlyList<IApprenticeDefinition> IGameCatalog.Apprentices => _apprentices ??= NonNull<ApprenticeDefinition, IApprenticeDefinition>(apprentices);
        IReadOnlyList<IRoomDefinition> IGameCatalog.Rooms => _rooms ??= NonNull<RoomDefinition, IRoomDefinition>(rooms);

        public bool TryGetWeapon(string id, out IWeaponDefinition weapon)
        {
            weapon = null;
            if (string.IsNullOrEmpty(id)) return false;

            _weaponsById ??= BuildWeaponIndex();
            if (!_weaponsById.TryGetValue(id, out var definition)) return false;

            weapon = definition;
            return true;
        }

        Dictionary<string, WeaponTypeDefinition> BuildWeaponIndex()
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

        static TInterface[] NonNull<TAsset, TInterface>(List<TAsset> assets) where TAsset : Object, TInterface
        {
            var result = new List<TInterface>(assets.Count);
            foreach (var asset in assets)
            {
                if (asset != null) result.Add(asset);
            }
            return result.ToArray();
        }

        void OnValidate()
        {
            _weaponsById = null;
            _upgrades = null;
            _apprentices = null;
            _rooms = null;
        }
    }
}
