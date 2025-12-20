using UnityEditor;
using UnityEngine;
using System;

namespace Kamgam.Terrain25DLib
{
	public partial class MeshVertex: MonoBehaviour
	{
#if UNITY_EDITOR
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
		private static void DrawGizmo(MeshVertex source, GizmoType type)
		{
			if (Selection.Contains(source.transform.parent.parent.gameObject))
				return;

			var size = HandleUtility.GetHandleSize(source.transform.position) * 0.2f;
			var color = Color.black;
			color.a = 0.7f;
			Gizmos.color = color;
			Gizmos.DrawSphere(source.transform.position, size);
		}
#endif
	}
}
