using Golgehalka.Monetization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Golgehalka.Core
{
    /// Bölüm geçiş akışı — reklam entegrasyonunun TEK noktası:
    ///   Zafer → sonuç ekranı → [interstitial*] → sonraki bölüm / harita
    ///   (*yalnızca ücretsiz sürümde; destekçi doğrudan geçer)
    public class LevelFlow : MonoBehaviour
    {
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private void OnEnable()
        {
            GameManager.Instance.OnStateChanged += HandleState;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleState;
        }

        private void HandleState(GameState state)
        {
            // Zafer/yenilgi ekranını UI gösterir; geçişler aşağıdaki
            // butonlardan tetiklenir (UI OnClick bağlantıları).
        }

        /// Zafer ekranındaki "Sonraki Bölüm" butonu.
        public void ContinueToNextLevel(string nextLevelScene)
        {
            AdsManager.Instance.ShowInterstitialThen(
                () => SceneManager.LoadScene(nextLevelScene));
        }

        /// Zafer/yenilgi ekranındaki "Haritaya Dön" butonu.
        public void BackToLevelSelect()
        {
            AdsManager.Instance.ShowInterstitialThen(
                () => SceneManager.LoadScene(levelSelectScene));
        }

        /// Yenilgi ekranındaki "Tekrar Dene" — reklam YOK:
        /// kaybeden oyuncuya reklam göstermek churn'ün bir numaralı sebebi.
        public void RetryLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
