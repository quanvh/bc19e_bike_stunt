// Copyright (c) 2014-2018 Anthony Carapetis
// This software is licensed under the MIT license.
// See LICENSE for more details.
// C# Version: 2020 KAMGAM e.U.

using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Collections;

namespace Kamgam.CurveShorteningFlow
{
    public static class Extensions
    {
        public static List<L> Splice<L>(this List<L> source, int start, int count, L[] values)
        {
            List<L> retVal = source.Skip(start).Take(count).ToList<L>();
            source.RemoveRange(start, count);
            source.InsertRange(start, values);
            return retVal;
        }
    }
}
