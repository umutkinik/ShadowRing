using Golgehalka.Combat;
using Golgehalka.Core;
using Golgehalka.Data;
using Golgehalka.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Golgehalka.EditorTools
{
    /// TEK TIKLA PROTOTİP: menüden "Gölgehalka → Prototip Sahne Kur".
    /// v3: 3 kahraman (Borin/Faelyn/Elwin), 5 düşman tipi, Act I'in 6 bölümü,
    /// bölüm seçici — hepsi balance-v1.md değerleriyle.
    public static class PrototypeSceneBuilder
    {
        private const string DataDir = "Assets/Data";
        private const string PrefabDir = "Assets/Prefabs";
        private const string ModelDir = "Assets/Models/Characters";
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("Gölgehalka/Prototip Sahne Kur")]
        public static void Build()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Scenes");

            // ---- 1) ORTAK PREFABLAR ----
            GameObject projectilePrefab = MakePrimitivePrefab(
                PrimitiveType.Sphere, "Projectile", 0.25f, new Color(1f, 0.85f, 0.3f),
                go => { Object.DestroyImmediate(go.GetComponent<Collider>()); go.AddComponent<Projectile>(); });

            // ---- 2) DÜŞMAN PREFAB + TANIMLARI (balance-v1.md) ----
            var voidSpawn = MakeEnemy("VoidSpawn", "void_spawn", new Color(0.5f, 0.2f, 0.6f), 1f,
                d => { d.maxHealth = 45; d.moveSpeed = 2.2f; d.goldReward = 4; });
            var wolfrider = MakeEnemy("Wolfrider", "wolfrider", new Color(0.6f, 0.45f, 0.2f), 1.1f,
                d => { d.maxHealth = 60; d.moveSpeed = 4f; d.goldReward = 6; });
            var bloodclaw = MakeEnemy("Bloodclaw", "bloodclaw", new Color(0.7f, 0.15f, 0.15f), 1.15f,
                d => { d.maxHealth = 180; d.armor = 0.35f; d.moveSpeed = 1.6f; d.goldReward = 12; });
            var behemoth = MakeEnemy("StoneBehemoth", "stone_behemoth", new Color(0.45f, 0.45f, 0.5f), 2.2f,
                d => { d.maxHealth = 1400; d.armor = 0.3f; d.moveSpeed = 0.9f; d.goldReward = 80; d.livesCost = 5; d.isBoss = true; });
            var shroudKing = MakeEnemy("ShroudKing", "shroud_king", new Color(0.2f, 0.1f, 0.3f), 1.6f,
                d => { d.maxHealth = 900; d.armor = 0.2f; d.moveSpeed = 1.4f; d.goldReward = 100; d.livesCost = 5; d.isBoss = true; });

            // ---- 3) KAHRAMANLAR (kule prefab'ı + tanım) ----
            var borin = MakeHero("borin", new Color(0.4f, 0.55f, 0.8f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.armorPenetration = 0.4f; h.projectileSpeed = 14f;
                h.tiers = Tiers(110, 25, 4.5f, 0.7f, 130, 42, 5f, 0.8f, 210, 70, 5.5f, 0.9f);
            });
            var faelyn = MakeHero("faelyn", new Color(0.35f, 0.65f, 0.35f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.canTargetFlying = true; h.projectileSpeed = 22f;
                h.tiers = Tiers(100, 12, 6f, 1.5f, 120, 20, 6.5f, 1.6f, 200, 34, 7f, 1.8f);
            });
            var elwin = MakeHero("elwin", new Color(0.55f, 0.5f, 0.75f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Magic; h.projectileSpeed = 16f; // büyü zırh deler
                h.tiers = Tiers(150, 15, 5f, 0.8f, 180, 25, 5.2f, 0.85f, 280, 42, 5.5f, 0.9f);
            });
            // Kalan 6 kahraman — balance-v1.md taban statları
            // (aura/taunt/krit/zehir yığını gibi özel mekanikler Faz 2'de eklenecek)
            var kael = MakeHero("kael", new Color(0.3f, 0.45f, 0.3f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.armorPenetration = 0.1f; h.projectileSpeed = 16f;
                h.tiers = Tiers(120, 18, 3.5f, 1f, 140, 30, 4f, 1.05f, 220, 50, 4.5f, 1.1f);
            });
            var baldric = MakeHero("baldric", new Color(0.7f, 0.55f, 0.2f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.projectileSpeed = 14f;
                h.tiers = Tiers(90, 10, 2.5f, 0.9f, 110, 16, 2.8f, 0.95f, 180, 28, 3.2f, 1f);
            });
            var milo = MakeHero("milo", new Color(0.45f, 0.6f, 0.7f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.projectileSpeed = 18f;
                h.tiers = Tiers(100, 14, 3f, 1.2f, 120, 24, 3.4f, 1.3f, 200, 40, 3.8f, 1.4f);
            });
            var pip = MakeHero("pip", new Color(0.75f, 0.6f, 0.35f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Physical; h.projectileSpeed = 14f;
                h.tiers = Tiers(80, 6, 3f, 1f, 100, 10, 3.3f, 1.05f, 160, 16, 3.6f, 1.1f);
            });
            var sylwen = MakeHero("sylwen", new Color(0.85f, 0.85f, 0.95f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Magic; h.projectileSpeed = 16f;
                h.tiers = Tiers(140, 8, 5.5f, 0.8f, 170, 14, 5.8f, 0.85f, 260, 22, 6.2f, 0.9f);
            });
            var ravox = MakeHero("ravox", new Color(0.3f, 0.3f, 0.35f), projectilePrefab, h =>
            {
                h.damageType = DamageType.Poison; h.projectileSpeed = 18f; // zehir zırh deler
                h.tiers = Tiers(130, 16, 4f, 1.1f, 150, 26, 4.4f, 1.15f, 240, 44, 4.8f, 1.2f);
            });

            // ---- 4) ACT I — 6 BÖLÜM (balance-v1.md dalga tasarımı) ----
            SpawnEntry S(EnemyDefinition d, int n, float iv = 0.8f, float delay = 0.5f) =>
                new SpawnEntry { enemy = d, count = n, interval = iv, startDelay = delay };
            WaveDefinition W(int no, params SpawnEntry[] e) =>
                new WaveDefinition { rewardGold = 25 + 5 * no, entries = e };

            // Her bölümün KENDİ haritası: yol (P) + platformlar (N, yoldan uzak)
            var levels = new[]
            {
                // B1 — Klasik S-kıvrımı
                MakeLevel(1, new[] {
                    W(1, S(voidSpawn, 5)), W(2, S(voidSpawn, 7)), W(3, S(voidSpawn, 9)),
                    W(4, S(voidSpawn, 11)), W(5, S(voidSpawn, 14)) },
                    P(-10, 5, -2, 5, 1, 2, -3, -1, 2, -4, 10, -4),
                    N(-7, 3.2f, -4, 6.8f, 1.5f, 4.2f, -3.5f, 1.2f, 0.5f, -1.2f, -5, -3, 4, -2.2f, 6, -5.8f)),

                // B2 — Yılan (üçlü yatay şerit)
                MakeLevel(2, new[] {
                    W(1, S(voidSpawn, 8)), W(2, S(voidSpawn, 10)), W(3, S(voidSpawn, 12, 0.6f)),
                    W(4, S(voidSpawn, 14)), W(5, S(voidSpawn, 16, 0.6f)), W(6, S(voidSpawn, 18, 0.5f)) },
                    P(-10, 6, 6, 6, 6, 2, -6, 2, -6, -2, 6, -2, 6, -6, -10, -6),
                    N(-8, 4, -4, 4, 2, 4, -2, 0, 3, 0, 8.5f, 0, -4, -4, 2, -4)),

                // B3 — U dönüşü
                MakeLevel(3, new[] {
                    W(1, S(voidSpawn, 8), S(wolfrider, 3, 0.6f, 3f)), W(2, S(voidSpawn, 10), S(wolfrider, 4, 0.6f, 3f)),
                    W(3, S(wolfrider, 6, 0.5f)), W(4, S(voidSpawn, 12), S(wolfrider, 5, 0.5f, 2f)),
                    W(5, S(voidSpawn, 14), S(wolfrider, 6, 0.5f, 2f)), W(6, S(wolfrider, 8, 0.4f), S(voidSpawn, 8, 0.8f, 4f)) },
                    P(-10, -5, 3, -5, 3, 0, -3, 0, -3, 5, -10, 5),
                    N(0, -2.8f, 5.5f, -2.5f, -6, -2.8f, 0, 2.3f, -6, 2.5f, 1, 5.2f, -1, -7, 5.5f, 2)),

                // B4 — Tarak (Behemot bölümü)
                MakeLevel(4, new[] {
                    W(1, S(voidSpawn, 12)), W(2, S(voidSpawn, 14), S(wolfrider, 5, 0.5f, 3f)),
                    W(3, S(wolfrider, 8, 0.5f)), W(4, S(behemoth, 1, 1f, 1f), S(voidSpawn, 8, 0.8f, 3f)),
                    W(5, S(voidSpawn, 16), S(wolfrider, 6, 0.5f, 3f)), W(6, S(voidSpawn, 18, 0.6f), S(wolfrider, 8, 0.4f, 4f)),
                    W(7, S(voidSpawn, 20, 0.5f), S(wolfrider, 10, 0.4f, 3f)) },
                    P(-10, 6, -3, 6, -3, -6, 4, -6, 4, 6, 10, 6),
                    N(-5.5f, 4, -1, 3, -1, -3, 1, -4.2f, 6.5f, 3, 6.5f, -3, 1, 0, -5.5f, -3)),

                // B5 — Çift viraj (Kanpençe bölümü)
                MakeLevel(5, new[] {
                    W(1, S(bloodclaw, 4, 1.2f), S(voidSpawn, 10, 0.7f, 3f)), W(2, S(bloodclaw, 6, 1f)),
                    W(3, S(wolfrider, 8, 0.5f), S(bloodclaw, 4, 1f, 4f)), W(4, S(voidSpawn, 16, 0.6f), S(bloodclaw, 6, 1f, 3f)),
                    W(5, S(bloodclaw, 8, 0.9f)), W(6, S(behemoth, 1, 1f, 1f), S(bloodclaw, 4, 1f, 4f)),
                    W(7, S(bloodclaw, 10, 0.8f), S(wolfrider, 8, 0.4f, 5f)) },
                    P(-10, 0, -4, 0, -4, 5, 2, 5, 2, -5, 8, -5, 8, 0, 10, 0),
                    N(-7, 2, -6.5f, -2.5f, -1, 2.8f, 0, 7, 4.5f, 2, 4.5f, -2.8f, -0.5f, -3, 10, -2.5f)),

                // B6 — Kale kuşatması (Kefen Kralı finali — yol merkeze, "kapıya" iner)
                MakeLevel(6, new[] {
                    W(1, S(voidSpawn, 14, 0.6f)), W(2, S(wolfrider, 8, 0.5f), S(voidSpawn, 10, 0.7f, 3f)),
                    W(3, S(bloodclaw, 6, 1f), S(wolfrider, 6, 0.5f, 4f)), W(4, S(behemoth, 1, 1f, 1f), S(voidSpawn, 12, 0.6f, 3f)),
                    W(5, S(bloodclaw, 8, 0.9f), S(voidSpawn, 12, 0.6f, 4f)), W(6, S(wolfrider, 12, 0.4f)),
                    W(7, S(bloodclaw, 10, 0.8f), S(wolfrider, 8, 0.4f, 5f)),
                    W(8, S(shroudKing, 1, 1f, 1f), S(voidSpawn, 15, 0.5f, 3f), S(bloodclaw, 6, 1f, 6f)) },
                    P(-10, -6, 6, -6, 6, 6, -6, 6, -6, 0, 2, 0, 2, -3),
                    N(-2, -3.5f, 3.8f, -3.6f, 8.5f, 0, 3, 3, -3, 3, -8.5f, 3, -4.5f, -2, -8, -3.5f)),
            };

            // Tema ataması: Act I dekorları (ağaç kümeleri + gözetleme kulesi) + zemin tonları
            var act1Decor = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Env/act1_trees.glb"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Env/act1_watchtower.glb"),
            };
            var groundTones = new[]
            {
                new Color(0.25f, 0.32f, 0.22f), new Color(0.23f, 0.34f, 0.20f),
                new Color(0.27f, 0.33f, 0.19f), new Color(0.23f, 0.30f, 0.24f),
                new Color(0.26f, 0.31f, 0.18f), new Color(0.21f, 0.27f, 0.22f),
            };
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].decorPrefabs = act1Decor;
                levels[i].decorCount = 9;
                levels[i].groundColor = groundTones[i];
                EditorUtility.SetDirty(levels[i]);
            }
            AssetDatabase.SaveAssets();

            // ---- 5) SAHNE ----
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.transform.position = new Vector3(0, 14, -9);
            cam.transform.rotation = Quaternion.Euler(55, 0, 0);
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.13f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2.2f, 1, 2.2f);
            Tint(ground, new Color(0.25f, 0.32f, 0.22f));

            // Yol objesi boş başlar — MapBuilder çalışma anında bölüme göre kurar
            var pathGO = new GameObject("WaypointPath");
            var path = pathGO.AddComponent<WaypointPath>();

            var mgr = new GameObject("_Managers");
            mgr.AddComponent<GameManager>();
            mgr.AddComponent<EconomyManager>();

            var mapBuilder = mgr.AddComponent<MapBuilder>();
            mapBuilder.path = path;
            mapBuilder.groundRenderer = ground.GetComponent<Renderer>();

            var wave = mgr.AddComponent<WaveManager>();
            wave.level = levels[0];
            wave.path = path;
            wave.mapBuilder = mapBuilder;

            var build = mgr.AddComponent<BuildManager>();
            var bso = new SerializedObject(build);
            bso.FindProperty("towerBasePrefab").objectReferenceValue = borin.towerPrefab;
            bso.ApplyModifiedPropertiesWithoutUndo();

            var hud = BuildHUD(wave, build,
                new[] { borin, faelyn, elwin, kael, baldric, milo, pip, sylwen, ravox }, levels);

            EditorSceneManager.SaveScene(scene, ScenePath);

            bool ok = wave.level != null && wave.path != null && hud.heroes.Length == 9 && hud.levels.Length == 6;
            EditorUtility.DisplayDialog("Gölgehalka",
                ok
                    ? "Act I hazır ve doğrulandı ✓\n\n9 kahraman · 5 düşman tipi · 6 bölüm · kule paneli\n\n" +
                      "Play → bölüm seç → kahraman seç → platforma tıkla → kuleye dokunarak yükselt/sat."
                    : "DİKKAT: bazı referanslar atanamadı! Console'a bak.",
                ok ? "Başlıyoruz!" : "Tamam");
        }

        // ================= FABRİKALAR =================

        private static EnemyDefinition MakeEnemy(
            string defName, string id, Color color, float modelScale, System.Action<EnemyDefinition> tune)
        {
            GameObject prefab = MakePrimitivePrefab(
                PrimitiveType.Capsule, "Enemy_" + defName, 0.6f, color,
                go =>
                {
                    go.AddComponent<Enemy>();
                    // Ayak hizası: kapsül merkezi yolda y=0.1'de durur; -0.13 lokal ofset
                    // model ayaklarını zemin üstüne (≈y=0.02) oturtur (-1 gömüyordu!)
                    if (AttachModel(go, ModelDir + "/" + id + ".glb",
                        new Vector3(0, -0.13f, 0), Vector3.one * (modelScale / 0.6f)))
                        go.GetComponent<MeshRenderer>().enabled = false;
                });

            return CreateAsset<EnemyDefinition>(DataDir + "/" + defName + ".asset", d =>
            {
                d.enemyId = id; d.nameKey = "enemy." + id + ".name";
                d.prefab = prefab; d.livesCost = 1;
                tune(d);
            });
        }

        private static HeroDefinition MakeHero(
            string id, Color color, GameObject projectile, System.Action<HeroDefinition> tune)
        {
            GameObject towerPrefab = MakeTowerPrefab(id, color);
            return CreateAsset<HeroDefinition>(DataDir + "/Hero_" + id + ".asset", h =>
            {
                h.heroId = id; h.nameKey = "hero." + id + ".name";
                h.projectilePrefab = projectile; h.towerPrefab = towerPrefab;
                tune(h);
            });
        }

        /// Kule prefab'ı: 3 kademe platformu (taş→bronz→altın) + üstünde kahraman.
        /// Platform GLB'leri yoksa renkli silindire düşer (asla kırılmaz).
        private static GameObject MakeTowerPrefab(string id, Color fallback)
        {
            var go = new GameObject("Tower_" + id);
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.9f, 0);
            col.size = new Vector3(1.5f, 2f, 1.5f);

            var fire = new GameObject("FirePoint");
            fire.transform.SetParent(go.transform);
            fire.transform.localPosition = new Vector3(0, 2.2f, 0);

            var tower = go.AddComponent<Tower>();

            // Kademe platformları
            string[] tierFiles = { "tier1_base", "tier2_base", "tier3_base" };
            var tierGOs = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Env/" + tierFiles[i] + ".glb");
                GameObject tierGO;
                if (model != null)
                {
                    tierGO = Object.Instantiate(model, go.transform);
                    tierGO.transform.localScale = Vector3.one * 0.75f;
                }
                else
                {
                    tierGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Object.DestroyImmediate(tierGO.GetComponent<Collider>());
                    tierGO.transform.SetParent(go.transform);
                    tierGO.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
                    Tint(tierGO, fallback);
                }
                tierGO.name = "Tier" + (i + 1);
                tierGO.transform.localPosition = Vector3.zero;
                tierGO.SetActive(i == 0);
                tierGOs[i] = tierGO;
            }

            // Kahraman modeli platformun üstünde
            GameObject heroGO;
            var heroModel = AssetDatabase.LoadAssetAtPath<GameObject>(ModelDir + "/" + id + ".glb");
            if (heroModel != null)
            {
                heroGO = Object.Instantiate(heroModel, go.transform);
            }
            else
            {
                heroGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Object.DestroyImmediate(heroGO.GetComponent<Collider>());
                heroGO.transform.SetParent(go.transform);
                Tint(heroGO, fallback);
            }
            heroGO.name = "HeroModel";
            heroGO.transform.localPosition = new Vector3(0, 0.32f, 0);

            // Serileştirilmiş alanları bağla
            var so = new SerializedObject(tower);
            so.FindProperty("firePoint").objectReferenceValue = fire.transform;
            so.FindProperty("heroModel").objectReferenceValue = heroGO.transform;
            var arr = so.FindProperty("tierVisuals");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = tierGOs[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = PrefabDir + "/Tower_" + id + ".prefab";
            AssetDatabase.DeleteAsset(prefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static TowerTier[] Tiers(
            int c1, float d1, float r1, float f1,
            int c2, float d2, float r2, float f2,
            int c3, float d3, float r3, float f3) => new[]
        {
            new TowerTier { cost = c1, damage = d1, range = r1, fireRate = f1 },
            new TowerTier { cost = c2, damage = d2, range = r2, fireRate = f2 },
            new TowerTier { cost = c3, damage = d3, range = r3, fireRate = f3 },
        };

        private static LevelDefinition MakeLevel(
            int no, WaveDefinition[] waves, Vector3[] wps, Vector3[] nodes) =>
            CreateAsset<LevelDefinition>(DataDir + "/Act1_Level" + no + ".asset", l =>
            {
                l.levelId = "act1_level" + no;
                l.nameKey = "campaign.act1.title";
                l.startingGold = no >= 4 ? 250 : 200;
                l.startingLives = 20;
                l.waves = waves;
                l.waypoints = wps;
                l.nodePositions = nodes;
            });

        /// x,z çiftlerinden waypoint dizisi (y=0.1 yol yüksekliği).
        private static Vector3[] P(params float[] c)
        {
            var v = new Vector3[c.Length / 2];
            for (int i = 0; i < v.Length; i++) v[i] = new Vector3(c[i * 2], 0.1f, c[i * 2 + 1]);
            return v;
        }

        /// x,z çiftlerinden platform dizisi (y=0.15).
        private static Vector3[] N(params float[] c)
        {
            var v = new Vector3[c.Length / 2];
            for (int i = 0; i < v.Length; i++) v[i] = new Vector3(c[i * 2], 0.15f, c[i * 2 + 1]);
            return v;
        }

        // ================= HUD (Canvas + TMP + lokalizasyon) =================

        private static MatchHUD BuildHUD(
            WaveManager wave, BuildManager build, HeroDefinition[] heroes, LevelDefinition[] levels)
        {
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var canvasGO = new GameObject("HUD_Canvas",
                typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasGO.AddComponent<MatchHUD>();
            hud.waveManager = wave;
            hud.buildManager = build;
            hud.heroes = heroes;
            hud.levels = levels;

            var t = canvasGO.transform;
            Color barBg = new Color(0.05f, 0.05f, 0.09f, 0.55f);
            Color btnBg = new Color(0.16f, 0.15f, 0.22f, 0.95f);

            // --- Üst bar ---
            var top = UIPanel(t, "TopBar", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 84), barBg);
            hud.goldText = UIText(top, "Gold", "Altın: 0", 36, new Vector2(30, 0), new Vector2(330, 70), TMPro.TextAlignmentOptions.Left);
            hud.livesText = UIText(top, "Lives", "Can: 20", 36, new Vector2(380, 0), new Vector2(260, 70), TMPro.TextAlignmentOptions.Left);
            hud.waveText = UIText(top, "Wave", "Dalga: 0/0", 36, new Vector2(660, 0), new Vector2(320, 70), TMPro.TextAlignmentOptions.Left);
            hud.levelText = UIText(top, "Level", "-", 36, new Vector2(1010, 0), new Vector2(300, 70), TMPro.TextAlignmentOptions.Left);

            // Hız + dil üst barda (alt bar 9 kahramana ayrıldı)
            hud.speedButtons = new UnityEngine.UI.Button[3];
            string[] speedLabels = { "1×", "2×", "3×" };
            for (int i = 0; i < 3; i++)
            {
                var (sb, _) = UIButton(top, "Speed" + speedLabels[i], speedLabels[i],
                    new Vector2(1370 + i * 106, 0), new Vector2(94, 62), btnBg, null);
                hud.speedButtons[i] = sb;
            }
            var (langBtn, langLbl) = UIButton(top, "Lang", "EN",
                new Vector2(1710, 0), new Vector2(104, 62), btnBg, null);
            hud.langButton = langBtn;
            hud.langButtonLabel = langLbl;

            // --- Bölüm seçici (ilk dalga öncesi) ---
            var lvlRow = UIPanel(t, "LevelRow", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -88), new Vector2(0, 76), new Color(0, 0, 0, 0.30f));
            hud.levelRow = lvlRow.gameObject;
            hud.levelButtons = new UnityEngine.UI.Button[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                var (b, _) = UIButton(lvlRow, "Lvl" + (i + 1), "B" + (i + 1),
                    new Vector2(40 + i * 120, 0), new Vector2(104, 60), btnBg, null);
                hud.levelButtons[i] = b;
            }

            // --- Alt bar ---
            var bottom = UIPanel(t, "BottomBar", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 96), barBg);

            var (nextBtn, _) = UIButton(bottom, "NextWave", "Sonraki Dalga",
                new Vector2(30, 0), new Vector2(310, 72), btnBg, "hud.next_wave");
            hud.nextWaveButton = nextBtn;

            // 9 kahraman — kompakt butonlar
            hud.heroButtons = new UnityEngine.UI.Button[heroes.Length];
            hud.heroButtonLabels = new TMPro.TMP_Text[heroes.Length];
            for (int i = 0; i < heroes.Length; i++)
            {
                var (b, lbl) = UIButton(bottom, "Hero_" + heroes[i].heroId, heroes[i].heroId,
                    new Vector2(370 + i * 166, 0), new Vector2(158, 72), btnBg, null);
                lbl.fontSize = 21;
                hud.heroButtons[i] = b;
                hud.heroButtonLabels[i] = lbl;
            }

            // --- Sonuç panelleri ---
            hud.victoryPanel = BuildResultPanel(t, "VictoryPanel", "game.victory",
                out var vStars, out var vBtn, btnBg);
            hud.starsText = vStars;
            hud.victoryButton = vBtn;

            hud.defeatPanel = BuildResultPanel(t, "DefeatPanel", "game.defeat",
                out _, out var dBtn, btnBg);
            hud.defeatButton = dBtn;

            // --- Kule yükseltme/satma paneli (sağ orta) ---
            var tp = UIPanel(t, "TowerPanel", new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-16, 0), new Vector2(360, 420),
                new Color(0.05f, 0.05f, 0.09f, 0.85f));
            hud.towerPanel = tp.gameObject;
            hud.towerTitle = UIText(tp, "Title", "-", 34, new Vector2(20, 165), new Vector2(320, 56), TMPro.TextAlignmentOptions.Left);
            hud.towerInfo = UIText(tp, "Info", "-", 24, new Vector2(20, 75), new Vector2(320, 110), TMPro.TextAlignmentOptions.TopLeft);
            var (upBtn, upLbl) = UIButton(tp, "Upgrade", "Yükselt", new Vector2(20, -30), new Vector2(320, 62), btnBg, null);
            hud.upgradeButton = upBtn; hud.upgradeLabel = upLbl;
            var (sellBtn, sellLbl) = UIButton(tp, "Sell", "Sat", new Vector2(20, -98), new Vector2(320, 56), btnBg, null);
            hud.sellButton = sellBtn; hud.sellLabel = sellLbl;
            var (closeBtn, _) = UIButton(tp, "Close", "Kapat", new Vector2(20, -162), new Vector2(320, 48), btnBg, "ui.close");
            hud.towerCloseButton = closeBtn;

            return hud;
        }

        private static GameObject BuildResultPanel(
            Transform parent, string name, string titleKey,
            out TMPro.TMP_Text stars, out UnityEngine.UI.Button button, Color btnBg)
        {
            var panel = UIPanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 400), new Color(0.04f, 0.04f, 0.08f, 0.88f));

            var title = UIText(panel, "Title", titleKey, 60, Vector2.zero, new Vector2(640, 90), TMPro.TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);   // pivot da merkez — kayma düzeltmesi
            title.rectTransform.anchoredPosition = new Vector2(0, 110);
            var lt = title.gameObject.AddComponent<LocalizedText>();
            lt.key = titleKey;

            stars = UIText(panel, "Stars", "", 54, Vector2.zero, new Vector2(640, 80), TMPro.TextAlignmentOptions.Center);
            stars.rectTransform.anchorMin = stars.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            stars.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            stars.rectTransform.anchoredPosition = new Vector2(0, 20);

            var (b, _) = UIButton(panel, "Action", "Play", new Vector2(0, 0), new Vector2(300, 76), btnBg, "menu.play");
            var brt = b.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(0, -110);
            button = b;

            return panel.gameObject;
        }

        // --- UI yardımcıları ---
        private static RectTransform UIPanel(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<UnityEngine.UI.Image>().color = color;
            return rt;
        }

        private static TMPro.TMP_Text UIText(Transform parent, string name, string text,
            float fontSize, Vector2 pos, Vector2 size, TMPro.TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(TMPro.TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var tmp = go.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.alignment = align;
            tmp.color = new Color(0.92f, 0.90f, 0.84f);
            return tmp;
        }

        private static (UnityEngine.UI.Button, TMPro.TMP_Text) UIButton(Transform parent, string name,
            string label, Vector2 pos, Vector2 size, Color bg, string locKey)
        {
            var go = new GameObject(name, typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = bg;
            var btn = go.GetComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = img;

            var lblGo = new GameObject("Label", typeof(TMPro.TextMeshProUGUI));
            lblGo.transform.SetParent(go.transform, false);
            var lrt = lblGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var tmp = lblGo.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 32;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.92f, 0.90f, 0.84f);
            if (!string.IsNullOrEmpty(locKey))
                lblGo.AddComponent<LocalizedText>().key = locKey;

            return (btn, tmp);
        }

        // ================= SAHNE PARÇALARI =================

        // ================= YARDIMCILAR =================

        /// GLB modelini (glTFast importu) verilen objenin altına çocuk olarak ekler.
        private static bool AttachModel(GameObject parent, string assetPath, Vector3 localPos, Vector3 localScale)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null) return false;
            var inst = Object.Instantiate(model, parent.transform);
            inst.name = "Model";
            inst.transform.localPosition = localPos;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = localScale;
            return true;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static T CreateAsset<T>(string assetPath, System.Action<T> init) where T : ScriptableObject
        {
            AssetDatabase.DeleteAsset(assetPath);
            var so = ScriptableObject.CreateInstance<T>();
            init(so);
            AssetDatabase.CreateAsset(so, assetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static GameObject MakePrimitivePrefab(
            PrimitiveType type, string name, float scale, Color color, System.Action<GameObject> setup)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.localScale = Vector3.one * scale;
            Tint(go, color);
            setup(go);
            string prefabPath = PrefabDir + "/" + name + ".prefab";
            AssetDatabase.DeleteAsset(prefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void Tint(GameObject go, Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { color = c };
            EnsureFolder("Assets", "Prefabs");
            string matPath = PrefabDir + "/Mat_" + go.name + "_" + ColorUtility.ToHtmlStringRGB(c) + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing == null) { AssetDatabase.CreateAsset(mat, matPath); existing = mat; }
            go.GetComponent<Renderer>().sharedMaterial = existing;
        }
    }
}
