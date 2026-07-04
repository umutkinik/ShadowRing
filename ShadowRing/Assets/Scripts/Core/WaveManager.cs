using System.Collections;
using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Core
{
    /// LevelDefinition'daki dalgaları sırayla sahneye döker,
    /// dalga bitişini takip eder, seviye sonunda zaferi ilan eder.
    public class WaveManager : MonoBehaviour
    {
        // Editor araçları doğrudan atayabilsin diye public (SerializedObject ataması
        // level için sessizce başarısız olmuştu — doğrudan atama kanıtlı yol).
        public LevelDefinition level;
        public WaypointPath path;

        public int CurrentWave { get; private set; }
        public int TotalWaves => level != null ? level.waves.Length : 0;

        private int aliveCount;
        private bool spawningDone;

        private void OnEnable() => Enemy.OnAnyEnemyDied += HandleEnemyGone;
        private void OnDisable() => Enemy.OnAnyEnemyDied -= HandleEnemyGone;

        /// Bölüm ekonomisini uygula: başlangıç altını + can (LevelDefinition'dan).
        private void Start() => ApplyLevelStart();

        private void ApplyLevelStart()
        {
            if (level == null) return;
            EconomyManager.Instance.SetGold(level.startingGold);
            GameManager.Instance.ResetLives(level.startingLives);
        }

        /// Bölüm seçici — yalnızca ilk dalga başlamadan önce değiştirilebilir.
        public void SetLevel(LevelDefinition newLevel)
        {
            if (CurrentWave > 0 || GameManager.Instance.State == GameState.WaveActive) return;
            level = newLevel;
            ApplyLevelStart(); // yeni bölümün altın/can değerleri anında yansır
        }

        /// UI'daki "Sonraki Dalga" butonu çağırır (hud.next_wave).
        public void StartNextWave()
        {
            if (level == null || path == null)
            {
                Debug.LogError("WaveManager: level/path atanmadı — sahneyi 'Gölgehalka → Prototip Sahne Kur' ile yeniden üret.");
                return;
            }
            if (GameManager.Instance.State == GameState.WaveActive) return;
            if (CurrentWave >= TotalWaves) return;
            StartCoroutine(RunWave(level.waves[CurrentWave]));
        }

        private IEnumerator RunWave(WaveDefinition wave)
        {
            GameManager.Instance.SetState(GameState.WaveActive);
            spawningDone = false;

            foreach (SpawnEntry entry in wave.entries)
            {
                yield return new WaitForSeconds(entry.startDelay);
                for (int i = 0; i < entry.count; i++)
                {
                    Spawn(entry.enemy);
                    yield return new WaitForSeconds(entry.interval);
                }
            }
            spawningDone = true;
            CheckWaveEnd(); // spawn bitmeden hepsi öldüyse
        }

        private void Spawn(EnemyDefinition def)
        {
            var go = Instantiate(def.prefab);
            go.GetComponent<Enemy>().Init(def, path);
            aliveCount++;
        }

        private void HandleEnemyGone(Enemy _)
        {
            aliveCount--;
            CheckWaveEnd();
        }

        private void CheckWaveEnd()
        {
            if (!spawningDone || aliveCount > 0) return;
            if (GameManager.Instance.State == GameState.Defeat) return;

            EconomyManager.Instance.AddGold((int)level.waves[CurrentWave].rewardGold);
            CurrentWave++;

            GameManager.Instance.SetState(
                CurrentWave >= TotalWaves ? GameState.Victory : GameState.BetweenWaves);
        }
    }
}
