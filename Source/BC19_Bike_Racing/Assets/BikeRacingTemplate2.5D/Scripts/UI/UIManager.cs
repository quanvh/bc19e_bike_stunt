using UnityEngine;
using System.Threading.Tasks;

namespace Kamgam.BikeRacing25D
{
    public class UIManager
    {
        public UILoadingScreen LoadingScreen;

        protected UIReferences menuRefs;

        #region Initialization
        protected string lastLoadError;

        public UIManager(UILoadingScreen loadingScreen)
        {
            LoadingScreen = loadingScreen;
        }

        /// <summary>
        /// Returns true if the menus have been loaded successfully.
        /// </summary>
        /// <param name="coroutineRunner"></param>
        /// <returns></returns>
        public async Task<bool> LoadMenusFromScene(MonoBehaviour coroutineRunner)
        {
            var loader = MultiSceneLoader.LoadSceneAsync(coroutineRunner, "UIs", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            while (!loader.IsDone)
                await Task.Yield();
            lastLoadError = loader.ErrorMessage;

            menuRefs = GameObject.FindObjectOfType<UIReferences>();

            return !loader.IsError && menuRefs != null;
        }

        public string GetLastLoadError()
        {
            return lastLoadError;
        }
        #endregion


        public T GetUI<T>() where T : IUI
        {
            return menuRefs.GetUI<T>();
        }

        public void HideAllImmediate(bool hideLoadingScreen = true)
        {
            if (hideLoadingScreen)
                LoadingScreen.HideImmediate();

            menuRefs.HideAllImmediate();
        }

        public void Show<T>() where T : IUI
        {
            var obj = menuRefs.GetUI<T>();
            obj?.Show();
        }

        public void Hide<T>() where T : IUI
        {
            var obj = menuRefs.GetUI<T>();
            obj?.Hide();
        }

        public void HideImmediate<T>() where T : IUI
        {
            menuRefs.GetUI<T>()?.HideImmediate();
        }

        public void ShowLoadingScreenImmediate()
        {
            LoadingScreen.ShowImmediate();
        }

        public void HideLoadingScreen()
        {
            LoadingScreen.Hide();
        }

        public void SetLoadingScreenText(string text)
        {
            LoadingScreen.SetText(text);
        }

        public async Task WaitForTransitionToEnd(IUI ui)
        {
            while (ui.IsTransitioning())
                await Task.Yield();
        }

        public void InitializeAll()
        {
            menuRefs.InitializeAll();
        }
    }
}

