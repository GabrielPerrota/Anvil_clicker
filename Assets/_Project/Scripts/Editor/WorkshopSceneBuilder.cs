using AnvilClicker.Data;
using AnvilClicker.Presentation;
using AnvilClicker.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Builds Scenes/Workshop.unity from scratch. The scene is fully generated: change this builder,
    /// not the scene file. The rooms themselves (floor, walls, stations) are built at runtime from the
    /// RoomDefinition assets, so the scene only holds the empty map, the player and the systems.
    /// Requires placeholder art, prefabs and sample data (see <see cref="AnvilClickerSetup"/>).
    /// </summary>
    internal static class WorkshopSceneBuilder
    {
        static readonly Color BackgroundColor = new Color(0.07f, 0.055f, 0.06f);
        static readonly Color AmbientColor = new Color(0.72f, 0.68f, 0.82f);

        /// <summary>
        /// Unity assigns fresh object ids on every rebuild, so automated runs only create the scene when
        /// it is missing; this keeps git diffs clean. Use the menu item to force a rebuild after changing this builder.
        /// </summary>
        public static void BuildIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AnvilClickerPaths.WorkshopScene) != null)
            {
                Debug.Log($"[Anvil Clicker] Scene already exists at {AnvilClickerPaths.WorkshopScene}; skipped (use the Build menu to force).");
                return;
            }

            Build();
        }

        [MenuItem(AnvilClickerPaths.MenuRoot + "Scenes/Build Workshop Scene", true)]
        static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem(AnvilClickerPaths.MenuRoot + "Scenes/Build Workshop Scene", priority = 60)]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Stop Play mode before building the Workshop scene.");

            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.Scenes);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Load assets only after NewScene: opening a scene in Single mode unloads unused assets, which
            // turns references loaded earlier into destroyed objects that get saved as missing (null).
            var assets = SceneAssets.Load();

            CreateGlobalLight();
            var world = CreateWorld(assets, out var stationsRoot);
            var player = CreatePlayer(assets, world);
            var cameraRig = CreateCameraRig(player.transform, world, out var camera);
            var ui = CreateUi(assets, camera);
            var systems = CreateSystems(assets, camera, player, world, ui, cameraRig);
            CreateRoomBuilder(assets, world, stationsRoot);
            CreateWorkers(assets);

            Validate(systems, world);
            EditorSceneManager.SaveScene(scene, AnvilClickerPaths.WorkshopScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(AnvilClickerPaths.WorkshopScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Anvil Clicker] Scene built at {AnvilClickerPaths.WorkshopScene}.");
        }

        sealed class SceneAssets
        {
            public GameDatabase Database;
            public Tile FloorTile, WallTile, InvisibleWallTile;
            public GameObject Barrier;
            public ParticleSystem Dust;
            public Sprite[] PlayerSprites;
            public Sprite WorkerSprite;
            public PanelSettings PanelSettings;
            public VisualTreeAsset Hud, ShopRow;

            public static SceneAssets Load() => new SceneAssets
            {
                Database = EditorAssetUtility.LoadRequired<GameDatabase>(AnvilClickerPaths.GameDatabase),
                FloorTile = EditorAssetUtility.LoadRequired<Tile>(AnvilClickerPaths.FloorTile),
                WallTile = EditorAssetUtility.LoadRequired<Tile>(AnvilClickerPaths.WallTile),
                InvisibleWallTile = EditorAssetUtility.LoadRequired<Tile>(AnvilClickerPaths.InvisibleWallTile),
                Barrier = EditorAssetUtility.LoadRequired<GameObject>(AnvilClickerPaths.FxPrefabs + "/Barrier.prefab"),
                Dust = EditorAssetUtility.LoadRequired<GameObject>(AnvilClickerPaths.FxPrefabs + "/BuildDust.prefab").GetComponent<ParticleSystem>(),
                // Order must match FacingDirection: DownRight, DownLeft, UpLeft, UpRight.
                PlayerSprites = new[]
                {
                    EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.PlayerDownRight),
                    EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.PlayerDownLeft),
                    EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.PlayerUpLeft),
                    EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.PlayerUpRight)
                },
                WorkerSprite = EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.WorkerSprite),
                PanelSettings = CreatePanelSettings(),
                Hud = EditorAssetUtility.LoadRequired<VisualTreeAsset>(AnvilClickerPaths.HudUxml),
                ShopRow = EditorAssetUtility.LoadRequired<VisualTreeAsset>(AnvilClickerPaths.ShopRowUxml)
            };
        }

        // --- World ---------------------------------------------------------------------------------

        static void CreateGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = AmbientColor;
            light.intensity = 0.5f;
        }

        static WorldGrid CreateWorld(SceneAssets assets, out Transform stationsRoot)
        {
            var gridGo = new GameObject("Grid");
            var grid = gridGo.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f);

            var floor = CreateTilemap(gridGo.transform, "Floor", "Floor", TilemapRenderer.Mode.Chunk);
            var walls = CreateTilemap(gridGo.transform, "Walls", "World", TilemapRenderer.Mode.Individual);

            // The blacksmith collides with every wall tile as one merged shape.
            var wallBody = walls.gameObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;
            var composite = walls.gameObject.AddComponent<CompositeCollider2D>();
            var tileCollider = walls.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            var world = gridGo.AddComponent<WorldGrid>();
            EditorAssetUtility.Edit(world, so =>
            {
                so.Require("grid").objectReferenceValue = grid;
                so.Require("floor").objectReferenceValue = floor;
                so.Require("walls").objectReferenceValue = walls;
            });

            stationsRoot = new GameObject("Stations").transform;
            return world;
        }

        static Tilemap CreateTilemap(Transform parent, string name, string sortingLayer, TilemapRenderer.Mode mode)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.mode = mode;
            return tilemap;
        }

        static GameObject CreatePlayer(SceneAssets assets, WorldGrid world)
        {
            var go = new GameObject("Player");
            var start = world.Grid.GetCellCenterWorld(new Vector3Int(WorldDataSeeds.PlayerStartCell.x, WorldDataSeeds.PlayerStartCell.y, 0));
            go.transform.position = start;

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.12f;
            collider.offset = new Vector2(0f, 0.04f);

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            var spriteRenderer = spriteGo.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = assets.PlayerSprites[0];
            spriteRenderer.sortingLayerName = "World";

            var controller = go.AddComponent<PlayerController>();
            EditorAssetUtility.Edit(controller, so =>
            {
                so.Require("body").objectReferenceValue = spriteRenderer;
                var sprites = so.Require("facingSprites");
                sprites.arraySize = assets.PlayerSprites.Length;
                for (var i = 0; i < assets.PlayerSprites.Length; i++) sprites.GetArrayElementAtIndex(i).objectReferenceValue = assets.PlayerSprites[i];
            });

            var follower = go.AddComponent<PathFollower>();
            EditorAssetUtility.Edit(follower, so =>
            {
                so.Require("player").objectReferenceValue = controller;
                so.Require("world").objectReferenceValue = world;
            });

            return go;
        }

        static CameraFollow CreateCameraRig(Transform target, WorldGrid world, out Camera camera)
        {
            var rig = new GameObject("Camera Rig");
            rig.transform.position = target.position;

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraGo.transform.SetParent(rig.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 0f, -10f);

            camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;

            cameraGo.AddComponent<UniversalAdditionalCameraData>();
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<CameraShake>();

            var follow = rig.AddComponent<CameraFollow>();
            var cameraRef = camera;
            EditorAssetUtility.Edit(follow, so =>
            {
                so.Require("cam").objectReferenceValue = cameraRef;
                so.Require("target").objectReferenceValue = target;
                so.Require("world").objectReferenceValue = world;
            });
            return follow;
        }

        static void CreateRoomBuilder(SceneAssets assets, WorldGrid world, Transform stationsRoot)
        {
            var go = new GameObject("Room Builder");
            var builder = go.AddComponent<RoomBuilder>();
            EditorAssetUtility.Edit(builder, so =>
            {
                so.Require("database").objectReferenceValue = assets.Database;
                so.Require("world").objectReferenceValue = world;
                so.Require("stationsRoot").objectReferenceValue = stationsRoot;
                so.Require("floorTile").objectReferenceValue = assets.FloorTile;
                so.Require("wallTile").objectReferenceValue = assets.WallTile;
                so.Require("invisibleWallTile").objectReferenceValue = assets.InvisibleWallTile;
                so.Require("barrierPrefab").objectReferenceValue = assets.Barrier;
                so.Require("dustPrefab").objectReferenceValue = assets.Dust;
            });
        }

        static void CreateWorkers(SceneAssets assets)
        {
            var root = new GameObject("Apprentice Workers");
            var spots = new Transform[WorldDataSeeds.WorkerCells.Length];

            // Spots are placed from grid coordinates, so they follow the isometric layout.
            var grid = Object.FindFirstObjectByType<Grid>();
            for (var i = 0; i < spots.Length; i++)
            {
                var cell = WorldDataSeeds.WorkerCells[i];
                var spot = new GameObject($"Spot {i + 1}").transform;
                spot.SetParent(root.transform, false);
                spot.position = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
                spots[i] = spot;
            }

            var workers = root.AddComponent<ApprenticeWorkers>();
            EditorAssetUtility.Edit(workers, so =>
            {
                so.Require("workerSprite").objectReferenceValue = assets.WorkerSprite;
                var list = so.Require("spots");
                list.arraySize = spots.Length;
                for (var i = 0; i < spots.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = spots[i];
            });
        }

        // --- UI ------------------------------------------------------------------------------------

        sealed class UiRig
        {
            public UIDocument Document;
            public FloatingTextLayer FloatingText;
            public StationPanelsPresenter Panels;
        }

        static UiRig CreateUi(SceneAssets assets, Camera camera)
        {
            var go = new GameObject("HUD");
            var document = go.AddComponent<UIDocument>();
            document.panelSettings = assets.PanelSettings;
            document.visualTreeAsset = assets.Hud;

            go.AddComponent<HudPresenter>();
            go.AddComponent<OfflineSummaryPresenter>();

            var floatingText = go.AddComponent<FloatingTextLayer>();
            EditorAssetUtility.Edit(floatingText, so => so.Require("worldCamera").objectReferenceValue = camera);

            var shop = go.AddComponent<ShopPresenter>();
            EditorAssetUtility.Edit(shop, so => so.Require("rowTemplate").objectReferenceValue = assets.ShopRow);

            var panels = go.AddComponent<StationPanelsPresenter>();
            return new UiRig { Document = document, FloatingText = floatingText, Panels = panels };
        }

        // --- Systems -------------------------------------------------------------------------------

        sealed class SystemsRig
        {
            public GameBootstrap Bootstrap;
        }

        static SystemsRig CreateSystems(SceneAssets assets, Camera camera, GameObject player, WorldGrid world, UiRig ui, CameraFollow cameraRig)
        {
            var go = new GameObject("Systems");

            var bootstrap = go.AddComponent<GameBootstrap>();
            EditorAssetUtility.Edit(bootstrap, so => so.Require("database").objectReferenceValue = assets.Database);
            go.AddComponent<GameLoop>();

            var forgeMode = go.AddComponent<ForgeMode>();
            var playerController = player.GetComponent<PlayerController>();
            EditorAssetUtility.Edit(forgeMode, so => so.Require("player").objectReferenceValue = playerController);

            var interaction = go.AddComponent<InteractionController>();
            EditorAssetUtility.Edit(interaction, so =>
            {
                so.Require("player").objectReferenceValue = playerController;
                so.Require("pathFollower").objectReferenceValue = player.GetComponent<PathFollower>();
                so.Require("world").objectReferenceValue = world;
                so.Require("forgeMode").objectReferenceValue = forgeMode;
                so.Require("worldCamera").objectReferenceValue = camera;
                so.Require("uiDocument").objectReferenceValue = ui.Document;
            });

            var input = go.AddComponent<ForgeInput>();
            EditorAssetUtility.Edit(input, so =>
            {
                so.Require("forgeMode").objectReferenceValue = forgeMode;
                so.Require("worldCamera").objectReferenceValue = camera;
                so.Require("uiDocument").objectReferenceValue = ui.Document;
            });

            var sfx = go.AddComponent<ProceduralSfx>();

            var feedback = go.AddComponent<StrikeFeedback>();
            EditorAssetUtility.Edit(feedback, so =>
            {
                so.Require("forgeMode").objectReferenceValue = forgeMode;
                so.Require("cameraShake").objectReferenceValue = camera.GetComponent<CameraShake>();
                so.Require("floatingText").objectReferenceValue = ui.FloatingText;
                so.Require("sfx").objectReferenceValue = sfx;
            });

            EditorAssetUtility.Edit(ui.Panels, so =>
            {
                so.Require("interaction").objectReferenceValue = interaction;
                so.Require("forgeMode").objectReferenceValue = forgeMode;
            });

            EditorAssetUtility.Edit(cameraRig, so => so.Require("forgeMode").objectReferenceValue = forgeMode);

            return new SystemsRig { Bootstrap = bootstrap };
        }

        /// <summary>Fails the build instead of saving a scene whose asset references were lost.</summary>
        static void Validate(SystemsRig systems, WorldGrid world)
        {
            var database = new SerializedObject(systems.Bootstrap).Require("database").objectReferenceValue as GameDatabase;
            if (database == null || database.Balance == null)
                throw new System.InvalidOperationException("Workshop scene: GameBootstrap lost its GameDatabase reference.");

            if (database.RoomAssets.Count == 0)
                throw new System.InvalidOperationException("Workshop scene: the database has no rooms to build.");

            var gridRef = new SerializedObject(world);
            foreach (var field in new[] { "grid", "floor", "walls" })
            {
                if (gridRef.Require(field).objectReferenceValue == null)
                    throw new System.InvalidOperationException($"Workshop scene: WorldGrid lost its '{field}' reference.");
            }
        }

        static PanelSettings CreatePanelSettings()
        {
            var panelSettings = EditorAssetUtility.LoadOrCreate<PanelSettings>(AnvilClickerPaths.PanelSettings);
            panelSettings.themeStyleSheet = EditorAssetUtility.LoadRequired<ThemeStyleSheet>(AnvilClickerPaths.RuntimeTheme);
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            EditorUtility.SetDirty(panelSettings);
            return panelSettings;
        }
    }
}
