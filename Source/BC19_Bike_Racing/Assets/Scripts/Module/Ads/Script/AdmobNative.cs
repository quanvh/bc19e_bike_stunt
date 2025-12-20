using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#if USE_ADMOB && USE_NATIVE
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace Bacon
{
    [Serializable]
    public class AdmobNative
    {
        [FoldoutGroup("@nativeName", expanded: false)] public NativeName nativeName;
        [FoldoutGroup("@nativeName"), SerializeField, Space] public List<string> native_android;
        [FoldoutGroup("@nativeName"), SerializeField, Space] private List<string> native_ios;

        [FoldoutGroup("@nativeName"), SerializeField, Space] private Sprite adIconSprite;

        [HideInInspector] public bool firstClickNative = false;

#if USE_ADMOB && USE_NATIVE
        private NativeAd nativeAd;
#endif
        private int nativeRetry;

        private bool LogDebug => AdsController.Instance.logDebug;
        private readonly string logPrefix = "[AdsController] ";

        float BannerSizeScale
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.BannerSizeScale;
                else return 0.75f;
            }
        }

        private int BannerSize
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.BannerSize;
                else return bannerSizeH;
            }
        }

        private bool RemoveAds => AdsController.Instance.RemoveAds;

        private bool DisabledAds => AdsController.Instance.DisabledAds;

        [HideInInspector] public bool isLoading = false;

        public void LoadNativeAd(Action OnLoaded, AdmobNativeUI _panel = null, bool reset = true)
        {
            if (RemoveAds || DisabledAds)
            {
                _panel.gameObject.SetActive(false);
                return;
            }
            isLoading = true;
            if (reset)
            {
                nativeRetry = 0;
            }
#if USE_ADMOB && USE_NATIVE

            string _idNative = string.Empty;
            int totalId = 0;
#if UNITY_IOS
            if(native_ios.Count <= nativeRetry) return;
            _idNative = native_ios[nativeRetry];
            totalId = native_ios.Count;
#else
            if (native_android.Count <= nativeRetry) return;
            _idNative = native_android[nativeRetry];
            totalId = native_android.Count;
#endif

            if (LogDebug)
                Debug.Log(logPrefix + "Native ad " + nativeName + " call: " + _idNative);
            if (_panel && _panel.adMessage)
            {
                _panel.adMessage.text = "Native call " + nativeName + " id: " + _idNative;
            }
            FirebaseLogger.NativeCall(_idNative);

            AdLoader adLoader = new AdLoader.Builder(_idNative.Trim())
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdLoaded += (object sender, NativeAdEventArgs args) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Native ads " + nativeName + " loaded: " + _idNative);
                    if (_panel && _panel.adMessage)
                        _panel.adMessage.text = "Native ads " + nativeName + " loaded";

                    FirebaseLogger.NativeReady(_idNative);


                    nativeAd = args.nativeAd;
                    SetNativeAds(_panel);

                    OnLoaded?.Invoke();
                    isLoading = false;
                });
            };

            adLoader.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs e) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Native ads " + nativeName + " HandleAdFailedToLoad: " + e.LoadAdError.ToString());
                    if (_panel && _panel.adMessage)
                        _panel.adMessage.text = "Native ads " + nativeName + " load failed: " + e.LoadAdError.ToString();
                    FirebaseLogger.NativeCallFail(_idNative);

                    if (nativeRetry < totalId - 1)
                    {
                        nativeRetry++;
                        LoadNativeAd(OnLoaded, _panel, false);
                    }
                    else
                    {
                        isLoading = false;
                    }
                });
            };

            adLoader.OnNativeAdClicked += (object sender, EventArgs args) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {

                    if (LogDebug)
                        Debug.Log("[AdsController] Native ads " + nativeName + " clicked!!");
                    if (nativeName == NativeName.Native_Banner)
                    {
                        firstClickNative = true;
                    }

                    AdsController.Instance.NativeClick();
                });
            };

            adLoader.OnNativeAdImpression += (object sender, EventArgs args) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Native ads " + nativeName + " impression!!");
                    if (_panel && _panel.adMessage)
                        _panel.adMessage.text = "Native ads " + nativeName + " impression!";
                    FirebaseLogger.NativeImpression(_idNative);
                });
            };
            adLoader.LoadAd(new AdRequest());
