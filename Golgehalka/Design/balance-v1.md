# Gölgehalka — Balans v1 (İlk Taslak)

> Bu sayılar oynanış testinde değişecek — amaç makul bir başlangıç noktası.
> Tüm değerler ScriptableObject'lere girilir; kod değişikliği gerektirmez.

## Kahramanlar (Kule Kademeleri)

DPS = hasar × atış hızı. Maliyet ekonomisi: ilk dalga ~200 altınla 2 ucuz kule açtırır.

| Kahraman | Rol | K1 Maliyet | K1 Hasar | Atış/sn | Menzil | K2 (maliyet/hasar) | K3 (maliyet/hasar) | Özellik |
|---|---|---|---|---|---|---|---|---|
| Kael | Yakın DPS + aura | 120 | 18 | 1.0 | 3.5 | 140 / 30 | 220 / 50 | Komşu kulelere +%10 hasar aurası |
| Faelyn | Menzilli tek hedef | 100 | 12 | 1.5 | 6.0 | 120 / 20 | 200 / 34 | Uçana +%50, K3'te üçlü atış |
| Borin | Zırh kırıcı | 110 | 25 | 0.7 | 2.5 | 130 / 42 | 210 / 70 | Zırh delme %40→60→80 |
| Elwin | Alan büyü | 150 | 15 | 0.8 | 5.0 | 180 / 25 | 280 / 42 | Zincir şimşek 2→3→5 sıçrama + yavaşlatma |
| Baldric | Tank/yem | 90 | 10 | 0.9 | 2.0 | 110 / 16 | 180 / 28 | Taunt yarıçapı 3; K3 boynuz: 2 sn stun (45 sn bekleme) |
| Milo | Gizli krit | 100 | 14 | 1.2 | 3.0 | 120 / 24 | 200 / 40 | %20 krit ×3; Gölgehalka riski: aktifken dalga %15 hız |
| Pip | Destek/ekonomi | 80 | 6 | 1.0 | 3.0 | 100 / 10 | 160 / 16 | Dalga sonu +%10 altın; komşuya onarım |
| Sylwen | Alan kontrol | 140 | 8 | 0.8 | 5.5 | 170 / 14 | 260 / 22 | Alan yavaşlatma %25→35→50 |
| Ravox | Zehir/elit avcısı | 130 | 16 | 1.1 | 4.0 | 150 / 26 | 240 / 44 | Zehir yığını (5 sn, yığın başı 3/sn); elite +%40 |

## Düşmanlar

| Düşman | HP | Hız | Zırh | Uçan | Altın | Can Cezası | İlk Görülme |
|---|---|---|---|---|---|---|---|
| Boşluk Yavrusu | 45 | 2.2 | 0 | — | 4 | 1 | Bölüm 1 |
| Kurtbinici | 60 | 4.0 | 0 | — | 6 | 1 | Bölüm 3 |
| Kanpençe | 180 | 1.6 | 0.35 | — | 12 | 1 | Bölüm 5 |
| Gök Dehşeti | 90 | 2.8 | 0 | ✓ | 10 | 1 | Bölüm 7 (Act II) |
| Taş Behemot | 1400 | 0.9 | 0.30 | — | 80 | 5 | Bölüm 4 (mini-boss) |
| Kefen Kralı | 900 | 1.4 | 0.20 | — | 100 | 5 | Bölüm 6 (Act I boss) |
| Molgroth | 2600 | 1.2 | 0.25 | — | 150 | 10 | Bölüm 9 (mini-boss) |
| Malketh | 3200 | 1.0 | 0.15 | — | 200 | 20 | Bölüm 12 (Act II boss) |
| Morwen | 4000 | 1.1 | 0.30 | — | 250 | 20 | Bölüm 15 (mini-boss) |
| Zarok (3 faz) | 3×3000 | 0.8/1.0/1.3 | 0.40/0.25/0.10 | — | — | 20 | Bölüm 18 (final) |

## Act I Dalga Tasarımı (Bölüm 1–6)

Formül: bölüm B, dalga D için sürü sayısı ≈ `4 + 2B + D`. Her bölüm 5-8 dalga.

| Bölüm | Dalga | İçerik | Öğrettiği Şey |
|---|---|---|---|
| 1 | 5 | Sadece Boşluk Yavrusu (5→12/dalga) | Yerleştirme + yükseltme temeli |
| 2 | 6 | Yavru + yoğun sürü dalgaları | Ekonomi yönetimi (Pip tanıtımı) |
| 3 | 6 | + Kurtbinici (hız!) | Yavaşlatma ihtiyacı (Sylwen/Elwin) |
| 4 | 7 | Ortada **Taş Behemot** | Tek hedef DPS + Baldric taunt |
| 5 | 7 | + Kanpençe (zırh!) | Zırh delme (Borin) |
| 6 | 8 | Karışık + **Kefen Kralı** finali | Tüm sistemlerin sınavı |

Boss dalgasında ek sürü gelir (boss asla yalnız değil — hedefleme kararı zorlaşır).

## Ekonomi Sabitleri

- Başlangıç: 200 altın, 20 can (boss bölümlerinde 250 altın)
- Kule satış iadesi: harcananın %60'ı
- Dalga sonu bonusu: 25 + 5×dalga_no
- Bölüm sonu yıldız: 3★ = hiç can kaybetmeden, 2★ ≥ 15 can, 1★ = tamamlama
