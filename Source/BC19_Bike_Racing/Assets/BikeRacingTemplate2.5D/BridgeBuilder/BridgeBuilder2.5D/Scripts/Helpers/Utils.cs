using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Kamgam.BridgeBuilder25D.Helpers
{
    public class Utils
    {
        /// <summary>
        /// Be ware that if called without 'rand' within a short time it will return
        /// identical results because System.Random default seed is based on the
        /// system clock.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static T PickRandomFrom<T>(IEnumerable<T> enumerable, System.Random rand = null)
        {
            if (rand == null)
            {
                rand = new System.Random();
            }
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static Transform FindOrCreate(Transform parent, string name)
        {
            Transform trans = null;
            
            if(parent != null)
                trans = parent.Find(name);

            if (trans == null)
            {
                var obj = new GameObject(name);
                if(parent != null)
                    obj.transform.parent = parent;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                trans = obj.transform;
            }

            return trans;
        }

        public static List<Transform> FindManyByName(Transform searchTarget, string searchTerm, bool exactMatch = true)
        {
            var result = new List<Transform>();
            for (int i = 0; i < searchTarget.childCount; i++)
            {
                if (
                    (exactMatch && searchTarget.GetChild(i).name.CompareTo(searchTerm) == 0)
                    || (!exactMatch && searchTarget.GetChild(i).name.Contains(searchTerm))
                    )
                {
                    result.Add(searchTarget.GetChild(i));
                }
            }
            return result;
        }

        public static T InstantiatePrefab<T>(GameObject prefab, Transform parent, bool startEnabled = false)
        {
            bool wasEnabled = prefab.activeSelf;
            prefab.SetActive(startEnabled);

            T result;
            if (parent != null)
            {
                result = GameObject.Instantiate(prefab, parent.position, parent.rotation, parent).GetComponent<T>();
            }
            else
            {
                result = GameObject.Instantiate(prefab).GetComponent<T>();
            }

#if UNITY_EDITOR
            prefab.SetActive(wasEnabled); // avoid changing the prefab source in the editor.
#endif
            return result;
        }

        public static GameObject SmartInstantiatePrefab(GameObject prefab, Transform parent, bool startEnabled = false)
        {
            return SmartInstantiatePrefab<Transform>(prefab, parent, startEnabled).gameObject;
        }

        /// <summary>
        /// Support instantiaction prefabs in Editor even if not in play mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="startEnabled"></param>
        /// <returns></returns>
        public static T SmartInstantiatePrefab<T>(GameObject prefab, Transform parent, bool startEnabled = false)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                return InstantiatePrefab<T>(prefab, parent, startEnabled);
            }
            else
            {
                bool wasEnabled = prefab.activeSelf;
                prefab.SetActive(startEnabled);

                var prefabInNirvana = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                prefabInNirvana.transform.parent = parent;
                prefabInNirvana.transform.position = parent.position;
                prefabInNirvana.transform.rotation = parent.rotation;
                var result = prefabInNirvana.GetComponent<T>();

                prefab.SetActive(wasEnabled); // avoid changing the prefab source in the editor.
                return result;
            }
#else
            return InstantiatePrefab<T>(prefab, parent, startEnabled);
#endif
        }

        public static GameObject SmartInstantiate(UnityEngine.GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            }
            else
#endif
            {
                return GameObject.Instantiate(prefab, parent);
            }
        }

        public static void SmartDestroy(IEnumerable<UnityEngine.Component> comps)
        {
            foreach (var comp in comps)
            {
                if (comp != null)
                {
                    SmartDestroy(comp);
                }
            }
        }

        public static void SmartDestroy(IEnumerable<UnityEngine.GameObject> objs)
        {
            foreach (var obj in objs)
            {
                if (obj != null)
                {
                    SmartDestroy(obj);
                }
            }
        }

        public static void SmartDestroy(UnityEngine.GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                GameObject.DestroyImmediate(obj);
            }
            else
#endif
            {
                GameObject.Destroy(obj);
            }
        }

        public static void SmartDestroy(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                GameObject.DestroyImmediate(obj);
            }
            else
#endif
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
