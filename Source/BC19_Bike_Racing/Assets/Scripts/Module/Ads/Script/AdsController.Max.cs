using System;
using UnityEngine;


namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {

#if USE_MAX
        [Header("MAX_ID"), Space]
        public string sdk_Android = "7PspscCcbGd6ohttmPcZTwGmZCihCW-Jwr7nSJN2a_9Mg0ERPs0tmGdKTK1gs__nr6XHQvK0vTNaTb1uR1mCIN";
        public string sdk_iOS = "";


        [Header("MAX_BANNER"), Space]
        public string bannerID_Android = "8f8d86ea0c0969cf";
        public string bannerID_iOS = "";
        public MaxSdkBase.BannerPosition max_banner_pos = MaxSdkBase.BannerPosition.BottomCenter;


        [Header("MAX_MREC"), Space]
        public string mrecOpenID_Android = "8f8d86ea0c0969cf";
        public string mrecOpenID_iOS = "";
        public MaxSdkBase.AdViewPosition mrec_open_pos = MaxSdkBase.AdViewPosition.BottomCenter;

        [Header("MAX_MREC"), Space]
        public string mrecID_Android = "8f8d86ea0c0969cf";
        public string mrecID_iOS = "";
        public MaxSdkBase.AdViewPosition mrec_pos = MaxSdkBase.AdViewPosition.BottomCenter;


        [Header("MAX_INTER"), Space]
        public string interID_Android = "e534c1562ae6013a";
        public string interID_iOS = "";


        [Header("MAX_REWARD"), Space]
        public string rewardID_Android = "1556e10d32163f40";
        public string rewardID_iOS = "";

        [Header("MAX_OPENAD"), Space]
        public string openadID_Android = "";
        public string openadID_iOS = "";

        private DateTime timeLoadInterMax;
        int interAttempt, rewardAttempt;
        string sdkKey, bannerID, mrecID, mrecOpenID, interID, rewardID, openadID;
        private bool showMaxMrec;
#endif


        public void InitMaxSdk()
        {
#if USE_MAX
#if UNITY_IOS
            sdkKey = sdk_iOS;
            bannerID = bannerID_iOS;
            mrecID = mrecID_iOS;
            mrecOpenID = mrecOpenID_iOS;
            interID = interID_iOS;
            rewardID = rewardID_iOS;
            openadID = openadID_iOS;
#else
            sdkKey = sdk_Android;
            bannerID = bannerID_Android;
            mrecID = mrecID_Android;
            mrecOpenID = mrecOpenID_Android;
            interID = interID_Android;
            rewardID = rewardID_Android;
            openadID = openadID_Android;
#endif
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                FirebaseLogger.InitMaxSuccess();

                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoaded;

                MaxSdk.LoadAppOpenAd(openadID);

                InitMaxInter();

                InitMaxReward();

                InitMaxBanner();

                InitMaxMrec();
            };

            MaxSdk.SetSdkKey(sdkKey);
            MaxSdk.InitializeSdk();
#endif
        }

#if USE_MAX
        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MaxSdk.LoadAppOpenAd(openadID);
        }

        public void OnAppOpenLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (logDebug)
                Debug.Log("App open load success...:");
            adCount++;
            if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
        }
#endif

        public bool MaxOpenAdReady()
        {
#if USE_MAX
            return MaxSdk.IsAppOpenAdReady(openadID);
#else
            return false;
#endif
        }

        public void ShowMaxOpenAds()
        {
            if (DisabledAds) return;
#if USE_MAX
            if (MaxSdk.IsAppOpenAdReady(openadID))
            {
                MaxSdk.ShowAppOpenAd(openadID);
            }
            else
            {
                MaxSdk.LoadAppOpenAd(openadID);
            }
#endif
        }

        public void OnApplicationPause(bool pause)
        {
#if USE_MAX
            if (!pause && Mediation == AD_MEDIATION.MAX)
            {
                if (!gameLoaded)
                    return;

                if (hideApp)
                {
                    hideApp = false;
                    return;
                }

                ShowMaxOpenAds();
            }
#endif
        }

