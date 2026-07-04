using UnityEngine;

namespace Golgehalka.Core
{
    /// Sahne-yerel ses yöneticisi: SFX + döngülü savaş müziği.
    /// Ses seviyeleri PlayerProfile'dan gelir (ayarlar ekranı bağlanınca canlı değişir).
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX")]
        public AudioClip arrow;
        public AudioClip hit;
        public AudioClip coin;
        public AudioClip build;
        public AudioClip upgrade;
        public AudioClip victory;
        public AudioClip defeat;
        public AudioClip click;
        public AudioClip flame;
        public AudioClip thunder;
        public AudioClip quake;

        [Header("Müzik")]
        public AudioClip battleMusic;

        private AudioSource sfxSource;
        private AudioSource musicSource;

        private void Awake()
        {
            Instance = this;
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        private void Start()
        {
            sfxSource.volume = PlayerProfile.Data.sfxVolume;
            musicSource.volume = PlayerProfile.Data.musicVolume * 0.55f; // müzik SFX'i bastırmasın

            if (battleMusic != null)
            {
                musicSource.clip = battleMusic;
                musicSource.Play();
            }

            GameManager.Instance.OnStateChanged += state =>
            {
                if (state == GameState.Victory) { musicSource.Stop(); PlayClip(victory); }
                if (state == GameState.Defeat) { musicSource.Stop(); PlayClip(defeat); }
            };
        }

        private void PlayClip(AudioClip c, float vol = 1f)
        {
            if (c != null) sfxSource.PlayOneShot(c, vol);
        }

        // Oyun kodunun kullandığı kısa statik API — Instance yoksa sessizce geçer
        public static void Arrow() { if (Instance != null) Instance.PlayClip(Instance.arrow, 0.55f); }
        public static void Hit() { if (Instance != null) Instance.PlayClip(Instance.hit, 0.7f); }
        public static void Coin() { if (Instance != null) Instance.PlayClip(Instance.coin, 0.8f); }
        public static void Build() { if (Instance != null) Instance.PlayClip(Instance.build); }
        public static void Upgrade() { if (Instance != null) Instance.PlayClip(Instance.upgrade); }
        public static void Click() { if (Instance != null) Instance.PlayClip(Instance.click); }
        public static void Flame() { if (Instance != null) Instance.PlayClip(Instance.flame, 0.9f); }
        public static void Thunder() { if (Instance != null) Instance.PlayClip(Instance.thunder); }
        public static void Quake() { if (Instance != null) Instance.PlayClip(Instance.quake); }
    }
}
