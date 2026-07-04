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

        public static event Action<Enemy> OnAnyEnemyDied; // WaveManager sayaç tutar

        /// Sahnedeki canlı düşman kaydı — kuleler sahne taraması yerine bunu gezer
        /// (FindObjectsByType'tan hem hızlı hem Unity sürüm-bağımsız).
        public static readonly List<Enemy> Active = new List<Enemy>();

        private WaypointPath path;
        private int waypointIndex;

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        public void Init(EnemyDefinition def, WaypointPath followPath)
        {
            Def = def;
            Health = def.maxHealth;
            path = followPath;
            waypointIndex = 0;
            transform.position = path.GetPoint(0);
        }

        private void Update()
        {
            if (!IsAlive || path == null) return;
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            Vector3 target = path.GetPoint(waypointIndex);
            transform.position = Vector3.MoveTowards(
                transform.position, target, Def.moveSpeed * Time.deltaTime);
            transform.LookAt(target);

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
            OnAnyEnemyDied?.Invoke(this);
            // TODO: ölüm animasyonu + havuz (object pooling) — Faz 1
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
