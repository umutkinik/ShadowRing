using System;
using UnityEngine;

namespace Golgehalka.Core
{
    public enum GameState { Preparing, WaveActive, BetweenWaves, Victory, Defeat }

    /// Maçın genel durumu: can, hız, zafer/yenilgi.
    /// Sahnede tek instance — diğer sistemler event'lere abone olur.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startingLives = 20;

        public int Lives { get; private set; }
        public GameState State { get; private set; } = GameState.Preparing;

        public event Action<int> OnLivesChanged;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            // Sahne-yerel singleton: "son gelen kazanır" — Unity 6 hızlı Play modunda
            // statikler sıfırlanmayabildiği için Destroy tabanlı kalıp kullanılmaz.
            Instance = this;
            Lives = startingLives;
            State = GameState.Preparing;
            Time.timeScale = 1f;
        }

        public void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            OnStateChanged?.Invoke(next);
            if (next == GameState.Victory || next == GameState.Defeat)
                Time.timeScale = 1f; // sonuç ekranı normal hızda
        }

        /// Bölüm başlangıcı — LevelDefinition.startingLives uygulanır.
        public void ResetLives(int amount)
        {
            Lives = amount;
            OnLivesChanged?.Invoke(Lives);
        }

        /// Düşman yolu bitirdiğinde çağrılır (leak).
        public void LoseLife(int amount = 1)
        {
            if (State == GameState.Defeat) return;
            Lives = Mathf.Max(0, Lives - amount);
            OnLivesChanged?.Invoke(Lives);
            if (Lives <= 0) SetState(GameState.Defeat);
        }

        /// 1x / 2x / 3x oyun hızı (mobil TD standardı).
        public void SetGameSpeed(float multiplier) => Time.timeScale = multiplier;
    }
}
