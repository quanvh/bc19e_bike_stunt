#if USE_FIREBASE
using Firebase.Analytics;
using System;
using System.Collections.Generic;
#endif
using UnityEngine;


namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region EVENT_FUNCTION
        public static void BuyItem(string name, string type, int level_id)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + BUY_ITEM + ", Parameter (" + "name: " + name + ", type: " + type + ", level_id: " + level_id + ")");

            Parameter[] parameters = new Parameter[]
            {
            new Parameter(ITEM_NAME, name),
            new Parameter(BUY_TYPE, type),
            new Parameter(LEVEL_ID,level_id.ToString()),
            };
            SendLog(BUY_ITEM, parameters);
#endif
        }

        public static void EarnVirtualCurrency(int value, string name)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + "EarnVirtualCurrency" + ", Parameter (" + "name: " + name + ", value: " + value + ")");

            SendLog(FirebaseAnalytics.EventEarnVirtualCurrency, new Parameter[]
                {
                new Parameter(FirebaseAnalytics.ParameterValue, value),
                new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name)
                }
            );
#endif
        }

        public static void SpendVirtualCurrency(int value, string name)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + "SpendVirtualCurrency" + ", Parameter (" + "name: " + name + ", value: " + value + ")");

            SendLog(FirebaseAnalytics.EventSpendVirtualCurrency, new Parameter[]
                {
                new Parameter(FirebaseAnalytics.ParameterValue, value),
                new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name)
                }
            );
#endif
        }

        public static void BuyCar(int mode_id, int level_id, int car_id, string type_buy)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + BUY_CAR + car_id + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ",type_buy: " + type_buy + ")");

            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(BUY_TYPE, type_buy),
            new Parameter(LEVEL_ID,level_id.ToString()),
            };
            SendLog(BUY_CAR + car_id, parameters);
#endif
        }

        public static void DayRetention(int day, int level_id)
        {
#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(LEVEL_ID, level_id.ToString())
            };
            SendLog("D" + day.ToString() + RETENTION, parameters);
#endif
        }

        public static void InappPurchase(string sku, string name)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + INAPP_PURCHASE + ", Parameter (" + "sku: " + sku + ",name: " + name + ")");

            Parameter[] parameters = new Parameter[]
            {
            new Parameter(INAPP_SKU,sku),
            new Parameter(INAPP_NAME,name),
            };
            SendLog(INAPP_PURCHASE, parameters);
#endif
        }

        public static void ClickButton(string name, int level_id = 0)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + CLICK_BUTTON + ", Parameter (" + "name: " + name + ",level_id: " + level_id + ")");

            Parameter[] parameters = new Parameter[]
            {
            new Parameter(BUTTON_NAME,name),
            new Parameter(LEVEL_ID,level_id.ToString()),
            };
            SendLog(CLICK_BUTTON, parameters);
#endif
        }

        public static void RegularEvent(string name, float _time = 0)
        {
#if USE_FIREBASE
            if (LogDebug)
                Debug.Log(prefix + name + ", Parameter (" + ")");

            Parameter[] parameters = new Parameter[] {
            new Parameter("time_wait",_time.ToString())
        };
            SendLog(name, parameters);
#endif
        }

        #endregion

#if USE_FIREBASE
        private static void SendLog(string logName, Parameter[] param = null)
        {
            try
            {
                List<Parameter> lst = new List<Parameter>();
                if (param != null)
                {
                    foreach (Parameter _param in param)
                    {
                        lst.Add(_param);
                    }
                }
                //lst.Add(new Parameter(DEVICE_ID, SystemInfo.deviceUniqueIdentifier));
                //lst.Add(new Parameter(TIME_STAMP, DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks));
                //lst.Add(new Parameter(NETWORK, Application.internetReachability.ToString()));

                if (FirebaseManager.AnalyticStatus == FirebaseStatus.Initialized)
                    FirebaseAnalytics.LogEvent(logName, lst.ToArray());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
#endif
    }
}