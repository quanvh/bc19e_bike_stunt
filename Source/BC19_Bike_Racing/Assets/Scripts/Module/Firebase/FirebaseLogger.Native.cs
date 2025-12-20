#if USE_FIREBASE
using Firebase.Analytics;

#endif
using UnityEngine;

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private static readonly string NATIVE_CALL = "Ad_native_call";
        private static readonly string NATIVE_READY = "Ad_native_ready";
        private static readonly string NATIVE_CALL_FAIL = "Ad_native_call_fail";
        private static readonly string NATIVE_IMPRESSION = "Ad_native_impression";


        public static void NativeCall(string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + NATIVE_CALL + ", Parameter (" + "adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(NATIVE_CALL, parameters);
#endif
        }

        public static void NativeCallFail(string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + NATIVE_CALL_FAIL + ", Parameter (" + "adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(NATIVE_CALL_FAIL, parameters);
#endif
        }

        public static void NativeReady(string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + NATIVE_READY + ", Parameter (" + "adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(NATIVE_READY, parameters);
#endif
        }

        public static void NativeImpression(string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + NATIVE_IMPRESSION + ", Parameter (" + "adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(NATIVE_IMPRESSION, parameters);
#endif
        }

    }
}