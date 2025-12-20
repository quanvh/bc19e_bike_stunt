using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bacon
{
    public class ButtonReward : MonoBehaviour
    {
        private Button _button;

        [SerializeField] private GameObject Loading, Content;

        [SerializeField, Space] private ADTYPE adType;

        [SerializeField, Space] UnityEvent OnRewardSuccess;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            rewardLoading = false;
            if (Loading) Loading.SetActive(false);
            if (Content) Content.SetActive(true);
            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardComplete += OnRewardAds;
                if (!AdsController.Instance.PreloadRewardDefault)
                {
                    AdsController.Instance.OnRewardLoaded += OnRewardLoaded;
                }
                if (_button != null)
                {
                    _button.onClick.AddListener(ShowReward);
                }
            }
        }

        private void OnDisable()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardComplete -= OnRewardAds;
                if (!AdsController.Instance.PreloadRewardDefault)
                {
                    AdsController.Instance.OnRewardLoaded -= OnRewardLoaded;
                }
                if (_button != null)
                {
                    _button.onClick.RemoveListener(ShowReward);
                }
            }
        }

        private void OnRewardAds(ADTYPE _adType)
        {
            if (_adType == adType) OnRewardSuccess?.Invoke();
        }

        private void OnRewardLoaded()
        {
            if (!AdsController.Instance.PreloadRewardDefault && rewardLoading)
            {
                if (_button) _button.interactable = true;
                AdsController.Instance.ShowReward(adType);
            }
        }


        private bool rewardLoading;
        private void ShowReward()
        {
            if (rewardLoading) return;
            FirebaseLogger.ClickButton(_button.name);

            if (AdsController.Instance.IsRewardReady())
            {
                AdsController.Instance.ShowReward(adType);
            }
            else if (!AdsController.Instance.PreloadRewardDefault)
            {
                rewardLoading = true;
                if (_button) _button.interactable = false;
                AdsController.Instance.LoadAdmobReward(RewardName.Reward_Default);
            }
        }


    }
}