#endif
        }

        private float bannerSizeW = 2500f;
        [FoldoutGroup("@nativeName"), SerializeField, ShowIf("nativeName", NativeName.Native_Banner)] private int bannerSizeH = 240;
        private readonly int iconSizeW = 140;

#if USE_NATIVE
        public void SetNativeAds(AdmobNativeUI _panel = null)
        {
            if (_panel != null && nativeAd != null)
            {
                nativeAd.OnPaidEvent += NativeAd_OnPaidEvent;

                Texture2D iconTexture = nativeAd.GetIconTexture();
                if (_panel.adIcon && iconTexture)
                {
                    var ratio = iconTexture.width * 1f / iconTexture.height;
                    if (iconTexture.width > iconSizeW || iconTexture.height > iconSizeW)
                        iconTexture = Resize(iconTexture, Mathf.FloorToInt(iconSizeW * ratio), iconSizeW);

                    _panel.adIcon.sprite = Sprite.Create(iconTexture,
                        new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    _panel.adIcon.preserveAspect = true;
                    bool iconRegis = nativeAd.RegisterIconImageGameObject(_panel.adIcon.gameObject);

                    if (LogDebug) Debug.Log(logPrefix + "Register icon" + nativeName + ": " + iconRegis);
                }
                else
                {
                    _panel.adIcon.sprite = adIconSprite;
                }

                if (_panel.adImage && nativeAd.GetImageTextures().Count > 0)
                {
                    Texture2D imageTexture = nativeAd.GetImageTextures()[0];
                    _panel.adImage.sprite = Sprite.Create(imageTexture,
                        new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.5f));
                    //adNativePanel.adImage.preserveAspect = true;
                    //SizeToParent(adNativePanel.adImage);
                    _panel.adImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)imageTexture.width / imageTexture.height;
                    nativeAd.RegisterImageGameObjects(new List<GameObject> { _panel.adImage.gameObject });
                }

                Texture2D iconAdChoices = nativeAd.GetAdChoicesLogoTexture();
                if (_panel.adChoices && iconAdChoices)
                {
                    _panel.adChoices.sprite = Sprite.Create(iconAdChoices, new Rect(0, 0, iconAdChoices.width, iconAdChoices.height), new Vector2(0.5f, 0.5f));
                    _panel.adChoices.preserveAspect = true;
                    bool iconRegis = nativeAd.RegisterAdChoicesLogoGameObject(_panel.adChoices.gameObject);
                    if (LogDebug) Debug.Log(logPrefix + "Register adchoices " + nativeName + ": " + iconRegis);

                }

                string headline = nativeAd.GetHeadlineText();
                if (_panel.adHeadline && !string.IsNullOrEmpty(headline))
                {
                    _panel.adHeadline.text = headline;
                    bool iconRegis = nativeAd.RegisterHeadlineTextGameObject(_panel.adHeadline.gameObject);
                    if (nativeName == NativeName.Native_Banner)
                    {
                        bannerSizeW = _panel.adHeadline.GetComponent<BoxCollider2D>().size.x;
                        if (firstClickNative)
                        {
                            _panel.adHeadline.GetComponent<BoxCollider2D>().size = new Vector2(bannerSizeW,
                                BannerSizeScale * GetBannerSize());
                        }
                        else
                        {
                            _panel.adHeadline.GetComponent<BoxCollider2D>().size = new Vector2(bannerSizeW,
                                 GetBannerSize());
                        }
                        _panel.adHeadline.GetComponent<Text>().raycastTarget = true;
                    }
                    if (LogDebug) Debug.Log(logPrefix + "Register Headline " + nativeName + ": " + iconRegis +
                        ", height: " + _panel.adHeadline.GetComponent<BoxCollider2D>().size.y);
                }
                ;

                if (_panel.adLabel)
                {
                    bool iconRegis = nativeAd.RegisterAdvertiserTextGameObject(_panel.adLabel);
                    if (LogDebug) Debug.Log(logPrefix + "Register ad label " + nativeName + ": " + iconRegis);
                }

                string advertiser = nativeAd.GetAdvertiserText();
                if (_panel.adAdvertiser && !string.IsNullOrEmpty(advertiser))
                {
                    _panel.adAdvertiser.text = advertiser;
                    //nativeAdOpen.RegisterAdvertiserTextGameObject(adNativePanel.adAdvertiser.gameObject);
                }

                string cta = nativeAd.GetCallToActionText();
                if (_panel.adCallToAction && !string.IsNullOrEmpty(cta))
                {
                    _panel.adCallToAction.text = cta;
                    if (_panel.adCTA)
                    {
                        bool iconRegis = nativeAd.RegisterCallToActionGameObject(_panel.adCTA);
                        if (LogDebug) Debug.Log(logPrefix + "Register CTA " + nativeName + ": " + iconRegis);
                    }
                }

                string body = nativeAd.GetBodyText();
                if (_panel.adBody && !string.IsNullOrEmpty(body))
                {
                    _panel.adBody.text = body;
                    bool iconRegis = nativeAd.RegisterBodyTextGameObject(_panel.adBody.gameObject);
                    if (LogDebug) Debug.Log(logPrefix + "Register ad body " + nativeName + ": " + iconRegis);
                }

                float star = (float)nativeAd.GetStarRating();
                if (_panel.adRating && star > 0)
                {
                    _panel.adRating.fillAmount = star / 5f;
                }

                string price = nativeAd.GetPrice();
                if (_panel.adPrice && !string.IsNullOrEmpty(price))
                {
                    _panel.adPrice.text = price;
                    nativeAd.RegisterPriceGameObject(_panel.adPrice.gameObject);
                }

                string store = nativeAd.GetStore();
                if (_panel.adStore && !string.IsNullOrEmpty(store))
                {
                    _panel.adStore.text = store;
                    nativeAd.RegisterStoreGameObject(_panel.adStore.gameObject);
                }

                _panel.gameObject.SetActive(true);
            }
        }

        private float GetBannerSize()
        {
            float _bannerHeight = BannerSize;
            //_bannerHeight *= MobileAds.Utils.GetDeviceScale();
            //_bannerHeight *= (Screen.height * 1920f) / (1080f * Screen.width);
            return _bannerHeight;
        }

        Texture2D Resize(Texture2D texture, int newWidth, int newHeight)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = tmp;
            Graphics.Blit(texture, tmp);
