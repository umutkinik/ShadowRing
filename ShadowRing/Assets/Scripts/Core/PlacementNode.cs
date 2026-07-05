using Golgehalka.Combat;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Kule yerleştirme noktası — haritadaki uygun slotlar.
    /// NOT: Unity, sahneye kaydedilen MonoBehaviour'ı yalnızca dosya adıyla aynı
    /// isimli sınıftan çözebilir — bu sınıf bu yüzden KENDİ dosyasında olmalı.
    public class PlacementNode : MonoBehaviour
    {
        public Tower Occupant { get; set; }
        public bool IsEmpty => Occupant == null;

        /// Boş yuva görseli (taş platform modeli) — kule kurulunca gizlenir.
        public GameObject visual;

        public void SetVisualVisible(bool v)
        {
            if (visual != null) { visual.SetActive(v); return; }
            var r = GetComponent<Renderer>();     // eski disk kurulumuna geriye uyum
            if (r != null) r.enabled = v;
        }
    }
}
