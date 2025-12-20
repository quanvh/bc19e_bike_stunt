using UnityEditor;
using UnityEngine;
using System;

namespace Kamgam.BridgeBuilder25D
{
	public partial class BezierPointGizmos
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmos(BezierPoint point, GizmoType type)
		{
			if (point.Spline == null || !point.Spline.EditMode)
					return;

			// Don't draw if point is selected and MOVE tools is active.
			if (Selection.Contains(point.gameObject) && Tools.current == Tool.Move)
				return;

			var size = HandleUtility.GetHandleSize(point.transform.position) * 0.1f;
			Gizmos.color = BezierSplineGizmos.GetColor(point.Spline);
			Gizmos.DrawSphere(point.transform.position, size);
		}
	}
}
