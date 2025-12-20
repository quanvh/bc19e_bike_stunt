// Copyright (c) 2014-2018 Anthony Carapetis
// This software is licensed under the MIT license.
// See LICENSE for more details.
// C# Version: 2020 KAMGAM e.U.

using System.Collections.Generic;
using System.Linq;
using System;

namespace Kamgam.CurveShorteningFlow
{
    public struct Point
    {
        public float x;
        public float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class ScalarFunction : CircularList<float>
    {
        public ScalarFunction(List<float> data) : base(data)
        {
        }

        public float Max()
        {
            return _data.Max();
        }

        public float Min()
        {
            return _data.Min();
        }
    }

    public class Curve : CircularList<Point>
    {
        public Curve() : base()
        {
        }

        public Curve(List<Point> data) : base(data)
        {
        }

        public ScalarFunction Curvature()
        {
            return new ScalarFunction(
                this.Neighbourhoods().Select((nbhd) => Geometry.Curvature(nbhd)).ToList()
            );
        }

        /// <summary>
        /// Computes (abs value of signed) area via the shoelace formula
        /// </summary>
        /// <returns></returns>
        public float Area()
        {
            var n = this.Count;

            float sum0 = 0;
            for (var i = 0; i < n; ++i)
            {
                sum0 += this.Get(i).x * this.Get(i + 1).y;
            }

            float sum1 = 0;
            for (var i = 0; i < n; ++i)
            {
                sum1 += this.Get(i + 1).x * this.Get(i).y;
            }

            return Math.Abs(sum0 - sum1) / 2f;
        }

        public Point Center()
        {
            return Geometry.Scale(Geometry.Add(this._data), 1f / this.Count);
        }

        public Curve Scale(float c)
        {
            var center = this.Center();
            return new Curve(this.Select(p => Geometry.ScaleFrom(p, center, c)).ToList());
        }
    }

    /// <summary>
    /// These helper functions perform simple operations on 2D vectors
    /// represented as Point objects.
    /// For example, |2u+v+w|^2 would be written 
    /// SquaredLength(Add(Scale(u,2),v,w)).
    /// </summary>
    public static class Geometry
    {
        public static Point Add(params Point[] vs)
        {
            return vs.Aggregate(
                (a, b) => new Point(a.x + b.x, a.y + b.y)
           );
        }

        public static Point Add(List<Point> vs)
        {
            return vs.Aggregate(
                (a, b) => new Point(a.x + b.x, a.y + b.y)
           );
        }

        public static Point Subtract(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        public static Point Scale(Point point, float factor)
        {
            return new Point(point.x * factor, point.y * factor);
        }

        public static Point ScaleFrom(Point point, Point center, float factor)
        {
            return Add(Scale(Subtract(point, center), factor), center);
        }

        public static float SquaredLength(Point point)
        {
            return point.x * point.x + point.y * point.y;
        }

        public static float Length(Point point)
        {
            return Convert.ToSingle(Math.Sqrt(point.x * point.x + point.y * point.y));
        }

        public static float Dot(Point a, Point b)
        {
            return a.x * b.x - a.y * b.y;
        }

        public static float Cross(Point a, Point b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static bool Equals(Point a, Point b)
        {
            return Math.Abs(a.x-b.x) < 0.0001f && Math.Abs(b.y - b.y) < 0.0001f;
        }

        public static float Curvature(Neighbourhood<Point> x)
        {
            var twiceDisplacement = Subtract(x(1), x(-1));
            var laplacian = Add(x(1), x(-1), Scale(x(0), -2));
            var dr2 = SquaredLength(Subtract(x(1), x(-1))) * 0.25f;
            return Math.Abs(0.5f * Cross(twiceDisplacement, laplacian) * Convert.ToSingle(Math.Pow(dr2,-3f/2f)) );
        }
    }
}
