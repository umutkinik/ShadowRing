using Golgehalka.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// Zafer/yenilgi paneli — yıldızları hesaplar, ilerlemeyi kaydeder,
    /// geçiş butonlarını LevelFlow'a (ve oradan reklam akışına) bağlar.
    public class ResultScreenController : MonoBehaviour
    {
        [SerializeField] private LevelFlow levelFlow;
        [SerializeField] private string levelId;          // "act1_level1"
        [SerializeField] private string nextLevelScene;   // boşsa "Sonraki" gizlenir (act sonu)
        [SerializeField] private int shardReward = 20;

        [Header("Zafer paneli")]
        [SerializeField] private Image[] starIcons;       // 3 yıldız görseli
        [SerializeField] private TMP_Text shardText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button mapButton;

        [Header("Yenilgi paneli")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button defeatMapButton;

        private void Start()
        {
            nextButton.onClick.AddListener(() => levelFlow.ContinueToNextLevel(nextLevelScene));
            mapButton.onClick.AddListener(levelFlow.BackToLevelSelect);
            retryButton.onClick.AddListener(levelFlow.RetryLevel);          // reklamsız!
            defeatMapButton.onClick.AddListener(levelFlow.BackToLevelSelect);

            nextButton.gameObject.SetActive(!string.IsNullOrEmpty(nextLevelScene));
            GameManager.Instance.OnStateChanged += HandleState;
        }

        private void HandleState(GameState state)
        {
            if (state != GameState.Victory) return;

            int stars = CalculateStars(GameManager.Instance.Lives);
            for (int i = 0; i < starIcons.Length; i++)
                starIcons[i].color = i < stars ? Color.white : new Color(1, 1, 1, 0.25f);

            shardText.text = "+" + shardReward;
            PlayerProfile.CompleteLevel(levelId, stars, shardReward);
        }

        /// balance-v1.md: 3★ = 20 can (hiç kayıp yok), 2★ ≥ 15, 1★ = tamamlama.
        private static int CalculateStars(int livesLeft)
        {
            if (livesLeft >= 20) return 3;
            if (livesLeft >= 15) return 2;
            return 1;
        }
    }
}
