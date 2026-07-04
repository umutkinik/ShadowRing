# ShadowRing — Gölgehalka: The Sundered Realm

Telif-güvenli, LOTR ruhundan esinlenen 2.5D kule savunma oyunu. **Android + iOS · Unity 6 (URP)**.

> Zarok Boşluk Efendisi'nin orduları Aethelmark'ı basar. Dokuz kahramanı kule gibi konumlandır,
> Gölgehalka'yı yok etmek için diyarı savun.

## Depo Yapısı

| Klasör | İçerik |
|---|---|
| `ShadowRing/` | Unity 6 projesi (oyunun kendisi) |
| `Golgehalka/Unity-Assets/` | Çekirdek C# kaynak (Unity projesine senkronlanır) |
| `Golgehalka/tools/meshy/` | Meshy text-to-3D üretim hattı + 36 asset (GLB) |
| `Golgehalka/Localization/` | 7 dil string tabloları (EN varsayılan, AR dahil RTL) |
| `Golgehalka/Design/` | Balans, sahne kurulumu, rig boru hattı, ses, store/ASO dokümanları |

## Hızlı Başlangıç

1. Unity Hub → `ShadowRing/` klasörünü aç (Unity 6000.5+, URP).
2. Menü: **Gölgehalka → Prototip Sahne Kur** → Play.
3. Akış: kahraman seç → platforma tıkla → "Sonraki Dalga".

## Özellikler (durum)

- ✅ Çekirdek TD döngüsü: yol, dalga, kule, ekonomi, can — oynanabilir
- ✅ 36 özgün 3D asset (Meshy): 9 kahraman, 10 düşman/boss, 8 silah, 6 mekân, 3 platform
- ✅ 7 dil altyapısı (EN/DE/RU/ZH/HI/AR/TR), RTL destekli
- ✅ Monetizasyon tasarımı: bölüm geçişi reklamı + "kahve parası" destekçi paketi (p2w yok)
- 🔜 Mixamo rig + animasyon, seviye seçimi, Act I içerik, reklam/IAP SDK bağlama

## Notlar

- `Golgehalka/tools/meshy/.env` (API anahtarı) commit edilmez — kendi anahtarınla oluştur.
- Asset lisansları: Meshy üretimi (hesap planına tabi) + CC0 kaynaklar (Kenney, Quaternius).
