using UnityEngine;
using System;
using System.Collections.Generic;


#if USE_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private bool isShowingAppOpen = false;


        public Action<bool> OnAppOpenLoad;

#if USE_ADMOB
        private AppOpenAd openAd;
#endif


        #region ADMOB_OPEN_AD
        private int openAdsRetry;
        public void LoadAdOpen(bool reset = true)
        {
            if (RemoveAds || DisabledAds)
            {
                return;
            }
            if (reset)
            {
                openAdsRetry = 0;
            }
            if (!gameLoaded)
            {
                loadOpenAd = false;
                return;
            }

#if USE_ADMOB
            string adUnitId = "unexpected_platform";
            int totalId = 0;
#if UNITY_ANDROID
            if (openad_android.Count <= openAdsRetry) return;
            adUnitId = openad_android[openAdsRetry];
            totalId = openad_android.Count;
#elif UNITY_IOS
        if (openad_ios.Count <= openAdsRetry) return;
        adUnitId = openad_ios[openAdsRetry];
        totalId = openad_ios.Count;
#endif
#if USE_TRIP
        if (localOpenAd.Count > openAdsRetry && !string.IsNullOrEmpty(localOpenAd[openAdsRetry]))
        {
            adUnitId = localOpenAd[openAdsRetry];
        }
#endif

            // Clean up the old ad before loading a new one.
            if (openAd != null)
            {
                openAd.Destroy();
                openAd = null;
            }

            if (logDebug)
                Debug.Log(logPrefix + "Admob openads load");
            FirebaseLogger.OpenadsCall();

            var adRequest = new AdRequest();

            AppOpenAd.Load(adUnitId.Trim(), adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        if (logDebug)
                            Debug.Log(logPrefix + "Admob load openads fail, reason: " + error.GetMessage());
                        FirebaseLogger.OpenadsFail(error.GetCode());
                        if (openAdsRetry < totalId - 1)
                        {
                            openAdsRetry++;
                            LoadAdOpen(false);
                        }
                        else
                        {
                            loadOpenAd = false;
                        }
                        return;
                    }

                    if (logDebug)
                        Debug.Log(logPrefix + "Admob load openads success");
                    FirebaseLogger.OpenadsReady();
                    openAd = ad;
                    RegisterEventHandlers(ad);
                });
#endif
        }

#if USE_ADMOB
        private void RegisterEventHandlers(AppOpenAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                ExecuteAction(() =>
                {
                    isShowingAppOpen = true;

                    if (logDebug)
                        Debug.Log(logPrefix + "Admob show openads success");
                    FirebaseLogger.OpenadsShowSuccess();
                });
            };
            ad.OnAdFullScreenContentClosed += () =>
            {
                ExecuteAction(() =>
                {
                    if (logDebug)
                        Debug.Log(logPrefix + "Admob openads close");

                    openAd = null;
                    isShowingAppOpen = false;

                    if (preloadAdOpen) LoadAdOpen();
                    else OnAppOpenLoad?.Invoke(false);
                });
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                ExecuteAction(() =>
                {
                    FirebaseLogger.OpenadsShowFail(error.GetCode());
                    if (logDebug)
                        Debug.LogFormat(logPrefix + "Admob show openads fail, reason: " + error.GetMessage());
                    openAd = null;

                    if (preloadAdOpen) LoadAdOpen();
                    else OnAppOpenLoad?.Invoke(false);
                });
            };
            ad.OnAdPaid += OnAdPaid;
        }
#endif

        [HideInInspector] public bool loadOpenAd = false;
#if USE_ADMOB
        private void OnAppStateChanged(AppState state)
        {
            if (gameLoaded && state == AppState.Foreground)
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Appstate foreground, ShowAds: " + ShowAds + ", loadOpenAd: " + loadOpenAd);
                if (RemoveAds || DisabledAds || (loadOpenAd && !preloadAdOpen) || isShowingAppOpen || ShowAds)
                {
                    return;
                }
                if (hideApp)
                {
                    hideApp = false;
                    return;
                }

                if (UseOpenAd)
                {
                    if (preloadAdOpen)
                    {
                        ShowAdmobOpenAds();
                    }
                    else
                    {
                        loadOpenAd = true;
                        OnAppOpenLoad?.Invoke(true);
                        LoadAdOpen();
                    }
                }
            }
        }
#endif


        private bool IsAppOpenAvailable
        {
            get
            {
#if USE_ADMOB
                return openAd != null && openAd.CanShowAd();
#else
            return false;
#endif
            }
        }

        public bool OpenAdsReady()
        {
#if USE_ADMOB
            if (UseOpenAd)
            {
                return IsAppOpenAvailable;
            }
            else return false;
#else
        return false;
#endif
        }

        public void ShowAdmobOpenAds()
        {
            if (logDebug)
                Debug.Log(logPrefix + "Admob show open ads: " + preloadAdOpen);
            FirebaseLogger.OpenadsShow();
#if USE_ADMOB
            if (RemoveAds || DisabledAds)
            {
                return;
            }
            if (openAd != null && openAd.CanShowAd())
            {
                openAd.Show();
            }
            else
            {
                FirebaseLogger.OpenadsNotReady();
                if (preloadAdOpen) LoadAdOpen();
            }
#endif
        }


        public void SetOpenAdId(string adUnit)
        {
#if UNITY_IOS
            if (openad_ios.Count > 0)
            {
                openad_ios[0] = adUnit;
            }
            else
            {
                openad_ios = new List<string> { adUnit };
            }
#else
            if (openad_android.Count > 0)
            {
                openad_android[0] = adUnit;
            }
            else
            {
                openad_android = new List<string> { adUnit };
            }
#endif
        }
        #endregion
    }
}