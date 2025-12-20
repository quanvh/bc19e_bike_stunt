using System;
using UnityEngine;

#if USE_TRIP
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using UnityEditor;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
#if USE_TRIP
    private AD_PLATFORM ad_platform = AD_PLATFORM.NONE;
    private Version currentVersion;

    [Header("LOCAL_FILE"), Space]
    [SerializeField, Space] private TextAsset androidAdUnit;
    [SerializeField, Space] private TextAsset iosAdUnit;
    [SerializeField, Space] private string localIrsKey = string.Empty;
    [SerializeField, Space] private List<string> localInterDefault;
    [SerializeField, Space] private List<string> localRewardDefault;
    [SerializeField, Space] private List<string> localBannerDefault;
    [SerializeField, Space] private string localBannerExtra;
    [SerializeField, Space] private List<string> localMrecDefault;
    [SerializeField, Space] private List<string> localInterOpen;
    [SerializeField, Space] private List<string> localMrecOpen;
    [SerializeField, Space] private List<string> localOpenAd;
    [SerializeField, Space] private List<string> localNativeOpen;
    [SerializeField, Space] private string localNativeDefault;
    [SerializeField, Space] private List<string> localNativeInter;
    [SerializeField, Space] private List<string> localNativeAge;
#endif


        #region AD_UNIT
