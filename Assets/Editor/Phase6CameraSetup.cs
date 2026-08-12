// Filename: Phase6CameraSetup.cs
// Folder: Assets/Editor/
// Purpose: Adds CameraFollow2D + LevelBounds to the Gameplay scene (Phase 6).
// Dependencies: BounderTrail.CameraSystem.CameraFollow2D, BounderTrail.Levels.LevelBounds
//
// Menu: Bounder Trail/Phase 6/Setup Camera System
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase6CameraSetup.SetupCameraSystem

#if UNITY_EDITOR
using BounderTrail.CameraSystem;
using BounderTrail.Core;
using BounderTrail.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase6CameraSetup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        private static readonly string[] GameplayScenes =
        {
            GameplayScenePath,
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        [MenuItem("Bounder Trail/Phase 6/Setup Camera System")]
        public static void SetupCameraSystem()
        {
            for (var i = 0; i < GameplayScenes.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenes[i]) != null)
                {
                    SetupCameraInScene(GameplayScenes[i]);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 6 camera system ready ({GameplayScenes.Length} scenes).");
        }

        private static void SetupCameraInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var levelRoot = GameObject.Find("_Level");
            if (levelRoot == null)
            {
                levelRoot = new GameObject("_Level");
            }

            var boundsObject = GameObject.Find("LevelBounds");
            if (boundsObject == null)
            {
                boundsObject = new GameObject("LevelBounds");
                boundsObject.transform.SetParent(levelRoot.transform, false);
            }

            boundsObject.transform.position = new Vector3(2f, 1f, 0f);
            var bounds = boundsObject.GetComponent<LevelBounds>();
            if (bounds == null)
            {
                bounds = boundsObject.AddComponent<LevelBounds>();
            }

            var boundsSo = new SerializedObject(bounds);
            boundsSo.FindProperty("size").vector2Value = new Vector2(36f, 16f);
            boundsSo.FindProperty("centerOffset").vector2Value = Vector2.zero;
            boundsSo.ApplyModifiedPropertiesWithoutUndo();

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = GameObject.Find("Main Camera");
                if (cameraObject != null)
                {
                    camera = cameraObject.GetComponent<Camera>();
                }
            }

            if (camera == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] No Main Camera found in Gameplay.");
                return;
            }

            camera.orthographic = true;
            camera.orthographicSize = ProjectConstants.GameplayOrthographicSize;

            var follow = camera.GetComponent<CameraFollow2D>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<CameraFollow2D>();
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            var followSo = new SerializedObject(follow);
            followSo.FindProperty("target").objectReferenceValue = player != null ? player.transform : null;
            followSo.FindProperty("focusOffset").vector2Value = new Vector2(0f, 0.75f);
            followSo.FindProperty("smoothTimeX").floatValue = 0.12f;
            followSo.FindProperty("smoothTimeY").floatValue = 0.18f;
            followSo.FindProperty("maxSpeed").floatValue = 40f;
            followSo.FindProperty("useDeadZone").boolValue = true;
            followSo.FindProperty("deadZoneSize").vector2Value = new Vector2(1.8f, 1.1f);
            followSo.FindProperty("levelBounds").objectReferenceValue = bounds;
            followSo.FindProperty("clampToBounds").boolValue = true;
            followSo.FindProperty("autoFindTarget").boolValue = true;
            followSo.ApplyModifiedPropertiesWithoutUndo();

            // Snap immediately in editor so the scene view starts framed.
            follow.SnapToTarget();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
