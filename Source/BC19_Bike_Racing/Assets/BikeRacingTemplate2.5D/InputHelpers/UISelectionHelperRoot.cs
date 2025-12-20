using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kamgam.InputHelpers
{
	/// <summary>
	/// By default Unity does not force any object to be selected (selection is null) 
	/// until a click with the mouse is made. This is not desirable, especially if 
	/// Keyboard only or Controller input used.<br />
	/// This class checks at regular intervals if a selection should be made and
	/// performs it.<br />
	/// It remembers the last selection and restores it if needed.<br />
	/// It searches for a reasonable selection if the current selection is null.<br />
	/// Expicit selections can be requested.<br />
	/// <br />
	/// You should only have one instance of this active at any time.
	/// </summary>
	public class UISelectionHelperRoot : MonoBehaviour
	{
		protected class SelectionCandidate
		{
			public GameObject GameObject;
			public int Priority;

			public SelectionCandidate(GameObject gameObject, int priority)
			{
				GameObject = gameObject;
				Priority = priority;
			}
		}

		#region MonoBehaviourSingleton
		private static bool destroyed = false;
		private static UISelectionHelperRoot instance = null;
		public static UISelectionHelperRoot Instance
		{
			get
			{
				if (instance == null && destroyed == false)
				{
					instance = (UISelectionHelperRoot)FindObjectOfType(typeof(UISelectionHelperRoot));
					if (instance == null)
					{
						instance = (new GameObject("UISelectionHelperRoot")).AddComponent<UISelectionHelperRoot>();
					}
					instance.initPool();
				}
				return instance;
			}
		}

		void Awake()
		{
			// Additive scene loading should make this obsolete.
			// DontDestroyOnLoad(transform.gameObject);

			// dummy call to make sure it is initialized
			var foo = Instance.AutoFixInvalidIntervalInSec;
			Instance.AutoFixInvalidIntervalInSec = foo;
		}

		public void OnDestroy()
		{
			destroyed = true;
			instance = null;
			if (requestedSelections != null)
			{
				requestedSelections.Clear();
			}
		}
		#endregion

		protected EventSystem eventSystem;
		public EventSystem EventSystem
		{
			get
			{
				if (eventSystem == null)
				{
					eventSystem = EventSystem.current;
				}
				return eventSystem;
			}
		}

		/// <summary>
		/// Set to false to disable the selection helper completely (it will do nothing).<br />
		/// Usually you set this to FALSE if a mouse is used and TRUE if a controller or
		/// keyboard is used.<br />
		/// </summary>
		[System.NonSerialized]
		public bool Enabled = true;

		/// <summary>
		/// Should the selection be fixed if the current selection is null?
		/// You can change it at any time.
		/// </summary>
		[System.NonSerialized]
		public bool AutoFixInvalidSelection = false;

		/// <summary>
		/// How often should be checked if no or an invalid object is selected?
		/// </summary>
		[System.NonSerialized]
		public float AutoFixInvalidIntervalInSec = 0.5f;

		/// <summary>
		/// Use this to register a custom "bool IsValid(GameObject obj)" function to accept or deny an object as being selectable.<br />
		/// By default each object is checked for:<br />
		/// a) whether the object is activeInHierarchy or not
		/// b) whether the selectable on the object is ".interactable" or not
		/// c) whether there is a CanvasGroup which is ".interactable" or not
		/// d) whether CustomIsValidFunc() is set and if set if it returns true
		/// </summary>
		public System.Predicate<GameObject> CustomIsValidFunc;

		/// <summary>
		/// A list of selection candidates within the last cycle. Usually a cycle is between one LateUpadte() to another LateUpdate().
		/// The order of the candidates is determined firstly by their priority. For same the priority the behaviour is undefined (usually behaves like a stack).
		/// </summary>
		protected List<SelectionCandidate> requestedSelections;
		protected List<int> pressedKeysCache = new List<int>(10); // remember the keys the user is already pressing, shouldn't be more than 10 ;)
		protected GameObject lastSelected;
		protected Vector3 lastSelectedPos;

		protected void initPool()
		{
			requestedSelections = new List<SelectionCandidate>(10);
			for (int i = 0; i < requestedSelections.Capacity; i++)
			{
				requestedSelections.Add(new SelectionCandidate(null, 0));
			}
		}

		protected bool addSelection(GameObject go, int priority, out bool alreadyInList)
		{
			if (go == null)
			{
				alreadyInList = false;
				return false;
			}

			if (requestedSelections == null)
			{
				alreadyInList = false;
				return false;
			}

			// Check if already in the list. If yes then update priority and return.
			for (int i = 0; i < requestedSelections.Count; i++)
			{
				if (requestedSelections[i].GameObject == go)
				{
					requestedSelections[i].Priority = priority;
					alreadyInList = true;
					return true;
				}
			}
			alreadyInList = false;

			// fill empty entry
			for (int i = 0; i < requestedSelections.Count; i++)
			{
				if (requestedSelections[i].GameObject == null)
				{
					requestedSelections[i].GameObject = go;
					requestedSelections[i].Priority = priority;
					return true;
				}
			}

			// or create a new one
			requestedSelections.Add(new SelectionCandidate(go, priority));
			return true;
		}

		protected bool hasSelectionCandidates()
		{
			if (requestedSelections == null)
				return false;

			for (int i = 0; i < requestedSelections.Count; i++)
			{
				if (requestedSelections[i].GameObject != null)
					return true;
			}

			return false;
		}

		protected void clearSelectionList()
		{
			for (int i = 0; i < requestedSelections.Count; i++)
			{
				requestedSelections[i].GameObject = null;
				requestedSelections[i].Priority = 0;
			}
		}

		/// <summary>
		/// Adds it to a list of selection candidates. The winner is chosen and selected in LateUpdate(), after which list is cleared.<br />
		/// The order of the candidates is determined firstly by their priority.  For same the priority the behaviour is undefined (usually behaves like a stack).
		/// </summary>
		/// </summary>
		/// <param name="go"></param>
		/// <param name="priority"></param>
		public void RequestSelection(GameObject go, int priority)
		{
			if (!Enabled)
				return;

			if(go == null)
            {
				EventSystem.SetSelectedGameObject(null);
				return;
            }

			bool alreadyInList;
			if (addSelection(go, priority, out alreadyInList))
			{
				// Set first selected on enable, BUT this is not enough, why?
				// Imagine the user is pressing the "right arrow" key.
				// Then a UI shows and triggers this OnEnable() method and therein the FirstSelected code.
				// Now the user may still hold down the "right arrow" key which is then 
				// interpreted by the InputModule as a command to change the currently selected
				// item. Suddenly the First Selected is no longer the selected.
				// Solution: wait for the user to release the pressed keys and then apply the
				// FirstSelected. Keep in mind: what if the user releases the cursor but
				// presses another key (that's why we only react to some keys in pressedKeysCache). 
				if (!alreadyInList)
				{
					fillPressedKeyCache();
				}
			}
		}

		public void DeselectAll()
		{
			if (!Enabled)
				return;

			if (EventSystem != null)
			{
				EventSystem.SetSelectedGameObject(null);
			}
		}

		protected void handleSelectionCandidates()
		{
			if (hasSelectionCandidates())
			{
				GameObject gameObjectToSelect;
				SelectionCandidate tmpSelection = null;
				for (int i = requestedSelections.Count - 1; i >= 0; i--)
				{
					if ((tmpSelection == null || requestedSelections[i].Priority > tmpSelection.Priority) && isValid(requestedSelections[i].GameObject))
					{
						tmpSelection = requestedSelections[i];
					}
				}
				if (tmpSelection != null)
				{
					gameObjectToSelect = tmpSelection.GameObject;
					clearSelectionList();

					// set selection
					setSelected(gameObjectToSelect);
				}
			}
		}

		protected void setSelected(GameObject go)
		{
			if (EventSystem != null && go != null)
			{
				bool wasSelected = EventSystem.currentSelectedGameObject == go;
				EventSystem.SetSelectedGameObject(go);
				// Ensure that even for reselection the select handler is being fired.
				if (wasSelected)
				{
					ExecuteEvents.ExecuteHierarchy(go, new BaseEventData(EventSystem), ExecuteEvents.selectHandler);
				}
			}
		}

		protected bool isValid(GameObject go)
		{
			if (go == null)
				return false;

			if (go.activeInHierarchy == false)
				return false;

			var selectable = go.GetComponent<Selectable>();
			if (selectable != null && (selectable.navigation.mode == Navigation.Mode.None || !selectable.interactable))
				return false;

			var canvasGroup = go.GetComponentInParent<CanvasGroup>();
			if (canvasGroup != null && !canvasGroup.interactable)
				return false;

			if (CustomIsValidFunc != null && !CustomIsValidFunc.Invoke(go))
				return false;

			return true;
		}

		protected void fillPressedKeyCache()
		{
			pressedKeysCache.Clear();

			// keyboard
			if (Input.GetKey(KeyCode.DownArrow)) pressedKeysCache.Add(1);
			if (Input.GetKey(KeyCode.UpArrow)) pressedKeysCache.Add(2);
			if (Input.GetKey(KeyCode.LeftArrow)) pressedKeysCache.Add(3);
			if (Input.GetKey(KeyCode.RightArrow)) pressedKeysCache.Add(4);

			// Controller support. The keys are different for every controller. What a nightmare xD
			/*
			// I encourage the use of InControl https://assetstore.unity.com/packages/tools/input-management/incontrol-14695
			if (InControl.InputManager.ActiveDevice.DeviceClass == InControl.InputDeviceClass.Controller && InControl.InputManager.ActiveDevices.Count > 0)
			{
				// stick
				if (InControl.InputManager.ActiveDevice.LeftStick.Left.IsPressed) pressedKeysCache.Add(5);
				if (InControl.InputManager.ActiveDevice.LeftStick.Right.IsPressed) pressedKeysCache.Add(6);
				if (InControl.InputManager.ActiveDevice.LeftStick.Up.IsPressed) pressedKeysCache.Add(7);
				if (InControl.InputManager.ActiveDevice.LeftStick.Down.IsPressed) pressedKeysCache.Add(8);

				// dpad
				if (InControl.InputManager.ActiveDevice.DPad.Left.IsPressed) pressedKeysCache.Add(9);
				if (InControl.InputManager.ActiveDevice.DPad.Right.IsPressed) pressedKeysCache.Add(10);
				if (InControl.InputManager.ActiveDevice.DPad.Up.IsPressed) pressedKeysCache.Add(11);
				if (InControl.InputManager.ActiveDevice.DPad.Down.IsPressed) pressedKeysCache.Add(12);
			}
			*/
		}

		protected bool keysReleased()
		{
			if (pressedKeysCache.Count > 0)
			{
				// keyboard
				removeKeyFromPressedKeyCacheIfNecessary(1);
				removeKeyFromPressedKeyCacheIfNecessary(2);
				removeKeyFromPressedKeyCacheIfNecessary(3);
				removeKeyFromPressedKeyCacheIfNecessary(4);

				// Uncomment if InControl is used.
				/*
				// controller - stick
				removeKeyFromPressedKeyCacheIfNecessary(5);
				removeKeyFromPressedKeyCacheIfNecessary(6);
				removeKeyFromPressedKeyCacheIfNecessary(7);
				removeKeyFromPressedKeyCacheIfNecessary(8);

				// controller - dpad
				removeKeyFromPressedKeyCacheIfNecessary(9);
				removeKeyFromPressedKeyCacheIfNecessary(10);
				removeKeyFromPressedKeyCacheIfNecessary(11);
				removeKeyFromPressedKeyCacheIfNecessary(12);
				*/

				// have all keys been released?
				if (pressedKeysCache.Count > 0)
				{
					return false;
				}
			}
			return true;
		}

		protected void removeKeyFromPressedKeyCacheIfNecessary(int key)
		{
			if (pressedKeysCache.Contains(key))
			{
				if (key == 1 && Input.GetKey(KeyCode.DownArrow) == false) pressedKeysCache.Remove(key);
				if (key == 2 && Input.GetKey(KeyCode.UpArrow) == false) pressedKeysCache.Remove(key);
				if (key == 3 && Input.GetKey(KeyCode.LeftArrow) == false) pressedKeysCache.Remove(key);
				if (key == 4 && Input.GetKey(KeyCode.RightArrow) == false) pressedKeysCache.Remove(key);

				// Uncomment if InControl is used.
				/*
				// stick
				if (key == 5 && InControl.InputManager.ActiveDevice.LeftStick.Left.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 6 && InControl.InputManager.ActiveDevice.LeftStick.Right.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 7 && InControl.InputManager.ActiveDevice.LeftStick.Up.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 8 && InControl.InputManager.ActiveDevice.LeftStick.Down.IsPressed == false) pressedKeysCache.Remove(key);

				// dpad
				if (key == 9 && InControl.InputManager.ActiveDevice.DPad.Left.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 10 && InControl.InputManager.ActiveDevice.DPad.Right.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 11 && InControl.InputManager.ActiveDevice.DPad.Up.IsPressed == false) pressedKeysCache.Remove(key);
				if (key == 12 && InControl.InputManager.ActiveDevice.DPad.Down.IsPressed == false) pressedKeysCache.Remove(key);
				*/
			}
		}

		protected float fixInvalidTimer;

		protected void Update()
		{
			if (!Enabled)
				return;

			if (EventSystem != null && instance != null)
			{
				// remember
				if (lastSelected != EventSystem.currentSelectedGameObject && EventSystem.currentSelectedGameObject != null && isValid(EventSystem.currentSelectedGameObject))
				{
					lastSelected = EventSystem.currentSelectedGameObject;
					lastSelectedPos = lastSelected.transform.position;
				}
			}
		}

		public static string Log;

		protected void LateUpdate()
		{
			if (!Enabled)
				return;

			if (EventSystem != null && instance != null)
			{
				// handle requested selections
				if (hasSelectionCandidates() && keysReleased())
				{
					handleSelectionCandidates();
				}

				// The code which is now in Update() was here once but the transform positions were off, thus moved to Update(), there it works.
				// This is very odd since the docs explicitly mention that LateUpdate() is THE place to read transforms for Camera follow etc.

				// Fixing invalid selections is
				fixInvalidTimer -= Time.unscaledDeltaTime;
				if (fixInvalidTimer <= 0)
				{
					fixInvalidTimer = AutoFixInvalidIntervalInSec;
					if (AutoFixInvalidSelection)
					{
						FixSelection();
					}
				}
			}

			/*
			if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
				Log = $"UISelectionHelperRoot: selectedObj[{EventSystem.current.currentSelectedGameObject.name}]";
			else
				Log = $"UISelectionHelperRoot: selectedObj[null]";
			//*/
		}

		public void FixSelection()
        {
			if (EventSystem.currentSelectedGameObject == null || !isValid(EventSystem.currentSelectedGameObject))
			{
				fixInvalidSelection();
			}
		}

		protected void fixInvalidSelection()
		{
			if (!Enabled)
				return;

			if (Selectable.allSelectableCount > 0)
			{
				// last selection is inactive, what to do now?
				float minSqrDistance = 9990999f;
				float sqrDistance;
				Selectable closestSelectable = null;
				foreach (var selectable in Selectable.allSelectablesArray)
				{
					if (isValid(selectable.gameObject))
					{
						sqrDistance = Vector3.Distance(lastSelectedPos, selectable.transform.position);
						if (sqrDistance < minSqrDistance)
						{
							minSqrDistance = sqrDistance;
							closestSelectable = selectable;
						}
					}
				}
				if (closestSelectable != null)
				{
					setSelected(closestSelectable.gameObject);
				}
			}
		}
	}
}