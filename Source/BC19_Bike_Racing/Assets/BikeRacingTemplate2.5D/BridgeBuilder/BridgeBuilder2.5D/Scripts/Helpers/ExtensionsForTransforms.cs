using UnityEngine;
using System.Collections.Generic;
using System;

namespace Kamgam.BridgeBuilder25D.Helpers
{
    public static class Extensions
    {
#if !UNITY_2021_1_OR_NEWER
        // Starting with Unity 2021 this method extists natively.

        /// <summary>
        /// Searches in the parents or this component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static T GetComponentInParent<T>(this Component comp, bool includeInactive)
        {
            int depthLimit = 100;
            T result;
            Transform t = comp.transform;
            while (t != null && depthLimit-- > 0)
            {
                if (t.gameObject.activeInHierarchy || includeInactive)
                {
                    result = t.GetComponent<T>();
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        t = t.parent;
                    }
                }
            }
            return default(T);
        }
#endif

        /// <summary>
        /// Returns the component of direct children.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSearch"></param>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static List<T> GetComponentsInDirectChildren<T>(this Component comp, bool includeInactive) where T : class
        {
            return GetDirectChildrenWith<T>(comp.transform, includeInactive);
        }

        /// <summary>
        /// Returns the component of direct children.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSearch"></param>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static List<T> GetDirectChildrenWith<T>(this Transform transform, bool includeInactive) where T : class
        {
            List<T> result = new List<T>();

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (includeInactive == true || child.gameObject.activeSelf == true)
                {
                    var comp = child.GetComponent<T>();
                    if (comp != null)
                    {
                        result.Add(comp);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the components which match the predicate.
        /// Don't you dare use this in Update(), it's slow!
        /// </summary>
        /// <typeparam name="TSearch">The type to search for (acts as a filter).</typeparam>
        /// <typeparam name="TReturn">The component to extract and return.</typeparam>
        /// <param name="objectToSearch"></param>
        /// <param name="includeInactive"></param>
        /// <param name="predicate"></param>
        /// <returns>Returns an empty list of no components are found.</returns>
        public static List<TReturn> GetComponentsInChildrenWhere<TSearch, TReturn>(this Transform transform, bool includeInactive, Func<TSearch, bool> predicate) where TSearch : Component
        {
            List<TReturn> result = new List<TReturn>();
            var tmp = transform.GetComponentsInChildren<TSearch>(includeInactive);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (includeInactive == true || tmp[i].gameObject.activeSelf == true)
                {
                    if (predicate(tmp[i]))
                    {
                        var comp = tmp[i].GetComponent<TReturn>();
                        if (comp != null)
                        {
                            result.Add(comp);
                        }
                    }
                }
            }

            return result;
        }

        public static Transform SetAnchoredPosX(this Transform t, float newX)
        {
            return (t as RectTransform).SetAnchoredPosX(newX);
        }

        public static RectTransform SetAnchoredPosX(this RectTransform t, float newX)
        {
            var pos = t.anchoredPosition;
            pos.x = newX;
            t.anchoredPosition = pos;
            return t;
        }

        public static Transform SetAnchoredPosY(this Transform t, float newY)
        {
            return (t as RectTransform).SetAnchoredPosY(newY);
        }

        public static RectTransform SetAnchoredPosY(this RectTransform t, float newY)
        {
            var pos = t.anchoredPosition;
            pos.y = newY;
            t.anchoredPosition = pos;
            return t;
        }

        public static Transform SetLocalPositionX(this Transform t, float newX)
        {
            var localPosition = t.localPosition;
            localPosition.x = newX;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform SetLocalPositionY(this Transform t, float newY)
        {
            var localPosition = t.localPosition;
            localPosition.y = newY;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform SetLocalPositionZ(this Transform t, float newZ)
        {
            var localPosition = t.localPosition;
            localPosition.z = newZ;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform SetPositionZ(this Transform t, float newZ)
        {
            var position = t.position;
            position.z = newZ;
            t.position = position;
            return t;
        }

        public static Transform AddToLocalPositionX(this Transform t, float deltaX)
        {
            var localPosition = t.localPosition;
            localPosition.x += deltaX;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform AddToLocalPositionY(this Transform t, float deltaY)
        {
            var localPosition = t.localPosition;
            localPosition.y += deltaY;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform AddToLocalPositionZ(this Transform t, float deltaZ)
        {
            var localPosition = t.localPosition;
            localPosition.z += deltaZ;
            t.localPosition = localPosition;
            return t;
        }

        public static Transform SetLocalScale(this Transform t, Vector3 newScale)
        {
            t.localScale = newScale;
            return t;
        }

        public static Transform SetLocalScale(this Transform t, float scaleXYZ)
        {
            t.localScale = new Vector3(scaleXYZ, scaleXYZ, scaleXYZ);
            return t;
        }


        public static Transform SetLocalScaleX(this Transform t, float newX)
        {
            var localScale = t.localScale;
            localScale.x = newX;
            t.localScale = localScale;
            return t;
        }

        public static Transform SetLocalScaleY(this Transform t, float newY)
        {
            var localScale = t.localScale;
            localScale.y = newY;
            t.localScale = localScale;
            return t;
        }

        public static Transform SetLocalScaleZ(this Transform t, float newZ)
        {
            var localScale = t.localScale;
            localScale.z = newZ;
            t.localScale = localScale;
            return t;
        }

        public static Transform SetLocalScaleUniform(this Transform t, float scale)
        {
            t.localScale = new Vector3(scale, scale, scale);
            return t;
        }

        public static Transform AddToLocalScaleUniform(this Transform t, float scaleDelta)
        {
            var scale = t.localScale;
            scale.x += scaleDelta;
            scale.y += scaleDelta;
            scale.z += scaleDelta;
            t.localScale = scale;
            return t;
        }

        /// <summary>
        /// Sets x, y and z to the scale value but keeps the sign.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Transform SetLocalScaleUniformKeepSign(this Transform t, float scale)
        {
            var localScale = t.localScale;
            t.localScale = new Vector3(
                localScale.x < 0 ? -scale : scale,
                localScale.y < 0 ? -scale : scale,
                localScale.z < 0 ? -scale : scale
                );
            return t;
        }

        /// <summary>
        /// If an object hast a loval
        /// </summary>
        /// <param name="t"></param>
        /// <param name="allowZero">If true then Vector3.zero is kept as zero. Otherwise it will return a Vector (1,1,1). Default: true</param>
        /// <returns></returns>
        public static Vector3 GetUniformSignedLocalScale(this Transform t, bool allowZero = true)
        {
            var localScale = t.localScale;

            if (allowZero && localScale.x == 0 && localScale.y == 0 && localScale.z == 0)
            {
                return Vector3.zero;
            }

            return new Vector3(
                Mathf.Sign(localScale.x),
                Mathf.Sign(localScale.y),
                Mathf.Sign(localScale.z)
                );
        }

        public static RectTransform SetLocalScaleUniform(this RectTransform t, float scale)
        {
            t.localScale = new Vector3(scale, scale, scale);
            return t;
        }

        public static Transform SetLocalEulerAngleX(this Transform t, float rotationX)
        {
            var angles = t.localRotation.eulerAngles;
            angles.x = rotationX;
            var rot = t.localRotation;
            rot.eulerAngles = angles;
            t.localRotation = rot;
            return t;
        }

        public static Transform SetLocalEulerAngleY(this Transform t, float rotationY)
        {
            var angles = t.localRotation.eulerAngles;
            angles.y = rotationY;
            t.localRotation = Quaternion.Euler(angles);
            return t;
        }

        public static Transform AddToLocalEulerAngleY(this Transform t, float rotationYDelta)
        {
            var angles = t.localRotation.eulerAngles;
            angles.y += rotationYDelta;
            var rot = t.localRotation;
            rot.eulerAngles = angles;
            t.localRotation = rot;
            return t;
        }

        public static Transform SetLocalEulerAngleZ(this Transform t, float rotationZ)
        {
            var angles = t.localRotation.eulerAngles;
            angles.z = rotationZ;
            var rot = t.localRotation;
            rot.eulerAngles = angles;
            t.localRotation = rot;
            return t;
        }

        public static Transform AddToLocalEulerAngleZ(this Transform t, float rotationZDelta)
        {
            var angles = t.localRotation.eulerAngles;
            angles.z += rotationZDelta;
            var rot = t.localRotation;
            rot.eulerAngles = angles;
            t.localRotation = rot;
            return t;
        }

        /// <summary>
        /// A method similar to Mathf.Max() only for vector lengths.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxMagnitude"></param>
        /// <returns></returns>
        public static Vector2 ScaleToMax(this Vector2 source, float maxMagnitude)
        {
            float magnitude = source.magnitude;
            if (magnitude > maxMagnitude)
            {
                return source * (maxMagnitude / magnitude);
            }
            else
            {
                return source;
            }
        }

        /// <summary>
        /// A method similar to Mathf.Max() only for vector lengths.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxMagnitude"></param>
        /// <returns></returns>
        public static Vector3 ScaleToMax(this Vector3 source, float maxMagnitude)
        {
            float magnitude = source.magnitude;
            if (magnitude > maxMagnitude)
            {
                return source * (maxMagnitude / magnitude);
            }
            else
            {
                return source;
            }
        }

        /// <summary>
        /// Returns a real NULL value even for UnityObjects.
        /// Allows you to use the null-conditional operator on Unity Objects.
        /// Thanks to: https://forum.unity.com/threads/unity-2017-and-null-conditional-operators.489176/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T RealNull<T>(this T obj) where T : UnityEngine.Object
        {
            return obj ? obj : null;
        }

        public static int IndexOfFirstActiveSelf(this IList<GameObject> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && source[i].activeSelf)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfFirstActiveInHierarchy(this IList<GameObject> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && source[i].activeInHierarchy)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfFirstActiveSelf(this IList<Component> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].gameObject != null && source[i].gameObject.activeSelf)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfFirstActiveInHierarchy(this IList<Component> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].gameObject != null && source[i].gameObject.activeInHierarchy)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
