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
        public Button[] speedButtons;      // 1× / 2× / 3×
        public Button langButton;
        public TMP_Text langButtonLabel;
        public Button victoryButton;       // "devam" → sahneyi yeniden yükle
        public Button defeatButton;        // "tekrar dene"

        [Header("Sonuç panelleri")]
        public GameObject victoryPanel;
        public TMP_Text starsText;
        public GameObject defeatPanel;

        [Header("Kule paneli")]
        public GameObject towerPanel;
        public TMP_Text towerTitle;
        public TMP_Text towerInfo;
        public Button upgradeButton;
        public TMP_Text upgradeLabel;
        public Button sellButton;
        public TMP_Text sellLabel;
        public Button towerCloseButton;

        private Combat.Tower selectedTower;

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

            SelectCampaignLevel(); // kampanya: ilk bitmemiş bölümden devam

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

            // Kule paneli
            BuildManager.OnTowerTapped += ShowTowerPanel;
            upgradeButton.onClick.AddListener(() =>
            {
                if (selectedTower != null && selectedTower.TryUpgrade()) RefreshTowerPanel();
            });
            sellButton.onClick.AddListener(() =>
            {
                if (selectedTower != null) selectedTower.Sell();
                HideTowerPanel();
            });
            towerCloseButton.onClick.AddListener(HideTowerPanel);
            EconomyManager.Instance.OnGoldChanged += _ =>
            {
                if (towerPanel.activeSelf) RefreshTowerPanel();
            };
            towerPanel.SetActive(false);

            victoryPanel.SetActive(false);
            defeatPanel.SetActive(false);
            RefreshStats();
        }

        private void OnDestroy() => BuildManager.OnTowerTapped -= ShowTowerPanel;

        private void ShowTowerPanel(Combat.Tower tower)
        {
            selectedTower = tower;
            towerPanel.SetActive(true);
            RefreshTowerPanel();
        }

        private void HideTowerPanel()
        {
            selectedTower = null;
            towerPanel.SetActive(false);
        }

        private void RefreshTowerPanel()
        {
            if (selectedTower == null) { HideTowerPanel(); return; }
            System.Func<string, string> L = LocalizationManager.Get;
            var tier = selectedTower.CurrentTier;

            towerTitle.text = selectedTower.Hero.heroId + " — K" + (selectedTower.TierIndex + 1);
            towerInfo.text =
                "DPS: " + (tier.damage * tier.fireRate).ToString("0.#") +
                "\n" + L("stat.damage") + ": " + tier.damage + "   " + L("stat.range") + ": " + tier.range +
                "\n" + L("stat.rate") + ": " + tier.fireRate + "/s";

            if (selectedTower.CanUpgrade)
            {
                upgradeLabel.text = L("shop.upgrade") + " (" + selectedTower.UpgradeCost + ")";
                upgradeButton.interactable = EconomyManager.Instance.Gold >= selectedTower.UpgradeCost;
            }
            else
            {
                upgradeLabel.text = "MAX";
                upgradeButton.interactable = false;
            }
            sellLabel.text = L("shop.sell") + " (+" + selectedTower.SellRefund + ")";
        }

        /// LiberationSans'ın kapsamadığı yazı sistemleri (Kiril vb.) için işletim
        /// sistemi fontundan dinamik TMP fontu üret. Bulunamazsa varsayılan kalır.
        /// (ZH/HI/AR tam desteği Faz 2'de Noto Sans asset'leriyle gelecek.)
        private void ApplyRuntimeFont()
        {
            // Yalnızca sistemde GERÇEKTEN kurulu fontları dene — uyarı spam'i olmasın
            var installed = new System.Collections.Generic.HashSet<string>(
                Font.GetOSInstalledFontNames());
            foreach (string name in new[] { "Arial", "Helvetica Neue", "Helvetica", "Roboto" })
            {
                if (!installed.Contains(name)) continue;
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

        /// Kampanya ilerleyişi: profildeki ilk tamamlanmamış bölüm yüklenir.
        /// Zafer bölümü profile işler → sahne yeniden yüklenince sıradaki gelir.
        private void SelectCampaignLevel()
        {
            if (levels == null || levels.Length == 0) return;
            int idx = levels.Length - 1; // hepsi bittiyse son bölüm (sonsuz mod gelene dek)
            for (int i = 0; i < levels.Length; i++)
                if (!PlayerProfile.IsCompleted(levels[i].levelId)) { idx = i; break; }
            waveManager.SetLevel(levels[idx]);
            RefreshStats();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            nextWaveButton.interactable =
                gm.State == GameState.Preparing ||
                gm.State == GameState.BetweenWaves;

            // Seçili kule yok olduysa (harita değişimi vb.) paneli kapat
            if (towerPanel.activeSelf && selectedTower == null) HideTowerPanel();
        }

        private void RefreshStats()
        {
            System.Func<string, string> L = LocalizationManager.Get; // C# 9 uyumlu açık tip
            goldText.text = L("hud.gold") + ": " + EconomyManager.Instance.Gold;
            livesText.text = L("hud.lives") + ": " + GameManager.Instance.Lives;
            waveText.text = L("hud.wave") + ": " + waveManager.CurrentWave + "/" + waveManager.TotalWaves;
            int levelNo = levels != null ? System.Array.IndexOf(levels, waveManager.level) + 1 : 0;
            levelText.text = levelNo > 0
                ? L("menu.campaign") + " " + levelNo + "/" + levels.Length
                : "-";
        }

        private void HandleState(GameState state)
        {
            if (state == GameState.Victory)
            {
                int stars = GameManager.Instance.Lives >= 20 ? 3 :
                            GameManager.Instance.Lives >= 15 ? 2 : 1;
                // '*' her fontta var — ★ dinamik OS fontunda eksik olabiliyor (□ sorunu)
                starsText.text = new string('*', stars) + new string('·', 3 - stars) + "   +20";

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
