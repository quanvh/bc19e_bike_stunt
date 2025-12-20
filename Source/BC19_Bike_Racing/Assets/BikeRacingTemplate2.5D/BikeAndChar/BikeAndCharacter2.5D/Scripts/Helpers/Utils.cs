using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public class Utils
    {
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
            if(UnityEditor.EditorApplication.isPlaying)
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
    }
}
