#if USE_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;


namespace Bacon
{
    public partial class FirebaseLogger : MonoBehaviour
    {
        private static readonly string LEVEL_START = "Level_all_start";
        private static readonly string LEVEL_RESTART = "Level_all_restart";
        private static readonly string LEVEL_FAIL = "Level_all_fail";
        private static readonly string LEVEL_FAIL_TIMEUP = "Level_all_failed_timeup";
        private static readonly string LEVEL_FAIL_CRASHED = "Level_all_failed_crashed";
        private static readonly string LEVEL_SUCCESS = "Level_all_win";
        private static readonly string LEVEL_REVIVE = "Level_all_revive";

        public static void LevelStart(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelStart" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(LEVEL_START, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_start", level_id));
                SendLog(string.Format("Level_{0:00}_start", level_id));
            }
#endif
        }

        public static void LevelSuccess(int mode_id, int level_id, int car_id, int star = 3, float _time = 0)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelSuccess" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            new Parameter(STAR, star.ToString()),
            new Parameter(TIME_PLAY, _time),
            };
            SendLog(LEVEL_SUCCESS, parameters);
            LevelUp(level_id, car_id);


            //Log level detail
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_win", level_id) + ", Parameter (" +
                        "car_id: " + car_id + ", time_play: " + _time + ")");

                SendLog(string.Format("Level_{0:00}_win", level_id), new Parameter[]
                    {
                    new Parameter(CAR_ID, car_id.ToString()),
                    new Parameter(TIME_PLAY, _time),
                    });
            }

            //Log level by star and time
            SendLog(string.Format("Level_{0:00}_{1:00}", level_id, star), new Parameter[]
                {
                new Parameter(CAR_ID, car_id.ToString()),
                new Parameter(TIME_PLAY, _time),
                });
#endif
        }

        public static void LevelUp(int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelUp" + ", Parameter (" + ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            SendLog(FirebaseAnalytics.EventLevelUp, new Parameter[]
            {
            new Parameter(FirebaseAnalytics.ParameterLevel,level_id.ToString()),
            new Parameter(FirebaseAnalytics.ParameterCharacter,car_id.ToString()),
            });
#endif
        }

        public static void LevelFail(int mode_id, int level_id, int car_id, string failType = "crash")
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelFail" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ",fail_type: " + failType + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            new Parameter(FAIL_REASON, failType),
            };
            SendLog(LEVEL_FAIL, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_failed", level_id));
                SendLog(string.Format("Level_{0:00}_failed", level_id));
            }
#endif
        }

        public static void LevelFailTimeup(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelFailTimeup" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(LEVEL_FAIL_TIMEUP, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_failed_timeup", level_id));
                SendLog(string.Format("Level_{0:00}_failed_timeup", level_id));
            }
#endif
        }

        public static void LevelFailCrashed(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelFailCrashed" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(LEVEL_FAIL_CRASHED, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_failed_crashed", level_id));
                SendLog(string.Format("Level_{0:00}_failed_crashed", level_id));
            }
#endif
        }

        public static void LevelRestart(int mode_id, int level_id, int car_id)
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelRestart" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            };
            SendLog(LEVEL_RESTART, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_restart", level_id));
                SendLog(string.Format("Level_{0:00}_restart", level_id));
            }
#endif
        }

        public static void LevelRevive(int mode_id, int level_id, int car_id, string failType = "crash")
        {
            if (LogDebug)
                Debug.Log(prefix + "LevelRevive" + ", Parameter (" + "mode_id: " + mode_id +
                    ", level_id: " + level_id + ", car_id: " + car_id + ", failType: " + failType + ")");

#if USE_FIREBASE
            Parameter[] parameters = new Parameter[]
            {
            new Parameter(MODE_ID, mode_id.ToString()),
            new Parameter(LEVEL_ID, level_id.ToString()),
            new Parameter(CAR_ID, car_id.ToString()),
            new Parameter(FAIL_REASON, failType),
            };
            SendLog(LEVEL_REVIVE, parameters);
            if (level_id <= LogLevelDetail)
            {
                if (LogDebug)
                    Debug.Log(prefix + string.Format("Level_{0:00}_revive_{1}", level_id, failType));
                SendLog(string.Format("Level_{0:000}_revive_{1}", level_id, failType));
            }
#endif
        }

    }
}