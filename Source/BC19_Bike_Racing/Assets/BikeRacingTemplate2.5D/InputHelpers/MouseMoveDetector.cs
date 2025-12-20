using UnityEngine;

namespace Kamgam.InputHelpers
{
	/// <summary>
	/// Tracks the mouse position and caculates the delta within the last frame (calculation is done in Update()).
	/// </summary>
	public class MouseMoveDetector : MonoBehaviour
	{
		#region MonoBehaviourSingleton
		private static bool destroyed = false;
		private static MouseMoveDetector instance = null;
		public static MouseMoveDetector Instance
		{
			get
			{
				if (instance == null && destroyed == false)
				{
					instance = (MouseMoveDetector)FindObjectOfType(typeof(MouseMoveDetector));
					if (instance == null)
					{
						instance = (new GameObject("MouseMoveDetector")).AddComponent<MouseMoveDetector>();
					}
				}
				return instance;
			}
		}

		void Awake()
		{
			// Additive scene loading should make this obsolete.
			// DontDestroyOnLoad(transform.gameObject);

			// dummy call to make sure it is initialized
			Instance.NoOp();
		}

		public void OnDestroy()
		{
			destroyed = true;
			instance = null;
		}

		public void NoOp() { }
        #endregion

		/// <summary>
		/// The distance the mouse moved during the last frame.
		/// </summary>
		public Vector3 MouseDelta { get; set; }

		/// <summary>
		/// The last known mouse position (see Input.mousePosition).
		/// </summary>
		public Vector3 LastMousePos { get; set; }

		bool firstRun = true;

		private void Update()
        {
			if (!Input.mousePresent)
				return;

            if (firstRun)
            {
				MouseDelta = Vector3.zero;
				LastMousePos = Input.mousePosition;
				firstRun = false;
			}
			else
            {
				MouseDelta = Input.mousePosition - LastMousePos;
				LastMousePos = Input.mousePosition;
			}
        }
    }
}