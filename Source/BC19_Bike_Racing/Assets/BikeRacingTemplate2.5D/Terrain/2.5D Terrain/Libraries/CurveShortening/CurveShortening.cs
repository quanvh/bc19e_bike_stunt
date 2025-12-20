// Copyright (c) 2014-2018 Anthony Carapetis
// This software is licensed under the MIT license.
// See LICENSE for more details.
// C# Version: 2020 KAMGAM e.U.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kamgam.CurveShorteningFlow
{
    public static class CurveShortening
    {
        public static void renderClosedCurve( Curve curve, Action<Point,Point> funcDrawLine)
        {
            for (int i = 0; i < curve.Count; i++)
            {
                funcDrawLine(curve.Get(i), curve.Get(i + 1));
            }
        }

        /// <summary>
        /// Returns a new shortened curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="delta">Has to be greater than zero.</param>
        /// <param name="deltaStepSize">If delta is greater than deltaStepSize, then multiple shortenings will be performed.</param>
        /// <param name="segmentLength"></param>
        /// <returns></returns>
        public static Curve ShortenCurve(Curve curve, float delta, float deltaStepSize, float segmentLength)
        {
            if (delta <= deltaStepSize)
            {
                return ShortenCurve(curve, delta, segmentLength);
            }
            else
            {
                Curve shortenedCurve = curve;
                do
                {
                    shortenedCurve = ShortenCurve(shortenedCurve, deltaStepSize, segmentLength);
                    delta -= deltaStepSize;
                }
                while (delta > deltaStepSize);
                shortenedCurve = ShortenCurve(shortenedCurve, delta, segmentLength);
                return shortenedCurve;
            }
        }

        /// <summary>
        /// Returns a new shortened curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="delta">Has to be greater than zero.</param>
        /// <param name="segmentLength"></param>
        /// <returns></returns>
        public static Curve ShortenCurve(Curve curve, float delta, float segmentLength)
        {
            // Don't shorten if it is too small or curvature is too extreme
            if (delta > 0 && curve.Count > 4 && curve.Curvature().Max() < 5000)
            {
                // shorten
                var shortenedCurve = curve.Map<Point, Curve>(Flow.ReparametrizedCSF(delta / curve.Curvature().Max()));

                // clean
                Flow.Remesh(shortenedCurve, segmentLength);
                Flow.Clean(shortenedCurve);

                return shortenedCurve;
            }
            else
            {
                return curve;
            }
        }

        public static Curve DemoCurve(float width, float height)
        {
            // Generate an interesting demo curve to start.
            var numOfPoints = 200;
            List<Point> points = new List<Point>();
            for (var i = 0; i < numOfPoints; i++)
            {
                var x = Convert.ToSingle(width / 2f + width * (0.05f * Math.Cos(2 * Math.PI * i / (float)numOfPoints)));
                points.Add(
                    new Point(
                        Convert.ToSingle(x + 0.2f * width * Math.Pow(Math.Cos(2 * Math.PI * i / (float)numOfPoints), 101)),
                        Convert.ToSingle(height * (0.15f + 0.05f * Math.Sin(2 * Math.PI * i / (float)numOfPoints) + 0.05f * Math.Sin(x / 5f) + 0.7f * Math.Pow(Math.Sin(2 * Math.PI * i / (float)numOfPoints), 150)))
                    )
                );
            }

            return new Curve(points);
        }

        public static Curve SimpleDemoCurve(float width, float height)
        {
            List<Point> points = new List<Point>();
            points.Add(new Point(0, 0));
            points.Add(new Point(width, 0));
            points.Add(new Point(width * 0.8f, height * 0.2f));
            points.Add(new Point(width, height));
            points.Add(new Point(0, height));
            points.Add(new Point(width * 0.5f, height * 0.5f));
            points.Add(new Point(0, 0));

            return new Curve(points);
        }
    }
}
