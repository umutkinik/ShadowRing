using Golgehalka.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// Maç içi HUD: altın / can / dalga sayaçları, sonraki dalga, hız, duraklat.
    /// Etiketler LocalizedText ile bağlanır; burada yalnız değerler güncellenir.
    public class HUDController : MonoBehaviour
    {
        [Header("Sayaçlar")]
        [SerializeField] private TMP_Text goldValue;
        [SerializeField] private TMP_Text livesValue;
        [SerializeField] private TMP_Text waveValue;

        [Header("Kontroller")]
        [SerializeField] private Button nextWaveButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private TMP_Text speedLabel;
        [SerializeField] private WaveManager waveManager;

        [Header("Paneller")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        private readonly float[] speeds = { 1f, 2f, 3f };
        private int speedIndex;

        private void Start()
        {
            EconomyManager.Instance.OnGoldChanged += g => goldValue.text = g.ToString();
            GameManager.Instance.OnLivesChanged += l => livesValue.text = l.ToString();
            GameManager.Instance.OnStateChanged += HandleState;

            nextWaveButton.onClick.AddListener(() =>
            {
                waveManager.StartNextWave();
                RefreshWave();
            });
            speedButton.onClick.AddListener(CycleSpeed);

            goldValue.text = EconomyManager.Instance.Gold.ToString();
            livesValue.text = GameManager.Instance.Lives.ToString();
            RefreshWave();
        }

        private void RefreshWave() =>
            waveValue.text = (waveManager.CurrentWave + 1) + " / " + waveManager.TotalWaves;

        private void CycleSpeed()
        {
            speedIndex = (speedIndex + 1) % speeds.Length;
            GameManager.Instance.SetGameSpeed(speeds[speedIndex]);
            speedLabel.text = speeds[speedIndex].ToString("0") + "×";
        }

        private void HandleState(GameState state)
        {
            // Dalga butonu yalnız dalgalar arasında aktif
            nextWaveButton.interactable =
                state == GameState.Preparing || state == GameState.BetweenWaves;

            if (state == GameState.Victory) victoryPanel.SetActive(true);
            if (state == GameState.Defeat) defeatPanel.SetActive(true);
            if (state == GameState.BetweenWaves) RefreshWave();
        }
    }
}
