using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AnvilClicker.Editor
{
    /// <summary>
    /// Placeholder art for the walkable workshop: isometric blocks (walls, stations), the blacksmith in four
    /// directions, a worker and the boards that close a room still for sale. Deterministic pixel art.
    /// </summary>
    internal static class WorldArtGenerator
    {
        const int Ppu = 64;

        static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        static readonly Color32 Outline = new Color32(24, 20, 26, 255);

        enum Pattern
        {
            None,
            Planks,
            Bricks
        }

        readonly struct BlockStyle
        {
            public readonly int Height;
            public readonly Color32 Top, Left, Right;
            public readonly Pattern Pattern;
            public readonly Action<Color32[], int, int> Details;

            public BlockStyle(int height, Color32 top, Color32 left, Color32 right, Pattern pattern = Pattern.None, Action<Color32[], int, int> details = null)
            {
                Height = height; Top = top; Left = left; Right = right; Pattern = pattern; Details = details;
            }
        }

        public static void Generate()
        {
            WriteBlock(AnvilClickerPaths.WallSprite, new BlockStyle(34, C(122, 112, 104), C(88, 80, 76), C(62, 56, 56), Pattern.Bricks));
            WriteBlock(AnvilClickerPaths.ForgeSprite, new BlockStyle(26, C(235, 110, 40), C(84, 76, 78), C(58, 52, 56), Pattern.Bricks, ForgeDetails));
            WriteBlock(AnvilClickerPaths.DeskSprite, new BlockStyle(20, C(214, 196, 150), C(128, 88, 52), C(98, 66, 38), Pattern.Planks, DeskDetails));
            WriteBlock(AnvilClickerPaths.CounterSprite, new BlockStyle(22, C(176, 128, 74), C(134, 92, 52), C(102, 68, 38), Pattern.Planks, CounterDetails));
            WriteBlock(AnvilClickerPaths.MessengerSprite, new BlockStyle(20, C(196, 178, 140), C(60, 86, 130), C(44, 64, 104), Pattern.Planks, MessengerDetails));
            WriteBlock(AnvilClickerPaths.StorageSprite, new BlockStyle(24, C(162, 120, 70), C(124, 86, 46), C(92, 62, 34), Pattern.Planks));
            WriteBlock(AnvilClickerPaths.BarrierSprite, new BlockStyle(28, C(150, 104, 58), C(112, 76, 42), C(86, 56, 32), Pattern.Planks, BarrierDetails));

            WritePlayer(AnvilClickerPaths.PlayerDownRight, facingUp: false, flip: false);
            WritePlayer(AnvilClickerPaths.PlayerDownLeft, facingUp: false, flip: true);
            WritePlayer(AnvilClickerPaths.PlayerUpLeft, facingUp: true, flip: true);
            WritePlayer(AnvilClickerPaths.PlayerUpRight, facingUp: true, flip: false);
            WriteWorker(AnvilClickerPaths.WorkerSprite);

            CreateWallTiles();
            AssetDatabase.SaveAssets();
        }

        static Color32 C(byte r, byte g, byte b) => new Color32(r, g, b, 255);

        // --- Blocks --------------------------------------------------------------------------------

        static void WriteBlock(string path, BlockStyle style)
        {
            const int width = 64;
            var height = 32 + style.Height;
            var topCenterY = 16 + style.Height;

            bool Inside(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                var edge = Math.Abs(x + 0.5f - 32f) / 2f;
                return y + 0.5f >= edge && y + 0.5f <= 32f + style.Height - edge;
            }

            bool OnTop(int x, int y) =>
                Math.Abs(x + 0.5f - 32f) / 32f + Math.Abs(y + 0.5f - topCenterY) / 16f <= 1f;

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (!Inside(x, y)) { pixels[y * width + x] = Clear; continue; }

                var edge = !Inside(x - 1, y) || !Inside(x + 1, y) || !Inside(x, y - 1) || !Inside(x, y + 1);
                var top = OnTop(x, y);
                Color32 color = top ? style.Top : x < 32 ? style.Left : style.Right;

                if (!top && !edge) color = ApplyPattern(color, style.Pattern, x, y);
                else if (edge) color = Outline;

                pixels[y * width + x] = PlaceholderArtGenerator.Dither(color, x, y, top ? 8 : 5);
            }

            style.Details?.Invoke(pixels, width, height);

            // Pivot at the middle of the footprint, so the block's feet sit on the cell centre.
            PlaceholderArtGenerator.WriteSprite(path, PlaceholderArtGenerator.ToTexture(width, height, pixels), Ppu, new Vector2(0.5f, 16f / height));
        }

        static Color32 ApplyPattern(Color32 color, Pattern pattern, int x, int y)
        {
            switch (pattern)
            {
                case Pattern.Planks:
                    return y % 8 == 0 ? Darker(color, 28) : color;
                case Pattern.Bricks:
                    var row = y / 8;
                    var offset = row % 2 == 0 ? 0 : 8;
                    return y % 8 == 0 || (x + offset) % 16 == 0 ? Darker(color, 22) : color;
                default:
                    return color;
            }
        }

        static Color32 Darker(Color32 color, int amount) =>
            new Color32(PlaceholderArtGenerator.Shift(color.r, -amount), PlaceholderArtGenerator.Shift(color.g, -amount), PlaceholderArtGenerator.Shift(color.b, -amount), color.a);

        static void Fill(Color32[] pixels, int width, int x0, int y0, int w, int h, Color32 color)
        {
            for (var y = y0; y < y0 + h; y++)
            for (var x = x0; x < x0 + w; x++)
            {
                var i = y * width + x;
                if (i >= 0 && i < pixels.Length && pixels[i].a > 0) pixels[i] = color;
            }
        }

        static void ForgeDetails(Color32[] p, int width, int height)
        {
            // Embers on the top face and a dark fire mouth on each side.
            Fill(p, width, 26, 40, 12, 3, C(255, 196, 70));
            Fill(p, width, 22, 36, 6, 2, C(255, 160, 50));
            Fill(p, width, 8, 14, 14, 12, C(30, 22, 24));
            Fill(p, width, 10, 16, 10, 5, C(240, 110, 40));
            Fill(p, width, 42, 14, 14, 12, C(26, 20, 22));
            Fill(p, width, 44, 16, 10, 5, C(220, 90, 36));
        }

        static void DeskDetails(Color32[] p, int width, int height)
        {
            // Scrolls and an inkwell on the top face.
            Fill(p, width, 20, 36, 14, 2, C(150, 124, 86));
            Fill(p, width, 24, 40, 12, 2, C(150, 124, 86));
            Fill(p, width, 38, 38, 3, 4, C(36, 34, 54));
        }

        static void CounterDetails(Color32[] p, int width, int height)
        {
            // A little pile of coins.
            Fill(p, width, 28, 40, 8, 2, C(255, 205, 70));
            Fill(p, width, 30, 42, 5, 2, C(255, 226, 120));
            Fill(p, width, 42, 38, 4, 2, C(255, 205, 70));
        }

        static void MessengerDetails(Color32[] p, int width, int height)
        {
            // A letter with a red wax seal.
            Fill(p, width, 26, 36, 14, 4, C(244, 238, 220));
            Fill(p, width, 31, 37, 3, 2, C(190, 40, 40));
        }

        static void BarrierDetails(Color32[] p, int width, int height)
        {
            // Crossed boards.
            for (var i = 0; i < 26; i++)
            {
                Fill(p, width, 12 + i, 6 + i, 3, 2, C(184, 140, 84));
                Fill(p, width, 49 - i, 6 + i, 3, 2, C(168, 124, 72));
            }
        }

        // --- Characters ----------------------------------------------------------------------------

        static void WritePlayer(string path, bool facingUp, bool flip)
        {
            const int width = 32, height = 48;
            var pixels = new Color32[width * height];

            // Shadow on the ground.
            Ellipse(pixels, width, height, 16, 3, 9, 3, new Color32(0, 0, 0, 90));

            var skin = C(236, 190, 150);
            var hair = C(92, 58, 34);
            var tunic = C(168, 66, 48);
            var apron = C(96, 70, 48);
            var trousers = C(52, 48, 64);
            var steel = C(168, 174, 186);

            Rect(pixels, width, 11, 4, 4, 12, trousers);         // legs
            Rect(pixels, width, 17, 4, 4, 12, trousers);
            Rect(pixels, width, 9, 15, 14, 16, tunic);           // torso
            if (!facingUp) Rect(pixels, width, 11, 15, 10, 12, apron);
            Rect(pixels, width, 6, 17, 4, 11, tunic);            // arms
            Rect(pixels, width, 22, 17, 4, 11, tunic);
            Rect(pixels, width, 6, 15, 4, 3, skin);              // hands
            Rect(pixels, width, 22, 15, 4, 3, skin);
            Ellipse(pixels, width, height, 16, 36, 7, 7, skin);  // head
            Ellipse(pixels, width, height, 16, 39, 7, 5, hair);  // hair
            if (facingUp) Ellipse(pixels, width, height, 16, 36, 7, 7, hair);

            if (!facingUp)
            {
                Rect(pixels, width, 13, 35, 2, 2, Outline);      // eyes, shifted to the side the character faces
                Rect(pixels, width, 18, 35, 2, 2, Outline);
            }

            Rect(pixels, width, 24, 20, 3, 10, C(120, 84, 52));  // hammer: handle and head
            Rect(pixels, width, 22, 28, 8, 5, steel);

            AddOutline(pixels, width, height);
            if (flip) FlipHorizontally(pixels, width, height);

            PlaceholderArtGenerator.WriteSprite(path, PlaceholderArtGenerator.ToTexture(width, height, pixels), Ppu, new Vector2(0.5f, 0.07f));
        }

        static void WriteWorker(string path)
        {
            const int width = 24, height = 32;
            var pixels = new Color32[width * height];

            Ellipse(pixels, width, height, 12, 2, 7, 2, new Color32(0, 0, 0, 90));
            Rect(pixels, width, 7, 3, 10, 14, C(235, 235, 235));
            Ellipse(pixels, width, height, 12, 22, 5, 5, C(250, 244, 236));
            Rect(pixels, width, 18, 9, 3, 10, C(124, 92, 60));
            Rect(pixels, width, 16, 17, 7, 4, C(180, 186, 196));
            AddOutline(pixels, width, height);

            PlaceholderArtGenerator.WriteSprite(path, PlaceholderArtGenerator.ToTexture(width, height, pixels), Ppu, new Vector2(0.5f, 0.07f));
        }

        static void Rect(Color32[] pixels, int width, int x0, int y0, int w, int h, Color32 color)
        {
            for (var y = y0; y < y0 + h; y++)
            for (var x = x0; x < x0 + w; x++)
            {
                var i = y * width + x;
                if (x >= 0 && x < width && i >= 0 && i < pixels.Length) pixels[i] = color;
            }
        }

        static void Ellipse(Color32[] pixels, int width, int height, int cx, int cy, int rx, int ry, Color32 color)
        {
            for (var y = cy - ry; y <= cy + ry; y++)
            for (var x = cx - rx; x <= cx + rx; x++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) continue;
                var dx = (x - cx) / (float)rx;
                var dy = (y - cy) / (float)ry;
                if (dx * dx + dy * dy <= 1f) pixels[y * width + x] = color;
            }
        }

        /// <summary>Dark outline around every opaque shape (shadow pixels are excluded).</summary>
        static void AddOutline(Color32[] pixels, int width, int height)
        {
            var copy = (Color32[])pixels.Clone();
            bool Solid(int x, int y) => x >= 0 && x < width && y >= 0 && y < height && copy[y * width + x].a > 200;

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (!Solid(x, y)) continue;
                if (!Solid(x - 1, y) || !Solid(x + 1, y) || !Solid(x, y - 1) || !Solid(x, y + 1)) pixels[y * width + x] = Outline;
            }
        }

        static void FlipHorizontally(Color32[] pixels, int width, int height)
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width / 2; x++)
            {
                var a = y * width + x;
                var b = y * width + (width - 1 - x);
                (pixels[a], pixels[b]) = (pixels[b], pixels[a]);
            }
        }

        // --- Tiles ---------------------------------------------------------------------------------

        static void CreateWallTiles()
        {
            EditorAssetUtility.EnsureFolder(AnvilClickerPaths.Tiles);

            var wall = LoadOrCreateTile(AnvilClickerPaths.WallTile);
            wall.sprite = EditorAssetUtility.LoadRequired<Sprite>(AnvilClickerPaths.WallSprite);
            wall.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(wall);

            var invisible = LoadOrCreateTile(AnvilClickerPaths.InvisibleWallTile);
            invisible.sprite = null;
            invisible.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(invisible);
        }

        static Tile LoadOrCreateTile(string path)
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null) return tile;

            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
            return tile;
        }
    }
}