#if USE_MAX
        public void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
        {
#if USE_FIREBASE
            FirebaseLogger.OnImpressAds(impressionData);
#endif
    }
#endif

        #region INTER_MAX
        public void InitMaxInter()
        {
#if USE_MAX
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterReady;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterLoadFail;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterOpen;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterClose;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterShowFail;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            LoadMaxInter();
#endif
        }

#if USE_MAX
        private void LoadMaxInter()
        {
            MaxSdk.LoadInterstitial(interID);
            timeLoadInterMax = DateTime.Now.ToUniversalTime();
            FirebaseLogger.InterCall(maxsdk);
        }

        private void OnInterReady(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
            // Reset retry attempt
            interAttempt = 0;
            Debug.Log("------------------ max inter ready: " + (DateTime.Now.ToUniversalTime() - timeLoadInterMax).Seconds);
            FirebaseLogger.InterReady(maxsdk);
        }

        private void OnInterLoadFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
            FirebaseLogger.InterCallFail(maxsdk, (int)errorInfo.Code);

            interAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interAttempt));
            Invoke(nameof(LoadMaxInter), (float)retryDelay);
        }

        private void OnInterOpen(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ShowAds = true;
            MuteGameSound();
            FirebaseLogger.InterShowSuccess(maxsdk, interPlacement, CurrentLevel, CurrentMode);
        }

        private void OnInterShowFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            ShowAds = false;
            LoadMaxInter();
            FirebaseLogger.InterShowFail(maxsdk, interPlacement, CurrentLevel, CurrentMode, (int)errorInfo.Code);
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterClose(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad.
            ShowAds = false;
            this.lastAdTime = DateTime.Now;
            _OnInterComplete?.Invoke();
            _OnInterComplete = null;
            OpenGameSound();
            LoadMaxInter();
        }
#endif
        #endregion

        #region REWARD_MAX
        public void InitMaxReward()
        {
#if USE_MAX
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardStart;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardClose;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardShowFail;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardGrant;

            LoadMaxReward();
#endif
        }

#if USE_MAX
        private void LoadMaxReward()
        {
            MaxSdk.LoadRewardedAd(rewardID);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

            // Reset retry attempt
            rewardAttempt = 0;

            Debug.Log("Reward load success...:");
            FirebaseLogger.RewardReady(maxsdk);

            OnRewardLoaded?.Invoke();
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            Debug.Log("Reward load failed...:");
            FirebaseLogger.RewardCallFail(maxsdk, (int)errorInfo.Code);

            rewardAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardAttempt));

            Invoke(nameof(LoadMaxReward), (float)retryDelay);
        }

        private void OnRewardStart(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ShowAds = true;
            Debug.Log("--- ironsource reward start: " + rewardPlacement);
            grantReward = false;
            MuteGameSound();
        }

        private void OnRewardShowFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            ShowAds = false;
            LoadMaxReward();
            Debug.Log("Reward show Failed Event...");
            grantReward = false;
            FirebaseLogger.RewardShowFail(maxsdk, rewardPlacement, CurrentLevel, CurrentMode, (int)errorInfo.Code);
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardClose(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            ShowAds = false;
            Debug.Log("--- max reward close: " + rewardPlacement);
            if(grantReward)
            {
            
                OnRewardComplete?.Invoke(currentAdType);
                _OnRewardComplete?.Invoke();
                _OnRewardComplete = null;
            }
            grantReward = false;
            LoadMaxReward();
            OpenGameSound();
        }

        private void OnRewardGrant(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            Debug.Log("--- max reward grant: " + rewardPlacement);
            if (rewardPlacement == null)
            {
                return;
            }
            grantReward = true;
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
#endif
        #endregion

        #region BANNER_MAX
        public void InitMaxBanner()
        {
#if USE_MAX
            MaxSdk.CreateBanner(bannerID, max_banner_pos);
            MaxSdk.SetBannerExtraParameter(bannerID, "adaptive_banner", "false");

            // Set background or background color for banners to be fully functional
            //MaxSdk.SetBannerBackgroundColor(bannerID, Color.grey);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoad;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerFail;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
#endif
        }

#if USE_MAX
        private void OnBannerLoad(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            FirebaseLogger.BannerReady(maxsdk);
        }

        private void OnBannerFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            FirebaseLogger.BannerCallFail(maxsdk, ((int)errorInfo.Code));
            Debug.Log("banner load fail: " + errorInfo.Code + ", message:" + errorInfo.Message);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
#endif
        #endregion

        #region MREC_MAX
        public void InitMaxMrec()
        {
#if USE_MAX
            MaxSdk.CreateMRec(mrecID, mrec_pos);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) =>
            {
                if (logDebug)
                    Debug.Log("Mrec load success...:");
                adCount++;
                if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
                if (showMaxMrec) MaxSdk.ShowMRec(mrecID);
                else MaxSdk.HideMRec(mrecID);
            };

            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += (string adUnitId, MaxSdkBase.ErrorInfo error) => { };

            MaxSdkCallbacks.MRec.OnAdClickedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => { };

            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) =>
            {

            };

            MaxSdkCallbacks.MRec.OnAdExpandedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => { };

            MaxSdkCallbacks.MRec.OnAdCollapsedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => { };
