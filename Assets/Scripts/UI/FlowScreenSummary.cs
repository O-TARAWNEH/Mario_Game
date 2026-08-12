// Filename: FlowScreenSummary.cs
// Folder: Assets/Scripts/UI/
// Purpose: Shows coin/score summary on Game Over and Level Complete panels (Phase 17/31).
// Dependencies: CollectibleCounter

using BounderTrail.Items;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Lightweight run summary for end-of-run screens.
    /// </summary>
    public class FlowScreenSummary : MonoBehaviour
    {
        [SerializeField] private Text summaryText;
        [SerializeField] private string format = "Coins: {0}   Score: {1}";
        [SerializeField] private string campaignCompleteFormat = "Campaign cleared!\nCoins: {0}   Score: {1}";

        private void OnEnable()
        {
            Refresh(false);
        }

        public void Refresh()
        {
            Refresh(false);
        }

        public void Refresh(bool campaignComplete)
        {
            if (summaryText == null)
            {
                return;
            }

            var coins = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.CoinCount : 0;
            var score = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.Score : 0;
            var template = campaignComplete && !string.IsNullOrEmpty(campaignCompleteFormat)
                ? campaignCompleteFormat
                : format;
            summaryText.text = string.Format(template, coins, score);
        }
    }
}
