using UnityEngine;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    /// <summary>
    /// All methods will draw in the XY plane.
    /// ToDo: add optional parameter to specify the plane.
    /// </summary>
    public static class UtilsDebug
    {
        public static string Dump(this string str, int index)
        {
            Debug.Log(index + ": " + str);
            return str;
        }

        public static string Dump(this string str)
        {
            Debug.Log(str);
            return str;
        }

        public static void DrawX(Vector3 position, float size = 1.0f, Color color = default(Color), float duration = 0)
        {
#if UNITY_EDITOR
            Debug.DrawLine(position + new Vector3(-size * 0.5f, -size * 0.5f), position + new Vector3(size * 0.5f, size * 0.5f), color, duration);
            Debug.DrawLine(position + new Vector3(-size * 0.5f, size * 0.5f), position + new Vector3(size * 0.5f, -size * 0.5f), color, duration);
#endif
        }

        /// <summary>
        /// Draws a normal to vector at position with a length of size.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vector"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="duration">Set to 0 in Editor to keep it for one frame.</param>
        public static void DrawNormal(Vector3 position, Vector3 vector, float size = 1.0f, Color color = default(Color), float duration = 0)
        {
#if UNITY_EDITOR
            Debug.DrawLine(position + new Vector3(vector.normalized.y * size * 0.5f, -vector.normalized.x * size * 0.5f),
                           position + new Vector3(-vector.normalized.y * size * 0.5f, vector.normalized.x * size * 0.5f), color, duration);
#endif
        }

        public static void DrawVector(Vector3 position, Vector3 vector, Color color = default(Color), float duration = 0f)
        {
#if UNITY_EDITOR
            float arrowSize = vector.magnitude / 5.0f;
            var arrowStartPos = position + vector - vector.normalized * arrowSize;

            // base
            DrawNormal(position, vector, arrowSize, color, duration);

            // line
            Debug.DrawLine(position, arrowStartPos, color, duration);

            // arrow
            Debug.DrawLine(arrowStartPos + new Vector3(vector.normalized.y * arrowSize * 0.5f, -vector.normalized.x * arrowSize * 0.5f),
                           arrowStartPos + new Vector3(-vector.normalized.y * arrowSize * 0.5f, vector.normalized.x * arrowSize * 0.5f), color, duration);
            Debug.DrawLine(arrowStartPos + new Vector3(vector.normalized.y * arrowSize * 0.5f, -vector.normalized.x * arrowSize * 0.5f),
                           position + vector, color, duration);
            Debug.DrawLine(arrowStartPos + new Vector3(-vector.normalized.y * arrowSize * 0.5f, vector.normalized.x * arrowSize * 0.5f),
                           position + vector, color, duration);
#endif
        }

        public static void DrawVectorPos(Vector3 startPosition, Vector3 endPosition, Color color = default(Color), float duration = 0f)
        {
#if UNITY_EDITOR
            var vector = endPosition - startPosition;
            float arrowSize = vector.magnitude / 5.0f;
            var arrowStartPos = startPosition + vector - vector.normalized * arrowSize;

            // base
            DrawNormal(startPosition, vector, arrowSize, color, duration);

            // line
            Debug.DrawLine(startPosition, arrowStartPos, color, duration);

            // arrow
            Debug.DrawLine(arrowStartPos + new Vector3(vector.normalized.y * arrowSize * 0.5f, -vector.normalized.x * arrowSize * 0.5f),
                           arrowStartPos + new Vector3(-vector.normalized.y * arrowSize * 0.5f, vector.normalized.x * arrowSize * 0.5f), color, duration);
            Debug.DrawLine(arrowStartPos + new Vector3(vector.normalized.y * arrowSize * 0.5f, -vector.normalized.x * arrowSize * 0.5f),
                           startPosition + vector, color, duration);
            Debug.DrawLine(arrowStartPos + new Vector3(-vector.normalized.y * arrowSize * 0.5f, vector.normalized.x * arrowSize * 0.5f),
                           startPosition + vector, color, duration);
#endif
        }

        /// <summary>
        /// Time is between 0.0f and 1.0f, just like in other regular LERP methods.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="vector"></param>
        /// <param name="time">Time is between 0.0f and 1.0f</param>
        /// <param name="color"></param>
        /// <param name="duration">Set to 0 in Editor to keep it for one frame.</param>
        public static void DrawVectorLERP(Vector3 position, Vector3 vector, float time, Color color = default(Color), float duration = 0f)
        {
#if UNITY_EDITOR
            float size = vector.magnitude / 5.0f;
            DrawVector(position, Vector3.Lerp(Vector3.zero, vector, time), color, duration);
            DrawNormal(position + vector, vector, size, color, duration);
#endif
        }

        public static void DrawBox(Vector3 center, float width, float height, Color color = default(Color), float duration = 0f)
        {
#if UNITY_EDITOR
            Debug.DrawLine(center + new Vector3(-width / 2f, height / 2f, 0), center + new Vector3(width / 2f, height / 2f, 0), color, duration);
            Debug.DrawLine(center + new Vector3(-width / 2f, -height / 2f, 0), center + new Vector3(width / 2f, -height / 2f, 0), color, duration);
            Debug.DrawLine(center + new Vector3(-width / 2f, -height / 2f, 0), center + new Vector3(-width / 2f, height / 2f, 0), color, duration);
            Debug.DrawLine(center + new Vector3(width / 2f, -height / 2f, 0), center + new Vector3(width / 2f, height / 2f, 0), color, duration);
#endif
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="duration">Set to 0 in Editor to keep it for one frame.</param>
        /// <param name="circleSegments"></param>
        public static void DrawCircle(Vector3 center, float radius, Color color = default(Color), float duration = 0f, int circleSegments = 10)
        {
#if UNITY_EDITOR
            // getCirclePoints
            var angleInRad = 0f;  // angle that will be increased each loop
            var step = Mathf.PI * 2f / circleSegments;
            Vector3 point = Vector3.zero;
            Vector3 previousPoint = Vector3.zero;
            for (int i = 0; i < circleSegments; ++i)
            {
                point.x = radius * Mathf.Cos(angleInRad);
                point.y = radius * Mathf.Sin(angleInRad);
                angleInRad += step;
                if(i == circleSegments-1)
                {
                    Debug.DrawLine(center + previousPoint, center + point, color, duration);
                    previousPoint.x = radius * Mathf.Cos(0);
                    previousPoint.y = radius * Mathf.Sin(0);
                    Debug.DrawLine(center + point, center + previousPoint, color, duration);
                }
                else if (i > 0)
                {
                    Debug.DrawLine(center + previousPoint, center + point, color, duration);
                }
                previousPoint = point;
            }
#endif
        }

        public static void DrawBounds(Bounds boundsInWorldSpace, Color color = default(Color), float duration = 0f)
        {
#if UNITY_EDITOR
            Vector3 point0 = boundsInWorldSpace.min;
            Vector3 point1 = boundsInWorldSpace.center + new Vector3(boundsInWorldSpace.extents.x, -boundsInWorldSpace.extents.y, -boundsInWorldSpace.extents.z);
            Vector3 point2 = boundsInWorldSpace.center + new Vector3(boundsInWorldSpace.extents.x, boundsInWorldSpace.extents.y, -boundsInWorldSpace.extents.z);
            Vector3 point3 = boundsInWorldSpace.center + new Vector3(-boundsInWorldSpace.extents.x, boundsInWorldSpace.extents.y, -boundsInWorldSpace.extents.z);
            Vector3 point4 = boundsInWorldSpace.center + new Vector3(-boundsInWorldSpace.extents.x, -boundsInWorldSpace.extents.y, boundsInWorldSpace.extents.z);
            Vector3 point5 = boundsInWorldSpace.center + new Vector3(boundsInWorldSpace.extents.x, -boundsInWorldSpace.extents.y, boundsInWorldSpace.extents.z);
            Vector3 point6 = boundsInWorldSpace.center + new Vector3(boundsInWorldSpace.extents.x, boundsInWorldSpace.extents.y, boundsInWorldSpace.extents.z);
            Vector3 point7 = boundsInWorldSpace.center + new Vector3(-boundsInWorldSpace.extents.x, boundsInWorldSpace.extents.y, boundsInWorldSpace.extents.z);

            Debug.DrawLine(point0, point1, color, duration);
            Debug.DrawLine(point1, point2, color, duration);
            Debug.DrawLine(point2, point3, color, duration);
            Debug.DrawLine(point3, point0, color, duration);

            Debug.DrawLine(point0, point4, color, duration);
            Debug.DrawLine(point1, point5, color, duration);
            Debug.DrawLine(point2, point6, color, duration);
            Debug.DrawLine(point3, point7, color, duration);

            Debug.DrawLine(point4, point5, color, duration);
            Debug.DrawLine(point5, point6, color, duration);
            Debug.DrawLine(point6, point7, color, duration);
            Debug.DrawLine(point7, point4, color, duration);
#endif
        }
    }
}
