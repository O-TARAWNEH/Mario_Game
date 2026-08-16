// Filename: FlowScreenSummary.cs
// Folder: Assets/Scripts/UI/
// Purpose: Shows coin/score summary on Game Over and Level Complete panels (Phase 17/31/42).
// Dependencies: CollectibleCounter, GameProgress

using BounderTrail.Items;
using BounderTrail.Save;
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
        [SerializeField] private string format = "Coins  {0}\nScore  {1}";
        [SerializeField] private string campaignCompleteFormat =
            "Campaign cleared!\nCoins  {0}\nScore  {1}\nBest  {2} coins / {3} score";
        [SerializeField] private string bonusLifeFormat = "\nBonus lives  +{0}";

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
            var bonusLives = CollectibleCounter.Instance != null
                ? CollectibleCounter.Instance.BonusLivesEarnedThisRun
                : 0;

            string text;
            if (campaignComplete && !string.IsNullOrEmpty(campaignCompleteFormat))
            {
                var bestCoins = GameProgress.Instance != null ? GameProgress.Instance.BestCoins : coins;
                var bestScore = GameProgress.Instance != null ? GameProgress.Instance.BestScore : score;
                text = string.Format(campaignCompleteFormat, coins, score, bestCoins, bestScore);
            }
            else
            {
                text = string.Format(format, coins, score);
            }

            if (bonusLives > 0 && !string.IsNullOrEmpty(bonusLifeFormat))
            {
                text += string.Format(bonusLifeFormat, bonusLives);
            }

            summaryText.text = text;
        }
    }
}
