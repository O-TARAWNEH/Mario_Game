// Filename: Phase11CombatInteractionsSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires player health/hurt feedback and confirms combat interaction defaults (Phase 11).
// Menu: Bounder Trail/Phase 11/Setup Combat Interactions
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase11CombatInteractionsSetup.SetupCombatInteractions

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Enemies;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase11CombatInteractionsSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 11/Setup Combat Interactions")]
        public static void SetupCombatInteractions()
        {
            UpdatePlayerPrefab();
            UpdateGameplayScenePlayer();
            TuneEnemyPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 11 combat interactions ready.");
        }

        private static void UpdatePlayerPrefab()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                WirePlayerCombat(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void UpdateGameplayScenePlayer()
        {
            if (!System.IO.File.Exists(GameplayScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player_Pip");
            if (player != null)
            {
                WirePlayerCombat(player);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, GameplayScenePath);
            }
        }

        private static void WirePlayerCombat(GameObject player)
        {
            var controller = player.GetComponent<PlayerController>();
            var death = player.GetComponent<PlayerDeath>();
            var body = player.GetComponent<Rigidbody2D>();
            var sprite = player.GetComponent<SpriteRenderer>();

            if (death == null)
            {
                death = player.AddComponent<PlayerDeath>();
            }

            var health = player.GetComponent<PlayerHealth>();
            if (health == null)
            {
                health = player.AddComponent<PlayerHealth>();
            }

            var feedback = player.GetComponent<PlayerHurtFeedback>();
            if (feedback == null)
            {
                feedback = player.AddComponent<PlayerHurtFeedback>();
            }

            var deathSo = new SerializedObject(death);
            deathSo.FindProperty("playerController").objectReferenceValue = controller;
            deathSo.FindProperty("rigidBody").objectReferenceValue = body;
            deathSo.FindProperty("triggerGameOverOnDeath").boolValue = true;
            deathSo.FindProperty("gameOverDelay").floatValue = 0.85f;
            deathSo.ApplyModifiedPropertiesWithoutUndo();

            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("playerController").objectReferenceValue = controller;
            healthSo.FindProperty("playerDeath").objectReferenceValue = death;
            healthSo.FindProperty("rigidBody").objectReferenceValue = body;
            healthSo.FindProperty("maxHealth").intValue = 3;
            healthSo.FindProperty("invulnerabilityDuration").floatValue = 1.25f;
            healthSo.FindProperty("knockbackForce").vector2Value = new Vector2(6.5f, 8f);
            healthSo.FindProperty("controlLockDuration").floatValue = 0.2f;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            var feedbackSo = new SerializedObject(feedback);
            feedbackSo.FindProperty("playerHealth").objectReferenceValue = health;
            feedbackSo.FindProperty("spriteRenderer").objectReferenceValue = sprite;
            feedbackSo.FindProperty("flashInterval").floatValue = 0.08f;
            feedbackSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TuneEnemyPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var health = root.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        var healthSo = new SerializedObject(health);
                        if (healthSo.FindProperty("invulnerabilityDuration") != null)
                        {
                            healthSo.FindProperty("invulnerabilityDuration").floatValue = 0.2f;
                        }

                        if (healthSo.FindProperty("hurtDuration") != null)
                        {
                            healthSo.FindProperty("hurtDuration").floatValue = 0.2f;
                        }

                        if (healthSo.FindProperty("applyKnockbackOnHit") != null)
                        {
                            healthSo.FindProperty("applyKnockbackOnHit").boolValue = true;
                        }

                        if (healthSo.FindProperty("knockbackForce") != null)
                        {
                            healthSo.FindProperty("knockbackForce").vector2Value = new Vector2(2.5f, 3.5f);
                        }

                        healthSo.ApplyModifiedPropertiesWithoutUndo();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
    }
}
#endif
