using UnityEditor;
using UnityEngine;

namespace Kamgam.BridgeBuilder25D
{
	public partial class Bridge25DGizmos
	{
		[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmosNotSelected(Bridge25D builder, GizmoType type)
		{
			drawIcon(builder);
		}

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmosSelected(Bridge25D builder, GizmoType type)
		{
			drawIcon(builder);
		}

		private static void drawIcon(Bridge25D builder)
        {
			var pos = builder.transform.position;
			Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + builder.transform.TransformVector(Vector3.right * 0.1f));
			Gizmos.color = Color.green;
			Gizmos.DrawLine(pos, pos + builder.transform.TransformVector(Vector3.up * 0.1f));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(pos, pos + builder.transform.TransformVector(Vector3.forward * 0.1f));

			if (!BridgeBuilder25DSettings.GetOrCreateSettings().ShowIcon)
				return;

			if (builder.Spline == null || builder.Spline.PointCount == 0)
				return;

			Vector3 positionDelta = Vector3.up;
			pos = (builder.Spline.Points[0].Position + builder.Spline.Points[1].Position) * 0.5f + positionDelta;
			Gizmos.DrawIcon(pos, "BridgeBuilder25D.tiff", true);
		}
    }
}
