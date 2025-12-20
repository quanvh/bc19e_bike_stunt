using System;
using UnityEngine;


public class AdCallbackProxy : AndroidJavaProxy
{
    #region Handler Callbacks

    public Action<string> OnAdLoadedEvent;
    public Action<NativeAdsInfo> OnPaidAdImpressionEvent;
    public Action<string, string> OnAdFailedToLoadEvent;
    public Action<string> OnAdClickedEvent;
    public Action<string> OnAdDisplayedEvent;
    public Action<string> OnAdOpenedEvent;
    public Action<string, string> OnAdDisplayFailedEvent;
    public Action<string> OnAdSwipeGestureClickedEvent;

    #endregion

    #region Callback Proxy

    public AdCallbackProxy() : base("com.bacon.unityadmobnative.Adapter.AdCallbackListener")
    {
    }

    //public AdCallbackProxy() : base("blackgems.unityadmobnative.Adapter.AdCallbackListener")
    //{
    //}

    public void onAdLoaded(string adUnit)
    {
        OnAdLoadedEvent?.Invoke(adUnit);
    }

    public void onPaidAdImpression(string adUnit, long adValue, string currencyCode, int precisionType,
        string mediationAdapterName)
    {
        OnPaidAdImpressionEvent?.Invoke(new NativeAdsInfo
        {
            adUnit = adUnit,
            adValue = adValue,
            currencyCode = currencyCode,
            precisionType = precisionType,
            mediationAdapterName = mediationAdapterName
        });
    }

    public void onAdFailedToLoad(string adUnit, string error)
    {
        OnAdFailedToLoadEvent?.Invoke(adUnit, error);
    }

    public void onAdClicked(string adUnit)
    {
        OnAdClickedEvent?.Invoke(adUnit);
    }

    public void onAdDisplayed(string adUnit)
    {
        OnAdDisplayedEvent?.Invoke(adUnit);
    }

    public void onAdDisplayedFailed(string adUnit, string error)
    {
        OnAdDisplayFailedEvent?.Invoke(adUnit, error);
    }

    public void onAdOpened(string adUnit)
    {
        OnAdOpenedEvent?.Invoke(adUnit);
    }

    public void onAdSwipeGestureClicked(string adUnit)
    {
        OnAdSwipeGestureClickedEvent?.Invoke(adUnit);
    }

    #endregion
}