using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
	/// <summary>
    /// One MeshPointInfo per point in the spline (BezierSpline.PointInfo);
    /// </summary>
	public class MeshPointInfo
	{
        public MeshBezierPointInfo MeshInfo0;
        public MeshBezierPointInfo MeshInfo1;
        public BezierSpline.PointInfo BezierInfo;

        public MeshPointInfo(MeshBezierPointInfo meshInfo0, MeshBezierPointInfo meshInfo1, BezierSpline.PointInfo bezierInfo)
        {
            MeshInfo0 = meshInfo0;
            MeshInfo1 = meshInfo1;
            BezierInfo = bezierInfo;
        }

        public static MeshPointInfo FindClosest(List<MeshPointInfo> infos, Vector3 worldPosition)
        {
            int numOfInfos = infos.Count;
            float minDistance = float.MaxValue;
            MeshPointInfo result = null;

            for (int i = 0; i < numOfInfos; i++)
            {
                float distance = Vector3.SqrMagnitude(worldPosition - infos[i].BezierInfo.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = infos[i];
                }
            }

            return result;
        }

        public static float CalculateTopOffset(MeshPointInfo a)
        {
            if (a == null)
                return 0f;

            if (a.MeshInfo0 == null && a.MeshInfo1 == null)
                return 0f;

            if (a.MeshInfo0 == null)
                return Mathf.Lerp(0f, a.MeshInfo1.TopOffset, a.BezierInfo.T);

            if (a.MeshInfo1 == null)
                return Mathf.Lerp(a.MeshInfo0.TopOffset, 0f, a.BezierInfo.T);

            return Mathf.Lerp(a.MeshInfo0.TopOffset, a.MeshInfo1.TopOffset, a.BezierInfo.T);
        }

        public static float CalculateFrontBevelMultiplier(MeshPointInfo a)
        {
            if (a == null)
                return 1f;

            if (a.MeshInfo0 == null && a.MeshInfo1 == null)
                return 1f;

            if (a.MeshInfo0 == null)
                return Mathf.Lerp(1f, a.MeshInfo1.FrontBevelMultiplier, a.BezierInfo.T);

            if (a.MeshInfo1 == null)
                return Mathf.Lerp(a.MeshInfo0.FrontBevelMultiplier, 1f, a.BezierInfo.T);

            return Mathf.Lerp(a.MeshInfo0.FrontBevelMultiplier, a.MeshInfo1.FrontBevelMultiplier, a.BezierInfo.T);
        }

        public static float CalculateBackBevelMultiplier(MeshPointInfo a)
        {
            if (a == null)
                return 1f;

            if (a.MeshInfo0 == null && a.MeshInfo1 == null)
                return 1f;

            if (a.MeshInfo0 == null)
                return Mathf.Lerp(1f, a.MeshInfo1.BackBevelMultiplier, a.BezierInfo.T);

            if (a.MeshInfo1 == null)
                return Mathf.Lerp(a.MeshInfo0.BackBevelMultiplier, 1f, a.BezierInfo.T);

            return Mathf.Lerp(a.MeshInfo0.BackBevelMultiplier, a.MeshInfo1.BackBevelMultiplier, a.BezierInfo.T);
        }

        public static float CalculateFrontMiddleMultiplier(MeshPointInfo a)
        {
            if (a == null)
                return 1f;

            if (a.MeshInfo0 == null && a.MeshInfo1 == null)
                return 1f;

            if (a.MeshInfo0 == null)
                return Mathf.Lerp(1f, a.MeshInfo1.FrontMiddleMultiplier, a.BezierInfo.T);

            if (a.MeshInfo1 == null)
                return Mathf.Lerp(a.MeshInfo0.FrontMiddleMultiplier, 1f, a.BezierInfo.T);

            return Mathf.Lerp(a.MeshInfo0.FrontMiddleMultiplier, a.MeshInfo1.FrontMiddleMultiplier, a.BezierInfo.T);
        }

        public static float CalculateBackMiddleMultiplier(MeshPointInfo a)
        {
            if (a == null)
                return 1f;

            if (a.MeshInfo0 == null && a.MeshInfo1 == null)
                return 1f;

            if (a.MeshInfo0 == null)
                return Mathf.Lerp(1f, a.MeshInfo1.BackMiddleMultiplier, a.BezierInfo.T);

            if (a.MeshInfo1 == null)
                return Mathf.Lerp(a.MeshInfo0.BackMiddleMultiplier, 1f, a.BezierInfo.T);

            return Mathf.Lerp(a.MeshInfo0.BackMiddleMultiplier, a.MeshInfo1.BackMiddleMultiplier, a.BezierInfo.T);
        }
    }
}