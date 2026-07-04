# Gölgehalka — Lokalizasyon Tabloları

7 dil, **varsayılan/kaynak dil: İngilizce (`en.json`)**. Tüm anahtarlar EN tablosunda tanımlanır;
diğer diller aynı anahtar setini birebir takip eder.

| Dosya | Dil | Yön |
|---|---|---|
| `en.json` | English (varsayılan) | LTR |
| `de.json` | Deutsch | LTR |
| `ru.json` | Русский | LTR |
| `zh-Hans.json` | 简体中文 | LTR |
| `hi.json` | हिन्दी | LTR |
| `ar.json` | العربية | **RTL** |
| `tr.json` | Türkçe | LTR |

## Unity'ye aktarım

1. Package Manager → **Localization** (com.unity.localization) kur.
2. `Project Settings → Localization` altında 7 locale oluştur (en, de, ru, zh-Hans, hi, ar, tr).
3. Bir **String Table Collection** ("UI") oluştur; bu JSON'lardaki anahtarları içeri aktar
   (CSV'ye çevirip Localization paketinin CSV import'u ile ya da küçük bir editor script'i ile).
4. UI metinlerinde `LocalizeStringEvent` bileşeni + anahtar bağla. **Metin asla sabit kodlanmaz.**

## Kurallar

- **Font:** Noto Sans ailesi (OFL). TextMeshPro fallback zinciri:
  `NotoSans` → `NotoSansSC` (zh) → `NotoSansDevanagari` (hi) → `NotoSansArabic` (ar) → `NotoSansCyrillic` kapsamı.
- **RTL (ar):** HUD/menü yerleşimi ayna çevrilir; sayılar LTR kalır. TMP için RTL desteğini etkinleştir
  (veya RTLTMPro benzeri açık kaynak çözüm kullan).
- **Genişleme payı:** Almanca ~%35 uzar — buton/panel genişlikleri esnek olmalı.
- **Türkçe İ/ı:** Büyük/küçük harf dönüşümünde `CultureInfo("tr-TR")` kullan; `ToUpper()` değil `ToUpper(culture)`.
- **Marka adı** ("Shadowband") tüm dillerde Latin kalır; alt başlık çevrilir.
- Doku/texture içine metin gömme — tüm yazılar UI katmanında.
- Yeni anahtar eklerken önce `en.json`'a ekle, sonra tüm dillere kopyala (eksik anahtar = build hatası saymalı).