#if UNITY_2021_3_OR_NEWER
            texture.Reinitialize(newWidth, newHeight, texture.format, false);
#else
        texture.Resize(newWidth, newHeight, texture.format, false);
#endif
            texture.filterMode = FilterMode.Bilinear;
            texture.ReadPixels(new Rect(Vector2.zero, new Vector2(newWidth, newHeight)), 0, 0);
            texture.Apply();
            RenderTexture.ReleaseTemporary(tmp);
            return texture;
        }

        private void NativeAd_OnPaidEvent(object sender, AdValueEventArgs e)
        {
            AdsController.Instance.OnAdPaid(e.AdValue);
        }
#endif

        public void SetAdUnit(string adUnit)
        {
#if UNITY_IOS
            if (native_ios.Count > 0)
            {
                native_ios[0] = adUnit;
            }
            else
            {
                native_ios = new List<string> { adUnit };
            }
#else
            if (native_android.Count > 0)
            {
                native_android[0] = adUnit;
            }
            else
            {
                native_android = new List<string> { adUnit };
            }
#endif
        }
    }

    public enum NativeName
    {
        None = 0,
        Native_Language = 1,
        Native_Age = 2,
        Native_Banner = 3,
        Native_Daily = 4,
        Native_Level = 5,
        Native_Inter = 6,
        Native_Default = 7,
        Native_Start_Level = 8,
        Native_End_Level = 9,
        Native_Exit = 10,
    }
}