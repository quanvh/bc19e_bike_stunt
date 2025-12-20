using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib.Helpers
{
    public static class UtilsPolygons
    {
        /// <summary>
        /// Is the point (p) within the polygon (polyPoints in X/Y plane) and between the given z limits.
        /// The polygon can be (concave or convex).
        /// From https://wiki.unity3d.com/index.php?title=PolyContainsPoint
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="p"></param>
        /// <param name="minZ">Point p is considered outside if below minZ.</param>
        /// <param name="maxZ">Point p is considered outside if above maxZ.</param>
        /// <returns></returns>
        public static bool ContainsPoint(IList<Vector3> polyPoints, Vector3 p, float minZ = float.MinValue, float maxZ = float.MaxValue)
        {
            if (p.z < minZ || p.z > maxZ)
                return false;

            var j = polyPoints.Count - 1;
            var inside = false;
            Vector2 pi, pj;
            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                pi = polyPoints[i];
                pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>
        /// Checks if a point is within the polygon (concave or convex).
        /// From https://wiki.unity3d.com/index.php?title=PolyContainsPoint
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool ContainsPoint(IList<Vector2> polyPoints, Vector2 p)
        {
            var j = polyPoints.Count - 1;
            var inside = false;
            Vector2 pi, pj;
            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                pi = polyPoints[i];
                pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        public static float TriangleArea(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            return 0.5f * (p1 - p0).magnitude * (p2 - p0).magnitude * Mathf.Sin(Mathf.Deg2Rad * Vector2.Angle(p1 - p0, p2 - p0));
        }
    }
}