#endif
        }

        public void ShowMaxMrec()
        {
            if (!UseAdmobMrec) return;
#if USE_MAX
            showMaxMrec = true;
            MaxSdk.ShowMRec(mrecID);
#endif
        }

        public void HideMaxMrec()
        {
#if USE_MAX
            showMaxMrec = false;
            MaxSdk.HideMRec(mrecID);
#endif
        }

        #endregion

        #region EXTERNAL_FUNTION
        public bool MaxInterReady()
        {
#if USE_MAX
            return MaxSdk.IsInterstitialReady(interID);
#else
            return false;
#endif
        }

        public void ShowMaxInter(Action onSuccess = null)
        {
#if USE_MAX
            FirebaseLogger.InterShow(maxsdk, interPlacement, CurrentLevel, CurrentMode);

            if (MaxInterReady())
            {
                _OnInterComplete = onSuccess;
                MaxSdk.ShowInterstitial(interID, interPlacement);

            }
            else
            {
                FirebaseLogger.InterShowNotReady(maxsdk, interPlacement, CurrentLevel, CurrentMode);
                LoadMaxInter();
                onSuccess?.Invoke();
            }
#else
            onSuccess?.Invoke();
#endif
        }

        public bool MaxRewardReady()
        {
#if USE_MAX
            return MaxSdk.IsRewardedAdReady(rewardID);
#else
            return false;
#endif
        }

        public void ShowMaxReward(Action OnSuccess = null)
        {
#if USE_MAX
            FirebaseLogger.RewardShow(maxsdk, rewardPlacement, CurrentLevel, CurrentMode);
            if (MaxRewardReady())
            {
                _OnRewardComplete = OnSuccess;
                MaxSdk.ShowRewardedAd(rewardID, rewardPlacement);
            }
            else
            {
                FirebaseLogger.RewardShowNotReady(maxsdk, rewardPlacement, CurrentLevel, CurrentMode);
                LoadMaxReward();
            }
#else
            OnSuccess?.Invoke();
#endif
        }

        public void ShowMaxBanner()
        {
#if USE_MAX
            if (RemoveAds || DisabledAds) MaxSdk.HideBanner(bannerID);
            else MaxSdk.ShowBanner(bannerID);
#endif
        }

        public void HideMaxBanner()
        {
#if USE_MAX
            MaxSdk.HideBanner(bannerID);
#endif
        }
        #endregion
    }
}