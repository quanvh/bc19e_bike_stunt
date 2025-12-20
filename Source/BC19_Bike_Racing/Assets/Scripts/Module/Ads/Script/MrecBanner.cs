using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    [Serializable]
    public class MrecBanner
    {
        [FoldoutGroup("@mrecName", expanded: false), Space] public MrecName mrecName;
        [FoldoutGroup("@mrecName"), SerializeField, Space] private bool ForceLoadOnCall = false;
        [FoldoutGroup("@mrecName"), SerializeField, Space] private RectanglePosition mrec_pos = RectanglePosition.BottomLeft;
        [FoldoutGroup("@mrecName"), SerializeField, Space] private Vector2Int mrec_offset;
        [FoldoutGroup("@mrecName"), SerializeField, Space] private List<string> mrec_android;
        [FoldoutGroup("@mrecName"), SerializeField, Space] private List<string> mrec_ios;

        [FoldoutGroup("@mrecName"), SerializeField, Space, HideIf("ForceLoadOnCall")] private float timeExpire = 90;

        [FoldoutGroup("@mrecName"), SerializeField, Space] private bool cappingFail = false;
        [FoldoutGroup("@mrecName"), SerializeField, Space, ShowIf("cappingFail")] private float cappingTime = 30;

        private int failCount = 0;

        private string Mrec_name => mrecName.ToString();
        private readonly string logPrefix = "[AdsController] ";
        private bool showMrec;
#if USE_ADMOB
        private BannerView mrecBanner;
#endif
        private int mrecRetry;

        private bool isDisplayed = false;
        private bool isAvailable = false;

        private bool LogDebug => AdsController.Instance.logDebug;

        private bool RemoveAds => AdsController.Instance.RemoveAds;

        private bool DisabledAds => AdsController.Instance.DisabledAds;

        private bool UseAdmobMrec => AdsController.Instance.UseAdmobMrec;

        public float ScreenRatio => Mathf.Max(Screen.width * 1f / Screen.height, Screen.height * 1f / Screen.width);

        public bool IsReady()
        {
#if USE_ADMOB
            return mrecBanner != null;
#else
            return false;
#endif
        }

        public void ShowMrecBanner(bool reload = true, RectanglePosition showPos = RectanglePosition.None)
        {
#if USE_ADMOB
            if (RemoveAds || DisabledAds || !UseAdmobMrec)
            {
                mrecBanner?.Hide();
                return;
            }
            showMrec = true;
            if (LogDebug)
                Debug.Log(logPrefix + "Admob " + Mrec_name + " show: " + showPos);

            if (showPos == RectanglePosition.None) showPos = mrec_pos;
            if (mrecBanner != null)
            {
                isDisplayed = true;
                SetBannerPos(mrecBanner, showPos, mrec_offset);
                mrecBanner.Show();
            }
            else if (reload)
            {
                LoadMrecBanner();
            }
#endif
        }


        public void HideMrecBanner()
        {
#if USE_ADMOB
            showMrec = false;
            if (LogDebug)
                Debug.Log(logPrefix + "Admob " + Mrec_name + " hide");

            mrecBanner?.Hide();
#endif
        }

        [HideInInspector] public bool isLoading = false;
        private DateTime timeLoad = DateTime.MinValue;
        public void LoadMrecBanner(bool reset = true)
        {
#if USE_ADMOB
            if (isAvailable && !ForceLoadOnCall && !isDisplayed && (DateTime.Now - timeLoad).TotalSeconds < timeExpire) return;
#endif

            if (reset)
            {
                mrecRetry = 0;
            }

#if USE_ADMOB
            string adUnitId = "unexpected_platform";
            int totalId = 0;
#if UNITY_ANDROID
            if (mrec_android.Count <= mrecRetry) return;
            adUnitId = mrec_android[mrecRetry];
            totalId = mrec_android.Count;
#elif UNITY_IOS
            if (mrec_ios.Count <= mrecRetry) return;
            adUnitId = mrec_ios[mrecRetry];
            totalId = mrec_ios.Count;
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
            timeLoad = DateTime.Now;

            if (mrecBanner != null)
            {
                isAvailable = false;
                mrecBanner.Destroy();
                mrecBanner = null;
            }

            if (LogDebug)
                Debug.Log(logPrefix + "Admob " + Mrec_name + " request: " + adUnitId);
            FirebaseLogger.BannerCall(Mrec_name);

            mrecBanner = new BannerView(adUnitId.Trim(), AdSize.MediumRectangle, AdPosition.BottomLeft);

            mrecBanner.OnBannerAdLoaded += () =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Mrec_name + " load success");
                    FirebaseLogger.BannerReady(Mrec_name);

                    failCount = 0;
                    isAvailable = true;
                    isDisplayed = false;

                    if (showMrec) mrecBanner.Show();
                    else mrecBanner.Hide();
                    AdsController.Instance.LoadMrecSuccess(mrecName);
                    isLoading = false;
                });
            };
            mrecBanner.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Mrec_name + " load fail, message: " + error.GetMessage());
                    FirebaseLogger.BannerCallFail(Mrec_name, error.GetCode());

                    failCount++;
                    if (mrecRetry < totalId - 1)
                    {
                        mrecRetry++;
                        LoadMrecBanner(false);
                    }
                    else
                    {
                        AdsController.Instance.LoadMrecFail(mrecName);
                        isLoading = false;
                    }
                });
            };
            mrecBanner.OnAdPaid += AdsController.Instance.OnAdPaid;

            SetBannerPos(mrecBanner, mrec_pos, mrec_offset);

            AdRequest request = new AdRequest();
            mrecBanner.LoadAd(request);
            mrecBanner.Hide();

