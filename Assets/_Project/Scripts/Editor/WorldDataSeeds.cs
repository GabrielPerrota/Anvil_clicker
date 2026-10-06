using System.Collections.Generic;
using AnvilClicker.Data;
using UnityEditor;
using UnityEngine;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Starting stations and rooms of the workshop. Values are written only when an asset is first created,
    /// so layout and balance tweaked in the Inspector survive re-runs; missing references are repaired.
    /// </summary>
    internal static class WorldDataSeeds
    {
        public const string WorkshopId = "room.workshop";
        public const string StorageId = "room.storage";

        /// <summary>Isometric cell where the blacksmith starts, in the middle of the workshop.</summary>
        public static readonly Vector2Int PlayerStartCell = new Vector2Int(3, 3);

        /// <summary>Spots where hired apprentices work, one per apprentice type.</summary>
        public static readonly Vector2Int[] WorkerCells =
        {
            new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(1, 4), new Vector2Int(4, 1)
        };

        readonly struct StationSeed
        {
            public readonly string File, Id, Name, PrefabName, PanelText;
            public readonly StationKind Kind;
            public readonly float Radius;

            public StationSeed(string file, string id, string name, StationKind kind, string prefabName, float radius, string panelText = "")
            {
                File = file; Id = id; Name = name; Kind = kind; PrefabName = prefabName; Radius = radius; PanelText = panelText;
            }
        }

        static readonly StationSeed[] StationList =
        {
            new StationSeed("Anvil", "station.anvil", "Bigorna", StationKind.Anvil, "Anvil", 1.5f),
            new StationSeed("Forge", "station.forge", "Forja", StationKind.Forge, "Forge", 1.6f),
            new StationSeed("UpgradeDesk", "station.upgrade_desk", "Mesa de Melhorias", StationKind.UpgradeDesk, "UpgradeDesk", 1.6f),
            new StationSeed("Counter", "station.counter", "Balcão de Vendas", StationKind.Counter, "Counter", 1.6f,
                "Em breve: aqui você venderá as armas forjadas e escolherá quais guardar para o reino."),
            new StationSeed("Messenger", "station.messenger", "Mesa do Mensageiro", StationKind.Messenger, "Messenger", 1.6f,
                "Em breve: o mensageiro do rei trará as cartas e as encomendas de cada crise do reino."),
            new StationSeed("Storage", "station.storage", "Depósito", StationKind.Storage, "Storage", 1.6f,
                "Em breve: o depósito guardará materiais e armas prontas."),
        };

        public static Dictionary<StationKind, StationDefinition> CreateStations()
        {
            var result = new Dictionary<StationKind, StationDefinition>();

            foreach (var seed in StationList)
            {
                var path = $"{AnvilClickerPaths.Stations}/Station_{seed.File}.asset";
                var station = EditorAssetUtility.LoadOrCreate<StationDefinition>(path, out var created);
                var prefab = EditorAssetUtility.LoadRequired<GameObject>(StationPrefabFactory.StationPath(seed.PrefabName));

                EditorAssetUtility.Edit(station, so =>
                {
                    if (created)
                    {
                        so.Require("id").stringValue = seed.Id;
                        so.Require("displayName").stringValue = seed.Name;
                        so.Require("kind").enumValueIndex = (int)seed.Kind;
                        so.Require("interactionRadius").floatValue = seed.Radius;
                        so.Require("panelText").stringValue = seed.PanelText;
                    }

                    var prefabRef = so.Require("prefab");
                    if (prefabRef.objectReferenceValue == null) prefabRef.objectReferenceValue = prefab;
                });

                result[seed.Kind] = station;
            }

            return result;
        }

        public static List<RoomDefinition> CreateRooms(Dictionary<StationKind, StationDefinition> stations)
        {
            var workshop = CreateRoom("Workshop", WorkshopId, "Oficina", "A ferraria onde tudo começa.", cost: 0, unlockAt: 0,
                area: new RectInt(0, 0, 8, 8), doors: new Vector2Int[0],
                placements: new[]
                {
                    (stations[StationKind.Anvil], new Vector2Int(4, 4)),
                    (stations[StationKind.Forge], new Vector2Int(6, 6)),
                    (stations[StationKind.UpgradeDesk], new Vector2Int(2, 6)),
                    (stations[StationKind.Counter], new Vector2Int(6, 2)),
                    (stations[StationKind.Messenger], new Vector2Int(7, 4)),
                });

            var storage = CreateRoom("Storage", StorageId, "Depósito", "Um galpão ao lado da oficina para guardar materiais e armas.",
                cost: 500, unlockAt: 200,
                area: new RectInt(9, 1, 6, 6), doors: new[] { new Vector2Int(8, 3), new Vector2Int(8, 4) },
                placements: new[] { (stations[StationKind.Storage], new Vector2Int(12, 4)) });

            return new List<RoomDefinition> { workshop, storage };
        }

        static RoomDefinition CreateRoom(string file, string id, string name, string description, double cost, double unlockAt,
            RectInt area, Vector2Int[] doors, (StationDefinition station, Vector2Int cell)[] placements)
        {
            var path = $"{AnvilClickerPaths.Rooms}/Room_{file}.asset";
            var room = EditorAssetUtility.LoadOrCreate<RoomDefinition>(path, out var created);
            if (!created) return room;

            EditorAssetUtility.Edit(room, so =>
            {
                so.Require("id").stringValue = id;
                so.Require("displayName").stringValue = name;
                so.Require("description").stringValue = description;
                so.Require("cost").doubleValue = cost;
                so.Require("unlockAtLifetimeGold").doubleValue = unlockAt;
                so.Require("area").rectIntValue = area;

                var stationsProperty = so.Require("stations");
                stationsProperty.ClearArray();
                for (var i = 0; i < placements.Length; i++)
                {
                    stationsProperty.InsertArrayElementAtIndex(i);
                    var element = stationsProperty.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("station").objectReferenceValue = placements[i].station;
                    element.FindPropertyRelative("cell").vector2IntValue = placements[i].cell;
                }

                var doorsProperty = so.Require("doorCells");
                doorsProperty.ClearArray();
                for (var i = 0; i < doors.Length; i++)
                {
                    doorsProperty.InsertArrayElementAtIndex(i);
                    doorsProperty.GetArrayElementAtIndex(i).vector2IntValue = doors[i];
                }
            });

            return room;
        }
    }
}
