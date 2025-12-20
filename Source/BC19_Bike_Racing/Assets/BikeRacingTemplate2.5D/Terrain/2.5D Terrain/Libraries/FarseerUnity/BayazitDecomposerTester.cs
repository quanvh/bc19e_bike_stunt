// Thanks to  Leonardo_Temperanza
// Source: https://answers.unity.com/questions/977416/2d-polygon-convex-decomposition-code.html
// Origianl Source: https://github.com/gabstv/Farseer-Unity3D/blob/master/LICENSE
// License: Microsoft Permissive License (Ms-PL)

using UnityEngine;
using System.Collections.Generic;

// Namespaced this to avoid collision. This does not imply any ownership by kamgam.
namespace Kamgam.Farseer
{
    public class BayazitDecomposerTester : MonoBehaviour
    {
        private PolygonCollider2D col;
        public bool show;

        void Start()
        {
            col = GetComponent<PolygonCollider2D>();

            if (col == null)
            {
                Debug.LogError("There is no 'PolygonCollider2D' attached to the object 'BayazitDecomposerTester' is attached to.");
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !show)
                return;

            Gizmos.color = Color.green;

            List<Vector2> worldColPoints = new List<Vector2>();

            foreach (Vector2 point in col.points)
            {
                Vector2 currentWorldPoint = this.transform.TransformPoint(point);
                worldColPoints.Add(currentWorldPoint);
            }

            List<Vector2> vertices = worldColPoints;

            List<List<Vector2>> listOfConvexPolygonPoints = BayazitDecomposer.ConvexPartition(vertices);

            foreach (List<Vector2> pointsOfIndivualConvexPolygon in listOfConvexPolygonPoints)
            {
                List<Vector2> currentPolygonVertices = pointsOfIndivualConvexPolygon;

                for (int i = 0; i < currentPolygonVertices.Count; i++)
                {
                    Vector2 currentVertex = currentPolygonVertices[i];
                    Vector2 nextVertex = currentPolygonVertices[i + 1 >= currentPolygonVertices.Count ? 0 : i + 1];

                    Gizmos.DrawLine(currentVertex, nextVertex);
                }
            }
        }
    }
}