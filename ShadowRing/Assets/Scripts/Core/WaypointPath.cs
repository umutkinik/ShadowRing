using UnityEngine;

namespace Golgehalka.Core
{
    /// Düşman yürüyüş yolu: sıralı waypoint'ler.
    /// Faz 0'da elle yerleştirilmiş boş objeler; ileride seviye editörü üretir.
    public class WaypointPath : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        public int Count => waypoints.Length;
        public Vector3 GetPoint(int index) => waypoints[index].position;

        /// MapBuilder çalışma anında yeni güzergâh atar (bölüm değişimi).
        public void SetWaypoints(Transform[] newWaypoints) => waypoints = newWaypoints;

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] == null || waypoints[i + 1] == null) continue;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
