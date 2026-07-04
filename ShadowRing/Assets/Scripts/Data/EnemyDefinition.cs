using UnityEngine;

namespace Golgehalka.Data
{
    /// Bir düşman tipinin verisi (Boşluk Yavrusu, Kanpençe, Gök Dehşeti...).
    /// Designer ScriptableObject olarak oluşturur; balans kod değişikliği gerektirmez.
    [CreateAssetMenu(menuName = "Golgehalka/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Kimlik")]
        public string enemyId;              // "void_spawn", "bloodclaw"...
        public string nameKey;              // lokalizasyon anahtarı (enemy.void_spawn.name)

        [Header("Savaş")]
        public float maxHealth = 50f;
        [Range(0f, 0.9f)] public float armor;   // fiziksel hasar azaltımı (0.3 = %30)
        public float moveSpeed = 2f;
        public bool isFlying;               // sadece hava vurabilen kuleler hedefler
        public bool isBoss;

        [Header("Ödül / Ceza")]
        public int goldReward = 5;
        public int livesCost = 1;           // yol sonuna ulaşırsa kaybedilen can

        [Header("Görsel")]
        public GameObject prefab;           // Meshy'den gelen model prefab'ı
    }
}
