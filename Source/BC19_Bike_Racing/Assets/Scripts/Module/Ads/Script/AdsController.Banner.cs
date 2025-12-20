using UnityEngine;
using System;
using System.Linq;


#if USE_ADMOB
using GoogleMobileAds.Api;
#endif


namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private readonly string small_banner = "small_banner";

#if USE_ADMOB
        private bool showSmallBanner;
        private BannerView bannerSmall;
#endif

        public Action<MrecName> OnMrecLoaded;
        public Action<MrecName> OnMrecSuccess;
        public Action<MrecName> OnMrecFail;

        public float ScreenRatio => Screen.width * 1f / Screen.height;

        public float ScreenSafeRatio => Screen.width * 1f / Screen.safeArea.height;

        private float ScreenScale;
        private float ScreenWidth;

        #region EXTERNAL_FUNCTION
        public void LoadMrecSuccess(MrecName _name)
        {
            if (_name == MrecName.Mrec_Open)
            {
                adCount++;
                if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
            }
            OnMrecLoaded?.Invoke(_name);
        }

        public void LoadMrecFail(MrecName _name)
        {
            if (_name == MrecName.Mrec_Open)
            {
                adCount++;
                if (adCount >= 2) OnFirstAdsLoaded?.Invoke();
            }
            OnMrecFail?.Invoke(_name);
        }

        public bool CheckMrec(MrecName _name)
        {
            MrecBanner _banner = mrec_banner.Where(t => t.mrecName == _name).First();
            if (_banner != null)
            {
                return _banner.IsReady();
            }
            else return false;
        }

        public void ShowMrec(MrecName _name, bool reload = true)
        {
            MrecBanner _banner = mrec_banner.Where(t => t.mrecName == _name).First();
            if (_banner != null)
            {
                if (!RemoveAds && !DisabledAds)
                {
                    _banner.ShowMrecBanner(reload);
                }
                else
                {
                    _banner.HideMrecBanner();
                }
            }
        }

        public void HideMrec(MrecName _name)
        {
            MrecBanner _banner = mrec_banner.Where(t => t.mrecName == _name).First();
            if (_banner != null)
            {
                _banner.HideMrecBanner();
            }
        }

        public void HideMrec()
        {
            foreach (var mrec in mrec_banner)
            {
                mrec.HideMrecBanner();
            }
        }

        public void LoadMrec(MrecName _name)
        {
            if (!RemoveAds && !DisabledAds)
            {
                MrecBanner _banner = mrec_banner.Where(t => t.mrecName == _name).First();
                if (_banner != null && !_banner.isLoading)
                {
                    _banner.LoadMrecBanner();
                }
            }
        }


        public void ShowSmallBanner()
        {
#if USE_ADMOB
            if (RemoveAds || DisabledAds || !UseAdmobBanner)
            {
                bannerSmall?.Hide();
                return;
            }
            showSmallBanner = true;
            if (logDebug)
                Debug.Log(logPrefix + "Admob " + small_banner + " show");
#if UNITY_IOS
            SetSmallBannerPos(bannerSmall);
#endif
            bannerSmall?.Show();
#endif
        }

        public void HideSmallBanner()
        {
#if USE_ADMOB
            showSmallBanner = false;
            if (logDebug)
                Debug.Log(logPrefix + "Admob " + small_banner + " hide");

            bannerSmall?.Hide();
#endif
        }

        #endregion

        private int smallBannerRetry;
        [HideInInspector] public bool smallBannerLoading = false;
        public void LoadSmallBanner(bool reset = true)
        {
            if (RemoveAds || DisabledAds)
            {
                return;
            }

            smallBannerLoading = true;
            if (reset)
            {
                smallBannerRetry = 0;
            }

#if USE_ADMOB
            string adUnitId = "unexpected_platform";
            int totalId = 0;
#if UNITY_ANDROID
            if (small_banner_android.Count <= smallBannerRetry) return;
            adUnitId = small_banner_android[smallBannerRetry];
            totalId = small_banner_android.Count;
#elif UNITY_IOS
            if (small_banner_ios.Count <= smallBannerRetry) return;
            adUnitId = small_banner_ios[smallBannerRetry];
            totalId = small_banner_ios.Count;
#endif

#if USE_TRIP
        if (localBannerDefault.Count > smallBannerRetry && !string.IsNullOrEmpty(localBannerDefault[smallBannerRetry]))
        {
            adUnitId = localBannerDefault[smallBannerRetry];
        }
#endif
            if (bannerSmall != null)
            {
                bannerSmall.Destroy();
                bannerSmall = null;
            }

            if (logDebug)
                Debug.Log(logPrefix + "Admob " + small_banner + " request");
            FirebaseLogger.BannerCall(small_banner, adUnitId);

            if (adaptiveBanner)
            {
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                bannerSmall = new BannerView(adUnitId.Trim(), adaptiveSize, small_banner_pos);
            }
            else
            {
                bannerSmall = new BannerView(adUnitId.Trim(), AdSize.Banner, small_banner_pos);
            }

            bannerSmall.OnBannerAdLoaded += () =>
            {
                ExecuteAction(() =>
                {
                    FirebaseLogger.BannerReady(small_banner, adUnitId);
                    if (logDebug)
                        Debug.Log(logPrefix + "Admob " + small_banner + " load success");

#if UNITY_IOS
                SetSmallBannerPos(bannerSmall);
#endif
                    if (collapseBanner)
                    {
                        bannerSmall.Show();
                    }
                    else
                    {
                        if (showSmallBanner) bannerSmall.Show();
                        else bannerSmall.Hide();
                    }
                    smallBannerLoading = false;
                });
            };
            bannerSmall.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                ExecuteAction(() =>
                {
                    if (smallBannerRetry < totalId - 1)
                    {
                        smallBannerRetry++;
                        LoadSmallBanner(false);
                    }

                    FirebaseLogger.BannerCallFail(small_banner, error.GetCode(), adUnitId);
                    if (logDebug)
                        Debug.Log(logPrefix + "Admob " + small_banner + " load fail, message: " + error.GetMessage());
                    smallBannerLoading = false;
                });
            };
            bannerSmall.OnAdPaid += OnAdPaid;

            AdRequest request = new AdRequest();
            if (collapseBanner && !UserOrganic)
            {
                request.Extras.Add("collapsible", "bottom");
            }
            bannerSmall.LoadAd(request);
            if (collapseBanner)
            {
                bannerSmall.Show();
            }
            else
            {
                bannerSmall.Hide();
            }
#endif
        }

#if USE_ADMOB
        public void SetSmallBannerPos(BannerView _banner)
        {
            if (Screen.height != Screen.safeArea.height)
            {
                float adWidth = (int)(_banner.GetWidthInPixels() / ScreenScale);
                int xMax = (int)(ScreenWidth - adWidth);
                int xCenter = (int)(xMax * 0.5f);

                float screenHeight = (Screen.width / Screen.safeArea.width) * ScreenWidth / ScreenRatio;

                if (Screen.orientation == ScreenOrientation.Portrait)
                {
                    screenHeight += (ScreenWidth / ScreenSafeRatio - screenHeight) / 2f;
                }

                float adHeight = (int)(_banner.GetHeightInPixels() / ScreenScale);


                int yMax = (int)(screenHeight - adHeight);
                _banner.SetPosition(xCenter, yMax);
            }
        }

#endif

    }
}