using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        public Action OnRewardLoaded;

        public Action OnRewardNotReady;

        public Action OnRewardLoadFail;

        public Action<ADTYPE> OnRewardComplete;

        public bool PreloadRewardDefault
        {
            get
            {
                AdmobReward _reward = RewardAds.Where(t => t.rewardName == RewardName.Reward_Default).First();
                if (_reward != null)
                {
                    return _reward.preload;
                }
                else return false;
            }
        }

        public void LoadRewardSuccess(RewardName _name)
        {
            if (_name == RewardName.Reward_Default)
            {
                OnRewardLoaded?.Invoke();
            }
        }

        public void LoadRewardFail(RewardName _name)
        {
            if (_name == RewardName.Reward_Default)
            {
                OnRewardLoadFail?.Invoke();
            }
        }

        public void RewardNotReady(RewardName _name)
        {
            if (_name == RewardName.Reward_Default)
            {
                OnRewardNotReady?.Invoke();
            }
        }

        public void RewardShowSuccess(RewardName _name)
        {
            if (_name == RewardName.Reward_Default)
            {
                OnRewardComplete?.Invoke(currentAdType);
            }
        }


        private bool AdmobRewardReady(RewardName _name = RewardName.Reward_Default)
        {
            AdmobReward _reward = RewardAds.Where(t => t.rewardName == _name).First();
            return _reward != null && _reward.IsReady();
        }

        public void LoadAdmobReward(RewardName _name)
        {
            if (!DisabledAds && UseAdmobReward)
            {
                AdmobReward _reward = RewardAds.Where(t => t.rewardName == _name).First();
                if (_reward != null && !_reward.isLoading)
                {
                    _reward.LoadAdmobReward();
                }
            }
        }

        private void ShowAdmobReward(Action OnSuccess = null, RewardName _name = RewardName.Reward_Default)
        {
            if (DisabledAds || !UseAdmobReward)
            {
                return;
            }

            AdmobReward _reward = RewardAds.Where(t => t.rewardName == _name).First();
            _reward?.ShowAdmobReward(() =>
            {
                OnSuccess?.Invoke();
            }, rewardPlacement);
        }

        public void PreloadAdmobReward(RewardName _name = RewardName.Reward_Default)
        {
            if (!DisabledAds && UseAdmobReward)
            {
                AdmobReward _reward = RewardAds.Where(t => t.rewardName == _name).First();
                if (_reward != null && !_reward.isLoading && _reward.preload)
                {
                    _reward.LoadAdmobReward();
                }
            }
        }
    }


    [Serializable]
    public class AdmobReward
    {
        [FoldoutGroup("@rewardName", expanded: false), Space] public RewardName rewardName;
        [FoldoutGroup("@rewardName"), Space] public bool preload;
        [FoldoutGroup("@rewardName"), SerializeField, Space] private List<string> rw_android;
        [FoldoutGroup("@rewardName"), SerializeField, Space] private List<string> rw_ios;

        [FoldoutGroup("@rewardName"), SerializeField, Space] private bool cappingFail = false;
        [FoldoutGroup("@rewardName"), SerializeField, Space, ShowIf("cappingFail")] private float cappingTime = 30;

        private int failCount = 0;

        private string Reward_name => rewardName.ToString();
        private readonly string logPrefix = "[AdsController] ";

        private string currentPlacement = "start";
        private bool LogDebug => AdsController.Instance.logDebug;


#if USE_ADMOB
        private RewardedAd rewardedAd;
#endif

        private bool DisabledAds => AdsController.Instance.DisabledAds;

        private bool UseAdmobReward => AdsController.Instance.UseAdmobReward;

        private int CurrentLevel => AdsController.Instance.CurrentLevel;

        private int CurrentMode => AdsController.Instance.CurrentMode;

        public bool IsReady()
        {
#if USE_ADMOB
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return false;
#endif
        }

        public void ShowAdmobReward(Action OnSuccess = null, string place = "")
        {
            if (DisabledAds || !UseAdmobReward)
            {
                return;
            }
            if (!string.IsNullOrEmpty(place))
            {
                currentPlacement = place;
            }
            else
            {
                currentPlacement = Reward_name;
            }

#if USE_ADMOB
            FirebaseLogger.RewardShow(Reward_name, currentPlacement, CurrentLevel, CurrentMode);
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                AdsController.Instance.MuteGameSound();
                rewardedAd.Show((Reward reward) =>
                {
                    OnSuccess?.Invoke();
                    AdsController.Instance.RewardShowSuccess(rewardName);
                });
            }
            else
            {
                FirebaseLogger.RewardShowNotReady(Reward_name, currentPlacement, CurrentLevel, CurrentMode);
                AdsController.Instance.RewardNotReady(rewardName);
                if (preload) LoadAdmobReward();
            }
#else
            OnSuccess?.Invoke();
#endif
        }

        private DateTime timeLoad = DateTime.MinValue;
        private int rewardRetry;
        [HideInInspector] public bool isLoading = false;
        public void LoadAdmobReward(bool reset = true)
        {
            if (DisabledAds || !UseAdmobReward)
            {
                return;
            }

            if (reset)
            {
                rewardRetry = 0;
            }

#if USE_ADMOB
            string adUnitId = "unexpected_platform";
            int totalId = 0;
#if UNITY_ANDROID
            if (rw_android.Count <= rewardRetry) return;
            adUnitId = rw_android[rewardRetry];
            totalId = rw_android.Count;
#elif UNITY_IOS
        if (rw_ios.Count <= rewardRetry) return;
        adUnitId =  rw_ios[rewardRetry];
        totalId = rw_ios.Count;
#endif
            if (cappingFail && failCount >= totalId)
            {
                if ((DateTime.Now - timeLoad).TotalSeconds > cappingTime)
                {
                    failCount = 0;
                }
                else
                {
                    return;
                }
            }

            isLoading = true;
            timeLoad = DateTime.Now;
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            if (LogDebug)
                Debug.Log(logPrefix + "Admob " + Reward_name + " load");
            FirebaseLogger.RewardCall(Reward_name);

            var adRequest = new AdRequest();

            RewardedAd.Load(adUnitId.Trim(), adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        if (LogDebug)
                            Debug.Log(
                                logPrefix + "Admob " + Reward_name + " load fail, message: " + error.GetMessage());
                        FirebaseLogger.RewardCallFail(Reward_name, error.GetCode());

                        failCount++;
                        if (rewardRetry < totalId - 1)
                        {
                            rewardRetry++;
                            LoadAdmobReward(false);
                        }
                        else
                        {
                            isLoading = false;
                            AdsController.Instance.LoadRewardFail(rewardName);
                        }

                        return;
                    }

                    rewardedAd = ad;
                    isLoading = false;
                    failCount = 0;

                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Reward_name + " load success");
                    AdsController.Instance.LoadRewardSuccess(rewardName);
                    FirebaseLogger.RewardReady(Reward_name);
                    RegisterEventHandlers(rewardedAd);
                });
