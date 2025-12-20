using Kamgam.Terrain25DLib.Helpers;
using Poly2Tri;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.Terrain25DLib
{
    /// <summary>
    /// One Mesh BEZIER Point Info per spline bezier point.
    /// The in between points are represented by MeshPointInfos.
    /// </summary>
	[RequireComponent(typeof(BezierPoint))]
	public class MeshBezierPointInfo : MonoBehaviour
	{
        protected BezierPoint bezierPoint;
        public BezierPoint BezierPoint
        {
            get
            {
                if (bezierPoint == null)
                {
                    bezierPoint = this.GetComponent<BezierPoint>();
                }
                return bezierPoint;
            }
        }

        public float FrontMiddleMultiplier = 1f;
        public float BackMiddleMultiplier = 1f;

        [Range(0.1f, 30f)]
        public float FrontBevelMultiplier = 1f;

        [Range(0.1f, 30f)]
        public float BackBevelMultiplier = 1f;

        [Range(0f, 10f)]
        public float TopOffset = 0f;

#if UNITY_EDITOR
        public void OnValidate()
        {
            FrontMiddleMultiplier = Mathf.Clamp(FrontMiddleMultiplier, 0.1f, 100000f);
            BackMiddleMultiplier = Mathf.Clamp(BackMiddleMultiplier, 0.1f, 100000f);
        }
#endif
    }
}