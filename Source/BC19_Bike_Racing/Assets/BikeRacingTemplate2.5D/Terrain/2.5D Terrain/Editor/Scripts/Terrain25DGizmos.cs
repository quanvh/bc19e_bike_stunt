using UnityEditor;
using UnityEngine;
using System;

namespace Kamgam.Terrain25DLib
{
	public partial class Terrain25DGizmos
	{
		[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmosNotSelected(Terrain25D terrain, GizmoType type)
		{
			drawIcon(terrain);
		}

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
		static void DrawGizmosSelected(Terrain25D terrain, GizmoType type)
		{
			drawIcon(terrain);
		}

		private static void drawIcon(Terrain25D terrain)
        {
			if (!Terrain25DSettings.GetOrCreateSettings().ShowIcon)
				return;

			// ensure the icon is drawn in front of the mesh
			Vector3 positionDelta = Vector3.zero;
			if (terrain.MeshGenerator)
            {
				positionDelta.z -= terrain.MeshGenerator.FrontMiddleWidth + terrain.MeshGenerator.FrontBevelWidth + 2f;
			}

			Gizmos.DrawIcon(terrain.transform.position + positionDelta, "Terrain25D.tiff", true);
		}
    }
}
