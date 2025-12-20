using UnityEngine;
using System.Collections.Generic;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public interface ITransformMemory
    {
        bool ApplyTo(GameObject gameObject);
        bool ApplyTo(Transform target);
        bool ApplyTo(RectTransform target);
    }

    public interface ITransformMemoryWithTarget
    {
       Transform GetTarget();
       void SetTarget( Transform transform );

        ITransformMemory GetMemory();
        void SetMemory(ITransformMemory transform);

        /// <summary>
        /// Returns true if the memory could be loaded, otherwise false (if the targert is null for example).
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        bool Apply();
    }

    public class TransformMemory : ITransformMemory
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public TransformMemory(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        public TransformMemory(Transform transform)
        {
            LocalPosition = transform.localPosition;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;
        }

        public TransformMemory(RectTransform transform)
        {
            Debug.LogWarning("TransformMemory used for RectTransform, use RectTransformMemory instead!");
            LocalPosition = transform.localPosition;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;
        }

        public bool ApplyTo(GameObject gameObject)
        {
            return ApplyTo(gameObject.GetComponent<Transform>());
        }

        public bool ApplyTo(Transform target)
        {
            if (target != null)
            {
                target.localPosition = this.LocalPosition;
                target.localRotation = this.LocalRotation;
                target.localScale = this.LocalScale;
                return true;
            }
            return false;
        }

        public bool ApplyTo(RectTransform target)
        {
            Debug.LogWarning("TransformMemory used for RectTransform, use RectTransformMemory instead!");
            return ApplyTo(target as Transform);
        }

        public static TransformMemory CreateFrom(Component target)
        {
            if (target != null)
            {
                var go = target.gameObject;
                if (go != null)
                {
                    return CreateFrom(go);
                }
            }
            return null;
        }

        public static TransformMemory CreateFrom(GameObject gameObject)
        {
            return CreateFrom(gameObject.GetComponent<Transform>());
        }

        public static TransformMemory CreateFrom(Transform source)
        {
            if (source != null)
            {
                return new TransformMemory(
                    source.localPosition,
                    source.localRotation,
                    source.localScale
                );
            }
            return null;
        }

        public static TransformMemory CreateFrom(RectTransform source)
        {
            Debug.LogWarning("TransformMemory used for RectTransform, use RectTransformMemory instead!");
            return CreateFrom(source as Transform);
        }
    }

    public class RectTransformMemory : ITransformMemory
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 AnchoredPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public RectTransformMemory(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Quaternion localRotation, Vector3 localScale)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            AnchoredPosition = anchoredPosition;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        public RectTransformMemory(RectTransform transform)
        {
            AnchorMin = transform.anchorMin;
            AnchorMax = transform.anchorMax;
            AnchoredPosition = transform.anchoredPosition;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;
        }

        public bool ApplyTo(GameObject gameObject)
        {
            return ApplyTo(gameObject.GetComponent<RectTransform>());
        }

        public bool ApplyTo(Transform target)
        {
            return ApplyTo(target as RectTransform);
        }

        public bool ApplyTo(RectTransform target)
        {
            if (target != null)
            {
                target.anchorMin = this.AnchorMin;
                target.anchorMax = this.AnchorMax;
                target.anchoredPosition = this.AnchoredPosition;
                target.localRotation = this.LocalRotation;
                target.localScale = this.LocalScale;
                return true;
            }
            return false;
        }

        public static RectTransformMemory CreateFrom(Component target)
        {
            if (target != null)
            {
                var go = target.gameObject;
                if (go != null)
                {
                    return CreateFrom(go);
                }
            }
            return null;
        }

        public static RectTransformMemory CreateFrom(GameObject gameObject)
        {
            return CreateFrom(gameObject.GetComponent<RectTransform>());
        }

        public static RectTransformMemory CreateFrom(Transform source)
        {
            return CreateFrom(source as RectTransform);
        }

        public static RectTransformMemory CreateFrom(RectTransform source)
        {
            if (source != null)
            {
                return new RectTransformMemory(
                    source.anchorMin,
                    source.anchorMax,
                    source.anchoredPosition,
                    source.localRotation,
                    source.localScale
                );
            }
            return null;
        }
    }

    public class TransformMemoryWithTarget : ITransformMemoryWithTarget
    {
        public ITransformMemory Memory;
        public Transform Target;

        public TransformMemoryWithTarget(ITransformMemory memory, Transform target)
        {
            Memory = memory;
            Target = target;
        }

        public ITransformMemory GetMemory()
        {
            return Memory;
        }

        public void SetMemory(ITransformMemory memory)
        {
            this.Memory = memory;
        }

        public Transform GetTarget()
        {
            return Target;
        }

        public void SetTarget(Transform targetTransform)
        {
            this.Target = targetTransform;
        }

        public bool Apply()
        {
            if (Memory != null && Target != null)
            {
                return Memory.ApplyTo(Target);
            }
            return false;
        }

        public static ITransformMemoryWithTarget CreateFrom( Transform target )
        {
            if (target != null)
            {
                return new TransformMemoryWithTarget(TransformMemory.CreateFrom(target), target);
            }
            return null;
        }

        public static ITransformMemoryWithTarget CreateFrom(RectTransform target)
        {
            if (target != null)
            {
                return new TransformMemoryWithTarget(RectTransformMemory.CreateFrom(target), target);
            }
            return null;
        }

        public static ITransformMemoryWithTarget CreateFrom(GameObject target)
        {
            if (target != null)
            {
                var rect = target.GetComponent<RectTransform>();
                if(rect != null)
                {
                    return new TransformMemoryWithTarget(RectTransformMemory.CreateFrom(rect), rect);
                }
                else
                {
                    var trans = target.GetComponent<Transform>();
                    if (trans != null)
                    {
                        return new TransformMemoryWithTarget(TransformMemory.CreateFrom(trans), trans);
                    }
                }
            }
            return null;
        }

        public static ITransformMemoryWithTarget CreateFrom(Component target)
        {
            if (target != null)
            {
                var go = target.gameObject;
                if (go != null)
                {
                    return CreateFrom(go);
                }
            }
            return null;
        }

        public static List<ITransformMemoryWithTarget> CreateFrom(IList<GameObject> gameObjects)
        {
            var result = new List<ITransformMemoryWithTarget>();
            foreach (var item in gameObjects)
            {
                var memory = CreateFrom(item);
                if(memory != null)
                {
                    result.Add(memory);
                }
            }
            return result;
        }

        public static List<ITransformMemoryWithTarget> CreateFrom(IList<Component> components)
        {
            var result = new List<ITransformMemoryWithTarget>();
            foreach (var item in components)
            {
                var memory = CreateFrom(item);
                if (memory != null)
                {
                    result.Add(memory);
                }
            }
            return result;
        }

        public static List<ITransformMemoryWithTarget> CreateFrom(IList<Transform> transforms)
        {
            var result = new List<ITransformMemoryWithTarget>();
            foreach (var item in transforms)
            {
                var memory = CreateFrom(item);
                if (memory != null)
                {
                    result.Add(memory);
                }
            }
            return result;
        }

        public static List<ITransformMemoryWithTarget> CreateFrom(IList<RectTransform> rectTransforms)
        {
            var result = new List<ITransformMemoryWithTarget>();
            foreach (var item in rectTransforms)
            {
                var memory = CreateFrom(item);
                if (memory != null)
                {
                    result.Add(memory);
                }
            }
            return result;
        }

        public static bool Apply(List<ITransformMemoryWithTarget> memoriesWithTargets)
        {
            bool succesForAll = true;
            bool succesForCurrent;
            foreach (var memory in memoriesWithTargets)
            {
                if (memory != null)
                {
                    succesForCurrent = memory.Apply();
                    if(succesForCurrent == false)
                    {
                        succesForAll = false;
                    }
                }
                else
                {
                    succesForAll = false;
                }
            }

            return succesForAll;
        }
    }
}
