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

            // ---- 4) ACT I — 6 BÖLÜM (balance-v1.md dalga tasarımı) ----
            SpawnEntry S(EnemyDefinition d, int n, float iv = 0.8f, float delay = 0.5f) =>
                new SpawnEntry { enemy = d, count = n, interval = iv, startDelay = delay };
            WaveDefinition W(int no, params SpawnEntry[] e) =>
                new WaveDefinition { rewardGold = 25 + 5 * no, entries = e };

            var levels = new[]
            {
                MakeLevel(1, new[] {
                    W(1, S(voidSpawn, 5)), W(2, S(voidSpawn, 7)), W(3, S(voidSpawn, 9)),
                    W(4, S(voidSpawn, 11)), W(5, S(voidSpawn, 14)) }),
                MakeLevel(2, new[] {
                    W(1, S(voidSpawn, 8)), W(2, S(voidSpawn, 10)), W(3, S(voidSpawn, 12, 0.6f)),
                    W(4, S(voidSpawn, 14)), W(5, S(voidSpawn, 16, 0.6f)), W(6, S(voidSpawn, 18, 0.5f)) }),
                MakeLevel(3, new[] {
                    W(1, S(voidSpawn, 8), S(wolfrider, 3, 0.6f, 3f)), W(2, S(voidSpawn, 10), S(wolfrider, 4, 0.6f, 3f)),
                    W(3, S(wolfrider, 6, 0.5f)), W(4, S(voidSpawn, 12), S(wolfrider, 5, 0.5f, 2f)),
                    W(5, S(voidSpawn, 14), S(wolfrider, 6, 0.5f, 2f)), W(6, S(wolfrider, 8, 0.4f), S(voidSpawn, 8, 0.8f, 4f)) }),
                MakeLevel(4, new[] {
                    W(1, S(voidSpawn, 12)), W(2, S(voidSpawn, 14), S(wolfrider, 5, 0.5f, 3f)),
                    W(3, S(wolfrider, 8, 0.5f)), W(4, S(behemoth, 1, 1f, 1f), S(voidSpawn, 8, 0.8f, 3f)),
                    W(5, S(voidSpawn, 16), S(wolfrider, 6, 0.5f, 3f)), W(6, S(voidSpawn, 18, 0.6f), S(wolfrider, 8, 0.4f, 4f)),
                    W(7, S(voidSpawn, 20, 0.5f), S(wolfrider, 10, 0.4f, 3f)) }),
                MakeLevel(5, new[] {
                    W(1, S(bloodclaw, 4, 1.2f), S(voidSpawn, 10, 0.7f, 3f)), W(2, S(bloodclaw, 6, 1f)),
                    W(3, S(wolfrider, 8, 0.5f), S(bloodclaw, 4, 1f, 4f)), W(4, S(voidSpawn, 16, 0.6f), S(bloodclaw, 6, 1f, 3f)),
                    W(5, S(bloodclaw, 8, 0.9f)), W(6, S(behemoth, 1, 1f, 1f), S(bloodclaw, 4, 1f, 4f)),
                    W(7, S(bloodclaw, 10, 0.8f), S(wolfrider, 8, 0.4f, 5f)) }),
                MakeLevel(6, new[] {
                    W(1, S(voidSpawn, 14, 0.6f)), W(2, S(wolfrider, 8, 0.5f), S(voidSpawn, 10, 0.7f, 3f)),
                    W(3, S(bloodclaw, 6, 1f), S(wolfrider, 6, 0.5f, 4f)), W(4, S(behemoth, 1, 1f, 1f), S(voidSpawn, 12, 0.6f, 3f)),
                    W(5, S(bloodclaw, 8, 0.9f), S(voidSpawn, 12, 0.6f, 4f)), W(6, S(wolfrider, 12, 0.4f)),
                    W(7, S(bloodclaw, 10, 0.8f), S(wolfrider, 8, 0.4f, 5f)),
                    W(8, S(shroudKing, 1, 1f, 1f), S(voidSpawn, 15, 0.5f, 3f), S(bloodclaw, 6, 1f, 6f)) }),
            };

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

            var path = BuildPath();
            BuildNodes();

            var mgr = new GameObject("_Managers");
            mgr.AddComponent<GameManager>();
            mgr.AddComponent<EconomyManager>();

            var wave = mgr.AddComponent<WaveManager>();
            wave.level = levels[0];
            wave.path = path;

            var build = mgr.AddComponent<BuildManager>();
            var bso = new SerializedObject(build);
            bso.FindProperty("towerBasePrefab").objectReferenceValue = borin.towerPrefab;
            bso.ApplyModifiedPropertiesWithoutUndo();

            var hud = mgr.AddComponent<DebugHUD>();
            hud.waveManager = wave;
            hud.buildManager = build;
            hud.heroes = new[] { borin, faelyn, elwin };
            hud.levels = levels;

            EditorSceneManager.SaveScene(scene, ScenePath);

            bool ok = wave.level != null && wave.path != null && hud.heroes.Length == 3 && hud.levels.Length == 6;
            EditorUtility.DisplayDialog("Gölgehalka",
                ok
                    ? "Act I hazır ve doğrulandı ✓\n\n3 kahraman · 5 düşman tipi · 6 bölüm\n\n" +
                      "Play → bölüm seç (B1-B6) → kahraman seç → platforma tıkla → Sonraki Dalga."
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
                    if (AttachModel(go, ModelDir + "/" + id + ".glb",
                        new Vector3(0, -1f, 0), Vector3.one * (modelScale / 0.6f)))
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
            GameObject towerPrefab = MakePrimitivePrefab(
                PrimitiveType.Cylinder, "Tower_" + id, 1f, color,
                go =>
                {
                    go.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
                    var fire = new GameObject("FirePoint");
                    fire.transform.SetParent(go.transform);
                    fire.transform.localPosition = new Vector3(0, 3f, 0);
                    var tower = go.AddComponent<Tower>();
                    var so = new SerializedObject(tower);
                    so.FindProperty("firePoint").objectReferenceValue = fire.transform;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    AttachModel(go, ModelDir + "/" + id + ".glb",
                        new Vector3(0, 1f, 0), new Vector3(1f / 0.8f, 1f / 0.5f, 1f / 0.8f));
                });

            return CreateAsset<HeroDefinition>(DataDir + "/Hero_" + id + ".asset", h =>
            {
                h.heroId = id; h.nameKey = "hero." + id + ".name";
                h.projectilePrefab = projectile; h.towerPrefab = towerPrefab;
                tune(h);
            });
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

        private static LevelDefinition MakeLevel(int no, WaveDefinition[] waves) =>
            CreateAsset<LevelDefinition>(DataDir + "/Act1_Level" + no + ".asset", l =>
            {
                l.levelId = "act1_level" + no;
                l.nameKey = "campaign.act1.title";
                l.startingGold = no >= 4 ? 250 : 200;
                l.startingLives = 20;
                l.waves = waves;
            });

        // ================= SAHNE PARÇALARI =================

        private static WaypointPath BuildPath()
        {
            var pathGO = new GameObject("WaypointPath");
            Vector3[] pts =
            {
                new Vector3(-10, 0.1f, 5), new Vector3(-2, 0.1f, 5), new Vector3(1, 0.1f, 2),
                new Vector3(-3, 0.1f, -1), new Vector3(2, 0.1f, -4), new Vector3(10, 0.1f, -4),
            };
            var wps = new Transform[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                var wp = new GameObject("WP" + i);
                wp.transform.SetParent(pathGO.transform);
                wp.transform.position = pts[i];
                wps[i] = wp.transform;
            }
            var path = pathGO.AddComponent<WaypointPath>();
            var pso = new SerializedObject(path);
            var arr = pso.FindProperty("waypoints");
            arr.arraySize = wps.Length;
            for (int i = 0; i < wps.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = wps[i];
            pso.ApplyModifiedPropertiesWithoutUndo();

            for (int i = 0; i < pts.Length - 1; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "PathSeg" + i;
                Object.DestroyImmediate(seg.GetComponent<Collider>());
                Vector3 a = pts[i], b = pts[i + 1];
                seg.transform.position = (a + b) / 2f + Vector3.down * 0.05f;
                seg.transform.rotation = Quaternion.LookRotation(b - a);
                seg.transform.localScale = new Vector3(1.2f, 0.05f, (b - a).magnitude);
                Tint(seg, new Color(0.45f, 0.38f, 0.28f));
                seg.transform.SetParent(pathGO.transform);
            }
            return path;
        }

        private static void BuildNodes()
        {
            Vector3[] nodePts =
            {
                new Vector3(-6, 0.15f, 3), new Vector3(-4, 0.15f, 7), new Vector3(0, 0.15f, 3.2f),
                new Vector3(-1, 0.15f, 0.5f), new Vector3(-5, 0.15f, -2.5f), new Vector3(0, 0.15f, -2),
                new Vector3(4, 0.15f, -2), new Vector3(5, 0.15f, -6),
            };
            var parent = new GameObject("PlacementNodes").transform;
            foreach (var p in nodePts)
            {
                var node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.name = "Node";
                node.transform.SetParent(parent);
                node.transform.position = p;
                node.transform.localScale = new Vector3(1.4f, 0.3f, 1.4f);
                Tint(node, new Color(0.55f, 0.5f, 0.4f));
                node.AddComponent<PlacementNode>();
            }
        }

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
