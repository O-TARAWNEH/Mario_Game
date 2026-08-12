// Filename: Phase10EnemyTypesSetup.cs
// Folder: Assets/Editor/
// Purpose: Builds all standard enemy type prefabs, animations, and samples (Phase 10).
// Menu: Bounder Trail/Phase 10/Setup Enemy Types
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase10EnemyTypesSetup.SetupEnemyTypes

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase10EnemyTypesSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/Enemies";
        private const string ArtFolder = "Assets/Art/Enemies";
        private const string AnimFolder = "Assets/Animations/Enemies";
        private const string ControllerPath = AnimFolder + "/Anim_Enemy.controller";
        private const string ProjectilePrefabPath = PrefabFolder + "/Enemy_Projectile.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 10/Setup Enemy Types")]
        public static void SetupEnemyTypes()
        {
            EnsureFolder("Assets/Prefabs", "Enemies");
            EnsureFolder("Assets/Art", "Enemies");
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets/Animations", "Enemies");
            EnsureTag("Enemy");
            EnsureLayer("Enemy");

            var controller = BuildEnemyAnimatorController();
            var projectile = CreateProjectilePrefab();

            var crawlbug = CreateWalker(
                "Enemy_Crawlbug",
                new Color(0.85f, 0.25f, 0.25f),
                moveSpeed: 2f,
                canPatrol: true,
                controller);

            var dartling = CreateWalker(
                "Enemy_Dartling",
                new Color(1f, 0.45f, 0.15f),
                moveSpeed: 4.5f,
                canPatrol: true,
                controller);

            var hopmite = CreateHopper(controller);
            var skimmer = CreateFlyer(controller);
            var spikewatch = CreateStationary(controller);
            var spitter = CreateShooter(controller, projectile);

            PlaceSamples(crawlbug, dartling, hopmite, skimmer, spikewatch, spitter);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 10 enemy types ready.");
        }

        private static GameObject CreateWalker(
            string name,
            Color color,
            float moveSpeed,
            bool canPatrol,
            RuntimeAnimatorController controller)
        {
            var sprite = CreateColorSprite($"{ArtFolder}/{name}_0.png", color, 28, 22);
            var go = CreateEnemyBase(name, sprite, controller, gravity: 3f, boxSize: new Vector2(0.85f, 0.7f));
            ConfigureMover(go, moveSpeed, groundChecks: true);
            ConfigureBrain(go, canPatrol: canPatrol, canChase: false, canAttack: false, turnLedges: true, turnWalls: true, initial: EnemyStateId.Patrol);
            ConfigureContact(go, canBeStomped: true, contactDamage: 1);
            AddAnimatorBridge(go);
            return SavePrefab(go, $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateHopper(RuntimeAnimatorController controller)
        {
            const string name = "Enemy_Hopmite";
            var sprite = CreateColorSprite($"{ArtFolder}/{name}_0.png", new Color(0.55f, 0.9f, 0.3f), 26, 24);
            var go = CreateEnemyBase(name, sprite, controller, gravity: 3f, boxSize: new Vector2(0.8f, 0.75f));
            ConfigureMover(go, 1.6f, groundChecks: true);
            ConfigureBrain(go, canPatrol: true, canChase: false, canAttack: false, turnLedges: true, turnWalls: true, initial: EnemyStateId.Patrol);
            ConfigureContact(go, canBeStomped: true, contactDamage: 1);

            var groundCheck = go.transform.Find("GroundCheck");
            var jumper = go.AddComponent<EnemyJumper>();
            var so = new SerializedObject(jumper);
            so.FindProperty("rigidBody").objectReferenceValue = go.GetComponent<Rigidbody2D>();
            so.FindProperty("brain").objectReferenceValue = go.GetComponent<EnemyBrain>();
            so.FindProperty("jumpForce").floatValue = 9f;
            so.FindProperty("jumpInterval").floatValue = 1.1f;
            so.FindProperty("groundLayers").intValue = LayerMask.GetMask("Ground");
            so.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            so.ApplyModifiedPropertiesWithoutUndo();

            AddAnimatorBridge(go);
            return SavePrefab(go, $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateFlyer(RuntimeAnimatorController controller)
        {
            const string name = "Enemy_Skimmer";
            var sprite = CreateColorSprite($"{ArtFolder}/{name}_0.png", new Color(0.35f, 0.7f, 1f), 30, 18);
            var go = CreateEnemyBase(name, sprite, controller, gravity: 0f, boxSize: new Vector2(0.9f, 0.55f));
            ConfigureMover(go, 2.2f, groundChecks: false);
            ConfigureBrain(go, canPatrol: true, canChase: false, canAttack: false, turnLedges: false, turnWalls: false, initial: EnemyStateId.Patrol);
            ConfigureContact(go, canBeStomped: true, contactDamage: 1);

            var flyer = go.AddComponent<EnemyFlyer>();
            var so = new SerializedObject(flyer);
            so.FindProperty("rigidBody").objectReferenceValue = go.GetComponent<Rigidbody2D>();
            so.FindProperty("mover").objectReferenceValue = go.GetComponent<EnemyMover>();
            so.FindProperty("hoverGravityScale").floatValue = 0f;
            so.FindProperty("enableBob").boolValue = true;
            so.FindProperty("autoFlip").boolValue = true;
            so.FindProperty("flipInterval").floatValue = 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            AddAnimatorBridge(go);
            return SavePrefab(go, $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateStationary(RuntimeAnimatorController controller)
        {
            const string name = "Enemy_Spikewatch";
            var sprite = CreateColorSprite($"{ArtFolder}/{name}_0.png", new Color(0.7f, 0.7f, 0.75f), 28, 28);
            var go = CreateEnemyBase(name, sprite, controller, gravity: 3f, boxSize: new Vector2(0.9f, 0.9f));
            ConfigureMover(go, 0f, groundChecks: false);
            ConfigureBrain(go, canPatrol: false, canChase: false, canAttack: false, turnLedges: false, turnWalls: false, initial: EnemyStateId.Idle);
            ConfigureContact(go, canBeStomped: false, contactDamage: 1);
            AddAnimatorBridge(go);
            return SavePrefab(go, $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateShooter(RuntimeAnimatorController controller, GameObject projectilePrefab)
        {
            const string name = "Enemy_Spitter";
            var sprite = CreateColorSprite($"{ArtFolder}/{name}_0.png", new Color(0.75f, 0.35f, 0.85f), 28, 26);
            var go = CreateEnemyBase(name, sprite, controller, gravity: 3f, boxSize: new Vector2(0.85f, 0.8f));
            ConfigureMover(go, 0f, groundChecks: false);
            ConfigureBrain(go, canPatrol: false, canChase: false, canAttack: true, turnLedges: false, turnWalls: false, initial: EnemyStateId.Idle);
            ConfigureContact(go, canBeStomped: true, contactDamage: 1);

            var sensor = go.GetComponent<EnemySensor>();
            var sensorSo = new SerializedObject(sensor);
            sensorSo.FindProperty("radius").floatValue = 7f;
            sensorSo.FindProperty("targetLayers").intValue = LayerMask.GetMask("Player");
            sensorSo.ApplyModifiedPropertiesWithoutUndo();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform, false);
            firePoint.transform.localPosition = new Vector3(0.4f, 0.1f, 0f);

            var shooter = go.AddComponent<EnemyShooter>();
            var so = new SerializedObject(shooter);
            so.FindProperty("sensor").objectReferenceValue = sensor;
            so.FindProperty("brain").objectReferenceValue = go.GetComponent<EnemyBrain>();
            so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab.GetComponent<EnemyProjectile>();
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.FindProperty("fireInterval").floatValue = 1.4f;
            so.FindProperty("projectileSpeed").floatValue = 7f;
            so.ApplyModifiedPropertiesWithoutUndo();

            AddAnimatorBridge(go);
            return SavePrefab(go, $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateProjectilePrefab()
        {
            var sprite = CreateColorSprite($"{ArtFolder}/Enemy_Projectile_0.png", new Color(1f, 0.9f, 0.2f), 12, 12);
            var go = new GameObject("Enemy_Projectile");
            go.layer = LayerMask.NameToLayer("Enemy");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 12;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
            var projectile = go.AddComponent<EnemyProjectile>();
            var so = new SerializedObject(projectile);
            so.FindProperty("speed").floatValue = 7f;
            so.FindProperty("damage").intValue = 1;
            so.FindProperty("destroyOnLayers").intValue = LayerMask.GetMask("Ground");
            so.ApplyModifiedPropertiesWithoutUndo();
            return SavePrefab(go, ProjectilePrefabPath);
        }

        private static GameObject CreateEnemyBase(
            string name,
            Sprite sprite,
            RuntimeAnimatorController controller,
            float gravity,
            Vector2 boxSize)
        {
            var go = new GameObject(name);
            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Enemy");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 8;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.gravityScale = gravity;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = boxSize;

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform, false);
            groundCheck.transform.localPosition = new Vector3(0.35f, -boxSize.y * 0.5f, 0f);

            var wallCheck = new GameObject("WallCheck");
            wallCheck.transform.SetParent(go.transform, false);
            wallCheck.transform.localPosition = new Vector3(boxSize.x * 0.55f, 0f, 0f);

            go.AddComponent<EnemyHealth>();
            go.AddComponent<EnemyMover>();
            go.AddComponent<EnemySensor>();
            go.AddComponent<EnemyBrain>();
            go.AddComponent<EnemyContact>();

            var animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            return go;
        }

        private static void ConfigureMover(GameObject go, float speed, bool groundChecks)
        {
            var mover = go.GetComponent<EnemyMover>();
            var so = new SerializedObject(mover);
            so.FindProperty("rigidBody").objectReferenceValue = go.GetComponent<Rigidbody2D>();
            so.FindProperty("spriteRenderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            so.FindProperty("moveSpeed").floatValue = speed;
            so.FindProperty("groundCheck").objectReferenceValue = groundChecks ? go.transform.Find("GroundCheck") : null;
            so.FindProperty("wallCheck").objectReferenceValue = groundChecks ? go.transform.Find("WallCheck") : null;
            so.FindProperty("groundLayers").intValue = LayerMask.GetMask("Ground");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBrain(
            GameObject go,
            bool canPatrol,
            bool canChase,
            bool canAttack,
            bool turnLedges,
            bool turnWalls,
            EnemyStateId initial)
        {
            var brain = go.GetComponent<EnemyBrain>();
            var so = new SerializedObject(brain);
            so.FindProperty("health").objectReferenceValue = go.GetComponent<EnemyHealth>();
            so.FindProperty("mover").objectReferenceValue = go.GetComponent<EnemyMover>();
            so.FindProperty("sensor").objectReferenceValue = go.GetComponent<EnemySensor>();
            so.FindProperty("canPatrol").boolValue = canPatrol;
            so.FindProperty("canChase").boolValue = canChase;
            so.FindProperty("canAttack").boolValue = canAttack;
            so.FindProperty("turnAtLedges").boolValue = turnLedges;
            so.FindProperty("turnAtWalls").boolValue = turnWalls;
            so.FindProperty("initialState").enumValueIndex = (int)initial;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureContact(GameObject go, bool canBeStomped, int contactDamage)
        {
            var contact = go.GetComponent<EnemyContact>();
            var so = new SerializedObject(contact);
            so.FindProperty("health").objectReferenceValue = go.GetComponent<EnemyHealth>();
            so.FindProperty("canBeStomped").boolValue = canBeStomped;
            so.FindProperty("dealContactDamage").boolValue = true;
            so.FindProperty("contactDamageToPlayer").intValue = contactDamage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddAnimatorBridge(GameObject go)
        {
            var bridge = go.AddComponent<EnemyAnimator>();
            var so = new SerializedObject(bridge);
            so.FindProperty("animator").objectReferenceValue = go.GetComponent<Animator>();
            so.FindProperty("brain").objectReferenceValue = go.GetComponent<EnemyBrain>();
            so.FindProperty("health").objectReferenceValue = go.GetComponent<EnemyHealth>();
            so.FindProperty("rigidBody").objectReferenceValue = go.GetComponent<Rigidbody2D>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static AnimatorController BuildEnemyAnimatorController()
        {
            if (File.Exists(ControllerPath))
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            // Minimal clips (single-frame placeholders) so states exist for EnemyAnimator.
            var idle = CreateEmptyClip("Anim_Enemy_Idle", loop: true);
            var patrol = CreateEmptyClip("Anim_Enemy_Patrol", loop: true);
            var chase = CreateEmptyClip("Anim_Enemy_Chase", loop: true);
            var attack = CreateEmptyClip("Anim_Enemy_Attack", loop: false);
            var hurt = CreateEmptyClip("Anim_Enemy_Hurt", loop: false);
            var dead = CreateEmptyClip("Anim_Enemy_Dead", loop: false);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("State", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;
            foreach (var existing in root.states)
            {
                root.RemoveState(existing.state);
            }

            var idleState = AddState(root, "Idle", idle, new Vector3(200, 0, 0));
            var patrolState = AddState(root, "Patrol", patrol, new Vector3(200, 70, 0));
            var chaseState = AddState(root, "Chase", chase, new Vector3(200, 140, 0));
            var attackState = AddState(root, "Attack", attack, new Vector3(450, 70, 0));
            var hurtState = AddState(root, "Hurt", hurt, new Vector3(450, 140, 0));
            var deadState = AddState(root, "Dead", dead, new Vector3(700, 70, 0));
            root.defaultState = idleState;

            AddIntTransition(root, idleState, 0);
            AddIntTransition(root, patrolState, 1);
            AddIntTransition(root, chaseState, 2);
            AddIntTransition(root, attackState, 3);
            AddIntTransition(root, hurtState, 4);

            var anyDead = root.AddAnyStateTransition(deadState);
            anyDead.hasExitTime = false;
            anyDead.duration = 0.05f;
            anyDead.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

            var anyHurt = root.AddAnyStateTransition(hurtState);
            anyHurt.hasExitTime = false;
            anyHurt.duration = 0.05f;
            anyHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");

            var anyAttack = root.AddAnyStateTransition(attackState);
            anyAttack.hasExitTime = false;
            anyAttack.duration = 0.05f;
            anyAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine root, string name, Motion motion, Vector3 pos)
        {
            var state = root.AddState(name, pos);
            state.motion = motion;
            return state;
        }

        private static void AddIntTransition(AnimatorStateMachine root, AnimatorState state, int value)
        {
            var t = root.AddAnyStateTransition(state);
            t.hasExitTime = false;
            t.duration = 0.05f;
            t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.Equals, value, "State");
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");
        }

        private static AnimationClip CreateEmptyClip(string clipName, bool loop)
        {
            var path = $"{AnimFolder}/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = 8f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void PlaceSamples(
            GameObject crawlbug,
            GameObject dartling,
            GameObject hopmite,
            GameObject skimmer,
            GameObject spikewatch,
            GameObject spitter)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var enemiesRoot = GameObject.Find("Enemies");
            if (enemiesRoot == null)
            {
                var levelRoot = GameObject.Find("LevelRoot");
                enemiesRoot = new GameObject("Enemies");
                if (levelRoot != null)
                {
                    enemiesRoot.transform.SetParent(levelRoot.transform, false);
                }
            }

            // Refresh crawlbugs to latest prefab if present.
            ReplaceOrPlace(enemiesRoot.transform, "Enemy_Crawlbug_A", crawlbug, new Vector3(1.2f, -1.9f, 0f));
            ReplaceOrPlace(enemiesRoot.transform, "Enemy_Crawlbug_B", crawlbug, new Vector3(6.2f, -1.9f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Dartling_A", dartling, new Vector3(-2.5f, -1.9f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Hopmite_A", hopmite, new Vector3(3.8f, -1.9f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Skimmer_A", skimmer, new Vector3(0.5f, 2.8f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Spikewatch_A", spikewatch, new Vector3(4.8f, -2.2f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Spitter_A", spitter, new Vector3(8.0f, 2.8f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void ReplaceOrPlace(Transform parent, string name, GameObject prefab, Vector3 position)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            PlaceIfMissing(parent, name, prefab, position);
        }

        private static void PlaceIfMissing(Transform parent, string name, GameObject prefab, Vector3 position)
        {
            if (parent.Find(name) != null)
            {
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Sprite CreateColorSprite(string assetPath, Color color, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
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

        private static void EnsureTag(string tag)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            var tags = so.FindProperty("tags");
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureLayer(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            var layers = so.FindProperty("layers");
            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            for (var i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }
        }
    }
}
#endif
