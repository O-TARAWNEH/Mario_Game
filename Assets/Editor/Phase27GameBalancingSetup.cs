// Filename: Phase27GameBalancingSetup.cs
// Folder: Assets/Editor/
// Purpose: Applies Phase 27 difficulty tuning, rebuilds balanced layouts, restores art/juice.
// Menu: Bounder Trail/Phase 27/Setup Game Balancing
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase27GameBalancingSetup.SetupGameBalancing

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Enemies;
using BounderTrail.Player;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase27GameBalancingSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string DartlingPath = "Assets/Prefabs/Enemies/Enemy_Dartling.prefab";
        private const string HopmitePath = "Assets/Prefabs/Enemies/Enemy_Hopmite.prefab";
        private const string SpitterPath = "Assets/Prefabs/Enemies/Enemy_Spitter.prefab";

        [MenuItem("Bounder Trail/Phase 27/Setup Game Balancing")]
        public static void SetupGameBalancing()
        {
            TunePlayer();
            TuneDartling();
            TuneHopmite();
            TuneSpitter();

            // Rebuild campaign layouts with Phase 27 placement fixes, then restore visuals/juice.
            Phase24LevelDesignSetup.SetupLevelDesign();
            Phase25ArtPolishSetup.SetupArtAndVisualPolish();
            Phase26VisualEffectsSetup.SetupVisualEffectsAndJuice();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 27 balancing complete " +
                "(player/enemy tunes + fairer checkpoints/placements; bosses N/A).");
        }

        private static void TunePlayer()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var controller = root.GetComponent<PlayerController>();
                if (controller != null)
                {
                    var so = new SerializedObject(controller);
                    // Slightly more forgiving edge jumps without changing jump height/speed.
                    so.FindProperty("coyoteTime").floatValue = 0.12f;
                    so.FindProperty("jumpBufferTime").floatValue = 0.14f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var powerUps = root.GetComponent<PlayerPowerUps>();
                if (powerUps != null)
                {
                    var so = new SerializedObject(powerUps);
                    // Covers L3 finale after shield pickup.
                    so.FindProperty("glowShieldDuration").floatValue = 6.5f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TuneDartling()
        {
            var root = PrefabUtility.LoadPrefabContents(DartlingPath);
            try
            {
                var mover = root.GetComponent<EnemyMover>();
                if (mover != null)
                {
                    var so = new SerializedObject(mover);
                    so.FindProperty("moveSpeed").floatValue = 3.6f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, DartlingPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TuneHopmite()
        {
            var root = PrefabUtility.LoadPrefabContents(HopmitePath);
            try
            {
                var jumper = root.GetComponent<EnemyJumper>();
                if (jumper != null)
                {
                    var so = new SerializedObject(jumper);
                    so.FindProperty("jumpInterval").floatValue = 1.45f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, HopmitePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TuneSpitter()
        {
            var root = PrefabUtility.LoadPrefabContents(SpitterPath);
            try
            {
                var shooter = root.GetComponent<EnemyShooter>();
                if (shooter != null)
                {
                    var so = new SerializedObject(shooter);
                    so.FindProperty("fireInterval").floatValue = 1.85f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var sensor = root.GetComponent<EnemySensor>();
                if (sensor != null)
                {
                    var so = new SerializedObject(sensor);
                    so.FindProperty("radius").floatValue = 5.5f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, SpitterPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
