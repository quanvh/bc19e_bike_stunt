namespace Bacon
{
    public class UIRemoveAds : UISplash
    {
        protected override void ShowCompleted()
        {
            rewardWait = false;
            base.ShowCompleted();
            StartCoroutine(DelayDone());

            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardLoaded += OnRewardLoaded;
                AdsController.Instance.OnRewardComplete += OnRewardComplete;
            }
        }

        protected override void HideStart()
        {
            base.HideStart();
            if (AdsController.Instance)
            {
                AdsController.Instance.OnRewardLoaded -= OnRewardLoaded;
                AdsController.Instance.OnRewardComplete -= OnRewardComplete;
            }
        }


        public void OnClose()
        {
            LoadGame();
        }


        private bool rewardWait = false;
        public void OnClickAds()
        {
            waitClose = false;
            if (AdsController.Instance.IsRewardReady())
            {
                AdsController.Instance.ShowReward(ADTYPE.REMOVE_ADS);
            }
            else if (!rewardWait)
            {
                rewardWait = true;
                AdsController.Instance.LoadAdmobReward(RewardName.Reward_Default);
            }
        }

        private void OnRewardLoaded()
        {
            if (rewardWait)
            {
                AdsController.Instance.ShowReward(ADTYPE.REMOVE_ADS);
            }
        }

        private void OnRewardComplete(ADTYPE adType)
        {
            if (adType == ADTYPE.REMOVE_ADS)
            {
                if (DataManager.Instance)
                {
                    DataManager.Instance.sessionVip = true;
                }
                UIToast.Instance.Toast("Thanks for watch video, Remove ads in this session!");
                Invoke(nameof(LoadGame), 1.5f);
            }
        }
    }
}