#endif
        }



#if USE_ADMOB
        private void SetBannerPos(BannerView _banner, RectanglePosition showPos, Vector2Int offset)
        {
            if (_banner == null) return;

            float screenScale = MobileAds.Utils.GetDeviceScale();
            if (screenScale == 0)
                screenScale = 2.8125f;
            float screenWidth = MobileAds.Utils.GetDeviceSafeWidth();
            if (screenWidth == 0)
                screenWidth = 384;
            float screenHeight;
            if (Screen.width > Screen.height)
                screenHeight = screenWidth / ScreenRatio;
            else
                screenHeight = screenWidth * ScreenRatio;


            float adWidth = (int)(_banner.GetWidthInPixels() / screenScale);
            float adHeight = (int)(_banner.GetHeightInPixels() / screenScale);
            int xMax = (int)(screenWidth - adWidth);
            int yMax = (int)(screenHeight - adHeight);
            int xCenter = (int)(xMax * 0.5f);
            int yCenter = (int)(yMax * 0.5f);

            switch (showPos)
            {
                case RectanglePosition.Center:
                    _banner.SetPosition(xCenter + offset.x, yCenter - offset.y);
                    break;
                case RectanglePosition.Top:
                    _banner.SetPosition(xCenter + offset.x, -offset.y);
                    break;
                case RectanglePosition.Bottom:
                    _banner.SetPosition(xCenter + offset.x, yMax - offset.y);
                    break;
                case RectanglePosition.TopLeft:
                    _banner.SetPosition(offset.x, -offset.y);
                    break;
                case RectanglePosition.TopRight:
                    _banner.SetPosition(xMax + offset.x, -offset.y);
                    break;
                case RectanglePosition.BottomLeft:
                    _banner.SetPosition(offset.x, yMax - offset.y);
                    break;
                case RectanglePosition.BottomRight:
                    _banner.SetPosition(xMax + offset.x, yMax - offset.y);
                    break;
                case RectanglePosition.Left:
                    _banner.SetPosition(offset.x, yCenter - offset.y);
                    break;
                case RectanglePosition.Right:
                    _banner.SetPosition(xMax + offset.x, yCenter - offset.y);
                    break;
                default: break;
            }
        }

#endif

        public void SetAdUnit(string adUnit, int _index = 0)
        {
#if UNITY_IOS
            if (mrec_ios.Count > _index)
            {
                mrec_ios[_index] = adUnit;
            }
            else
            {
                mrec_ios.Add(adUnit);
            }
#else
            if (mrec_android.Count > _index)
            {
                mrec_android[_index] = adUnit;
            }
            else
            {
                mrec_android.Add(adUnit);
            }
#endif
        }
    }

    public enum MrecName
    {
        None = 0,
        Mrec_Open = 1,
        Mrec_Start_Level = 2,
        Mrec_End_Level = 3,
        Mrec_Default = 4,
        Mrec_Daily = 5,
        Mrec_Spin = 6,
        Mrec_Exit = 7,
    }

    public enum RectanglePosition
    {
        None,
        Center,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}