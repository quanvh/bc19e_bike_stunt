using System;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [ExecuteInEditMode]
    [Serializable]
    public partial class BezierPoint : MonoBehaviour
    {
        /// <summary>
        /// Mirrored: The handles are mirrored.
        /// Broken: Each handle moves independently.
        /// None: This point has no handles (both handles are located ON the point).
        /// </summary>
        public enum HandleType
        {
            Mirrored,
            Broken,
            None
        }

        [SerializeField]
        protected HandleType _handleType;
        public HandleType handleType
        {
            get { return _handleType; }
            set
            {
                if (_handleType == value)
                    return;

                _handleType = value;

                if (handleType == HandleType.Mirrored)
                {
                    setHandle1LocalPos(Handle1LocalPos);
                    setHandle2LocalPos(Handle2LocalPos);
                }
            }
        }

        protected BezierSpline _spline;
        public BezierSpline Spline
        {
            get
            {
                if (_spline == null)
                {
                    _spline = transform?.parent?.GetComponent<BezierSpline>();
                }
                return _spline;
            }
        }

        public bool IsFirst
        {
            get { return Spline != null && Spline.IsFirstPoint(this); }
        }

        public bool IsLast
        {
            get { return Spline != null && Spline.IsLastPoint(this); }
        }

        /// <summary>
        /// The world position of that point.
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set
            {
                transform.position = value;

                if (Spline.Mode2D)
                {
                    var localPos = transform.localPosition;
                    localPos.z = 0;
                    transform.localPosition = localPos;
                }
            }
        }

        public Vector3 LocalPosition
        {
            get { return transform.localPosition; }
            set
            {
                if (Spline.Mode2D)
                    value.z = 0;

                transform.localPosition = value;
            }
        }

        [SerializeField]
        private Vector3 _handle1LocalPos;
        public Vector3 Handle1LocalPos
        {
            get
            {
                if (handleType == HandleType.None)
                    return Vector3.zero;
                else
                    return _handle1LocalPos;
            }

            set
            {
                if (_handle1LocalPos == value)
                    return;

                setHandle1LocalPos(value);
            }
        }

        protected void setHandle1LocalPos(Vector3 value)
        {
            _handle1LocalPos = value;
            if (Spline.Mode2D)
                _handle1LocalPos.z = 0;

            if (handleType == HandleType.Mirrored)
                _handle2LocalPos = -_handle1LocalPos;

            Spline.SetDirty();
        }

        public Vector3 Handle1Pos
        {
            get
            {
                if (handleType == HandleType.None)
                    return Position;
                else
                    return transform.TransformPoint(Handle1LocalPos);
            }

            set
            {
                Handle1LocalPos = transform.InverseTransformPoint(value);
            }
        }

        [SerializeField]
        private Vector3 _handle2LocalPos;
        public Vector3 Handle2LocalPos
        {
            get
            {
                if (handleType == HandleType.None)
                    return Vector3.zero;
                else
                    return _handle2LocalPos;
            }

            set
            {
                if (_handle2LocalPos == value)
                    return;

                setHandle2LocalPos(value);
            }
        }

        protected void setHandle2LocalPos(Vector3 value)
        {
            _handle2LocalPos = value;

            if (Spline.Mode2D)
                _handle2LocalPos.z = 0;

            if (handleType == HandleType.Mirrored)
                _handle1LocalPos = -_handle2LocalPos;

            Spline.SetDirty();
        }

        public Vector3 Handle2Pos
        {
            get
            {
                if (handleType == HandleType.None)
                    return Position;
                else
                    return transform.TransformPoint(Handle2LocalPos);
            }

            set
            {
                Handle2LocalPos = transform.InverseTransformPoint(value);
            }
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            Spline?.SetDirty();
        }

        private void OnDisable()
        {
            Spline?.SetDirty();
        }

        private void OnDestroy()
        {
            if (Spline != null && Spline.PointCount > 0)
                UnityEditor.Selection.activeGameObject = Spline.GetClosestPointToPos(Position, this).gameObject;
        }
#endif
    }
}
