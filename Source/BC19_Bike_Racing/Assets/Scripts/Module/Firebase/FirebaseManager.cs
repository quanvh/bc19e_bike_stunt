using System;
using UnityEngine;


#if USE_FIREBASE
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System.Collections.Generic;
using System.Threading.Tasks;
#endif


namespace Bacon
{
    public class FirebaseManager : MonoBehaviour
    {
        [Header("CUSTOM LOG")]
        [Space] public bool PushAdmobRev;

        [Space] public bool PushAdmobIos;

        [Space] public bool PushAdmobAndroid;

        [Space] public bool PushAFCustom;

        [Header("DEBUG")]
        [Space] public bool logDebug;

        [Space] public bool logAdUnit;

        public static FirebaseManager Instance { get; private set; }

        [SerializeField]
        private FirebaseStatus status = FirebaseStatus.UnAvailable;
        public static FirebaseStatus Status
        {
            get
            {
                if (Instance)
                    return Instance.status;
                return FirebaseStatus.Faulted;
            }
            private set
            {
                if (Instance)
                    Instance.status = value;
            }
        }

        public static FirebaseStatus AnalyticStatus = FirebaseStatus.Initialing;
        public static FirebaseStatus RemoteStatus = FirebaseStatus.Initialing;


        private Action OnRemoteSuccess;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private DateTime timeStartStep;

        public void InitFirebase(Action _onSuccess)
        {
#if USE_FIREBASE
            timeStartStep = DateTime.Now.ToUniversalTime();
            OnRemoteSuccess = _onSuccess;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                AnalyticStatus = FirebaseStatus.Initialing;

                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    AnalyticStatus = FirebaseStatus.Initialized;
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    if (logDebug)
                        Debug.Log("[Firebase] Init firebase successful");
                    InitializeFirebase();
                }
                else
                {
                    AnalyticStatus = FirebaseStatus.UnkownError;
                    if (logDebug)
                        Debug.Log("[Firebase] Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
#else
            _onSuccess?.Invoke();
#endif
        }

#if USE_FIREBASE
        void InitializeFirebase()
        {
            RemoteStatus = FirebaseStatus.Initialing;
            RemoteConfig _remote = DataManager.Instance.remoteConfig;
            Dictionary<string, object> defaults = new Dictionary<string, object>
            {
                { "AdsConfig", _remote.AdsConfig },
                { "AdsUnit", _remote.AdsUnit },
                { "GameConfig", _remote.GameConfig },
            };

            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
            FetchFireBase();
        }

        public void FetchFireBase()
        {
            FetchDataAsync();
        }

        public Task FetchDataAsync()
        {
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWith(FetchComplete);
        }

        void FetchComplete(Task fetchTask)
        {
            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                        .ContinueWith(task => SetAllKeys());
                    break;
                case LastFetchStatus.Failure:
                    RemoteStatus = FirebaseStatus.UnkownError;
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Debug.Log("[Firebase Remote] Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            Debug.Log("[Firebase Remote] Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    RemoteStatus = FirebaseStatus.Pending;
                    Debug.Log("[Firebase Remote] Latest Fetch call still pending.");
                    break;
                default:
                    RemoteStatus = FirebaseStatus.UnkownError;
                    break;
            }
        }

        public void SetAllKeys()
        {
            if (DataManager.Instance)
            {
                RemoteConfig _remote = DataManager.Instance._player.remoteConfig;

                _remote.AdsConfig = FirebaseRemoteConfig.DefaultInstance.GetValue("AdsConfig").StringValue;
                _remote.AdsUnit = FirebaseRemoteConfig.DefaultInstance.GetValue("AdsUnit").StringValue;
                _remote.GameConfig = FirebaseRemoteConfig.DefaultInstance.GetValue("GameConfig").StringValue;

                _remote.ApplyJsonToObject(_remote.AdsConfig);
                _remote.ApplyJsonToObject(_remote.GameConfig);

                SetAds(_remote.AdsUnit);

                DataManager.Instance.remoteConfig = _remote;
                DataManager.Instance._player.remoteConfig = _remote;

                RemoteStatus = FirebaseStatus.Initialized;
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    FirebaseLogger.RegularEvent("Fetch_remote_success", (DateTime.Now.ToUniversalTime() - timeStartStep).Seconds);
                    OnRemoteSuccess?.Invoke();
                });
            }
        }

