using Golgehalka.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// Ana menü: Oyna / Kahramanlar / Dükkân / Ayarlar (+dil seçici).
    /// Buton metinleri LocalizedText ile bağlanır (menu.play vb.).
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private TMP_Dropdown languageDropdown;

        // Dropdown sırası — LocalizationManager.Supported ile aynı
        private static readonly string[] LocaleCodes = { "en", "de", "ru", "zh-Hans", "hi", "ar", "tr" };
        private static readonly string[] LocaleNames =
            { "English", "Deutsch", "Русский", "简体中文", "हिन्दी", "العربية", "Türkçe" };

        private void Start()
        {
            LocalizationManager.Init();
            SetupLanguageDropdown();
        }

        public void OnPlay() => SceneManager.LoadScene(levelSelectScene);
        public void OnSettings() => settingsPanel.SetActive(true);
        public void OnShop() => shopPanel.SetActive(true);

        private void SetupLanguageDropdown()
        {
            languageDropdown.ClearOptions();
            var opts = new System.Collections.Generic.List<string>(LocaleNames);
            languageDropdown.AddOptions(opts);
            languageDropdown.value = System.Array.IndexOf(LocaleCodes, LocalizationManager.CurrentLocale);
            languageDropdown.onValueChanged.AddListener(
                i => LocalizationManager.SetLocale(LocaleCodes[i]));
        }

        // Ayarlar panelindeki ses slider'ları
        public void OnMusicVolume(float v) { PlayerProfile.Data.musicVolume = v; PlayerProfile.Save(); }
        public void OnSfxVolume(float v) { PlayerProfile.Data.sfxVolume = v; PlayerProfile.Save(); }
    }
}
