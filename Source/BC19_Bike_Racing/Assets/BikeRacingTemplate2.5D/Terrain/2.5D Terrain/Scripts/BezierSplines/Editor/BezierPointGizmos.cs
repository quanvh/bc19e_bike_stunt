using UnityEditor;
using UnityEngine;
using System;

namespace Kamgam.Terrain25DLib
{
	public partial class BezierPointGizmos
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmos(BezierPoint point, GizmoType type)
		{
			if (point.Spline == null || !point.Spline.EditMode)
				return;

			var size = HandleUtility.GetHandleSize(point.transform.position) * 0.1f;
			Gizmos.color = BezierSplineGizmos.GetColor(point.Spline);
			Gizmos.DrawSphere(point.transform.position, size);
		}
	}
}
