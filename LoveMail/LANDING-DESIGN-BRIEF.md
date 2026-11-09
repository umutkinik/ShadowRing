# LoveMail — Landing Site Tasarım Tarifi

> Amaç: Tasarımcı, geliştirici veya AI araçlarına verildiğinde **aynı görsel dili** üretecek tek referans.
> Kapsam: `lovemail.cool` ana landing + 5 domain alt sayfaları + OG/paylaşım kartları.
> Format: Her bölümde önce **karar**, sonra **neden**, sonra **token/ölçü**.

---

## 1. Marka Konumu (tasarımı yönlendiren 3 cümle)

1. **"E-posta, ama kalbi olan."** Gmail'in soğuk gri dünyasına karşı sıcak, kişisel, biraz yaramaz.
2. **Beş kişilik, tek sen.** Aynı insan farklı bağlamda farklı ton kullanır; domainler bu tonların kostümü.
3. **Cesaret ürünü.** İtiraf etmek, anonim söylemek, geleceğe yazmak. Tasarım bu cesareti ödüllendirmeli: oyuncu, güvenli, utandırmayan.

**Marka kişiliği (5 sıfat):** Sıcak · Cesur · Şakacı · Net · Güvenilir
**Olmadığı şeyler:** Kurumsal, soğuk, "startup mavisi", aşırı sevgililer günü kırmızısı, çocuksu.

**Referans hissiyat:** Duolingo'nun oyunculuğu + Notion'un sadeliği + Spotify Wrapped'in paylaşılabilirliği. Linear'ın koyu modu değil, Pinterest'in kremi.

---

## 2. Renk Sistemi

### 2.1 Çekirdek marka renkleri

| Token | Hex | Kullanım |
|---|---|---|
| `brand.heart` | `#FF5C8A` | Birincil aksiyon, logo kalbi, vurgular |
| `brand.heart-deep` | `#E63E6D` | Hover, aktif durum |
| `brand.ink` | `#17141F` | Ana metin, koyu mod zemini |
| `brand.cream` | `#FFF6F1` | Açık mod sayfa zemini (saf beyaz **değil**) |
| `brand.paper` | `#FFFFFF` | Kart yüzeyleri |
| `brand.mist` | `#F1E9EC` | Ayırıcı, pasif zemin |
| `brand.smoke` | `#6B6473` | İkincil metin |
| `brand.success` | `#22C55E` | Eşleşme, doğrulama |
| `brand.warning` | `#F59E0B` | Kota, uyarı |
| `brand.danger` | `#EF4444` | Hata, silme |

**Neden pembe ama fuşya değil:** `#FF5C8A` beyaz üzerinde 3,1:1 (büyük metin ve ikon için yeterli), ink üzerinde 7,4:1. Sevgililer günü kırmızısına kaymıyor, erkek kullanıcıyı itmiyor.

### 2.2 Domain aksanları ("vibe" renkleri)

Her domain bir karakter. Landing'de 5 kart, uygulamada kimlik rozeti, OG kartta çerçeve rengi olarak kullanılır.

| Domain | Aksan | Hex | Koyu eş | Karakter | Emoji |
|---|---|---|---|---|---|
| `.cool` | Buz Mavisi | `#4CC9F0` | `#1B8FB8` | Sakin, varsayılan, güvenilir | 🧊 |
| `.fun` | Güneş Sarısı | `#FFC845` | `#D99A00` | Enerjik, etkinlik, oyun | 🎉 |
| `.live` | Elektrik Moru | `#8B5CF6` | `#6D3FE0` | Yayıncı, sahne, link-in-bio | 📡 |
| `.lol` | Asit Yeşili | `#B8F23D` | `#7FB800` | Meme, absürt, hızlı | 😂 |
| `.wtf` | Yangın Kırmızısı | `#FF3B3B` | `#C21F1F` | Sert, burner, sarkastik | 🔥 |

**Kural:** Bir ekranda en fazla **1 vibe rengi + brand.heart** aynı anda baskın olur. Beş rengin yan yana geldiği tek yer "vibe seçici" bölümü ve logo animasyonudur.

### 2.3 Gradyanlar (yalnızca hero ve OG kart)

