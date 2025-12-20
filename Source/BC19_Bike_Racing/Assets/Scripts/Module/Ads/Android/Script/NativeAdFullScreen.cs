using UnityEngine;

public class NativeAdFullScreen
{
    public AndroidJavaObject NativeAdFullScreenObject;
    public AdCloseProxy AdCloseProxy;
    public AdCallbackProxy NativeAdAdCallback;
    public AdCallbackProxy NativeAdEndCardAdCallback;

    public void LoadNativeAds()
    {
        NativeAdFullScreenObject.Call("loadNativeAds");
    }

    public void LoadNativeAdsEndCard()
    {
        NativeAdFullScreenObject.Call("loadNativeAdsEndCard");
    }

    public void ShowNativeAds()
    {
        NativeAdFullScreenObject.Call("showNativeAds");
    }

    public void CloseNative()
    {
        NativeAdFullScreenObject.Call("closeNative");
    }

    public void SetIsUsingEndCardNativeAds(bool isUsingEndCardNativeAds)
    {
        NativeAdFullScreenObject.Call("setIsUsingEndCardNativeAds", isUsingEndCardNativeAds);
    }

    public void SetShowingFakeNextView(bool isUsingShowingFakeNextView)
    {
        NativeAdFullScreenObject.Call("setShowingFakeNextView", isUsingShowingFakeNextView);
    }

    public void SetNativeAdsLayout(string nativeAdsLayout)
    {
        NativeAdFullScreenObject.Call("setNativeAdsLayout", nativeAdsLayout);
    }

    public void SetDPI()
    {
        float dpi = Screen.dpi;
        if (dpi == 0) dpi = 160;
        NativeAdFullScreenObject.Call("SetDPI", dpi);
    }
}