using UnityEngine;


namespace Bacon
{
    public class SoundFx : MonoBehaviour
    {
        private AudioSource m_AudioSource;
        public AudioClip soundClip;

        private void Awake()
        {
            TryGetComponent(out m_AudioSource);
        }

        public void PlaySound()
        {
            if (AudioController.Instance && soundClip)
            {
                AudioController.Instance.Play(soundClip, m_AudioSource);
            }
        }
    }
}