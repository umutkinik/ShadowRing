using System.Collections;
using System.Collections.Generic;
using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Rastgele ortam olayları — doğa da Boşluk'a karşı savaşır:
    ///   🐉 Ejderha geçişi (alev şeridi)
    ///   ⚡ Yıldırım (tek noktaya büyük hasar)
    ///   🌋 Ateş yağmuru (alan bombardımanı)
    ///   🌍 Deprem (tüm düşmanlar yavaşlar + kamera sarsılır)
    /// Yalnızca dalga aktifken tetiklenir; oyuncunun lehine çalışır.
    public class AmbientEvents : MonoBehaviour
    {
        public GameObject dragonPrefab;
        public float minInterval = 28f;
        public float maxInterval = 55f;

        private float timer;
        private bool busy;
        private Material flameMat, boltMat, dustMat;

        private void Start() => timer = Random.Range(minInterval, maxInterval);

        private void Update()
        {
            if (busy) return;
            if (GameManager.Instance.State != GameState.WaveActive) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            float roll = Random.value;
            if (roll < 0.35f && dragonPrefab != null) StartCoroutine(Run(DragonPass()));
            else if (roll < 0.60f) StartCoroutine(Run(LightningStrike()));
            else if (roll < 0.85f) StartCoroutine(Run(FireRain()));
            else StartCoroutine(Run(Earthquake()));
        }

        private IEnumerator Run(IEnumerator ev)
        {
            busy = true;
            yield return StartCoroutine(ev);
            busy = false;
            timer = Random.Range(minInterval, maxInterval);
        }

        // ---------- 🐉 EJDERHA ----------
        private IEnumerator DragonPass()
        {
            bool ltr = Random.value < 0.5f;
            float z = Random.Range(-5f, 6f);
            Vector3 from = new Vector3(ltr ? -17f : 17f, 4.5f, z);
            Vector3 to = new Vector3(-from.x, 4.5f, z + Random.Range(-2f, 2f));

            var dragon = Instantiate(dragonPrefab, from, Quaternion.LookRotation(to - from));
            AudioManager.Flame();

            float fireTick = 0f;
            while ((dragon.transform.position - to).sqrMagnitude > 1f)
            {
                dragon.transform.position = Vector3.MoveTowards(
                    dragon.transform.position, to, 9f * Time.deltaTime);
                fireTick -= Time.deltaTime;
                if (fireTick <= 0f)
                {
                    fireTick = 0.22f;
                    Vector3 g = dragon.transform.position; g.y = 0.15f;
                    DamageArea(g, 2.1f, 30f, DamageType.Fire);
                    StartCoroutine(Puff(g, FlameMat(), 1.4f, 0.45f));
                }
                yield return null;
            }
            Destroy(dragon);
        }

        // ---------- ⚡ YILDIRIM ----------
        private IEnumerator LightningStrike()
        {
            // Hedef: rastgele canlı düşman (yoksa olay iptal)
            Enemy target = null;
            if (Enemy.Active.Count > 0)
                target = Enemy.Active[Random.Range(0, Enemy.Active.Count)];
            if (target == null || !target.IsAlive) yield break;

            Vector3 hit = target.transform.position; hit.y = 0.1f;
            AudioManager.Thunder();

            // Gökten inen parlak sütun + flaş ışığı — iki kez titreşir
            for (int flash = 0; flash < 2; flash++)
            {
                var bolt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(bolt.GetComponent<Collider>());
                bolt.GetComponent<Renderer>().sharedMaterial = BoltMat();
                bolt.transform.position = hit + Vector3.up * 5f;
                bolt.transform.localScale = new Vector3(0.22f - flash * 0.08f, 5f, 0.22f - flash * 0.08f);

                var lightGO = new GameObject("Flash");
                var l = lightGO.AddComponent<Light>();
                l.type = UnityEngine.LightType.Point;
                l.color = new Color(0.8f, 0.85f, 1f);
                l.intensity = 9f; l.range = 14f;
                lightGO.transform.position = hit + Vector3.up * 2.5f;

                yield return new WaitForSeconds(0.07f);
                Destroy(bolt); Destroy(lightGO);
                yield return new WaitForSeconds(0.05f);
            }

            DamageArea(hit, 1.6f, 90f, DamageType.Magic); // zırh işlemez
            StartCoroutine(Puff(hit, BoltMat(), 1.2f, 0.3f));
        }

        // ---------- 🌋 ATEŞ YAĞMURU ----------
        private IEnumerator FireRain()
        {
            int meteors = Random.Range(6, 10);
            for (int i = 0; i < meteors; i++)
            {
                // Düşman varsa yakınına, yoksa rastgele noktaya
                Vector3 c;
                if (Enemy.Active.Count > 0)
                {
                    var e = Enemy.Active[Random.Range(0, Enemy.Active.Count)];
                    c = (e != null ? e.transform.position : Vector3.zero) +
                        new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                }
                else c = new Vector3(Random.Range(-9f, 9f), 0, Random.Range(-6f, 6f));
                c.y = 0.12f;

                StartCoroutine(MeteorFall(c));
                if (i % 2 == 0) AudioManager.Flame();
                yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));
            }
            yield return new WaitForSeconds(0.9f); // son çarpmalar tamamlansın
        }

        private IEnumerator MeteorFall(Vector3 impact)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(rock.GetComponent<Collider>());
            rock.GetComponent<Renderer>().sharedMaterial = FlameMat();
            rock.transform.localScale = Vector3.one * 0.45f;
            Vector3 start = impact + new Vector3(1.5f, 11f, 1f);
            rock.transform.position = start;

            for (float t = 0; t < 1f; t += Time.deltaTime * 2.4f)
            {
                rock.transform.position = Vector3.Lerp(start, impact, t);
                yield return null;
            }
            Destroy(rock);
            DamageArea(impact, 1.8f, 25f, DamageType.Fire);
            StartCoroutine(Puff(impact, FlameMat(), 1.5f, 0.4f));
        }

        // ---------- 🌍 DEPREM ----------
        private IEnumerator Earthquake()
        {
            AudioManager.Quake();

            // Tüm canlı düşmanlar 3 sn %65 yavaşlar
            var slowed = new List<Enemy>();
            foreach (Enemy e in Enemy.Active)
                if (e != null && e.IsAlive) { e.SpeedFactor = 0.35f; slowed.Add(e); }

            // Toz bulutları
            foreach (Enemy e in slowed)
                if (Random.value < 0.5f)
                    StartCoroutine(Puff(e.transform.position + Vector3.down * 0.2f, DustMat(), 1.1f, 0.6f));

            // Kamera sarsıntısı
            var cam = Camera.main.transform;
            Vector3 basePos = cam.position;
            for (float t = 0; t < 1.4f; t += Time.deltaTime)
            {
                float k = 1f - t / 1.4f; // sönümlenen şiddet
                cam.position = basePos + new Vector3(
                    (Random.value - 0.5f) * 0.35f * k, 0, (Random.value - 0.5f) * 0.35f * k);
                yield return null;
            }
            cam.position = basePos;

            yield return new WaitForSeconds(1.6f);
            foreach (Enemy e in slowed)
                if (e != null) e.SpeedFactor = 1f;
        }

        // ---------- ortak yardımcılar ----------
        private void DamageArea(Vector3 center, float radius, float damage, DamageType type)
        {
            foreach (Enemy e in new List<Enemy>(Enemy.Active))
            {
                if (e == null || !e.IsAlive) continue;
                Vector3 d = e.transform.position - center; d.y = 0;
                if (d.sqrMagnitude < radius * radius)
                    e.TakeDamage(damage, type);
            }
        }

        private IEnumerator Puff(Vector3 pos, Material mat, float size, float life)
        {
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(puff.GetComponent<Collider>());
            puff.GetComponent<Renderer>().sharedMaterial = mat;
            puff.transform.position = pos;
            for (float t = 0; t < life; t += Time.deltaTime)
            {
                puff.transform.localScale = Vector3.one * Mathf.Lerp(size, 0.05f, t / life);
                yield return null;
            }
            Destroy(puff);
        }

        private Material FlameMat() => flameMat != null ? flameMat : flameMat = Emissive(new Color(1f, 0.55f, 0.15f), new Color(1.2f, 0.45f, 0.1f));
        private Material BoltMat() => boltMat != null ? boltMat : boltMat = Emissive(new Color(0.85f, 0.9f, 1f), new Color(1.4f, 1.5f, 1.8f));
        private Material DustMat() => dustMat != null ? dustMat : dustMat = Emissive(new Color(0.6f, 0.55f, 0.45f), Color.black);

        private static Material Emissive(Color baseCol, Color emission)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = baseCol };
            if (emission != Color.black)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission);
            }
            return m;
        }
    }
}
