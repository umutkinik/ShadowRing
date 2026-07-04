using Golgehalka.Core;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.UI
{
    /// PROTOTİP HUD — Canvas kurulumu gerektirmeyen IMGUI.
    /// Gerçek (TMP tabanlı, lokalize) HUD Faz 1'de HUDController ile kurulur;
    /// bu sınıf yalnızca çekirdek döngüyü test etmek için.
    public class DebugHUD : MonoBehaviour
    {
        public WaveManager waveManager;
        public BuildManager buildManager;
        public HeroDefinition[] heroes;

        private void OnGUI()
        {
            // Kablolama koptuysa hata basmak yerine ekranda söyle
            if (waveManager == null || buildManager == null ||
                EconomyManager.Instance == null || GameManager.Instance == null)
            {
                GUI.Label(new Rect(20, 20, 800, 40),
                    "DebugHUD: referans eksik — sahneyi 'Gölgehalka → Prototip Sahne Kur' ile yeniden üret.");
                return;
            }

            GUI.skin.button.fontSize = 28;
            GUI.skin.label.fontSize = 28;
            GUI.skin.box.fontSize = 24;

            // Üst bilgi çubuğu
            GUILayout.BeginArea(new Rect(20, 20, 640, 60));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Altın: " + EconomyManager.Instance.Gold);
            GUILayout.Space(24);
            GUILayout.Label("Can: " + GameManager.Instance.Lives);
            GUILayout.Space(24);
            GUILayout.Label("Dalga: " + (waveManager.CurrentWave) + "/" + waveManager.TotalWaves);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Durum
            var state = GameManager.Instance.State;
            if (state == GameState.Victory || state == GameState.Defeat)
            {
                GUI.Box(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 40, 320, 80),
                    state == GameState.Victory ? "ZAFER!" : "YENİLGİ");
                return;
            }

            // Alt kontrol çubuğu
            GUILayout.BeginArea(new Rect(20, Screen.height - 90, Screen.width - 40, 70));
            GUILayout.BeginHorizontal();

            GUI.enabled = state == GameState.Preparing || state == GameState.BetweenWaves;
            if (GUILayout.Button("Sonraki Dalga", GUILayout.Height(56)))
                waveManager.StartNextWave();
            GUI.enabled = true;

            GUILayout.Space(20);
            foreach (var h in heroes)
            {
                bool selected = buildManager.SelectedHero == h;
                string label = (selected ? "▶ " : "") + h.heroId + " (" + h.tiers[0].cost + ")";
                if (GUILayout.Button(label, GUILayout.Height(56)))
                    buildManager.SelectHero(h);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Hız 1×", GUILayout.Height(56))) GameManager.Instance.SetGameSpeed(1f);
            if (GUILayout.Button("2×", GUILayout.Height(56))) GameManager.Instance.SetGameSpeed(2f);
            if (GUILayout.Button("3×", GUILayout.Height(56))) GameManager.Instance.SetGameSpeed(3f);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
