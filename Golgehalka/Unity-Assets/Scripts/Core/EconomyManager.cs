using System;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Maç içi altın ekonomisi. Meta para birimleri (parça/shard vb.) burada DEĞİL —
    /// onlar kalıcı profil sisteminde tutulur.
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingGold = 200;

        public int Gold { get; private set; }
        public event Action<int> OnGoldChanged;

        private void Awake()
        {
            // Sahne-yerel singleton: "son gelen kazanır" (bkz. GameManager notu).
            Instance = this;
            Gold = startingGold;
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        /// Yeterli altın varsa harcar ve true döner; UI "yetersiz altın" mesajını
        /// false dönüşünde gösterir (shop.not_enough_gold).
        public bool TrySpend(int cost)
        {
            if (Gold < cost) return false;
            Gold -= cost;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }
    }
}
