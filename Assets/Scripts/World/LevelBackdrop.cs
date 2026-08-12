// Filename: LevelBackdrop.cs
// Folder: Assets/Scripts/World/
// Purpose: Visual-only parallax backdrop layers (Phase 25/33). No gameplay impact.
// Dependencies: None

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Keeps decorative backdrop layers aligned with the camera using light parallax.
    /// </summary>
    public class LevelBackdrop : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform farLayer;
        [SerializeField] private Transform midLayer;
        [SerializeField] private Transform nearLayer;
        [SerializeField] private Vector2 farParallax = new Vector2(0.12f, 0.04f);
        [SerializeField] private Vector2 midParallax = new Vector2(0.28f, 0.08f);
        [SerializeField] private Vector2 nearParallax = new Vector2(0.45f, 0.12f);
        [SerializeField] private Vector3 farOffset = new Vector3(0f, 1.5f, 10f);
        [SerializeField] private Vector3 midOffset = new Vector3(0f, -0.5f, 8f);
        [SerializeField] private Vector3 nearOffset = new Vector3(0f, 1.2f, 6f);

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                var cam = Camera.main;
                if (cam == null)
                {
                    return;
                }

                cameraTransform = cam.transform;
            }

            var camPos = cameraTransform.position;
            Apply(farLayer, camPos, farParallax, farOffset);
            Apply(midLayer, camPos, midParallax, midOffset);
            Apply(nearLayer, camPos, nearParallax, nearOffset);
        }

        private static void Apply(Transform layer, Vector3 camPos, Vector2 parallax, Vector3 offset)
        {
            if (layer == null)
            {
                return;
            }

            layer.position = new Vector3(
                camPos.x * parallax.x + offset.x,
                camPos.y * parallax.y + offset.y,
                offset.z);
        }
    }
}
