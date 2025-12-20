using Kamgam.InputHelpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The UIBase is an implementation of IUI which shows/hides the ui immediately.
    /// <br /><br />
    /// It also adds a base method for Submit/PointerUp events by calling 
    /// EventTriggerUtils.AddOnClickTriggersToAllChildren(...) in Awake()
    /// <br /><br />
    /// If you want to get informed of touch or mouse input simply override
    /// OnClick(GameObject obj, BaseEventData evt) to get the events.
    /// </summary>
    public class UIBase : MonoBehaviour, IUI, IInputMatrixReceiver
    {
        protected Canvas canvas;
        public Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = this.GetComponentInParent<Canvas>();
                }
                return canvas;
            }
        }

        /// <summary>
        /// Is the ui logically shown (will change immediately upon calling any Hide* or Show* method).<br />
        /// This does NOT tell you whether or not the ui is visually visible.
        /// </summary>
        protected bool isShown = false;

        /// <summary>
        /// Is the UI active (should receive input) according to the InputMatrix.
        /// </summary>
        protected bool isActiveInInputMatrix = false;

        /// <summary>
        /// Has the Initialize() method been called yet?
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// The UIStack used by the InputMatrix (which UIStack this ui belongs to).
        /// </summary>
        /// <returns></returns>
        public virtual UIStack GetUIStack() => UIStack.Menu;

        /// <summary>
        /// Does this UI allow parallel input in the InputMatrix?<br />
        /// This information is passed along to the InputMatrix.Push() and Pop() methods.
        /// </summary>
        /// <returns></returns>
        public virtual bool AllowParallelInput() => false;

        /// <summary>
        /// Is the ui logically shown (will change immediately upon calling any Hide* or Show* method).<br />
        /// This does NOT tell you whether or not the ui is visually visible.
        /// </summary>
        public bool IsShown()
        {
            return isShown;
        }

        /// <summary>
        /// Is the UI active (should receive input) according to the InputMatrix.
        /// </summary>
        public bool IsActiveInInputMatrix()
        {
            return isActiveInInputMatrix;
        }

        /// <summary>
        /// Is the ui currently in transition between shown/hidden state (aka is it animationg in or out).
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTransitioning()
        {
            // This ui does not animate, thus no transitioning.
            return false;
        }

        public virtual void Show()
        {
            ShowImmediate();
            UIInputMatrix.Instance.Push(GetUIStack(), this, AllowParallelInput());
        }

        public virtual void Hide()
        {
            HideImmediate();
            UIInputMatrix.Instance.Pop(GetUIStack(), this);
        }

        public virtual void ShowImmediate()
        {
            isShown = true;
            gameObject.SetActive(true);
            UIInputMatrix.Instance.Push(GetUIStack(), this, AllowParallelInput());
        }

        public virtual void HideImmediate()
        {
            isShown = false;
            gameObject.SetActive(false);
            UIInputMatrix.Instance.Pop(GetUIStack(), this);
        }

        public void SetSortOrder(int sortOrder)
        {
            if (Canvas != null)
                Canvas.sortingOrder = sortOrder;
        }

        public int GetSortOrder()
        {
            if (Canvas != null)
                return Canvas.sortingOrder;

            return 0;
        }

        // Click Events
        protected virtual void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the UI (adds triggers).<br />
        /// Call this before showing the UI for the first time. If you don't it will
        /// be called automatically in Awake() which may lead to hickups (low fps).
        /// </summary>
        public void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            AddTriggers();
        }

        /// <summary>
        /// Adds triggers to any interactable ui elements which does not yet have any triggers. Can be called multiple times.<br />
        /// Is called automatically by Initialize()<br />
        /// Use this to update trigger for newly added elements (dynamic lists for example).
        /// </summary>
        public void AddTriggers()
        {
            EventTriggerUtils.AddOnClickTriggersToAllChildren(gameObject, OnObjectClickedOrSubmitted, includeInactive: true);
        }

        /// <summary>
        /// The callback method that is triggered by the added event triggers (see AddTriggers()).
        /// </summary>
        /// <param name="evt"></param>
        public void OnObjectClickedOrSubmitted(BaseEventData evt)
        {
            // Skip handling input if not shown
            if (!isShown)
                return;

            // Skip handling input if not active in input matrix
            if (!isActiveInInputMatrix)
                return;

            // Actually selectedObject may be the wrong object if while holing down on a button the selection changes.
            // But this is very unlikely, thus we ignore it.
            var obj = evt.selectedObject;
            if (obj != null)
            {
                OnClick(obj, evt);
            }
        }

        public virtual void OnClick(GameObject obj, BaseEventData evt) {}

        public void OnActivatedInMatrix()
        {
            isActiveInInputMatrix = true;
            // Debug.Log("activated in matrix " + this.name);
        }

        public void OnDeactivatedInMatrix()
        {
            isActiveInInputMatrix = false;
            // Debug.Log("deactivated in matrix " + this.name);
        }
    }
}
