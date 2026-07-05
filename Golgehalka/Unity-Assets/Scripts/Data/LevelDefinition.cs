using UnityEngine;

namespace Golgehalka.Data
{
    /// Bir dalga içindeki tek spawn girdisi: "X düşmandan N adet, D sn arayla".
    [System.Serializable]
    public class SpawnEntry
    {
        public EnemyDefinition enemy;
        public int count = 5;
        public float interval = 1f;     // aynı girdinin düşmanları arası süre
        public float startDelay;        // dalga başladıktan sonra bekleme
    }

    [System.Serializable]
    public class WaveDefinition
    {
        public SpawnEntry[] entries;
        public float rewardGold = 25;   // dalga sonu bonusu
    }

    /// Bir bölümün tamamı: dalgalar + başlangıç ekonomisi.
    /// 18 bölüm = 18 LevelDefinition asset'i.
    [CreateAssetMenu(menuName = "Golgehalka/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string levelId;          // "act1_level3"
        public string nameKey;          // lokalizasyon anahtarı
        public int startingGold = 200;
        public int startingLives = 20;
        public WaveDefinition[] waves;

        [Header("Harita — her bölümün kendi yolu ve platformları")]
        public Vector3[] waypoints;      // düşman güzergâhı
        public Vector3[] nodePositions;  // kule platformu yerleri

        [Header("Tema — dekor ve atmosfer")]
        public GameObject[] decorPrefabs;               // serpiştirilecek çevre modelleri
        public int decorCount = 10;
        public Color groundColor = new Color(0.25f, 0.32f, 0.22f);

        [Header("Boyanmış harita (Kingdom Rush modu) — varsa zemin/yol/dekor yerine geçer")]
        public Texture2D mapArt;
        public bool mapFlipX;   // Act II ayna varyantı
        public bool mapFlipZ;   // Act III ayna varyantı
    }
}
