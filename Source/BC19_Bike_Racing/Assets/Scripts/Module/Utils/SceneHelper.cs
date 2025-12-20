using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
1. Open the scene that is being loaded, containing the game world.
2. Window->Lighting->Settings
3. At the very bottom, uncheck "Auto Generate" and then click "Generate Lighting".
4. Save the scene (leaving "Auto Generate" unchecked).
*/

namespace Bacon
{
    public class SceneHelper : MonoBehaviour
    {
        protected static SceneHelper instance;
        public static bool isLoaded { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log("OnSceneUnloaded: " + scene.name);
            isLoaded = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode arg)
        {
            Debug.Log("OnSceneLoaded: " + scene.name);
            isLoaded = false;
            StartCoroutine(WaitForLOad());
        }

        public static void LoadScene(string sceneName, Action<bool> callback = null)
        {
            instance?.StartCoroutine(DOLoadScene(sceneName, callback));
        }

        private IEnumerator WaitForLOad()
        {
            yield return null;
            isLoaded = true;
        }

        public static IEnumerator DOLoadScene(string sceneName, Action<bool> callback)
        {
            yield return new WaitForSeconds(0.25f);
            //UILoading.SetProcess(null, 0.5f);
            if (SceneManager.sceneCount > 1)
            {
                var scene = SceneManager.GetSceneAt(1);
                if (scene != null)
                {
                    var sceneUnload = SceneManager.UnloadSceneAsync(scene);
                    if (sceneUnload == null)
                    {
                        callback?.Invoke(false);
                        yield break;
                    }

                    while (sceneUnload.progress < 0.9f)
                        yield return null;

                    while (!sceneUnload.isDone)
                        yield return null;

                    yield return new WaitForSeconds(0.25f);
                }
            }

            //UILoading.SetProcess(null, 0.75f);

            var sceneload = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (sceneload == null)
            {
                callback?.Invoke(false);
                yield break;
            }

            sceneload.allowSceneActivation = true;

            while (sceneload.progress < 0.9f)
                yield return null;

            while (!sceneload.isDone)
                yield return null;

            yield return new WaitForSeconds(0.25f);

            if (SceneManager.sceneCount > 1)
            {
                bool hasEx = false;
                var scene = SceneManager.GetSceneAt(1);

                yield return new WaitForEndOfFrame();

                if (scene != null)
                {
                    float timeOut = 3;
                    while (!scene.isLoaded)
                    {
                        timeOut -= Time.deltaTime;
                        if (timeOut <= 0)
                        {
                            callback?.Invoke(false);
                            yield break;
                        }
                        yield return null;
                    }

                    try
                    {
                        SceneManager.SetActiveScene(scene);
                    }
                    catch (Exception ex)
                    {
                        hasEx = true;
                        Debug.LogException(ex);
                    }

                    if (hasEx)
                    {
                        callback?.Invoke(false);
                        yield break;
                    }
                }
                else
                {
                    callback?.Invoke(false);
                    yield break;
                }
            }

            //UILoading.SetProcess(null, 1f);
            yield return new WaitForSeconds(0.25f);
            callback?.Invoke(true);
        }

        public static void Unload(int index = 1)
        {
            instance?.StartCoroutine(DOUnload(index));
        }

        public static IEnumerator DOUnload(int index = 1)
        {
            if (SceneManager.sceneCount > index)
            {
                var gamePlay = SceneManager.GetSceneAt(index);
                if (gamePlay != null)
                {
                    var sceneUnload = SceneManager.UnloadSceneAsync(gamePlay);
                    if (sceneUnload == null)
                    {
                        yield break;
                    }

                    while (sceneUnload.progress < 0.9f)
                        yield return null;

                    while (!sceneUnload.isDone)
                        yield return null;

                    yield return new WaitForSeconds(0.25f);
                }
            }

            var main = SceneManager.GetSceneAt(0);
            SceneManager.SetActiveScene(main);

            yield return new WaitForSeconds(0.25f);
        }
    }
}