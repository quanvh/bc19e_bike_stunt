using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string String)
        {
            return string.IsNullOrEmpty((String ?? "" ).Trim());
        }

        public static string ToString(this string String, string defaultValueIfNull)
        {
            return string.IsNullOrEmpty(String) ? defaultValueIfNull : String;
        }

        /// <summary>
        /// A random float between 0f and 1f (both inclusive).
        /// </summary>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static float Value(this System.Random rnd)
        {
            return rnd.Next(0, 10000001) / 10000000f;
        }

        public static float Range(this System.Random rnd, float minInc, float maxInc)
        {
            return minInc + rnd.Value() * (maxInc - minInc);
        }

        public static int Range(this System.Random rnd, int minInc, int maxExc)
        {
            return rnd.Next(minInc, maxExc);
        }

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
            while(t != null && depthLimit-- > 0)
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
        public static List<TReturn> GetComponentsInChildrenWhere<TSearch,TReturn>(this Transform transform, bool includeInactive, Func<TSearch, bool> predicate) where TSearch : Component
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
            if ( magnitude > maxMagnitude )
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

        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        /// <summary>
        /// Thanks to https://stackoverflow.com/a/50191835
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="child">Works only with children which are direct children of ScrollRect.content</param>
        /// <returns></returns>
        public static Vector2 GetNormalizedPositionToBringChildIntoView(this UnityEngine.UI.ScrollRect instance, RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 targetPos = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );

            var pos = instance.content.localPosition;
            instance.content.localPosition = targetPos;
            var result = new Vector2(
                Mathf.Clamp(instance.horizontalNormalizedPosition, 0f, 1f),
                Mathf.Clamp(instance.verticalNormalizedPosition, 0f, 1f)
            );
            instance.content.localPosition = pos;
            return result;
        }

        /// <summary>
        /// Moves the content to bring the Rect of "child" into viewport (moves only if needed).
        /// </summary>
        /// <param name="instance">The ScrollRect</param>
        /// <param name="child">It does not matter how deep the child is nested.</param>
        /// <returns></returns>
        public static void BringChildIntoView(this UnityEngine.UI.ScrollRect instance, RectTransform child)
        {
            instance.content.ForceUpdateRectTransforms();
            instance.viewport.ForceUpdateRectTransforms();

            Rect childRectInViewportLocalCoords = instance.viewport.TransformRectFrom(child);
            Rect viewportRectInViewportLocalCoords = instance.viewport.rect;
            var newContentPosition = instance.content.localPosition;

            // update content postition based on viewport and child (clamp to viewport)
            bool moveNeeded = false;
            float deltaXMin = viewportRectInViewportLocalCoords.xMin - childRectInViewportLocalCoords.xMin;
            if(deltaXMin > 0.001f) // clamp to <= 0
            {
                newContentPosition.x += deltaXMin;
                moveNeeded = true;
            }
            float deltaXMax = viewportRectInViewportLocalCoords.xMax - childRectInViewportLocalCoords.xMax;
            if (deltaXMax < -0.001f) // clamp to >= 0
            {
                newContentPosition.x += deltaXMax;
                moveNeeded = true;
            }
            float deltaYMin = viewportRectInViewportLocalCoords.yMin - childRectInViewportLocalCoords.yMin;
            if (deltaYMin > 0.001f) // clamp to <= 0
            {
                newContentPosition.y += deltaYMin;
                moveNeeded = true;
            }
            float deltaYMax = viewportRectInViewportLocalCoords.yMax - childRectInViewportLocalCoords.yMax;
            if (deltaYMax < -0.001f) // clamp to >= 0
            {
                newContentPosition.y += deltaYMax;
                moveNeeded = true;
            }

            // apply final position
            if (moveNeeded)
            {
                instance.content.localPosition = newContentPosition;
                instance.content.ForceUpdateRectTransforms();
            }
        }

        /// <summary>
        /// Forces the rect transforms (viewport and content) to update.
        /// </summary>
        /// <param name="instance"></param>
        public static void ForceUpdate(this UnityEngine.UI.ScrollRect instance)
        {
            instance.content.ForceUpdateRectTransforms();
            instance.viewport.ForceUpdateRectTransforms();
        }

        /// <summary>
        /// Converts a Rect from one RectTransfrom to this RectTransfrom (as if the "from" is a child of this).
        /// Hint: use the Canvas Transform as "to" to get the reference pixel positions.
        /// Similar to: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UI/ScriptBindings/RectTransformUtility.cs
        /// </summary>
        /// <param name="from">The rect transform which should be transformed as if it was a child(!) of this transform.</param>
        /// <returns></returns>
        public static Rect TransformRectFrom(this Transform to, Transform from)
        {
            RectTransform fromRectTrans = from.GetComponent<RectTransform>();
            RectTransform toRectTrans = to.GetComponent<RectTransform>();

            if (fromRectTrans != null && toRectTrans != null)
            {
                Vector3[] fromWorldCorners = new Vector3[4];
                Vector3[] toLocalCorners = new Vector3[4];
                Matrix4x4 toLocal = to.worldToLocalMatrix;
                fromRectTrans.GetWorldCorners(fromWorldCorners);
                for (int i = 0; i < 4; i++)
                {
                    toLocalCorners[i] = toLocal.MultiplyPoint3x4(fromWorldCorners[i]);
                }

                return new Rect(toLocalCorners[0].x, toLocalCorners[0].y, toLocalCorners[2].x - toLocalCorners[1].x, toLocalCorners[1].y - toLocalCorners[0].y);
            }

            return default(Rect);
        }

        /// <summary>
        /// Finds the closest in the list based on bounds.center.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static RectTransform FindClosestToCenter(this RectTransform source, params RectTransform[] list)
        {
            if(list == null || list.Length == 0)
            {
                return null;
            }

            // long drag, go to closest
            var center = source.rect.center;
            Bounds bounds;
            float minDistance = Mathf.Infinity;
            float tmpDistance;
            RectTransform result = null;
            for (int i = 0; i < list.Length; i++)
            {
                bounds = source.TransformBoundsTo(list[i]);
                tmpDistance = (center - (Vector2)bounds.center).sqrMagnitude;
                if (tmpDistance < minDistance)
                {
                    minDistance = tmpDistance;
                    result = list[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
        {
            Vector3[] corners = new Vector3[4];

            // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
            var bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(corners);

                var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                var matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Transform the point of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        public static Vector3 TransformPointTo(this RectTransform source, Vector3 point, Transform target)
        {
            // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
            var result = Vector3.zero;
            if (source != null)
            {
                result = target.worldToLocalMatrix.MultiplyPoint3x4(source.TransformPoint(point));
            }
            return result;
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Scroll the target element to the vertical center of the scroll rect's viewport.
        /// Assumes the target element is part of the scroll rect's contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect's content to center vertically</param>
        /// <param name="axis"></param>
        public static float CalcScrollToCenterNormalizedPosition(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Horizontal)
        {
            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);

            // Normalize and apply the calculated offset
            if (axis == RectTransform.Axis.Vertical)
            {
                var offset = viewRect.center.y - elementBounds.center.y;
                var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                return Mathf.Clamp(scrollPos, 0, 1);
            }
            else
            {
                var offset = viewRect.center.x - elementBounds.center.x;
                var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
                return Mathf.Clamp(scrollPos, 0, 1);
            }
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Scroll the target element to the vertical center of the scroll rect's viewport.
        /// Assumes the target element is part of the scroll rect's contents. Will use the given position within target instead of the center.
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="target"></param>
        /// <param name="posInTargetOnAxis"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float CalcScrollToCenterNormalizedPosition(this ScrollRect scrollRect, RectTransform target, float posInTargetOnAxis, RectTransform.Axis axis = RectTransform.Axis.Horizontal)
        {
            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;

            Vector3 pos = target.TransformPointTo(
                axis == RectTransform.Axis.Horizontal ? new Vector3(posInTargetOnAxis, 0,0) : new Vector3(0, posInTargetOnAxis, 0),
                view
            );

            // Normalize and apply the calculated offset
            if (axis == RectTransform.Axis.Vertical)
            {
                var offset = viewRect.center.y - pos.y;
                var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                return Mathf.Clamp(scrollPos, 0, 1);
            }
            else
            {
                var offset = viewRect.center.x - pos.x;
                var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
                return Mathf.Clamp(scrollPos, 0, 1);
            }
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
        /// <returns>The normalized scoll distance</returns>
        public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect's internal SetNormalizedPosition method
            var viewport = scrollRect.viewport;
            var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

            var content = scrollRect.content;
            var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        /// <summary>
        /// Thanks to: https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
        /// Scroll the target element to the vertical center of the scroll rect's viewport.
        /// Assumes the target element is part of the scroll rect's contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect's content to center vertically</param>
        /// <param name="axis"></param>
        public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis)
        {
            scrollRect.content.ForceUpdateRectTransforms();
            scrollRect.viewport.ForceUpdateRectTransforms();

            if (axis == RectTransform.Axis.Vertical)
            {
                scrollRect.verticalNormalizedPosition = CalcScrollToCenterNormalizedPosition(scrollRect, target, axis);
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = CalcScrollToCenterNormalizedPosition(scrollRect, target, axis);
            }

            scrollRect.content.ForceUpdateRectTransforms();
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

        /// <summary>
        /// Replaces the last occurence of "oldValue" with "newValue"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static string ReplaceLast(this string source, string oldValue, string newValue)
        {
            int place = source.LastIndexOf(oldValue);

            if (place == -1)
            {
                return source;
            }

            string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
            return result;
        }

        public static int IndexOfFirstActiveSelf( this IList<GameObject> source )
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

        /// <summary>
        /// Uses the clips PCM rate (frequency) and timeSamples to calculate a more exact time than "time".
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float ExactTime(this AudioSource source)
        {
            if(source.clip == null)
            {
                return source.time;
            }
            else
            {
                return source.timeSamples / (float)source.clip.frequency;
            }
        }

        /// <summary>
        /// Uses the clips PCM rate (frequency) and timeSamples to calculate a remaining time.
        /// Returns 0 if no clip exists.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float TimeLeft(this AudioSource source)
        {
            if (source.clip == null)
            {
                return 0f;
            }
            else
            {
                return (source.clip.samples - source.timeSamples) / (float)source.clip.frequency;
            }
        }
    }
}
