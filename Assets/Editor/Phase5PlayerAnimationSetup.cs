// Filename: Phase5PlayerAnimationSetup.cs
// Folder: Assets/Editor/
// Purpose: Builds placeholder player animation clips, AnimatorController, and wires Player_Pip (Phase 5).
// Dependencies: BounderTrail.Player.*
//
// Menu: Bounder Trail/Phase 5/Setup Player Animation
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase5PlayerAnimationSetup.SetupPlayerAnimation

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase5PlayerAnimationSetup
    {
        private const string AnimFolder = "Assets/Animations/Player";
        private const string ArtFolder = "Assets/Art/Player";
        private const string ControllerPath = AnimFolder + "/Anim_Pip.controller";
        private const string PrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";

        [MenuItem("Bounder Trail/Phase 5/Setup Player Animation")]
        public static void SetupPlayerAnimation()
        {
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets/Animations", "Player");
            EnsureFolder("Assets/Art", "Player");

            var idle = CreateSpriteSet("Pip_Idle", new Color(0.25f, 0.85f, 1f), bob: true, frames: 2);
            var walk = CreateSpriteSet("Pip_Walk", new Color(0.2f, 0.75f, 0.95f), bob: true, frames: 4, lean: true);
            var run = CreateSpriteSet("Pip_Run", new Color(0.15f, 0.7f, 1f), bob: true, frames: 4, lean: true, stretchX: true);
            var jump = CreateSpriteSet("Pip_Jump", new Color(0.35f, 0.9f, 1f), bob: false, frames: 1, taller: true);
            var fall = CreateSpriteSet("Pip_Fall", new Color(0.2f, 0.65f, 0.9f), bob: false, frames: 1, wider: true);
            var land = CreateSpriteSet("Pip_Land", new Color(0.3f, 0.8f, 0.95f), bob: false, frames: 2, squashed: true);
            var death = CreateSpriteSet("Pip_Death", new Color(0.55f, 0.55f, 0.7f), bob: false, frames: 3, fallen: true);

            var idleClip = CreateSpriteClip("Anim_Pip_Idle", idle, 4f, loop: true);
            var walkClip = CreateSpriteClip("Anim_Pip_Walk", walk, 10f, loop: true);
            var runClip = CreateSpriteClip("Anim_Pip_Run", run, 14f, loop: true);
            var jumpClip = CreateSpriteClip("Anim_Pip_Jump", jump, 8f, loop: true);
            var fallClip = CreateSpriteClip("Anim_Pip_Fall", fall, 8f, loop: true);
            var landClip = CreateSpriteClip("Anim_Pip_Land", land, 12f, loop: false);
            var deathClip = CreateSpriteClip("Anim_Pip_Death", death, 8f, loop: false);

            var controller = BuildAnimatorController(
                idleClip, walkClip, runClip, jumpClip, fallClip, landClip, deathClip);

            WirePlayerPrefab(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 5 player animation ready.");
        }

        private static AnimatorController BuildAnimatorController(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip jump,
            AnimationClip fall,
            AnimationClip land,
            AnimationClip death)
        {
            if (File.Exists(ControllerPath))
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsFalling", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Land", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;
            // Clear default state created by CreateAnimatorControllerAtPath.
            var defaultStates = root.states;
            foreach (var s in defaultStates)
            {
                root.RemoveState(s.state);
            }

            var idleState = root.AddState("Idle", new Vector3(250, 0, 0));
            idleState.motion = idle;
            root.defaultState = idleState;

            var walkState = root.AddState("Walk", new Vector3(250, 80, 0));
            walkState.motion = walk;

            var runState = root.AddState("Run", new Vector3(250, 160, 0));
            runState.motion = run;

            var jumpState = root.AddState("Jump", new Vector3(520, 0, 0));
            jumpState.motion = jump;

            var fallState = root.AddState("Fall", new Vector3(520, 80, 0));
            fallState.motion = fall;

            var landState = root.AddState("Land", new Vector3(520, 160, 0));
            landState.motion = land;

            var deathState = root.AddState("Death", new Vector3(780, 80, 0));
            deathState.motion = death;

            // Ground locomotion.
            var idleToWalk = idleState.AddTransition(walkState);
            Configure(idleToWalk);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");

            var walkToIdle = walkState.AddTransition(idleState);
            Configure(walkToIdle);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            walkToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            var walkToRun = walkState.AddTransition(runState);
            Configure(walkToRun);
            walkToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            walkToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            var idleToRun = idleState.AddTransition(runState);
            Configure(idleToRun);
            idleToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            idleToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            var runToWalk = runState.AddTransition(walkState);
            Configure(runToWalk);
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");
            runToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            runToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            var runToIdle = runState.AddTransition(idleState);
            Configure(runToIdle);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            runToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            // Airborne.
            AddAirTransition(idleState, jumpState, "IsJumping");
            AddAirTransition(walkState, jumpState, "IsJumping");
            AddAirTransition(runState, jumpState, "IsJumping");
            AddAirTransition(landState, jumpState, "IsJumping");

            AddAirTransition(idleState, fallState, "IsFalling");
            AddAirTransition(walkState, fallState, "IsFalling");
            AddAirTransition(runState, fallState, "IsFalling");
            AddAirTransition(jumpState, fallState, "IsFalling");
            AddAirTransition(landState, fallState, "IsFalling");

            var fallToJump = fallState.AddTransition(jumpState);
            Configure(fallToJump);
            fallToJump.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");

            // Landing.
            var landFromFall = fallState.AddTransition(landState);
            Configure(landFromFall);
            landFromFall.AddCondition(AnimatorConditionMode.If, 0f, "Land");

            var landFromJump = jumpState.AddTransition(landState);
            Configure(landFromJump);
            landFromJump.AddCondition(AnimatorConditionMode.If, 0f, "Land");

            var landToIdle = landState.AddTransition(idleState);
            landToIdle.hasExitTime = true;
            landToIdle.exitTime = 0.9f;
            landToIdle.duration = 0.05f;
            landToIdle.hasFixedDuration = true;

            var landToWalk = landState.AddTransition(walkState);
            Configure(landToWalk);
            landToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            landToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            // Death from any state.
            var anyToDeath = root.AddAnyStateTransition(deathState);
            Configure(anyToDeath);
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void Configure(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.hasFixedDuration = true;
            transition.canTransitionToSelf = false;
        }

        private static void AddAirTransition(AnimatorState from, AnimatorState to, string boolParameter)
        {
            var transition = from.AddTransition(to);
            Configure(transition);
            transition.AddCondition(AnimatorConditionMode.If, 0f, boolParameter);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");
        }

        private static void WirePlayerPrefab(RuntimeAnimatorController controller)
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var animator = root.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = root.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var death = root.GetComponent<PlayerDeath>();
                if (death == null)
                {
                    death = root.AddComponent<PlayerDeath>();
                }

                var playerAnimator = root.GetComponent<PlayerAnimator>();
                if (playerAnimator == null)
                {
                    playerAnimator = root.AddComponent<PlayerAnimator>();
                }

                var controllerComp = root.GetComponent<PlayerController>();
                var sensor = root.GetComponent<PlayerGroundSensor>();
                var body = root.GetComponent<Rigidbody2D>();

                var deathSo = new SerializedObject(death);
                deathSo.FindProperty("playerController").objectReferenceValue = controllerComp;
                deathSo.FindProperty("rigidBody").objectReferenceValue = body;
                deathSo.FindProperty("enableDebugKillKey").boolValue = true;
                deathSo.ApplyModifiedPropertiesWithoutUndo();

                var animSo = new SerializedObject(playerAnimator);
                animSo.FindProperty("animator").objectReferenceValue = animator;
                animSo.FindProperty("playerController").objectReferenceValue = controllerComp;
                animSo.FindProperty("groundSensor").objectReferenceValue = sensor;
                animSo.FindProperty("playerDeath").objectReferenceValue = death;
                animSo.FindProperty("rigidBody").objectReferenceValue = body;
                animSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Refresh scene instance if present.
            var scenePath = "Assets/Scenes/Gameplay.unity";
            if (File.Exists(scenePath))
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                    scenePath,
                    UnityEditor.SceneManagement.OpenSceneMode.Single);
                var player = GameObject.Find("Player_Pip");
                if (player != null)
                {
                    PrefabUtility.RevertObjectOverride(player, InteractionMode.AutomatedAction);
                }

                // Keep Phase 2 hint current with death debug key.
                var hint = GameObject.Find("HudHintText");
                if (hint != null)
                {
                    var text = hint.GetComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = "GAMEPLAY | Esc=Pause | Shift=Run | K=Death | G=Game Over | C=Level Complete";
                    }
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            }
        }

        private static AnimationClip CreateSpriteClip(string clipName, Sprite[] frames, float frameRate, bool loop)
        {
            var path = $"{AnimFolder}/{clipName}.anim";
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
            return clip;
        }

        private static Sprite[] CreateSpriteSet(
            string baseName,
            Color color,
            bool bob,
            int frames,
            bool lean = false,
            bool stretchX = false,
            bool taller = false,
            bool wider = false,
            bool squashed = false,
            bool fallen = false)
        {
            var sprites = new Sprite[frames];
            for (var i = 0; i < frames; i++)
            {
                var t = frames == 1 ? 0f : i / (float)(frames - 1);
                var width = 32;
                var height = 32;

                if (taller)
                {
                    height = 36;
                }

                if (wider || stretchX)
                {
                    width = 36;
                }

                if (squashed)
                {
                    width = 36;
                    height = 24;
                }

                if (fallen)
                {
                    width = 40;
                    height = 18;
                }

                if (bob)
                {
                    height += (i % 2 == 0) ? 0 : 2;
                }

                if (lean && i % 2 == 1)
                {
                    width += 2;
                }

                var path = $"{ArtFolder}/{baseName}_{i}.png";
                sprites[i] = CreateColorSprite(path, color * (1f - (t * 0.08f)), width, height, 32f);
            }

            return sprites;
        }

        private static Sprite CreateColorSprite(string assetPath, Color color, int width, int height, float ppu)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Soft border so frames are visually distinct.
                    var edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    pixels[y * width + x] = edge ? color * 0.75f : color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
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
    }
}
#endif
