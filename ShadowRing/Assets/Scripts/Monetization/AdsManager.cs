using System;
using UnityEngine;

namespace Golgehalka.Monetization
{
    /// Reklam akışı — TEK kural: ücretsiz sürümde bölüm geçişlerinde interstitial.
    /// Destekçi (AdsRemoved) hiç reklam görmez. Başka reklam yüzeyi YOK (kasma yok).
    ///
    /// SDK: Unity LevelPlay (ironSource) veya Google AdMob — aşağıdaki üç metodu
    /// seçilen SDK'nın çağrılarıyla doldur. Oyun kodu yalnızca bu sınıfı bilir.
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }

        [Tooltip("İki interstitial arası minimum süre (sn) — art arda geçişte üst üste reklam olmasın")]
        [SerializeField] private float minSecondsBetweenAds = 90f;

        private float lastAdTime = -999f;
        private Action pendingCallback;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSdk();
        }

        private void InitializeSdk()
        {
            if (PurchaseManager.Instance != null && PurchaseManager.Instance.AdsRemoved)
                return; // destekçi — SDK'yı hiç başlatma (gizlilik + performans bonusu)
            // TODO: LevelPlay/AdMob init + ilk interstitial'ı önden yükle (preload)
        }

        /// BÖLÜM GEÇİŞİ AKIŞI — LevelFlow zafer sonrası bunu çağırır:
        /// reklamsız hak varsa / reklam hazır değilse / süre dolmadıysa → doğrudan devam.
        /// Aksi halde reklamı göster, kapanınca devam et. Oyuncu asla kilitlenmez.
        public void ShowInterstitialThen(Action onDone)
        {
            bool skip =
                (PurchaseManager.Instance != null && PurchaseManager.Instance.AdsRemoved)
                || Time.unscaledTime - lastAdTime < minSecondsBetweenAds
                || !IsAdReady();

            if (skip) { onDone?.Invoke(); return; }

            pendingCallback = onDone;
            lastAdTime = Time.unscaledTime;
            // TODO: SDK show çağrısı — kapatma/hata callback'i OnAdClosed()'a bağlanır
            OnAdClosed(); // SDK entegre edilene dek anında devam (editor/dev akışı)
        }

        private bool IsAdReady()
        {
            // TODO: SDK "isLoaded/isReady" kontrolü. Hazır değilse oyuncu bekletilmez.
            return true;
        }

        /// SDK'nın "reklam kapandı" VE "gösterim hatası" callback'lerinin İKİSİ de
        /// buraya bağlanır — devam akışı her durumda garanti edilir.
        private void OnAdClosed()
        {
            var cb = pendingCallback;
            pendingCallback = null;
            // TODO: sonraki interstitial'ı önden yükle
            cb?.Invoke();
        }
    }
}
