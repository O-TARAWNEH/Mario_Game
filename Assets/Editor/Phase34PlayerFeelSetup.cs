// Filename: Phase34PlayerFeelSetup.cs
// Folder: Assets/Editor/
// Purpose: Mario-inspired movement retune — snappy jump arc, apex hang (Phase 34).
// Menu: Bounder Trail/Phase 34/Setup Player Feel
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase34PlayerFeelSetup.SetupPlayerFeel

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase34PlayerFeelSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";

        [MenuItem("Bounder Trail/Phase 34/Setup Player Feel")]
        public static void SetupPlayerFeel()
        {
            if (!System.IO.File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {PlayerPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var controller = root.GetComponent<PlayerController>();
                if (controller == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] PlayerController missing on Pip prefab.");
                    return;
                }

                var so = new SerializedObject(controller);
                SetFloat(so, "walkSpeed", 7.2f);
                SetFloat(so, "runSpeed", 10.8f);
                SetFloat(so, "acceleration", 92f);
                SetFloat(so, "deceleration", 98f);
                SetFloat(so, "airAcceleration", 50f);
                SetFloat(so, "airDeceleration", 44f);
                SetFloat(so, "airControl", 0.82f);
                SetFloat(so, "jumpForce", 16.2f);
                SetFloat(so, "coyoteTime", 0.12f);
                SetFloat(so, "jumpBufferTime", 0.12f);
                SetFloat(so, "jumpCutMultiplier", 0.48f);
                SetFloat(so, "jumpCutGravityMultiplier", 2.35f);
                SetFloat(so, "gravity", 3.35f);
                SetFloat(so, "fallGravityMultiplier", 2.15f);
                SetFloat(so, "apexHangGravityMultiplier", 0.42f);
                SetFloat(so, "apexHangVelocityThreshold", 1.25f);
                SetFloat(so, "maximumFallSpeed", 24f);
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 34 player feel applied — snappier run/jump/fall, apex hang on.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("Bounder Trail/Phase 34/Validate Player Feel")]
        public static void ValidatePlayerFeel()
        {
            var issues = 0;
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var so = new SerializedObject(root.GetComponent<PlayerController>());
                issues += AssertMin(so, "fallGravityMultiplier", 2f);
                issues += AssertMin(so, "walkSpeed", 7f);
                issues += AssertHasField(so, "apexHangGravityMultiplier");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 34 player feel validation passed.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 34 player feel validation failed ({issues} issue(s)).");
            }
        }

        private static void SetFloat(SerializedObject so, string prop, float value)
        {
            var p = so.FindProperty(prop);
            if (p != null)
            {
                p.floatValue = value;
            }
        }

        private static int AssertMin(SerializedObject so, string prop, float min)
        {
            var p = so.FindProperty(prop);
            if (p == null || p.floatValue < min)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected {prop} >= {min}");
                return 1;
            }

            return 0;
        }

        private static int AssertHasField(SerializedObject so, string prop)
        {
            if (so.FindProperty(prop) == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing field {prop} on PlayerController");
                return 1;
            }

            return 0;
        }
    }
}
#endif
