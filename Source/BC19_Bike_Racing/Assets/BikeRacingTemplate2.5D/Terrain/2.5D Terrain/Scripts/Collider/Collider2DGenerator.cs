using Kamgam.Terrain25DLib.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public class Collider2DGenerator : MonoBehaviour
    {
        [Tooltip("The layer of the generated collider")]
        public SingleUnityLayer Layer;

        public void GenerateColliders()
        {
            var splineController = transform.parent.GetComponentInChildren<SplineController>(includeInactive: true);
            if (splineController.SplineCount > 0)
            {
                if (!splineController.HasCombinationResult)
                    splineController.CombineAndRememberInfo();

                var shapes = splineController.CombinationResult.Shapes;
                var holes = splineController.CombinationResult.Holes;
                GenerateColliders(shapes, holes);
            }
        }

        public void GenerateColliders(List<Vector3[]> shapesWorldSpace, List<Vector3[]> holesWorldSpace)
        {
            if (shapesWorldSpace == null)
                return;

            var shapesVector2 = new List<Vector2[]>();
            foreach (var polygon in shapesWorldSpace)
            {
                var points = new Vector2[polygon.Length];
                for (int i = 0; i < polygon.Length; i++)
                {
                    points[i] = new Vector2(polygon[i].x, polygon[i].y);
                }
                shapesVector2.Add(points);
            }

            var holesVector2 = new List<Vector2[]>();
            if (holesWorldSpace != null)
            {
                foreach (var polygon in holesWorldSpace)
                {
                    var points = new Vector2[polygon.Length];
                    for (int i = 0; i < polygon.Length; i++)
                    {
                        points[i] = new Vector2(polygon[i].x, polygon[i].y);
                    }
                    holesVector2.Add(points);
                }
            }

            GenerateColliders(shapesVector2, holesVector2);
        }

        public void GenerateColliders(List<Vector2[]> shapesWorldSpace, List<Vector2[]> holesWorldSpace)
        {
            if (shapesWorldSpace == null)
                return;

            var collider = transform.GetComponentInChildren<PolygonCollider2D>();

            // create new collider
            if (collider == null)
            {
                collider = new GameObject("PolygonCollider2D", typeof(PolygonCollider2D)).GetComponent<PolygonCollider2D>();
                collider.transform.parent = transform;
                collider.transform.localScale = Vector3.one;
                collider.transform.localRotation = Quaternion.identity;
                collider.transform.localPosition = Vector3.zero;
            }
            collider.gameObject.layer = Layer.LayerIndex;

            // update collider
            int numOfShapes = shapesWorldSpace.Count;
            int numOfHoles = holesWorldSpace == null ? 0 : holesWorldSpace.Count;
            collider.pathCount = numOfShapes + numOfHoles;
            int pathIndex = 0;

            // shapes
            for (int i = 0; i < shapesWorldSpace.Count; i++)
            {
                var polygon = shapesWorldSpace[i];
                var points = new Vector2[polygon.Length];
                for (int k = 0; k < points.Length; k++)
                {
                    points[k] = (Vector2) transform.InverseTransformPoint(polygon[k]);
                }
                collider.SetPath(pathIndex, points);
                pathIndex++;
            }

            // holes
            if (numOfHoles > 0)
            {
                // points are already reversed (see SplineController.CombineSplines(...))
                for (int i = 0; i < holesWorldSpace.Count; i++)
                {
                    var polygon = holesWorldSpace[i];
                    var points = new Vector2[polygon.Length];
                    for (int k = 0; k < points.Length; k++)
                    {
                        points[k] = (Vector2)transform.InverseTransformPoint(polygon[k]);
                    }
                    collider.SetPath(pathIndex, points);
                    pathIndex++;
                }
            }
        }
    }
}