        private void SetAds(string _config)
        {
            StructAdsUnit _adsConfig = JsonUtility.FromJson<StructAdsUnit>(_config);
            foreach (AdmobInter item in AdsController.Instance.InterAds)
            {
                if (item.interName == InterName.Inter_Open)
                {
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Open))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Open);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Open_1))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Open_1, 1);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Open_2))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Open_2, 2);
                    }
                }
                else if (item.interName == InterName.Inter_Default)
                {
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Default))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Default);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Default_1))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Default_1, 1);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Inter_Default_2))
                    {
                        item.SetAdUnit(_adsConfig.Inter_Default_2, 2);
                    }
                }
            }
            foreach (MrecBanner item in AdsController.Instance.mrec_banner)
            {
                if (item.mrecName == MrecName.Mrec_Open)
                {
                    if (!string.IsNullOrEmpty(_adsConfig.Mrec_Open))
                    {
                        item.SetAdUnit(_adsConfig.Mrec_Open);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Mrec_Open_1))
                    {
                        item.SetAdUnit(_adsConfig.Mrec_Open_1, 1);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Mrec_Open_2))
                    {
                        item.SetAdUnit(_adsConfig.Mrec_Open_2, 2);
                    }
                }
                else if (item.mrecName == MrecName.Mrec_End_Level && !string.IsNullOrEmpty(_adsConfig.Mrec_Endlevel))
                {
                    item.SetAdUnit(_adsConfig.Mrec_Endlevel);
                }
                else if (item.mrecName == MrecName.Mrec_Start_Level && !string.IsNullOrEmpty(_adsConfig.Mrec_Startlevel))
                {
                    item.SetAdUnit(_adsConfig.Mrec_Startlevel);
                }
                if (item.mrecName == MrecName.Mrec_Default && !string.IsNullOrEmpty(_adsConfig.Mrec_Default))
                {
                    item.SetAdUnit(_adsConfig.Mrec_Default);
                }
            }
            foreach (AdmobReward item in AdsController.Instance.RewardAds)
            {
                if (item.rewardName == RewardName.Reward_Default)
                {
                    if (!string.IsNullOrEmpty(_adsConfig.Reward_Default))
                    {
                        item.SetAdUnit(_adsConfig.Reward_Default);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Reward_Default_1))
                    {
                        item.SetAdUnit(_adsConfig.Reward_Default_1, 1);
                    }
                    if (!string.IsNullOrEmpty(_adsConfig.Reward_Default_2))
                    {
                        item.SetAdUnit(_adsConfig.Reward_Default_2, 2);
                    }
                }
            }
            foreach (AdmobNative item in AdsController.Instance.NativeAds)
            {
                if (item.nativeName == NativeName.Native_Banner && !string.IsNullOrEmpty(_adsConfig.Native_Banner))
                {
                    item.SetAdUnit(_adsConfig.Native_Banner);
                }
            }
            if (!string.IsNullOrEmpty(_adsConfig.Open_Ad))
            {
                AdsController.Instance.SetOpenAdId(_adsConfig.Open_Ad);
            }

#if USE_FACEBOOK
            if (!string.IsNullOrEmpty(_adsConfig.FB_App))
            {
                Facebook.Unity.Settings.FacebookSettings.AppIds = new List<string>(new[] { _adsConfig.FB_App });
            }
            if (!string.IsNullOrEmpty(_adsConfig.FB_Token))
            {
                Facebook.Unity.Settings.FacebookSettings.ClientTokens = new List<string>(new[] { _adsConfig.FB_Token });
            }
#endif
        }
#endif
    }

    public class StructAdsUnit
    {
        [SerializeField] public string Inter_Open;
        [SerializeField] public string Inter_Open_1;
        [SerializeField] public string Inter_Open_2;
        [SerializeField] public string Inter_Default;
        [SerializeField] public string Inter_Default_1;
        [SerializeField] public string Inter_Default_2;
        [SerializeField] public string Mrec_Open;
        [SerializeField] public string Mrec_Open_1;
        [SerializeField] public string Mrec_Open_2;
        [SerializeField] public string Mrec_Default;
        [SerializeField] public string Mrec_Endlevel;
        [SerializeField] public string Mrec_Startlevel;
        [SerializeField] public string Reward_Default;
        [SerializeField] public string Reward_Default_1;
        [SerializeField] public string Reward_Default_2;
        [SerializeField] public string Open_Ad;
        [SerializeField] public string Native_Banner;

        [SerializeField] public string FB_App;
        [SerializeField] public string FB_Token;
    }

    public enum FirebaseStatus
    {
        UnAvailable,
        Checking,
        Available,
        Initialing,
        Initialized,
        Getting,
        Completed,
        Faulted,
        Canceled,
        TimeOut,
        NoInternet,
        UnkownError,
        Success,
        Fetching,
        Pending
    }
}