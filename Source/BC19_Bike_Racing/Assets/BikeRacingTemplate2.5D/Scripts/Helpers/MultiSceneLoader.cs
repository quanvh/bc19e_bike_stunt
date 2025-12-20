using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// Loads and unloads multiple scenes.
    /// </summary>
    public class MultiSceneLoader
    {
        /// <summary>
        /// isDone will be true if all scenes loaded or if an error occurred (check isError for that).
        /// </summary>
        protected bool isDone;
        public bool IsDone
        {
            get => isDone;
        }

        public bool isError;
        public bool IsError
        {
            get => isError;
        }

        public string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
        }

        protected float progress;
        public float Progress
        {
            get => progress;
        }

        public AsyncOperation LoadOperation;

        /// <summary>
        /// Creates a new loader object and loads a scene using coroutines to wait for the result.
        /// </summary>
        /// <param name="coroutineRunner"></param>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static MultiSceneLoader LoadSceneAsync(MonoBehaviour coroutineRunner, string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            return LoadScenesAsync(coroutineRunner, new List<string> { sceneName }, mode);
        }

        /// <summary>
        /// Creates a new loader object and loads the scenes using coroutines to wait for the result.
        /// </summary>
        /// <param name="coroutineRunner"></param>
        /// <param name="sceneNames"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static MultiSceneLoader LoadScenesAsync(MonoBehaviour coroutineRunner, IList<string> sceneNames, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            var loader = new MultiSceneLoader();
            coroutineRunner.StartCoroutine(loader.LoadScenesAsync(sceneNames, mode));
            return loader;
        }

        public IEnumerator LoadScenesAsync(IList<string> sceneNames, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (sceneNames == null || sceneNames.Count == 0)
            {
                errorMessage = "No scenes to load.";
                isError = true;
                isDone = true;
                progress = 1f;
                yield break;
            }

            if (!AreScenesInBuild(sceneNames))
            {
                errorMessage = "Some of the given scene names are not in the build.";
                isError = true;
                isDone = true;
                progress = 1f;
                yield break;
            }

            isDone = false;
            isError = false;
            errorMessage = null;
            progress = 0;
            int scenesLoaded = 0;
            int scenesCount = sceneNames.Count;
            while (isDone == false && scenesLoaded < scenesCount)
            {
                LoadOperation = null;
                try
                {
                    LoadOperation = SceneManager.LoadSceneAsync(sceneNames[scenesLoaded], mode);
                }
                catch (System.Exception e)
                {
                    isError = true;
                    errorMessage = e.Message;
                    isDone = true;
                    progress = 1;
                }
                while (isDone == false && LoadOperation != null && LoadOperation.isDone == false)
                {
                    yield return null;
                }
                scenesLoaded++;
                progress = Mathf.Clamp01(scenesLoaded * (1.0f / scenesCount));
            }
            isDone = true;
            progress = 1;
        }

        /// <summary>
        /// Creates a new loader object and unloads a scene using coroutines to wait for the result.
        /// </summary>
        /// <param name="coroutineRunner"></param>
        /// <param name="sceneName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MultiSceneLoader UnloadSceneAsync(MonoBehaviour coroutineRunner, string sceneName, UnityEngine.SceneManagement.UnloadSceneOptions options)
        {
            return UnloadScenesAsync(coroutineRunner, new List<string> { sceneName }, options);
        }

        /// <summary>
        /// Creates a new loader object and unloads the scenes using coroutines to wait for the result.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sceneNames"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MultiSceneLoader UnloadScenesAsync(MonoBehaviour owner, IList<string> sceneNames, UnityEngine.SceneManagement.UnloadSceneOptions options)
        {
            var loader = new MultiSceneLoader();
            owner.StartCoroutine(loader.UnloadScenesAsync(sceneNames, options));
            return loader;
        }

        public IEnumerator UnloadScenesAsync(IList<string> sceneNames, UnityEngine.SceneManagement.UnloadSceneOptions options)
        {
            isDone = false;
            isError = false;
            errorMessage = null;
            progress = 0;
            int scenesLoaded = 0;
            int scenesCount = sceneNames.Count;
            while (isDone == false && scenesLoaded < scenesCount)
            {
                AsyncOperation loadLevelOperation = null;
                try
                {
                    loadLevelOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneNames[scenesLoaded], options);
                }
                catch (System.Exception e)
                {
                    isError = true;
                    errorMessage = e.Message;
                    isDone = true;
                    progress = 1;
                }
                while (isDone == false && loadLevelOperation != null && loadLevelOperation.isDone == false)
                {
                    yield return null;
                }
                scenesLoaded++;
                progress = Mathf.Clamp01(scenesLoaded * (1.0f / scenesCount));
            }
            isDone = true;
            progress = 1;
        }

        public static T GetRoot<T>(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene != null)
            {
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var comp = root.GetComponent<T>();
                    if (comp != null)
                        return comp;
                }
            }

            return default(T);
        }

        public static bool AreScenesInBuild(IList<string> sceneNames)
        {
            foreach (var name in sceneNames)
            {
                if (IsSceneInBuild(name) == false)
                {
                    return false;
                }
            }
            return true;
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
