#if USE_FIREBASE
using Firebase.Analytics;
using System.Linq;
using Unity.VisualScripting;

#endif
using UnityEngine;

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {

        private static readonly string AD_BANNER_CALL = "Ad_banner_call";
        private static readonly string AD_BANNER_READY = "Ad_banner_ready";
        private static readonly string AD_BANNER_CALL_FAIL = "Ad_banner_call_fail";


        public static void BannerCall(string typeAds, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_BANNER_CALL + ", Parameter (" + "typeAds: " + typeAds + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_BANNER_CALL, parameters);
#endif
        }

        public static void BannerReady(string typeAds, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_BANNER_READY + ", Parameter (" + "typeAds: " + typeAds + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_BANNER_READY, parameters);
#endif
        }

        public static void BannerCallFail(string typeAds, int errorCode, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_BANNER_CALL_FAIL + ", Parameter (" + "typeAds: " + typeAds + ", errorCode: " + errorCode
                    + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(ERROR_CODE, errorCode.ToString()),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_BANNER_CALL_FAIL, parameters);
#endif
        }

    }
}