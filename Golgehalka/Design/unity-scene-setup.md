# Unity Sahne Kurulumu — İlk Oynanabilir Bölüm (Adım Adım)

Hedef: `Unity-Assets/Scripts/` kodlarıyla 15 dakikada çalışan bir prototip sahne.

## 0. Proje

1. Unity Hub → New Project → **3D (URP)** → Unity 2022.3 LTS+ → isim `Golgehalka`.
2. `Unity-Assets/Scripts/` klasörünü `Assets/` altına kopyala (derleme hatasız geçmeli).
3. Build Settings → platform Android/iOS'a geç (URP mobil ayarları otomatik).

## 1. Kamera (2.5D)

- Main Camera: Position `(0, 12, -8)`, Rotation `(55, 0, 0)`, **Projection: Orthographic**, Size `7`.
- Bu açı klasik TD izometrik hissi verir; Meshy modelleri 3D olduğu için parallax doğal olur.

## 2. Zemin & Yol

1. Plane (20×20) → zemin. Prototipte gri; sonra `act1` çevre asset'leri.
2. Boş obje `Path` → `WaypointPath.cs` ekle. Altına 5-8 boş obje (`WP0`, `WP1`...) — düşman güzergâhı.
   Waypoint'leri sahnede S-kıvrımı olacak şekilde diz; Gizmo sarı çizgiyle gösterir.
3. Yol üzerine görsel işaret için ince uzun küpler (prototip).

## 3. Yerleştirme Noktaları

- Küp (1×0.2×1) → `PlacementNode.cs` ekle → Layer: `PlacementNode` (yeni layer oluştur).
- Yolun etrafına 6-10 adet diz. Prefab yap.

## 4. Yöneticiler

Boş obje `_Managers` altında:
| Obje | Bileşen | Not |
|---|---|---|
| GameManager | `GameManager.cs` | startingLives: 20 |
| EconomyManager | `EconomyManager.cs` | startingGold: 200 |
| WaveManager | `WaveManager.cs` | level + path referansı bağla |
| BuildManager | `BuildManager.cs` | towerBasePrefab + nodeLayer bağla |
| AdsManager | `AdsManager.cs` | SDK'sız da çalışır (dev modu) |
| PurchaseManager | `PurchaseManager.cs` | editor testte direkt hak verir |
| LevelFlow | `LevelFlow.cs` | sahne adlarını gir |

## 5. Veri Asset'leri

1. `Assets/Data/` klasörü aç. Project → Create → **Golgehalka** menüsü:
2. **Enemy Definition** → `VoidSpawn`: HP 45, hız 2.2, zırh 0, altın 4 (bkz. balance-v1.md).
   - prefab: şimdilik Kapsül + `Enemy.cs` (Meshy modeli riglenince değişir).
3. **Hero Definition** → `Borin`: balance-v1.md'deki kademe değerleri.
   - projectilePrefab: küçük Küre + `Projectile.cs`.
4. **Level Definition** → `Act1_Level1`: 5 dalga, her dalgada VoidSpawn (5, 7, 9, 11, 14 adet).

## 6. Kule Tabanı Prefabı

- Boş obje `TowerBase` → `Tower.cs` + child `FirePoint` (yükseklik ~1.5).
- Prototipte Silindir; Meshy `tier1_base` + kahraman modeli gelince altına oturur.

## 7. Geçici UI (prototip)

- Canvas → TextMeshPro: altın/can/dalga sayaçları (Economy/GameManager event'lerine bağla).
- Button "Sonraki Dalga" → `WaveManager.StartNextWave()`.
- Kahraman seç butonu → `BuildManager.SelectHero(borinDef)`.

## 8. Test Kontrol Listesi

- [ ] Dalga başlıyor, düşmanlar yolu izliyor
- [ ] Node'a dokununca (editörde Input.touch yerine mouse için `Input.GetMouseButtonDown(0)` geçici ekle) kule kuruluyor, altın düşüyor
- [ ] Kule ateş ediyor, düşman ölünce altın geliyor
- [ ] Düşman yolu bitirince can düşüyor; 0 can → Defeat
- [ ] Tüm dalgalar bitince → Victory
- [ ] Victory'den devam → interstitial hook'u çalışıyor (log'da görünür)

> Not: `BuildManager` mobil dokunuş dinler. Editör testinde `Input.touchCount` bloğunu
> mouse fallback ile genişletmek gerekir — Faz 0 sonunda Input System paketine geçilecek.
