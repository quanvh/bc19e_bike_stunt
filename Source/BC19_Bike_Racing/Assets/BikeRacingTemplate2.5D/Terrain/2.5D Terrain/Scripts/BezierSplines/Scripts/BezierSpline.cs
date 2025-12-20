using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Kamgam.Terrain25DLib.BezierPoint;

namespace Kamgam.Terrain25DLib
{
    /// <summary>
    /// Math is based on the https://catlikecoding.com/unity/tutorials/curves-and-splines/ tutorial.
    /// Since that code was free to use, so is the BezierSpline class.
    /// </summary>
    [Serializable]
    public partial class BezierSpline : MonoBehaviour
    {
        public Action<BezierSpline> OnChanged;

        [System.NonSerialized]
        public bool EditMode = false; // only used in Editor

        public enum BooleanOp
        {
            Add,
            Subtract
        }

        protected int lastPointCount = -1;

        [SerializeField]
        protected bool _mode2D;

        /// <summary>
        /// If true then points and handles will always remain at Z = 0;
        /// </summary>
        public bool Mode2D
        {
            get => _mode2D;
            set
            {
                if (_mode2D == value)
                    return;

                _mode2D = value;
                SetDirty();
            }
        }

        public BooleanOp CombinationType = BooleanOp.Add;

        /// <summary>
        /// Number of segments between each pair of bezier points.
        /// </summary>
        public int Resolution = 10;

        public bool LengthIsDirty { get; private set; }

        /// <summary>
        /// Is the spline closed or open (closed means start and endpoint are connected).
        /// </summary>
        [SerializeField]
        protected bool _closed = true;
        public bool Closed
        {
            get { return _closed; }
            set
            {
                if (_closed == value)
                    return;

                _closed = value;
                SetDirty();
            }
        }

        protected List<BezierPoint> _points;
        public List<BezierPoint> Points
        {
            get
            {
                if (_points == null || _points.Count == 0)
                {
                    _points = this.GetComponentsInChildren<BezierPoint>(includeInactive: false).ToList();
                }
                return _points;
            }
        }

        public void RefreshPoints()
        {
            _points = null;
        }

        public int PointCount
        {
            get { return Points.Count; }
        }

        public bool IsValid
        {
            get => Points != null && Points.Count > 1 && Points[0].Position != Points[1].Position;
        }

        /// <summary>
        /// The approximates length of the spline. The higher the resolution the better (and slower).
        /// </summary>
        private float _length;
        public float Length
        {
            get
            {
                if (LengthIsDirty)
                {
                    _length = 0;
                    for (int i = 0; i < Points.Count - 1; i++)
                    {
                        _length += ApproximateCurveLength(Points[i], Points[i + 1], Resolution);
                    }

                    if (Closed)
                        _length += ApproximateCurveLength(Points[Points.Count - 1], Points[0], Resolution);

                    LengthIsDirty = false;
                }

                return _length;
            }
        }

        /// <summary>
        /// Was the spline set to dirty since the last time the flag was set to false?
        /// </summary>
        [System.NonSerialized]
        public bool WasDirty = false;

        protected SplineController controller;
        public SplineController Controller
        {
            get
            {
                if (controller == null)
                {
                    controller = this.GetComponentInParent<SplineController>();
                }
                return controller;
            }
        }


        void Awake()
        {
            SetDirty();
        }

        void OnTransformChildrenChanged()
        {
            SetDirty();
            RenamePoints();
        }

        public void RenamePoints()
        {
            for (int i = 0; i < PointCount; i++)
            {
                if (Points[i].name.StartsWith("Point"))
                {
                    Points[i].name = "Point " + (i + 1);
                }
            }
        }

        public bool IsFirstPoint(BezierPoint point)
        {
            return Points != null && Points.Count > 0 && Points[0] == point;
        }

        public bool IsLastPoint(BezierPoint point)
        {
            return Points != null && Points.Count > 0 && Points[Points.Count - 1] == point;
        }

