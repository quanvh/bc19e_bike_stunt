using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    public static class UtilsAsync
    {
        /// <summary>
        /// Behaves like a "await Task.WhenAny" call with the exception of logging all exceptions.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task<Task> WhenAny(List<Task> tasks)
        {
            var resultTask = await Task.WhenAny(tasks);

            if (resultTask.Exception != null && resultTask.Exception.InnerExceptions != null)
            {
                Array.ForEach(resultTask.Exception.InnerExceptions.ToArray(), e =>
                {
                    Debug.LogException(e);
                });
            }

            return resultTask;
        }

        /// <summary>
        /// Behaves like a "await Task.WhenAny" call with the exception of logging all exceptions.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task<Task> WhenAny(params Task[] tasks)
        {
            var resultTask = await Task.WhenAny(tasks);

            if(resultTask.Exception != null && resultTask.Exception.InnerExceptions != null)
            {
                Array.ForEach(resultTask.Exception.InnerExceptions.ToArray(), e =>
                {
                    Debug.LogException(e);
                });
            }

            return resultTask;
        }

        /// <summary>
        /// Behaves like a "await Task.WhenAny" call with the exception of logging all exceptions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task<Task<T>> WhenAny<T>(params Task<T>[] tasks)
        {
            var resultTask = await Task.WhenAny<T>(tasks);

            if (resultTask.Exception != null && resultTask.Exception.InnerExceptions != null)
            {
                Array.ForEach(resultTask.Exception.InnerExceptions.ToArray(), e =>
                {
                    Debug.LogException(e);
                });
            }

            return resultTask;
        }

        /// <summary>
        /// Behaves like a "await Task.WhenAll" with the exception of logging all exceptions.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task<Task> WhenAll(params Task[] tasks)
        {
            var allTask = Task.WhenAll(tasks);
            await allTask;

            // error propagation for the tasks
            Array.ForEach(tasks, task =>
            {
                if (task.Exception != null && task.Exception.InnerExceptions != null)
                {
                    Array.ForEach(task.Exception.InnerExceptions.ToArray(), e =>
                    {
                        Debug.LogException(e);
                    });
                }
            });

            return allTask;
        }

        public static async Task<bool> PressKey(CancellationToken ct, KeyCode keyCode)
        {
            while( Input.GetKeyDown(keyCode) == false )
            {
                if( ct.IsCancellationRequested )
                {
                    return false;
                }
                await Task.Yield();
            }
            return true;
        }

        public static async Task<T> PressAnyButton<T>( CancellationToken ct, params T[] buttons ) where T : Button
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            try
            {
                var linkedCt = linkedCts.Token;
                var tasks = buttons.Where(b=> b != null).Select(b => PressButton(linkedCt, b)).ToArray();
                var finishedTask = await UtilsAsync.WhenAny(tasks); // first button to finish
                linkedCts.Cancel(); // cancel all remaining buttons
                return finishedTask.Result;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        public static async Task<T> PressButton<T>(CancellationToken ct, T button) where T : Button
        {
            if( button == null )
            {
#if UNITY_EDITOR
                Debug.LogWarning("UtilsAsync.PressButton() returned immediately because button is NULL.");
#endif
                return null;
            }

            bool isPressed = false;
            UnityAction pressedFunc = () => isPressed = true;
            button.onClick.AddListener(pressedFunc);

            while( !isPressed )
            {
                // cancel ?
                if ( ct.IsCancellationRequested )
                {
                    break;
                }

                await Task.Yield(); //Todo: wait for frame end instead of yield?
            }

            button.onClick.RemoveListener(pressedFunc);

            // cancel ?
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            else
            {
                return button;
            }
        }


        public static async Task<T> LoadScene<T>(string name, UnityEngine.SceneManagement.LoadSceneMode mode, CancellationToken ct) where T : UnityEngine.MonoBehaviour
        {
            if (!IsSceneInBuild(name))
            {
                throw new Exception("UtilsAsync.LoadScene: No scene with name '" + name + "' in list of built scenes (add it in the build settings).");
            }

            var loadResult = await LoadScene(name, mode, ct);
            if(loadResult == null)
            {
                return null;
            }

            var result = GameObject.FindObjectOfType<T>();

            // cancel ?
            if( ct.IsCancellationRequested && result != null )
            {
                if (result.gameObject != null)
                {
                    GameObject.DestroyImmediate(result.gameObject, true);
                }
                return null;
            }

            return result;
        }

        public static async Task<AsyncOperation> LoadScene(string name, UnityEngine.SceneManagement.LoadSceneMode mode, CancellationToken ct)
        {
            if (IsSceneInBuild(name) == false)
            {
                Debug.LogError("UtilsAsync.LoadScene: No scene with name '" + name + "' in list of built scenes (add it in the build settings).");
                return null;
            }

            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, mode); // can not be cancelled
            while (!asyncOperation.isDone)
                await Task.Yield();

            // cancel ?
            if( ct.IsCancellationRequested )
            {
                asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(name); // can not be cancelled
                while (!asyncOperation.isDone)
                    await Task.Yield();

                return null;
            }

            return asyncOperation;
        }

        public static async Task<AsyncOperation> UnloadScene(string name, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == false)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneByName(name).IsValid())
                {
                    var asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(name);
                    while (!asyncOperation.isDone)
                        await Task.Yield();
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public static AsyncOperation UnloadScene(string name)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneByName(name).IsValid())
            {
                return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(name);
            }
            else
            {
                return null;
            }
        }

        public static bool IsSceneInBuild(string sceneName)
        {
            List<string> scenesInBuild = new List<string>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/");
                scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            }

            return scenesInBuild.Contains(sceneName);
        }
    }
}
