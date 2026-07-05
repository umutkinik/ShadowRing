using UnityEngine;

namespace Golgehalka.Data
{
    public enum DamageType { Physical, Piercing, Magic, Poison, Fire }
    public enum TargetPriority { First, Last, Strongest, Nearest }

    /// Kahramana özgü mermi görünümü — okçu ok atar, büyücü büyü, cüce balta!
    public enum ProjectileStyle { Orb, Arrow, SpinningAxe, MagicBolt, PoisonOrb }

    /// Bir kule kademesinin istatistikleri (maç içi 3 kademe yükseltme).
    [System.Serializable]
    public class TowerTier
    {
        public int cost = 100;
        public float damage = 10f;
        public float range = 4f;
        public float fireRate = 1f;         // saniyedeki atış
        public GameObject tierModel;        // kademeye göre görsel değişim
    }

    /// Bir kahramanın (kule sınıfının) verisi — Kael, Faelyn, Borin...
    [CreateAssetMenu(menuName = "Golgehalka/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        [Header("Kimlik")]
        public string heroId;               // "borin", "faelyn"...
        public string nameKey;              // lokalizasyon anahtarı (hero.borin.name)
        public Sprite icon;

        [Header("Savaş")]
        public DamageType damageType = DamageType.Physical;
        public bool canTargetFlying;
        [Range(0f, 1f)] public float armorPenetration;  // Borin: yüksek, Faelyn: düşük
        public TowerTier[] tiers = new TowerTier[3];    // kademe 1-2-3

        [Header("Mermi")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 12f;
        public ProjectileStyle projectileStyle = ProjectileStyle.Orb;
        public Color projectileColor = new Color(1f, 0.85f, 0.3f);

        [Header("Görsel")]
        public GameObject towerPrefab;      // kahramana özel kule prefab'ı (modeliyle)
    }
}
