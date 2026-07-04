using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Combat
{
    /// Hedef takipli mermi. Faz 1'de object pooling'e taşınacak.
    public class Projectile : MonoBehaviour
    {
        private Enemy target;
        private float damage;
        private DamageType type;
        private float armorPen;
        private float speed;

        public void Init(Enemy target, float damage, DamageType type, float armorPen, float speed)
        {
            this.target = target;
            this.damage = damage;
            this.type = type;
            this.armorPen = armorPen;
            this.speed = speed;
        }

        private void Update()
        {
            // Hedef öldü/yok olduysa mermiyi temizle
            if (target == null || !target.IsAlive) { Destroy(gameObject); return; }

            Vector3 dest = target.transform.position + Vector3.up * 0.5f;
            transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            transform.LookAt(dest);

            if ((transform.position - dest).sqrMagnitude < 0.04f)
            {
                target.TakeDamage(damage, type, armorPen);
                Destroy(gameObject);
            }
        }
    }
}
