using TMPro;
using UnityEngine;

namespace Golgehalka.UI
{
    /// Bir TMP metnine lokalizasyon anahtarı bağlar; dil değişince otomatik yeniler.
    /// RTL (Arapça) için hizalamayı aynalar — kapsamlı RTL shaping için Faz 2'de
    /// RTLTMPro entegre edilecek (bkz. Localization/README.md).
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private TMP_Text label;
        private TextAlignmentOptions originalAlignment;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            originalAlignment = label.alignment;
        }

        private void OnEnable()
        {
            LocalizationManager.OnLocaleChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => LocalizationManager.OnLocaleChanged -= Refresh;

        public void SetKey(string newKey) { key = newKey; Refresh(); }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(key)) return;
            label.text = LocalizationManager.Get(key);
            label.isRightToLeftText = LocalizationManager.IsRTL;
            label.alignment = LocalizationManager.IsRTL ? Mirror(originalAlignment) : originalAlignment;
        }

        private static TextAlignmentOptions Mirror(TextAlignmentOptions a)
        {
            switch (a)
            {
                case TextAlignmentOptions.Left: return TextAlignmentOptions.Right;
                case TextAlignmentOptions.Right: return TextAlignmentOptions.Left;
                case TextAlignmentOptions.TopLeft: return TextAlignmentOptions.TopRight;
                case TextAlignmentOptions.TopRight: return TextAlignmentOptions.TopLeft;
                case TextAlignmentOptions.BottomLeft: return TextAlignmentOptions.BottomRight;
                case TextAlignmentOptions.BottomRight: return TextAlignmentOptions.BottomLeft;
                default: return a;
            }
        }
    }
}