#if USE_TRIP
    [SerializeField] private AdsConfig configData;

    private readonly string token = "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";
    //private readonly string uriPrefix = "https://raw.githubusercontent.com/anthais/xtool/main/";
    private readonly string uriPrefix = "raw.githubusercontent.com/tripsoftreport/xtool/master/";
    public IEnumerator RequestAdConfig()
    {
        string uri = "https://" + token + "@" + uriPrefix + Application.identifier + "_" + ad_platform + ".json";
        if (logDebug)
        {
            Debug.Log(logPrefix + "Request uri: " + uri);
        }
        using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                if (logDebug)
                    Debug.LogError(logPrefix + "Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                if (logDebug)
                    Debug.LogError(logPrefix + "HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                if (logDebug)
                    Debug.Log(logPrefix + "Received: " + webRequest.downloadHandler.text);
                SetAdData(webRequest.downloadHandler.text, true);
                break;
        }
    }

    private readonly int maxIds = 3;
    private void InitListEmpty(List<string> str)
    {
        for (int i = 0; i < maxIds; i++)
        {
            str.Add("");
        }
    }

    private void InitLocalId()
    {
        InitListEmpty(localInterDefault);
        InitListEmpty(localRewardDefault);
        InitListEmpty(localBannerDefault);
        InitListEmpty(localMrecDefault);
        InitListEmpty(localInterOpen);
        InitListEmpty(localMrecOpen);
        InitListEmpty(localNativeOpen);
        InitListEmpty(localNativeAge);
        InitListEmpty(localNativeInter);
        InitListEmpty(localOpenAd);

    }

    public void LoadAdConfig()
    {
        InitLocalId();

        ad_platform = GetBuildTarget();

        if (File.Exists(Path.Combine(Application.persistentDataPath + "/adUnitIds.json")))
        {
            string saveString = File.ReadAllText(Path.Combine(Application.persistentDataPath + "/adUnitIds.json"));
            configData = JsonUtility.FromJson<AdsConfig>(saveString);
            if (logDebug)
            {
                Debug.Log(logPrefix + "Data exist: " + configData.ToString());
            }
        }
        else
        {
#if UNITY_IOS
            configData = JsonUtility.FromJson<AdsConfig>(iosAdUnit.text);
#else
            configData = JsonUtility.FromJson<AdsConfig>(androidAdUnit.text);
#endif
            if (logDebug)
            {
                Debug.Log(logPrefix + "Get data from resource: " + configData.ToString());
            }
        }


        if (configData.code.Equals("200") && Enum.TryParse(configData.game.platform, out AD_PLATFORM _platform)
            && _platform == ad_platform
            && Application.identifier.ToLower().Equals(configData.game.packageId.ToLower())
            )
        {
            if (logDebug)
            {
                Debug.Log(logPrefix + "Check data success, platform: " + configData.game.platform +
                    ", packageID: " + configData.game.packageId);
            }
            foreach (var _data in configData.versions)
            {
                {
                    currentVersion = new Version(_data.code);
                    if (logDebug)
                    {
                        Debug.Log(logPrefix + "Local version: " + currentVersion.Major + " - " + currentVersion.Minor
                            + " - " + currentVersion.Build + " - " + currentVersion.Revision);
                    }
                    foreach (var _mediation in _data.mediations)
                        SetMediation(_mediation);
                    break;
                }
            }
            StartCoroutine(RequestAdConfig());
        }
    }

    private void SetAdData(string _adsData, bool save = false)
    {
        try
        {
            var newData = JsonUtility.FromJson<AdsConfig>(_adsData);
            if (logDebug)
            {
                Debug.Log(logPrefix + " Recived data, code: " + newData.code + ", packageID: " + newData.game.packageId);
            }

            if (newData.code.Equals("200") && Enum.TryParse(newData.game.platform, out AD_PLATFORM _platform)
                && _platform == ad_platform
                && Application.identifier.ToLower().Equals(newData.game.packageId.ToLower()))
            {
                AdsData localVersion;
                localVersion = newData.versions.Where(t => currentVersion == new Version(t.code)).FirstOrDefault();

                if (localVersion == null || localVersion.code == null)
                {
                    localVersion = newData.versions.Where(t => t.code.Equals("0.0.0")).FirstOrDefault();
                    if (logDebug)
                    {
                        Debug.Log(logPrefix + "Get General version: " + localVersion.ToString());
                    }
                }
                else
                {
                    if (logDebug)
                    {
                        Debug.Log(logPrefix + "Matched version: " + localVersion.ToString());
                    }
                }
                if (localVersion != null && localVersion.code != null)
                {
                    if (save)
                    {
                        SaveAdConfig(localVersion);
                    }
                    foreach (var _mediation in localVersion.mediations)
                        SetMediation(_mediation);
                }
            }
        }
        catch (Exception ex)
        {
            if (logDebug)
                Debug.LogError(logPrefix + "Set Ad unit error: " + ex.Message);
        }
    }

    public void SaveAdConfig(AdsData adsData)
    {
        if (logDebug)
        {
            Debug.Log(logPrefix + " Save ads ID");
        }
        if (configData.versions.Length > 0)
        {
            configData.versions[0].mediations = adsData.mediations;
            foreach (var _data in configData.versions[0].mediations)
            {
                var onlineData = adsData.mediations.Where(t => t.code.Equals(_data.code)).FirstOrDefault();
                if (onlineData != null && onlineData.adUnits != null)
                {
                    _data.appCode = onlineData.appCode;
                    foreach (var _adUnit in _data.adUnits)
                    {
                        var onlineAdUnit = onlineData.adUnits.Where(t => t.placement.Equals(_adUnit.placement)).FirstOrDefault();
                        if (onlineAdUnit != null && onlineAdUnit.id != null)
                        {
                            _adUnit.name = onlineAdUnit.name;
                            _adUnit.id = onlineAdUnit.id;
                        }
                    }
                }
            }
        }


        File.WriteAllText(Path.Combine(Application.persistentDataPath + "/adUnitIds.json"),
            JsonUtility.ToJson(configData));
    }
#endif
        #endregion


#if USE_TRIP
    private AD_PLATFORM GetBuildTarget()
    {
#if UNITY_EDITOR
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            return AD_PLATFORM.ios;
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            return AD_PLATFORM.android;
        else return AD_PLATFORM.NONE;
#else
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return AD_PLATFORM.ios;
        else if (Application.platform == RuntimePlatform.Android)
            return AD_PLATFORM.android;
        else return AD_PLATFORM.NONE;
#endif
    }

    private void SetMediation(AdsMediation _mediation)
    {
        if (Enum.TryParse(_mediation.code, out AD_MEDIATION _mediationType))
        {
            if (logDebug)
            {
                Debug.Log(logPrefix + "Set mediation: " + _mediationType);
            }
            if (_mediationType == AD_MEDIATION.ADMOB)
                SetAdmobMediation(_mediation);
            else if (_mediationType == AD_MEDIATION.IRON)
                SetIronMediation(_mediation);
            else if (_mediationType == AD_MEDIATION.MAX)
                SetMaxMediation(_mediation);
        }
    }

    private void SetAdmobMediation(AdsMediation _mediation)
    {
#if USE_ADMOB
        foreach (var adUnit in _mediation.adUnits)
        {
            if (Enum.TryParse(adUnit.placement, out AD_PLACEMENT _placement))
            {
                switch (_placement)
                {
                    case AD_PLACEMENT.BannerDefaultHigh:
                        if (localBannerDefault.Count > 0)
                            localBannerDefault[0] = adUnit.id;
                        //localBannerExtra = adUnit.extra;
                        break;
                    case AD_PLACEMENT.BannerDefaultMedium:
                        if (localBannerDefault.Count > 1)
                            localBannerDefault[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.BannerDefaultAll:
                        if (localBannerDefault.Count > 2)
                            localBannerDefault[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterDefaultHigh:
                        if (localInterDefault.Count > 0)
                            localInterDefault[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterDefaultMedium:
                        if (localInterDefault.Count > 1)
                            localInterDefault[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterDefaultAll:
                        if (localInterDefault.Count > 2)
                            localInterDefault[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.RewardedDefaultHigh:
                        if (localRewardDefault.Count > 0)
                            localRewardDefault[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.RewardedDefaultMedium:
                        if (localRewardDefault.Count > 1)
                            localRewardDefault[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.RewardedDefaultAll:
                        if (localRewardDefault.Count > 2)
                            localRewardDefault[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecDefaultHigh:
                        if (localMrecDefault.Count > 0)
                            localMrecDefault[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecDefaultMedium:
                        if (localMrecDefault.Count > 1)
                            localMrecDefault[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecDefaultAll:
                        if (localMrecDefault.Count > 2)
                            localMrecDefault[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.AppOpenDefaultHigh:
                        if (localOpenAd.Count > 0)
                            localOpenAd[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.AppOpenDefaultMedium:
                        if (localOpenAd.Count > 1)
                            localOpenAd[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.AppOpenDefaultAll:
                        if (localOpenAd.Count > 2)
                            localOpenAd[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecOpenHigh:
                        if (localMrecOpen.Count > 0)
                            localMrecOpen[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecOpenMedium:
                        if (localMrecOpen.Count > 1)
                            localMrecOpen[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.MrecOpenAll:
                        if (localMrecOpen.Count > 2)
                            localMrecOpen[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterOpenHigh:
                        if (localInterOpen.Count > 0)
                            localInterOpen[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterOpenMedium:
                        if (localInterOpen.Count > 1)
                            localInterOpen[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.InterOpenAll:
                        if (localInterOpen.Count > 2)
                            localInterOpen[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeOpenHigh:
                        if (localNativeOpen.Count > 0)
                            localNativeOpen[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeOpenMedium:
                        if (localNativeOpen.Count > 1)
                            localNativeOpen[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeOpenAll:
                        if (localNativeOpen.Count > 2)
                            localNativeOpen[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeDefault:
                        localNativeDefault = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeInterHigh:
                        if (localNativeInter.Count > 0)
                            localNativeInter[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeInterMedium:
                        if (localNativeInter.Count > 1)
                            localNativeInter[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeInterAll:
                        if (localNativeInter.Count > 2)
                            localNativeInter[2] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeAgeHigh:
                        if (localNativeAge.Count > 0)
                            localNativeAge[0] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeAgeMedium:
                        if (localNativeAge.Count > 1)
                            localNativeAge[1] = adUnit.id;
                        break;
                    case AD_PLACEMENT.NativeAgeAll:
                        if (localNativeAge.Count > 2)
                            localNativeAge[2] = adUnit.id;
                        break;

                    default: break;
                }
            }
        }
#endif
    }

    private void SetIronMediation(AdsMediation _mediation)
    {
#if USE_IRON
        localIrsKey = _mediation.appCode;
#endif
    }

    private void SetMaxMediation(AdsMediation _mediation)
    {
#if USE_MAX

#endif
    }
#endif
    }

    [Serializable]
    public class AdsConfig
    {
        public string code;
        public AdsGameData game;
        public AdsData[] versions;
    }


    [Serializable]
    public class AdsGameData
    {
        public string platform;
        public string packageId;
    }

    [Serializable]
    public class AdsData
    {
        public string code;
        public AdsMediation[] mediations;
    }

    [Serializable]
    public class AdsMediation
    {
        public string code;
        public string appCode;
        public AdsUnit[] adUnits;
    }

    [Serializable]
    public class AdsUnit
    {
        public string id;
        public string name;
        public string placement;
        public string format;
        //public string extra;
    }

    public enum AD_PLATFORM
    {
        NONE = 0,
        android = 1,
        ios = 2,
    }

    public enum AD_MEDIATION
    {
        NONE = 0,
        MAX = 1,
        IRON = 2,
        ADMOB = 3
    }

    public enum AD_PLACEMENT
    {
        None = 0,
        InterDefault = 1,
        RewardedDefault = 2,
        MrecDefault = 3,
        BannerDefault = 4,
        InterOpen = 5,
        MrecOpen = 6,
        AppOpenDefault = 7,
        NativeOpen = 8,
        NativeResult = 9,
        NativeDaily = 10,
        NativeLevel = 11,
        NativeIngame = 12,
        NativeDefault = 13,
        NativeInter = 14,
        NativeAge = 15,

        InterDefaultHigh = 101,
        InterDefaultMedium = 102,
        InterDefaultAll = 103,

        RewardedDefaultHigh = 104,
        RewardedDefaultMedium = 105,
        RewardedDefaultAll = 106,

        MrecDefaultHigh = 107,
        MrecDefaultMedium = 108,
        MrecDefaultAll = 109,

        BannerDefaultHigh = 110,
        BannerDefaultMedium = 111,
        BannerDefaultAll = 112,

        InterOpenHigh = 113,
        InterOpenMedium = 114,
        InterOpenAll = 115,

        MrecOpenHigh = 116,
        MrecOpenMedium = 117,
        MrecOpenAll = 118,

        AppOpenDefaultHigh = 119,
        AppOpenDefaultMedium = 120,
        AppOpenDefaultAll = 121,

        NativeOpenHigh = 122,
        NativeOpenMedium = 123,
        NativeOpenAll = 124,

        NativeAgeHigh = 125,
        NativeAgeMedium = 126,
        NativeAgeAll = 127,

        NativeInterHigh = 128,
        NativeInterMedium = 129,
        NativeInterAll = 130
    }

    public enum AD_FORMAT
    {
        None = 0,
        Interstitial = 1,
        Rewarded = 2,
        Banner = 3,
        AppOpen = 5,
        Native = 6
    }

}