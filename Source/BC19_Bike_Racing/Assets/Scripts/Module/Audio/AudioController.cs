using UnityEngine;

namespace Bacon
{
    public class AudioController : MonoBehaviour
    {
        [HideInInspector]
        public AudioSource EffectsSource;
        [HideInInspector]
        public AudioSource MusicSource;

        [SerializeField] private bool AutoPlayMusic = false;

        public AudioClip background;
        public AudioClip click;
        public AudioClip spin;
        public AudioClip claimReward;

        public static AudioController Instance = null;

        public float PlayerMusic
        {
            get
            {
                if (DataManager.Instance) return DataManager.Instance._player.music;
                else return 0;
            }
            set
            {
                if (DataManager.Instance) DataManager.Instance._player.music = value;
            }
        }

        public float PlayerSound
        {
            get
            {
                if (DataManager.Instance) return DataManager.Instance._player.sound;
                else return 0;
            }
            set
            {
                if (DataManager.Instance) DataManager.Instance._player.sound = value;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            EffectsSource = gameObject.AddComponent<AudioSource>();
            MusicSource = gameObject.AddComponent<AudioSource>();

            SetMusicVolume(PlayerMusic);
            SetSoundVolume(PlayerSound);

            //Auto play background
            if (AutoPlayMusic)
            {
                ScaleMusic(0.5f);
                BackGround();
            }
        }


        public void Play(AudioClip clip, AudioSource _source = null)
        {
            if (_source == null)
            {
                EffectsSource.clip = clip;
                EffectsSource.volume = PlayerSound;
                EffectsSource.Play();
            }
            else
            {
                _source.clip = clip;
                _source.volume = PlayerSound;
                _source.Play();
            }

        }


        public void StopSound()
        {
            EffectsSource.Stop();
        }

        public void StopMusic()
        {
            MusicSource.Stop();
        }

        // Play a single clip through the music source.
        public void PlayMusic(AudioClip clip)
        {
            MusicSource.clip = clip;
            MusicSource.loop = true;
            MusicSource.volume = PlayerMusic;
            MusicSource.Play();
        }

        public void SetMusicVolume(float volume)
        {
            MusicSource.volume = volume;
        }

        public void ScaleMusic(float scale)
        {
            MusicSource.volume = PlayerMusic * scale;
        }

        public void SetSoundVolume(float volume)
        {
            EffectsSource.volume = volume;
        }

        public void PauseAllSound(bool bg = false)
        {
            EffectsSource.volume = 0f;
            EffectsSource.Pause();
            if (bg)
            {
                MusicSource.volume = 0f;
                MusicSource.Pause();
            }

        }

        public void ResumeSound()
        {
            EffectsSource.volume = PlayerSound;
            EffectsSource.UnPause();
            EffectsSource.volume = PlayerMusic;
            MusicSource.UnPause();

        }


        #region external call function
        public void BackGround()
        {
            if (!MusicSource.isPlaying)
                this.PlayMusic(background);
        }
        public void Click() { this.Play(click); }

        public void Spin() { this.Play(spin); }

        public void ClaimReward() { this.Play(claimReward); }
        #endregion
    }
}