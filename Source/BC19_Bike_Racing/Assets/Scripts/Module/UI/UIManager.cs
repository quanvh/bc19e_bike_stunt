using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bacon
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance = null;

        public static Transform RootTransform = null;

        public static List<PopupBase> PopupList = new List<PopupBase>();

        public static RectTransform RootRectTransform = null;

        private static Vector2 startAnchoredPosition2D = Vector2.zero;

        private PopupBase CurrentPopup = null, LastPopup = null;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }

            Instance = this;
            if (RootTransform == null)
            {
                RootTransform = GetComponent<Transform>();
            }

            if (RootRectTransform == null)
            {
                RootRectTransform = GetComponent<RectTransform>();
            }
            AddPopupList();
        }

        public T GetUI<T>() where T : PopupBase
        {
            foreach (var ui in PopupList)
            {
                if (ui is T t)
                    return t;
            }

            return default;
        }

        private void AddPopupList()
        {
            //Get all popupbase component in Safearea - get transform child 0
            foreach (Transform t in transform.GetChild(0))
            {
                if (t.TryGetComponent(out PopupBase pop))
                    PopupList.Add(pop);
            }
        }

        public void ShowPopup<T>(float delay) where T : PopupBase => ShowPopup<T>(null, null, null, delay);

        public void ShowPopup<T>(bool hideAll) where T : PopupBase => ShowPopup<T>(null, null, null, 0, hideAll);

        public void ShowPopup<T>(float delay, bool hideAll) where T : PopupBase => ShowPopup<T>(null, null, null, delay, hideAll);

        public void ShowPopup<T>(Action OnHide, bool hideAll) where T : PopupBase => ShowPopup<T>(null, null, OnHide, 0, hideAll);


        public void ShowPopup<T>(Action<T> OnShowStart, bool hideAll) where T : PopupBase
            => ShowPopup<T>(OnShowStart, null, null, 0, hideAll);

        public void ShowPopup<T>(Action<T> OnShowStart, Action OnHide, bool hideAll) where T : PopupBase
            => ShowPopup<T>(OnShowStart, null, OnHide, 0, hideAll);

        public void ShowPopup<T>(Action<T> OnShowStart = null, Action<T> OnShowComplete = null,
            Action OnHide = null, float delay = 0f, bool hideAll = true) where T : PopupBase
        {
            //set default delay for popup Hide
            if (delay == -1) delay = 0.175f;

            if (hideAll)
            {
                foreach (PopupBase popup in PopupList)
                {
                    if (popup is not T)
                    {
                        if (popup.isActiveAndEnabled && !popup.IsAnchor)
                            popup.Hide();
                    }
                    else
                    {
                        if (CurrentPopup != null) LastPopup = CurrentPopup;
                        CurrentPopup = popup;

                        if (delay == 0)
                        {
                            popup.Show(popup => OnShowStart?.Invoke((T)popup), popup => OnShowComplete?.Invoke((T)popup), OnHide);
                        }
                        else
                        {
                            StartCoroutine(DoShowPopup<T>(OnShowStart, OnShowComplete, OnHide, delay));
                        }
                    }
                }
            }
            else
            {
                StartCoroutine(DoShowPopup<T>(OnShowStart, OnShowComplete, OnHide, delay));
            }
        }

        private IEnumerator DoShowPopup<T>(Action<T> OnShowStart = null, Action<T> OnShowComplete = null,
            Action OnHide = null, float delay = 0f) where T : PopupBase
        {
            yield return new WaitForSeconds(delay);
            PopupBase popup = GetUI<T>();
            if (popup != null)
            {
                popup.Show(popup => OnShowStart?.Invoke((T)popup), popup => OnShowComplete?.Invoke((T)popup), OnHide);
            }
        }

        private void DoHidePopup(PopupBase popup, Action OnHideComplete = null, bool showPrevious = false)
        {
            if (popup != null)
            {
                popup.Hide(() =>
                {
                    OnHideComplete?.Invoke();

                    if (showPrevious && LastPopup != null)
                    {
                        LastPopup.Show(t =>
                        {
                            CurrentPopup = LastPopup;
                            LastPopup = null;
                        });
                    }

                });
            }
        }

        public void HidePopup(bool force)
        {
            foreach (PopupBase popup in PopupList)
            {
                if (popup.isActiveAndEnabled && (!popup.IsAnchor || force))
                    popup.Hide();
            }
        }

        public void HidePopup<T>(Action OnHideComplete = null) where T : PopupBase
        {
            PopupBase popup = GetUI<T>();
            DoHidePopup(popup, OnHideComplete);
        }

        public void HidePopup(Action OnHideComplete = null, bool showPrevious = false)
        {
            DoHidePopup(CurrentPopup, OnHideComplete, showPrevious);
        }

        public static Vector2 GetPosBy(RectTransform rectTransform, MoveDirection position)
        {
            try
            {
                Vector3 vector = Vector3.zero;
                Vector3 zero = Vector3.zero;
                _ = Vector3.zero;
                if (RootTransform == null)
                {
                    RootTransform = Instance?.GetComponent<Transform>();
                }

                if (RootRectTransform == null)
                {
                    RootRectTransform = Instance?.GetComponent<RectTransform>();
                }

                float num = RootRectTransform.rect.width / 2f + rectTransform.rect.width * rectTransform.pivot.x;
                float num2 = RootRectTransform.rect.height / 2f + rectTransform.rect.height * rectTransform.pivot.y;
                switch (position)
                {
                    case MoveDirection.ParentPosition:
                        if (RootRectTransform == null)
                        {
                            return vector;
                        }

                        vector = new Vector2(RootRectTransform.anchoredPosition.x + zero.x, RootRectTransform.anchoredPosition.y + zero.y);
                        break;
                    case MoveDirection.TopScreenEdge:
                        vector = new Vector2(zero.x + startAnchoredPosition2D.x, zero.y + num2);
                        break;
                    case MoveDirection.RightScreenEdge:
                        vector = new Vector2(zero.x + num, zero.y + startAnchoredPosition2D.y);
                        break;
                    case MoveDirection.BottomScreenEdge:
                        vector = new Vector2(zero.x + startAnchoredPosition2D.x, zero.y - num2);
                        break;
                    case MoveDirection.LeftScreenEdge:
                        vector = new Vector2(zero.x - num, zero.y + startAnchoredPosition2D.y);
                        break;
                    default:
                        Debug.LogWarning("[UIManager] This should not happen! DoMoveIn in UIAnimator went to the default setting!");
                        break;
                }

                return vector;
            }
            catch (Exception ex)
            {
                Debug.LogError("[UIManager] GetPosition: " + rectTransform.name + " " + ex.Message + "\n" + ex.StackTrace);
                return default(Vector3);
            }
        }

    }

    public enum PopupAnim
    {
        None,
        SlideIn,
        SlideOut,
        FadeIn,
        FadeOut
    }

    public enum MoveDirection
    {
        ParentPosition,
        TopScreenEdge,
        RightScreenEdge,
        BottomScreenEdge,
        LeftScreenEdge
    }
}