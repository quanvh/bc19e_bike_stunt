using Kamgam.CurveShorteningFlow;
using Kamgam.Terrain25DLib.ClipperLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public static class CurveShorteningHelper
    {
		public static List<IntPoint> ShortenClipperCurve(List<IntPoint> path, float delta, float segmentLength)
		{
			var curve = new Curve(ToCurvePoints(path));
			curve = CurveShortening.ShortenCurve(curve, delta, 0.5f, segmentLength);

			var shortenedPath = ToClipperPoints(curve.Data, ClipperHelper.VecToClipFactor);

			// for each point in the path take one new point from the shortened path
			var newPath = new List<IntPoint>();
            foreach (var point in path)
            {
				long minDistance = long.MaxValue;
				IntPoint nearestPoint = new IntPoint(0, 0);
                foreach (var tmpPoint in shortenedPath)
                {
					long distanceSqr = (point.X - tmpPoint.X) * (point.X - tmpPoint.X) + (point.Y - tmpPoint.Y) * (point.Y - tmpPoint.Y);
					if(distanceSqr < minDistance)
                    {
						minDistance = distanceSqr;
						nearestPoint = tmpPoint;
					}
				}

				if (!newPath.Contains(nearestPoint))
				{
					// If the shortened point if outside the path then use the path instead of the nearest point.
					if (Clipper.PointInPolygon(nearestPoint, path) == 0)
					{
						nearestPoint = point;
					}
					newPath.Add(nearestPoint);
				}
				else
                {
					newPath.Add(point);
				}
            }

			return newPath;
		}

		public static List<Point> ToCurvePoints(IList<IntPoint> clipperPath, float precision = ClipperHelper.VecToClipFactor)
		{
			var result = new List<Point>();

			foreach (var p in clipperPath)
			{
				result.Add(new Point(
					p.X / (float)precision,
					p.Y / (float)precision
					));
			}

			return result;
		}

		public static List<IntPoint> ToClipperPoints(IList<Point> curve, float precision = ClipperHelper.VecToClipFactor)
		{
			var result = new List<IntPoint>();

			foreach (var p in curve)
			{
				result.Add(new IntPoint(
					(int)(p.x * (float)precision),
					(int)(p.y * (float)precision)
					));
			}

			return result;
		}
	}
}
