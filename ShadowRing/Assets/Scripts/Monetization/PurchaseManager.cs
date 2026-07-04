using System;
using UnityEngine;

namespace Golgehalka.Monetization
{
    /// "Kahve parası" destekçi modeli — mağazalar gerçek "istediğin kadar öde"
    /// desteklemediği için 3 sabit kademe sunulur; HEPSİ aynı hakkı açar: reklamsız oyun.
    /// Kullanıcı ne ödemek istiyorsa o kademeyi seçer.
    ///
    /// Kurulum: Package Manager → In App Purchasing (com.unity.purchasing) kur,
    /// aşağıdaki ürün kimliklerini App Store Connect + Google Play Console'da
    /// NON-CONSUMABLE olarak tanımla, sonra UNITY_PURCHASING ile derle.
    public class PurchaseManager : MonoBehaviour
    {
        public static PurchaseManager Instance { get; private set; }

        // Mağaza ürün kimlikleri — üç kademe, tek hak (no_ads)
        public const string SkuCoffeeSmall = "com.golgehalka.supporter.coffee";       // ~0.99 USD
        public const string SkuCoffeeLarge = "com.golgehalka.supporter.coffee_large"; // ~2.99 USD
        public const string SkuCoffeeFeast = "com.golgehalka.supporter.coffee_cake";  // ~4.99 USD

        private const string NoAdsKey = "golgehalka_no_ads";

        /// Reklamsız hakkı — AdsManager her gösterimden önce bunu kontrol eder.
        public bool AdsRemoved
        {
            get { return PlayerPrefs.GetInt(NoAdsKey, 0) == 1; }
            private set { PlayerPrefs.SetInt(NoAdsKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public event Action OnAdsRemoved; // UI "teşekkürler" ekranı + reklam katmanını kapat

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStore();
        }

        private void InitializeStore()
        {
#if UNITY_PURCHASING
            // Unity IAP init: üç SKU'yu NonConsumable olarak kaydet.
            // ConfigurationBuilder + IStoreListener implementasyonu buraya —
            // ProcessPurchase içinde hangi SKU gelirse gelsin GrantNoAds() çağrılır.
#endif
        }

        /// Mağaza ekranındaki üç butondan biri çağırır.
        public void BuySupporter(string sku)
        {
#if UNITY_PURCHASING
            // controller.InitiatePurchase(sku);
#else
            Debug.LogWarning("IAP paketi kurulu değil — editor test: hak doğrudan veriliyor.");
            GrantNoAds();
#endif
        }

        /// iOS'ta ZORUNLU "Restore Purchases" butonu çağırır.
        public void RestorePurchases()
        {
#if UNITY_PURCHASING && UNITY_IOS
            // extensions.GetExtension<IAppleExtensions>().RestoreTransactions(...)
#endif
        }

        /// Satın alma başarılı olduğunda (hangi kademe olursa olsun) çağrılır.
        public void GrantNoAds()
        {
            if (AdsRemoved) return;
            AdsRemoved = true;
            OnAdsRemoved?.Invoke();
            Debug.Log("Destekçi ✓ — reklamlar kalıcı olarak kaldırıldı.");
        }
    }
}
