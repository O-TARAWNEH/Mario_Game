// Filename: Phase4PlayerFeelSetup.cs
// Folder: Assets/Editor/
// Purpose: Applies Phase 4 feel tuning to Player_Pip prefab and lightly updates Gameplay.
// Dependencies: BounderTrail.Player.PlayerController, PlayerGroundSensor
//
// Menu: Bounder Trail/Phase 4/Apply Player Feel Tuning
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase4PlayerFeelSetup.ApplyPlayerFeelTuning

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase4PlayerFeelSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string GroundSpritePath = "Assets/Art/World/Ground_Placeholder.png";

        [MenuItem("Bounder Trail/Phase 4/Apply Player Feel Tuning")]
        public static void ApplyPlayerFeelTuning()
        {
            UpdatePlayerPrefab();
            UpdateGameplaySceneForFeelTests();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 4 player feel tuning applied.");
        }

        private static void UpdatePlayerPrefab()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var controller = prefabRoot.GetComponent<PlayerController>();
                var sensor = prefabRoot.GetComponent<PlayerGroundSensor>();
                var groundCheck = prefabRoot.transform.Find("GroundCheck");

                if (controller == null || sensor == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Player_Pip is missing controller/sensor.");
                    return;
                }

                var controllerSo = new SerializedObject(controller);
                Set(controllerSo, "walkSpeed", 6.5f);
                Set(controllerSo, "runSpeed", 9.5f);
                Set(controllerSo, "acceleration", 75f);
                Set(controllerSo, "deceleration", 85f);
                Set(controllerSo, "airAcceleration", 45f);
                Set(controllerSo, "airDeceleration", 40f);
                Set(controllerSo, "airControl", 0.75f);
                Set(controllerSo, "jumpForce", 15f);
                Set(controllerSo, "coyoteTime", 0.1f);
                Set(controllerSo, "jumpBufferTime", 0.12f);
                Set(controllerSo, "jumpCutMultiplier", 0.45f);
                Set(controllerSo, "jumpCutGravityMultiplier", 2.2f);
                Set(controllerSo, "gravity", 3.2f);
                Set(controllerSo, "fallGravityMultiplier", 1.55f);
                Set(controllerSo, "maximumFallSpeed", 22f);
                Set(controllerSo, "projectMoveOnSlope", true);
                Set(controllerSo, "groundedStickForce", 2.5f);
                controllerSo.ApplyModifiedPropertiesWithoutUndo();

                var sensorSo = new SerializedObject(sensor);
                if (groundCheck != null)
                {
                    sensorSo.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck;
                }

                Set(sensorSo, "checkRadius", 0.12f);
                Set(sensorSo, "probeDistance", 0.18f);
                Set(sensorSo, "edgeProbeOffset", 0.28f);
                Set(sensorSo, "maxSlopeAngle", 50f);
                var groundLayers = sensorSo.FindProperty("groundLayers");
                if (groundLayers != null)
                {
                    groundLayers.intValue = LayerMask.GetMask("Ground");
                }

                sensorSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void UpdateGameplaySceneForFeelTests()
        {
            if (!System.IO.File.Exists(GameplayScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var world = GameObject.Find("_World");
            if (world == null)
            {
                var level = GameObject.Find("_Level");
                if (level != null)
                {
                    world = new GameObject("_World");
                    world.transform.SetParent(level.transform, false);
                }
            }

            // Add a mild ramp for slope feel testing if missing.
            if (world != null && GameObject.Find("Platform_Slope") == null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);
                var slope = new GameObject("Platform_Slope");
                slope.transform.SetParent(world.transform, false);
                slope.transform.position = new Vector3(-7.5f, -2.1f, 0f);
                slope.transform.rotation = Quaternion.Euler(0f, 0f, 18f);
                slope.transform.localScale = new Vector3(3.2f, 0.6f, 1f);
                slope.tag = "Ground";
                slope.layer = LayerMask.NameToLayer("Ground");

                var renderer = slope.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 0;

                slope.AddComponent<BoxCollider2D>().size = Vector2.one;
            }

            // Refresh scene player instance from prefab if present.
            var player = GameObject.Find("Player_Pip");
            if (player != null)
            {
                PrefabUtility.RevertObjectOverride(player, InteractionMode.AutomatedAction);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void Set(SerializedObject so, string propertyName, float value)
        {
            var property = so.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void Set(SerializedObject so, string propertyName, bool value)
        {
            var property = so.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }
    }
}
#endif
