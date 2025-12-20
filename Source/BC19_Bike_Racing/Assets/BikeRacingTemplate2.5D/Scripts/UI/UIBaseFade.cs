using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The UIBase is an implementation of IUI which shows/hides the ui with a delay if there is a canvas group attached to the object.
    /// </summary>
    public class UIBaseFade : UIBase
    {
        [System.NonSerialized]
        public float FadeSpeed = 3f;

        /// <summary>
        /// -1 = fading out
        ///  0 = not fading
        ///  1 = fading in
        /// </summary>
        int isFading = 0;
        float targetAlpha;

        protected CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null)
                {
                    canvasGroup = this.GetComponent<CanvasGroup>();
                }
                return canvasGroup;
            }
        }

        public override bool IsTransitioning()
        {
            return isFading != 0;
        }

        public override void Show()
        {
            // logically it's shown immediately
            isShown = true;

            // Schedule fadeing if a CanvasGroup exists
            isFading = CanvasGroup != null ? 1 : 0;
            targetAlpha = 1f;

            // make UI interactable if a CanvasGroup exists
            if (CanvasGroup != null)
                CanvasGroup.interactable = true;

            // set active immediately
            gameObject.SetActive(true);

            Update();

            UIInputMatrix.Instance.Push(GetUIStack(), this, AllowParallelInput());
        }

        public override void ShowImmediate()
        {
            base.ShowImmediate();

            if (CanvasGroup != null)
                CanvasGroup.alpha = 1f;
        }

        public override void Hide()
        {
            // logically it's hidden immediately
            isShown = false;

            // make UI not interactable if a CanvasGroup exists
            if (CanvasGroup != null)
                CanvasGroup.interactable = false;

            // If there is no CanvasGroup then disable immediately
            if (CanvasGroup == null)
                gameObject.SetActive(false);

            // Schedule fadeing if a CanvasGroup exists
            // The IF exists to avoid being stuck in isFading = -1 if Hide() is called on an already inactive object.
            if (gameObject.activeInHierarchy)
                isFading = CanvasGroup != null ? -1 : 0;
            else
                isFading = 0;
            targetAlpha = 0f;

            Update();

            UIInputMatrix.Instance.Pop(GetUIStack(), this);
        }

        public override void HideImmediate()
        {
            base.HideImmediate();

            if (CanvasGroup != null)
                CanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (isFading == -1)
            {
                float newAlpha = CanvasGroup.alpha - (Time.deltaTime * FadeSpeed);
                CanvasGroup.alpha = newAlpha;
                if (newAlpha <= targetAlpha)
                {
                    CanvasGroup.alpha = targetAlpha;
                    isFading = 0;
                    gameObject.SetActive(false);
                }
            }
            else if (isFading == 1)
            {
                float newAlpha = CanvasGroup.alpha + (Time.deltaTime * FadeSpeed);
                CanvasGroup.alpha = newAlpha;
                if (newAlpha >= targetAlpha)
                {
                    CanvasGroup.alpha = targetAlpha;
                    isFading = 0;
                }
            }
        }
    }
}
