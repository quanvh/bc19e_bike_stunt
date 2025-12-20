using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
	public class UIReferences : MonoBehaviour
	{
		public static UIReferences Instance;

		protected List<IUI> uis = new List<IUI>();

        private void Awake()
        {
			Instance = this;

			var roots = gameObject.scene.GetRootGameObjects();
            foreach (var root in roots)
            {
				var ui = root.GetComponent<IUI>();
				if (ui != null)
					uis.Add(ui);
            }
		}

		public T GetUI<T>() where T : IUI
        {
            foreach (var ui in uis)
            {
				if (ui is T)
					return (T)ui;
            }

			return default(T);
        }

		public void HideAllImmediate()
        {
            foreach (var ui in uis)
				ui.HideImmediate();
		}

		public void InitializeAll()
		{
			foreach (var ui in uis)
				ui.Initialize();
		}
	}
}

