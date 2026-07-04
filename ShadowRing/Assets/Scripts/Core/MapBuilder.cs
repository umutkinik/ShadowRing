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
        public WaypointPath path;   // sahnedeki (boş) yol objesi

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
