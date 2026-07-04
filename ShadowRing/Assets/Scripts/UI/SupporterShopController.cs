using Golgehalka.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace Golgehalka.UI
{
    /// "Kahve parası" destekçi ekranı — üç kademe, tek hak.
    /// Metinler: iap.remove_ads / iap.pwyw_subtitle / iap.tier.* / iap.restore (LocalizedText).
    /// Zaten destekçiyse mağaza yerine teşekkür görünümü gösterilir.
    public class SupporterShopController : MonoBehaviour
    {
        [SerializeField] private Button coffeeSmallButton;
        [SerializeField] private Button coffeeLargeButton;
        [SerializeField] private Button coffeeFeastButton;
        [SerializeField] private Button restoreButton;   // iOS'ta zorunlu; Android'de gizlenebilir
        [SerializeField] private GameObject purchaseGroup;
        [SerializeField] private GameObject thanksGroup;  // iap.thanks metni

        private void OnEnable()
        {
            coffeeSmallButton.onClick.AddListener(() => Buy(PurchaseManager.SkuCoffeeSmall));
            coffeeLargeButton.onClick.AddListener(() => Buy(PurchaseManager.SkuCoffeeLarge));
            coffeeFeastButton.onClick.AddListener(() => Buy(PurchaseManager.SkuCoffeeFeast));
            restoreButton.onClick.AddListener(() => PurchaseManager.Instance.RestorePurchases());
            PurchaseManager.Instance.OnAdsRemoved += ShowThanks;

#if !UNITY_IOS
            restoreButton.gameObject.SetActive(false);
#endif
            Refresh();
        }

        private void OnDisable()
        {
            coffeeSmallButton.onClick.RemoveAllListeners();
            coffeeLargeButton.onClick.RemoveAllListeners();
            coffeeFeastButton.onClick.RemoveAllListeners();
            restoreButton.onClick.RemoveAllListeners();
            if (PurchaseManager.Instance != null)
                PurchaseManager.Instance.OnAdsRemoved -= ShowThanks;
        }

        private void Buy(string sku) => PurchaseManager.Instance.BuySupporter(sku);

        private void ShowThanks() => Refresh();

        private void Refresh()
        {
            bool supporter = PurchaseManager.Instance.AdsRemoved;
            purchaseGroup.SetActive(!supporter);
            thanksGroup.SetActive(supporter);
        }
    }
}
