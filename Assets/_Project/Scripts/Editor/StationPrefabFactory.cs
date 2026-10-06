using AnvilClicker.Data;
using AnvilClicker.Presentation;
using AnvilClicker.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnvilClicker.Editor
{
    /// <summary>Builds the station, barrier and dust prefabs from the placeholder sprites. Safe to run repeatedly.</summary>
    internal static class StationPrefabFactory
    {
        static readonly Color ForgeFire = new Color(1f, 0.55f, 0.2f);

        [MenuItem(AnvilClickerPaths.MenuRoot + "Art/Generate Station Prefabs", priority = 21)]
        public static void Generate()
        {
            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.StationPrefabs);
            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.FxPrefabs);

            Save(BuildAnvil(), StationPath("Anvil"));
            Save(BuildBlock("Forge", AnvilClickerPaths.ForgeSprite, new Vector2(0f, 0.3f), new Vector2(0.95f, 0.8f), addForgeFire: true), StationPath("Forge"));
            Save(BuildBlock("Upgrade Desk", AnvilClickerPaths.DeskSprite, new Vector2(0f, 0.25f), new Vector2(0.95f, 0.65f)), StationPath("UpgradeDesk"));
            Save(BuildBlock("Counter", AnvilClickerPaths.CounterSprite, new Vector2(0f, 0.25f), new Vector2(0.95f, 0.7f)), StationPath("Counter"));
            Save(BuildBlock("Messenger Desk", AnvilClickerPaths.MessengerSprite, new Vector2(0f, 0.25f), new Vector2(0.95f, 0.65f)), StationPath("Messenger"));
            Save(BuildBlock("Storage", AnvilClickerPaths.StorageSprite, new Vector2(0f, 0.25f), new Vector2(0.95f, 0.7f)), StationPath("Storage"));
            Save(BuildBarrier(), AnvilClickerPaths.FxPrefabs + "/Barrier.prefab");
            Save(BuildDust(), AnvilClickerPaths.FxPrefabs + "/BuildDust.prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("[Anvil Clicker] Station prefabs ready.");
        }

        public static string StationPath(string name) => $"{AnvilClickerPaths.StationPrefabs}/Station_{name}.prefab";

        // --- Builders ------------------------------------------------------------------------------

        static GameObject NewRoot(string name)
        {
            var root = new GameObject(name);
            root.AddComponent<Station>();
            return root;
        }

        static SpriteRenderer AddSprite(GameObject root, string spritePath, float scale = 1f)
        {
            var child = new GameObject("Sprite");
            child.transform.SetParent(root.transform, false);
            child.transform.localScale = Vector3.one * scale;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = EditorAssetUtility.LoadRequired<Sprite>(spritePath);
            renderer.sortingLayerName = "World";
            return renderer;
        }

        /// <summary>Walls the blacksmith bumps into (a diamond a bit smaller than the cell) and a box to click on.</summary>
        static void AddColliders(GameObject root, Vector2 clickCentre, Vector2 clickSize)
        {
            var body = root.AddComponent<PolygonCollider2D>();
            body.SetPath(0, new[] { new Vector2(-0.45f, 0f), new Vector2(0f, 0.225f), new Vector2(0.45f, 0f), new Vector2(0f, -0.225f) });

            var click = new GameObject("Click Area");
            click.transform.SetParent(root.transform, false);
            var box = click.AddComponent<BoxCollider2D>();
            box.isTrigger = true; // only for mouse picking; it never blocks anybody
            box.offset = clickCentre;
            box.size = clickSize;
        }

        static GameObject BuildBlock(string name, string spritePath, Vector2 _, Vector2 clickSize, bool addForgeFire = false)
        {
            var root = NewRoot(name);
            AddSprite(root, spritePath);
            AddColliders(root, new Vector2(0f, clickSize.y * 0.5f), clickSize);

            if (addForgeFire)
            {
                var lightGo = new GameObject("Fire");
                lightGo.transform.SetParent(root.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);

                var fire = lightGo.AddComponent<Light2D>();
                fire.lightType = Light2D.LightType.Point;
                fire.color = ForgeFire;
                fire.intensity = 1.1f;
                fire.pointLightInnerRadius = 0.3f;
                fire.pointLightOuterRadius = 3.2f;

                var glow = root.AddComponent<ForgeGlow>();
                EditorAssetUtility.Edit(glow, so => so.Require("fire").objectReferenceValue = fire);
            }

            return root;
        }

        static GameObject BuildAnvil()
        {
            var root = NewRoot("Anvil");

            // The anvil art is 2 × 1.5 units at 32 px per unit; scale it down to sit on a one-unit cell.
            var sprite = AddSprite(root, AnvilClickerPaths.AnvilSprite, scale: 0.5f);
            sprite.transform.localPosition = new Vector3(0f, -0.08f, 0f);

            AddColliders(root, new Vector2(0f, 0.38f), new Vector2(0.95f, 0.8f));

            var strikePoint = new GameObject("StrikePoint").transform;
            strikePoint.SetParent(root.transform, false);
            strikePoint.localPosition = new Vector3(0.12f, 0.6f, 0f);

            var sparks = BuildSparks(strikePoint);
            var squash = sprite.gameObject.AddComponent<AnvilSquash>();

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(root.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var glow = glowGo.AddComponent<Light2D>();
            glow.lightType = Light2D.LightType.Point;
            glow.color = ForgeFire;
            glow.intensity = 0.6f;
            glow.pointLightInnerRadius = 0.2f;
            glow.pointLightOuterRadius = 2.4f;

            var rig = root.AddComponent<AnvilRig>();
            EditorAssetUtility.Edit(rig, so =>
            {
                so.Require("strikePoint").objectReferenceValue = strikePoint;
                so.Require("sparks").objectReferenceValue = sparks;
                so.Require("squash").objectReferenceValue = squash;
                so.Require("glow").objectReferenceValue = glow;
            });

            return root;
        }

        static GameObject BuildBarrier()
        {
            var root = new GameObject("Barrier");
            AddSprite(root, AnvilClickerPaths.BarrierSprite);
            return root;
        }

        static GameObject BuildDust()
        {
            var go = new GameObject("BuildDust");
            var dust = go.AddComponent<ParticleSystem>();
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = dust.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startColor = new Color(0.78f, 0.72f, 0.64f, 0.7f);
            main.gravityModifier = -0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = dust.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

            var shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.35f;

            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            var colour = dust.colorOverLifetime;
            colour.enabled = true;
            colour.color = fade;

            var size = dust.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.6f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = ArtMaterials.Sparks();
            renderer.sortingLayerName = "FX";
            return go;
        }

        /// <summary>Sparks thrown by a hammer strike, as in M1.</summary>
        static ParticleSystem BuildSparks(Transform parent)
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
            var colour = sparks.colorOverLifetime;
            colour.enabled = true;
            colour.color = fade;

            var size = sparks.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = ArtMaterials.Sparks();
            renderer.sortingLayerName = "FX";
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.04f;
            renderer.lengthScale = 1.5f;

            return sparks;
        }

        static void Save(GameObject instance, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }
    }

    internal static class ArtMaterials
    {
        /// <summary>Unlit sprite material shared by every particle effect.</summary>
        public static Material Sparks()
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
            if (shader == null) throw new System.InvalidOperationException("No sprite shader found for the particle material.");

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
    }
}
