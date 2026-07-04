# Store Hazırlığı + ASO Planı

## Kimlik

- **Ad:** Shadowband: The Sundered Realm (TR mağazada: "Gölgehalka" alt başlıkla)
- **Paket:** `com.golgehalka.shadowband` (iOS bundle + Android applicationId aynı)
- **Kategori:** Strategy / Tower Defense · **Yaş:** 9+ (fantezi şiddeti)

## ASO Anahtar Kelimeleri (EN)

tower defense, fantasy TD, offline tower defense, hero defense, castle defense,
strategy offline, epic fantasy battle — "offline" vurgusu önemli: reklam hariç
internet gerektirmeyen TD'ler aramada avantajlı.

## Mağaza Metinleri

**EN — kısa açıklama (80 kr):**
"Epic fantasy tower defense. Command 9 heroes, defend the realm from the Void!"

**TR — kısa açıklama:**
"Destansı kule savunma. 9 kahramana komuta et, diyarı Boşluk'tan koru!"

**Uzun açıklama iskeleti (7 dile çevrilecek — Localization tablosu mantığıyla):**
1. Kanca: "Gölgehalka yeniden ortaya çıktı — ve Zarok onu geri istiyor."
2. Özellikler: 9 benzersiz kahraman · 18 bölüm + 3 boss aktı · sonsuz mod ·
   çevrimdışı oynanış · 7 dil desteği
3. Dürüst monetizasyon cümlesi: "Tek seferlik küçük bir destekle tüm reklamları kaldır.
   Pay-to-win yok — asla."

**Kural:** Her dilde açıklama o dilin Localization tablosundaki üslubu izler;
DE/RU/ZH/HI/AR çevirileri yayın öncesi ana dil konuşuruna kontrol ettirilir.

## Görsel Varlıklar

| Varlık | Boyut | İçerik |
|---|---|---|
| App icon | 1024×1024 | Zarok'a karşı kahraman silueti + halka motifi (mevcut appicon.png revize) |
| Ekran görüntüleri ×6-8 | cihaz başına | 1: savaş anı, 2: kahraman vitrini, 3: boss savaşı, 4: harita, 5: yükseltme, 6: "reklamsız destekçi" tanıtımı |
| Feature graphic (Play) | 1024×500 | Act III kuşatma sahnesi |
| Önizleme videosu | 15-30 sn | dalga savunması → boss girişi → zafer |

Ekran görüntüsü çerçeve metinleri 7 dilde ayrı üretilir (store lokalizasyonu indirmeyi ciddi artırır).

## Yayın Öncesi Zorunluluklar

- [ ] **Gizlilik politikası URL'si** — reklam SDK'sı kullanıldığı için İKİ mağazada da zorunlu.
      (AdMob/LevelPlay veri toplama beyanı + Play Data Safety formu + Apple Privacy Nutrition Label)
- [ ] iOS **ATT izni** (App Tracking Transparency) — kişiselleştirilmiş reklam için izin diyaloğu;
      reddedilirse kişiselleştirilmemiş reklam gösterilir
- [ ] GDPR/UMP consent akışı (Avrupa) — AdMob UMP SDK'sı
- [ ] Çocuk hedeflemesi İŞARETLENMEZ (9+ genel kitle) — COPPA karmaşasından kaçınılır
- [ ] Google Play: kapalı test (14 gün / 12 testçi kuralı) → üretim
- [ ] Apple: TestFlight → İnceleme (IAP restore butonu olmadan RED yerler — kodda hazır ✓)
- [ ] Meshy ticari kullanım şartı doğrulaması + asset lisans tablosu (`Audio/LICENSES.md` dahil)

## Lansman Sonrası (LiveOps hafif)

Haftalık zorluk rotasyonu (remote config ile dalga modifiyesi — build gerektirmez) ·
sezonluk kozmetik (Faz 3+) · yorumlara 7 dilde şablon yanıtlar.