#endif
        }


#if USE_ADMOB
        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    FirebaseLogger.RewardShowSuccess(Reward_name, currentPlacement, CurrentLevel, CurrentMode);
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Reward_name + " open: " + currentPlacement);

                    AdsController.Instance.MuteGameSound();
                });
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(logPrefix + "Admob " + Reward_name + " close: " + currentPlacement);

                    AdsController.Instance.OpenGameSound();
                    if (preload) LoadAdmobReward();
                });
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                AdsController.Instance.ExecuteAction(() =>
                {
                    if (LogDebug)
                        Debug.Log(
                            logPrefix + "Admob reward show fail, message: " + error.GetMessage());
                    FirebaseLogger.RewardShowFail(Reward_name, currentPlacement, CurrentLevel, CurrentMode, error.GetCode());

                    AdsController.Instance.OpenGameSound();

                    if (preload) LoadAdmobReward();
                });
            };

            ad.OnAdPaid += AdsController.Instance.OnAdPaid;
        }
#endif

        public void SetAdUnit(string adUnit, int _index = 0)
        {
#if UNITY_IOS
            if (rw_ios.Count > _index)
            {
                rw_ios[_index] = adUnit;
            }
            else
            {
                rw_ios.Add(adUnit);
            }
#else
            if (rw_android.Count > _index)
            {
                rw_android[_index] = adUnit;
            }
            else
            {
                rw_android.Add(adUnit);
            }
#endif
        }
    }


    public enum RewardName
    {
        None = 0,
        Reward_Default = 1,
    }
}