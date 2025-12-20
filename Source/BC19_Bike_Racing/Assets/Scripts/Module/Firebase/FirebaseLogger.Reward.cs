#if USE_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private static readonly string AD_REWARD_CALL = "Ad_reward_call";
        private static readonly string AD_REWARD_READY = "Ad_reward_call_available";
        private static readonly string AD_REWARD_CALL_FAIL = "Ad_reward_call_failed";
        private static readonly string AD_REWARD_SHOW = "Ad_reward_show_Active";
        private static readonly string AD_REWARD_SHOW_SUCCESS = "Ad_reward_show_Success";
        private static readonly string AD_REWARD_SHOW_FAIL = "Ad_reward_show_Failed";
        private static readonly string AD_REWARD_SHOW_NOT_READY = "Ad_reward_show_unavailable";


        public static void RewardCall(string typeAds, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_CALL + ", Parameter (" + "typeAds: " + typeAds + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_REWARD_CALL, parameters);
#endif
        }

        public static void RewardReady(string typeAds, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_READY + ", Parameter (" + "typeAds: " + typeAds + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_REWARD_READY, parameters);
#endif
        }

        public static void RewardCallFail(string typeAds, int errorCode, string adUnit = "")
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_CALL_FAIL + ", Parameter (" + "typeAds: " + typeAds + ", errorCode: " + errorCode
                    + ", adUnit: " + adUnit + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(ERROR_CODE, errorCode.ToString()),
                new Parameter(AD_ID, LogAdUnit ? adUnit : ""),
            };
            SendLog(AD_REWARD_CALL_FAIL, parameters);
#endif
        }

        public static void RewardShow(string typeAds, string place, int level_id, int mode_id)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_SHOW + ", Parameter (" + "typeAds: " + typeAds + ", place: " + place
                    + ", level_id: " + level_id + ", mode_id: " + mode_id + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(PLACEMENT, place),
                new Parameter(LEVEL_ID, level_id.ToString()),
                new Parameter(MODE_ID, mode_id.ToString()),
            };
            SendLog(AD_REWARD_SHOW, parameters);
#endif
        }

        public static void RewardShowSuccess(string typeAds, string place, int level_id, int mode_id)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_SHOW_SUCCESS + ", Parameter (" + "typeAds: " + typeAds
                    + ", place: " + place + ", level_id: " + level_id + ", mode_id: " + mode_id + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(PLACEMENT, place),
                new Parameter(LEVEL_ID, level_id.ToString()),
                new Parameter(MODE_ID, mode_id.ToString()),
            };
            SendLog(AD_REWARD_SHOW_SUCCESS, parameters);
#endif
        }

        public static void RewardShowFail(string typeAds, string place, int level_id, int mode_id, int errorCode)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_SHOW_FAIL + ", Parameter (" + "typeAds: " + typeAds + ", place: " + place
                    + ", level_id: " + level_id + ", mode_id: " + mode_id + ", errorCode: " + errorCode + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(PLACEMENT, place),
                new Parameter(LEVEL_ID, level_id.ToString()),
                new Parameter(MODE_ID, mode_id.ToString()),
                new Parameter(ERROR_CODE, errorCode.ToString()),
            };
            SendLog(AD_REWARD_SHOW_FAIL, parameters);
#endif
        }

        public static void RewardShowNotReady(string typeAds, string place, int level_id, int mode_id)
        {
            if (LogDebug)
                Debug.Log(prefix + AD_REWARD_SHOW_NOT_READY + ", Parameter (" + "typeAds: " + typeAds + ", place: " + place
                    + ", level_id: " + level_id + ", mode_id: " + mode_id + ")");
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
                new Parameter(TYPE_AD, typeAds),
                new Parameter(PLACEMENT, place),
                new Parameter(MODE_ID, mode_id.ToString()),
                new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(AD_REWARD_SHOW_NOT_READY, parameters);
#endif
        }

    }
}