```css
--grad-hero:   linear-gradient(135deg, #FF5C8A 0%, #8B5CF6 100%);
--grad-cool:   linear-gradient(135deg, #4CC9F0 0%, #8B5CF6 100%);
--grad-fun:    linear-gradient(135deg, #FFC845 0%, #FF5C8A 100%);
--grad-live:   linear-gradient(135deg, #8B5CF6 0%, #FF5C8A 100%);
--grad-lol:    linear-gradient(135deg, #B8F23D 0%, #4CC9F0 100%);
--grad-wtf:    linear-gradient(135deg, #FF3B3B 0%, #17141F 100%);
```

Gradyan metin üzerine değil, **zemin veya kenarlık** olarak. Gövde metni asla gradyan üstüne gelmez.

### 2.4 Koyu mod

Zemin `#17141F`, kart `#221E2B`, kenarlık `#2F2938`, metin `#F6F1F3`, ikincil `#A89FAD`. Vibe renkleri koyu modda **%10 daha açık** tonlarda, gradyanlar %70 opaklıkta. Varsayılan: sistem tercihi; landing açık modda tasarlanır, koyu mod türetilir.

---

## 3. Tipografi

| Rol | Font | Ağırlık | Neden |
|---|---|---|---|
| Display / başlık | **Fraunces** (Google Fonts, değişken) | 600-700, `opsz` 144, `WONK` 1 | Yumuşak serif, "kalp" hissi, kurumsal sans'tan ayrışır. Değişken font tek dosya. |
| Gövde / arayüz | **Inter** (veya Geist) | 400, 500, 600 | Okunur, her dil için glif var (TR ş ğ ı sorunsuz). |
| E-posta adresleri, kod, handle | **JetBrains Mono** | 500 | Adresin "gerçek" olduğunu hissettirir, `ahmet@lovemail.lol` net okunur. |

**Ölçek (rem, 16px taban):**

```
display-xl  4.5rem / 1.0   Fraunces 700  (hero başlık, sadece desktop)
display-lg  3.25rem / 1.05 Fraunces 700
display-md  2.25rem / 1.1  Fraunces 600
heading     1.5rem / 1.25  Inter 600
body-lg     1.125rem / 1.6 Inter 400
body        1rem / 1.6     Inter 400
small       0.875rem / 1.5 Inter 500
mono        1rem / 1.4     JetBrains Mono 500, letter-spacing -0.01em
```

**Kurallar:**
- Başlıklar cümle düzeninde (Sentence case), ASLA tamamı büyük harf.
- Fraunces italik yalnızca tek kelimelik vurguda: "Söyle *ona*."
- Satır genişliği gövde metninde 60-70 karakter.
- Türkçe ve İngilizce aynı ölçekte test edilir; TR metin %15-20 daha uzun, hero başlıkları için 2 satır sınırı.

---

## 4. Şekil, Boşluk, Derinlik

| Token | Değer | Not |
|---|---|---|
| `radius.sm` | 8px | Input, rozet |
| `radius.md` | 14px | Buton |
| `radius.lg` | 24px | Kart |
| `radius.xl` | 32px | Hero kutusu, modal |
| `radius.pill` | 999px | Domain rozetleri, etiketler |
| Boşluk ölçeği | 4 · 8 · 12 · 16 · 24 · 32 · 48 · 64 · 96 · 128 | 8'in katları, bölüm araları 96-128 |
| `shadow.card` | `0 1px 2px rgba(23,20,31,.04), 0 8px 24px rgba(23,20,31,.06)` | Yumuşak, pembe değil nötr |
| `shadow.pop` | `0 12px 40px rgba(255,92,138,.25)` | Yalnızca birincil CTA hover ve eşleşme kartı |
| Kenarlık | 1px `brand.mist` | Koyu modda `#2F2938` |

**Köşeler yuvarlak ama yumuşak değil:** 24px kart, 14px buton. "Blob" veya organik şekil yok; sabit radius, düz çizgi, net hiyerarşi.

---

## 5. İkon ve İllüstrasyon

