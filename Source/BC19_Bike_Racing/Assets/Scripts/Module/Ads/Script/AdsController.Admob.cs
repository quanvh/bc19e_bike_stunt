using System;
using UnityEngine;
using System.Linq;


#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private bool UseInterOpen => UseAdmobInter && UseAdmobInterSplash && !RemoveAds && !DisabledAds && !UserOrganic;

        public void InitGoogleAds()
        {
            if (logDebug)
                Debug.Log(logPrefix + "Admob init");

#if USE_ADMOB
            MobileAds.SetiOSAppPauseOnBackground(iOSPause);
            MobileAds.RaiseAdEventsOnUnityMainThread = RaiseOnMainThread;
            if (Mediation != AD_MEDIATION.ADMOB && disableMediation)
            {
                MobileAds.DisableMediationInitialization();
            }
            MobileAds.Initialize(initStatus =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Admob init success");
                LoadAdmobAds();
            });
#endif
        }

        public void InitSplashInter()
        {
            if (logDebug)
                Debug.Log(logPrefix + "Admob first init");

#if USE_ADMOB
            MobileAds.SetiOSAppPauseOnBackground(iOSPause);
            MobileAds.RaiseAdEventsOnUnityMainThread = RaiseOnMainThread;
            if (Mediation != AD_MEDIATION.ADMOB && disableMediation)
            {
                MobileAds.DisableMediationInitialization();
            }
            MobileAds.Initialize(initStatus =>
            {
                if (logDebug)
                {
                    Debug.Log(logPrefix + "Admob first init success");

                    //Log mediation
                    var adapterStatusMap = initStatus.getAdapterStatusMap();
                    if (adapterStatusMap != null)
                    {
                        foreach (var item in adapterStatusMap)
                        {
                            Debug.Log(string.Format("Adapter {0} is {1}",
                                item.Key,
                                item.Value.InitializationState));
                        }
                    }
                }
                if (DataManager.Instance.LoadAdsOpen())
                {
                    if (useMrecOpen)
                    {
                        LoadMrec(MrecName.Mrec_Open);
                    }
                    else
                    {
                        if (!DataManager.Instance._player.chooseLang)
                        {
                            RequestNative(NativeName.Native_Language);
                        }
                        else
                        {
                            RequestNative(NativeName.Native_Age);
                        }
                    }
                }
                else adCount++;

                if (UseInterOpen)
                {
                    LoadAdmobInter(InterName.Inter_Open);
                }
                else
                {
                    LoadAdmobAds();
                    adCount++;
                    if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
                }
            });
#endif
        }


        public void LoadAdmobAds()
        {
#if USE_ADMOB
            if (loadAllAds) return;
            loadAllAds = true;

            if (!RemoveAds && !DisabledAds)
            {
                if (UseOpenAd && preloadAdOpen) LoadAdOpen();

                PreloadAdmobInter(InterName.Inter_Default);

                //if (UseAdmobMrec)
                //{
                //    LoadMrec(MrecName.Mrec_Default);
                //}

                if (UseAdmobBanner && useSmallBanner)
                {
                    LoadSmallBanner();
                }
            }

            if (UseAdmobReward) PreloadAdmobReward(RewardName.Reward_Default);
#endif
        }
        public bool AdmobInterReady(InterName _name)
        {
            AdmobInter _inter = InterAds.Where(t => t.interName == _name).First();
            return _inter != null && _inter.IsReady();

        }


        public void ShowInterSplash(Action _onComplete = null)
        {
            if (!UseInterOpen)
            {
                _onComplete?.Invoke();
                return;
            }
#if USE_ADMOB
            AdmobInter _inter = InterAds.Where(t => t.interName == InterName.Inter_Open).First();

            if (_inter != null)
            {
                _inter.ShowAdmobInter(_onComplete);
            }
            else
            {
                _onComplete?.Invoke();
            }
#else
            _onComplete?.Invoke();
#endif
        }


#if USE_ADMOB
        public void OnAdPaid(AdValue adValue)
        {
            ExecuteAction(() =>
            {
                FirebaseLogger.OnImpressAds(adValue);
            });
        }

        public void OnAdPaid(AdValue adValue, string adType = "")
        {
            ExecuteAction(() =>
            {
                FirebaseLogger.OnImpressAds(adValue, adType);
            });
        }
#endif
    }
}