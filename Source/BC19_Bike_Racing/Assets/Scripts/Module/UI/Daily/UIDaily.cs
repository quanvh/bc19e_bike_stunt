using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UIDaily : PopupBase
    {
        [Header("PARAM"), Space]
        public RewardData dailyData;

        public GameObject DailyItem;

        public Transform DailyContent;

        public Button btnClaim;

        public Text txtGold;

        [Header("TIME TICK")]
        [SerializeField] private bool ShowTick = false;
        [SerializeField, ShowIf("ShowTick")] private bool logDebug = false;
        [SerializeField, ShowIf("ShowTick")] private Text txtTick;

        protected override void ShowStart()
        {
            base.ShowStart();
            InitDaily();
        }

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            if (AdsController.Instance && AdsController.Instance.gameLoaded
            && DataManager.Instance.Remote.DecreaseNativeBanner)
            {
                RouteController.Instance.ShowNativeBanner = false;
            }
        }

        protected override void HideStart()
        {
            base.HideStart();
            if (AdsController.Instance && AdsController.Instance.gameLoaded
            && DataManager.Instance.Remote.DecreaseNativeBanner)
            {
                RouteController.Instance.ShowNativeBanner = true;
            }
        }

        private TimeSpan timeRemain;
        private void Update()
        {
            if (ShowTick && isShow)
            {
                timeRemain = DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow;
                if (logDebug)
                {
                    Debug.Log("[UIDaily] Time span remain: " + timeRemain.Hours + " hours, " +
                        timeRemain.Minutes + " minutes, " + timeRemain.Seconds + " seconds.");
                }
                if (txtTick)
                {
                    txtTick.text = timeRemain.ToString();
                }
            }
        }


        public void InitDaily()
        {
            txtGold.text = _player.currentGold.ToString("N0",
                        CultureInfo.CreateSpecificCulture("en-US"));
            for (int i = 0; i < dailyData.listItems.Count; i++)
            {
                GameObject _item;
                if (DailyContent.childCount > i)
                {
                    _item = DailyContent.GetChild(i).gameObject;
                }
                else
                {
                    _item = Instantiate(DailyItem, DailyContent);
                }
                _item.GetComponent<UIDailyItem>().SetData(dailyData.listItems[i]);
                if (dailyData.listItems[i].ID == _player.dayLogin)
                {
                    _item.GetComponent<Button>().onClick.AddListener(() => OnClaim());
                }
            }
            if (btnClaim)
            {
                btnClaim.interactable = DataManager.Instance.ShowDaily;
            }
        }


        public void OnClaim()
        {
            if (!_player.dailyReward)
            {
                StartCoroutine(ClaimDaily());
            }
        }

        private IEnumerator ClaimDaily(int multiple = 1)
        {
            _player.dailyReward = true;
            _player.dailyRewardShow = true;
            _player.lastClaim = DateTime.UtcNow;

            RewardModel _reward = dailyData.listItems.Where(t => t.ID == _player.dayLogin).FirstOrDefault();

            string _message = "";
            if (_reward != null)
            {
                if (_reward.Type == REWARD_TYPE.GOLD)
                {
                    _player.currentGold += multiple * _reward.Value;
                    RouteController.Instance.goldWinLevel += multiple * _reward.Value;
                    _message = "You got " + (multiple * _reward.Value) + " golds";
                }
                else if (_reward.Type == REWARD_TYPE.CAR)
                {
                    DataManager.Instance.UnlockCar(_reward.Value);
                    _player.currentCar = _reward.Value;
                    _message = "You got a new bikes";
                }
                if (btnClaim) btnClaim.interactable = false;
            }
            txtGold.text = _player.currentGold.ToString("N0",
                        CultureInfo.CreateSpecificCulture("en-US"));
            DataManager.Instance.Save();

            foreach (Transform item in DailyContent)
            {
                item.gameObject.GetComponent<UIDailyItem>().OnClaim();
            }

            yield return new WaitForSeconds(1.0f);

            if (!string.IsNullOrEmpty(_message))
            {
                UIToast.Instance.Toast(_message);
            }

            Hide();
        }


        public void OnClose()
        {
            Hide();
        }
    }
}