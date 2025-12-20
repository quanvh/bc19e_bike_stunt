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
    public static class Flow
    {
        /// <summary>
        /// Forward Euler approximation to CSF with tangential reparametrization
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static LocalFunction<Point, Point> ReparametrizedCSF(float dt)
        {
            return (point, index, x) =>
            {
                var laplacian = Geometry.Add(x(1), x(-1), Geometry.Scale(x(0), -2));
                var dr2 = Geometry.SquaredLength(Geometry.Subtract(x(1), x(-1))) * 0.25f;
                return Geometry.Add(x(0), Geometry.Scale(laplacian, dt / dr2));
            };
        }

        /// <summary>
        /// Remesh: Redivide curve to keep nodes evenly distributed
        /// </summary>
        /// <param name="cu"></param>
        /// <param name="seglength"></param>
        public static void Remesh(Curve cu, float seglength)
        {
            // Remesh: Redivide curve to keep nodes evenly distributed
            for (var i = 0; i < cu.Count; i++)
            {
                var a = cu.Get(i);
                var b = cu.Get(i + 1);
                var displacement = Geometry.Subtract(b, a);
                var dr2 = Geometry.SquaredLength(displacement);

                if (dr2 > 4 * seglength * seglength)
                {
                    // If vertices are too far apart, add a new vertex in between
                    cu.Splice(1 + i, 0,
                        Geometry.Add(a, Geometry.Scale(displacement, seglength * Convert.ToSingle(Math.Pow(dr2, -1f/2f))))
                    );
                }

                else if (cu.Count > 4 && dr2 * 4 < seglength * seglength)
                {
                    // If vertices are too close, remove one of them
                    cu.Splice(i--, 1);
                }
            }
        }

        public static void Clean(Curve cu)
        {
            for (var i = 0; i < cu.Count; i++)
            {
                if (Geometry.Equals(cu.Get(i), cu.Get(i + 2))) cu.Splice(i--, 2);
            }
        }
    }
}
