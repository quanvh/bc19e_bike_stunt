using UnityEngine;
using System.Collections.Generic;
using Kamgam.Terrain25DLib.ClipperLib;

namespace Kamgam.Terrain25DLib
{
    public static class ClipperHelper
    {
        public const float VecToClipFactor = 10000f;

        public static List<List<IntPoint>> ToClipperPointLists(IList<Vector3[]> vectorLists, float precision = VecToClipFactor)
        {
            var result = new List<List<IntPoint>>();

            foreach (var vectors in vectorLists)
            {
                result.Add(ToClipperPointList(vectors, precision));
            }

            return result;
        }

        public static List<IntPoint> ToClipperPointList(IList<Vector3> vectors, float precision = VecToClipFactor)
        {
            var result = new List<IntPoint>();

            for (int i = 0; i < vectors.Count; i++)
            {
                if (i < vectors.Count - 1 || vectors[i] != vectors[0]) // skip last if identical to first
                {
                    result.Add(
                        new IntPoint(
                            vectors[i].x * precision,
                            vectors[i].y * precision
                        )
                    );
                }
            }

            return result;
        }

        public static List<List<IntPoint>> ToClipperPointLists(IList<Vector2[]> vectorLists, float precision = VecToClipFactor)
        {
            var result = new List<List<IntPoint>>();

            foreach (var vectors in vectorLists)
            {
                result.Add(ToClipperPointList(vectors, precision));
            }

            return result;
        }

        public static List<IntPoint> ToClipperPointList(IList<Vector2> vectors, float precision = VecToClipFactor)
        {
            var result = new List<IntPoint>();

            for (int i = 0; i < vectors.Count; i++)
            {
                result.Add(
                    new IntPoint(
                        vectors[i].x * precision,
                        vectors[i].y * precision
                    )
                );
            }

            return result;
        }

        public static List<Vector2> ToVector2List(IList<IntPoint> points, float precision = VecToClipFactor)
        {
            var result = new List<Vector2>();

            for (int i = 0; i < points.Count; i++)
            {
                result.Add(
                    new Vector2(
                        points[i].X / precision,
                        points[i].Y / precision
                    )
                );
            }

            return result;
        }

        public static List<Vector3> ToVector3List(IList<IntPoint> points, float precision = VecToClipFactor, float zValue = 0f)
        {
            var result = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                result.Add(
                    new Vector3(
                        points[i].X / precision,
                        points[i].Y / precision,
                        zValue
                    )
                );
            }

            return result;
        }

        public static Vector3 ToVector3(IntPoint point, float precision = VecToClipFactor, float zValue = 0f)
        {
            return new Vector3(
                point.X / precision,
                point.Y / precision,
                zValue
            );
        }

        public static Vector2[] ToVector2Array(IList<IntPoint> points, float precision = VecToClipFactor)
        {
            var result = new Vector2[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                result[i] =
                    new Vector2(
                        points[i].X / precision,
                        points[i].Y / precision
                    );
            }

            return result;
        }

        public static List<Vector3[]> ToVector3Arrays(IList<List<IntPoint>> pointLists, float precision = VecToClipFactor, float zValue = 0f)
        {
            var result = new List<Vector3[]>();

            for (int i = 0; i < pointLists.Count; i++)
            {
                result.Add(ToVector3Array(pointLists[i], precision, zValue));
            }

            return result;
        }

        public static Vector3[] ToVector3Array(IList<IntPoint> points, float precision = VecToClipFactor, float zValue = 0f)
        {
            var result = new Vector3[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                result[i] =
                    new Vector3(
                        points[i].X / precision,
                        points[i].Y / precision,
                        zValue
                    );
            }

            return result;
        }

        /// <summary>
        /// Returns the results as a list.<br />
        /// Index 0 are the shapes, Index 1 are the holes.<br />
        /// Results must not be self intersecting.
        /// </summary>
        /// <param name="solutions"></param>
        /// <returns></returns>
        public static List<List<List<IntPoint>>> SplitIntoShapesAndHoles(List<List<IntPoint>> solutions)
        {
            var shapes = new List<List<IntPoint>>();
            var holes = new List<List<IntPoint>>();

            foreach (var solution in solutions)
            {
                if (Clipper.Orientation(solution))
                    shapes.Add(solution);
                else
                    holes.Add(solution);
            }

            return new List<List<List<IntPoint>>> { shapes, holes };
        }

        /// <summary>
        /// Returns true if the point is inside or touching one or more polygons.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygons"></param>
        /// <returns></returns>
        public static bool PointInPolygons(IntPoint point, List<List<IntPoint>> polygons)
        {
            foreach (var path in polygons)
            {
                if (Clipper.PointInPolygon(point, path) != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all points which are more than "distance" appart.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static void MergeCurvePoints(List<IntPoint> points, int distance)
        {
            int d2 = distance * distance;
            long d;
            for (int a = points.Count - 1; a >= 0; a--)
            {
                if (a > 0)
                {
                    for (int b = a - 1; b >= 0; b--)
                    {
                        d = (points[a].X - points[b].X) * (points[a].X - points[b].X) + (points[a].Y - points[b].Y) * (points[a].Y - points[b].Y);
                        if (d < d2)
                        {
                            points.RemoveAt(a);
                            break;
                        }
                    }
                }
            }
        }
    }
}