- **İkon seti:** Phosphor Icons (duotone), 1.5px stroke, 24px grid. Ücretsiz, Türkçe arayüzde yeterli kapsam.
- **Logo:** Kelime markası "lovemail" Fraunces 700 küçük harf; `o` harfi yerine hafif eğik kalp zarfı (zarf kapağı kalp şeklinde). Mono renk versiyonu şart. Domain uzantısı logonun sağında pill içinde değişir: `lovemail` **.lol**
- **İllüstrasyon stili:** 3D değil, düz renk + hafif grain dokulu **"kağıt kolaj"**. Zarf, kalp, mühür, posta pulu, kağıt uçak motifleri. Karakter yok (insan çizimi yok), nesneler var. Bu hem üretimi ucuzlatır hem her kültüre uyar.
- **Fotoğraf:** Kullanılmaz. Gerçek ürün ekran görüntüsü ve kolaj illüstrasyon yeterli.
- **Emoji:** Sistem emojisi değil, **Noto Color Emoji** veya Twemoji ile tutarlılık. Vibe kartlarında ve eşleşme animasyonunda.

### AI görsel üretim yönergesi (Midjourney / Flux / Ideogram)

```
flat paper-collage illustration of a pastel envelope with a heart-shaped wax seal,
subtle paper grain texture, soft shadows, warm cream background #FFF6F1,
accent color #FF5C8A, minimal composition, centered, no text, no people,
editorial style, 3:2 aspect ratio --style raw --v 6
```

Varyasyonlar: `envelope opening with confetti` (eşleşme), `stack of five envelopes in five colors` (vibe seçici), `envelope with a tiny clock` (zaman kapsülü), `envelope with a burning fuse` (burner, .wtf), `megaphone made of paper` (public sayfa, .live).

---

## 6. Hareket (Motion)

