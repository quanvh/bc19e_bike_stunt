using Kamgam.Terrain25DLib.ClipperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public partial class SplineController : MonoBehaviour
    {
        public float Start;
        protected BezierSpline[] _splines;
        public BezierSpline[] Splines
        {
            get
            {
                if (_splines == null)
                {
                    _splines = this.GetComponentsInChildren<BezierSpline>(includeInactive: true);
                }
                return _splines;
            }
        }

        public int SplineCount
        {
            get
            {
                if (Splines == null)
                    return 0;

                return _splines.Length;
            }
        }

        public void RefreshSplines()
        {
            _splines = null;
        }

        [Serializable]
        public class Combination
        {
            public List<Vector3[]> Shapes;
            public List<Vector3[]> Holes;
            public List<BezierSpline.PointInfo[]> Infos;
            public Transform Space;

            public Combination(List<List<Vector3[]>> paths, Transform space, List<BezierSpline.PointInfo[]> infos)
            {
                Shapes = paths[0];
                Holes = paths[1];
                Infos = infos;
                Space = space;
            }

            public List<Vector3[]> GetShapesCopy()
            {
                return CopyPaths(Shapes);
            }

            public List<Vector3[]> GetHolesCopy()
            {
                return CopyPaths(Holes);
            }

            public BezierSpline.PointInfo GetClosestInfo(Vector3 worldPosition)
            {
                BezierSpline.PointInfo result = Infos[0][0];

                foreach (var path in Infos)
                {
                    float minDistance = float.MaxValue;
                    float distance;
                    foreach (var info in path)
                    {
                        distance = (worldPosition - info.Position).sqrMagnitude;
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            result = info;
                        }
                    }
                }

                return result;
            }

            public static List<Vector3[]> CopyPaths(List<Vector3[]> paths)
            {
                var copies = new List<Vector3[]>();
                foreach (var path in paths)
                {
                    var pathCopy = new Vector3[path.Length];
                    Array.Copy(path, pathCopy, path.Length);
                    copies.Add(pathCopy);
                }
                return copies;
            }
        }

        public Combination CombinationResult;

        public bool HasCombinationResult => CombinationResult != null;

        /// <summary>
        /// Combines the Splines and transform all points to target space (assuming localPosition z is 0).<br />
        /// The first and last point are identical (closed shape).<br />
        /// </summary>
        /// <param name="targetSpace">If NULL then the points will be in world space.</param>
        /// <returns>TRUE if a new Combination has been created.</returns>
        public bool CombineAndRememberInfo(Transform targetSpace = null)
        {
            if (SplineCount == 0)
                return false;

            RefreshSplines();
            var solutionsInTargetSpace = CombineSplines(Splines, targetSpace);

            var pointInfos = new List<BezierSpline.PointInfo[]>();
            foreach (var spline in Splines)
            {
                // skip if the spline has no points
                if (!spline.IsValid)
                    continue;

                var infos = new BezierSpline.PointInfo[spline.SampledPointInfos.Length];
                for (int i = 0; i < spline.SampledPointInfos.Length; i++)
                {
                    infos[i] = spline.SampledPointInfos[i];
                }
                pointInfos.Add(infos);
            }

            CombinationResult = new Combination(solutionsInTargetSpace, targetSpace, pointInfos);

            return true;
        }

        /// <summary>
        /// If targetSpace is not NULL then the point positions will be relative to that transform. Otherwise: WorldSpace.
        /// </summary>
        /// <param name="targetSpace"></param>
        /// <returns></returns>
        public List<List<Vector3[]>> CombineSplines(Transform targetSpace = null)
        {
            RefreshSplines();
            return CombineSplines(Splines, targetSpace);
        }

        /// <summary>
        /// If targetSpace is not NULL then the point positions will be relative to that transform. Otherwise: WorldSpace.<br />
        /// It is assumed the points in splines are in WorldSpace corrdinates.<br />
        /// 1) Spline points will be converted to targetSpace space (or not converted at all if targetSpace is NULL).<br />
        /// 2) The Z coordinate of all points is set to 0. It is assumed they have already been 0 in regard to targetSpace.<br />
        /// 3) Clipper will resolve the boolean operations in 2D space (x,y).<br />
        /// 4) Points lists are then returned (one list per resulting polygon). Z will be 0, X and Y are in targetSpace (or WorldSpace if NULL).
        /// </summary>
        /// <param name="splines">Points in WorldSpace</param>
        /// <param name="targetSpace">Spline points will be converted to this space, then z will be set to 0</param>
        /// <returns>A list of lists. The first list are the shapes. The second list are the holes.</returns>
        public List<List<Vector3[]>> CombineSplines(IList<BezierSpline> splines, Transform targetSpace = null)
        {
            int numOfAdditiveSplines = splines.Count(s => s.CombinationType == BezierSpline.BooleanOp.Add);
            int numOfSubtractiveSplines = splines.Count(s => s.CombinationType == BezierSpline.BooleanOp.Subtract);

            if (numOfAdditiveSplines == 0)
            {
                Debug.LogWarning("No splines to combine. Aborting combine operation.");
                return new List<List<Vector3[]>>() { null, null };
            }

            Paths subjects = new Paths(numOfAdditiveSplines);
            Paths clips = new Paths(numOfSubtractiveSplines);
            for (int i = 0; i < splines.Count; i++)
            {
                if (!splines[i].IsValid)
                    continue;

                var spline = splines[i];
                Paths paths = null;
                if (spline.CombinationType == BezierSpline.BooleanOp.Add)
                {
                    paths = subjects;
                }
                else if (spline.CombinationType == BezierSpline.BooleanOp.Subtract)
                {
                    paths = clips;
                }

                if (paths != null)
                {
                    // Clipper works in 2D, thus the z coordinate is dropped. It assumes all points have had z = 0 in local space.
                    var newPath = new Path(spline.SampledPoints.Length);
                    for (int p = 0; p < spline.SampledPoints.Length; p++)
                    {
                        var point = spline.SampledPoints[p];

                        // transform from world space to local space
                        point = transform.InverseTransformPoint(point);

                        newPath.Add(
                            new IntPoint(
                                (int)(point.x * 1000f),
                                (int)(point.y * 1000f)
                            )
                        );
                    }

                    if (splines[i].CombinationType == BezierSpline.BooleanOp.Subtract)
                        newPath.Reverse();

                    paths.Add(newPath);
                }
            }

            Clipper c = new Clipper();

            // merge subjects
            Paths mergedSubjects = new Paths();
            c.Clear();
            c.AddPaths(subjects, PolyType.ptSubject, closed: true);
            c.Execute(
                ClipType.ctUnion, mergedSubjects,
                PolyFillType.pftNonZero, PolyFillType.pftNonZero
                );

            mergedSubjects = Clipper.SimplifyPolygons(mergedSubjects);

            // merge clips
            Paths mergedClips = new Paths();
            c.Clear();
            c.AddPaths(clips, PolyType.ptSubject, closed: true);
            c.Execute(
                ClipType.ctUnion, mergedClips,
                PolyFillType.pftNonZero, PolyFillType.pftNonZero
                );

            Paths solutions = new Paths();
            c.Clear();
            c.AddPaths(mergedSubjects, PolyType.ptSubject, closed: true);
            c.AddPaths(mergedClips, PolyType.ptClip, closed: true);
            c.Execute(
                ClipType.ctDifference, solutions,
                PolyFillType.pftNonZero, PolyFillType.pftEvenOdd
                );

            // Find holes and put them in a separate list.
            var shapes = new List<List<IntPoint>>();
            var holes = new List<List<IntPoint>>();

            for (int i = 0; i < solutions.Count; i++)
            {
                bool isHole = false;
                for (int j = 0; j < solutions.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (Clipper.PointInPolygon(solutions[i][0], solutions[j]) != 0)
                    {
                        isHole = true;
                        break;
                    }
                }

                if (isHole)
                    holes.Add(solutions[i]);
                else
                    shapes.Add(solutions[i]);
            }

            // convert IntPoints to Vector3 and also transform points to target space.
            var shapesVector3 = convertClipperResultToVector3(shapes, targetSpace);
            var holesVector3 = convertClipperResultToVector3(holes, targetSpace);


            // D E B U G

            // Draw clips in red for debugging purposes.
            /*
			foreach (Path solution in clips)
			{
				for (int i = 1; i < solution.Count; i++)
				{
					var p0 = new Vector3(solution[i - 1].X / 1000f, solution[i - 1].Y / 1000f, 10f);
					var p1 = new Vector3(solution[i].X / 1000f, solution[i].Y / 1000f, 10f);
					Debug.DrawLine(p0, p1, Color.red, 10f);
				}
			}
			//*/

            // Draw results in blue for debugging purposes.
            /*
			var color = Color.blue;
			float z = 0f;
			foreach (Path solution in solutions)
            {
                for (int i = 1; i < solution.Count; i++)
                {
					var p0 = new Vector3(solution[i - 1].X / 1000f, solution[i - 1].Y / 1000f, z);
					var p1 = new Vector3(solution[i    ].X / 1000f, solution[i    ].Y / 1000f, z);
					Debug.DrawLine(p0, p1, color, 10f);
				}
				color.r = color.r + 0.2f;
				color.g = color.r + 0.2f;
				color.b = 1f;
				z += 1.0f;
			}
			//*/

            return new List<List<Vector3[]>>() { shapesVector3, holesVector3 };
        }

        protected List<Vector3[]> convertClipperResultToVector3(Paths solutions, Transform targetSpace = null)
        {
            var result = new List<Vector3[]>();

            float z = 0f; // It is assumed that the localPosition.z of the points was 0, thus is still remains 0.
            foreach (List<ClipperLib.IntPoint> solution in solutions)
            {
                var solutionPoints = new Vector3[solution.Count + 1];
                for (int i = 0; i < solution.Count; i++)
                {
                    var point = new Vector3(solution[i].X / 1000f, solution[i].Y / 1000f, z);
                    var pointInTargetSpace = transform.TransformPoint(point); // world point
                    if (targetSpace != null)
                        pointInTargetSpace = targetSpace.InverseTransformPoint(pointInTargetSpace); // world to target space

                    solutionPoints[i] = pointInTargetSpace;
                }
                // clipper does not duplicate the first points as the last, thus we have to add it manually (all polygons are considered closed from here on).
                solutionPoints[solution.Count] = solutionPoints[0];

                result.Add(solutionPoints);
            }

            return result;
        }

        public BezierSpline AddCurve(Vector3 worldPosition, BezierSpline.BooleanOp combinationType = BezierSpline.BooleanOp.Add,
            List<SplinePoint> _lstPoint = null, bool Mode2D = false)
        {
            int splineNr = transform.childCount + 1;
            var spline = new GameObject("BezierSpline " + splineNr, typeof(BezierSpline)).GetComponent<BezierSpline>();
            spline.Mode2D = Mode2D;
            spline.transform.parent = transform;
            spline.transform.localScale = Vector3.one;
            spline.transform.localRotation = Quaternion.identity;
            spline.transform.position = worldPosition;

            if (_lstPoint == null)
            {
                spline.AddPointAt(new Vector3(10f, 10f, 0f));
                spline.AddPointAt(new Vector3(-10f, 10f, 0f));
                spline.AddPointAt(new Vector3(-10f, -10f, 0f));
                spline.AddPointAt(new Vector3(10f, -10f, 0f));
            }
            else
            {
                _lstPoint.OrderBy(t => t.ID);
                foreach (var point in _lstPoint)
                {
                    spline.AddPointAt(point.Position, point.Rotation,
                        point.HandleType, point.Handle1Pos, point.Handle2Pos);
                }
            }

            spline.CombinationType = combinationType;

            _splines = null;

            spline.SetDirty();
            return spline;
        }


        public void SaveShape(TerrainLevelData data)
        {
            CombineAndRememberInfo();
            data.Shapes.Clear();
            foreach (BezierSpline spline in _splines)
            {
                var points = new List<SplinePoint>();
                int i = 0;
                foreach (var point in spline.Points)
                {
                    i++;
                    var newPoint = new SplinePoint();
                    newPoint.ID = i;
                    newPoint.Position = point.LocalPosition;
                    newPoint.Rotation = point.transform.localEulerAngles;
                    newPoint.HandleType = point.handleType;
                    newPoint.Handle1Pos = point.Handle1LocalPos;
                    newPoint.Handle2Pos = point.Handle2LocalPos;

                    points.Add(newPoint);
                }


                var newShape = new SplineShape();
                newShape.Mode2D = spline.Mode2D;
                newShape.CombinationType = spline.CombinationType;
                newShape.Points = points;
                newShape.Position = spline.transform.localPosition;
                newShape.Rotation = spline.transform.localEulerAngles;
                data.Shapes.Add(newShape);
            }
        }
    }
}