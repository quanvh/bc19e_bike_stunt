using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {

        public Action OnInterFailed;

        public Action OnInterLoaded;

        public bool PreloadInterDefault
        {
            get
            {
                AdmobInter _inter = InterAds.Where(t => t.interName == InterName.Inter_Default).First();
                if (_inter != null)
                {
                    return _inter.preload;
                }
                else return false;
            }
        }

        public void LoadInterSuccess(InterName _name)
        {
            if (_name == InterName.Inter_Open)
            {
                adCount++;
                if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
                LoadAdmobAds();
            }
            OnInterLoaded?.Invoke();
        }

        public void LoadInterFail(InterName _name)
        {
            if (_name == InterName.Inter_Open)
            {
                adCount++;
                if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
                LoadAdmobAds();
            }
            OnInterFailed?.Invoke();
        }

        public void ShowAdmobInter(Action onSuccess = null, InterName _name = InterName.Inter_Default)
        {
            if (RemoveAds || DisabledAds || !UseAdmobInter)
            {
                onSuccess?.Invoke();
                return;
            }
            AdmobInter _inter = InterAds.Where(t => t.interName == _name).First();
            _inter?.ShowAdmobInter(() =>
            {
                lastAdTime = DateTime.Now;
                onSuccess?.Invoke();
            }, interPlacement);
        }

        public void LoadAdmobInter(InterName _name)
        {
            if (!RemoveAds && !DisabledAds && UseAdmobInter)
            {
                AdmobInter _inter = InterAds.Where(t => t.interName == _name).First();
                if (_inter != null && !_inter.isLoading)
                {
                    if (UseNative && _inter.useNative)
                    {
                        _inter.LoadAndroidInter();
                    }
                    else
                    {
                        _inter.LoadAdmobInter();
                    }
                }
            }
        }

        private void PreloadAdmobInter(InterName _name)
        {
            if (!RemoveAds && !DisabledAds && UseAdmobInter)
            {
                AdmobInter _inter = InterAds.Where(t => t.interName == _name).First();
                if (_inter != null && !_inter.isLoading && _inter.preload)
                {
                    if (UseNative && _inter.useNative)
                    {
                        _inter.LoadAndroidInter();
                    }
                    else
                    {
                        _inter.LoadAdmobInter();
                    }
                }
            }
        }
    }

    [Serializable]
    public class AdmobInter
    {
        [FoldoutGroup("@interName", expanded: false), Space] public InterName interName;
        [FoldoutGroup("@interName"), Space] public bool preload;
        [FoldoutGroup("@interName"), Space] public bool useNative;
        [FoldoutGroup("@interName"), SerializeField, Space] private List<string> inter_android;
        [FoldoutGroup("@interName"), SerializeField, Space] private List<string> inter_ios;


        [FoldoutGroup("@interName"), SerializeField, Space] private bool cappingFail = false;
        [FoldoutGroup("@interName"), SerializeField, Space, ShowIf("cappingFail")] private float cappingTime = 30;

        private int failCount = 0;

        private string Inter_name => interName.ToString();
        private readonly string logPrefix = "[AdsController] ";

        private string currentPlacement = "start";
        private DateTime timeLoad = DateTime.MinValue;

        private bool LogDebug => AdsController.Instance.logDebug;

        public event Action OnInterComplete;

#if USE_ADMOB
        private InterstitialAd inter;
#endif

        private bool RemoveAds => AdsController.Instance.RemoveAds;

        private bool DisabledAds => AdsController.Instance.DisabledAds;

        private bool UseAdmobInter => AdsController.Instance.UseAdmobInter;

        private int CurrentLevel => AdsController.Instance.CurrentLevel;

        private int CurrentMode => AdsController.Instance.CurrentMode;

        public void SetAdUnit(string adUnit, int _index = 0)
        {
#if UNITY_IOS
            if (inter_ios.Count > _index)
            {
                inter_ios[_index] = adUnit;
            }
            else
            {
                inter_ios.Add(adUnit);
            }
#else
            if (inter_android.Count > _index)
            {
                inter_android[_index] = adUnit;
            }
            else
            {
                inter_android.Add(adUnit);
            }
#endif
        }

        public bool IsReady()
        {
#if USE_ADMOB
            return inter != null && inter.CanShowAd();
#else
        return false;
#endif
        }

        public void ShowAdmobInter(Action onSuccess = null, string place = "")
        {
            if (RemoveAds || DisabledAds || !UseAdmobInter)
            {
                onSuccess?.Invoke();
                return;
            }
            if (!string.IsNullOrEmpty(place))
            {
                currentPlacement = place;
            }
            else
            {
                currentPlacement = Inter_name;
            }
#if USE_ADMOB
            FirebaseLogger.InterShow(Inter_name, currentPlacement, CurrentLevel, CurrentMode);
            if (AdsController.Instance.UseNative && useNative)
            {
                if (IsIntersAndroidReady)
                {
                    AdsController.Instance.MuteGameSound();
                    _naAd.ShowNativeAds();
                    OnInterComplete = onSuccess;
                }
                else
                {
                    onSuccess?.Invoke();
                    if (preload) LoadAndroidInter();
                }
            }
            else
            {
                if (inter != null && inter.CanShowAd())
                {
                    AdsController.Instance.MuteGameSound();
                    inter.Show();
                    OnInterComplete = onSuccess;
                }
                else
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Don't have interstitial, use admob - don't use reward");
                    FirebaseLogger.InterShowNotReady(Inter_name, currentPlacement, CurrentLevel, CurrentMode);

                    onSuccess?.Invoke();
                    if (preload) LoadAdmobInter();
                }
            }
#else
            onSuccess?.Invoke();
#endif
        }

        private int interRetry;
        [HideInInspector] public bool isLoading = false;
        [HideInInspector] public bool isReady = false;
        public bool IsIntersAndroidReady => isReady;
        public void LoadAdmobInter(bool reset = true)
        {
            if (RemoveAds || DisabledAds || !UseAdmobInter)
            {
                return;
            }

            if (reset)
            {
                interRetry = 0;
            }

#if USE_ADMOB
            int totalId = 0;
            string adUnitId = "unexpected_platform";
#if UNITY_ANDROID
            if (inter_android.Count <= interRetry) return;
            adUnitId = inter_android[interRetry];
            totalId = inter_android.Count;
#elif UNITY_IOS
            if (inter_ios.Count <= interRetry) return;
            adUnitId = inter_ios[interRetry];
            totalId = inter_ios.Count;
#endif

            if (cappingFail && failCount >= totalId)
            {
                if ((DateTime.Now - timeLoad).TotalSeconds > cappingTime)
                {
                    failCount = 0;
                }
                else
                {
                    return;
                }
            }

            isLoading = true;

            if (inter != null)
            {
                inter.Destroy();
                inter = null;
            }

            if (LogDebug)
                Debug.Log(logPrefix + "Admob " + Inter_name + " load: " + adUnitId);
            FirebaseLogger.InterCall(Inter_name);
            timeLoad = DateTime.Now;

            var adRequest = new AdRequest();

            InterstitialAd.Load(adUnitId.Trim(), adRequest,
             (InterstitialAd ad, LoadAdError error) =>
             {
                 // if error is not null, the load request failed.
                 if (error != null || ad == null)
                 {
                     if (LogDebug)
                         Debug.Log(logPrefix + "Admob " + Inter_name + " load fail: " + error.GetMessage());
                     FirebaseLogger.InterCallFail(Inter_name, error.GetCode());

                     failCount++;
                     if (interRetry < totalId - 1)
                     {
                         interRetry++;
                         LoadAdmobInter(false);
                     }
                     else
                     {
                         AdsController.Instance.LoadInterFail(interName);
                         isLoading = false;
                     }
                     return;
                 }

                 failCount = 0;
                 inter = ad;
                 if (LogDebug)
                     Debug.Log(logPrefix + "Admob " + Inter_name + " load success: " + (DateTime.Now - timeLoad).Seconds);
                 FirebaseLogger.InterReady(Inter_name);
                 AdsController.Instance.LoadInterSuccess(interName);

                 RegisterEventHandlers(inter);
                 isLoading = false;
             });
#endif
        }

