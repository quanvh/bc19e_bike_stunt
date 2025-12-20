#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Kamgam.BridgeBuilder25D.Helpers
{
    public static class UtilsEditor
    {
        public static bool IsEditing()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode == false && BuildPipeline.isBuildingPlayer == false;
        }

		protected class DelayedExecution
		{
			public double Time;
			public Action Action;

            public DelayedExecution(double time, Action action)
            {
                Time = time;
                Action = action;
            }
        }

		static List<DelayedExecution> delayedExecutions;

		public static void ExecuteDelayed(float delayInSec, Action action)
        {
			if (action == null)
				return;

			if (delayedExecutions == null)
				delayedExecutions = new List<DelayedExecution>();

			delayedExecutions.Add(new DelayedExecution(EditorApplication.timeSinceStartup + delayInSec, action));

			EditorApplication.update -= onExecuteDelayed;
            EditorApplication.update += onExecuteDelayed;
        }

        static void onExecuteDelayed()
        {
            for (int i = delayedExecutions.Count-1; i >= 0; i--)
            {
				if (EditorApplication.timeSinceStartup >= delayedExecutions[i].Time)
                {
					delayedExecutions[i].Action.Invoke();
					delayedExecutions.RemoveAt(i);
                }
            }

			if(delayedExecutions.Count == 0)
				EditorApplication.update -= onExecuteDelayed;
		}

		/// <summary>
		/// Returns true if any property has changed.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public static bool DrawDefaultInspector(UnityEngine.Object obj, bool skipScript = false, bool enterChildren = false, bool applyChanges = true)
        {
            var serializedObject = new SerializedObject(obj);
            return DrawDefaultInspector(serializedObject, skipScript, enterChildren, applyChanges);
        }

        public static bool DrawDefaultInspector(SerializedObject serializedObject, bool skipScript = false, bool enterChildren = false, bool applyChanges = true)
        {
            serializedObject.Update();
            SerializedProperty serializedProperty = serializedObject.GetIterator();
            
            if(skipScript)
                serializedProperty.NextVisible(enterChildren: true);

            while (serializedProperty.NextVisible(enterChildren))
            {
                EditorGUILayout.PropertyField(serializedProperty);
            }

            bool wasModified = serializedObject.hasModifiedProperties;

			if (wasModified && applyChanges)
				serializedObject.ApplyModifiedProperties();

			return wasModified;
        }

		/// <summary>
		/// Calculates the intersection point of a ray from the center of the EditorSceneView camera forward.
		/// </summary>
		/// <param name="planeTransform">The transfrom which the XY plane will be constructed from. Set to null for global space XY.</param>
		/// <param name="position">The result (world space position of the hit).</param>
		/// <param name="defaultPosition">position value will be set to this if not hit.</param>
		/// <returns>True if hit, False if not hit.</returns>
		public static bool IntersectEditorCameraRayWithXYInTransform(Transform planeTransform, out Vector3 position, Vector3 defaultPosition)
		{
			// raycast from the editor camera center to the controller XY plane and save result in position.
			var cam = SceneView.lastActiveSceneView.camera;
			if (cam != null)
			{
				var rect = cam.pixelRect;
				var ray = cam.ScreenPointToRay(
					new Vector2(
						rect.width * 0.5f, rect.height * 0.5f
						)
					);

				Plane plane;
				if (planeTransform != null)
					plane = new Plane(planeTransform.TransformDirection(Vector3.forward), planeTransform.TransformPoint(Vector3.zero));
				else
					plane = new Plane(Vector3.forward, Vector3.zero);
				float enter;
				if (plane.Raycast(ray, out enter))
				{
					Vector3 hitPoint = ray.GetPoint(enter);
					position = hitPoint;
					return true;
				}
			}

			position = defaultPosition;
			return false;
		}

        public static KeyCode TryGetKeyCodeForBinding(string binding, KeyCode defaultValue)
        {
            // https://docs.unity3d.com/2019.4/Documentation/ScriptReference/ShortcutManagement.IShortcutManager.GetAvailableShortcutIds.html
            // Throws ArgumentException if shortcutId is not available, i.e. when GetAvailableShortcutIds does not contain shortcutId.
            var bindings = ShortcutManager.instance.GetAvailableShortcutIds();
            foreach (var b in bindings)
            {
                if (b == binding)
                {
                    var combo = ShortcutManager.instance.GetShortcutBinding(binding).keyCombinationSequence;
                    foreach (var c in combo)
                    {
                        return c.keyCode;
                    }
                }
            }

            return defaultValue;
        }
    }
}
#endif