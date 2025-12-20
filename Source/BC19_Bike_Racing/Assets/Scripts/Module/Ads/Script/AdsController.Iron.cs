using System;
using UnityEngine;


namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private DateTime timeLoadIrsInter;

        private void OnEnable()
        {
#if USE_IRON
            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionSuccessEvent;

            // ironsource Reward  Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent += OnRewardOpen;
            IronSourceRewardedVideoEvents.onAdClosedEvent += OnRewardClose;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += OnRewardAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += OnRewardUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardShowFail;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewardGrant;


            // ironsource Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent += OnInterReady;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += OnInterLoadFail;
            IronSourceInterstitialEvents.onAdOpenedEvent += OnInterOpen;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += OnInterShowSuccess;
            IronSourceInterstitialEvents.onAdShowFailedEvent += OnInterShowFail;
            IronSourceInterstitialEvents.onAdClosedEvent += OnInterClose;

            // ironsource Banner events
            IronSourceBannerEvents.onAdLoadedEvent += OnBannerLoad;
            IronSourceBannerEvents.onAdLoadFailedEvent += OnBannerFail;
#endif
        }

        #region IRONSOURCE_FUNCTION
        public void InitIronsource(Action _onComplete = null)
        {
            IrsInitSuccess = _onComplete;
#if USE_IRON
            //IronSource.Agent.validateIntegration();
            var developerSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
            if (developerSettings != null)
            {
#if UNITY_IOS
            string appKey = developerSettings.IOSAppKey;
#else
                string appKey = developerSettings.AndroidAppKey;
#endif
                if (logDebug)
                    Debug.Log(logPrefix + "Ironsource App key: " + appKey);
                if (appKey.Equals(string.Empty))
                {
                    if (logDebug)
                        Debug.Log(logPrefix + "IronSource Initilizer Cannot init without AppKey");
                }
                else
                {
                    IronSource.Agent.init(appKey);
                }
            }
#else
            IrsInitSuccess?.Invoke();
#endif
        }

        public void LoadIrsInter()
        {
#if USE_IRON
            timeLoadIrsInter = DateTime.Now.ToUniversalTime();
            IronSource.Agent.loadInterstitial();
            FirebaseLogger.InterCall(ironsrc);
#endif
        }

        private bool IrsInterReady()
        {
#if USE_IRON
            return IronSource.Agent.isInterstitialReady();
#else
            return false;
#endif
        }

        private void ShowIrsInter(Action onSuccess = null)
        {
#if USE_IRON
            isShowingAds = true;
            _OnInterComplete = onSuccess;
            IronSource.Agent.showInterstitial();
#endif
        }

        public void LoadIrsBanner()
        {
#if USE_IRON
            FirebaseLogger.BannerCall(ironsrc);
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, banner_pos);
            IronSource.Agent.hideBanner();
#endif
        }

        public void ShowIrsBanner()
        {
#if USE_IRON
            if (RemoveAds || DisabledAds) IronSource.Agent.hideBanner();
            else IronSource.Agent.displayBanner();
#endif
        }

        public void HideIrsBanner()
        {
#if USE_IRON
            IronSource.Agent.hideBanner();
#endif
        }

        private bool IrsRewardReady()
        {
#if USE_IRON
            return IronSource.Agent.isRewardedVideoAvailable();
#else
            return false;
#endif
        }

        public void LoadIrsReward()
        {
#if USE_IRON
            IronSource.Agent.loadRewardedVideo();
#endif
        }

        private void ShowIrsReward(Action OnSuccess)
        {
#if USE_IRON
            _OnRewardComplete = OnSuccess;
            IronSource.Agent.showRewardedVideo();
#else
            OnSuccess?.Invoke();
#endif
        }
        #endregion

        #region IRONSOURCE_EVENT
