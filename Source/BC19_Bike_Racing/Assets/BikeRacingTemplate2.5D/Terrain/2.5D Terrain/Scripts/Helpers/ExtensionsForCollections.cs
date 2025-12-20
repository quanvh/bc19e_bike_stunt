using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

namespace Kamgam.Terrain25DLib.Helpers
{
    public static class ExtensionsForCollections
    {
        public static bool IsFirstOf<T>(this T element, IList<T> list)
        {
            if (list == null || list.Count < 1)
                return false;

            return list.IndexOf(element) == 0;
        }

        public static bool IsLastOf<T>(this T element, IList<T> list)
        {
            if (list == null || list.Count < 1)
                return false;

            return list.IndexOf(element) == list.Count - 1;
        }

        public static bool IsNullOrEmpty(this IList List)
        {
            return (List == null || List.Count < 1);
        }

        public static bool IsNullOrEmpty(this IDictionary Dictionary)
        {
            return (Dictionary == null || Dictionary.Count < 1);
        }

        public static bool AddIfNotContained<T>(this IList<T> source, T item)
        {
            if(source.Contains(item))
            {
                return false;
            }
            else
            {
                source.Add(item);
                return true;
            }
        }

        public static void AddIfNotContained<T>(this IList<T> source, IList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (source.Contains(items[i]) == false)
                {
                    source.Add(items[i]);
                }
            }
        }

        public static void AddIfNotContained<T>(this IList<T> source, T item, Func<T, bool> predicate)
        {
            int index = source.FirstIndexNonAlloc(predicate);
            if (index < 0)
            {
                source.Add(item);
            }
        }

        public static void AddOrReplace<T>(this IList<T> source, T item)
        {
            int index = source.IndexOf(item);
            if (index >= 0)
            {
                source[index] = item;
            }
            else
            {
                source.Add(item);
            }
        }

        public static void AddOrReplace<T>(this IList<T> source, T item, Func<T,bool> predicate)
        {
            int index = source.FirstIndexNonAlloc(predicate);
            if (index >= 0)
            {
                source[index] = item;
            }
            else
            {
                source.Add(item);
            }
        }

        public static bool AddIfNotContained<T1, T2>(this IDictionary<T1, T2> source, T1 key, T2 value)
        {
            if (source.ContainsKey(key))
            {
                return false;
            }
            else
            {
                source.Add(key, value);
                return true;
            }
        }

        public static void AddOrReplace<T1, T2>(this IDictionary<T1, T2> source, T1 key, T2 value)
        {
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else
            {
                source.Add(key, value);
            }
        }

        public static T2 GetOrDefault<T1, T2>(this IDictionary<T1, T2> source, T1 key, T2 defaultValue = default(T2))
        {
            if (source == null)
            {
                return defaultValue;
            }

            if (source.ContainsKey(key))
            {
                return source[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public static void RemoveNullTolerant<T>(this IList<T> source, T itemToRemove)
        {
            if (source == null || source.Count == 0)
                return;

            source.Remove(itemToRemove);
        }

        public static void Remove<T>(this IList<T> source, IList<T> itemsToRemove)
        {
            if (source == null)
                return;

            for (int i = itemsToRemove.Count-1; i >= 0 ; i--)
            {
                source.Remove(itemsToRemove[i]);
            }
        }

        /// <summary>
        /// Removes the item in both lists only if it is contained in the "source" list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsToRemove"></param>
        public static void RemoveFromBothIfContained<T>(this IList<T> source, IList<T> itemsToRemove)
        {
            if (source == null)
                return;

            for (int i = itemsToRemove.Count-1; i >= 0; i--)
            {
                if (source.Contains(itemsToRemove[i]) == true)
                {
                    source.Remove(itemsToRemove[i]);
                    itemsToRemove.RemoveAt(i);
                }
            }
        }

        public static IList<T> RemoveNullValues<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null || list[i].Equals(null)) // Equals(null) is necessary because == null would return false on MissingRefs or UnityObjects. Unity overrides Equals().
                {
                    list.RemoveAt(i);
                }
            }
            return list;
        }

