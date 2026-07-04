using System.Collections.Generic;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Combat
{
    /// Yerleştirilmiş kahraman kulesi: hedef seç, ateş et, kademe yükselt.
    public class Tower : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private TargetPriority priority = TargetPriority.First;

        public HeroDefinition Hero { get; private set; }
        public int TierIndex { get; private set; }
        private TowerTier Tier => Hero.tiers[TierIndex];

        private float cooldown;

        public void Init(HeroDefinition hero)
        {
            Hero = hero;
            TierIndex = 0;
            ApplyTierVisual();
        }

        public bool CanUpgrade => TierIndex < Hero.tiers.Length - 1;
        public int UpgradeCost => CanUpgrade ? Hero.tiers[TierIndex + 1].cost : 0;

        public bool TryUpgrade()
        {
            if (!CanUpgrade || !Core.EconomyManager.Instance.TrySpend(UpgradeCost))
                return false;
            TierIndex++;
            ApplyTierVisual();
            return true;
        }

        private void ApplyTierVisual()
        {
            // Kademeye göre model değişimi — tierModel prefab'ları
            for (int i = 0; i < Hero.tiers.Length; i++)
                if (Hero.tiers[i].tierModel != null)
                    Hero.tiers[i].tierModel.SetActive(i == TierIndex);
        }

        private void Update()
        {
            cooldown -= Time.deltaTime;
            if (cooldown > 0f) return;

            Enemy target = PickTarget();
            if (target == null) return;

            Fire(target);
            cooldown = 1f / Tier.fireRate;
        }

        private Enemy PickTarget()
        {
            // Canlı düşman kaydını gez (Enemy.Active) — sahne taramasından hızlı,
            // Unity API sürüm değişimlerinden bağımsız
            Enemy best = null;
            float bestScore = float.MinValue;
            foreach (Enemy e in Enemy.Active)
            {
                if (!e.IsAlive) continue;
                if (e.Def.isFlying && !Hero.canTargetFlying) continue;
                float dist = Vector3.Distance(transform.position, e.transform.position);
                if (dist > Tier.range) continue;

                float score = priority switch
                {
                    TargetPriority.First     => e.PathProgress,
                    TargetPriority.Last      => -e.PathProgress,
                    TargetPriority.Strongest => e.Health,
                    TargetPriority.Nearest   => -dist,
                    _ => 0f
                };
                if (score > bestScore) { bestScore = score; best = e; }
            }
            return best;
        }

        private void Fire(Enemy target)
        {
            var go = Instantiate(Hero.projectilePrefab,
                firePoint != null ? firePoint.position : transform.position,
                Quaternion.identity);
            go.GetComponent<Projectile>().Init(
                target, Tier.damage, Hero.damageType, Hero.armorPenetration, Hero.projectileSpeed);
        }

        private void OnDrawGizmosSelected()
        {
            if (Hero == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Tier.range);
        }
    }
}
