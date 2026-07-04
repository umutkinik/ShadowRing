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

        private readonly List<GameObject> spawned = new List<GameObject>();
        private Material pathMat, nodeMat;

        private void EnsureMaterials()
        {
            if (pathMat != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            pathMat = new Material(shader) { color = new Color(0.45f, 0.38f, 0.28f) };
            nodeMat = new Material(shader) { color = new Color(0.55f, 0.5f, 0.4f) };
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

            // 2) Yol şeritleri (görsel)
            for (int i = 0; i < level.waypoints.Length - 1; i++)
            {
                Vector3 a = level.waypoints[i], b = level.waypoints[i + 1];
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "PathSeg" + i;
                Destroy(seg.GetComponent<Collider>());
                seg.transform.position = (a + b) / 2f + Vector3.down * 0.05f;
                seg.transform.rotation = Quaternion.LookRotation(b - a);
                seg.transform.localScale = new Vector3(1.2f, 0.05f, (b - a).magnitude + 1.2f);
                seg.GetComponent<Renderer>().sharedMaterial = pathMat;
                seg.transform.SetParent(path.transform);
                spawned.Add(seg);
            }

            // 3) Kule platformları
            foreach (Vector3 p in level.nodePositions)
            {
                var node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.name = "Node";
                node.transform.position = p;
                node.transform.localScale = new Vector3(1.4f, 0.3f, 1.4f);
                node.GetComponent<Renderer>().sharedMaterial = nodeMat;
                node.AddComponent<PlacementNode>();
                spawned.Add(node);
            }

            // 4) Tema: zemin rengi + dekor serpme
            if (groundRenderer != null)
            {
                groundRenderer.material.color = level.groundColor; // instance materyal
            }
            ScatterDecor(level);
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