        /// <summary>
        /// Contains one or more of the given items?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool ContainsAny<T>(this IList<T> source, IList<T> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (source.Contains(items[i]) == true)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Contains one or more of the given items?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool ContainsAny<T>(this IList<T> source, params T[] items)
        {
            for (int i = items.Length - 1; i >= 0; i--)
            {
                if (source.Contains(items[i]) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAll<T>(this IList<T> source, IList<T> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (source.Contains(items[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Use Linq to take the last N elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        /// <summary>
        /// Use linq to get a distict list by a selector.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> compareFunc)
        {
            foreach (TSource element in source)
            {
                if (compareFunc(element))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Shuffle<T>(this IList<T> list, int seed = -1)
        {
            var stateOld = UnityEngine.Random.state;
            if (seed >= 0)
            {
                UnityEngine.Random.InitState(seed);
            }

            int n = list.Count;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            if (seed >= 0)
            {
                UnityEngine.Random.state = stateOld;
            }
        }

        public static bool Contains<T>(this T[] array, T value)
        {
            if(array == null)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if(array[i].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// ForEach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void ForEach<T>(this T[] source, Action<T> predicate)
        {
            for (int i = source.Length - 1; i >= 0; i--)
            {
                predicate(source[i]);
            }
        }

        #region nonalloc
        public static T FirstNonAlloc<T>(this IList<T> source)
        {
            return source[0];
        }

        public static T FirstNonAlloc<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            throw new Exception("No matching element found!");
        }

        /// <summary>
        /// Fetches the very first element or returns the default if there is none (null tolerant).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T FirstOrDefaultNonAlloc<T>(this IList<T> source)
        {
            if (source != null && source.Count > 0)
            {
                return source[0];
            }
            return default(T);
        }

        public static T FirstOrDefaultNonAlloc<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default(T);
        }

        /// <summary>
        /// Finds the first values which matches. If none is found then defaultValue is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T FirstOr<T>(this IList<T> source, Func<T, bool> predicate, T defaultValue)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns the found index or -1 if no match has been found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int FirstIndexNonAlloc<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the found index or -1 if no match has been found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> source, T item)
        {
            return source.IndexOf(item);
        }

        /// <summary>
        /// Last Element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T LastNonAlloc<T>(this IList<T> source)
        {
            return source[source.Count - 1];
        }

        /// <summary>
        /// Last Element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T LastOrDefaultNonAlloc<T>(this IList<T> source)
        {
            if (source != null && source.Count > 0)
            {
                return source[source.Count - 1];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Removes the last elements and returns that element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Pop<T>(this IList<T> source)
        {
            var result = source[source.Count - 1];
            source.RemoveAt(source.Count - 1);
            return result;
        }

        /// <summary>
        /// Last Element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T LastOrDefaultNonAlloc<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default(T);
        }

        public static int LastIndexNonAlloc<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Fills the result array with matching elements and returns the number of matches found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="result"></param>
        /// <returns>Number of matching entries.</returns>
        public static int WhereNonAlloc<T>(this T[] source, Func<T, bool> predicate, ref T[] result)
        {
            int validCount = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    result[validCount] = source[i];
                    validCount++;
                }
            }
            return validCount;
        }

        /// <summary>
        /// Fills the result array with matching elements and returns the number of matches found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="result"></param>
        /// <returns>Number of matching entries.</returns>
        public static int WhereNonAlloc<T>(this List<T> source, Func<T, bool> predicate, ref List<T> result)
        {
            result.Clear();
            int validCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    result.Add(source[i]);
                    validCount++;
                }
            }
            return validCount;
        }

        public static bool AnyNonAlloc<T>(this T[] source)
        {
            return source != null && source.Length > 0;
        }

        public static bool AnyNonAlloc<T>(this List<T> source)
        {
            return source != null && source.Count > 0;
        }

        public static float MaxNonAlloc<T>(this T[] source, Func<T, float> predicate)
        {
            float max = Mathf.NegativeInfinity;
            float current;
            for (int i = 0; i < source.Length; i++)
            {
                current = predicate(source[i]);
                if(current > max)
                {
                    max = current;
                }
            }
            return max;
        }

        public static float MaxNonAlloc<T>(this List<T> source, Func<T, float> predicate)
        {
            float max = Mathf.NegativeInfinity;
            float current;
            for (int i = 0; i < source.Count; i++)
            {
                current = predicate(source[i]);
                if (current > max)
                {
                    max = current;
                }
            }
            return max;
        }

        public static float MinNonAlloc<T>(this T[] source, Func<T, float> predicate)
        {
            float min = Mathf.Infinity;
            float current;
            for (int i = 0; i < source.Length; i++)
            {
                current = predicate(source[i]);
                if (current < min)
                {
                    min = current;
                }
            }
            return min;
        }

        public static float MinNonAlloc<T>(this List<T> source, Func<T, float> predicate)
        {
            float min = Mathf.Infinity;
            float current;
            for (int i = 0; i < source.Count; i++)
            {
                current = predicate(source[i]);
                if (current < min)
                {
                    min = current;
                }
            }
            return min;
        }

        public static int MaxNonAlloc<T>(this T[] source, Func<T, int> predicate)
        {
            int max = int.MinValue;
            int current;
            for (int i = 0; i < source.Length; i++)
            {
                current = predicate(source[i]);
                if (current > max)
                {
                    max = current;
                }
            }
            return max;
        }

        public static int MaxNonAlloc<T>(this List<T> source, Func<T, int> predicate)
        {
            int max = int.MinValue;
            int current;
            for (int i = 0; i < source.Count; i++)
            {
                current = predicate(source[i]);
                if (current > max)
                {
                    max = current;
                }
            }
            return max;
        }

        public static int MinNonAlloc<T>(this T[] source, Func<T, int> predicate)
        {
            int min = int.MaxValue;
            int current;
            for (int i = 0; i < source.Length; i++)
            {
                current = predicate(source[i]);
                if (current < min)
                {
                    min = current;
                }
            }
            return min;
        }

        public static int MinNonAlloc<T>(this List<T> source, Func<T, int> predicate)
        {
            int min = int.MaxValue;
            int current;
            for (int i = 0; i < source.Count; i++)
            {
                current = predicate(source[i]);
                if (current < min)
                {
                    min = current;
                }
            }
            return min;
        }

        public static int CountNonAlloc<T>(this T[] source, Func<T, bool> predicate = null)
        {
            if(predicate == null)
            {
                return source.Length;
            }

            int validCount = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    validCount++;
                }
            }
            return validCount;
        }

        public static int CountNonAlloc<T>(this IList<T> source, Func<T, bool> predicate = null)
        {
            if (source == null)
                return 0;

            if (predicate == null)
            {
                return source.Count;
            }

            int validCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    validCount++;
                }
            }
            return validCount;
        }

        public static void TakeFirstNonAlloc<T>(this T[] source, int N, ref T[] result)
        {
            int max = Mathf.Min(N, Mathf.Min(result.Length, source.Length));
            for (int i = 0; i < max; i++)
            {
                result[i] = source[i];
            }
        }

        /// <summary>
        /// The result list has to be big enough to hold the results, otherwise allocation may occur via List expansion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <param name="N"></param>
        public static void TakeFirstNonAlloc<T>(this List<T> source, int N, ref List<T> result)
        {
            result.Clear();
            int max = Mathf.Min(N, source.Count);
            for (int i = 0; i < max; i++)
            {
                result.Add(source[i]);
            }
        }

        public static void TakeLastNonAlloc<T>(this T[] source, int N, ref T[] result)
        {
            N = Mathf.Min(N, Mathf.Min(result.Length, source.Length));
            int resultCounter = 0;
            for (int i = source.Length - N; i < source.Length; i++)
            {
                result[resultCounter] = source[i];
                resultCounter++;
            }
        }

        /// <summary>
        /// The result list has to be big enough to hold the results, otherwise allocation may occur via List expansion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="N"></param>
        /// <param name="result"></param>
        public static void TakeLastNonAlloc<T>(this List<T> source, int N, ref List<T> result)
        {
            result.Clear();
            N = Mathf.Min(N, source.Count);
            int resultCounter = 0;
            for (int i = source.Count - N; i < source.Count; i++)
            {
                result.Add(source[i]);
                resultCounter++;
            }
        }

        public static void TakeLastNonAlloc<T>(this Queue<T> source, int N, ref List<T> result)
        {
            result.Clear();
            N = Mathf.Min(N, source.Count);
            int counter = 0;
            int endIndex = source.Count;
            int startIndex = endIndex - N;
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (counter >= startIndex && counter < endIndex)
                {
                    result.Add(enumerator.Current);
                }
                counter++;
            }
        }

        public static void InsertOrderedNonAlloc<T>(this List<T> source, T value, Func<T, T, int> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i], value) < 0)
                {
                    source.Insert(i, value);
                    return;
                }
            }
            source.Add(value);
        }
        #endregion

        public static void RemoveLast<T>(this List<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }

        public static void RemoveFirst<T>(this List<T> source)
        {
            source.RemoveAt(0);
        }

        public static void Clear<T>(this T[] source, T emptyValue)
        {
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = emptyValue;
            }
        }

        public static void Clear<T>(this T[] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = default(T);
            }
        }

        public static void Swap<T>(this IList<T> source, int indexA, int indexB)
        {
            T tmp = source[indexA];
            source[indexA] = source[indexB];
            source[indexB] = tmp;
        }

        public static void Swap<T>(this IList<T> source, T itemA, T itemB)
        {
            source.Swap(source.IndexOf(itemA), source.IndexOf(itemB));
        }

        public static void Swap<T>(this IList<T> source, int indexA, T itemB)
        {
            source.Swap(indexA, source.IndexOf(itemB));
        }

        public static void Swap<T>(this IList<T> source, T itemA, int indexB)
        {
            source.Swap(source.IndexOf(itemA), indexB);
        }

        /// <summary>
        /// If the queue already has sizeLimit items in it then one will be dequeued before enqueing the new item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <param name="sizeLimit"></param>
        public static void EnqueueWithLimit<T>(this Queue<T> source, T item, int sizeLimit)
        {
            if (source.Count >= sizeLimit)
            {
                source.Dequeue();
            }
            source.Enqueue(item);
        }

        #region Unity specific
        public static void SetActive(this IList<GameObject> items, bool active)
        {
            if (items == null || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                    items[i].SetActive(active);
            }
        }

        public static void SetActive(this IList<MonoBehaviour> items, bool active)
        {
            if (items == null || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].gameObject != null)
                    items[i].gameObject.SetActive(active);
            }
        }

        public static void SetActive(this IList<Component> items, bool active)
        {
            if (items == null || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].gameObject != null)
                    items[i].gameObject.SetActive(active);
            }
        }
        #endregion
    }
}
