using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Draws simple pixel-art placeholders so the game is playable before real art exists.
    /// Output is deterministic: regenerating produces byte-identical PNGs.
    /// </summary>
    internal static class PlaceholderArtGenerator
    {
        const int AnvilPixelsPerUnit = 32;
        const int FloorPixelsPerUnit = 64; // 64x32 px tile = 1 x 0.5 units, the isometric grid cell

        static readonly Color32 Outline = new Color32(24, 22, 28, 255);
        static readonly Color32 IronDark = new Color32(62, 64, 74, 255);
        static readonly Color32 IronMid = new Color32(92, 96, 108, 255);
        static readonly Color32 IronLight = new Color32(168, 174, 186, 255);
        static readonly Color32 StoneBase = new Color32(70, 58, 52, 255);
        static readonly Color32 StoneEdge = new Color32(44, 36, 33, 255);
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        [MenuItem(AnvilClickerPaths.MenuRoot + "Art/Generate Placeholder Sprites", priority = 20)]
        public static void Generate()
        {
            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.Placeholders);

            WriteSprite(AnvilClickerPaths.AnvilSprite, DrawAnvil(), AnvilPixelsPerUnit, new Vector2(0.5f, 0f));
            WriteSprite(AnvilClickerPaths.SparkSprite, DrawSpark(), AnvilPixelsPerUnit, new Vector2(0.5f, 0.5f));
            WriteSprite(AnvilClickerPaths.FloorSprite, DrawFloorTile(), FloorPixelsPerUnit, new Vector2(0.5f, 0.5f));
            WriteSprite(AnvilClickerPaths.WhiteSprite, Fill(4, 4, new Color32(255, 255, 255, 255)), AnvilPixelsPerUnit, new Vector2(0.5f, 0.5f));

            CreateFloorTile();
            AssetDatabase.SaveAssets();
            Debug.Log("[Anvil Clicker] Placeholder sprites generated.");
        }

        // --- Drawing -----------------------------------------------------------------------------

        static Texture2D DrawAnvil()
        {
            const int width = 64, height = 48;

            // Silhouette, y = 0 at the bottom: feet, waist, body with a horn on the left and a heel on the right.
            bool Inside(int x, int y)
            {
                if (y < 0 || y >= height || x < 0 || x >= width) return false;
                if (y <= 7) return x >= 14 && x <= 49;                         // feet
                if (y <= 11) return x >= 18 && x <= 45;                        // flare
                if (y <= 21) return x >= 24 && x <= 39;                        // waist
                if (y <= 25) return x >= 18 && x <= 47;                        // shoulders
                if (y <= 35)
                {
                    if (x >= 12 && x <= 54) return true;                       // body
                    var hornHalf = x * 5 / 12;                                 // horn tapers to the left
                    if (x < 12) return y >= 30 - hornHalf && y <= 31 + hornHalf && y >= 27;
                    return x <= 58 && y >= 30;                                 // heel
                }
                return false;
            }

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (!Inside(x, y)) { pixels[y * width + x] = Clear; continue; }

                var edge = !Inside(x - 1, y) || !Inside(x + 1, y) || !Inside(x, y - 1) || !Inside(x, y + 1);
                Color32 color;
                if (edge) color = Outline;
                else if (y >= 33) color = IronLight;                           // polished face
                else if (y <= 7 || x > 44) color = IronDark;                   // shade on feet and right side
                else color = IronMid;

                pixels[y * width + x] = Dither(color, x, y, 6);
            }

            return ToTexture(width, height, pixels);
        }

        static Texture2D DrawSpark()
        {
            const int size = 8;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x + 0.5f - size / 2f) / (size / 2f);
                var dy = (y + 0.5f - size / 2f) / (size / 2f);
                var alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * alpha * 255));
            }
            return ToTexture(size, size, pixels);
        }

        static Texture2D DrawFloorTile()
        {
            const int width = 64, height = 32;
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var d = Mathf.Abs(x + 0.5f - width / 2f) / (width / 2f) + Mathf.Abs(y + 0.5f - height / 2f) / (height / 2f);
                if (d > 1f) { pixels[y * width + x] = Clear; continue; }

                var color = d > 0.9f ? StoneEdge : StoneBase;
                pixels[y * width + x] = Dither(color, x, y, 10);
            }
            return ToTexture(width, height, pixels);
        }

        static Texture2D Fill(int width, int height, Color32 color)
        {
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = color;
            return ToTexture(width, height, pixels);
        }

        /// <summary>Deterministic per-pixel brightness noise so flat areas don't look plastic.</summary>
        static Color32 Dither(Color32 color, int x, int y, int amplitude)
        {
            unchecked
            {
                var hash = (uint)(x * 73856093) ^ (uint)(y * 19349663);
                hash = (hash ^ (hash >> 13)) * 1274126177u;
                var offset = (int)(hash % (uint)(amplitude * 2 + 1)) - amplitude;
                return new Color32(Shift(color.r, offset), Shift(color.g, offset), Shift(color.b, offset), color.a);
            }
        }

        static byte Shift(byte channel, int offset) => (byte)Mathf.Clamp(channel + offset, 0, 255);

        static Texture2D ToTexture(int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        // --- Import ------------------------------------------------------------------------------

        static void WriteSprite(string path, Texture2D texture, int pixelsPerUnit, Vector2 pivot)
        {
            var bytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath) || !BytesEqual(File.ReadAllBytes(fullPath), bytes))
            {
                File.WriteAllBytes(fullPath, bytes);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException($"No texture importer for '{path}'.");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        static void CreateFloorTile()
        {
            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.Tiles);
            var sprite = EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.FloorSprite);

            var tile = AssetDatabase.LoadAssetAtPath<Tile>(AnvilClickerPaths.FloorTile);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, AnvilClickerPaths.FloorTile);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
        }
    }
}
