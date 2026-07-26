using UnityEngine;

namespace PizzaOnTop.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Clips")]
        public AudioClip[] footstepClips = new AudioClip[2];
        public AudioClip deathClip;
        public AudioClip jumpClip;
        public AudioClip landClip;
        public AudioClip musicClip;

        [Header("Global Volume Controls")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        
        [Header("Music Volume")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;

        [Header("SFX Volumes")]
        [Range(0f, 1f)] public float globalSfxVolume = 1f;
        [Range(0f, 1f)] public float footstepVolume = 1f;
        [Range(0f, 1f)] public float deathVolume = 1f;
        [Range(0f, 1f)] public float jumpVolume = 1f;
        [Range(0f, 1f)] public float landVolume = 1f;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            PlayMusic();
        }

        private void Update()
        {
            if (musicSource != null)
            {
                musicSource.volume = masterVolume * musicVolume;
            }
        }

        public void PlayMusic()
        {
            if (musicClip != null)
            {
                musicSource.clip = musicClip;
                musicSource.volume = masterVolume * musicVolume;
                musicSource.Play();
            }
        }

        public void PlayFootstep()
        {
            if (footstepClips != null && footstepClips.Length > 0)
            {
                AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
                if (clip != null)
                {
                    sfxSource.PlayOneShot(clip, masterVolume * globalSfxVolume * footstepVolume);
                }
            }
        }

        public void PlayDeath()
        {
            if (deathClip != null) sfxSource.PlayOneShot(deathClip, masterVolume * globalSfxVolume * deathVolume);
        }

        public void PlayJump()
        {
            if (jumpClip != null) sfxSource.PlayOneShot(jumpClip, masterVolume * globalSfxVolume * jumpVolume);
        }

        public void PlayLand()
        {
            if (landClip != null) sfxSource.PlayOneShot(landClip, masterVolume * globalSfxVolume * landVolume);
        }
    }
}
