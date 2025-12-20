using UnityEngine;

namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
#if USE_FIREBASE
        #region VARIABLE_NAME
        //private static readonly string NETWORK = "network";
        private static readonly string MODE_ID = "mode_id";
        private static readonly string LEVEL_ID = "level_id";
        private static readonly string CAR_ID = "car_id";
        private static readonly string STAR = "star";
        private static readonly string TIME_PLAY = "time_play";
        private static readonly string TYPE_AD = "TypeAds";
        private static readonly string ERROR_CODE = "Error";
        private static readonly string PLACEMENT = "Placement";
        private static readonly string BUTTON_NAME = "name";
        private static readonly string INAPP_SKU = "sku";
        private static readonly string INAPP_NAME = "name";
        private static readonly string ITEM_NAME = "name_item";
        private static readonly string BUY_TYPE = "type_buy";
        private static readonly string AD_ID = "ad_id";
        private static readonly string FAIL_REASON = "fail_type";
        #endregion

        #region TEXT_VALUE
#if USE_IRON
        private static readonly string AD_PLATFORM = "ironSource";
#endif
        private static readonly string CURRENCY = "USD";
        #endregion

        #region FUNCTION_NAME

        private static readonly string INAPP_PURCHASE = "InappPurchase";
        private static readonly string CLICK_BUTTON = "ClickButton";
        private static readonly string RETENTION = "Retention";
        private static readonly string BUY_ITEM = "BuyItem_Character";
        private static readonly string BUY_CAR = "Buy_Car_";
        #endregion

        #region AD_EVENT
        private static readonly string IR_INIT_SUCCESS = "Init_irons_success";
        private static readonly string IR_INIT_FAIL = "Init_irons_failed";
        private static readonly string MAX_INIT_SUCCESS = "Init_max_success";
        private static readonly string MAX_INIT_FAIL = "Init_max_failed";

        //private static readonly string AD_PAID = "paid_ad_impression";
        private static readonly string AD_PAID = "ad_impression";
        private static readonly string AD_ROAS_DAILY = "Daily_Ads_Revenue";
#if UNITY_IOS
        private static readonly string AD_PAID_IOS = "ad_impression_ios";
#endif
#if UNITY_ANDROID
        private static readonly string AD_PAID_ANDROID = "ad_impression_android";
#endif
#if USE_APPSFLYER
        private static readonly string AD_PAID_CUSTOM = "ad_imperssion_custom";
#endif

        #endregion
#endif

        private static readonly string prefix = "===[Firebase Log] Send event: ";

        private static bool LogDebug
        {
            get
            {
                if (FirebaseManager.Instance)
                    return FirebaseManager.Instance.logDebug;
                else return false;
            }
        }

        private static bool LogAdUnit
        {
            get
            {
                if (FirebaseManager.Instance)
                    return FirebaseManager.Instance.logAdUnit;
                else return false;
            }
        }

        private static int LogLevelDetail = 10;
    }
}