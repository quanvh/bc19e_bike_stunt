using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        private bool firstStart;
        private DateTime lastAdTime;

        [HideInInspector] public bool hideApp = false;
        [HideInInspector] public int adCount = 0;
        [HideInInspector] public bool gameLoaded = false;

        private readonly float defaultDuration = 5f;
        private string interPlacement = "start";
        private string rewardPlacement = "";

        private readonly string ironsrc = "ironsrc";
#if USE_MAX
        private readonly string maxsdk = "max";
#endif
#if USE_MAX || USE_IRON
        private bool grantReward = false;
        private Action _OnRewardComplete;
        private Action _OnInterComplete;
#endif

        public Action OnFirstAdsLoaded;

        public static AdsController Instance = null;

        private readonly string logPrefix = "[AdsController] ";

        [SerializeField] private AD_MEDIATION Mediation;
        [SerializeField, Space, HideIf("Mediation", AD_MEDIATION.ADMOB)] private bool disableMediation = false;
        [SerializeField] private bool iOSPause = false;
        [SerializeField] private bool RaiseOnMainThread = true;
        [SerializeField] private bool ExecuteInUpdate = true;

        [Space, Header("SMALL BANNER")]
        public bool useSmallBanner = false;

        [FoldoutGroup("Banner Config", expanded: false), Space]
        [ShowIf("useSmallBanner")] public bool collapseBanner = false;

        [SerializeField, FoldoutGroup("Banner Config"), Space, ShowIf("useSmallBanner")]
        private bool adaptiveBanner = false;

#if USE_ADMOB
        [SerializeField, FoldoutGroup("Banner Config"), Space, ShowIf("useSmallBanner")]
        private AdPosition small_banner_pos = AdPosition.Bottom;
#endif
        [SerializeField, FoldoutGroup("Banner Config"), Space, ShowIf("useSmallBanner")]
        private List<string> small_banner_android;

        [SerializeField, FoldoutGroup("Banner Config"), Space, ShowIf("useSmallBanner")]
        private List<string> small_banner_ios;


        [Header("MREC"), Space]
        public bool useMrecOpen = true;
        public List<MrecBanner> mrec_banner;


        [Space, Header("ADMOB INTER")]
        public List<AdmobInter> InterAds;


        [Header("ADMOB REWARD"), Space]
        public List<AdmobReward> RewardAds;


        [Header("OPEN ADS"), Space]
        public bool useAppOpen = true;

        [FoldoutGroup("OpenAds Config", expanded: false), Space]
        [SerializeField, ShowIf("useAppOpen")] private bool preloadAdOpen = false;
        [SerializeField, FoldoutGroup("OpenAds Config"), Space, ShowIf("useAppOpen")] private List<string> openad_android;
        [SerializeField, FoldoutGroup("OpenAds Config"), Space, ShowIf("useAppOpen")] private List<string> openad_ios;


        [Header("NATIVE"), Space]
        public List<AdmobNative> NativeAds;


#if USE_IRON
        [Space, Header("IRONSOURCE BANNER")]
        [SerializeField, ShowIf("Mediation", AD_MEDIATION.IRON)] private IronSourceBannerPosition banner_pos = IronSourceBannerPosition.BOTTOM;
#endif

        [Space, Header("DEBUG")]
        public bool logDebug = false;

        protected bool ShowAds = false;

        #region GETTER
        public int CurrentLevel
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.CurrentLevel.ID;
                else return 0;
            }
        }

        public int CurrentMode
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.CurrentMode;
                else return 1;
            }
        }

        public double DurationBetweenAds
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.DurationBetweenAds;
                else return defaultDuration;
            }
        }

        public bool RemoveAds
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.VipUser;
                else return false;
            }
        }

        public bool DisabledAds
        {
            get
            {
                if (DataManager.Instance)
                    return !DataManager.Instance.Remote.ShowInter;
                else return false;
            }
        }

        public bool UseAdmobInter
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobInter;
                else return false;
            }
        }

        public bool UseAdmobInterSplash
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobInterSplash;
                else return false;
            }
        }

        public bool UseAdmobReward
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobReward;
                else return false;
            }
        }

        public bool UseAdmobBanner
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobBanner;
                else return false;
            }
        }

        public bool UseAdmobMrec
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobMrec;
                else return false;
            }
        }

        public bool UseOpenAd
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseOpenAd;
                else return useAppOpen;
            }
        }

        public float TimeFirstInter
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeFirstInter;
                else return 1f;
            }
        }

        public bool UserOrganic
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.UserOrganic;
                else return false;
            }
        }


        public bool UseNative
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseNative;
                else return false;
            }
        }


        public int TimeLeftNativeAds
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeLeftNativeAds;
                else return 5;
            }
        }

        public int TimeVisibleClose
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeVisibleClose;
                else return 2;
            }
        }

        public int TimeEnableClose
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeEnableClose;
                else return 3;
            }
        }
        #endregion
    }


    public enum ADTYPE
    {
        NONE = 0,
        SPIN = 1,
        FREE_COIN = 2,
        SKIP_LEVEL = 3,
        REVIVE = 4,
        CLAIM_X3 = 5,
        UNLOCK_MODE = 6,
        CLAIM_CAR = 7,
        COMPLETE_X3 = 8,
        TIME_UP = 9,
        BUY_CAR = 10,
        BUY_CHAR = 11,
        BUY_HELMET = 12,
        BUY_WHEEL = 13,
        LUCKY_BOX = 14,
        CLAIM_CHAR = 15,
        CLAIM_HELMET = 16,
        CLAIM_WHEEL = 17,
        SPIN_CAR = 18,
        SPIN_CHAR = 19,
        SPIN_HELMET = 20,
        CLAIM_DISCOUNT = 21,
        REMOVE_ADS = 22,
        FREE_SHOP = 23,
    }

}