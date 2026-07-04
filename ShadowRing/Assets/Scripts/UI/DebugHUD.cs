using Golgehalka.Core;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.UI
{
    /// PROTOTİP HUD — Canvas kurulumu gerektirmeyen IMGUI.
    /// Gerçek (TMP tabanlı, lokalize) HUD Faz 1'de HUDController ile kurulur.
    public class DebugHUD : MonoBehaviour
    {
        public WaveManager waveManager;
        public BuildManager buildManager;
        public HeroDefinition[] heroes;
        public LevelDefinition[] levels;   // Act I bölümleri (seçici)

        private bool victorySaved;

        private void OnGUI()
        {
            if (waveManager == null || buildManager == null ||
                EconomyManager.Instance == null || GameManager.Instance == null)
            {
                GUI.Label(new Rect(20, 20, 800, 40),
                    "DebugHUD: referans eksik — sahneyi 'Gölgehalka → Prototip Sahne Kur' ile yeniden üret.");
                return;
            }

            GUI.skin.button.fontSize = 26;
            GUI.skin.label.fontSize = 26;
            GUI.skin.box.fontSize = 24;

            DrawTopBar();

            var state = GameManager.Instance.State;
            if (state == GameState.Victory) { DrawVictory(); return; }
            if (state == GameState.Defeat)
            {
                GUI.Box(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 40, 320, 80), "YENİLGİ");
                return;
            }

            DrawLevelPicker(state);
            DrawBottomBar(state);
        }

        private void DrawTopBar()
        {
            GUILayout.BeginArea(new Rect(20, 16, Screen.width - 40, 46));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Altın: " + EconomyManager.Instance.Gold);
            GUILayout.Space(24);
            GUILayout.Label("Can: " + GameManager.Instance.Lives);
            GUILayout.Space(24);
            GUILayout.Label("Dalga: " + waveManager.CurrentWave + "/" + waveManager.TotalWaves);
            GUILayout.Space(24);
            if (waveManager.level != null)
                GUILayout.Label("Bölüm: " + waveManager.level.levelId);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// Bölüm seçimi — yalnızca ilk dalga başlamadan görünür.
        private void DrawLevelPicker(GameState state)
        {
            if (levels == null || levels.Length == 0) return;
            if (state != GameState.Preparing || waveManager.CurrentWave > 0) return;

            GUILayout.BeginArea(new Rect(20, 70, Screen.width - 40, 56));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bölüm seç:", GUILayout.Width(150));
            for (int i = 0; i < levels.Length; i++)
            {
                bool current = waveManager.level == levels[i];
                if (GUILayout.Button((current ? "▶ " : "") + "B" + (i + 1), GUILayout.Height(44)))
                    waveManager.SetLevel(levels[i]);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawVictory()
        {
            int stars = CalculateStars(GameManager.Instance.Lives);

            // Profil kaydı — yalnız bir kez (yıldız + parça ödülü)
            if (!victorySaved && waveManager.level != null)
            {
                PlayerProfile.CompleteLevel(waveManager.level.levelId, stars, 20);
                victorySaved = true;
            }

            string starText = new string('★', stars) + new string('☆', 3 - stars);
            GUI.Box(new Rect(Screen.width / 2f - 200, Screen.height / 2f - 60, 400, 120),
                "ZAFER!\n" + starText + "   +20 parça");
        }

        /// balance-v1.md: 3★ = hiç can kaybı yok, 2★ ≥ 15 can, 1★ = tamamlama.
        private static int CalculateStars(int livesLeft)
        {
            if (livesLeft >= 20) return 3;
            if (livesLeft >= 15) return 2;
            return 1;
        }

        private void DrawBottomBar(GameState state)
        {
            GUILayout.BeginArea(new Rect(20, Screen.height - 86, Screen.width - 40, 66));
            GUILayout.BeginHorizontal();

            GUI.enabled = state == GameState.Preparing || state == GameState.BetweenWaves;
            if (GUILayout.Button("Sonraki Dalga", GUILayout.Height(52)))
                waveManager.StartNextWave();
            GUI.enabled = true;

            GUILayout.Space(16);
            foreach (var h in heroes)
            {
                bool selected = buildManager.SelectedHero == h;
                string label = (selected ? "▶ " : "") + h.heroId + " (" + h.tiers[0].cost + ")";
                if (GUILayout.Button(label, GUILayout.Height(52)))
                    buildManager.SelectHero(h);
            }

            GUILayout.Space(16);
            if (GUILayout.Button("1×", GUILayout.Height(52))) GameManager.Instance.SetGameSpeed(1f);
            if (GUILayout.Button("2×", GUILayout.Height(52))) GameManager.Instance.SetGameSpeed(2f);
            if (GUILayout.Button("3×", GUILayout.Height(52))) GameManager.Instance.SetGameSpeed(3f);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
