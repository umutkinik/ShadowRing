using UnityEngine;

namespace Golgehalka.Core
{
    /// Koddan partikül efekti üretici — asset gerektirmez.
    /// Yumuşak nokta dokusu + URP partikül materyali çalışma anında yaratılır.
    public static class VFX
    {
        private static Material sharedMat;
        private static Texture2D dotTex;

        public static Material ParticleMaterial => Mat();

        private static Texture2D Dot()
        {
            if (dotTex != null) return dotTex;
            const int S = 64;
            dotTex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(S / 2f, S / 2f)) / (S / 2f);
                    float a = Mathf.Clamp01(1f - d);
                    dotTex.SetPixel(x, y, new Color(1, 1, 1, a * a));
                }
            dotTex.Apply();
            return dotTex;
        }

        private static Material Mat()
        {
            if (sharedMat != null) return sharedMat;
            sharedMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            sharedMat.SetTexture("_BaseMap", Dot());
            sharedMat.SetTexture("_MainTex", Dot());
            // URP'de şeffaflık: _Surface tek başına yetmez, blend durumları elle kurulur
            // (eksikti → partiküller opak KARE görünüyordu)
            sharedMat.SetFloat("_Surface", 1f);
            sharedMat.SetFloat("_Blend", 0f);
            sharedMat.SetOverrideTag("RenderType", "Transparent");
            sharedMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            sharedMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            sharedMat.SetInt("_ZWrite", 0);
            sharedMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            sharedMat.renderQueue = 3000;
            return sharedMat;
        }

        /// Tek seferlik patlama: ölüm, çarpma, alev, toz...
        public static void Burst(Vector3 pos, Color color, int count = 16,
            float speed = 3f, float size = 0.25f, float life = 0.5f, float gravity = 0f)
        {
            var go = new GameObject("VFX_Burst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.loop = false;
            main.startLifetime = life;
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size * 1.3f);
            main.startColor = color;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission; em.enabled = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.12f;

            // Yaşam boyunca solma
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0, 1) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            go.GetComponent<ParticleSystemRenderer>().material = Mat();
            ps.Emit(count);
            Object.Destroy(go, life + 0.35f);
        }

        /// Sürekli iz — ejderha alevi, mermi kuyruğu vb. Ebeveyn yok olunca iz de gider.
        public static GameObject Trail(Transform parent, Color color,
            float rate = 40f, float life = 0.7f, float size = 0.18f)
        {
            var go = new GameObject("VFX_Trail");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = life;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size * 1.4f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // iz geride kalır

            var em = ps.emission; em.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0.9f, 0), new GradientAlphaKey(0, 1) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            go.GetComponent<ParticleSystemRenderer>().material = Mat();
            ps.Play();
            return go;
        }
    }
}
