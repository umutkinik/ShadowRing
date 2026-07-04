# Meshy → Mixamo → Unity Rig Boru Hattı

Her karakter için ~15 dk manuel iş. Toplu üretim bittiğinde 9 kahraman + 10 düşman işlenecek.

## Genel Akış

```
output/<char>/<char>.glb
   → Blender (kontrol + FBX'e çevir)
   → Mixamo (auto-rig + animasyonlar)
   → Unity (Humanoid import + Animator + prefab)
```

## 1. Blender Kontrolü (karakter başına ~3 dk)

1. `File → Import → glTF 2.0` ile GLB'yi aç.
2. Kontrol listesi:
   - [ ] Ölçek makul mü? (insansı ~1.7-1.8m; `S` ile ölçekle, sonra `Ctrl+A → All Transforms`)
   - [ ] Ayaklar zeminde mi (Z=0)? Model origin'i ayak hizasında olmalı.
   - [ ] Ayrık parçalar var mı? (kopuk silah vb. → silahı AYRI objeye böl: `P → Selection`)
3. **Silahı ayır ve ayrı FBX olarak dışa aktar** — Mixamo silahlı rig'de zorlanır;
   silah Unity'de el kemiğine (hand bone) child olarak takılır.
4. `File → Export → FBX`: karakter (silahsız) `<char>_body.fbx`, silah `<char>_weapon.fbx`.
   FBX ayarı: "Selected Objects", Scale 1.0, Apply Transform ✓.

## 2. Mixamo (karakter başına ~5 dk, ücretsiz Adobe hesabı)

1. mixamo.com → Upload Character → `<char>_body.fbx`.
2. Auto-rigger: çene/bilek/dirsek/diz/kasık işaretçilerini yerleştir → rig oluşur.
   - Cüce/yarımlık gibi kısa karakterlerde işaretçileri orantıya göre ayarla.
   - Dört ayaklılar (kurt, örümcek) Mixamo'da rig OLMAZ — bunlar için aşağıdaki "İstisnalar" bölümü.
3. Şu animasyonları indir (her biri FBX, **Without Skin**, 30 fps):
   | Animasyon | Mixamo arama | Kullanım |
   |---|---|---|
   | Idle | "Idle" (sword and shield idle vb.) | kule beklemede |
   | Attack | "Sword Slash" / "Standing Melee Attack" / okçu için "Standing Aim Recoil" | atış anı |
   | Victory | "Victory" | bölüm sonu vitrin |
   | Death | "Death" (düşmanlar için) | ölüm |
   | Walk | "Walking" (yalnız düşmanlar) | yol yürüyüşü |
4. Bir kez de **With Skin** indir (T-pose/idle) — bu, riglenmiş gövde dosyan olur.

## 3. Unity Import (karakter başına ~5 dk)

1. `Assets/Characters/<char>/` klasörüne: riglenmiş gövde + animasyon FBX'leri + silah FBX.
2. Gövde FBX → Inspector → Rig → **Animation Type: Humanoid** → Apply.
   Animasyon FBX'leri → Humanoid + Avatar kaynağı: gövdenin avatarı ("Copy From Other Avatar").
3. Animator Controller oluştur (`<char>_AC`): Idle (varsayılan) ⇄ Attack (trigger "Attack"),
   düşmanlar için Walk (varsayılan) + Death (trigger "Die").
4. Prefab kur:
   ```
   <char>_prefab
   ├── model (Animator + avatar)
   │   └── ...RightHand kemiği → weapon (silah FBX child)
   ├── Tower.cs veya Enemy.cs
   └── (düşmansa) collider + sağlık çubuğu için ankraj
   ```
5. `Tower.Fire()` içinde `animator.SetTrigger("Attack")` çağrısı eklenir (Faz 1 cilası).

## İstisnalar (insansı olmayanlar)

| Asset | Çözüm |
|---|---|
| Kurtbinici, Morwen (örümcek), Gök Dehşeti | Mixamo desteklemez → Basit **prosedürel animasyon**: gövde yalpalama + bacak IK yerine sallanma (LeanTween/DOTween ile bob & tilt). TD kamera mesafesinde yeterli. Faz 2'de gerekirse Blender'da elle rig. |
| Silahlar/eserler | Rig gerekmez — envanter/vitrin döner platformda sergilenir |
| Platformlar, mekân parçaları | Statik — doğrudan prefab |

## Performans Notları (mobil)

- Meshy çıktısı 4K doku içerir → Unity'de Max Size **1024** (kahraman) / **512** (düşman sürüsü).
- Her karaktere LOD eklemek yerine: TD kamerası sabit olduğundan tek LOD yeterli;
  kalabalık düşman tiplerinde (void_spawn) GPU instancing açık materyal kullan.
- Animator sayısı 60+ olursa `Animator.cullingMode = CullUpdateTransforms`.
