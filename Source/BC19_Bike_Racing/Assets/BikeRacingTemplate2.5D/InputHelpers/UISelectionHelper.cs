using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.InputHelpers
{
	/// <summary>
	/// Serves two purposes:
	/// A) It remembers the current selection in OnEnable() and restores it in OnDisable()
	/// B) Selects the FirstSelected object (if there is one).
	/// </summary>
	public class UISelectionHelper : MonoBehaviour
	{
		[Tooltip("Should it remember the previously selected object and try to restore in OnDisable()?")]
		public bool RestorePreviousSelection = true;

		[Tooltip("Delay the first selected request for 2 frames to allow custom logic in OnEnable() or Start() to do their stuff.\n\nUse this if FirstSelected might be disabled in OnEnable() or Start().")]
		public bool Delay = false;

		[Tooltip("If multiple helpers define a first selected then higher priorities will take precedence.")]
		public int Priority = 1;

		[Tooltip("Gets selected if it is active.")]
		public GameObject FirstSelected;

		[Tooltip("Gets selected if FirstSelect is null or inactive. The first active item in the list will be used.")]
		public GameObject[] SecondarySelectedList;

		protected EventSystem eventSystem;
		public EventSystem EventSystem
		{
			get
			{
				if (eventSystem == null || eventSystem.isActiveAndEnabled == false)
				{
					eventSystem = EventSystem.current;
				}
				return eventSystem;
			}
		}

		/// <summary>
		/// The object which has been selected before the FirstSelected kicked in.
		/// </summary>
		protected GameObject selectedBefore;

		public GameObject GetFirstSelected()
		{
			if (FirstSelected != null && FirstSelected.activeInHierarchy)
			{
				return FirstSelected;
			}
			else
			{
				foreach (var secondarySelected in SecondarySelectedList)
				{
					if (secondarySelected != null && secondarySelected.activeInHierarchy)
					{
						return secondarySelected;
					}
				}
			}
			return null;
		}

		public bool HasFocus()
		{
			return EventSystem != null && EventSystem.currentSelectedGameObject != null && EventSystem.currentSelectedGameObject.transform.IsChildOf(this.transform);
		}

		public void OnEnable()
		{
			if (Delay)
			{
				StartCoroutine(firstSelectedDelayed());
			}
			else
			{
				RequestFirstSelected();
			}
		}

		protected IEnumerator firstSelectedDelayed()
		{
			yield return new WaitForFrames(2);
			RequestFirstSelected();
		}

		protected IEnumerator firstSelectedDelayed(float delayInSec)
		{
			yield return new WaitForSecondsRealtime(delayInSec);
			RequestFirstSelected();
		}

		public virtual void RequestFirstSelected()
		{
			if (RestorePreviousSelection)
			{
				selectedBefore = EventSystem.currentSelectedGameObject;
			}
			UISelectionHelperRoot.Instance.RequestSelection(GetFirstSelected(), Priority);
		}

		public virtual void RequestFirstSelected(float delayInSec)
		{
			if (delayInSec == 0f)
			{
				RequestFirstSelected();
			}
			else
			{
				StartCoroutine(firstSelectedDelayed(delayInSec));
			}
		}

		public virtual void OnDisable()
		{
			if (RestorePreviousSelection)
			{
				if (selectedBefore != null && selectedBefore.activeInHierarchy)
				{
					UISelectionHelperRoot.Instance.RequestSelection(selectedBefore, Priority);
				}
			}
			selectedBefore = null;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(UISelectionHelper), editorForChildClasses: true)]
	public class UiSelectionHelperEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var helper = this.target as UISelectionHelper;
			if (GUILayout.Button("RequestFirstSelected"))
			{
				helper.RequestFirstSelected();
			}
		}
	}
#endif
}
