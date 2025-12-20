using UnityEngine;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Kamgam.BikeRacing25D
{
    public abstract class UIDialogSceneAsync<ReturnType> : UIBaseFade
    {
        /// <summary>
        /// Bootstrap method which loads the dialog scene and shows the dialog within.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sceneName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<ReturnType> SpawnFromScene<T>(string sceneName, CancellationToken ct) where T : UIDialogSceneAsync<ReturnType>
        {
            var dialog = await UtilsAsync.LoadScene<T>(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive, ct);
            return await dialog.executeWrapper(ct);
        }

        /// <summary>
        /// Should async dialog wait for the transition to end or can it return immediately after a button was pressed?
        /// </summary>
        public bool WaitForTransition;

        protected async Task<ReturnType> executeWrapper(CancellationToken ct)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            try
            {
                var linkedCt = linkedCts.Token;
                
                // Show and wait for transition
                Show();
                while (IsTransitioning())
                    await Task.Yield();

                // The actual dialog content (implemented by descendant class)
                ReturnType result = await execute(linkedCt);

                // Hide and wait for transition
                Hide();

                // Return after or before hide transition?
                if (WaitForTransition)
                {
                    while (IsTransitioning())
                        await Task.Yield();
                    _ = UtilsAsync.UnloadScene(gameObject.scene.name);
                }
                else if(!IsTransitioning())
                {
                    _ = UtilsAsync.UnloadScene(gameObject.scene.name);
                }
                else
                {
                    // Use a coroutine to wait for the transition to end and then unload the scene.
                    StartCoroutine(UnloadAfterTransitionEnded());
                }

                return result;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        protected virtual async Task<ReturnType> execute(CancellationToken ct)
        {
            // derived classes can add custom tasks here.
            await Task.Yield();
            return default(ReturnType);
        }

        public IEnumerator UnloadAfterTransitionEnded()
        {
            while(IsTransitioning())
                yield return null;

            _ = UtilsAsync.UnloadScene(gameObject.scene.name);
        }

        /// <summary>
        /// Utility helper to add waiting for a button to a list of tasks.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="tasksToWaitFor"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        public static Task<Button> AddButtonPressToTasks(CancellationToken ct, List<Task> tasksToWaitFor, Button button)
        {
            var task = UtilsAsync.PressButton(ct, button);
            tasksToWaitFor.Add(task);
            return task;
        }
    }
}
