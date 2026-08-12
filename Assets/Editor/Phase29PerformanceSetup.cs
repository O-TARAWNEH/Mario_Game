// Filename: Phase29PerformanceSetup.cs
// Folder: Assets/Editor/
// Purpose: Applies and validates Phase 29 performance optimizations (no gameplay changes).
// Menu: Bounder Trail/Phase 29/Apply Performance Optimizations
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase29PerformanceSetup.ApplyPerformanceOptimizations

#if UNITY_EDITOR
using BounderTrail.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase29PerformanceSetup
    {
        private static readonly string[] RequiredScripts =
        {
            "Assets/Scripts/UI/GameplayHud.cs",
            "Assets/Scripts/Vfx/SimpleBurstVfx.cs",
            "Assets/Scripts/Vfx/GameplayVisualJuice.cs",
            "Assets/Scripts/Camera/CameraShake2D.cs",
            "Assets/Scripts/Player/PlayerHealth.cs",
            "Assets/Scripts/Enemies/EnemyHealth.cs",
            "Assets/Scripts/Player/PlayerDeath.cs"
        };

        [MenuItem("Bounder Trail/Phase 29/Apply Performance Optimizations")]
        public static void ApplyPerformanceOptimizations()
        {
            var textureFixes = ClampArtTextureImportSizes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = ValidateInternal(logPass: false);
            if (issues == 0)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 29 applied — " +
                    $"clamped {textureFixes} art texture max sizes; validation passed.");
            }
            else
            {
                Debug.LogError(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 29 applied textures ({textureFixes}) " +
                    $"but validation reported {issues} issue(s).");
            }
        }

        [MenuItem("Bounder Trail/Phase 29/Validate Performance Optimizations")]
        public static void ValidatePerformanceOptimizations()
        {
            ValidateInternal(logPass: true);
        }

        private static int ValidateInternal(bool logPass)
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 29 — validating performance deliverables.");

            for (var i = 0; i < RequiredScripts.Length; i++)
            {
                var path = RequiredScripts[i];
                if (!File.Exists(path) && AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing: {path}");
                    issues++;
                }
            }

            issues += AssertContains("Assets/Scripts/UI/GameplayHud.cs", "_powerUpTickActive");
            issues += AssertContains("Assets/Scripts/Vfx/SimpleBurstVfx.cs", "MaxPoolSize");
            issues += AssertContains("Assets/Scripts/Vfx/GameplayVisualJuice.cs", "SubscribeLevelLoader");
            issues += AssertContains("Assets/Scripts/Camera/CameraShake2D.cs", "enabled = false");
            issues += AssertContains("Assets/Scripts/Player/PlayerHealth.cs", "enabled = false");
            issues += AssertContains("Assets/Scripts/Enemies/EnemyHealth.cs", "enabled = false");
            issues += AssertContains("Assets/Scripts/Player/PlayerDeath.cs", "DEVELOPMENT_BUILD");

            // Spot-check art import caps stayed reasonable after apply.
            issues += AssertTextureMaxAtMost("Assets/Art/Backgrounds/BG_Sky_Meadow.png", 256);
            issues += AssertTextureMaxAtMost("Assets/Art/Player/Pip_Idle_0.png", 128);
            issues += AssertTextureMaxAtMost("Assets/Art/VFX/FX_Sparkle.png", 64);

            if (issues == 0 && logPass)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 29 validation passed — performance opts present.");
            }
            else if (issues > 0)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 29 validation failed ({issues} issue(s)).");
            }

            return issues;
        }

        private static int ClampArtTextureImportSizes()
        {
            var changed = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/Art/"))
                {
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                var targetMax = RecommendMaxSize(path, importer);
                if (importer.maxTextureSize <= targetMax)
                {
                    // Still normalize platform overrides that may be higher.
                    if (!NeedsPlatformClamp(importer, targetMax))
                    {
                        continue;
                    }
                }

                importer.maxTextureSize = targetMax;
                ClampPlatform(importer, "DefaultTexturePlatform", targetMax);
                ClampPlatform(importer, "Standalone", targetMax);
                ClampPlatform(importer, "WebGL", targetMax);
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                changed++;
            }

            return changed;
        }

        private static int RecommendMaxSize(string path, TextureImporter importer)
        {
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            var largest = Mathf.Max(width, height);
            if (largest <= 0)
            {
                largest = path.Contains("/Backgrounds/") ? 256 : 64;
            }

            // Next power-of-two ceiling, then clamp by category so tiny sprites don't keep a 2048 budget.
            var pot = Mathf.NextPowerOfTwo(largest);
            if (path.Contains("/Backgrounds/"))
            {
                return Mathf.Clamp(pot, 64, 256);
            }

            if (path.Contains("/UI/"))
            {
                return Mathf.Clamp(pot, 32, 256);
            }

            if (path.Contains("/VFX/"))
            {
                return Mathf.Clamp(pot, 16, 64);
            }

            return Mathf.Clamp(pot, 16, 128);
        }

        private static bool NeedsPlatformClamp(TextureImporter importer, int targetMax)
        {
            return importer.maxTextureSize > targetMax
                   || PlatformExceeds(importer, "Standalone", targetMax)
                   || PlatformExceeds(importer, "WebGL", targetMax);
        }

        private static bool PlatformExceeds(TextureImporter importer, string platform, int targetMax)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            return settings.overridden && settings.maxTextureSize > targetMax;
        }

        private static void ClampPlatform(TextureImporter importer, string platform, int targetMax)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            if (platform == "DefaultTexturePlatform")
            {
                settings.name = platform;
                settings.maxTextureSize = targetMax;
                settings.overridden = false;
                importer.SetPlatformTextureSettings(settings);
                return;
            }

            if (!settings.overridden)
            {
                return;
            }

            if (settings.maxTextureSize > targetMax)
            {
                settings.maxTextureSize = targetMax;
                importer.SetPlatformTextureSettings(settings);
            }
        }

        private static int AssertContains(string path, string marker)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file for assert: {path}");
                return 1;
            }

            var text = File.ReadAllText(path);
            if (text.Contains(marker))
            {
                return 0;
            }

            Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected marker '{marker}' in {path}");
            return 1;
        }

        private static int AssertTextureMaxAtMost(string path, int maxAllowed)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing texture: {path}");
                return 1;
            }

            if (importer.maxTextureSize <= maxAllowed)
            {
                return 0;
            }

            Debug.LogError(
                $"{GameLog.ProjectPrefix}[Setup] Texture max size {importer.maxTextureSize} > {maxAllowed} for {path}. " +
                "Run Apply Performance Optimizations.");
            return 1;
        }
    }
}
#endif
