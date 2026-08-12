// Filename: ProjectConstants.cs
// Folder: Assets/Scripts/Core/
// Purpose: Shared project identity and foundation constants (not gameplay tuning).
// Dependencies: None.

namespace BounderTrail.Core
{
    /// <summary>
    /// Non-gameplay project constants and naming anchors.
    /// Gameplay tuning values belong on components / data assets in later phases.
    /// </summary>
    public static class ProjectConstants
    {
        public const string GameTitle = "Bounder Trail";
        public const string CompanyName = "MGames";
        public const string ProductName = "BounderTrail";

        public const int TargetFrameRate = 60;
        // UI scaling reference size (used by CanvasScaler in editor setup scripts).
        // Kept at 1280x720 so UI doesn't get overly small on common smaller game-view sizes.
        public const int ReferenceWidth = 1280;
        public const int ReferenceHeight = 720;

        // Gameplay viewport — larger ref Y shows more world (PixelPerfectCamera: ortho = refY / (2 * PPU)).
        public const int GameplayRefResolutionX = 640;
        public const int GameplayRefResolutionY = 480;
        public const int GameplayAssetsPpu = 32;
        public const float GameplayOrthographicSize = GameplayRefResolutionY / (2f * GameplayAssetsPpu);

        public const string BootstrapSceneName = "Bootstrap";
        public const string BootstrapObjectName = "GameBootstrap";

        // Core loop scenes (Phase 2)
        public const string MainMenuSceneName = "MainMenu";
        public const string GameplaySceneName = "Gameplay";

        // Campaign level scenes (later phases)
        public const string Level01SceneName = "Level_01_LumenMeadows";
        public const string Level02SceneName = "Level_02_CascadeCliffs";
        public const string Level03SceneName = "Level_03_SkybridgeSpire";
    }
}
