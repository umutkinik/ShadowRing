using Golgehalka.Core;
using Golgehalka.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// GERÇEK maç HUD'ı — Canvas + TMP + lokalizasyon.
    /// Builder tarafından kurulup kablolanır (Gölgehalka → Prototip Sahne Kur).
    /// Dil butonu 7 dili sırayla gezer; tüm etiketler anında güncellenir.
    public class MatchHUD : MonoBehaviour
    {
        [Header("Sistem referansları")]
        public WaveManager waveManager;
        public BuildManager buildManager;
        public HeroDefinition[] heroes;
        public LevelDefinition[] levels;

        [Header("Üst bar")]
        public TMP_Text goldText;
        public TMP_Text livesText;
        public TMP_Text waveText;
        public TMP_Text levelText;

        [Header("Kontroller")]
        public Button nextWaveButton;
        public Button[] heroButtons;
        public TMP_Text[] heroButtonLabels;
        public Button[] levelButtons;
        public GameObject levelRow;
        public Button[] speedButtons;      // 1× / 2× / 3×
        public Button langButton;
        public TMP_Text langButtonLabel;
        public Button victoryButton;       // "devam" → sahneyi yeniden yükle
        public Button defeatButton;        // "tekrar dene"

        [Header("Sonuç panelleri")]
        public GameObject victoryPanel;
        public TMP_Text starsText;
        public GameObject defeatPanel;

        private static readonly string[] Locales = { "en", "de", "ru", "zh-Hans", "hi", "ar", "tr" };
        private bool victorySaved;

        private void Awake()
        {
            ApplyRuntimeFont();
        }

        private void Start()
        {
            LocalizationManager.Init();

            EconomyManager.Instance.OnGoldChanged += _ => RefreshStats();
            GameManager.Instance.OnLivesChanged += _ => RefreshStats();
            GameManager.Instance.OnStateChanged += HandleState;
            LocalizationManager.OnLocaleChanged += RefreshStats;

            nextWaveButton.onClick.AddListener(() => { waveManager.StartNextWave(); RefreshStats(); });

            for (int i = 0; i < heroButtons.Length; i++)
            {
                int idx = i;
                heroButtons[i].onClick.AddListener(() => buildManager.SelectHero(heroes[idx]));
                heroButtonLabels[i].text = heroes[i].heroId + " (" + heroes[i].tiers[0].cost + ")";
            }

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int idx = i;
                levelButtons[i].onClick.AddListener(() =>
                {
                    waveManager.SetLevel(levels[idx]);
                    RefreshStats();
                });
            }

            float[] speeds = { 1f, 2f, 3f };
            for (int i = 0; i < speedButtons.Length; i++)
            {
                int idx = i;
                speedButtons[i].onClick.AddListener(() => SetSpeed(speeds[idx]));
            }
            langButton.onClick.AddListener(CycleLanguage);
            victoryButton.onClick.AddListener(ReloadScene);
            defeatButton.onClick.AddListener(ReloadScene);
            langButtonLabel.text = LocalizationManager.CurrentLocale.ToUpperInvariant();

            victoryPanel.SetActive(false);
            defeatPanel.SetActive(false);
            RefreshStats();
        }

        /// LiberationSans'ın kapsamadığı yazı sistemleri (Kiril vb.) için işletim
        /// sistemi fontundan dinamik TMP fontu üret. Bulunamazsa varsayılan kalır.
        /// (ZH/HI/AR tam desteği Faz 2'de Noto Sans asset'leriyle gelecek.)
        private void ApplyRuntimeFont()
        {
            foreach (string name in new[] { "Arial", "Helvetica Neue", "Roboto" })
            {
                try
                {
                    Font os = Font.CreateDynamicFontFromOSFont(name, 48);
                    if (os == null) continue;
                    var tmpFont = TMP_FontAsset.CreateFontAsset(os);
                    if (tmpFont == null) continue;
                    foreach (var t in GetComponentsInChildren<TMP_Text>(true))
                        t.font = tmpFont;
                    return;
                }
                catch { /* sıradaki adayı dene */ }
            }
        }

        /// Dil butonu — 7 dili sırayla gezer.
        public void CycleLanguage()
        {
            int i = System.Array.IndexOf(Locales, LocalizationManager.CurrentLocale);
            LocalizationManager.SetLocale(Locales[(i + 1) % Locales.Length]);
            langButtonLabel.text = LocalizationManager.CurrentLocale.ToUpperInvariant();
        }

        public void SetSpeed(float multiplier) => GameManager.Instance.SetGameSpeed(multiplier);

        private void Update()
        {
            // Bölüm seçici yalnızca ilk dalga öncesi görünür
            bool pickable = GameManager.Instance.State == GameState.Preparing &&
                            waveManager.CurrentWave == 0;
            if (levelRow.activeSelf != pickable) levelRow.SetActive(pickable);

            nextWaveButton.interactable =
                GameManager.Instance.State == GameState.Preparing ||
                GameManager.Instance.State == GameState.BetweenWaves;
        }

        private void RefreshStats()
        {
            var L = LocalizationManager.Get;
            goldText.text = L("hud.gold") + ": " + EconomyManager.Instance.Gold;
            livesText.text = L("hud.lives") + ": " + GameManager.Instance.Lives;
            waveText.text = L("hud.wave") + ": " + waveManager.CurrentWave + "/" + waveManager.TotalWaves;
            levelText.text = waveManager.level != null ? waveManager.level.levelId : "-";
        }

        private void HandleState(GameState state)
        {
            if (state == GameState.Victory)
            {
                int stars = GameManager.Instance.Lives >= 20 ? 3 :
                            GameManager.Instance.Lives >= 15 ? 2 : 1;
                starsText.text = new string('★', stars) + new string('☆', 3 - stars);

                if (!victorySaved && waveManager.level != null)
                {
                    PlayerProfile.CompleteLevel(waveManager.level.levelId, stars, 20);
                    victorySaved = true;
                }
                victoryPanel.SetActive(true);
            }
            if (state == GameState.Defeat) defeatPanel.SetActive(true);
            if (state == GameState.BetweenWaves) RefreshStats();
        }

        /// Zafer/yenilgi panellerindeki buton.
        public void ReloadScene() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
