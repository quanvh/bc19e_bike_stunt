using UnityEngine;

namespace Bacon
{
    public class UIInit : MonoBehaviour
    {
        public static UIInit Instance;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardNotReady += OnRewardNotReady;
                AdsController.Instance.OnAppOpenLoad += LoadingAds;
            }
            if (DataManager.Instance)
            {
                DataManager.Instance.OnUnlockMode += OnUnlockMode;
                DataManager.Instance.DiskFullAction += OnDiskFull;
            }
            UIManager.Instance.ShowPopup<UILoading>();
        }

        private void OnDestroy()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardNotReady -= OnRewardNotReady;
                AdsController.Instance.OnAppOpenLoad -= LoadingAds;
            }
            if (DataManager.Instance)
            {
                DataManager.Instance.OnUnlockMode -= OnUnlockMode;
                DataManager.Instance.DiskFullAction -= OnDiskFull;
            }
        }

        private void OnRewardNotReady()
        {
            UIToast.Instance.Toast("Video not available. Please try again later");
        }

        public void OnClaimRewardLuckyBox(int _gold, int _blueprint)
        {
            if (_gold > 0 && _blueprint <= 0)
            {
                UIToast.Instance.Toast("You've just received " + _gold + " gold");
            }
            else if (_gold <= 0 && _blueprint > 0)
            {
                UIToast.Instance.Toast("You've just received " + _blueprint + " blueprint");
            }
            else if (_gold > 0 && _blueprint > 0)
            {
                UIToast.Instance.Toast("You've just received " + _gold + " gold and " + _blueprint + " blueprint");
            }

        }

        public void LoadingAds(bool active)
        {
            if (active) UIManager.Instance.ShowPopup<UILoading>(false);
            else UIManager.Instance.HidePopup<UILoading>();
        }

        public void OnUnlockMode(int modeID)
        {
            var modeUnlocked = DataManager.Instance.GetMode(modeID);
            UIToast.Instance.Toast("You've just unlock new mode: " + modeUnlocked.Name);
        }

        public void OnDiskFull()
        {
            UIToast.Instance.Toast("Disk full. Clean your device storage or game data will be lost");
        }
    }
}