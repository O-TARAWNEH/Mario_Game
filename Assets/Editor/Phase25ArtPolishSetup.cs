// Filename: Phase25ArtPolishSetup.cs
// Folder: Assets/Editor/
// Purpose: Replaces placeholder sprites with geometric stylized art + level backdrops/UI chrome (Phase 25).
// Menu: Bounder Trail/Phase 25/Setup Art And Visual Polish
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase25ArtPolishSetup.SetupArtAndVisualPolish
// Does NOT change gameplay systems — art paths, visual wiring, and backdrop only.

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase25ArtPolishSetup
    {
        private const float Ppu = 32f;

        private static readonly Color PipBody = new Color(0.22f, 0.82f, 0.98f, 1f);
        private static readonly Color PipFace = new Color(0.78f, 0.97f, 1f, 1f);
        private static readonly Color PipShade = new Color(0.12f, 0.55f, 0.72f, 1f);
        private static readonly Color PipEye = new Color(0.08f, 0.14f, 0.22f, 1f);
        private static readonly Color Ink = new Color(0.08f, 0.12f, 0.16f, 1f);

        [MenuItem("Bounder Trail/Phase 25/Setup Art And Visual Polish")]
        public static void SetupArtAndVisualPolish()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Player");
            EnsureFolder("Assets/Art", "Enemies");
            EnsureFolder("Assets/Art", "World");
            EnsureFolder("Assets/Art", "Items");
            EnsureFolder("Assets/Art", "UI");
            EnsureFolder("Assets/Art", "Tiles");
            EnsureFolder("Assets/Art", "Backgrounds");
            EnsureFolder("Assets/Art", "VFX");
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets/Animations", "Player");

            var pipIdle = BuildPipFrames("Pip_Idle", PipPose.Idle, 2);
            var pipWalk = BuildPipFrames("Pip_Walk", PipPose.Walk, 4);
            var pipRun = BuildPipFrames("Pip_Run", PipPose.Run, 4);
            var pipJump = BuildPipFrames("Pip_Jump", PipPose.Jump, 1);
            var pipFall = BuildPipFrames("Pip_Fall", PipPose.Fall, 1);
            var pipLand = BuildPipFrames("Pip_Land", PipPose.Land, 2);
            var pipDeath = BuildPipFrames("Pip_Death", PipPose.Death, 3);
            WriteSprite("Assets/Art/Player/Pip_Placeholder.png", DrawPip(PipPose.Idle, 0, 2), 32, 32);

            BindPipClip("Anim_Pip_Idle", pipIdle, 4f, true);
            BindPipClip("Anim_Pip_Walk", pipWalk, 10f, true);
            BindPipClip("Anim_Pip_Run", pipRun, 14f, true);
            BindPipClip("Anim_Pip_Jump", pipJump, 8f, true);
            BindPipClip("Anim_Pip_Fall", pipFall, 8f, true);
            BindPipClip("Anim_Pip_Land", pipLand, 12f, false);
            BindPipClip("Anim_Pip_Death", pipDeath, 8f, false);

            BuildEnemyArt();
            BuildWorldArt();
            BuildItemArt();
            BuildVfxArt();
            var uiPanel = WriteSprite("Assets/Art/UI/UI_Panel.png", DrawUiPanel(), 64, 64);
            var uiButton = WriteSprite("Assets/Art/UI/UI_Button.png", DrawUiButton(), 64, 32);
            var uiBar = WriteSprite("Assets/Art/UI/UI_HudBar.png", DrawUiHudBar(), 128, 32);
            WireTile();
            ApplyUiChrome(uiPanel, uiButton, uiBar);
            PlaceLevelBackdrops();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 25 art polish complete (geometric style, gameplay unchanged).");
        }

        private enum PipPose
        {
            Idle,
            Walk,
            Run,
            Jump,
            Fall,
            Land,
            Death
        }

        private static Sprite[] BuildPipFrames(string baseName, PipPose pose, int frames)
        {
            var sprites = new Sprite[frames];
            for (var i = 0; i < frames; i++)
            {
                sprites[i] = WriteSprite($"Assets/Art/Player/{baseName}_{i}.png", DrawPip(pose, i, frames), 32, 32);
            }

            return sprites;
        }

        private static Color[] DrawPip(PipPose pose, int frame, int frames)
        {
            var c = Clear(32, 32);
            var t = frames <= 1 ? 0f : frame / (float)(frames - 1);
            var bob = pose == PipPose.Idle || pose == PipPose.Walk || pose == PipPose.Run
                ? (frame % 2 == 0 ? 0 : 1)
                : 0;
            var lean = (pose == PipPose.Walk || pose == PipPose.Run) && frame % 2 == 1 ? 1 : 0;
            var bodyY = 10 + bob;
            var bodyH = 14;
            var bodyW = 14;
            var bodyX = 9 + lean;

            switch (pose)
            {
                case PipPose.Jump:
                    bodyY = 12;
                    bodyH = 16;
                    bodyW = 12;
                    bodyX = 10;
                    break;
                case PipPose.Fall:
                    bodyY = 9;
                    bodyH = 12;
                    bodyW = 16;
                    bodyX = 8;
                    break;
                case PipPose.Land:
                    bodyY = 8;
                    bodyH = 11;
                    bodyW = 16;
                    bodyX = 8;
                    break;
                case PipPose.Death:
                    bodyY = 6;
                    bodyH = 10;
                    bodyW = 18;
                    bodyX = 7;
                    break;
            }

            FillEllipse(c, 32, 32, bodyX + bodyW / 2, bodyY + bodyH / 2, bodyW / 2, bodyH / 2, PipBody);
            FillEllipse(c, 32, 32, bodyX + bodyW / 2, bodyY + bodyH / 2 + 1, bodyW / 2 - 2, bodyH / 2 - 3, PipFace);
            FillRect(c, 32, 32, bodyX + 1, bodyY + 1, bodyW - 2, 3, PipShade);

            if (pose != PipPose.Death)
            {
                var eyeY = bodyY + bodyH / 2 + 2;
                Plot(c, 32, 32, bodyX + 4, eyeY, PipEye);
                Plot(c, 32, 32, bodyX + bodyW - 5, eyeY, PipEye);
                Plot(c, 32, 32, bodyX + 5, eyeY, Color.white);
                Plot(c, 32, 32, bodyX + bodyW - 4, eyeY, Color.white);
            }
            else
            {
                // X eyes
                Plot(c, 32, 32, bodyX + 4, bodyY + 5, Ink);
                Plot(c, 32, 32, bodyX + 6, bodyY + 7, Ink);
                Plot(c, 32, 32, bodyX + 6, bodyY + 5, Ink);
                Plot(c, 32, 32, bodyX + 4, bodyY + 7, Ink);
                Plot(c, 32, 32, bodyX + bodyW - 7, bodyY + 5, Ink);
                Plot(c, 32, 32, bodyX + bodyW - 5, bodyY + 7, Ink);
                Plot(c, 32, 32, bodyX + bodyW - 5, bodyY + 5, Ink);
                Plot(c, 32, 32, bodyX + bodyW - 7, bodyY + 7, Ink);
            }

            // Feet / limb cues for locomotion readability.
            if (pose == PipPose.Walk || pose == PipPose.Run)
            {
                var step = frame % 4;
                var footY = bodyY - 2;
                FillRect(c, 32, 32, bodyX + 2 + (step == 1 || step == 2 ? 2 : 0), footY, 4, 3, PipShade);
                FillRect(c, 32, 32, bodyX + bodyW - 6 - (step == 0 || step == 3 ? 2 : 0), footY, 4, 3, PipShade);
            }
            else if (pose == PipPose.Jump)
            {
                FillRect(c, 32, 32, bodyX + 2, bodyY - 1, 3, 3, PipShade);
                FillRect(c, 32, 32, bodyX + bodyW - 5, bodyY - 1, 3, 3, PipShade);
            }
            else if (pose != PipPose.Death)
            {
                FillRect(c, 32, 32, bodyX + 3, bodyY - 1, 3, 2, PipShade);
                FillRect(c, 32, 32, bodyX + bodyW - 6, bodyY - 1, 3, 2, PipShade);
            }

            OutlineEllipse(c, 32, 32, bodyX + bodyW / 2, bodyY + bodyH / 2, bodyW / 2, bodyH / 2, Ink);
            _ = t;
            return c;
        }

        private static void BuildEnemyArt()
        {
            WriteSprite("Assets/Art/Enemies/Crawlbug_Placeholder.png", DrawCrawlbug(0), 28, 22);
            WriteSprite("Assets/Art/Enemies/Enemy_Crawlbug_0.png", DrawCrawlbug(0), 28, 22);
            WriteSprite("Assets/Art/Enemies/Enemy_Dartling_0.png", DrawDartling(), 28, 22);
            WriteSprite("Assets/Art/Enemies/Enemy_Hopmite_0.png", DrawHopmite(), 26, 24);
            WriteSprite("Assets/Art/Enemies/Enemy_Skimmer_0.png", DrawSkimmer(), 30, 18);
            WriteSprite("Assets/Art/Enemies/Enemy_Spikewatch_0.png", DrawSpikewatch(), 28, 28);
            WriteSprite("Assets/Art/Enemies/Enemy_Spitter_0.png", DrawSpitter(), 28, 26);
            WriteSprite("Assets/Art/Enemies/Enemy_Projectile_0.png", DrawProjectile(), 12, 12);
        }

        private static Color[] DrawCrawlbug(int frame)
        {
            var c = Clear(28, 22);
            var red = new Color(0.86f, 0.24f, 0.24f, 1f);
            var dark = new Color(0.55f, 0.12f, 0.12f, 1f);
            FillEllipse(c, 28, 22, 14, 11, 10, 7, red);
            FillEllipse(c, 28, 22, 14, 12, 7, 4, new Color(1f, 0.45f, 0.4f, 1f));
            Plot(c, 28, 22, 10, 13, PipEye);
            Plot(c, 28, 22, 17, 13, PipEye);
            // Legs
            var legShift = frame % 2;
            FillRect(c, 28, 22, 6, 3 + legShift, 2, 4, dark);
            FillRect(c, 28, 22, 12, 2 + (1 - legShift), 2, 4, dark);
            FillRect(c, 28, 22, 18, 3 + legShift, 2, 4, dark);
            OutlineEllipse(c, 28, 22, 14, 11, 10, 7, Ink);
            return c;
        }

        private static Color[] DrawDartling()
        {
            var c = Clear(28, 22);
            var orange = new Color(1f, 0.48f, 0.16f, 1f);
            FillTriangle(c, 28, 22, 24, 11, 4, 4, 4, 18, orange);
            FillRect(c, 28, 22, 8, 9, 10, 5, new Color(1f, 0.7f, 0.35f, 1f));
            Plot(c, 28, 22, 18, 12, PipEye);
            OutlineEllipse(c, 28, 22, 12, 11, 8, 5, Ink);
            return c;
        }

        private static Color[] DrawHopmite()
        {
            var c = Clear(26, 24);
            var lime = new Color(0.5f, 0.9f, 0.28f, 1f);
            FillEllipse(c, 26, 24, 13, 13, 9, 8, lime);
            FillEllipse(c, 26, 24, 13, 14, 6, 5, new Color(0.75f, 1f, 0.45f, 1f));
            Plot(c, 26, 24, 10, 15, PipEye);
            Plot(c, 26, 24, 16, 15, PipEye);
            FillRect(c, 26, 24, 6, 4, 3, 4, new Color(0.3f, 0.55f, 0.15f, 1f));
            FillRect(c, 26, 24, 17, 4, 3, 4, new Color(0.3f, 0.55f, 0.15f, 1f));
            OutlineEllipse(c, 26, 24, 13, 13, 9, 8, Ink);
            return c;
        }

        private static Color[] DrawSkimmer()
        {
            var c = Clear(30, 18);
            var blue = new Color(0.35f, 0.7f, 1f, 1f);
            FillEllipse(c, 30, 18, 15, 9, 11, 5, blue);
            FillEllipse(c, 30, 18, 15, 10, 7, 3, new Color(0.7f, 0.9f, 1f, 1f));
            FillEllipse(c, 30, 18, 6, 9, 4, 2, new Color(0.85f, 0.95f, 1f, 0.9f));
            FillEllipse(c, 30, 18, 24, 9, 4, 2, new Color(0.85f, 0.95f, 1f, 0.9f));
            Plot(c, 30, 18, 12, 10, PipEye);
            Plot(c, 30, 18, 18, 10, PipEye);
            OutlineEllipse(c, 30, 18, 15, 9, 11, 5, Ink);
            return c;
        }

        private static Color[] DrawSpikewatch()
        {
            var c = Clear(28, 28);
            var gray = new Color(0.72f, 0.74f, 0.78f, 1f);
            FillEllipse(c, 28, 28, 14, 12, 8, 8, gray);
            FillTriangle(c, 28, 28, 14, 26, 8, 14, 20, 14, new Color(0.55f, 0.58f, 0.62f, 1f));
            FillTriangle(c, 28, 28, 14, 2, 8, 12, 20, 12, new Color(0.55f, 0.58f, 0.62f, 1f));
            FillTriangle(c, 28, 28, 2, 14, 12, 8, 12, 20, new Color(0.55f, 0.58f, 0.62f, 1f));
            FillTriangle(c, 28, 28, 26, 14, 16, 8, 16, 20, new Color(0.55f, 0.58f, 0.62f, 1f));
            Plot(c, 28, 28, 12, 13, PipEye);
            Plot(c, 28, 28, 16, 13, PipEye);
            OutlineEllipse(c, 28, 28, 14, 12, 8, 8, Ink);
            return c;
        }

        private static Color[] DrawSpitter()
        {
            var c = Clear(28, 26);
            var purple = new Color(0.72f, 0.35f, 0.88f, 1f);
            FillRect(c, 28, 26, 6, 4, 16, 14, purple);
            FillRect(c, 28, 26, 8, 16, 12, 6, new Color(0.5f, 0.22f, 0.65f, 1f));
            FillEllipse(c, 28, 26, 14, 18, 5, 4, new Color(0.95f, 0.7f, 1f, 1f));
            Plot(c, 28, 26, 11, 20, PipEye);
            Plot(c, 28, 26, 16, 20, PipEye);
            FillRect(c, 28, 26, 12, 22, 4, 3, new Color(1f, 0.85f, 0.25f, 1f));
            OutlineRect(c, 28, 26, 6, 4, 16, 14, Ink);
            return c;
        }

        private static Color[] DrawProjectile()
        {
            var c = Clear(12, 12);
            FillEllipse(c, 12, 12, 6, 6, 5, 5, new Color(1f, 0.9f, 0.25f, 1f));
            FillEllipse(c, 12, 12, 6, 6, 2, 2, Color.white);
            OutlineEllipse(c, 12, 12, 6, 6, 5, 5, Ink);
            return c;
        }

        private static void BuildWorldArt()
        {
            WriteSprite("Assets/Art/World/Env_Solid.png", DrawPlatform(new Color(0.32f, 0.72f, 0.34f), new Color(0.55f, 0.88f, 0.45f)), 64, 16);
            WriteSprite("Assets/Art/World/Env_OneWay.png", DrawPlatform(new Color(0.4f, 0.82f, 0.55f), new Color(0.7f, 0.95f, 0.7f)), 64, 12);
            WriteSprite("Assets/Art/World/Env_Moving.png", DrawPlatform(new Color(0.25f, 0.65f, 0.85f), new Color(0.55f, 0.85f, 1f)), 64, 14);
            WriteSprite("Assets/Art/World/Env_Bounce.png", DrawBounce(), 48, 16);
            WriteSprite("Assets/Art/World/Env_Exit.png", DrawExit(), 24, 40);
            WriteSprite("Assets/Art/World/Ground_Placeholder.png", DrawPlatform(new Color(0.32f, 0.72f, 0.34f), new Color(0.55f, 0.88f, 0.45f)), 64, 16);
            WriteSprite("Assets/Art/World/Hazard_DeathZone.png", DrawDeathZone(), 96, 24);
            WriteSprite("Assets/Art/World/Hazard_Spikes.png", DrawSpikes(), 48, 16);
            WriteSprite("Assets/Art/World/Hazard_MovingSpike.png", DrawSpikes(), 40, 16);
            WriteSprite("Assets/Art/World/Hazard_Fire.png", DrawFire(), 40, 28);
            WriteSprite("Assets/Art/World/Checkpoint_Flag.png", DrawFlag(), 24, 40);
            WriteSprite("Assets/Art/Tiles/Tile_Ground.png", DrawTile(), 32, 32);

            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Meadow.png", DrawSky(new Color(0.45f, 0.78f, 1f), new Color(0.75f, 0.92f, 1f)), 256, 128);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Meadow.png", DrawHills(new Color(0.35f, 0.7f, 0.4f), new Color(0.25f, 0.55f, 0.32f)), 256, 64);
            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Cliffs.png", DrawSky(new Color(0.35f, 0.55f, 0.85f), new Color(0.7f, 0.8f, 0.95f)), 256, 128);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Cliffs.png", DrawHills(new Color(0.55f, 0.45f, 0.35f), new Color(0.4f, 0.32f, 0.25f)), 256, 64);
            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Spire.png", DrawSky(new Color(0.25f, 0.28f, 0.55f), new Color(0.55f, 0.45f, 0.75f)), 256, 128);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Spire.png", DrawHills(new Color(0.35f, 0.3f, 0.55f), new Color(0.22f, 0.2f, 0.4f)), 256, 64);
        }

        private static Color[] DrawPlatform(Color face, Color top)
        {
            var c = Clear(64, 16);
            FillRect(c, 64, 16, 0, 0, 64, 16, face);
            FillRect(c, 64, 16, 0, 11, 64, 5, top);
            FillRect(c, 64, 16, 0, 0, 64, 2, face * 0.7f);
            OutlineRect(c, 64, 16, 0, 0, 64, 16, Ink);
            return c;
        }

        private static Color[] DrawBounce()
        {
            var c = Clear(48, 16);
            var orange = new Color(0.95f, 0.55f, 0.18f, 1f);
            FillRect(c, 48, 16, 2, 2, 44, 12, orange);
            FillRect(c, 48, 16, 4, 8, 40, 5, new Color(1f, 0.8f, 0.35f, 1f));
            FillRect(c, 48, 16, 18, 4, 12, 3, Color.white);
            OutlineRect(c, 48, 16, 2, 2, 44, 12, Ink);
            return c;
        }

        private static Color[] DrawExit()
        {
            var c = Clear(24, 40);
            FillRect(c, 24, 40, 4, 2, 16, 36, new Color(0.18f, 0.75f, 0.45f, 1f));
            FillRect(c, 24, 40, 7, 8, 10, 22, new Color(0.08f, 0.35f, 0.25f, 1f));
            FillRect(c, 24, 40, 4, 30, 16, 6, new Color(0.45f, 0.95f, 0.65f, 1f));
            Plot(c, 24, 40, 15, 18, new Color(1f, 0.9f, 0.3f, 1f));
            OutlineRect(c, 24, 40, 4, 2, 16, 36, Ink);
            return c;
        }

        private static Color[] DrawDeathZone()
        {
            var c = Clear(96, 24);
            for (var y = 0; y < 24; y++)
            {
                for (var x = 0; x < 96; x++)
                {
                    var a = 0.35f + (y / 24f) * 0.45f;
                    c[y * 96 + x] = new Color(0.12f, 0.05f, 0.2f, a);
                }
            }

            for (var x = 0; x < 96; x += 8)
            {
                FillTriangle(c, 96, 24, x + 4, 22, x, 10, x + 8, 10, new Color(0.35f, 0.1f, 0.45f, 0.85f));
            }

            return c;
        }

        private static Color[] DrawSpikes()
        {
            var c = Clear(48, 16);
            var gray = new Color(0.7f, 0.72f, 0.78f, 1f);
            for (var i = 0; i < 6; i++)
            {
                var x = 2 + i * 8;
                FillTriangle(c, 48, 16, x + 3, 14, x, 2, x + 6, 2, gray);
                OutlineTriangle(c, 48, 16, x + 3, 14, x, 2, x + 6, 2, Ink);
            }

            return c;
        }

        private static Color[] DrawFire()
        {
            var c = Clear(40, 28);
            FillTriangle(c, 40, 28, 20, 26, 6, 4, 34, 4, new Color(1f, 0.35f, 0.08f, 1f));
            FillTriangle(c, 40, 28, 20, 22, 12, 6, 28, 6, new Color(1f, 0.75f, 0.2f, 1f));
            FillTriangle(c, 40, 28, 20, 16, 16, 8, 24, 8, new Color(1f, 0.95f, 0.55f, 1f));
            OutlineTriangle(c, 40, 28, 20, 26, 6, 4, 34, 4, Ink);
            return c;
        }

        private static Color[] DrawFlag()
        {
            var c = Clear(24, 40);
            FillRect(c, 24, 40, 4, 2, 3, 36, new Color(0.55f, 0.4f, 0.25f, 1f));
            FillRect(c, 24, 40, 7, 24, 14, 10, new Color(0.2f, 0.85f, 0.55f, 1f));
            FillRect(c, 24, 40, 7, 28, 14, 3, new Color(0.85f, 1f, 0.9f, 1f));
            OutlineRect(c, 24, 40, 7, 24, 14, 10, Ink);
            return c;
        }

        private static Color[] DrawTile()
        {
            var c = Clear(32, 32);
            FillRect(c, 32, 32, 0, 0, 32, 32, new Color(0.32f, 0.7f, 0.34f, 1f));
            FillRect(c, 32, 32, 0, 24, 32, 8, new Color(0.55f, 0.88f, 0.45f, 1f));
            FillRect(c, 32, 32, 0, 0, 32, 3, new Color(0.22f, 0.48f, 0.24f, 1f));
            OutlineRect(c, 32, 32, 0, 0, 32, 32, Ink);
            return c;
        }

        private static Color[] DrawSky(Color bottom, Color top)
        {
            var c = Clear(256, 128);
            for (var y = 0; y < 128; y++)
            {
                var t = y / 127f;
                var col = Color.Lerp(bottom, top, t);
                for (var x = 0; x < 256; x++)
                {
                    c[y * 256 + x] = col;
                }
            }

            // Soft clouds
            FillEllipse(c, 256, 128, 60, 90, 28, 10, new Color(1f, 1f, 1f, 0.35f));
            FillEllipse(c, 256, 128, 160, 100, 36, 12, new Color(1f, 1f, 1f, 0.28f));
            FillEllipse(c, 256, 128, 210, 80, 22, 8, new Color(1f, 1f, 1f, 0.22f));
            return c;
        }

        private static Color[] DrawHills(Color near, Color far)
        {
            var c = Clear(256, 64);
            FillEllipse(c, 256, 64, 40, 10, 50, 28, far);
            FillEllipse(c, 256, 64, 140, 8, 60, 30, far);
            FillEllipse(c, 256, 64, 230, 12, 45, 26, far);
            FillEllipse(c, 256, 64, 80, 0, 55, 34, near);
            FillEllipse(c, 256, 64, 190, 0, 60, 32, near);
            return c;
        }

        private static void BuildItemArt()
        {
            WriteSprite("Assets/Art/Items/Coin_Placeholder.png", DrawCoin(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_SpeedBurst.png", DrawSpeedBolt(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_GlowShield.png", DrawShield(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_HeartDrop.png", DrawHeart(), 24, 24);
        }

        private static Color[] DrawCoin()
        {
            var c = Clear(24, 24);
            FillEllipse(c, 24, 24, 12, 12, 9, 9, new Color(1f, 0.82f, 0.2f, 1f));
            FillEllipse(c, 24, 24, 12, 12, 6, 6, new Color(1f, 0.95f, 0.45f, 1f));
            FillEllipse(c, 24, 24, 10, 14, 2, 2, Color.white);
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, Ink);
            return c;
        }

        private static Color[] DrawSpeedBolt()
        {
            var c = Clear(24, 24);
            FillTriangle(c, 24, 24, 14, 22, 4, 12, 12, 12, new Color(1f, 0.75f, 0.15f, 1f));
            FillTriangle(c, 24, 24, 10, 2, 20, 12, 12, 12, new Color(1f, 0.9f, 0.35f, 1f));
            OutlineTriangle(c, 24, 24, 14, 22, 4, 12, 12, 12, Ink);
            return c;
        }

        private static Color[] DrawShield()
        {
            var c = Clear(24, 24);
            FillEllipse(c, 24, 24, 12, 12, 9, 9, new Color(0.35f, 0.85f, 1f, 0.85f));
            FillEllipse(c, 24, 24, 12, 12, 5, 5, new Color(0.85f, 0.98f, 1f, 0.5f));
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, Ink);
            return c;
        }

        private static Color[] DrawHeart()
        {
            var c = Clear(24, 24);
            var pink = new Color(1f, 0.4f, 0.55f, 1f);
            FillEllipse(c, 24, 24, 8, 15, 5, 5, pink);
            FillEllipse(c, 24, 24, 16, 15, 5, 5, pink);
            FillTriangle(c, 24, 24, 12, 4, 3, 14, 21, 14, pink);
            OutlineEllipse(c, 24, 24, 8, 15, 5, 5, Ink);
            OutlineEllipse(c, 24, 24, 16, 15, 5, 5, Ink);
            return c;
        }

        private static void BuildVfxArt()
        {
            WriteSprite("Assets/Art/VFX/FX_Sparkle.png", DrawSparkle(), 16, 16);
            WriteSprite("Assets/Art/VFX/FX_Dust.png", DrawDust(), 16, 16);
            WriteSprite("Assets/Art/VFX/FX_HitRing.png", DrawHitRing(), 24, 24);
        }

        private static Color[] DrawSparkle()
        {
            var c = Clear(16, 16);
            FillRect(c, 16, 16, 7, 2, 2, 12, new Color(1f, 0.95f, 0.55f, 1f));
            FillRect(c, 16, 16, 2, 7, 12, 2, new Color(1f, 0.95f, 0.55f, 1f));
            FillEllipse(c, 16, 16, 8, 8, 2, 2, Color.white);
            return c;
        }

        private static Color[] DrawDust()
        {
            var c = Clear(16, 16);
            FillEllipse(c, 16, 16, 5, 6, 3, 2, new Color(1f, 1f, 1f, 0.45f));
            FillEllipse(c, 16, 16, 11, 8, 3, 2, new Color(1f, 1f, 1f, 0.35f));
            FillEllipse(c, 16, 16, 8, 11, 2, 2, new Color(1f, 1f, 1f, 0.25f));
            return c;
        }

        private static Color[] DrawHitRing()
        {
            var c = Clear(24, 24);
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, new Color(1f, 0.55f, 0.45f, 1f));
            OutlineEllipse(c, 24, 24, 12, 12, 7, 7, new Color(1f, 0.8f, 0.7f, 0.8f));
            return c;
        }

        private static Color[] DrawUiPanel()
        {
            var c = Clear(64, 64);
            FillRect(c, 64, 64, 0, 0, 64, 64, new Color(0.06f, 0.1f, 0.14f, 0.92f));
            OutlineRect(c, 64, 64, 0, 0, 64, 64, new Color(0.25f, 0.55f, 0.4f, 1f));
            OutlineRect(c, 64, 64, 2, 2, 60, 60, new Color(0.18f, 0.35f, 0.28f, 1f));
            return c;
        }

        private static Color[] DrawUiButton()
        {
            var c = Clear(64, 32);
            FillRect(c, 64, 32, 0, 0, 64, 32, new Color(0.18f, 0.48f, 0.34f, 1f));
            FillRect(c, 64, 32, 0, 22, 64, 10, new Color(0.28f, 0.62f, 0.44f, 1f));
            OutlineRect(c, 64, 32, 0, 0, 64, 32, Ink);
            return c;
        }

        private static Color[] DrawUiHudBar()
        {
            var c = Clear(128, 32);
            for (var y = 0; y < 32; y++)
            {
                var a = 0.55f - y / 32f * 0.25f;
                for (var x = 0; x < 128; x++)
                {
                    c[y * 128 + x] = new Color(0.02f, 0.05f, 0.08f, a);
                }
            }

            FillRect(c, 128, 32, 0, 0, 128, 2, new Color(0.25f, 0.55f, 0.4f, 0.8f));
            return c;
        }

        private static void BindPipClip(string clipName, Sprite[] frames, float frameRate, bool loop)
        {
            var path = $"Assets/Animations/Player/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = frameRate;
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (var i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        private static void WireTile()
        {
            var tilePath = "Assets/Art/Tiles/Tile_GroundBasic.asset";
            var spritePath = "Assets/Art/Tiles/Tile_Ground.png";
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                for (var i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            if (tile == null || sprite == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Could not wire Tile_GroundBasic (tile={tile != null}, sprite={sprite != null}).");
                return;
            }

            var so = new SerializedObject(tile);
            so.FindProperty("m_Sprite").objectReferenceValue = sprite;
            so.FindProperty("m_ColliderType").enumValueIndex = (int)Tile.ColliderType.Sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyUiChrome(Sprite panel, Sprite button, Sprite bar)
        {
            ApplyUiToScene("Assets/Scenes/MainMenu.unity", panel, button, bar);
            ApplyUiToScene("Assets/Scenes/Gameplay.unity", panel, button, bar);
            ApplyUiToScene($"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity", panel, button, bar);
            ApplyUiToScene($"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity", panel, button, bar);
            ApplyUiToScene($"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity", panel, button, bar);
        }

        private static void ApplyUiToScene(string scenePath, Sprite panel, Sprite button, Sprite bar)
        {
            if (!File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < images.Length; i++)
            {
                var img = images[i];
                var n = img.gameObject.name;
                if (n.Contains("Button") || n.EndsWith("Btn"))
                {
                    img.sprite = button;
                    img.type = Image.Type.Simple;
                    img.preserveAspect = false;
                }
                else if (n.Contains("Panel") || n.Contains("Overlay") || n == "RootPanel" || n == "SettingsPanel")
                {
                    if (img.color.a > 0.2f)
                    {
                        img.sprite = panel;
                        img.type = Image.Type.Simple;
                    }
                }
                else if (n == "HudBar")
                {
                    img.sprite = bar;
                    img.type = Image.Type.Simple;
                    img.color = Color.white;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void PlaceLevelBackdrops()
        {
            PlaceBackdrop(
                $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
                "Assets/Art/Backgrounds/BG_Sky_Meadow.png",
                "Assets/Art/Backgrounds/BG_Hills_Meadow.png",
                new Color(0.45f, 0.75f, 0.95f));
            PlaceBackdrop(
                $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
                "Assets/Art/Backgrounds/BG_Sky_Cliffs.png",
                "Assets/Art/Backgrounds/BG_Hills_Cliffs.png",
                new Color(0.35f, 0.5f, 0.75f));
            PlaceBackdrop(
                $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity",
                "Assets/Art/Backgrounds/BG_Sky_Spire.png",
                "Assets/Art/Backgrounds/BG_Hills_Spire.png",
                new Color(0.22f, 0.24f, 0.42f));
        }

        private static void PlaceBackdrop(string scenePath, string skyPath, string hillsPath, Color cameraClear)
        {
            if (!File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<BounderTrail.Levels.LevelRoot>();
            if (levelRoot == null)
            {
                return;
            }

            var decorations = levelRoot.transform.Find("Decorations");
            if (decorations == null)
            {
                var go = new GameObject("Decorations");
                go.transform.SetParent(levelRoot.transform, false);
                decorations = go.transform;
            }

            var existing = decorations.Find("LevelBackdrop");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("LevelBackdrop");
            root.transform.SetParent(decorations, false);
            var backdrop = root.AddComponent<LevelBackdrop>();

            var skySprite = AssetDatabase.LoadAssetAtPath<Sprite>(skyPath);
            var hillsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(hillsPath);

            var far = CreateBackdropLayer(root.transform, "Far_Sky", skySprite, new Vector3(0f, 2f, 10f), new Vector3(18f, 10f, 1f), -20);
            var mid = CreateBackdropLayer(root.transform, "Mid_Hills", hillsSprite, new Vector3(0f, -1.5f, 8f), new Vector3(16f, 4f, 1f), -15);

            var so = new SerializedObject(backdrop);
            so.FindProperty("farLayer").objectReferenceValue = far.transform;
            so.FindProperty("midLayer").objectReferenceValue = mid.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            var cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = cameraClear;
                EditorUtility.SetDirty(cam);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static GameObject CreateBackdropLayer(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 localPos,
            Vector3 scale,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = Color.white;
            return go;
        }

        // --- Pixel helpers ---

        private static Color[] Clear(int w, int h)
        {
            var c = new Color[w * h];
            for (var i = 0; i < c.Length; i++)
            {
                c[i] = Color.clear;
            }

            return c;
        }

        private static void Plot(Color[] c, int w, int h, int x, int y, Color col)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            c[y * w + x] = AlphaBlend(c[y * w + x], col);
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            if (src.a >= 0.999f)
            {
                return src;
            }

            var a = src.a + dst.a * (1f - src.a);
            if (a <= 0.001f)
            {
                return Color.clear;
            }

            return new Color(
                (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a,
                (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a,
                (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a,
                a);
        }

        private static void FillRect(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            for (var yy = y; yy < y + rh; yy++)
            {
                for (var xx = x; xx < x + rw; xx++)
                {
                    Plot(c, w, h, xx, yy, col);
                }
            }
        }

        private static void OutlineRect(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            for (var xx = x; xx < x + rw; xx++)
            {
                Plot(c, w, h, xx, y, col);
                Plot(c, w, h, xx, y + rh - 1, col);
            }

            for (var yy = y; yy < y + rh; yy++)
            {
                Plot(c, w, h, x, yy, col);
                Plot(c, w, h, x + rw - 1, yy, col);
            }
        }

        private static void FillEllipse(Color[] c, int w, int h, int cx, int cy, int rx, int ry, Color col)
        {
            var rx2 = rx * rx;
            var ry2 = ry * ry;
            for (var y = -ry; y <= ry; y++)
            {
                for (var x = -rx; x <= rx; x++)
                {
                    if (x * x * ry2 + y * y * rx2 <= rx2 * ry2)
                    {
                        Plot(c, w, h, cx + x, cy + y, col);
                    }
                }
            }
        }

        private static void OutlineEllipse(Color[] c, int w, int h, int cx, int cy, int rx, int ry, Color col)
        {
            var rx2 = rx * rx;
            var ry2 = ry * ry;
            var innerRx = Mathf.Max(1, rx - 1);
            var innerRy = Mathf.Max(1, ry - 1);
            var irx2 = innerRx * innerRx;
            var iry2 = innerRy * innerRy;
            for (var y = -ry; y <= ry; y++)
            {
                for (var x = -rx; x <= rx; x++)
                {
                    var outer = x * x * ry2 + y * y * rx2 <= rx2 * ry2;
                    var inner = x * x * iry2 + y * y * irx2 <= irx2 * iry2;
                    if (outer && !inner)
                    {
                        Plot(c, w, h, cx + x, cy + y, col);
                    }
                }
            }
        }

        private static void FillTriangle(Color[] c, int w, int h, int x0, int y0, int x1, int y1, int x2, int y2, Color col)
        {
            var minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            var maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            var minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            var maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (PointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                    {
                        Plot(c, w, h, x, y, col);
                    }
                }
            }
        }

        private static void OutlineTriangle(Color[] c, int w, int h, int x0, int y0, int x1, int y1, int x2, int y2, Color col)
        {
            DrawLine(c, w, h, x0, y0, x1, y1, col);
            DrawLine(c, w, h, x1, y1, x2, y2, col);
            DrawLine(c, w, h, x2, y2, x0, y0, col);
        }

        private static bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            var d1 = Sign(px, py, x0, y0, x1, y1);
            var d2 = Sign(px, py, x1, y1, x2, y2);
            var d3 = Sign(px, py, x2, y2, x0, y0);
            var hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        private static int Sign(int px, int py, int x0, int y0, int x1, int y1)
        {
            return (px - x1) * (y0 - y1) - (x0 - x1) * (py - y1);
        }

        private static void DrawLine(Color[] c, int w, int h, int x0, int y0, int x1, int y1, Color col)
        {
            var dx = Mathf.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Mathf.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;
            while (true)
            {
                Plot(c, w, h, x0, y0, col);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private static Sprite WriteSprite(string assetPath, Color[] pixels, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            var dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = Ppu;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
