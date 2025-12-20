using Bacon;
using System;
using UnityEngine;

public class AdCloseProxy : AndroidJavaProxy
{
    public Action OnAdClosedEvent;

    public AdCloseProxy() : base("com.bacon.unityadmobnative.Adapter.AdCloseListener")
    {
    }

    //public AdCloseProxy() : base("blackgems.unityadmobnative.Adapter.AdCloseListener")
    //{
    //}

    public void onAdClosed()
    {

        AdsController.Instance.OpenGameSound();
        OnAdClosedEvent?.Invoke();
    }
}