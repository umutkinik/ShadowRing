using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Kalıcı oyuncu verisi — JSON olarak persistentDataPath'e yazılır.
    /// no_ads hakkı PlayerPrefs'te AYRICA tutulur (PurchaseManager) —
    /// kayıt dosyası silinse bile satın alım hakkı korunur + restore edilebilir.
    [Serializable]
    public class ProfileData
    {
        public int schemaVersion = 1;
        public string languageCode = "";            // boş = cihaz dilini kullan (varsayılan EN fallback)
        public float musicVolume = 1f;
        public float sfxVolume = 1f;

        public List<string> completedLevels = new List<string>();   // "act1_level3"
        public List<LevelStars> stars = new List<LevelStars>();
        public List<HeroProgress> heroes = new List<HeroProgress>();
        public int shards;                          // meta para birimi (kahraman seviyesi için)
    }

    [Serializable]
    public class LevelStars { public string levelId; public int count; }

    [Serializable]
    public class HeroProgress
    {
        public string heroId;
        public int level = 1;                       // kalıcı kahraman seviyesi (maç-içi kademeden ayrı)
        public bool unlocked;
        public List<string> equippedArtifacts = new List<string>(); // "safakmarka"...
    }

    /// Yükle/kaydet + basit erişim API'si. Bulut kaydı Faz 3'te bu sınıfın
    /// arkasına eklenir (arayüz değişmez).
    public static class PlayerProfile
    {
        private static ProfileData data;
        private static string FilePath =>
            Path.Combine(Application.persistentDataPath, "profile.json");

        public static ProfileData Data
        {
            get { if (data == null) Load(); return data; }
        }

        public static void Load()
        {
            try
            {
                data = File.Exists(FilePath)
                    ? JsonUtility.FromJson<ProfileData>(File.ReadAllText(FilePath))
                    : NewProfile();
            }
            catch (Exception e)
            {
                Debug.LogError("Profil okunamadı, yenisi açılıyor: " + e.Message);
                data = NewProfile();
            }
        }

        public static void Save()
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(data, true)); }
            catch (Exception e) { Debug.LogError("Profil yazılamadı: " + e.Message); }
        }

        private static ProfileData NewProfile()
        {
            var p = new ProfileData();
            // İlk üç kahraman baştan açık — oyuncu seçim hissiyle başlar
            p.heroes.Add(new HeroProgress { heroId = "kael", unlocked = true });
            p.heroes.Add(new HeroProgress { heroId = "faelyn", unlocked = true });
            p.heroes.Add(new HeroProgress { heroId = "borin", unlocked = true });
            return p;
        }

        /// Kampanya ilerlemesini sıfırla (kahraman kilidi/ayarlara dokunmaz).
        public static void ResetProgress()
        {
            Data.completedLevels.Clear();
            Data.stars.Clear();
            Data.shards = 0;
            Save();
        }

        // ---- Bölüm ilerlemesi ----
        public static bool IsCompleted(string levelId) => Data.completedLevels.Contains(levelId);

        public static void CompleteLevel(string levelId, int starCount, int shardReward)
        {
            if (!Data.completedLevels.Contains(levelId))
                Data.completedLevels.Add(levelId);

            var s = Data.stars.Find(x => x.levelId == levelId);
            if (s == null) Data.stars.Add(new LevelStars { levelId = levelId, count = starCount });
            else s.count = Mathf.Max(s.count, starCount); // en iyi skor korunur

            Data.shards += shardReward;
            Save();
        }

        // ---- Kahraman meta ilerleme ----
        public static HeroProgress GetHero(string heroId)
        {
            var h = Data.heroes.Find(x => x.heroId == heroId);
            if (h == null) { h = new HeroProgress { heroId = heroId }; Data.heroes.Add(h); }
            return h;
        }

        /// Parça (shard) harcayarak kalıcı seviye atlat. Maliyet: seviye × 50.
        public static bool TryLevelUpHero(string heroId)
        {
            var h = GetHero(heroId);
            int cost = h.level * 50;
            if (!h.unlocked || Data.shards < cost) return false;
            Data.shards -= cost;
            h.level++;
            Save();
            return true;
        }

        /// Kalıcı seviye bonusu — Tower hasar hesabında çarpan olarak kullanılır:
        /// her meta seviye +%5 hasar (basit, anlaşılır, p2w değil — shard sadece oyunla kazanılır).
        public static float DamageMultiplier(string heroId) =>
            1f + (GetHero(heroId).level - 1) * 0.05f;
    }
}
