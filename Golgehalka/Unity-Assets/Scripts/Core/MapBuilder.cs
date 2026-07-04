using System.Collections.Generic;
using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Haritayı ÇALIŞMA ANINDA bölüm verisinden kurar:
    /// LevelDefinition.waypoints → yol + görsel şeritler,
    /// LevelDefinition.nodePositions → kule platformları.
    /// Böylece 6 bölüm = 6 farklı harita; bölüm seçilince harita anında değişir.
    public class MapBuilder : MonoBehaviour
    {
        public WaypointPath path;        // sahnedeki (boş) yol objesi
        public Renderer groundRenderer;  // bölüm rengine boyanır
        public Texture2D groundTexture;  // çim (CC0 Poly Haven)
        public Texture2D pathTexture;    // toprak (CC0 Poly Haven)

        private readonly List<GameObject> spawned = new List<GameObject>();
        private Material pathMat, nodeMat;

        private void EnsureMaterials()
        {
            if (pathMat != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            pathMat = new Material(shader) { color = new Color(0.9f, 0.82f, 0.7f) };
            if (pathTexture != null)
            {
                pathMat.mainTexture = pathTexture;
                pathMat.mainTextureScale = new Vector2(0.5f, 0.5f);
            }
            nodeMat = new Material(shader) { color = new Color(0.62f, 0.6f, 0.55f) };
        }

        public void BuildFor(LevelDefinition level)
        {
            if (level == null || level.waypoints == null || level.waypoints.Length < 2)
            {
                Debug.LogError("MapBuilder: bölümde harita verisi yok — " +
                               (level != null ? level.levelId : "null"));
                return;
            }
            EnsureMaterials();
            Clear();

            // 1) Waypoint'ler
            var wps = new Transform[level.waypoints.Length];
            for (int i = 0; i < level.waypoints.Length; i++)
            {
                var wp = new GameObject("WP" + i);
                wp.transform.SetParent(path.transform);
                wp.transform.position = level.waypoints[i];
                wps[i] = wp.transform;
                spawned.Add(wp);
            }
            path.SetWaypoints(wps);

            // 2) Yol şeritleri (dokulu) + köşe yumuşatma diskleri
            for (int i = 0; i < level.waypoints.Length - 1; i++)
            {
                Vector3 a = level.waypoints[i], b = level.waypoints[i + 1];
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "PathSeg" + i;
                Destroy(seg.GetComponent<Collider>());
                seg.transform.position = (a + b) / 2f + Vector3.down * 0.05f;
                seg.transform.rotation = Quaternion.LookRotation(b - a);
                seg.transform.localScale = new Vector3(1.3f, 0.05f, (b - a).magnitude);
                seg.GetComponent<Renderer>().sharedMaterial = pathMat;
                seg.transform.SetParent(path.transform);
                spawned.Add(seg);
            }
            for (int i = 1; i < level.waypoints.Length - 1; i++)
            {
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "PathCorner" + i;
                Destroy(cap.GetComponent<Collider>());
                cap.transform.position = level.waypoints[i] + Vector3.down * 0.05f;
                cap.transform.localScale = new Vector3(1.3f, 0.024f, 1.3f);
                cap.GetComponent<Renderer>().sharedMaterial = pathMat;
                cap.transform.SetParent(path.transform);
                spawned.Add(cap);
            }

            // 3) Kule platformları — taş yuva diskleri
            foreach (Vector3 p in level.nodePositions)
            {
                var node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                node.name = "Node";
                node.transform.position = p;
                node.transform.localScale = new Vector3(1.5f, 0.07f, 1.5f);
                node.GetComponent<Renderer>().sharedMaterial = nodeMat;
                // İnce diske dokunmak zor — geniş görünmez tıklama hacmi ekle
                Destroy(node.GetComponent<Collider>());
                var box = node.AddComponent<BoxCollider>();
                box.size = new Vector3(0.95f, 8f, 0.95f);
                node.AddComponent<PlacementNode>();
                spawned.Add(node);
            }

            // 4) Tema: dokulu zemin + bölüm tonu + dekor + orman sınırı
            if (groundRenderer != null)
            {
                var gm = groundRenderer.material; // instance
                if (groundTexture != null)
                {
                    gm.mainTexture = groundTexture;
                    gm.mainTextureScale = new Vector2(9f, 6f);
                }
                gm.color = Color.Lerp(Color.white, level.groundColor, 0.35f);
            }
            ScatterDecor(level);
            PlantBorderForest(level);
        }

        /// Harita kenarını ağaç kuşağıyla sar — "boşlukta yüzen zemin" hissini bitirir.
        private void PlantBorderForest(LevelDefinition level)
        {
            if (level.decorPrefabs == null || level.decorPrefabs.Length == 0) return;
            var trees = level.decorPrefabs[0];
            if (trees == null) return;
            var rng = new System.Random(level.levelId.GetHashCode() * 31 + 7);

            for (int i = 0; i < 26; i++)
            {
                // Dikdörtgen bant: iç saha (±11, ±8) dışı, görüş alanı (±14, ±10) içi
                float x, z;
                if (rng.NextDouble() < 0.5)
                {   // üst/alt bant
                    x = (float)(rng.NextDouble() * 28 - 14);
                    z = (float)(rng.NextDouble() * 2 + 8.2) * (rng.NextDouble() < 0.5 ? 1 : -1);
                }
                else
                {   // sol/sağ bant
                    x = (float)(rng.NextDouble() * 2.6 + 11.2) * (rng.NextDouble() < 0.5 ? 1 : -1);
                    z = (float)(rng.NextDouble() * 20 - 10);
                }
                var deco = Instantiate(trees, new Vector3(x, 0, z),
                    Quaternion.Euler(0, (float)(rng.NextDouble() * 360), 0));
                deco.transform.localScale = Vector3.one * (0.9f + (float)rng.NextDouble() * 0.7f);
                spawned.Add(deco);
            }
        }

        /// Çevre modellerini yoldan/platformlardan uzağa, bölüm başına
        /// DETERMİNİSTİK (levelId tohumlu) şekilde serpiştirir.
        private void ScatterDecor(LevelDefinition level)
        {
            if (level.decorPrefabs == null || level.decorPrefabs.Length == 0) return;
            var rng = new System.Random(level.levelId.GetHashCode());

            int placed = 0, attempts = 0;
            while (placed < level.decorCount && attempts < level.decorCount * 25)
            {
                attempts++;
                // Kenar şeritleri dahil geniş alan — yoğun yollu haritalarda da yer bulunur
                var pos = new Vector3(
                    (float)(rng.NextDouble() * 26f - 13f), 0f,
                    (float)(rng.NextDouble() * 18f - 9f));

                if (DistanceToPath(pos, level.waypoints) < 1.8f) continue;
                bool nearNode = false;
                foreach (Vector3 n in level.nodePositions)
                    if (Vector3.Distance(pos, new Vector3(n.x, 0, n.z)) < 1.9f) { nearNode = true; break; }
                if (nearNode) continue;

                var prefab = level.decorPrefabs[rng.Next(level.decorPrefabs.Length)];
                if (prefab == null) continue;
                var deco = Instantiate(prefab, pos, Quaternion.Euler(0, (float)(rng.NextDouble() * 360), 0));
                float s = 0.8f + (float)rng.NextDouble() * 0.5f; // boyut çeşitliliği
                deco.transform.localScale = Vector3.one * s;
                spawned.Add(deco);
                placed++;
            }
        }

        private static float DistanceToPath(Vector3 p, Vector3[] wps)
        {
            float best = float.MaxValue;
            for (int i = 0; i < wps.Length - 1; i++)
            {
                Vector3 a = new Vector3(wps[i].x, 0, wps[i].z);
                Vector3 b = new Vector3(wps[i + 1].x, 0, wps[i + 1].z);
                Vector3 ab = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector3.Distance(p, a + ab * t));
            }
            return best;
        }

        /// Önceki haritayı ve üzerine kurulmuş kuleleri temizle.
        private void Clear()
        {
            // Kuleler (kayıt listesinin kopyası üzerinden — Destroy listeyi değiştirir)
            foreach (Tower t in new List<Tower>(Tower.Active))
                if (t != null) Destroy(t.gameObject);

            foreach (GameObject go in spawned)
                if (go != null) Destroy(go);
            spawned.Clear();
        }
    }
}
