using UnityEditor;
using UnityEngine;
using static Kamgam.Terrain25DLib.BezierSpline;

namespace Kamgam.Terrain25DLib
{
	public class BezierSplineGizmos
	{
		public static Color GetColor(BezierSpline spline)
        {
            switch (spline.CombinationType)
            {
                case BooleanOp.Add:
					return Terrain25DSettings.GetOrCreateSettings().AddColor;
                case BooleanOp.Subtract:
					return Terrain25DSettings.GetOrCreateSettings().SubtractColor;
				default:
					return Terrain25DSettings.GetOrCreateSettings().AddColor;
			}
        }

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmos(BezierSpline spline, GizmoType type)
		{
			Gizmos.color = GetColor(spline);

			if (!spline.EditMode)
				return;

			if (spline.HasPointCountChanged() || spline.HasTransformChanged())
			{
				spline.SetDirty();
			}

			// curve
			if (spline.Points.Count > 1)
			{
				if (spline.SampledPoints == null)
					spline.SampleSpline(spline.Resolution);

				DrawSampledSpline(spline);
			}
		}

		protected static Vector3[] tmpCurvePoints;

        /// <summary>
        /// Draws a curve in the Editor using Gizmo.DrawLine() && Handles.DrawAAPolyLine.
        /// </summary>
        public static void DrawSampledSpline(BezierSpline spline)
		{
			var previousPoint = spline.SampledPoints[0];
			for (int i = 1; i < spline.SampledPoints.Length; i++)
			{
				var currentPoint = spline.SampledPoints[i];
				Gizmos.DrawLine(previousPoint, currentPoint);
				previousPoint = currentPoint;
			}

			if (IsSelected(spline))
			{
				Handles.color = GetColor(spline);
				Handles.DrawAAPolyLine(3.5f, spline.SampledPoints);
			}
		}

		public static bool IsSelected(BezierSpline spline)
        {
			if (Selection.Contains(spline.gameObject))
				return true;

            for (int i = 0; i < spline.PointCount; i++)
            {
				if (Selection.Contains(spline.Points[i].gameObject))
					return true;
            }

			return false;
		}
	}
}