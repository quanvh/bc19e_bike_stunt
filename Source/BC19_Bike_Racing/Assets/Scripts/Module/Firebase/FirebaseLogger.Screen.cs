#if USE_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;


namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private static readonly string SCREEN_WIN_SHOW = "Screen_win_show";
        private static readonly string SCREEN_WIN_X3 = "Screen_win_x3";
        private static readonly string SCREEN_WIN_NEXT = "Screen_win_next";
        private static readonly string SCREEN_HOME_CLICK = "Screen_home_click";
        private static readonly string SCREEN_GET_CAR_ADS = "Screen_GetCarAds";
        private static readonly string SCREEN_FAIL = "Screen_failed";
        private static readonly string SCREEN_FAIL_RESUMEADS = "Screen_failed_resumeAds";
        private static readonly string SCREEN_FAIL_REPLAY = "Screen_failed_replay";
        private static readonly string SCREEN_FAIL_HOME = "Screen_failed_home";
        private static readonly string SCREEN_TIMEUP = "Screen_timeup";
        private static readonly string SCREEN_TIMEUP_CONTINUEADS = "Screen_timeup_continueAds";
        private static readonly string SCREEN_TIMEUP_HOME = "Screen_timeup_home";
        private static readonly string SCREEN_TIMEUP_REPLAY = "Screen_timeup_replay";

        public static void ScreenWinShow(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenWinShow" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_WIN_SHOW, parameters);
#endif
        }

        public static void ScreenWinX3(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenWinX3" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_WIN_X3, parameters);
#endif
        }

        public static void ScreenWinNext(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenWinNext" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_WIN_NEXT, parameters);
#endif
        }

        public static void ScreenHomeClick(string button)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenHomeClick" + ", Parameter (" + "button: " + button + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(BUTTON_NAME, button),
            };
            SendLog(SCREEN_HOME_CLICK, parameters);
#endif
        }

        public static void ScreenGetCarAds(string button, int mode_id, int level_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenGetCarAds" + ", Parameter (" + "button: " + button + ", mode_id: " + mode_id + ", level_id: " + level_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(BUTTON_NAME, button),
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            };
            SendLog(SCREEN_GET_CAR_ADS, parameters);
#endif
        }

        public static void ScreenFail(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenFail" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_FAIL, parameters);
#endif
        }

        public static void ScreenFailResumeAds(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenFailResumeAds" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_FAIL_RESUMEADS, parameters);
#endif
        }

        public static void ScreenFailReplay(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenFailReplay" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_FAIL_REPLAY, parameters);
#endif
        }

        public static void ScreenFailHome(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenFailHome" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_FAIL_HOME, parameters);
#endif
        }

        public static void ScreenTimeup(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenTimeup" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_TIMEUP, parameters);
#endif
        }

        public static void ScreenTimeupContinueAds(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenTimeupContinueAds" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_TIMEUP_CONTINUEADS, parameters);
#endif
        }

        public static void ScreenTimeupHome(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenTimeupHome" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_TIMEUP_HOME, parameters);
#endif
        }

        public static void ScreenTimeupReplay(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "ScreenTimeupReplay" + ", Parameter (" + "mode_id: " + mode_id + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(SCREEN_TIMEUP_REPLAY, parameters);
#endif
        }

    }
}