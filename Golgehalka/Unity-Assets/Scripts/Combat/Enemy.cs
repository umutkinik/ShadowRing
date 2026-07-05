using System;
using System.Collections.Generic;
using Golgehalka.Core;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Combat
{
    /// Yol takip eden düşman: can, zırh, ölüm ödülü, leak cezası.
    public class Enemy : MonoBehaviour
    {
        public EnemyDefinition Def { get; private set; }
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;

        /// Yol üzerindeki ilerleme — "First/Last" hedefleme için kullanılır.
        public float PathProgress { get; private set; }

        /// Hız çarpanı — yavaşlatma etkileri (deprem, Sylwen aurası vb.) için.
        public float SpeedFactor { get; set; } = 1f;

        public static event Action<Enemy> OnAnyEnemyDied; // WaveManager sayaç tutar

        /// Sahnedeki canlı düşman kaydı — kuleler sahne taraması yerine bunu gezer
        /// (FindObjectsByType'tan hem hızlı hem Unity sürüm-bağımsız).
        public static readonly List<Enemy> Active = new List<Enemy>();

        private WaypointPath path;
        private int waypointIndex;
        private Transform model;        // prosedürel yürüyüş sallanması için
        private float bobPhase;
        private float stepTimer;        // ağır tepinme: adım toz bulutu zamanlayıcısı
        private Animation walkAnim;     // Meshy rig'inden gelen gerçek yürüyüş (varsa)

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        private float modelBaseY; // otomatik zemin hizalamasından gelen taban yükseklik

        private void Awake()
        {
            model = transform.Find("Model");
            if (model != null) modelBaseY = model.localPosition.y;
            bobPhase = UnityEngine.Random.Range(0f, 6.28f); // sürüde senkron kırıcı rastgele faz
        }

        public void Init(EnemyDefinition def, WaypointPath followPath)
        {
            Def = def;
            Health = def.maxHealth;
            path = followPath;
            waypointIndex = 0;
            transform.position = path.GetPoint(0) + FlyOffset();

            // Rigli model: yürüyüşü hıza oranla oynat — yavaş dev = ağır adımlar
            walkAnim = GetComponentInChildren<Animation>();
            if (walkAnim != null && walkAnim.GetClipCount() > 0)
            {
                float pace = Mathf.Clamp(def.moveSpeed / 2.2f, 0.5f, 1.3f)
                             * UnityEngine.Random.Range(0.95f, 1.1f);
                foreach (AnimationState st in walkAnim)
                {
                    st.wrapMode = WrapMode.Loop;
                    st.speed = pace;
                    walkAnim.Play(st.name);
                    break;
                }
            }
            else walkAnim = null;
        }

        /// Uçan birimler yolun 1.5 birim üzerinde süzülür.
        private Vector3 FlyOffset() =>
            Def != null && Def.isFlying ? Vector3.up * 1.5f : Vector3.zero;

        private void Update()
        {
            if (!IsAlive || path == null) return;
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            Vector3 target = path.GetPoint(waypointIndex) + FlyOffset();
            transform.position = Vector3.MoveTowards(
                transform.position, target, Def.moveSpeed * SpeedFactor * Time.deltaTime);
            transform.LookAt(target);

            // Prosedürel yürüyüş — YALNIZCA iskelet animasyonu olmayanlarda.
            // Ağır boss'lar (yavaş devler) tepinerek yürür: derin salınım + adım tozu.
            if (walkAnim == null && model != null)
            {
                bool heavy = Def.isBoss && Def.moveSpeed < 1.3f;
                float rate = heavy ? 2.4f : 4f + Def.moveSpeed * 2.5f;
                float t = Time.time * rate + bobPhase;

                model.localRotation = Quaternion.Euler(
                    heavy ? Mathf.Sin(t * 0.5f) * 3f : 0f, 0f,
                    Mathf.Sin(t) * (heavy ? 4.5f : 6f));
                var lp = model.localPosition;
                lp.y = modelBaseY + Mathf.Abs(Mathf.Sin(t)) * (heavy ? 0.13f : 0.07f);
                model.localPosition = lp;

                if (heavy)
                {
                    stepTimer -= Time.deltaTime;
                    if (stepTimer <= 0f)
                    {
                        stepTimer = Mathf.PI / rate; // her yarım salınım = bir adım
                        Vector3 foot = transform.position; foot.y = 0.1f;
                        VFX.Burst(foot, new Color(0.55f, 0.5f, 0.4f, 0.8f), 9, 1.3f, 0.35f, 0.6f, -0.05f);
                    }
                }
            }

            if ((transform.position - target).sqrMagnitude < 0.01f)
            {
                waypointIndex++;
                PathProgress = (float)waypointIndex / path.Count;
                if (waypointIndex >= path.Count) ReachEnd();
            }
        }

        /// Hasar uygula. Zırh yalnızca fiziksel hasarı azaltır;
        /// Piercing zırhın bir kısmını yok sayar (armorPen).
        public void TakeDamage(float amount, DamageType type, float armorPen = 0f)
        {
            if (!IsAlive) return;
            float effectiveArmor = type == DamageType.Physical || type == DamageType.Piercing
                ? Def.armor * (1f - armorPen)
                : 0f; // büyü/zehir/ateş zırhı deler
            Health -= amount * (1f - effectiveArmor);
            if (Health <= 0f) Die();
        }

        private void Die()
        {
            EconomyManager.Instance.AddGold(Def.goldReward);
            // Boşluk enerjisi dağılması + koyu "pof" sesi + altın çınlaması
            VFX.Burst(transform.position + Vector3.up * 0.5f,
                new Color(0.55f, 0.2f, 0.7f), 20, 3.4f, 0.3f, 0.55f);
            AudioManager.Die();
            AudioManager.Coin();
            OnAnyEnemyDied?.Invoke(this);
            StartCoroutine(DeathShrink());
        }

        private System.Collections.IEnumerator DeathShrink()
        {
            Vector3 start = transform.localScale;
            for (float t = 0; t < 0.18f; t += Time.deltaTime)
            {
                transform.localScale = start * (1f - t / 0.18f);
                yield return null;
            }
            Destroy(gameObject);
        }

        private void ReachEnd()
        {
            Health = 0f;
            GameManager.Instance.LoseLife(Def.livesCost);
            OnAnyEnemyDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
