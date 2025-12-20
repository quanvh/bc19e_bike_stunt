using UnityEngine;
using System;

#if USE_FIREBASE
using Firebase.Analytics;
#endif

#if USE_APPSFLYER
using AppsFlyerSDK;
using System.Collections.Generic;
#endif

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        public static void InitMaxSuccess()
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(MAX_INIT_SUCCESS, parameters);
#endif
        }

        public static void InitMaxFail(int errorCode)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(ERROR_CODE, errorCode.ToString()),
            };
            SendLog(MAX_INIT_FAIL, parameters);
#endif
        }

        public static void InitIrsSuccess()
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(IR_INIT_SUCCESS, parameters);
#endif
        }

        public static void InitIrsFail(int errorCode)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(ERROR_CODE, errorCode.ToString()),
            };
            SendLog(IR_INIT_FAIL, parameters);
#endif
        }

#if USE_FIREBASE && USE_IRON
        public static void OnImpressAds(IronSourceImpressionData impressionData)
        {
#if USE_APPSFLYER && USE_APPSFLYER_CONNECTOR
            Dictionary<string, string> dic = new Dictionary<string, string> {
            { "AdUnitID", impressionData.adUnit },
        };

            AppsFlyerAdRevenue.logAdRevenue(impressionData.adNetwork,
                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
                (double)impressionData.revenue, "USD", dic);
#endif

            Parameter[] parameters = new Parameter[]
            {
            new Parameter(FirebaseAnalytics.ParameterAdPlatform, AD_PLATFORM),
            new Parameter(FirebaseAnalytics.ParameterAdSource, impressionData.adNetwork),
            new Parameter(FirebaseAnalytics.ParameterAdFormat, impressionData.adUnit),
            new Parameter(FirebaseAnalytics.ParameterAdUnitName, impressionData.instanceName),
            new Parameter(FirebaseAnalytics.ParameterCurrency, CURRENCY),
            new Parameter(FirebaseAnalytics.ParameterValue, (double)impressionData.revenue),
            };
            SendLog(AD_PAID, parameters);
        }
#endif

#if USE_FIREBASE && USE_MAX
    public static void OnImpressAds(MaxSdkBase.AdInfo impressionData)
    {
        double revenue = impressionData.Revenue;

        Parameter[] parameters = new Parameter[]
                    {
            new Parameter(FirebaseAnalytics.ParameterAdPlatform, "AppLovin"),
            new Parameter(FirebaseAnalytics.ParameterAdSource, impressionData.NetworkName),
            new Parameter(FirebaseAnalytics.ParameterAdFormat,impressionData.Placement),
            new Parameter(FirebaseAnalytics.ParameterAdUnitName,impressionData.AdUnitIdentifier),
            new Parameter(FirebaseAnalytics.ParameterCurrency,"USD"),
            new Parameter(FirebaseAnalytics.ParameterValue,revenue),
                    };
        SendLog(AD_PAID, parameters);
    }
#endif

#if USE_ADMOB

        private static readonly float microValue = 1000000;
        public static void OnImpressAds(AdValue adValue, string adType = "")
        {
            if (LogDebug)
            {
                Debug.Log(String.Format(prefix + "Admob ad paid {0} {1}.",
                adValue.Value / microValue,
                adValue.CurrencyCode));
            }
#if USE_APPSFLYER && USE_APPSFLYER_CONNECTOR
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            var adRevenueData = new AFAdRevenueData("admob", MediationNetwork.GoogleAdMob, adValue.CurrencyCode, (double)adValue.Value / microValue);
            AppsFlyer.logAdRevenue(adRevenueData, additionalParams);

            if (FirebaseManager.Instance.PushAFCustom)
            {
                Dictionary<string, string> dicCustom = new Dictionary<string, string>
                {
                    { AFInAppEvents.REVENUE, ((double)adValue.Value / microValue).ToString() }
                };

                AppsFlyer.sendEvent(AD_PAID_CUSTOM, dicCustom);
            }
#endif

            Parameter[] parameters = new Parameter[]
                {
                    new Parameter(FirebaseAnalytics.ParameterCurrency, adValue.CurrencyCode),
                    new Parameter(FirebaseAnalytics.ParameterValue, (double)adValue.Value/microValue),
                };
            if (FirebaseManager.Instance.PushAdmobRev)
            {
                SendLog(AD_PAID, parameters);
            }

#if UNITY_ANDROID
            if (FirebaseManager.Instance.PushAdmobAndroid)
            {
                SendLog(AD_PAID_ANDROID, parameters);
            }
#elif UNITY_IOS
            if (FirebaseManager.Instance.PushAdmobIos)
            {
                SendLog(AD_PAID_IOS, parameters);
            }
#endif

        }
#endif

        public static void TroasRevenue(double revenue)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(FirebaseAnalytics.ParameterCurrency, CURRENCY),
            new Parameter(FirebaseAnalytics.ParameterValue,revenue),
            };
            SendLog(AD_ROAS_DAILY, parameters);
#endif
        }

        public static void OnImpressAdsAndroidNative(long adValue, string currencyCode = "")
        {
            if (LogDebug)
            {
                Debug.Log(String.Format(prefix + "Admob ad paid {0} {1}.",
                adValue / microValue,
                currencyCode));
            }
#if USE_APPSFLYER && USE_APPSFLYER_CONNECTOR
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            var adRevenueData = new AFAdRevenueData("admob", MediationNetwork.GoogleAdMob, currencyCode, (double)adValue / microValue);
            AppsFlyer.logAdRevenue(adRevenueData, additionalParams);

            if (FirebaseManager.Instance.PushAFCustom)
            {
                Dictionary<string, string> dicCustom = new Dictionary<string, string>
                {
                    { AFInAppEvents.REVENUE, ((double)adValue / microValue).ToString() }
                };

                AppsFlyer.sendEvent(AD_PAID_CUSTOM, dicCustom);
            }
#endif

            Parameter[] parameters = new Parameter[]
                {
                    new Parameter(FirebaseAnalytics.ParameterCurrency, currencyCode),
                    new Parameter(FirebaseAnalytics.ParameterValue, (double)adValue/microValue),
                };
            if (FirebaseManager.Instance.PushAdmobRev)
            {
                SendLog(AD_PAID, parameters);
            }

#if UNITY_ANDROID
            if (FirebaseManager.Instance.PushAdmobAndroid)
            {
                SendLog(AD_PAID_ANDROID, parameters);
            }
#elif UNITY_IOS
            if (FirebaseManager.Instance.PushAdmobIos)
            {
                SendLog(AD_PAID_IOS, parameters);
            }
#endif
        }

    }
}