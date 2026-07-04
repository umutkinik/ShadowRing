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
    }
}
