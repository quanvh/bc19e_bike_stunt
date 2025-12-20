using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;


#if USE_FACEBOOK
using Facebook.Unity;
#endif

namespace Bacon
{
    public class RouteController : MonoBehaviour
    {
        public static RouteController Instance = null;

        [SerializeField] private string garageScene = "1_GarageScene";
        [SerializeField] private string raceScene = "2_RaceScene";

        [HideInInspector] public float goldWinLevel = 0;

        [Header("LANGUAGE")]
        public bool ShowLanguage = true;
        public bool UseMrecLanguage = true;

        [Header("AGE")]
        public bool ShowAge = true;

        [Header("DEBUG")]
        public bool logDebug;

        [HideInInspector] public THEME_NAME CurrentTheme;

        private Version CurrentVersion;

        private bool RemoveAds => AdsController.Instance.RemoveAds;

        private bool DisabledAds => AdsController.Instance.DisabledAds;

        private bool UseAdmobBanner => AdsController.Instance.UseAdmobBanner;

        private bool UseNativeBanner => !AdsController.Instance.useSmallBanner;


        private bool sceneLoaded = false;
        private bool checkedUpdate = false;
        private bool checkNetwork = false;
        public bool InitDone => sceneLoaded && checkedUpdate;

        public int WatchAdsFree;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            CurrentVersion = new Version(Application.version);

            WatchAdsFree = 0;

            StartCoroutine(StartGame());

