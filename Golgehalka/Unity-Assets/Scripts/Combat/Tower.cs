using System.Collections.Generic;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Combat
{
    /// Yerleştirilmiş kahraman kulesi: hedef seç, ateş et, kademe yükselt.
    public class Tower : MonoBehaviour
    {
        /// Sahnedeki kurulu kuleler — harita yeniden kurulurken temizlik için.
        public static readonly System.Collections.Generic.List<Tower> Active =
            new System.Collections.Generic.List<Tower>();

        [SerializeField] private Transform firePoint;
        [SerializeField] private TargetPriority priority = TargetPriority.First;
        [SerializeField] private GameObject[] tierVisuals; // K1/K2/K3 platform modelleri
        [SerializeField] private Transform heroModel;      // atışta hedefe döner

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        public HeroDefinition Hero { get; private set; }
        public int TierIndex { get; private set; }
        public TowerTier CurrentTier => Hero.tiers[TierIndex];
        private TowerTier Tier => Hero.tiers[TierIndex];

        /// Kurulduğu platform — satışta boşaltmak için.
        public Core.PlacementNode Node { get; set; }
        /// Bu kuleye harcanan toplam altın (satış iadesi %60 bunun üzerinden).
        public int TotalSpent { get; private set; }

        private float cooldown;

        public void Init(HeroDefinition hero)
        {
            Hero = hero;
            TierIndex = 0;
            TotalSpent = hero.tiers[0].cost;
            ApplyTierVisual();
        }

        public bool CanUpgrade => TierIndex < Hero.tiers.Length - 1;
        public int UpgradeCost => CanUpgrade ? Hero.tiers[TierIndex + 1].cost : 0;
        public int SellRefund => Mathf.RoundToInt(TotalSpent * 0.6f); // balance-v1.md

        public bool TryUpgrade()
        {
            if (!CanUpgrade) return false;
            int cost = UpgradeCost;
            if (!Core.EconomyManager.Instance.TrySpend(cost)) return false;
            TotalSpent += cost;
            TierIndex++;
            ApplyTierVisual();
            Core.AudioManager.Upgrade();
            return true;
        }

        /// Kuleyi sat: %60 iade + platform boşalır (taş görünümü geri gelir).
        public void Sell()
        {
            Core.EconomyManager.Instance.AddGold(SellRefund);
            if (Node != null)
            {
                Node.Occupant = null;
                var r = Node.GetComponent<Renderer>();
                if (r != null) r.enabled = true;
            }
            Destroy(gameObject);
        }

        private void ApplyTierVisual()
        {
            // Kademe platformu değişimi: K1 taş → K2 bronz → K3 altın rünlü
            if (tierVisuals == null) return;
            for (int i = 0; i < tierVisuals.Length; i++)
                if (tierVisuals[i] != null)
                    tierVisuals[i].SetActive(i == TierIndex);
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
            // Kahraman hedefe döner + kısa "atış tepkisi" (scale punch)
            if (heroModel != null)
            {
                Vector3 dir = target.transform.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                    heroModel.rotation = Quaternion.LookRotation(dir);
                StopAllCoroutines();
                StartCoroutine(AttackPunch());
            }

            var go = Instantiate(Hero.projectilePrefab,
                firePoint != null ? firePoint.position : transform.position,
                Quaternion.identity);
            go.GetComponent<Projectile>().Init(
                target, Tier.damage, Hero.damageType, Hero.armorPenetration, Hero.projectileSpeed);
            Core.AudioManager.Arrow();
        }

        private System.Collections.IEnumerator AttackPunch()
        {
            Vector3 baseScale = heroModel.localScale;
            for (float t = 0; t < 0.14f; t += Time.deltaTime)
            {
                float k = 1f + Mathf.Sin(t / 0.14f * Mathf.PI) * 0.1f;
                heroModel.localScale = baseScale * k;
                yield return null;
            }
            heroModel.localScale = baseScale;
        }

        private void OnDrawGizmosSelected()
        {
            if (Hero == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Tier.range);
        }
    }
}
