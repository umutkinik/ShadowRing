using Golgehalka.Core;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Combat
{
    /// Hedef takipli mermi — kahraman kimliğine göre şekillenir:
    /// ok, dönen balta, büyü ışını, zehir topu ya da klasik küre.
    public class Projectile : MonoBehaviour
    {
        private Enemy target;
        private float damage;
        private DamageType type;
        private float armorPen;
        private float speed;
        private Color color = new Color(1f, 0.85f, 0.3f);
        private ProjectileStyle style = ProjectileStyle.Orb;
        private Transform spinner;      // dönen balta görseli

        public void Init(Enemy target, float damage, DamageType type, float armorPen, float speed,
            ProjectileStyle style = ProjectileStyle.Orb, Color? color = null)
        {
            this.target = target;
            this.damage = damage;
            this.type = type;
            this.armorPen = armorPen;
            this.speed = speed;
            this.style = style;
            if (color.HasValue) this.color = color.Value;
            BuildVisual();
        }

        /// Stil görselini çalışma anında kur — ek asset gerekmez.
        private void BuildVisual()
        {
            var baseRenderer = GetComponent<Renderer>();

            switch (style)
            {
                case ProjectileStyle.Arrow:
                    // İnce şaft + koyu uç; LookAt zaten uçuş yönüne çevirir
                    if (baseRenderer != null) baseRenderer.enabled = false;
                    var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(shaft.GetComponent<Collider>());
                    shaft.transform.SetParent(transform, false);
                    shaft.transform.localScale = new Vector3(0.06f, 0.06f, 0.55f);
                    Tint(shaft, color);
                    var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(tip.GetComponent<Collider>());
                    tip.transform.SetParent(transform, false);
                    tip.transform.localPosition = new Vector3(0, 0, 0.3f);
                    tip.transform.localScale = new Vector3(0.1f, 0.1f, 0.12f);
                    Tint(tip, new Color(0.35f, 0.35f, 0.4f));
                    break;

                case ProjectileStyle.SpinningAxe:
                    if (baseRenderer != null) baseRenderer.enabled = false;
                    var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(blade.GetComponent<Collider>());
                    blade.transform.SetParent(transform, false);
                    blade.transform.localScale = new Vector3(0.38f, 0.05f, 0.3f);
                    Tint(blade, color);
                    spinner = blade.transform;
                    break;

                case ProjectileStyle.MagicBolt:
                    TintSelf(baseRenderer, color, emissive: true);
                    transform.localScale = Vector3.one * 0.3f;
                    VFX.Trail(transform, color, 45f, 0.35f, 0.14f); // büyü izi
                    break;

                case ProjectileStyle.PoisonOrb:
                    TintSelf(baseRenderer, color, emissive: true);
                    transform.localScale = Vector3.one * 0.24f;
                    VFX.Trail(transform, new Color(color.r, color.g, color.b, 0.7f), 30f, 0.5f, 0.1f);
                    break;

                default: // Orb
                    TintSelf(baseRenderer, color, emissive: false);
                    break;
            }
        }

        private static void Tint(GameObject go, Color c)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = c };
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        private void TintSelf(Renderer r, Color c, bool emissive)
        {
            if (r == null) return;
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = c };
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * 1.6f);
            }
            r.material = m;
        }

        private void Update()
        {
            // Hedef öldü/yok olduysa mermiyi temizle
            if (target == null || !target.IsAlive) { Destroy(gameObject); return; }

            Vector3 dest = target.transform.position + Vector3.up * 0.5f;
            transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            transform.LookAt(dest);

            if (spinner != null)
                spinner.Rotate(0, 720f * Time.deltaTime, 0, Space.Self); // balta çarkı

            if ((transform.position - dest).sqrMagnitude < 0.04f)
            {
                target.TakeDamage(damage, type, armorPen);
                // Çarpma efekti mermi renginde
                VFX.Burst(dest, color, 8, 2.4f, 0.14f, 0.28f);
                AudioManager.Hit();
                Destroy(gameObject);
            }
        }
    }
}
