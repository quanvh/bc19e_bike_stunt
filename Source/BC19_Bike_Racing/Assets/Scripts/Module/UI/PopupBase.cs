using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;


namespace Bacon
{
    public class PopupBase : MonoBehaviour
    {

        protected RectTransform rectTransform;

        protected CanvasGroup canvasGroup;

        protected Action actionOnHideCompleted;

        [FoldoutGroup("POPUP BASE", expanded: false)]
        public PopupAnim animationIn = PopupAnim.SlideIn;

        [FoldoutGroup("POPUP BASE")]
        public Ease easeIn = Ease.OutCubic;

        [FoldoutGroup("POPUP BASE")]
        public MoveDirection positionStart = MoveDirection.BottomScreenEdge;

        [FoldoutGroup("POPUP BASE"), Range(0f, 10f)]
        public float timeAnimationIn = 0.25f;

        [FoldoutGroup("POPUP BASE"), Range(0f, 10f)]
        public float timeDelayIn;

        [FoldoutGroup("POPUP BASE"), Space(10f)]
        public PopupAnim animationOut = PopupAnim.SlideOut;

        [FoldoutGroup("POPUP BASE")]
        public Ease easeOut = Ease.InCubic;

        [FoldoutGroup("POPUP BASE")]
        public MoveDirection positionOut = MoveDirection.BottomScreenEdge;

        [FoldoutGroup("POPUP BASE"), Range(0f, 10f)]
        public float timeAnimationOut = 0.175f;

        [FoldoutGroup("POPUP BASE"), Range(0f, 10f)]
        public float timeDelayOut;

        [FoldoutGroup("POPUP BASE"), Space]
        public UnityEvent onShowStart;

        [FoldoutGroup("POPUP BASE"), Space]
        public UnityEvent onShowCompleted;

        [FoldoutGroup("POPUP BASE"), Space]
        public UnityEvent onHideCompleted;

        [FoldoutGroup("POPUP BASE"), Space]
        public bool IsAnchor = false;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if ((animationIn == PopupAnim.FadeIn || animationOut == PopupAnim.FadeOut) && !TryGetComponent(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        protected virtual void OnEnable()
        {
            if (DataManager.Instance)
                _player = DataManager.Instance._player;
        }

        protected virtual void OnDisable()
        {
            isShow = false;
        }

        protected virtual void ShowStart() { }

        protected virtual void ShowCompleted() { }

        protected virtual void HideStart() { }

        protected bool isShow = false;
        private bool animating = false;
        public void Show(Action<PopupBase> actionOnShowStart = null, Action<PopupBase> actionOnShowCompleted = null, Action actionOnHideCompleted = null)
        {
            if (animating) return;
            animating = true;

            if (!UIManager.PopupList.Contains(this))
            {
                UIManager.PopupList.Add(this);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(true);
            rectTransform.DOKill(complete: true);
            rectTransform.anchoredPosition = UIManager.GetPosBy(rectTransform, positionStart);
            if (canvasGroup != null)
            {
                canvasGroup.DOKill(complete: true);
                DOTween.To(() => canvasGroup.alpha, delegate (float x)
                {
                    canvasGroup.alpha = x;
                }, 1f, timeAnimationIn).SetUpdate(UpdateType.Normal, isIndependentUpdate: true).SetDelay(timeDelayIn)
                    .SetEase(easeIn)
                    .OnStart(delegate
                    {
                        isShow = true;
                        ShowStart();
                        onShowStart?.Invoke();
                        actionOnShowStart?.Invoke(this);
                    })
                    .OnComplete(delegate
                    {
                        animating = false;
                        onShowCompleted?.Invoke();
                        ShowCompleted();
                        actionOnShowCompleted?.Invoke(this);
                    })
                    .SetTarget(canvasGroup);
            }
            else
            {
                DOTween.To(() => rectTransform.anchoredPosition, delegate (Vector2 x)
                {
                    rectTransform.anchoredPosition = x;
                }, Vector2.zero, timeAnimationIn).SetDelay(timeDelayIn).SetEase(easeIn)
                    .SetUpdate(UpdateType.Normal, isIndependentUpdate: true)
                    .OnStart(delegate
                    {
                        isShow = true;
                        ShowStart();
                        onShowStart?.Invoke();
                        actionOnShowStart?.Invoke(this);
                    })
                    .OnComplete(delegate
                    {
                        animating = false;
                        onShowCompleted?.Invoke();
                        ShowCompleted();
                        actionOnShowCompleted?.Invoke(this);
                    })
                    .SetTarget(rectTransform);
            }

            this.actionOnHideCompleted = actionOnHideCompleted;
        }

        public void Hide(Action onCompleted = null)
        {
            if (animating) return;
            animating = true;
            if (canvasGroup != null)
            {
                canvasGroup.DOKill(complete: true);
                DOTween.To(() => canvasGroup.alpha, delegate (float x)
                {
                    canvasGroup.alpha = x;
                }, 0f, timeAnimationOut).SetUpdate(UpdateType.Normal, isIndependentUpdate: true).SetDelay(timeDelayOut)
                    .SetEase(easeOut)
                    .OnComplete(delegate
                    {
                        isShow = false;
                        actionOnHideCompleted?.Invoke();
                        actionOnHideCompleted = null;
                        onHideCompleted?.Invoke();
                        onCompleted?.Invoke();
                        animating = false;
                        gameObject.SetActive(false);
                    })
                    .OnStart(HideStart)
                    .SetTarget(canvasGroup);
                return;
            }

            Vector2 posBy = UIManager.GetPosBy(rectTransform, positionOut);
            rectTransform.DOKill(complete: true);
            DOTween.To(() => rectTransform.anchoredPosition, delegate (Vector2 x)
            {
                rectTransform.anchoredPosition = x;
            }, posBy, timeAnimationOut).SetDelay(timeDelayOut).SetEase(easeOut)
                .SetUpdate(UpdateType.Normal, isIndependentUpdate: true)
                .OnComplete(delegate
                {
                    isShow = false;
                    actionOnHideCompleted?.Invoke();
                    actionOnHideCompleted = null;
                    onHideCompleted?.Invoke();
                    onCompleted?.Invoke();
                    animating = false;
                    gameObject.SetActive(false);
                })
                .OnStart(HideStart)
                .SetTarget(rectTransform);
        }

        public void SetShow()
        {
            Show();
        }

        public void SetHide()
        {
            Hide();
        }

        protected PlayerModel _player;
    }
}