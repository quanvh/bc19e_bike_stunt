using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UIToast : PopupBase
    {
        [Header("POPUP PARAM"), Space]
        [SerializeField] Image icon = null;
        [SerializeField] Text message = null;

        [SerializeField] protected AudioSource audioSource = null;

        protected float elapsedTime = 0;

        public static UIToast Instance = null;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public void Toast(string message, float durationAutoHide = 3f, Sprite icon = null)
        {
            if (this.icon != null && icon != null)
                this.icon.sprite = icon;

            this.message.text = message;

            StopAllCoroutines();
            Show(null, (t) =>
            {
                if (audioSource != null)
                    audioSource.Play();
            });
            elapsedTime = durationAutoHide;
            StartCoroutine(DOAutoHide());
        }

        private IEnumerator DOAutoHide()
        {
            yield return new WaitForSeconds(elapsedTime);
            Hide();
        }
    }
}