# Gölgehalka — The Sundered Realm

LOTR ruhundan esinlenen, **telif-güvenli** 2.5D kule savunma oyunu. Android + iOS, Unity.
Tasarım incili: paylaşılan artifact (evren, kahramanlar, düşmanlar, silahlar, bölümler,
ihtiyaç ağacı, üretim flow).

## Klasör Yapısı

```
Golgehalka/
├── tools/meshy/            # Meshy text-to-3D üretim hattı
│   ├── meshy_gen.py        #   preview → refine → GLB indir
│   ├── prompts/heroes.json #   9 kahramanın prompt tanımları
│   ├── output/<char>/      #   üretilen .glb + thumbnail + meta
│   └── .env                #   MESHY_API_KEY (git'e girmez!)
├── Localization/           # 7 dil JSON tabloları (EN varsayılan)
└── Unity-Assets/Scripts/   # Unity projesine kopyalanacak çekirdek kod
    ├── Core/               #   GameManager, Economy, Wave, Path, Build
    ├── Combat/             #   Tower, Enemy, Projectile
    └── Data/               #   ScriptableObject tanımları (Hero/Enemy/Level)
```

## Kurulum (Faz 0)

1. **Unity Hub** → yeni proje: `3D (URP)`, Unity 2022 LTS+, isim `Golgehalka`.
2. `Unity-Assets/Scripts/` klasörünü projenin `Assets/` altına kopyala.
3. Package Manager → **Localization** paketi kur; `Localization/README.md`'deki adımları izle.
4. Sahne kur: zemin + `Path` (waypoint'ler) + birkaç `PlacementNode` + `GameManager`,
   `EconomyManager`, `WaveManager`, `BuildManager` objeleri.
5. İlk `HeroDefinition` + `EnemyDefinition` + `LevelDefinition` asset'lerini oluştur
   (Create → Golgehalka menüsü) ve bağla.
6. Prototip görselleri: Kenney/Quaternius CC0; kahramanlar Meshy'den geldikçe değişir.

## Meshy Üretimi

```bash
cd tools/meshy
python3 meshy_gen.py --list            # karakterleri gör
python3 meshy_gen.py --char borin      # üret (preview+refine ≈ 15-20 kredi)
```

Çıktı: `output/borin/borin.glb` → Blender'da kontrol → [Mixamo](https://www.mixamo.com)
auto-rig + idle/attack animasyonları → Unity'ye import.

⚠️ API key: `.env` git'e girmez. Üretim (client) tarafında key ASLA gömülmez —
Meshy sadece geliştirme aşamasında asset üretmek için kullanılır.

## Diller

EN (varsayılan) · DE · RU · ZH-Hans · HI · AR (RTL) · TR — bkz. `Localization/README.md`.
