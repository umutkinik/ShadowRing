using Golgehalka.Combat;
using Golgehalka.Core;
using Golgehalka.Data;
using Golgehalka.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Golgehalka.EditorTools
{
    /// TEK TIKLA PROTOTİP: menüden "Gölgehalka → Prototip Sahne Kur" seç.
    /// Veri asset'leri + prefab'lar + kablolanmış sahne otomatik oluşur,
    /// Play'e basınca oynanabilir (Borin kulesi vs Boşluk Yavruları, 5 dalga).
    public static class PrototypeSceneBuilder
    {
        private const string DataDir = "Assets/Data";
        private const string PrefabDir = "Assets/Prefabs";
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("Gölgehalka/Prototip Sahne Kur")]
        public static void Build()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Scenes");

            // ---- 1) PREFABLAR ----
            GameObject projectilePrefab = MakePrimitivePrefab(
                PrimitiveType.Sphere, "Projectile", 0.25f, new Color(1f, 0.85f, 0.3f),
                go => { Object.DestroyImmediate(go.GetComponent<Collider>()); go.AddComponent<Projectile>(); });

            GameObject enemyPrefab = MakePrimitivePrefab(
                PrimitiveType.Capsule, "Enemy_VoidSpawn", 0.6f, new Color(0.5f, 0.2f, 0.6f),
                go =>
                {
                    go.AddComponent<Enemy>();
                    // Meshy modeli varsa kapsülü gizle, modeli tak (yoksa mor kapsül kalır)
                    if (AttachModel(go, "Assets/Models/Characters/void_spawn.glb",
                        new Vector3(0, -1f, 0), new Vector3(1f / 0.6f, 1f / 0.6f, 1f / 0.6f)))
                        go.GetComponent<MeshRenderer>().enabled = false;
                });

            GameObject towerPrefab = MakePrimitivePrefab(
                PrimitiveType.Cylinder, "TowerBase", 1f, new Color(0.4f, 0.55f, 0.8f),
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
                    // Borin modeli silindirin üstünde nöbette (ölçek: ebeveyn ölçeğini telafi eder)
                    AttachModel(go, "Assets/Models/Characters/borin.glb",
                        new Vector3(0, 1f, 0), new Vector3(1f / 0.8f, 1f / 0.5f, 1f / 0.8f));
                });

            // ---- 2) VERİ ASSET'LERİ (balance-v1.md değerleri) ----
            var voidSpawn = CreateAsset<EnemyDefinition>(DataDir + "/VoidSpawn.asset", d =>
            {
                d.enemyId = "void_spawn"; d.nameKey = "enemy.void_spawn.name";
                d.maxHealth = 45; d.armor = 0f; d.moveSpeed = 2.2f;
                d.goldReward = 4; d.livesCost = 1; d.prefab = enemyPrefab;
            });

            var borin = CreateAsset<HeroDefinition>(DataDir + "/Hero_Borin.asset", h =>
            {
                h.heroId = "borin"; h.nameKey = "hero.borin.name";
                h.damageType = DamageType.Physical; h.armorPenetration = 0.4f;
                h.projectilePrefab = projectilePrefab; h.projectileSpeed = 14f;
                h.tiers = new[]
                {
                    new TowerTier { cost = 110, damage = 25, range = 4.5f, fireRate = 0.7f },
                    new TowerTier { cost = 130, damage = 42, range = 5f, fireRate = 0.8f },
                    new TowerTier { cost = 210, damage = 70, range = 5.5f, fireRate = 0.9f },
                };
            });

            var level = CreateAsset<LevelDefinition>(DataDir + "/Act1_Level1.asset", l =>
            {
                l.levelId = "act1_level1"; l.nameKey = "campaign.act1.title";
                l.startingGold = 200; l.startingLives = 20;
                int[] counts = { 5, 7, 9, 11, 14 };
                l.waves = new WaveDefinition[counts.Length];
                for (int i = 0; i < counts.Length; i++)
                    l.waves[i] = new WaveDefinition
                    {
                        rewardGold = 25 + 5 * (i + 1),
                        entries = new[] { new SpawnEntry
                            { enemy = voidSpawn, count = counts[i], interval = 0.8f, startDelay = 0.5f } }
                    };
            });

            // ---- 3) SAHNE ----
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
            Tint(ground, new Color(0.25f, 0.32f, 0.22f)); // Yeşiloluk çimeni

            // Yol: S-kıvrımı
            var pathGO = new GameObject("WaypointPath");
            var wpParent = pathGO.transform;
            Vector3[] pts =
            {
                new Vector3(-10, 0.1f, 5), new Vector3(-2, 0.1f, 5), new Vector3(1, 0.1f, 2),
                new Vector3(-3, 0.1f, -1), new Vector3(2, 0.1f, -4), new Vector3(10, 0.1f, -4),
            };
            var wps = new Transform[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                var wp = new GameObject("WP" + i);
                wp.transform.SetParent(wpParent);
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

            // Yolu görünür yap (ince şeritler)
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "PathSeg" + i;
                Object.DestroyImmediate(seg.GetComponent<Collider>());
                Vector3 a = pts[i], b = pts[i + 1];
                seg.transform.position = (a + b) / 2f + Vector3.down * 0.05f;
                seg.transform.rotation = Quaternion.LookRotation(b - a);
                seg.transform.localScale = new Vector3(1.2f, 0.05f, (b - a).magnitude);
                Tint(seg, new Color(0.45f, 0.38f, 0.28f)); // toprak yolu
                seg.transform.SetParent(pathGO.transform);
            }

            // Yerleştirme noktaları
            Vector3[] nodePts =
            {
                new Vector3(-6, 0.15f, 3), new Vector3(-4, 0.15f, 7), new Vector3(0, 0.15f, 3.2f),
                new Vector3(-1, 0.15f, 0.5f), new Vector3(-5, 0.15f, -2.5f), new Vector3(0, 0.15f, -2),
                new Vector3(4, 0.15f, -2), new Vector3(5, 0.15f, -6),
            };
            var nodesParent = new GameObject("PlacementNodes").transform;
            foreach (var p in nodePts)
            {
                var node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.name = "Node";
                node.transform.SetParent(nodesParent);
                node.transform.position = p;
                node.transform.localScale = new Vector3(1.4f, 0.3f, 1.4f);
                Tint(node, new Color(0.55f, 0.5f, 0.4f));
                node.AddComponent<PlacementNode>();
            }

            // Yöneticiler
            var mgr = new GameObject("_Managers");
            mgr.AddComponent<GameManager>();
            mgr.AddComponent<EconomyManager>();

            var wave = mgr.AddComponent<WaveManager>();
            // Doğrudan alan ataması — SerializedObject asset ataması 'level' için
            // sessizce fileID:0 yazmıştı; public alan ataması güvenilir yol.
            wave.level = level;
            wave.path = path;

            var build = mgr.AddComponent<BuildManager>();
            var bso = new SerializedObject(build);
            bso.FindProperty("towerBasePrefab").objectReferenceValue = towerPrefab;
            bso.ApplyModifiedPropertiesWithoutUndo();

            var hud = mgr.AddComponent<DebugHUD>();
            hud.waveManager = wave;
            hud.buildManager = build;
            hud.heroes = new[] { borin };

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Kurulum sonrası kendi kendini doğrulama — referans kopmuşsa hemen söyle
            bool ok = wave.level != null && wave.path != null &&
                      hud.waveManager != null && hud.heroes.Length > 0;
            EditorUtility.DisplayDialog("Gölgehalka",
                ok
                    ? "Prototip sahne hazır ve doğrulandı ✓\n\n" + ScenePath +
                      "\n\nPlay'e bas → 'borin' butonuna tıkla → bir taş platforma tıkla → 'Sonraki Dalga'."
                    : "DİKKAT: bazı referanslar atanamadı! Console'a bak.",
                ok ? "Başlıyoruz!" : "Tamam");
        }

        // ---- yardımcılar ----

        /// GLB modelini (glTFast importu) verilen objenin altına çocuk olarak ekler.
        /// Model bulunamazsa false döner — prototip ilkel şekliyle devam eder.
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
            // Diskten geri yükle — döndürülen referansın kalıcı (persistent) olduğu garanti
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

        /// URP Lit tabanlı basit renkli materyal üretir ve uygular.
        private static void Tint(GameObject go, Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { color = c };
            EnsureFolder("Assets", "Prefabs"); // materyaller de buraya
            string matPath = PrefabDir + "/Mat_" + go.name + "_" + ColorUtility.ToHtmlStringRGB(c) + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing == null) { AssetDatabase.CreateAsset(mat, matPath); existing = mat; }
            go.GetComponent<Renderer>().sharedMaterial = existing;
        }
    }
}
