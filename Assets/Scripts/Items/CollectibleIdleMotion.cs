// Filename: CollectibleIdleMotion.cs
// Folder: Assets/Scripts/Items/
// Purpose: Light bob/spin so placeable coins/power-ups read as pickups (Phase 12/13).
// Dependencies: Collectible, PowerUpPickup

using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// Idle motion for collectibles and power-up pickups. Stops once collected.
    /// </summary>
    public class CollectibleIdleMotion : MonoBehaviour
    {
        [SerializeField] private Collectible collectible;
        [SerializeField] private PowerUpPickup powerUpPickup;
        [SerializeField] private float bobAmplitude = 0.12f;
        [SerializeField] private float bobSpeed = 2.6f;
        [SerializeField] private float spinDegreesPerSecond = 90f;

        private Vector3 _origin;
        private float _phase;

        private void Awake()
        {
            if (collectible == null)
            {
                collectible = GetComponent<Collectible>();
            }

            if (powerUpPickup == null)
            {
                powerUpPickup = GetComponent<PowerUpPickup>();
            }

            _origin = transform.localPosition;
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if ((collectible != null && collectible.IsCollected)
                || (powerUpPickup != null && powerUpPickup.IsCollected))
            {
                enabled = false;
                return;
            }

            _phase += bobSpeed * Time.deltaTime;
            var offset = Mathf.Sin(_phase) * bobAmplitude;
            transform.localPosition = _origin + Vector3.up * offset;

            if (Mathf.Abs(spinDegreesPerSecond) > 0.01f)
            {
                transform.Rotate(0f, 0f, -spinDegreesPerSecond * Time.deltaTime, Space.Self);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bobAmplitude = Mathf.Max(0f, bobAmplitude);
            bobSpeed = Mathf.Max(0f, bobSpeed);
        }
#endif
    }
}