        public void AddPoint()
        {
            var newPoint = createNewPoint(Points[PointCount - 1].LocalPosition + new Vector3(1f, 0f, 0f), Vector3.zero);
            Points.Add(newPoint);

            SetDirty();
        }


        public BezierPoint AddPointAt(Vector3 localPosition)
        {
            var newPoint = createNewPoint(localPosition, Vector3.zero);
            SetDirty();

            return newPoint;
        }

        public BezierPoint AddPointAt(Vector3 localPosition, Vector3 euler,
            HandleType _handleType, Vector3 _handle1LocalPos, Vector3 _handle2LocalPos)
        {
            var newPoint = CreateNewPoint(localPosition, euler, _handleType, _handle1LocalPos, _handle2LocalPos);
            SetDirty();

            return newPoint;
        }

        protected BezierPoint CreateNewPoint(Vector3 localPosition, Vector3 euler,
            HandleType _handleType, Vector3 _handle1LocalPos, Vector3 _handle2LocalPos)
        {
            var newPoint = new GameObject("Point " + (PointCount + 1), typeof(BezierPoint)).GetComponent<BezierPoint>();
            newPoint.transform.SetParent(transform);

            newPoint.handleType = _handleType;
            if (_handleType != HandleType.None)
            {
                newPoint.Handle1LocalPos = _handle1LocalPos;
                newPoint.Handle2LocalPos = _handle2LocalPos;
            }

            newPoint.transform.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(euler));
            newPoint.transform.localScale = Vector3.one;

            return newPoint;
        }

        protected BezierPoint createNewPoint(Vector3 localPosition, Vector3 euler, bool arrange = true)
        {
            var position = transform.TransformPoint(localPosition);

            var newPoint = new GameObject("Point " + (PointCount + 1), typeof(BezierPoint)).GetComponent<BezierPoint>();
            newPoint.handleType = BezierPoint.HandleType.None;

            if (arrange && SampledPoints != null && SampledPoints.Length > 0)
            {
                // find the closes point on the sampled curve and inset the new point afterwards
                float minDistance = float.MaxValue;
                int closestIndex = 0;
                for (int i = 0; i < SampledPoints.Length; i++)
                {
                    float sqrDistance = Vector3.SqrMagnitude(SampledPoints[i] - position);
                    if (sqrDistance < minDistance)
                    {
                        minDistance = sqrDistance;
                        closestIndex = i;
                    }
                }
                var info = SampledPointInfos[closestIndex];
                var point = info.Point0;

                // Attempts to create reasonable handles for the new point (TODO: improve to match the curve exactly).
                var pointBefore = point.transform.InverseTransformPoint(SampledPoints[(closestIndex - 1 + SampledPoints.Length) % SampledPoints.Length]);
                var pointAfter = point.transform.InverseTransformPoint(SampledPoints[(closestIndex + 1) % SampledPoints.Length]);
                var handleDirection = (pointAfter - pointBefore).normalized * 4f;

                newPoint.gameObject.transform.parent = this.transform;
                newPoint.transform.SetSiblingIndex(point.transform.GetSiblingIndex() + 1);

                newPoint.Handle1LocalPos = -handleDirection;
                newPoint.Handle2LocalPos = handleDirection;

                newPoint.handleType = info.Point0.handleType;
            }
            else
            {
                newPoint.gameObject.transform.parent = this.transform;
            }

            newPoint.transform.localRotation = Quaternion.Euler(euler);
            newPoint.transform.localPosition = localPosition;
            newPoint.transform.localScale = Vector3.one;

            return newPoint;
        }

        public void RemovePoint(BezierPoint point)
        {
            if (Points.Contains(point))
            {
                Points.Remove(point);
                RefreshPoints();
            }

            SetDirty();
        }

        public void RemoveDeletedPoints()
        {
            for (int i = Points.Count - 1; i >= 0; i--)
            {
                if (Points[i] == null || Points[i].gameObject == null)
                {
                    Points.RemoveAt(i);
                }
            }
        }

