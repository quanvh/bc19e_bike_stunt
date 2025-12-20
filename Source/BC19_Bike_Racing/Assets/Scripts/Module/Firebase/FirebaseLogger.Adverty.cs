#if USE_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;


namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private static readonly string AD_INGAME_ID = "ad_adverty_id";
        private static readonly string AD_INGAME_NAME = "ad_adverty_name";


        private static readonly string AD_INGAME_ACTIVE = "Ad_adverty_active";
        private static readonly string AD_INGAME_ACTIVE_FAIL = "Ad_adverty_active_fail";
        private static readonly string AD_INGAME_DEACTIVE = "Ad_adverty_deactive";
        private static readonly string AD_INGAME_VIEWED = "Ad_adverty_viewed";

        public static void AdIngameActive(int adId, string adName, int level_id)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(AD_INGAME_ID, adId.ToString()),
            new Parameter(AD_INGAME_NAME, adName),
            new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(AD_INGAME_ACTIVE, parameters);
#endif
        }

        public static void AdIngameActiveFail(int adId, string adName, int level_id)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(AD_INGAME_ID, adId.ToString()),
            new Parameter(AD_INGAME_NAME, adName),
            new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(AD_INGAME_ACTIVE_FAIL, parameters);
#endif
        }

        public static void AdIngameDeactive(int adId, string adName, int level_id)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(AD_INGAME_ID, adId.ToString()),
            new Parameter(AD_INGAME_NAME, adName),
            new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(AD_INGAME_DEACTIVE, parameters);
#endif
        }

        public static void AdIngameViewed(int adId, string adName, int level_id)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(AD_INGAME_ID, adId.ToString()),
            new Parameter(AD_INGAME_NAME, adName),
            new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(AD_INGAME_VIEWED, parameters);
#endif
        }

    }
}
