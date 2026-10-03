namespace AnvilClicker.Editor
{
    /// <summary>Every asset path the editor tools read or write. Change a location here only.</summary>
    internal static class AnvilClickerPaths
    {
        public const string MenuRoot = "Tools/Anvil Clicker/";

        public const string Root = "Assets/_Project";

        public const string Art = Root + "/Art";
        public const string Placeholders = Art + "/Placeholders";
        public const string Materials = Art + "/Materials";
        public const string Tiles = Art + "/Tiles";
        public const string Audio = Root + "/Audio";
        public const string Prefabs = Root + "/Prefabs";
        public const string Scenes = Root + "/Scenes";
        public const string ScriptableObjects = Root + "/ScriptableObjects";
        public const string Weapons = ScriptableObjects + "/Weapons";
        public const string Balance = ScriptableObjects + "/Balance";
        public const string Settings = Root + "/Settings";
        public const string UI = Root + "/UI";

        public static readonly string[] AllFolders =
        {
            Root, Art, Placeholders, Materials, Tiles, Audio, Prefabs, Scenes,
            ScriptableObjects, Weapons, Balance, Settings, UI,
            Root + "/Scripts", Root + "/Tests"
        };

        public const string AnvilSprite = Placeholders + "/anvil.png";
        public const string SparkSprite = Placeholders + "/spark.png";
        public const string FloorSprite = Placeholders + "/floor_stone.png";
        public const string WhiteSprite = Placeholders + "/white.png";
        public const string SparksMaterial = Materials + "/Sparks.mat";
        public const string FloorTile = Tiles + "/FloorStone.asset";

        public const string IronDagger = Weapons + "/Weapon_IronDagger.asset";
        public const string GameBalance = Balance + "/GameBalance.asset";
        public const string GameDatabase = ScriptableObjects + "/GameDatabase.asset";

        public const string Renderer2D = Settings + "/Rendering/Renderer2D.asset";
        public const string InputActions = Settings + "/Input/AnvilControls.inputactions";

        public const string HudUxml = UI + "/HUD/HUD.uxml";
        public const string RuntimeTheme = UI + "/Themes/AnvilRuntimeTheme.tss";
        public const string PanelSettings = UI + "/AnvilPanelSettings.asset";

        public const string WorkshopScene = Scenes + "/Workshop.unity";
    }
}
