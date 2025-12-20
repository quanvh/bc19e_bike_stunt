using System;
using UnityEngine;

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private bool loadAllAds;
        private Action IrsInitSuccess;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            firstStart = true;
            loadAllAds = false;
            adCount = 0;
            gameLoaded = false;
            ShowAds = false;

            lastAdTime = DateTime.Now;

            //if (!Debug.isDebugBuild) logDebug = false;

#if USE_ADVERTY
        if (adverty_autoInit)
            StartCoroutine(InitAdverty());
#endif

#if USE_TRIP
        LoadAdConfig();
#endif

#if USE_ADMOB
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            ScreenScale = MobileAds.Utils.GetDeviceScale();
            ScreenWidth = MobileAds.Utils.GetDeviceSafeWidth();
            if (ScreenScale == 0) ScreenScale = 2.8125f;
            if (ScreenWidth == 0) ScreenWidth = 384;
#endif

#if USE_ANDROID_NATIVE
        InitAdsAndroid();
#endif
        }

        private void OnDestroy()
        {
#if USE_ADMOB
            AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
#endif
        }

        #region EXTERNAL_FUNTION
        public bool IsInterReady()
        {
            switch (Mediation)
            {
                case AD_MEDIATION.IRON:
#if USE_IRON
#if USE_ADMOB
                    return IrsInterReady() || (UseAdmobInter && AdmobInterReady(InterName.Inter_Default));
#else
            return IrsInterReady();
#endif
#else
#if USE_ADMOB
                    return UseAdmobInter && AdmobInterReady(InterName.Inter_Default);
#else
        return false;
#endif
#endif
                case AD_MEDIATION.ADMOB:
                    return AdmobInterReady(InterName.Inter_Default);
                case AD_MEDIATION.MAX:
                    return MaxInterReady();
                case AD_MEDIATION.NONE: return false;
                default: return false;
            }
        }

        public void ShowInter(string place = "", Action onSuccess = null)
        {
            interPlacement = place;
            if (RemoveAds)
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Game controller not establish or removed ads");
                onSuccess?.Invoke();
                return;
            }
            if (DisabledAds)
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Remote config not show interstitial");
                onSuccess?.Invoke();
                return;
            }

            if (IsDuration())
            {
                if (Mediation == AD_MEDIATION.IRON)
                {
                    FirebaseLogger.InterShow(ironsrc, place, CurrentLevel, CurrentMode);
                    if (IrsInterReady()) ShowIrsInter(onSuccess);
                    else
                    {
                        FirebaseLogger.InterShowNotReady(ironsrc, place, CurrentLevel, CurrentMode);
                        LoadIrsInter();
                        if (UseAdmobInter) ShowAdmobInter(onSuccess);
                        else
                        {
                            if (logDebug)
                                Debug.Log(logPrefix + "Don't have interstitial, don't use admob inter");
                            onSuccess?.Invoke();
                        }
                    }
                }
                else if (Mediation == AD_MEDIATION.MAX)
                {
                    ShowMaxInter(onSuccess);
                }
                else if (Mediation == AD_MEDIATION.ADMOB)
                {
                    ShowAdmobInter(onSuccess);
                }
                else
                {
                    onSuccess?.Invoke();
                }
            }
            else
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Time less than duration config");
                onSuccess?.Invoke();
            }
        }

        public bool IsRewardReady()
        {
            switch (Mediation)
            {
                case AD_MEDIATION.IRON:
#if USE_IRON
#if USE_ADMOB
                    return IrsRewardReady() || (UseAdmobReward && AdmobRewardReady());
#else
            return IrsRewardReady();
#endif
#else
#if USE_ADMOB
                    return UseAdmobReward && AdmobRewardReady();
#else
        return false;
#endif
#endif
                case AD_MEDIATION.ADMOB:
                    return AdmobRewardReady();
                case AD_MEDIATION.MAX:
                    return MaxRewardReady();
                case AD_MEDIATION.NONE:
                    return false;
                default: return false;
            }
        }


        private ADTYPE currentAdType = ADTYPE.NONE;
        public void ShowReward(ADTYPE _adtype, Action OnSuccess = null)
        {
            currentAdType = _adtype;
            rewardPlacement = currentAdType.ToString();
            //#if UNITY_EDITOR
            //            OnRewardComplete?.Invoke(true, currentAdType);
            //            return;
            //#endif

            switch (Mediation)
            {
                case AD_MEDIATION.IRON:
#if USE_IRON
                    FirebaseLogger.RewardShow(ironsrc, rewardPlacement, CurrentLevel, CurrentMode);
                    if (IrsRewardReady())
                    {
                        isShowingAds = true;
                        ShowIrsReward(OnSuccess);
                    }
                    else
                    {
                        // LoadIrsReward();
                        FirebaseLogger.RewardShowNotReady(ironsrc, rewardPlacement, CurrentLevel, CurrentMode);
                        if (UseAdmobReward) ShowAdmobReward(OnSuccess);
                        else OnRewardNotReady?.Invoke();
                    }
#endif
                    break;
                case AD_MEDIATION.ADMOB:
                    ShowAdmobReward(OnSuccess);
                    break;
                case AD_MEDIATION.MAX:
                    ShowMaxReward(OnSuccess);
                    break;
                default:
                    break;
            }

        }

        public void DisplayBanner(bool active)
        {
            if (DisabledAds || RemoveAds)
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Remote config dont show add ads");
                return;
            }
            switch (Mediation)
            {
                case AD_MEDIATION.IRON:
                    if (UseAdmobBanner)
                    {
                        if (!active) HideSmallBanner();
                        else ShowSmallBanner();
                    }
                    else
                    {
                        if (!active) HideIrsBanner();
                        else ShowIrsBanner();
                    }
                    break;
                case AD_MEDIATION.ADMOB:
                    if (!active) HideSmallBanner();
                    else ShowSmallBanner();
                    break;
                case AD_MEDIATION.MAX:
                    if (!active) HideMaxBanner();
                    else ShowMaxBanner();
                    break;
                default: break;
            }

        }

        #endregion


#if USE_ANDROID_NATIVE
    private AndroidJavaObject _unityActivity;
    private AndroidJavaClass _nativeAdsManagerClass;
    public AndroidJavaObject _nativeAdsManager;

    private void InitAdsAndroid()
    {
#if !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (_unityActivity == null)
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Failed to get current Unity activity.");
                return;
            }

            _nativeAdsManagerClass = new AndroidJavaClass("com.bacon.unityadmobnative.Core.NativeAdsManager");

            if (_nativeAdsManagerClass == null)
            {
                if (logDebug)
                    Debug.LogError(logPrefix + "Can't get NativeAdsManagerClass.");
                return;
            }

            _nativeAdsManager = _nativeAdsManagerClass.CallStatic<AndroidJavaObject>("getInstance");
            if (_nativeAdsManagerClass == null)
            {
                if (logDebug)
                    Debug.LogError(logPrefix + "Failed to initialize NativeAdsManager.");
                return;
            }
            _nativeAdsManager.Call("initializeSdk", _unityActivity);
            if (logDebug)
                Debug.Log(logPrefix + $"Native ads manager initialized!");
#endif
    }
#endif
    }
}