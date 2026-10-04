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
    /// not the scene file. Requires placeholder art and sample data (see <see cref="AnvilClickerSetup"/>).
    /// </summary>
    internal static class WorkshopSceneBuilder
    {
        const int FloorRadius = 4;

        static readonly Color BackgroundColor = new Color(0.07f, 0.055f, 0.06f);
        static readonly Color AmbientColor = new Color(0.72f, 0.68f, 0.82f);
        static readonly Color ForgeLightColor = new Color(1f, 0.55f, 0.22f);

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
            var database = EditorAssetUtility.LoadRequired<GameDatabase>(AnvilClickerPaths.GameDatabase);
            var anvilSprite = EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.AnvilSprite);
            var floorTile = EditorAssetUtility.LoadRequired<Tile>(AnvilClickerPaths.FloorTile);
            var sparksMaterial = CreateSparksMaterial();
            var panelSettings = CreatePanelSettings();
            var hud = EditorAssetUtility.LoadRequired<VisualTreeAsset>(AnvilClickerPaths.HudUxml);

            var camera = CreateCamera();
            CreateGlobalLight();
            var floor = CreateFloor(floorTile);
            var anvil = CreateAnvil(anvilSprite, sparksMaterial);
            var ui = CreateUi(panelSettings, hud, camera);
            var bootstrap = CreateSystems(database, camera, anvil, ui);

            Validate(bootstrap, floor);
            EditorSceneManager.SaveScene(scene, AnvilClickerPaths.WorkshopScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(AnvilClickerPaths.WorkshopScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Anvil Clicker] Scene built at {AnvilClickerPaths.WorkshopScene}.");
        }

        // --- World ---------------------------------------------------------------------------------

        static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0.9f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;

            go.AddComponent<UniversalAdditionalCameraData>();
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraShake>();
            return camera;
        }

        static void CreateGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = AmbientColor;
            light.intensity = 0.45f;
        }

        static Tilemap CreateFloor(Tile floorTile)
        {
            var grid = new GameObject("Grid").AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f);

            var floorGo = new GameObject("Floor");
            floorGo.transform.SetParent(grid.transform, false);
            var tilemap = floorGo.AddComponent<Tilemap>();
            var renderer = floorGo.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = "Floor";
            renderer.mode = TilemapRenderer.Mode.Chunk;

            for (var x = -FloorRadius; x <= FloorRadius; x++)
            for (var y = -FloorRadius; y <= FloorRadius; y++)
                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);

            return tilemap;
        }

        sealed class AnvilRig
        {
            public GameObject Root;
            public Collider2D Collider;
            public Transform StrikePoint;
            public ParticleSystem Sparks;
            public AnvilSquash Squash;
            public Light2D ForgeLight;
        }

        static AnvilRig CreateAnvil(Sprite sprite, Material sparksMaterial)
        {
            var root = new GameObject("Anvil");
            var spriteRenderer = root.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingLayerName = "World";

            var collider = root.AddComponent<BoxCollider2D>(); // auto-sized to the sprite
            var squash = root.AddComponent<AnvilSquash>();

            var strikePoint = new GameObject("StrikePoint").transform;
            strikePoint.SetParent(root.transform, false);
            strikePoint.localPosition = new Vector3(0.25f, 1.12f, 0f);

            var sparks = CreateSparks(strikePoint, sparksMaterial);

            var lightGo = new GameObject("Forge Glow");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var forgeLight = lightGo.AddComponent<Light2D>();
            forgeLight.lightType = Light2D.LightType.Point;
            forgeLight.color = ForgeLightColor;
            forgeLight.intensity = 0.9f;
            forgeLight.pointLightInnerRadius = 0.4f;
            forgeLight.pointLightOuterRadius = 3.8f;

            return new AnvilRig
            {
                Root = root,
                Collider = collider,
                StrikePoint = strikePoint,
                Sparks = sparks,
                Squash = squash,
                ForgeLight = forgeLight
            };
        }

        static ParticleSystem CreateSparks(Transform parent, Material material)
        {
            var go = new GameObject("Sparks");
            go.transform.SetParent(parent, false);

            var sparks = go.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = sparks.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.45f), new Color(1f, 0.45f, 0.1f));
            main.gravityModifier = 1.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 400;

            var emission = sparks.emission;
            emission.rateOverTime = 0f;

            var shape = sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 55f;
            shape.radius = 0.05f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // cone points up (+Y) instead of +Z

            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            var colorOverLifetime = sparks.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = fade;

            var sizeOverLifetime = sparks.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = "FX";
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.04f;
            renderer.lengthScale = 1.5f;

            return sparks;
        }

        // --- UI ------------------------------------------------------------------------------------

        sealed class UiRig
        {
            public FloatingTextLayer FloatingText;
        }

        static UiRig CreateUi(PanelSettings panelSettings, VisualTreeAsset hud, Camera camera)
        {
            var go = new GameObject("HUD");
            var document = go.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = hud;

            go.AddComponent<HudPresenter>();
            var floatingText = go.AddComponent<FloatingTextLayer>();
            EditorAssetUtility.Edit(floatingText, so => so.Require("worldCamera").objectReferenceValue = camera);

            return new UiRig { FloatingText = floatingText };
        }

        // --- Systems -------------------------------------------------------------------------------

        static GameBootstrap CreateSystems(GameDatabase database, Camera camera, AnvilRig anvil, UiRig ui)
        {
            var go = new GameObject("Systems");

            var bootstrap = go.AddComponent<GameBootstrap>();
            EditorAssetUtility.Edit(bootstrap, so => so.Require("database").objectReferenceValue = database);

            var input = go.AddComponent<ForgeInput>();
            EditorAssetUtility.Edit(input, so =>
            {
                so.Require("anvilCollider").objectReferenceValue = anvil.Collider;
                so.Require("worldCamera").objectReferenceValue = camera;
            });

            var sfx = go.AddComponent<ProceduralSfx>();

            var feedback = go.AddComponent<StrikeFeedback>();
            EditorAssetUtility.Edit(feedback, so =>
            {
                so.Require("strikePoint").objectReferenceValue = anvil.StrikePoint;
                so.Require("sparks").objectReferenceValue = anvil.Sparks;
                so.Require("squash").objectReferenceValue = anvil.Squash;
                so.Require("cameraShake").objectReferenceValue = camera.GetComponent<CameraShake>();
                so.Require("floatingText").objectReferenceValue = ui.FloatingText;
                so.Require("sfx").objectReferenceValue = sfx;
                so.Require("forgeLight").objectReferenceValue = anvil.ForgeLight;
            });

            return bootstrap;
        }

        /// <summary>Fails the build instead of saving a scene whose asset references were lost.</summary>
        static void Validate(GameBootstrap bootstrap, Tilemap floor)
        {
            var database = new SerializedObject(bootstrap).Require("database").objectReferenceValue as GameDatabase;
            if (database == null || database.Balance == null)
                throw new System.InvalidOperationException("Workshop scene: GameBootstrap lost its GameDatabase reference.");

            if (floor.GetUsedTilesCount() == 0)
                throw new System.InvalidOperationException("Workshop scene: the floor tilemap has no tiles.");
        }

        // --- Assets --------------------------------------------------------------------------------

        static Material CreateSparksMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null) throw new System.InvalidOperationException("No sprite shader found for the sparks material.");

            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.Materials);
            var material = AssetDatabase.LoadAssetAtPath<Material>(AnvilClickerPaths.SparksMaterial);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, AnvilClickerPaths.SparksMaterial);
            }

            material.shader = shader;
            material.mainTexture = EditorAssetUtility.LoadRequired<Texture2D>(AnvilClickerPaths.SparkSprite);
            EditorUtility.SetDirty(material);
            return material;
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