        public BezierPoint GetClosestPointToPos(Vector3 position, BezierPoint except = null)
        {
            float minDistance = float.MaxValue;
            BezierPoint minPoint = Points[0];
            foreach (var p in Points)
            {
                float sqrDist = Vector3.SqrMagnitude(p.transform.position - position);
                if (sqrDist < minDistance)
                {
                    minDistance = sqrDist;
                    minPoint = p;
                }
            }

            return minPoint;
        }

        public BezierPoint GetPointAtIndex(int index)
        {
            if (PointCount == 0)
                return null;

            return Points[index];
        }

        /// <summary>
        /// Returns a point at t percent along the spline.
        /// </summary>
        public Vector3 GetPointAt(float t)
        {
            t = Mathf.Clamp01(t);

            // shortcuts
            if (t <= 0f)
            {
                return Points[0].Position;
            }
            else if (t >= 1f)
            {
                if (Closed)
                    return Points[0].Position;
                else
                    return Points[Points.Count - 1].Position;
            }

            if (Points.Count == 2 && !Closed)
            {
                return GetCurvePoint(Points[0], Points[1], t);
            }

            float totalPercentage = 0;
            float curvePercentage = 0;

            BezierPoint p0 = null;
            BezierPoint p1 = null;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                curvePercentage = ApproximateCurveLength(Points[i], Points[i + 1], Resolution) / Length;
                if (totalPercentage + curvePercentage > t)
                {
                    p0 = Points[i];
                    p1 = Points[i + 1];
                    break;
                }
                else
                {
                    totalPercentage += curvePercentage;
                }
            }

            // In case t falls within the last "closing" segment.
            if (Closed && p0 == null)
            {
                p0 = Points[Points.Count - 1];
                p1 = Points[0];
            }

            t -= totalPercentage;

