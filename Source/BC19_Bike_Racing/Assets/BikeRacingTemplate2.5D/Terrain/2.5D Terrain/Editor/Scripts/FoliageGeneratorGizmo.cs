using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public class FoliageGeneratorGizmo
    {
        [DrawGizmo(GizmoType.InSelectionHierarchy)/* | GizmoType.Pickable)*/]
        static void DrawGizmos(FoliageGenerator generator, GizmoType gizmoType)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == false
                && generator.MeshRenderers != null
                && generator.MeshRenderers.Length > 0
                && generator.GeneratorSets.Length > 0)
            {
                for (int i = -1; i < generator.GeneratorSets.Length; i++)
                {
                    // first draw the generator, then the sets
                    var set = i >= 0 ? generator.GeneratorSets[i] : null;

                    if (set != null && !set.Enabled)
                        continue;
                
                    var bounds = generator.GetBounds();
                    float minWorldY = bounds.min.y;
                    float maxWorldY = bounds.max.y;

                    float margin = (set != null && set.Settings != null) ? set.Settings.Margin : 0f;
                    float start = (set != null) ? set.Start : generator.Start;
                    float end = (set != null) ? set.End : generator.End;

                    (float minX, float maxX, _, _) = FoliageGeneratorSet.CalculateMinMaxXZ(generator, margin, inLocalSpace: true);
                    float distanceX = maxX - minX;
                    float finalMinX = minX + distanceX * (start / 100f);
                    float finalMaxX = minX + distanceX * (end / 100f);

                    var minWorld = generator.transform.TransformPoint(new Vector3(finalMinX, 0, 0));
                    minWorld.y = maxWorldY + 1f + generator.RayStartOffsetY - i * 0.2f;
                    var maxWorld = generator.transform.TransformPoint(new Vector3(finalMaxX, 0, 0));
                    maxWorld.y = maxWorldY + 1f + generator.RayStartOffsetY - i * 0.2f;
                    var direction = maxWorld - minWorld;
                    
                    var normal = new Vector3(direction.y, -direction.x, direction.z).normalized * (maxWorldY - minWorldY);
                    Gizmos.color = set == null ? Color.green : new Color((i+1) * 0.3f, 1f, 0f);
                    Gizmos.DrawRay(minWorld, direction);
                    Gizmos.DrawRay(minWorld, normal);
                    Gizmos.DrawRay(maxWorld, normal);
                }
            }
        }
    }
}
