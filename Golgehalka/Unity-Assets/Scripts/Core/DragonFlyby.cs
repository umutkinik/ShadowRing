using System.Collections;
using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Ortam gösterisi: rastgele aralıklarla bir Gök Dehşeti haritanın üzerinden
    /// uçar, altındaki düşmanlara alev yağdırır. Hem sahneyi canlandırır hem
    /// oyuncuya küçük bir "müttefik" sürprizi verir.
    public class DragonFlyby : MonoBehaviour
    {
        public GameObject dragonPrefab;
        public float minInterval = 35f;
        public float maxInterval = 75f;
        public float flightHeight = 4.5f;
        public float speed = 9f;
        public float fireDamage = 30f;

        private float timer;
        private bool flying;
        private Material flameMat;

        private void Start() => ResetTimer();

        private void ResetTimer() =>
            timer = Random.Range(minInterval, maxInterval);

        private void Update()
        {
            if (flying || dragonPrefab == null) return;
            // Yalnızca savaş sırasında — hazırlıkta gökyüzü sakin
            if (GameManager.Instance.State != GameState.WaveActive) return;

            timer -= Time.deltaTime;
            if (timer <= 0f) StartCoroutine(Fly());
        }

        private IEnumerator Fly()
        {
            flying = true;
            bool leftToRight = Random.value < 0.5f;
            float z = Random.Range(-5f, 6f);
            Vector3 from = new Vector3(leftToRight ? -17f : 17f, flightHeight, z);
            Vector3 to = new Vector3(-from.x, flightHeight, z + Random.Range(-2f, 2f));

            var dragon = Instantiate(dragonPrefab, from, Quaternion.LookRotation(to - from));
            AudioManager.Flame();

            float fireTick = 0f;
            while ((dragon.transform.position - to).sqrMagnitude > 1f)
            {
                dragon.transform.position = Vector3.MoveTowards(
                    dragon.transform.position, to, speed * Time.deltaTime);
                // Hafif süzülme
                dragon.transform.position += Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.01f);

                fireTick -= Time.deltaTime;
                if (fireTick <= 0f)
                {
                    fireTick = 0.22f;
                    BreatheFire(dragon.transform.position);
                }
                yield return null;
            }
            Destroy(dragon);
            flying = false;
            ResetTimer();
        }

        /// Ejderhanın altındaki şeride alev: yakındaki düşmanlar ateş hasarı alır.
        private void BreatheFire(Vector3 dragonPos)
        {
            Vector3 groundPos = new Vector3(dragonPos.x, 0.15f, dragonPos.z);

            foreach (Enemy e in new System.Collections.Generic.List<Enemy>(Enemy.Active))
            {
                if (e == null || !e.IsAlive) continue;
                Vector3 d = e.transform.position - groundPos; d.y = 0;
                if (d.sqrMagnitude < 2.1f * 2.1f)
                    e.TakeDamage(fireDamage, DamageType.Fire);
            }
            StartCoroutine(FlamePuff(groundPos));
        }

        private IEnumerator FlamePuff(Vector3 pos)
        {
            if (flameMat == null)
            {
                flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                { color = new Color(1f, 0.55f, 0.15f) };
                flameMat.EnableKeyword("_EMISSION");
                flameMat.SetColor("_EmissionColor", new Color(1.2f, 0.45f, 0.1f));
            }
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(puff.GetComponent<Collider>());
            puff.GetComponent<Renderer>().sharedMaterial = flameMat;
            puff.transform.position = pos;

            for (float t = 0; t < 0.45f; t += Time.deltaTime)
            {
                float k = t / 0.45f;
                puff.transform.localScale = Vector3.one * Mathf.Lerp(1.4f, 0.1f, k);
                yield return null;
            }
            Destroy(puff);
        }
    }
}