            return GetCurvePoint(p0, p1, t / curvePercentage);
        }

        /// <summary>
        /// Returns the curve (the two BezierPoints neighbouring the position and the local t on the curve) at t percent along the spline.
        /// </summary>
        public (BezierPoint, BezierPoint, float) GetCurveAt(float t)
        {
            t = Mathf.Clamp01(t);

            // shortcuts
            if (Points == null || Points.Count == 0)
                return (null, null, 0f);

            if (PointCount < 2)
                return (Points[0], Points[0], 0f);

            if (t <= 0f)
            {
                return (Points[0], Points[1], 0f);
            }
            else if (t >= 1f)
            {
                if (Closed)
                    return (Points[Points.Count - 1], Points[0], 1f);
                else
                    return (Points[Points.Count - 2], Points[Points.Count - 1], 1f);
            }

            if (Points.Count == 2 && !Closed)
            {
                return (Points[0], Points[1], t);
            }

            float totalPercentage = 0;
            float curvePercentage = 0;

            BezierPoint p0 = null;
            BezierPoint p1 = null;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                curvePercentage = ApproximateCurveLength(Points[i], Points[i + 1], Resolution) / Length;
                if (totalPercentage + curvePercentage > t)
                {
                    p0 = Points[i];
                    p1 = Points[i + 1];
                    break;
                }
                else
                {
                    totalPercentage += curvePercentage;
                }
            }

            // In case t falls within the last "closing" segment.
            if (Closed && p0 == null)
            {
                p0 = Points[Points.Count - 1];
                p1 = Points[0];
            }

            t -= totalPercentage;

            return (p0, p1, t / curvePercentage);
        }

        /// <summary>
        /// Get the index of the given point in this curve
        /// </summary>
        public int GetPointIndex(BezierPoint point)
        {
            int index = Points.IndexOf(point);
            return index;
        }

        /// <summary>
        /// Forces the spline to recalculate the length.
        /// </summary>
        public void SetDirty()
        {
            LengthIsDirty = true;
            RefreshPoints();
            SampleSpline(Resolution);
            WasDirty = true;
        }

        /// <summary>
        /// Gets the point t percent along a curve.
        /// </summary>
        public static Vector3 GetCurvePoint(BezierPoint p0, BezierPoint p1, float t)
        {
            if (p0.Handle2LocalPos != Vector3.zero)
            {
                if (p1.Handle1LocalPos != Vector3.zero)
                    return GetCubicCurvePoint(p0.Position, p0.Handle2Pos, p1.Handle1Pos, p1.Position, t); // all 4 points
                else
                    return GetQuadraticCurvePoint(p0.Position, p0.Handle2Pos, p1.Position, t); // 3 points
            }
            else
            {
                if (p1.Handle1LocalPos != Vector3.zero)
                    return GetQuadraticCurvePoint(p0.Position, p1.Handle1Pos, p1.Position, t); // 3 points
                else
                    return Vector3.Lerp(p0.Position, p1.Position, t); // 2 points
            }
        }

        /// <summary>
        /// Returns the point t percent along a cubic curve.
        /// </summary>
        public static Vector3 GetCubicCurvePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        /// <summary>
        /// Returns the point t percent along a quadratic curve.
        /// </summary>
        public static Vector3 GetQuadraticCurvePoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        /// <summary>
        /// Approximates the length. The higher the resolution the better the approximation (and the slower).
        /// </summary>
        public static float ApproximateCurveLength(BezierPoint p0, BezierPoint p1, int resolution = 10)
        {
            float resolutionAsFloat = resolution;
            float total = 0;
            Vector3 lastPosition = p0.Position;
            Vector3 currentPosition;

            for (int i = 0; i < resolution + 1; i++)
            {
                currentPosition = GetCurvePoint(p0, p1, i / resolutionAsFloat);
                total += (currentPosition - lastPosition).magnitude;
                lastPosition = currentPosition;
            }

            return total;
        }

        /*
		public Vector3 GetDirection(float t)
		{
			return GetVelocity(t).normalized;
		}

		public Vector3 GetVelocity(float t)
		{
			var (p0, p1, tc) = GetCurveAt(t);

			if (p0.Handle2LocalPos != Vector3.zero)
			{
				if (p1.Handle1LocalPos != Vector3.zero)
					return GetFirstDerivative(p0.Position, p0.Handle2Pos, p1.Handle1Pos, p1.Position, t); // all 4 points
				else
					return GetFirstDerivative(p0.Position, p0.Handle2Pos, p1.Position, t); // 3 points
			}
			else
			{
				if (p1.Handle1LocalPos != Vector3.zero)
					return GetFirstDerivative(p0.Position, p1.Handle1Pos, p1.Position, t); // 3 points
				else
					return GetFirstDerivative(p0.Position, p1.Position); // 2 points
			}
		}
		*/

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1)
        {
            return p1 - p0;
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }

        /// <summary>
        /// Uses the hasChanged flags of the transform to detect changes.
        /// </summary>
        /// <returns></returns>
        public bool HasTransformChanged()
        {
            bool changed = false;

            if (transform.hasChanged)
            {
                changed = true;
                transform.hasChanged = false;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).transform;
                if (child.hasChanged)
                {
                    changed = true;
                    child.hasChanged = false;
                }
            }

            return changed;
        }

        public bool HasPointCountChanged()
        {
            int pointCount = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    pointCount++;
            }

            if (lastPointCount != pointCount)
            {
                lastPointCount = pointCount;
                return true;
            }
            return false;
        }

        [Serializable]
        public class PointInfo
        {
            public Vector3 Position;
            public BezierPoint Point0;
            public BezierPoint Point1;
            public float T;

            public PointInfo(Vector3 position, BezierPoint point0, BezierPoint point1, float t)
            {
                Position = position;
                Point0 = point0;
                Point1 = point1;
                T = t;
            }
        }

        protected Vector3[] _sampledPoints;
        protected PointInfo[] _sampledPointInfos;

        /// <summary>
        /// The point coordinates are in WorldSpace.
        /// </summary>
        public Vector3[] SampledPoints { get => _sampledPoints; }
        public PointInfo[] SampledPointInfos { get => _sampledPointInfos; }

        /// <summary>
        /// Calculates the approximated positions along the curve and stores the results in SampledPoints and SampledInfos.
        ///  The coordinates are in WorldSpace.
        ///  If the curve is closed then the first and last point are equal.
        /// </summary>
        public void SampleSpline(int resolutionPerCurve)//, Transform targetSpace = null)
        {
            if (!IsValid)
                return;

            if (Mode2D)
            {
                foreach (var p in Points)
                {
                    var pos = p.transform.localPosition;
                    pos.z = 0f;
                    p.transform.localPosition = pos;
                }
            }

            int totalPoints = (PointCount - (Closed ? 0 : 1)) * resolutionPerCurve;

            if (_sampledPoints == null || _sampledPoints.Length != totalPoints + 1)
            {
                _sampledPoints = new Vector3[totalPoints + 1];
            }

            if (_sampledPointInfos == null || _sampledPointInfos.Length != totalPoints + 1)
            {
                _sampledPointInfos = new PointInfo[totalPoints + 1];
            }

            _sampledPoints[0] = Points[0].Position;
            _sampledPointInfos[0] = new PointInfo(Points[0].Position, Points[0], Points[1], 0f);

            int index = 0;
            for (int i = 0; i < Points.Count - 1; i++)
            {
                SampleCurve(index, Points[i], Points[i + 1], resolutionPerCurve);
                index += resolutionPerCurve;
            }

            if (Closed)
                SampleCurve(index, Points[Points.Count - 1], Points[0], resolutionPerCurve);

            if (Closed)
            {
                _sampledPoints[totalPoints] = _sampledPoints[0]; // to equalize floating point errors
                _sampledPointInfos[totalPoints].T = 1f;
            }

            // convert to another space
            /*
			if (targetSpace != null)
			{
                for (int i = 0; i < _sampledPoints.Length; i++)
                {
					_sampledPoints[i] = targetSpace.InverseTransformPoint(_sampledPoints[i]);
                }
			}*/

            OnChanged?.Invoke(this);
        }

        /// <summary>
        /// Returns -1 if none was found.
        /// </summary>
        /// <param name="posInWorldSpace"></param>
        /// <returns></returns>
        public int GetSampledPointIndexClosestTo(Vector3 posInWorldSpace)
        {
            if (SampledPoints == null || SampledPoints.Length == 0)
                return -1;

            float minDistance = float.MaxValue;
            int minPointIndex = -1;
            for (int i = 0; i < SampledPoints.Length; i++)
            {
                float sqrDist = Vector3.SqrMagnitude(SampledPoints[i] - posInWorldSpace);
                if (sqrDist < minDistance)
                {
                    minDistance = sqrDist;
                    minPointIndex = i;
                }
            }

            return minPointIndex;
        }

        public PointInfo GetSampledPointInfoClosestTo(Vector3 posInWorldSpace)
        {
            int index = GetSampledPointIndexClosestTo(posInWorldSpace);

            if (index < 0)
                return null;

            return SampledPointInfos[index];
        }

        /// <summary>
        /// Calculates the approximated positions along the curve and stores the results in SampledPoints and SampledInfos beginning at startIndex.
        /// <br />The coordinates are in WorldSpace.
        /// </summary>
        public static void SampleCurve(int startIndex, BezierPoint p0, BezierPoint p1, int resolution)
        {
            var spline = p0.Spline;
            float resolutionAsFloat = resolution;

            for (int i = 1; i <= resolution; i++)
            {
                spline.SampledPoints[startIndex + i] = GetCurvePoint(p0, p1, i / resolutionAsFloat);
                spline.SampledPointInfos[startIndex + i] = new PointInfo(spline.SampledPoints[startIndex + i], p0, p1, i / resolutionAsFloat);
            }
            spline.SampledPoints[startIndex + resolution] = p1.Position;
            spline.SampledPointInfos[startIndex + resolution].T = 1f;
        }

        public void ReversePointOrder()
        {
            int half = Mathf.FloorToInt(transform.childCount / 2f);
            for (int i = 0; i < half; i++)
            {
                transform.GetChild(i).SetSiblingIndex(transform.childCount - 1 - i);
                transform.GetChild(transform.childCount - 1 - i - 1).SetSiblingIndex(i);
            }
            SetDirty();
        }
    }
}