#if USE_IRON
        private void SdkInitializationCompletedEvent()
        {
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource sdk init complete");
            FirebaseLogger.InitIrsSuccess();

            IrsInitSuccess?.Invoke();

            // LoadIrsReward();
            LoadIrsInter();
            if (!UseAdmobBanner) LoadIrsBanner();
        }

        private void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            FirebaseLogger.OnImpressAds(impressionData);
        }

        private void OnBannerLoad(IronSourceAdInfo adInfo)
        {
            FirebaseLogger.BannerReady(ironsrc);
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource banner loaded successful");
        }

        private void OnBannerFail(IronSourceError err)
        {
            FirebaseLogger.BannerCallFail(ironsrc, err.getErrorCode());
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource banner load fail: " +
                    err.getErrorCode() + ", message:" + err.ToString());
        }

        void OnInterLoadFail(IronSourceError error)
        {
            FirebaseLogger.InterCallFail(ironsrc, error.getCode());
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource inter load fail: " +
                    error.getErrorCode() + ", message: " + error.ToString());
        }

        private void OnInterReady(IronSourceAdInfo adInfo)
        {
            FirebaseLogger.InterReady(ironsrc);
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource inter ready: " +
                    (DateTime.Now.ToUniversalTime() - timeLoadIrsInter).Seconds);
        }

        private void OnInterOpen(IronSourceAdInfo adInfo)
        {
            MuteGameSound();
        }

        private void OnInterClose(IronSourceAdInfo adInfo)
        {
            lastAdTime = DateTime.Now;
            _OnInterComplete?.Invoke();
            _OnInterComplete = null;
            LoadIrsInter();
            OpenGameSound();
            isShowingAds = false;
        }

        void OnInterShowSuccess(IronSourceAdInfo adInfo)
        {
            FirebaseLogger.InterShowSuccess(ironsrc, currentPlacement, CurrentLevel, CurrentMode);
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource inter show success: " + currentPlacement);
        }

        void OnInterShowFail(IronSourceError error, IronSourceAdInfo adInfo)
        {
            FirebaseLogger.InterShowFail(ironsrc, currentPlacement, CurrentLevel, CurrentMode, error.getCode());
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource inter show fail: " + currentPlacement);
        }

        private void OnRewardStart()
        {
            grantReward = false;
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward start: " + currentPlacement);
        }

        private void OnRewardGrant(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            if (placement == null)
            {
                return;
            }
            grantReward = true;
            if (logDebug)
                Debug.Log(logPrefix + "Ironsrouce reward granted: " + currentPlacement);
        }

        private void OnRewardOpen(IronSourceAdInfo adInfo)
        {
            FirebaseLogger.RewardShowSuccess(ironsrc, currentPlacement, CurrentLevel, CurrentMode);
            grantReward = false;
            MuteGameSound();
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward open:" + currentPlacement);
        }

        private void OnRewardAvailable(IronSourceAdInfo adInfo)
        {
            FirebaseLogger.RewardReady(ironsrc);
            OnRewardLoaded?.Invoke();

            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward available");
        }

        private void OnRewardUnavailable()
        {
            FirebaseLogger.RewardCallFail(ironsrc, -1);

            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward unavailable ");
        }


        private void OnRewardShowFail(IronSourceError obj, IronSourceAdInfo adInfo)
        {
            grantReward = false;
            FirebaseLogger.RewardShowFail(ironsrc, currentPlacement, CurrentLevel, CurrentMode, obj.getCode());
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward show fail: " + currentPlacement);
        }

        private void OnRewardClose(IronSourceAdInfo adInfo)
        {
            if(grantReward == true)
            {
                OnRewardComplete?.Invoke(currentAdType);
                _OnRewardComplete?.Invoke();
                _OnRewardComplete = null;
            }
            grantReward = false;

            // LoadIrsReward();
            OpenGameSound();
            isShowingAds = false;
            if (logDebug)
                Debug.Log(logPrefix + "Ironsource reward close");
        }
#endif
        #endregion
    }
}