#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib.Helpers
{
    public static class UtilsEditor
    {
        public static bool IsEditing()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode == false && BuildPipeline.isBuildingPlayer == false;
        }

		// reflection cache for mesh raycast (speeds up code execution)
		private static bool cacheBuilt;
		private static Type[] _rcEditorTypes;
		private static Type _rcHandleUtilityType;
		private static System.Reflection.MethodInfo _rcIntersectRayMeshMethod;

		private static void buildRayMeshReflectionCache( bool forceRebuild = false )
		{
			try
			{
				if (cacheBuilt == false || forceRebuild)
				{
					cacheBuilt = true;
					_rcEditorTypes = typeof(Editor).Assembly.GetTypes();
					if (_rcEditorTypes != null && _rcEditorTypes.Length > 0)
					{
						_rcHandleUtilityType = _rcEditorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
						if (_rcHandleUtilityType != null)
						{
							_rcIntersectRayMeshMethod = _rcHandleUtilityType.GetMethod(
								"IntersectRayMesh",
								System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
						}
					}
				}
			}
			catch (Exception)
			{
				// fail silently
			}
		}

		/// <summary>
		/// Checks if a ray intersects with a mesh and saves the result in hit.
		/// Return an integer meaning: -1 = reflection didn't work, 1 = mesh was hit by the ray, 0 = mesh was not hit by the ray.
		/// Thanks to: https://forum.unity.com/threads/editor-raycast-against-scene-meshes-without-collider-editor-select-object-using-gui-coordinate.485502/#post-3162431
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="meshFilter"></param>
		/// <param name="hit"></param>
		/// <returns>An integer meaning: -1 = reflection didn't work, 1 = mesh was hit by the ray, 0 = mesh was not hit by the ray</returns>
		public static int IntersectRayMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
		{
			if (_rcIntersectRayMeshMethod == null)
			{
				buildRayMeshReflectionCache(false);
			}

			// this depends on the reflection cache
			if (_rcIntersectRayMeshMethod != null)
			{
				var parameters = new object[] { ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, null };
				var result = _rcIntersectRayMeshMethod.Invoke(null, parameters);
				hit = (RaycastHit)parameters[3];
				if (result != null)
				{
					if ((bool)result == true)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
			}

			hit = default(RaycastHit);
			return -1;
		}

		/// <summary>
		/// Checks if a ray intersects with a mesh and saves the result in hit.
		/// Return an integer meaning: -1 = reflection didn't work, 1 = mesh was hit by the ray, 0 = mesh was not hit by the ray.
		/// Thanks to: https://forum.unity.com/threads/editor-raycast-against-scene-meshes-without-collider-editor-select-object-using-gui-coordinate.485502/#post-3162431
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="meshFilter"></param>
		/// <param name="hit"></param>
		/// <returns>Whether or not one or more hits have occurred.</returns>
		public static int IntersectRayMeshMultiple(Ray ray, MeshFilter meshFilter, List<RaycastHit> hits, float rayResumeDistance = 0.1f)
		{
			int hitResult;
			RaycastHit hit;
			do
			{
				hitResult = IntersectRayMesh(ray, meshFilter, out hit);
				if(hitResult == 1)
					hits.Add(hit);
				ray.origin = hit.point + ray.direction * rayResumeDistance;
			}
			while (hitResult == 1);

			return hits.Count;
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
	}
}
#endif