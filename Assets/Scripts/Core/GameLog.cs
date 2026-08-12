// Filename: GameLog.cs
// Folder: Assets/Scripts/Core/
// Purpose: Centralized logging/debugging conventions for Bounder Trail.
// Dependencies: None (UnityEngine only).

using UnityEngine;

namespace BounderTrail.Core
{
    /// <summary>
    /// Lightweight project logging wrapper.
    /// Prefer GameLog over raw Debug.Log so messages stay consistent and filterable.
    /// </summary>
    public static class GameLog
    {
        public const string ProjectPrefix = "[BounderTrail]";

        public static bool EnableInfo = true;
        public static bool EnableWarnings = true;
        public static bool EnableErrors = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureForPlayer()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            // Shipping players stay quiet unless something warns/errors (Phase 32).
            EnableInfo = false;
#endif
        }

        public static void Info(string category, string message)
        {
            if (!EnableInfo)
            {
                return;
            }

            Debug.Log(Format(category, message));
        }

        public static void Warning(string category, string message)
        {
            if (!EnableWarnings)
            {
                return;
            }

            Debug.LogWarning(Format(category, message));
        }

        public static void Error(string category, string message)
        {
            if (!EnableErrors)
            {
                return;
            }

            Debug.LogError(Format(category, message));
        }

        /// <summary>
        /// Draws a one-frame debug line in the Scene view / Game view (Gizmos/debug draw).
        /// Safe no-op wrapper for consistent debug usage later.
        /// </summary>
        public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f)
        {
            Debug.DrawRay(origin, direction, color, duration);
        }

        private static string Format(string category, string message)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                category = "General";
            }

            return $"{ProjectPrefix}[{category}] {message}";
        }
    }
}
