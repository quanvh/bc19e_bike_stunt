using System;
using UnityEngine;
using System.Linq;


namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        public event Action OnNativeLoaded;

        public event Action OnNativeClick;

        public void RequestNative(NativeName _name, AdmobNativeUI _panel = null)
        {
#if USE_ADMOB

            AdmobNative _native = NativeAds.Where(t => t.nativeName == _name).First();
            if (_native != null)
            {
                if (_name == NativeName.Native_Banner && (RemoveAds || !UseAdmobBanner))
                {
                    _panel?.gameObject.SetActive(false);
                    return;
                }
                else if (!_native.isLoading)
                {

#if UNITY_ANDROID
                    //use native android
                    RequestNativeAndroid();
#else

                    _native.LoadNativeAd(OnNativeLoaded, _panel);
#endif
                }
            }
#endif
        }

        public void NativeClick()
        {
            OnNativeClick?.Invoke();
        }


        public void SetNativeAds(NativeName _name, AdmobNativeUI adNativePanel = null)
        {
#if USE_ADMOB && USE_NATIVE
            AdmobNative _native = NativeAds.Where(t => t.nativeName == _name).First();
            if (_native != null)
            {
                _native.SetNativeAds(adNativePanel);
            }
#endif
        }

        public void RequestNativeAndroid()
        {
            AdmobNative _native = NativeAds.Where(t => t.nativeName == NativeName.Native_Banner).First();
            if (_native.native_android.Count > 0)
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                _nativeAdsManager.Call("LoadNativeBanner", _native.native_android[0]);
#endif
            }
        }

        public void RemoveNativeAndroid()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            _nativeAdsManager.Call("HideNativeBanner");
#endif
        }

        public void OnAndroidNativeAdLoaded(string adUnit)
        {
            ExecuteAction(() =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Native android ads loaded: " + adUnit);

                FirebaseLogger.NativeReady(adUnit);
            });
        }

        public void OnAndroidFailedToLoad(string _adNativeError)
        {
            if (string.IsNullOrEmpty(_adNativeError)) return;

            AdNativeError adNativeError = JsonUtility.FromJson<AdNativeError>(_adNativeError);
            ExecuteAction(() =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Native android ads HandleAdFailedToLoad: " + adNativeError.message);
                FirebaseLogger.NativeCallFail(adNativeError.adUnit);
            });
        }

        public void OnAndroidNativeAdImpression(string adUnit)
        {
            ExecuteAction(() =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Native android ads " + adUnit + " impression!!");
                FirebaseLogger.NativeImpression(adUnit);
            });
        }

        public void OnAndroidNativeAdClicked(string adUnit)
        {
            ExecuteAction(() =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Native android ads " + adUnit + " clicked!!");
                if (RouteController.Instance)
                {
                    RouteController.Instance.ShowNativeDefault();
                }
            });
        }

        public void OnAndroidNativeAdClickClosed(string adUnit)
        {
            ExecuteAction(() =>
            {
                if (logDebug)
                    Debug.Log(logPrefix + "Native android ads " + adUnit + " closed!!");
                RemoveNativeAndroid();
                if (RouteController.Instance)
                {
                    RouteController.Instance.ShowNativeDefault();
                }
            });
        }

        public void OnImpressAds(string _adNativeInfo)
        {
            if (string.IsNullOrEmpty(_adNativeInfo)) return;

            AdNativeInfo adNativeInfo = JsonUtility.FromJson<AdNativeInfo>(_adNativeInfo);
            ExecuteAction(() =>
            {
                FirebaseLogger.OnImpressAdsAndroidNative(adNativeInfo.adValue, adNativeInfo.currencyCode);
            });
        }
    }

    public class AdNativeInfo
    {
        public string adUnit;
        public long adValue;
        public string currencyCode;
    }

    public class AdNativeError
    {
        public string adUnit;
        public long message;
    }
}