| İlke | Değer |
|---|---|
| Süre | Mikro 120-180ms, geçiş 240-320ms, hero 600-900ms |
| Eğri | `cubic-bezier(.2,.8,.2,1)` (hızlı başla, yumuşak bitir) |
| Hero adres daktilosu | `ahmet@lovemail.` sonrası uzantı 1,6 sn'de bir değişir, her uzantı kendi rengiyle; `prefers-reduced-motion` ise statik `.cool` |
| Kaydırma animasyonu | Yalnızca ilk görünümde, 16px yukarı + fade, bölüm başına tek seferlik |
| Eşleşme | Konfeti 1,2 sn, kalp büyür 1,0 → 1,15 → 1,0, haptik (mobil) |
| Hover | Kart 2px yukarı + shadow.card → shadow.pop yok (yalnız CTA'da) |
| Yasak | Paralaks, sonsuz dönen arka plan, otomatik oynayan video, imleç izleme |

---

## 7. Landing Sayfa Anatomisi

Hedef: ilk ekranda 5 saniyede "ne bu, neden havalı, nasıl alırım". Tek sütun, maksimum genişlik 1200px, mobil öncelikli.

### 7.1 Nav (yapışkan, 64px)
Sol: logo + değişen uzantı pill. Orta: Özellikler · Vibe'lar · Fiyat · SSS. Sağ: "Giriş" (metin) + "Adresini kap" (heart buton). Kaydırınca zemin `cream` %85 + blur 12px.

### 7.2 Hero
- Başlık (display-xl, 2 satır): **"Kalbi olan e-posta."**
- Alt metin (body-lg, smoke): "Bir isim al, beş kimlik kazan. İtiraf et, anonim konuş, geleceğe yaz."
- Canlı giriş alanı: `[ adın ]@lovemail.[cool▾]` mono font, uzantı daktilo animasyonu; alanın altında yeşil tik ile "ahmet uygun ✓" canlı kontrol.
- Birincil CTA: "Adresini kap, ücretsiz" · İkincil: "Nasıl çalışır →"
- Sağda/altta kolaj illüstrasyon: 5 renkli zarf yelpazesi.
- Sosyal kanıt satırı: "🔥 1.284 kişi bekleme listesinde" (gerçek sayı, API'den).

### 7.3 Vibe seçici (5 kart, yatay kaydırmalı mobilde)
Her kart: emoji, uzantı mono büyük, 1 cümle karakter, örnek adres, kart kenarlığı domain aksanı, hover'da gradyanı. Başlık: **"Hangi lovemail'sin?"** Tıklayınca hero giriş alanındaki uzantı değişir (sayfa içi).

### 7.4 Özellik 1: Karşılıklı itiraf
İki telefon mockup'ı yan yana: solda itiraf gönderme, sağda "Eşleştiniz 💘". Metin: "Söyle. O da söylediyse ikiniz de öğrenirsiniz. Söylemediyse kimse bilmez." Alt not küçük: "Eşleşme olmadan hiçbir bildirim gitmez."

### 7.5 Özellik 2: Anonim sayfa (.live)
Örnek `lovemail.live/u/ahmet` sayfası mockup'ı + Instagram story kartı. Metin: "Linkini at, ne düşündüklerini öğren." Güven rozeti: "Raporla · Engelle · Moderasyon".

### 7.6 Özellik 3: Burner (.wtf) + Zaman kapsülü (.fun)
İki yarım kart. Burner: yanan fitil illüstrasyonu, "Kayıt formlarına gerçek adresini verme." Kapsül: saatli zarf, "Bir yıl sonraki sana mail at."

### 7.7 "Nasıl çalışır" 3 adım
Numara pill (heart), kısa başlık, 1 cümle. 1 İsim seç · 2 Vibe'ını al · 3 Paylaş.

### 7.8 Fiyat
İki kart: Free ve Plus. Plus kartı 1px heart kenarlık + "Çok tercih edilen" pill. Fiyat Fraunces display-md, dönem small. Özellik listesi tik ikonlu.

### 7.9 SSS (akordeon, 6 soru)
Gizlilik, anonimlik nasıl korunur, Gmail'e gelir mi, ücretsiz mi kalır, veriler nerede, hesabımı silebilir miyim.

### 7.10 Son CTA bandı
`grad-hero` zemin, beyaz display-lg: "Adın hâlâ boşta." Mono giriş alanı tekrar. Tek buton.

### 7.11 Footer
4 sütun: Ürün · Vibe'lar (5 domain linki) · Şirket · Yasal (KVKK, Gizlilik, Şartlar, abuse@). Altta 5 domain pill'i sırayla renkli. Dil seçici TR/EN.

---

## 8. Bileşen Spesifikasyonları

**Buton (primary):** yükseklik 48px, padding 0 24px, radius.md, zemin heart, metin paper 600, hover heart-deep + 1px yukarı, focus 3px `#FF5C8A40` halka. Loading: spinner + metin kalır.

**Buton (secondary):** şeffaf, 1px ink %15 kenarlık, hover mist zemin.

**Input (adres):** yükseklik 56px, mono font, sol kısım kullanıcı girişi, sağ kısım `@lovemail.` + uzantı dropdown'u pill içinde domain rengiyle. Durum renkleri: uygun success, alınmış danger, kontrol ediliyor smoke.

**Domain pill:** 28px yükseklik, radius.pill, zemin aksan %15, metin aksan koyu eşi, mono small.

**Kart:** paper zemin, radius.lg, 1px mist, padding 24-32px, shadow.card.

**Mockup çerçevesi:** iPhone benzeri ama markasız; 12px çerçeve, ink renk, 40px radius, ekran içinde gerçek UI.

---

## 9. Paylaşım Kartları (OG / Story)

| Format | Ölçü | İçerik |
|---|---|---|
| OG (X, WhatsApp, Slack) | 1200×630 | Sol: başlık Fraunces, sağ: zarf kolaj; alt şerit 5 domain pill |
| Instagram story | 1080×1920 | Üstte anonim mesaj kartı (paper, radius.xl), altta `lovemail.live/u/<handle>` mono + QR, zemin vibe gradyanı |
| Instagram post | 1080×1080 | Mesaj kartı ortada, köşede logo |
| Eşleşme kartı | 1080×1920 | İki kalp, "Eşleştik 💘", tarih, isimler **isteğe bağlı** gizli |

Kart üretimi `@vercel/og` ile sunucuda; fontlar gömülü; kullanıcı metni 280 karakterde kesilir; her kartta filigran `lovemail.live` %60 opaklık.

---

## 10. Erişilebilirlik ve Performans

- Metin kontrastı en az 4,5:1 gövde, 3:1 büyük metin. Heart renkli metin yalnızca display boyutunda.
- Tüm vibe renkleri yalnızca **rengi tekrarlayan bir ikon veya etiketle** anlam taşır (renk körlüğü).
- Klavye ile hero adres alanı → uzantı seçici → CTA sırası doğal.
- `prefers-reduced-motion` tüm animasyonları kapatır.
- Fontlar `font-display: swap`, Fraunces subset (Latin + Latin-Ext), toplam font < 180 KB.
- LCP hedefi < 2,0 sn mobil 4G; hero illüstrasyon AVIF, 60 KB altı.
- Lighthouse 95+ dört kategoride.

---

## 11. Tailwind Token Taslağı

```ts
// tailwind.config.ts (özet)
export default {
  theme: {
    extend: {
      colors: {
        heart: { DEFAULT: "#FF5C8A", deep: "#E63E6D" },
        ink: "#17141F", cream: "#FFF6F1", paper: "#FFFFFF",
        mist: "#F1E9EC", smoke: "#6B6473",
        vibe: {
          cool: { DEFAULT: "#4CC9F0", deep: "#1B8FB8" },
          fun:  { DEFAULT: "#FFC845", deep: "#D99A00" },
          live: { DEFAULT: "#8B5CF6", deep: "#6D3FE0" },
          lol:  { DEFAULT: "#B8F23D", deep: "#7FB800" },
          wtf:  { DEFAULT: "#FF3B3B", deep: "#C21F1F" },
        },
      },
      fontFamily: {
        display: ["Fraunces", "serif"],
        sans: ["Inter", "system-ui", "sans-serif"],
        mono: ["JetBrains Mono", "monospace"],
      },
      borderRadius: { sm: "8px", md: "14px", lg: "24px", xl: "32px" },
      boxShadow: {
        card: "0 1px 2px rgba(23,20,31,.04), 0 8px 24px rgba(23,20,31,.06)",
        pop: "0 12px 40px rgba(255,92,138,.25)",
      },
      transitionTimingFunction: { brand: "cubic-bezier(.2,.8,.2,1)" },
    },
  },
};
```

---

## 12. Ses ve Metin Tonu (tasarımla birlikte gider)

- Kısa, ikinci tekil şahıs, emir kipi yumuşak: "Adresini kap", "Söyle ona", "Linkini at".
- Nokta kullan, ünlem nadir. Emoji başlıkta değil, pill ve kartta.
- İngilizce versiyon birebir çeviri değil, aynı ritim: "Email with a heartbeat." / "Claim your name."
- Hata mesajları suçlamaz: "Bu isim alınmış. `ahmet.k` ya da `ahmetk` boşta." (öneri her zaman var)
- Yasak kelimeler: "devrim", "yeni nesil", "yapay zekâ destekli", "güçlendirilmiş".

---

## 13. Yap / Yapma

| ✅ Yap | ❌ Yapma |
|---|---|
| Cream zemin, bol boşluk, tek güçlü pembe | Saf beyaz üstüne 5 rengi aynı anda |
| Mono font ile gerçek adres göster | Soyut "mail ikonu" ile anlat |
| Gerçek UI mockup'ı | Stok fotoğraf, gülen insanlar |
| Sentence case başlık | TÜMÜ BÜYÜK HARF |
| Kolaj zarf illüstrasyonu | 3D render, glassmorphism, neon |
| Eşleşme için konfeti, bir kez | Sürekli hareket eden arka plan |
| Güven rozetleri anonim özelliğin hemen yanında | Anonimliği "sınırsız özgürlük" gibi satmak |
| Domain renklerini kimlik olarak kullan | Domain renklerini dekor olarak serpiştir |

---

## 14. Teslimat Kontrol Listesi (tasarımcıya / AI'a)

- [ ] Figma: renk ve tip stilleri bu tokenlarla isimlendirilmiş
- [ ] 5 vibe kartı + 5 OG kart varyantı
- [ ] Hero desktop (1440) + mobil (390) + tablet (820)
- [ ] Koyu mod hero ve fiyat bölümü
- [ ] Buton, input, pill, kart bileşenleri tüm durumlarıyla (default, hover, focus, disabled, loading, error)
- [ ] Logo: renkli, mono siyah, mono beyaz, favicon 32/180/512, maskable ikon
- [ ] Kolaj illüstrasyon seti: 6 adet (hero, eşleşme, kapsül, burner, megafon, 5 zarf)
- [ ] TR ve EN metinli hero karşılaştırması (taşma kontrolü)
- [ ] Reduced-motion statik hali
