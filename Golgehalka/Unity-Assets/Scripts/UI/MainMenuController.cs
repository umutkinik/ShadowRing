using Golgehalka.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// Ana menü: karanlık atmosfer, Zarok vitrini, köz partikülleri.
    /// Butonlar çalışma anında kablolanır (editor listener'ları serileşmez).
    public class MainMenuController : MonoBehaviour
    {
        public Button playButton;
        public Button langButton;
        public TMP_Text langLabel;
        public Button creditsButton;
        public GameObject creditsPanel;
        public Button creditsCloseButton;
        public Transform zarok;              // yavaş vitrin dönüşü (3D mod)
        public bool useKeyArt;               // tam ekran illüstrasyon modu

        private static readonly string[] Locales = { "en", "de", "ru", "zh-Hans", "hi", "ar", "tr" };

        private void Awake()
        {
            // MatchHUD ile aynı yaklaşım: OS fontundan geniş kapsamlı dinamik font
            var installed = new System.Collections.Generic.HashSet<string>(Font.GetOSInstalledFontNames());
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
                    {
                        // Başlıklar Cinzel'de kalır — sadece buton/panel metinleri değişir
                        if (t.name == "Title" || t.name == "Subtitle") continue;
                        t.font = tmpFont;
                    }
                    break;
                }
                catch { }
            }
        }

        private void Start()
        {
            LocalizationManager.Init();
            langLabel.text = LocalizationManager.CurrentLocale.ToUpperInvariant();

            playButton.onClick.AddListener(() => SceneManager.LoadScene("Prototype"));
            langButton.onClick.AddListener(CycleLanguage);
            creditsButton.onClick.AddListener(() => creditsPanel.SetActive(true));
            creditsCloseButton.onClick.AddListener(() => creditsPanel.SetActive(false));
            creditsPanel.SetActive(false);

            if (!useKeyArt) SpawnEmbers(); // key-art'ta közler görselin içinde
        }

        private void Update()
        {
            if (zarok != null)
                zarok.Rotate(0, 4f * Time.deltaTime, 0); // heybetli vitrin dönüşü
        }

        private void CycleLanguage()
        {
            int i = System.Array.IndexOf(Locales, LocalizationManager.CurrentLocale);
            LocalizationManager.SetLocale(Locales[(i + 1) % Locales.Length]);
            langLabel.text = LocalizationManager.CurrentLocale.ToUpperInvariant();
        }

        /// Havada süzülen közler — Diablo menü havasının yarısı budur.
        private void SpawnEmbers()
        {
            var go = new GameObject("Embers");
            go.transform.position = new Vector3(0, 0.1f, -1f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.07f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.6f, 0.2f), new Color(1f, 0.35f, 0.1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission; em.rateOverTime = 16f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 0.2f, 5f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

            go.GetComponent<ParticleSystemRenderer>().material = VFX.ParticleMaterial;
            ps.Play();
        }
    }
}
