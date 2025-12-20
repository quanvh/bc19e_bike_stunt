using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Bacon
{
    [Serializable]
    public class RemoteConfig
    {
        public RemoteConfig()
        {
            Version = "1.0";
            ShowUMP = false;
            ShowRemoveAds = 50;

            UseAdmobInter = true;
            UseAdmobReward = true;
            UseAdmobBanner = true;
            UseAdmobMrec = true;
            UseAdmobInterSplash = true;
            UseOpenAd = true;
            UseNative = false;
            AdsConfig = "";
            TimeLeftNativeAds = 5;
            TimeVisibleClose = 3;
            TimeEnableClose = 1;

            AdsUnit = "";
            GameConfig = "";

            TimeLoadNative = 15f;
            BannerSize = 360;
            BannerSizeScale = 0.7f;
            DecreaseNativeBanner = false;
            TimeLoadBanner = 30;
            RemoteAds = false;
            MoreGame = false;

            ShowInter = true;
            InterStartLevel = false;
            InterStartFromLevel = 100;

            DurationBetweenAds = 0;
            TimeFirstInter = 1f;
            TimeLoadOpenAds = 5f;
            TimeAutoCloseAge = 10f;
            TimeShowCloseAge = 0.5f;
            TimeLoadInter = 6f;


        }
        [Header("VERSION"), Space]
        public string Version;
        public bool ShowUMP;
        public bool RemoteAds;
        public bool MoreGame;
        public int ShowRemoveAds;

        [Header("ADS"), Space]
        public bool ShowInter;
        public bool UseAdmobInter;
        public bool UseAdmobReward;
        public bool UseAdmobBanner;
        public bool UseAdmobMrec;
        public bool UseAdmobInterSplash;
        public bool UseOpenAd;
        public bool UseNative;
        public int TimeLeftNativeAds;
        public int TimeVisibleClose;
        public int TimeEnableClose;

        [Header("SMALL BANNER"), Space]
        public float TimeLoadBanner;
        public float TimeLoadNative;
        public float BannerSizeScale;
        public int BannerSize;
        public bool DecreaseNativeBanner;

        [Header("ADS INTER"), Space]
        public float TimeLoadInter;
        public bool InterStartLevel;
        public int InterStartFromLevel;
        public float DurationBetweenAds;
        public float TimeFirstInter;

        [Header("TIME CONFIG"), Space]
        public float TimeLoadOpenAds;
        public float TimeAutoCloseAge;
        public float TimeShowCloseAge;

        [Header("ADS CONFIG"), Space]
        public string AdsConfig;
        public string AdsUnit;

        [Header("GAME CONFIG"), Space]
        public string GameConfig;


        public void ApplyJsonToObject(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("JSON is empty!");
                return;
            }

            var dict = UnityEngine.Purchasing.MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            if (dict == null)
            {
                return;
            }

            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (dict.ContainsKey(field.Name))
                {
                    object jsonValue = dict[field.Name];

                    try
                    {
                        object converted = Convert.ChangeType(jsonValue, field.FieldType);
                        field.SetValue(this, converted);
                    }
                    catch
                    {
                        Debug.LogWarning($"Failed to assign value for field: {field.Name}");
                    }
                }
            }
        }
    }
}