#if USE_ADMOB

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Inter_name + " open");

                    AdsController.Instance.MuteGameSound();
                });
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Inter_name + " close");
                    FirebaseLogger.InterShowSuccess(Inter_name, currentPlacement, CurrentLevel, CurrentMode);

                    OnInterComplete?.Invoke();
                    OnInterComplete = null;

                    AdsController.Instance.OpenGameSound();
                    if (preload) LoadAdmobInter();
                });
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Inter_name + " show fail, error: " + error.GetMessage());
                    FirebaseLogger.InterShowFail(Inter_name, currentPlacement, CurrentLevel, CurrentMode, error.GetCode());

                    AdsController.Instance.OpenGameSound();
                    if (preload) LoadAdmobInter();
                });
            };

            ad.OnAdPaid += AdsController.Instance.OnAdPaid;
        }
#endif

#if USE_ADMOB

        public NativeAdFullScreen _naAd;

        public void LoadAndroidInter()
        {
#if !UNITY_EDITOR
        isLoading = true;
        _naAd = CreateNativeAdFullScreenWithOutEndCard(
            inter_android[0],
            AdsController.Instance.TimeLeftNativeAds,
            AdsController.Instance.TimeVisibleClose,
            AdsController.Instance.TimeEnableClose);

        //_naAd.SetNativeAdsLayout("native_ad");

        // Call Back Native ads
        _naAd.NativeAdAdCallback.OnAdLoadedEvent += OnAdLoadedEvent;
        _naAd.NativeAdAdCallback.OnPaidAdImpressionEvent += OnPaidAdImpressionEvent;
        _naAd.NativeAdAdCallback.OnAdFailedToLoadEvent += OnAdFailedToLoadEvent;
        _naAd.NativeAdAdCallback.OnAdClickedEvent += OnAdClickedEvent;
        _naAd.NativeAdAdCallback.OnAdDisplayedEvent += OnAdDisplayedEvent;
        _naAd.NativeAdAdCallback.OnAdOpenedEvent += OnAdOpenedEvent;
        _naAd.NativeAdAdCallback.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
        _naAd.NativeAdAdCallback.OnAdSwipeGestureClickedEvent += OnAdSwipeGestureClickedEvent;

        // Close Event
        _naAd.AdCloseProxy.OnAdClosedEvent += OnAdClosedEvent;

        _naAd.LoadNativeAds();
#endif
        }

        private void OnAdSwipeGestureClickedEvent(string adUnit)
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdSwipeGestureClickedEvent!");
        }

        private void OnPaidAdImpressionEvent(NativeAdsInfo nativeAdsInfo)
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + $"OnPaidAdImpressionEvent: adUnit:{nativeAdsInfo.adUnit} | mediationAdapterName:{nativeAdsInfo.mediationAdapterName} | adValue:{nativeAdsInfo.adValue}");
        }

        private void OnAdFailedToLoadEvent(string adUnit, string errorMessage)
        {
            isLoading = false;
            isReady = false;

            AdsController.Instance.OnInterFailed?.Invoke();
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + $"OnAdFailedToLoadEvent: {errorMessage}");
        }

        private void OnAdDisplayFailedEvent(string adUnit, string errorMessage)
        {
            isReady = false;
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + $"OnAdDisplayFailedEvent: {errorMessage}");
        }

        private void OnAdClosedEvent()
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdClosedEvent");
            isReady = false;
            OnInterComplete?.Invoke();
            OnInterComplete = null;
            if (preload)
            {
                _naAd.LoadNativeAds();
            }
        }

        private void OnAdDisplayedEvent(string adUnit)
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdImpressionEvent");
        }

        private void OnAdOpenedEvent(string adUnit)
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdOpenedEvent");
        }

        private void OnAdLoadedEvent(string adUnit)
        {
            isLoading = false;
            isReady = true;
            AdsController.Instance.OnInterLoaded?.Invoke();

            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdLoadedEvent");
        }

        private void OnAdClickedEvent(string adUnit)
        {
            if (AdsController.Instance.logDebug)
                Debug.Log(logPrefix + "OnAdsClicked");
        }

        public NativeAdFullScreen CreateNativeAdFullScreenWithOutEndCard(string adUnitId, int timeLeftNativeAds, int timeVisibleClose, int timeEnableClose)
        {
            AdCallbackProxy adCallbackProxy = new AdCallbackProxy();
            AdCloseProxy adCloseProxy = new AdCloseProxy();
            AndroidJavaObject adFullScreen = AdsController.Instance._nativeAdsManager.Call<AndroidJavaObject>(
                "CreateNativeAdFullScreen",
                adUnitId,
                adCallbackProxy,
                adCloseProxy,
                timeLeftNativeAds,
                timeVisibleClose,
                timeEnableClose
            );
            NativeAdFullScreen nativeAdFullScreen = new NativeAdFullScreen
            {
                NativeAdFullScreenObject = adFullScreen,
                NativeAdAdCallback = adCallbackProxy,
                AdCloseProxy = adCloseProxy
            };
            return nativeAdFullScreen;
        }

#endif
    }

    public enum InterName
    {
        None = 0,
        Inter_Open = 1,
        Inter_Default = 2,
    }

}