// Filename: CollectibleCounterUI.cs
// Folder: Assets/Scripts/UI/
// Purpose: Minimal on-screen coin/score counter until full HUD phase (Phase 12).
// Dependencies: BounderTrail.Items.CollectibleCounter

using BounderTrail.Items;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Simple Text readout bound to CollectibleCounter.
    /// Full HUD art/layout comes in a later phase.
    /// </summary>
    public class CollectibleCounterUI : MonoBehaviour
    {
        [SerializeField] private Text coinText;
        [SerializeField] private Text scoreText;
        [SerializeField] private string coinFormat = "Coins: {0}";
        [SerializeField] private string scoreFormat = "Score: {0}";

        private void OnEnable()
        {
            BindCounter();
            Refresh();
        }

        private void Start()
        {
            BindCounter();
            Refresh();
        }

        private void OnDisable()
        {
            if (CollectibleCounter.Instance != null)
            {
                CollectibleCounter.Instance.CountsChanged -= Refresh;
            }
        }

        private void BindCounter()
        {
            if (CollectibleCounter.Instance == null)
            {
                return;
            }

            CollectibleCounter.Instance.CountsChanged -= Refresh;
            CollectibleCounter.Instance.CountsChanged += Refresh;
        }

        private void Refresh()
        {
            var coins = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.CoinCount : 0;
            var score = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.Score : 0;

            if (coinText != null)
            {
                coinText.text = string.Format(coinFormat, coins);
            }

            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, score);
            }
        }
    }
}
