using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Golgehalka.UI
{
    /// Hafif JSON lokalizasyon çalışma zamanı — Localization/ klasöründeki
    /// tabloları StreamingAssets'ten okur. Prototip için yeterli; Faz 2'de
    /// Unity Localization paketine geçilirse yalnızca bu sınıf değişir
    /// (LocalizedText bileşeni aynı kalır).
    ///
    /// Kurulum: Localization/*.json dosyalarını Assets/StreamingAssets/Localization/ altına kopyala.
    public static class LocalizationManager
    {
        public const string DefaultLocale = "en";  // varsayılan İngilizce
        private static readonly string[] Supported = { "en", "de", "ru", "zh-Hans", "hi", "ar", "tr" };

        private static Dictionary<string, string> table = new Dictionary<string, string>();
        public static string CurrentLocale { get; private set; } = DefaultLocale;
        public static bool IsRTL => CurrentLocale == "ar";
        public static event Action OnLocaleChanged;

        /// Uygulama açılışında çağır: kayıtlı tercih > cihaz dili > EN.
        public static void Init()
        {
            string saved = Core.PlayerProfile.Data.languageCode;
            SetLocale(!string.IsNullOrEmpty(saved) ? saved : DetectDeviceLocale());
        }

        private static string DetectDeviceLocale()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.German: return "de";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.Chinese: return "zh-Hans";
                case SystemLanguage.Hindi: return "hi";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Turkish: return "tr";
                default: return DefaultLocale;
            }
        }

        public static void SetLocale(string code)
        {
            if (Array.IndexOf(Supported, code) < 0) code = DefaultLocale;
            string path = Path.Combine(Application.streamingAssetsPath, "Localization", code + ".json");
            try
            {
                // Android'de StreamingAssets jar içindedir — orada UnityWebRequest gerekir;
                // Faz 1'de bu okuma async hale getirilecek. Editor/iOS'ta File.ReadAllText çalışır.
                var root = JsonUtility.FromJson<LocaleFile>(WrapStrings(File.ReadAllText(path)));
                table = root.ToDictionary();
                CurrentLocale = code;
                Core.PlayerProfile.Data.languageCode = code;
                Core.PlayerProfile.Save();
                OnLocaleChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Locale yüklenemedi (" + code + "): " + e.Message);
                if (code != DefaultLocale) SetLocale(DefaultLocale);
            }
        }

        /// Anahtar → çeviri. Bulunamazsa anahtarın kendisi döner (QA'da hemen görünür).
        public static string Get(string key) =>
            table.TryGetValue(key, out string v) ? v : key;

        // ---- JsonUtility sözlük desteklemediği için basit dönüştürme ----
        [Serializable]
        private class LocaleFile
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();
            public Dictionary<string, string> ToDictionary()
            {
                var d = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count; i++) d[keys[i]] = values[i];
                return d;
            }
        }

        /// {"strings":{"a":"b",...}} yapısını keys/values listesine çevirir.
        private static string WrapStrings(string raw)
        {
            var keys = new List<string>();
            var values = new List<string>();
            int idx = raw.IndexOf("\"strings\"", StringComparison.Ordinal);
            int brace = raw.IndexOf('{', idx);
            int end = raw.LastIndexOf('}');
            string body = raw.Substring(brace + 1, raw.LastIndexOf('}', end - 1) - brace - 1);
            foreach (string line in body.Split('\n'))
            {
                string t = line.Trim().TrimEnd(',');
                int sep = t.IndexOf("\":", StringComparison.Ordinal);
                if (sep < 0 || !t.StartsWith("\"")) continue;
                keys.Add(t.Substring(1, sep - 1));
                string val = t.Substring(sep + 2).Trim();
                values.Add(val.Length >= 2 ? val.Substring(1, val.Length - 2) : "");
            }
            return JsonUtility.ToJson(new LocaleFile { keys = keys, values = values });
        }
    }
}
