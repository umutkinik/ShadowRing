using Golgehalka.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Golgehalka.EditorTools
{
    /// Diablo-vari ana menü: karanlıkta ateş ışığıyla aydınlanan Zarok,
    /// altın başlık, dikey buton sütunu, köz partikülleri (çalışma anında).
    public static class MainMenuBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Gölgehalka/Ana Menü Kur")]
        public static void Build()
        {
            PrototypeSceneBuilder.EnsureFolder("Assets", "Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Kamera — alçak açıdan Zarok'a bakar
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            cam.fieldOfView = 45f;
            cam.transform.position = new Vector3(0, 2.1f, -5.9f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.012f, 0.025f);
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            cam.transform.LookAt(new Vector3(0, 1.4f, 1.2f));

            // Var olan post-fx profili menüde de kullan (vinyet)
            var profile = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(
                "Assets/Settings/GamePostFX.asset");
            if (profile != null)
            {
                var volGO = new GameObject("PostFX");
                var vol = volGO.AddComponent<UnityEngine.Rendering.Volume>();
                vol.isGlobal = true;
                vol.profile = profile;
            }

            // Loş mor dolgu ışığı + turuncu "şömine" + mor rim
            var fill = new GameObject("Fill Light").AddComponent<Light>();
            fill.type = UnityEngine.LightType.Directional;
            fill.color = new Color(0.45f, 0.4f, 0.65f);
            fill.intensity = 0.35f;
            fill.transform.rotation = Quaternion.Euler(55, -25, 0);

            var key = new GameObject("Fire Light").AddComponent<Light>();
            key.type = UnityEngine.LightType.Point;
            key.color = new Color(1f, 0.45f, 0.15f);
            key.intensity = 8f; key.range = 14f;
            key.transform.position = new Vector3(2.4f, 2.4f, -1.8f);

            var rim = new GameObject("Rim Light").AddComponent<Light>();
            rim.type = UnityEngine.LightType.Point;
            rim.color = new Color(0.55f, 0.25f, 0.95f);
            rim.intensity = 7f; rim.range = 16f;
            rim.transform.position = new Vector3(-2.4f, 3.4f, 3.2f);

            // Karanlık zemin
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(2f, 1, 2f);
            var fm = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.05f, 0.045f, 0.065f) };
            AssetDatabase.DeleteAsset("Assets/Prefabs/Mat_MenuFloor.mat");
            AssetDatabase.CreateAsset(fm, "Assets/Prefabs/Mat_MenuFloor.mat");
            floor.GetComponent<Renderer>().sharedMaterial = fm;

            // Zarok — sahnenin efendisi: dev, merkez-geri, tehditkâr
            Transform zarokT = null;
            var zarokModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Characters/zarok.glb");
            if (zarokModel != null)
            {
                var z = Object.Instantiate(zarokModel);
                z.name = "Zarok";
                z.transform.position = new Vector3(0, 0, 2.8f);
                z.transform.rotation = Quaternion.Euler(0, 178, 0);
                z.transform.localScale = Vector3.one * 2.1f;
                zarokT = z.transform;
            }

            // Kadro dizilimi: solda kahramanlar, sağda Boşluk ordusu — karşı karşıya
            void Cast(string id, float x, float z, float yRot, float scale)
            {
                var m = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Characters/" + id + ".glb");
                if (m == null) return;
                var g = Object.Instantiate(m);
                g.name = "Cast_" + id;
                g.transform.position = new Vector3(x, 0, z);
                g.transform.rotation = Quaternion.Euler(0, yRot, 0);
                g.transform.localScale = Vector3.one * scale;
            }
            // Kahramanlar (kameraya dönük, hafif içe açılı)
            Cast("kael", -4.9f, 1.6f, 128, 1.15f);
            Cast("borin", -3.9f, 0.9f, 138, 1.1f);
            Cast("faelyn", -3.0f, 1.5f, 130, 1.12f);
            Cast("elwin", -2.1f, 0.8f, 145, 1.15f);
            Cast("baldric", -1.3f, 1.6f, 140, 1.1f);
            // Boşluk ordusu
            Cast("bloodclaw", 1.3f, 1.5f, 222, 1.2f);
            Cast("shroud_king", 2.1f, 0.8f, 218, 1.25f);
            Cast("molgroth", 3.0f, 1.6f, 230, 1.5f);
            Cast("morwen", 4.0f, 0.9f, 225, 1.35f);
            Cast("stone_behemoth", 5.0f, 1.7f, 232, 1.6f);

            // --- UI ---
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var canvasGO = new GameObject("MenuCanvas",
                typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var ctrl = canvasGO.AddComponent<MainMenuController>();
            ctrl.zarok = zarokT;
            var t = canvasGO.transform;
            Color btnBg = new Color(0.1f, 0.06f, 0.05f, 0.88f);
            Color gold = new Color(0.85f, 0.66f, 0.32f);

            // Cinzel (Trajan akrabası, OFL) → TMP font asset'i — Diablo başlık ruhu
            TMP_FontAsset cinzel = null;
            var cinzelTtf = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Cinzel.ttf");
            if (cinzelTtf != null)
            {
                cinzel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel SDF.asset");
                if (cinzel == null)
                {
                    cinzel = TMP_FontAsset.CreateFontAsset(cinzelTtf);
                    cinzel.name = "Cinzel SDF";
                    AssetDatabase.CreateAsset(cinzel, "Assets/Fonts/Cinzel SDF.asset");
                    if (cinzel.material != null) AssetDatabase.AddObjectToAsset(cinzel.material, cinzel);
                    if (cinzel.atlasTexture != null) AssetDatabase.AddObjectToAsset(cinzel.atlasTexture, cinzel);
                    AssetDatabase.SaveAssets();
                }
            }

            // Başlık — altın degrade + koyu kontur (Diablo tarzı)
            var title = PrototypeSceneBuilder.UIText(t, "Title", "SHADOWRING", 132,
                Vector2.zero, new Vector2(1500, 180), TMPro.TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0, -52);
            title.characterSpacing = 10f;
            if (cinzel != null) title.font = cinzel;
            title.enableVertexGradient = true;
            title.colorGradient = new VertexGradient(
                new Color(1f, 0.88f, 0.55f), new Color(1f, 0.88f, 0.55f),   // üst: açık altın
                new Color(0.55f, 0.36f, 0.12f), new Color(0.55f, 0.36f, 0.12f)); // alt: bronz
            title.outlineWidth = 0.22f;
            title.outlineColor = new Color32(30, 15, 5, 255);

            var sub = PrototypeSceneBuilder.UIText(t, "Subtitle", "The Sundered Realm", 36,
                Vector2.zero, new Vector2(900, 60), TMPro.TextAlignmentOptions.Center);
            sub.rectTransform.anchorMin = sub.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            sub.rectTransform.pivot = new Vector2(0.5f, 1f);
            sub.rectTransform.anchoredPosition = new Vector2(0, -224);
            sub.color = new Color(0.72f, 0.62f, 0.48f);
            sub.characterSpacing = 6f;
            if (cinzel != null) sub.font = cinzel;

            // Buton sütunu (alt-orta)
            var (playB, playLbl) = PrototypeSceneBuilder.UIButton(t, "Play", "Play",
                Vector2.zero, new Vector2(430, 92), btnBg, "menu.play");
            Center(playB.GetComponent<RectTransform>(), new Vector2(0, 330));
            playLbl.color = gold; playLbl.fontSize = 44;

            var (langB, langLbl) = PrototypeSceneBuilder.UIButton(t, "Lang", "EN",
                Vector2.zero, new Vector2(300, 64), btnBg, null);
            Center(langB.GetComponent<RectTransform>(), new Vector2(0, 218));
            ctrl.langButton = langB; ctrl.langLabel = langLbl;

            var (credB, _) = PrototypeSceneBuilder.UIButton(t, "Credits", "Credits",
                Vector2.zero, new Vector2(300, 64), btnBg, null);
            Center(credB.GetComponent<RectTransform>(), new Vector2(0, 138));

            ctrl.playButton = playB;
            ctrl.creditsButton = credB;

            // Krediler paneli
            var panel = PrototypeSceneBuilder.UIPanel(t, "CreditsPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1000, 560), new Color(0.03f, 0.025f, 0.05f, 0.94f));
            var credText = PrototypeSceneBuilder.UIText(panel, "Text",
                "SHADOWRING — The Sundered Realm\n\n" +
                "Müzik: \"Heroic Age\" — Kevin MacLeod (incompetech.com)\n" +
                "Creative Commons BY 4.0\n\n" +
                "Dokular: Poly Haven (CC0)\n" +
                "3D Modeller: Meshy AI ile üretildi\n" +
                "Ses Efektleri: özgün sentez",
                30, new Vector2(60, 40), new Vector2(880, 400), TMPro.TextAlignmentOptions.TopLeft);
            credText.color = new Color(0.8f, 0.75f, 0.68f);
            var (closeB, _) = PrototypeSceneBuilder.UIButton(panel, "Close", "Close",
                Vector2.zero, new Vector2(280, 64), btnBg, "ui.close");
            var closeRt = closeB.GetComponent<RectTransform>();
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.anchoredPosition = new Vector2(0, 28);
            ctrl.creditsPanel = panel.gameObject;
            ctrl.creditsCloseButton = closeB;

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Build listesi: menü ilk, oyun ikinci — LoadScene("Prototype") çalışsın
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity", true),
            };

            EditorUtility.DisplayDialog("Gölgehalka",
                "Ana menü hazır ✓\n\nPlay'e bas: közler süzülür, Zarok döner, ateş+mor ışık.\n" +
                "\"Play\" butonu oyun sahnesini açar. Build listesi de ayarlandı.",
                "Karanlık çöksün!");
        }

        private static void Center(RectTransform rt, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
        }
    }
}
