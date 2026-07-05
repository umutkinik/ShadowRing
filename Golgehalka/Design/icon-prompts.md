# SHADOWRING — Oyun İkonu AI Direktifleri

## Teknik hedefler (üretmeden önce bil)

| Platform | Boyut | Format | Not |
|---|---|---|---|
| iOS App Store | 1024×1024 | PNG, **alfasız/şeffaflıksız** | Köşeleri Apple kendisi yuvarlar — köşelere önemli detay koyma |
| Google Play Store | 512×512 | PNG ≤ 1 MB | Google kendi maskesini uygular |
| Android adaptif ikon | 1024×1024 ön katman | PNG şeffaf | Konu merkezdeki **%60'lık güvenli alanda** kalmalı (kenarlar cihaza göre kırpılır) |

**Tek üretim yeter:** 1024×1024, konu merkezde ve kadrajın ~%60'ı içinde → hepsine ölçeklenir.

## Altın kurallar

1. **METİN YOK.** İkonda yazı/harf küçük boyutta okunmaz, 7 dilde zaten çevrilemez.
2. **48 piksele küçült, hâlâ tanınıyor mu?** Tek güçlü siluet, tek odak. Kalabalık sahne = bulanık leke.
3. **Koyu zemin + altın/kor vurgu** — ana menü key-art'ımızla aynı dil (Diablo-vari).
4. Telif güvenliği: prompt'a asla "Lord of the Rings", "One Ring", "Sauron", "elvish inscription" yazma. Bizim evren: **shadow ring / gölge halka**.

## Konsept A — Gölge Halka (ÖNERİLEN, marka logosu)

```
Mobile game app icon, dark fantasy style. A massive ancient ring of
blackened iron floating upright, wreathed in purple-black shadow mist,
inner edge glowing with molten gold ember light. Centered composition
on a near-black background with subtle dark stone texture. Dramatic
rim lighting, painterly AAA fantasy art style, rich contrast,
volumetric glow. Single iconic object, no text, no letters,
no watermark, no border, square 1:1.
```

## Konsept B — Halka + Kule (tower defense vurgusu)

```
Mobile game app icon, dark fantasy tower defense game. A lone stone
watchtower with a glowing golden beacon, seen through a giant shadowy
ring that frames it like an eclipse. Purple storm clouds behind,
ember sparks rising. Centered, symmetrical, painterly Diablo-esque
style, high contrast, dramatic lighting. No text, no letters,
no watermark, square 1:1.
```

## Konsept C — Halka + Kılıç (aksiyon vurgusu)

```
Mobile game app icon, dark epic fantasy. A cracked ancient sword
standing point-down inside a floating ring of dark metal, gold light
leaking through the cracks, black-violet mist swirling. Centered
single subject on near-black textured background, painterly style,
strong silhouette, cinematic glow. No text, no watermark, square 1:1.
```

## Negatif prompt (destekleyen araçlarda)

```
text, letters, words, logo, watermark, signature, frame, border,
photorealistic photo, blurry, cluttered, multiple objects, human face
```

## Üretim akışı

1. Aynı prompt'la 4-8 varyasyon üret, **48px'e küçültüp** yan yana bak — en net siluet kazanır.
2. Seçileni 1024×1024 PNG kaydet → `Golgehalka/art/icon/icon_master.png`.
3. Bana "ikonu kaydettim" de: alfa temizliği + 512 Play sürümü + Android adaptif ön/arka katman ayrımı + Unity Player Settings bağlamasını ben yaparım.

> Arka plan katmanı (Android adaptif) için ayrıca üretim gerekmez: master'ın
> köşe dokusundan düz koyu (#0E0D11) bir katman türetilir.
