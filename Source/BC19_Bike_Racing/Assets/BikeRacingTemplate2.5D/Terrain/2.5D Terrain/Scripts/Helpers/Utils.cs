using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace Kamgam.Terrain25DLib.Helpers
{
    public class Utils
    {
		public static float AngleX(Vector2 p0, Vector2 p1)
        {
            return Mathf.Asin(Mathf.Abs((p1 - p0).normalized.y)) * Mathf.Rad2Deg;
        }
		
        /// <summary>
        /// Tries to retrieve the value from obj.fieldName and returns it. Returns default(T) on error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <param name="throwException">Throw an exception instead of returning default(T) on error.</param>
        /// <returns>Returns default(T) on error or throws and exception if 'throwException' is true.</returns>
        public static T GetFieldValue<T>(object obj, string fieldName, bool throwException = true, T defaultValue = default(T))
        {
            T result;
            bool success = GetFieldValue<T>(obj, fieldName, out result);
            if (success)
            {
                return result;
            }
            else
            {
                if (throwException)
                {
                    throw new Exception("Utils.GetFieldValue(" + obj + ",'" + fieldName + "') failed.");
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Tries to retrieve the value from obj.fieldName and stores it in out result. Returns true on success.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <param name="result">The value of the field of type T.</param>
        /// <returns>True if value could be fetched, false otherwise.</returns>
        public static bool GetFieldValue<T>(object obj, string fieldName, out T result)
        {
            if (obj == null)
            {
                result = default(T);
                return false;
            }

            Type t = obj.GetType();
            FieldInfo fieldInfo = null;
            while (t != null)
            {
                fieldInfo = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                    break;

                t = t.BaseType;
            }

            if (fieldInfo == null)
            {
                result = default(T);
                return false;
            }

            try
            {
                result = (T)fieldInfo.GetValue(obj);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        public static bool SetFieldValue(object obj, string fieldName, object value)
        {
            try
            {

                if (obj == null)
                    return false;

                Type t = obj.GetType();
                FieldInfo fieldInfo = null;
                while (t != null)
                {
                    fieldInfo = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (fieldInfo != null)
                        break;

                    t = t.BaseType;
                }

                if (fieldInfo == null)
                    return false;

                fieldInfo.SetValue(obj, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                for (int i = obj.transform.childCount-1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(obj.transform.GetChild(i).gameObject);
                }
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
		
// ENUM RND
        public static T RandomResultForRatiosEnum<T>(int seed, params float[] ratios) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return RandomResultForRatiosEnum<T>(new System.Random(seed), ratios);
        }

        public static T RandomResultForRatiosEnum<T>(System.Random rnd, params float[] ratios) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (ratios.Length == 1)
            {
                System.Array values = System.Enum.GetValues(typeof(T));
                return (T)values.GetValue(0);
            }

            float total = ratios.Sum();
            float randomValue = rnd.Next(0, 10000001) / 10000000f * total;

            return ResultForRatiosEnum<T>(randomValue, ratios);
        }

        public static T RandomResultForRatiosEnum<T>(params float[] ratios) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            float total = ratios.Sum();
            float randomValue = UnityEngine.Random.value * total;
            System.Array values = System.Enum.GetValues(typeof(T));

            if (ratios.Length == 1)
                return (T)values.GetValue(0);

            return ResultForRatiosEnum<T>(randomValue, ratios);
        }

        public static T ResultForRatiosEnum<T>(float randomValue, params float[] ratios) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            System.Array values = System.Enum.GetValues(typeof(T));

            if (ratios.Length == 1)
                return (T)values.GetValue(0);

            float current = 0f;
            for (int i = 0; i < ratios.Length; i++)
            {
                if (ratios[i] > 0)
                {
                    current += ratios[i];
                    if (randomValue <= current)
                    {
                        return (T)values.GetValue(i);
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns the random result index based on the given ratios.
        /// Will return -1 if something went wrong.
        /// Example: RandomResultForRatios(1, 0.5f, 3f) will return 0,1 or 2 (with 2 being most likely).
        /// </summary>
        /// <returns></returns>
        public static int RandomResultForRatios(params float[] ratios)
        {
            if (ratios.Length == 1)
                return 0;

            float total = ratios.Sum();
            float randomValue = UnityEngine.Random.value * total;
            return ResultForRatios(randomValue, ratios);
        }

        public static int RandomResultForRatios(int seed, params float[] ratios)
        {
            return RandomResultForRatios(new System.Random(seed), ratios);
        }

        public static int RandomResultForRatios(System.Random rnd, params float[] ratios)
        {
            if (ratios.Length == 1)
                return 0;

            float total = ratios.Sum();
            float randomValue = rnd.Next(0, 10000001) / 10000000f * total;

            return ResultForRatios(randomValue, ratios);
        }

        public static int ResultForRatios(float randomValue, params float[] ratios)
        {
            if (ratios.Length == 1)
                return 0;

            float current = 0f;
            for (int i = 0; i < ratios.Length; i++)
            {
                if (ratios[i] > 0)
                {
                    current += ratios[i];
                    if (randomValue <= current)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
		
/// <summary>
        /// Returns true if the random result check against likelyhood was positive.<br />
        /// Example: RandomResult(0.3f) has a 30% chance of returning true.
        /// If likelyhood is >= 1.0f then true is returned.
        /// </summary>
        /// <param name="likelyhood">Float between 0.0f and 1.0f.</param>
        /// <param name="epsilon">Float value margin (default 1/100th of a percent).</param>
        /// <returns></returns>
        public static bool RandomResult(float likelyhood, float epsilon = 0.0001f)
        {
            if (likelyhood > 1.0f - epsilon)
            {
                return true;
            }
            else if (likelyhood < epsilon)
            {
                return false;
            }
            else
            {
                return UnityEngine.Random.value <= likelyhood;
            }
        }

        /// <summary>
        /// Returns true if the random result check against likelyhood was positive.<br />
        /// Example: RandomResult(0.3f) has a 30% chance of returning true.
        /// If likelyhood is >= 1.0f then true is returned.
        /// </summary>
        /// <param name="seed">Base the random value on this seed.</param>
        /// <param name="likelyhood">Float between 0.0f and 1.0f.</param>
        /// <param name="epsilon">Float value margin (default 1/100th of a percent).</param>
        /// <returns></returns>
        public static bool RandomResult(int seed, float likelyhood, float epsilon = 0.0001f)
        {
            if (likelyhood > 1.0f - epsilon)
            {
                return true;
            }
            else if (likelyhood < epsilon)
            {
                return false;
            }
            else
            {
                var rnd = new System.Random(seed);
                return Value(rnd) <= likelyhood;
            }
        }

        /// <summary>
        /// Returns true if the random result check against likelyhood was positive.<br />
        /// Example: RandomResult(0.3f) has a 30% chance of returning true.
        /// If likelyhood is >= 1.0f then true is returned.
        /// </summary>
        /// <param name="rnd">Base the random value on this generator.</param>
        /// <param name="likelyhood">Float between 0.0f and 1.0f.</param>
        /// <param name="epsilon">Float value margin (default 1/100th of a percent).</param>
        /// <returns></returns>
        public static bool RandomResult(System.Random rnd, float likelyhood, float epsilon = 0.0001f)
        {
            if (likelyhood > 1.0f - epsilon)
            {
                return true;
            }
            else if (likelyhood < epsilon)
            {
                return false;
            }
            else
            {
                return Value(rnd) <= likelyhood;
            }
        }
		
        public static bool FloatEqual(float a, float b, float delta = 0.0001f)
        {
            return Mathf.Abs(a - b) < delta;
        }

        public static bool FloatEqual(double a, double b)
        {
            return a - b > -0.0001d && a - b < 0.0001d;
        }
		
		/// <summary>
        /// Generates a vector in the normal plane, result depends on the given vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 CreateNormal(Vector3 vector)
        {
            return new Vector3(1, 1, (vector.x + vector.y) / -vector.z);
        }

		/// <summary>
        /// A random float between 0f and 1f (both inclusive).
        /// </summary>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static float Value(System.Random rnd)
        {
            return rnd.Next(0, 10000001) / 10000000f;
        }

        /// <inheritdoc cref="FindObjectsInAllLoadedScenes"/>
        public static T FindObjectInAllLoadedScenes<T>(bool includeInactive, Predicate<UnityEngine.SceneManagement.Scene> scenePredicate = null)
        {
            var results = FindObjectsInAllLoadedScenes<T>(includeInactive, scenePredicate);
            if (results != null && results.Count > 0)
            {
                return results[0];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns all Components of type T in all loaded scenes.
        /// Be aware that this may also return objects which are scheduled for destruction at the end of the current frame.
        /// Be double aware that this will NOT return any objects marked as "DontDestroyOnLoad" since those are moved to a
        /// unique scene which is not accessible during runtime.
        /// See: https://gamedev.stackexchange.com/questions/140014/how-can-i-get-all-dontdestroyonload-gameobjects
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scenePredicate">Use this to exclude scenes from being searched.</param>
        /// <returns></returns>
        public static List<T> FindObjectsInAllLoadedScenes<T>(bool includeInactive, Predicate<UnityEngine.SceneManagement.Scene> scenePredicate = null)
        {
            // TODO: For UNity2020+ we could use Object.FindObjectsOfType with includeInactive = true instead.
            UnityEngine.SceneManagement.Scene[] scenes = new UnityEngine.SceneManagement.Scene[UnityEngine.SceneManagement.SceneManager.sceneCount];
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scenePredicate == null || scenePredicate(scene))
                {
                    scenes[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                }
            }

            return FindObjectsInScenes<T>(includeInactive, scenes);
        }

        /// <summary>
        /// Returns all components in the currently active scene.
        /// Be aware that this may also return objects which are scheduled for destruction at the end of the current frame.
        /// Be double aware that this will NOT return any objects marked as "DontDestroyOnLoad" since those are moved to a
        /// unique scene which is not accessible during runtime.
        /// See: https://gamedev.stackexchange.com/questions/140014/how-can-i-get-all-dontdestroyonload-gameobjects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static List<T> FindObjectsInActiveScene<T>(bool includeInactive)
        {
            return FindObjectsInScenes<T>(includeInactive, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        public static List<T> FindObjectsInScenes<T>(bool includeInactive, params UnityEngine.SceneManagement.Scene[] scenes)
        {
            try
            {
                return scenes
                    .Where(s => s.IsValid())
                    .SelectMany(s => s.GetRootGameObjects())
                    .Where(g => includeInactive || g.activeInHierarchy)
                    .SelectMany(g => g.GetComponentsInChildren<T>(includeInactive))
                    .ToList();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public static T FindObjectInScenes<T>(bool includeInactive, params UnityEngine.SceneManagement.Scene[] scenes)
        {
            try
            {
                return scenes
                    .Where(s => s.IsValid())
                    .SelectMany(s => s.GetRootGameObjects())
                    .Where(g => includeInactive || g.activeInHierarchy)
                    .SelectMany(g => g.GetComponentsInChildren<T>(includeInactive))
                    .First();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns all objects with the tag in all loaded scenes.
        /// Be aware that this may also return objects which are scheduled for destruction at the end of the current frame.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<T> FindObjectsWithTagInAllLoadedScenes<T>(string tag, bool includeInactive)
        {
            UnityEngine.SceneManagement.Scene[] scenes = new UnityEngine.SceneManagement.Scene[UnityEngine.SceneManagement.SceneManager.sceneCount];
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                scenes[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            }

            return FindObjectsWithTagInScenes<T>(tag, includeInactive, scenes);
        }

        public static List<T> FindObjectsWithTagInActiveScene<T>(string tag, bool includeInactive)
        {
            return FindObjectsWithTagInScenes<T>(tag, includeInactive, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        public static List<T> FindObjectsWithTagInScenes<T>(string tag, bool includeInactive, params UnityEngine.SceneManagement.Scene[] scenes)
        {
            try
            {
                return scenes
                    .Where(s => s.IsValid())
                    .SelectMany(s => s.GetRootGameObjects())
                    .Where(g => includeInactive || g.activeInHierarchy)
                    .SelectMany(g => g.GetComponentsInChildren<T>(includeInactive))
                    .Where(g => g is Component && (g as Component).tag == tag)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public static T FindObjectWithTag<T>(string tag)
        {
            var obj = GameObject.FindGameObjectWithTag(tag);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return default(T);
        }

        public static List<TInclude> FindRootObjectsInActiveSceneWithout<TInclude, TExclude>(bool includeInactive)
        {
            return FindRootObjectsInScenesWithout<TInclude, TExclude>(includeInactive, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        public static List<TInclude> FindRootObjectsInScenesWithout<TInclude, TExclude>(bool includeInactive, params UnityEngine.SceneManagement.Scene[] scenes)
        {
            try
            {
                var result = new List<TInclude>();
                foreach (var scene in scenes)
                {
                    if (scene.IsValid())
                    {
                        var gameObjects = scene.GetRootGameObjects();
                        foreach (var gameObject in gameObjects)
                        {
                            if (gameObject.TryGetComponent<TExclude>(out var _) == false) // non alloc in Editor (2019.2+)
                            {
                                result.AddRange(gameObject.GetComponentsInChildren<TInclude>(includeInactive));
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new List<TInclude>();
            }
        }
		
        public static Type GetTypeInSameAssemblyAs<T>(string internalTypeName)
        {
            return GetTypeInSameAssemblyAs(typeof(T), internalTypeName);
        }

        public static Type GetTypeInSameAssemblyAs(Type publicType, string internalTypeName)
        {
            var assembly = publicType.Assembly;
            return assembly.GetType(internalTypeName);
        }
		
        public static T GetMethodResult<T>(object instance, string methodName)
        {
            return GetMethodResult<T>(instance, methodName, default(T));
        }

        public static T GetMethodResult<T>(object instance, string methodName, T defaultValue)
        {
            return GetMethodResult<T>(instance, methodName, defaultValue, null);
        }

        public static T GetMethodResult<T>(object instance, string methodName, T defaultValue, params object[] parameters)
        {
            var publicType = instance.GetType();
            if (publicType != null)
            {
                var method = publicType.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    return (T)method.Invoke(instance, parameters);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError("Utils.GetMehtodResult(): method " + methodName + "not found.");
                }
#endif
            }
            return defaultValue;
        }

        public static T GetMethodResult<T>(Type publicType, string methodName)
        {
            return GetMethodResult<T>(publicType, methodName, default(T));
        }

        public static T GetMethodResult<T>(Type publicType, string methodName, T defaultValue)
        {
            return GetMethodResult<T>(publicType, methodName, defaultValue, new object[] { });
        }

        public static T GetMethodResult<T>(Type publicType, string methodName, T defaultValue, params object[] parameters)
        {
            if (publicType != null)
            {
                var method = publicType.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    return (T)method.Invoke(null, parameters);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError("Utils.GetMehtodResult(): method " + methodName + "not found.");
                }
#endif
            }
            return defaultValue;
        }


        /// <summary>
        /// Results are always positive, even if k is negative.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int Mod(int k, int n)
        {
            return (k % n + n) % n;
        }
    }
}
