# Ses Planı — Açık Kaynak / CC0 Kaynaklar

## Kaynaklar (lisans disiplinli)

| Kaynak | Lisans | Kullanım |
|---|---|---|
| **Kenney Audio** (kenney.nl/assets) | CC0 | UI tık, altın, inşa sesleri — ilk tercih |
| **Freesound.org** | Karışık! CC0 filtresiyle ara | Savaş SFX (kılıç, ok, patlama) |
| **OpenGameArt (müzik)** | CC0/CC-BY dikkat | Fantezi orkestral parçalar |
| **Pixabay Music** | Pixabay lisansı (ticari serbest, atıf yok) | Menü + savaş müziği |
| **incompetech** (Kevin MacLeod) | CC-BY (atıf zorunlu!) | Geniş fantezi kataloğu |

> Her indirilen dosya `Audio/LICENSES.md` tablosuna eklenir: dosya, kaynak URL, lisans, atıf gereği.
> CC-BY kullanılırsa oyun içi "Krediler" ekranı zorunlu olur.

## İhtiyaç Listesi (MVP)

**Müzik (4 parça):** menü teması (sakin fantezi) · Act I savaş (orman/macera) · Act II savaş (kuşatma/gerilim) · boss teması (koyu orkestral)

**SFX (öncelik sırasıyla):**
1. UI: buton, altın harcama, yükseltme, hata (yetersiz altın)
2. Savaş: ok atışı, kılıç, balta, büyü şimşeği, zehir, düşman ölümü ×3 varyant
3. Akış: dalga borusu (Baldric'in boynuzu!), zafer fanfarı, yenilgi, boss girişi
4. Ambians: Act başına 1 loop (kuş/orman, rüzgar/taş, alev/uğultu)

## Teknik

- Format: OGG (Android+iOS ikisinde de sorunsuz), müzik 128kbps, SFX 96kbps mono
- `AudioMixer`: Music / SFX kanalları → ayarlardaki slider'lar (PlayerProfile.musicVolume)
- Adaptif katman (Faz 2): boss dalgasında müzik yoğunlaşır (ikinci katman fade-in)
