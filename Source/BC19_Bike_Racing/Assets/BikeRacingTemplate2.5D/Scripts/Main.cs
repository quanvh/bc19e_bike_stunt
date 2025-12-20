using Kamgam.InputHelpers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The main object is to root of all logic in the game.<br />
    /// It does:<br />
    /// 1) Initialize some helpers (Singletons)<br />
    /// 2) Loads SaveGame<br />
    /// 3) Saves SaveGame (to ensure everything works)<br />
    /// 4) Loads the UIs<br />
    /// 5) Shows the MainMenu.<br />
    /// <br />
    /// The override of the default execution order is not really
    /// necessary but done to ensure this being the first object to initialize.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class Main : MonoBehaviour
    {
        // The game version. This will also be stored in the savegame upon Save().
        public static string GameVersion => Application.version;

        [Header("Dependencies (Scriptable Objects, Prefabs)")]
        public SaveGameProvider SaveGameProvider;
        public LevelDataProvider LevelDataProvider;
        public GameObject BikeAndCharacterPrefab;

        [Header("Scene References")]
        public Camera MainCamera;
        public UILoadingScreen LoadingScreen;

        // Code-only references
        public UIManager UIManager;
        public Game Game;

        // States, Flags ...
        protected ILevel currentLevel;
        protected Scene mainScene;
        protected bool showGameFinishedMsg;

        /// <summary>
        /// Determines whether the touch interface is used or not.
        /// </summary>
        /// <returns></returns>
        public static bool IsTouchEnabled()
        {
            // return true; // Use this to simulate touch in the Editor.
            return Input.touchSupported;
        }

        /// <summary>
        /// Asks the InputMatrix whether this object should receive input or not.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsValidSelectionTarget(GameObject obj)
        {
            return UIInputMatrix.Instance.ShouldReceiveInput(obj);
        }

        public void Awake()
        {
            Application.targetFrameRate = 60;
#if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = true; // Disable logging in builds.
#endif

            // Memorize the main scene
            mainScene = SceneManager.GetActiveScene();

            // Configure the input matrix singleton. (see class description or manual for details)
            var inputMatrix = UIInputMatrix.Instance;
            UIInputMatrix.Init(inputMatrix);
            //inputMatrix.LogEnabled = true; // Enable logging for debugging

            // The loading screen is visible by default, thus we need to add it to the matrix manually.
            inputMatrix.Push(LoadingScreen.GetUIStack(), LoadingScreen);

            // Our global UI manager
            UIManager = new UIManager(LoadingScreen);
            LoadingScreen.Initialize();

            // Configure the UISelectionHelperRoot singleton.
            UISelectionHelperRoot.Instance.Enabled = !Input.mousePresent && !IsTouchEnabled(); // disable if mouse or touch
            UISelectionHelperRoot.Instance.CustomIsValidFunc = IsValidSelectionTarget;
            UISelectionHelperRoot.Instance.AutoFixInvalidSelection = !IsTouchEnabled();
        }

        public async void Start()
        {
            bool success = loadAndSaveGame();
            if (!success)
                return;

            // Do your own init stuff here ...

            // load the menus scene and setup the menus
            bool menusLoaded = await UIManager.LoadMenusFromScene(coroutineRunner: this);
            if (menusLoaded)
            {
                UIManager.InitializeAll();
            }
            else
            {
                LoadingScreen.SetText(UIManager.GetLastLoadError());
                return;
            }

            // Our global game instance
            Game = new Game(UIManager, BikeAndCharacterPrefab);

            // Register listeners
            UIManager.GetUI<UIMainMenu>().OnLoadLevel += loadLevel;
            Game.OnUnloadLevel += unloadLevel;
            Game.OnLevelFinished += onLevelFinished;

            // all good: hide loading screen and show main menu
            UIManager.HideAllImmediate(hideLoadingScreen: false);
            UIManager.HideLoadingScreen();
            UIManager.Show<UIMainMenu>();
        }

        protected bool loadAndSaveGame()
        {
            // Load savegame
            var saveGame = SaveGameProvider.LoadIfNotYetLoaded();
            if (saveGame == null)
            {
                UIManager.SetLoadingScreenText("Huston we have a problem! - SaveGame is null.");
                return false;
            }

            // Write the savegame to be sure that writing it is working too.
            bool savedSuccess = SaveGameProvider.Save();
            if (!savedSuccess)
            {
                UIManager.SetLoadingScreenText("Huston we have a problem! - SaveGame could not be saved.");
                return false;
            }

            return true;
        }

        async void loadLevel(int levelNr)
        {
            var tokenSource = new System.Threading.CancellationTokenSource();
            var ct = tokenSource.Token;
            try
            {
                var levelData = LevelDataProvider.GetLevelData(levelNr);
                if (levelData == null)
                    return;

                bool isUnlocked = SaveGameProvider.SaveGame.IsLevelUnlocked(levelNr);
                if (!isUnlocked)
                    return;

                if (!MultiSceneLoader.IsSceneInBuild(levelData.SceneName))
                {
#if UNITY_EDITOR
                    Debug.LogError("Scene " + levelData.SceneName + " not in build.");
#endif
                    return;
                }

                // Show loading screen
                UIManager.HideImmediate<UIMainMenu>();
                UIManager.ShowLoadingScreenImmediate();

                // Load level
                var loader = MultiSceneLoader.LoadSceneAsync(this, levelData.SceneName, LoadSceneMode.Additive);
                while (!loader.IsDone)
                    await Task.Yield();

                // Make level the active scene
                var levelScene = SceneManager.GetSceneByName(levelData.SceneName);
                SceneManager.SetActiveScene(levelScene);

                currentLevel = MultiSceneLoader.GetRoot<ILevel>(levelData.SceneName);

                // Wait for level to initialize
                currentLevel.InitAfterLoad();
                while (!currentLevel.IsReady())
                    await Task.Yield();

                // Swap cameras
                MainCamera.gameObject.SetActive(false);
                currentLevel.GetCamera().gameObject.SetActive(true);

                UIManager.HideLoadingScreen();

                // start game
                Game.InitAfterLevelLoad(currentLevel, levelData);
            }
            finally
            {
                tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Called immediately after the player finishes a level (user is not back in menu yet).
        /// </summary>
        /// <param name="win">Did the player win the race?</param>
        /// <param name="levelData">What level was it?</param>
        void onLevelFinished(bool win, LevelData levelData)
        {
            if (win)
            {
                // Mark level completed if won
                var saveGameLevelData = SaveGameProvider.SaveGame.GetLevelData(levelData.LevelNr);
                bool firstWin = !saveGameLevelData.Completed;
                saveGameLevelData.Completed = true;

                SaveGameProvider.Save();

                // If the last level was finished then schedule a game finished
                // message to be shown when the player returns to the menu.
                if (firstWin && LevelDataProvider.IsLastLevel(levelData.LevelNr))
                {
                    showGameFinishedMsg = true;
                }
            }
        }

        /// <summary>
        /// Unloads the current level and shows menu afterwards.
        /// </summary>
        /// <param name="levelNr">LevelNr of the current level (this level will be unloaded).</param>
        async void unloadLevel(int levelNr)
        {
            var tokenSource = new System.Threading.CancellationTokenSource();
            var ct = tokenSource.Token;
            try
            {
                var levelData = LevelDataProvider.GetLevelData(levelNr);
                if (levelData == null)
                    return;

                UIManager.ShowLoadingScreenImmediate();
            
                // Unload level
                var loader = MultiSceneLoader.UnloadSceneAsync(this, levelData.SceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                while (!loader.IsDone)
                    await Task.Yield();

                // reset active scene to main scene
                SceneManager.SetActiveScene(mainScene);

                // forget about the level scene
                currentLevel = null;
                MainCamera.gameObject.SetActive(true);

                Game.CleanUpAfterLevelUnload();

                // show menu
                UIManager.HideLoadingScreen();
                UIManager.Show<UIMainMenu>();

                // show game finished message if necessary
                if (showGameFinishedMsg)
                {
                    showGameFinishedMsg = false;
                    await Task.Delay(1000);
                    await UIDialogAlertAsync.Spawn("Congratulations!\nYou beat the game.", ct);
                }
            }
            finally
            {
                tokenSource.Cancel();
            }
        }

        void Update()
        {
            // The game itself is no MonoBehaviour, thus we have to pump the loop manually.
            if (Game != null)
                Game.Update();

            updateSelectionHelper();
        }

        void updateSelectionHelper()
        {
            // Disable selection helper if the mouse is used
            if (MouseMoveDetector.Instance.MouseDelta.sqrMagnitude > 5f && UISelectionHelperRoot.Instance.Enabled)
            {
                UISelectionHelperRoot.Instance.RequestSelection(null, 999);
                UISelectionHelperRoot.Instance.Enabled = false;
            }

            // Enable selection helper is keyboard is used.
            bool anyMouseButtonDown = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
            if (Input.anyKey && !anyMouseButtonDown && !UISelectionHelperRoot.Instance.Enabled)
            {
                UISelectionHelperRoot.Instance.Enabled = true;
                UISelectionHelperRoot.Instance.FixSelection();
            }
        }

        void LateUpdate()
        {
            // The game itself is no MonoBehaviour, thus we have to pump the loop manually.
            if (Game != null)
                Game.LateUpdate();
        }

        private void FixedUpdate()
        {
            // The game itself is no MonoBehaviour, thus we have to pump the loop manually.
            if (Game != null)
                Game.FixedUpdate();
        }
    }
}