            ScoreText.OnEmitComplete += OnScoreComplete;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                checkNetwork = true;
            }
        }

        private void OnDestroy()
        {
            ScoreText.OnEmitComplete -= OnScoreComplete;
        }

        private bool _nativebannerShowing;
        public bool ShowNativeBanner
        {
            get { return _nativebannerShowing; }
            set
            {
                _nativebannerShowing = value;
                if (RemoveAds || DisabledAds || !UseAdmobBanner || Application.internetReachability == NetworkReachability.NotReachable)
                {
                    if (bannerAd != null)
                        bannerAd.gameObject.SetActive(false);
                }
                else
                {
                    if (bannerAd != null)
                        bannerAd.gameObject.SetActive(_nativebannerShowing);
                }
            }
        }

        private float _timeStartBanner;
        private void Update()
        {
            if (checkNetwork)
            {
                if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork
                    || Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    checkNetwork = false;
                    StartCoroutine(UMP.Instance.DOGatherConsent(InitGoogleAds()));
                }
            }

            if (Application.internetReachability != NetworkReachability.NotReachable &&
                AdsController.Instance && AdsController.Instance.gameLoaded)
            {
                if (UseNativeBanner && _nativebannerShowing &&
                    Time.time - _timeStartBanner >= DataManager.Instance.Remote.TimeLoadNative)
                {
                    ShowNativeDefault();
                }
                else if (UseAdmobBanner &&
                    Time.time - _timeStartBanner >= DataManager.Instance.Remote.TimeLoadBanner)
                {
                    ShowSmallBanner();
                }
            }
        }


        [SerializeField] AdmobNativeUI bannerAd;
        public void ShowNativeDefault()
        {
            if (bannerAd == null) return;
            _timeStartBanner = Time.time;
            if (RemoveAds || DisabledAds || !UseAdmobBanner)
            {
                return;
            }

            AdsController.Instance.RequestNative(NativeName.Native_Banner, bannerAd);
        }

        public void ShowSmallBanner()
        {
            _timeStartBanner = Time.time;
            if (RemoveAds || DisabledAds || !UseAdmobBanner)
            {
                return;
            }
            if (AdsController.Instance && !AdsController.Instance.smallBannerLoading)
            {
                AdsController.Instance.LoadSmallBanner();
            }
        }

        private void OnScoreComplete()
        {
            goldWinLevel = 0;
        }


        private readonly float timeWaitRemote = 2.5f;
        protected IEnumerator StartGame()
        {
            yield return DataManager.Instance.DoLoadData();

            yield return DataManager.Instance.DetectCountry();

            yield return new WaitForSeconds(0.1f);

            yield return DoInitFirebase();

            SceneHelper.LoadScene(garageScene, (isDone) =>
            {
                sceneLoaded = isDone;
            });


            yield return new WaitForSeconds(timeWaitRemote);

            CheckUpdate();

            yield return new WaitForSeconds(0.1f);

            AdsController.Instance.InitIronsource();
        }

        IEnumerator DoInitFirebase()
        {
            if (FirebaseManager.Instance)
            {
                FirebaseManager.Instance.InitFirebase(CheckUpdate);
            }
            yield return null;
        }

        private bool checkingUpdate = false;
        private void CheckUpdate()
        {
            if (checkingUpdate) return;

            checkingUpdate = true;
            if (ForceUpdate())
            {
                UIManager.Instance.ShowPopup<UIUpdate>();
            }
            else
            {
                checkedUpdate = true;
                StartCoroutine(UMP.Instance.DOGatherConsent(InitAdmob()));
            }
        }

        private IEnumerator InitAdmob()
        {
            yield return new WaitForSeconds(0.1f);
            AdsController.Instance.InitSplashInter();

            yield return new WaitForSeconds(0.1f);
            InitFacebook();
        }

        private IEnumerator InitGoogleAds()
        {
            yield return new WaitForSeconds(0.1f);
            AdsController.Instance.InitGoogleAds();
        }

        private bool ForceUpdate()
        {
            if (DataManager.Instance)
            {
                var minVersion = new Version(DataManager.Instance._player.remoteConfig.Version);
                if (logDebug)
                {
                    Debug.Log("Local version: " + CurrentVersion.Major + " - " + CurrentVersion.Minor
                        + " - " + CurrentVersion.Build + " - " + CurrentVersion.Revision
                        + ";\nMin version: " + minVersion.Major + " - " + minVersion.Minor
                        + " - " + minVersion.Build + " - " + minVersion.Revision);
                }
                return CurrentVersion < minVersion;
            }
            return false;
        }


        public void GetUserInfo()
        {
            if (ShowLanguage && !DataManager.Instance._player.chooseLang)
            {
                UIManager.Instance.ShowPopup<UILanguage>();
            }
            else if (ShowAge && !DataManager.Instance._player.chooseAge)
            {
                UIManager.Instance.ShowPopup<UISelectAge>();
            }
            else if (DataManager.Instance.LoadAdsOpen())
            {
                UIManager.Instance.ShowPopup<UIRemoveAds>();
            }
            else
            {
                InitGame();
            }
        }



        public void InitGame()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.gameLoaded = true;
                if (UseNativeBanner)
                {
                    _nativebannerShowing = true;
                    //ShowNativeDefault();
                }
                else
                {
                    //ShowSmallBanner();
                }
            }

            //Check to show garage or start level imidiate
            StartGarage();
        }

        private bool coroutineRuning;
        public void PrepareGarage()
        {
            if (coroutineRuning) return;

            coroutineRuning = true;
            StartCoroutine(GarageCoroutine());
        }

        private IEnumerator GarageCoroutine()
        {
            UIManager.Instance.ShowPopup<UILoading>();
            LevelController.Instance.ClearBike();

            yield return SceneHelper.DOLoadScene(garageScene, null);
            coroutineRuning = false;

            yield return new WaitForSeconds(0.3f);
            StartGarage();
        }

        private void StartGarage()
        {
            UIManager.Instance.ShowPopup<UIGarage>();
            AudioController.Instance.ScaleMusic(1.0f);
        }

        public void StartLevel()
        {
            if (coroutineRuning) return;

            coroutineRuning = true;
            StartCoroutine(StartLevelCoroutine());
        }

        IEnumerator StartLevelCoroutine()
        {
            UIManager.Instance.ShowPopup<UILoading>();
            LevelController.Instance.ClearBike();
            AudioController.Instance.ScaleMusic(0.5f);

            yield return SceneHelper.DOLoadScene(raceScene, null);

            yield return LevelController.Instance.StartLevel();
            coroutineRuning = false;
        }

        private void InitFacebook()
        {
#if USE_FACEBOOK
            if (!FB.IsInitialized)
            {
                FB.Init(() =>
                {
                    if (logDebug)
                        Debug.Log("[Facebook] Init facebook Completed");
                }, (showGame) => { });
            }
            else FB.ActivateApp();
#endif
        }

    }
}