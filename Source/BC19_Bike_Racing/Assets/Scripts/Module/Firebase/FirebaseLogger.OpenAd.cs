#if USE_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {

        private static readonly string AD_OPEN_CALL = "Ad_open_call";
        private static readonly string AD_OPEN_READY = "Ad_open_ready";
        private static readonly string AD_OPEN_CALL_FAIL = "Ad_open_call_fail";
        private static readonly string AD_OPEN_SHOW = "Ad_open_show";
        private static readonly string AD_OPEN_SHOW_FAIL = "Ad_open_show_fail";
        private static readonly string AD_OPEN_SHOW_SUCCESS = "Ad_open_show_success";
        private static readonly string AD_OPEN_NOT_READY = "Ad_open_not_ready";


        public static void OpenadsCall()
        {

            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_CALL + ", Parameter ()");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(AD_OPEN_CALL, parameters);
#endif
        }

        public static void OpenadsReady()
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_READY + ", Parameter ()");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(AD_OPEN_READY, parameters);
#endif
        }

        public static void OpenadsFail(int errorCode)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_CALL_FAIL + ", Parameter ( errorCode: " + errorCode + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(ERROR_CODE, errorCode.ToString()),
            };
            SendLog(AD_OPEN_CALL_FAIL, parameters);
#endif
        }

        public static void OpenadsShow()
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_SHOW + ", Parameter ()");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(AD_OPEN_SHOW, parameters);
#endif
        }

        public static void OpenadsShowFail(int errorCode)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_SHOW_FAIL + ", Parameter (" + "errorCode: " + errorCode + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(ERROR_CODE, errorCode.ToString()),
            };
            SendLog(AD_OPEN_SHOW_FAIL, parameters);
#endif
        }

        public static void OpenadsShowSuccess()
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_SHOW_SUCCESS + ", Parameter ()");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(AD_OPEN_SHOW_SUCCESS, parameters);
#endif
        }

        public static void OpenadsNotReady()
        {
            if (LogDebug)
                Debug.Log(prefix + AD_OPEN_NOT_READY + ", Parameter ()");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[] { };
            SendLog(AD_OPEN_NOT_READY, parameters);
#endif
        }

    }
}