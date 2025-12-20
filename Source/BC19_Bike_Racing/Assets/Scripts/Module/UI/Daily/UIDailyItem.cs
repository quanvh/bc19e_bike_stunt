using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UIDailyItem : MonoBehaviour
    {
        public Image imgThumb;
        public GameObject Active;
        public GameObject Icon;

        public Image imgTick;

        public Text txtDay;
        public Text txtNumber;


        private RewardModel currentReward;

        public void SetData(RewardModel _daily)
        {
            currentReward = _daily;
            switch (currentReward.Type)
            {
                case REWARD_TYPE.GOLD:
                    SetRewardGold(_daily);
                    break;
                case REWARD_TYPE.CAR:
                    SetRewardCar(_daily);
                    break;
            }
        }

        private void SetRewardGold(RewardModel _daily)
        {
            SetDataReward(_daily);
            Icon.SetActive(true);
            if (txtNumber) txtNumber.text = _daily.Value.ToString();
        }

        private void SetRewardCar(RewardModel _daily)
        {
            SetDataReward(_daily);
            Icon.SetActive(false);
            if (txtNumber) txtNumber.text = _daily.Name;
            if (imgThumb)
            {
                imgThumb.transform.localScale = 0.35f * Vector3.one;
            }
        }

        private void SetDataReward(RewardModel _daily)
        {

            PlayerModel _player = DataManager.Instance._player;
            bool current = _player.dayLogin == _daily.ID && !_player.dailyReward;

            imgTick.gameObject.SetActive(_player.dayLogin > _daily.ID || (_player.dayLogin == _daily.ID && _player.dailyReward));
            Active.SetActive(current);

            if (imgThumb)
            {
                imgThumb.sprite = _daily.Thumb;
                imgThumb.SetNativeSize();
            }

            if (txtDay) txtDay.text = "Day " + _daily.ID;

        }

        readonly float timeAnim = 0.75f;
        public void OnClaim()
        {
            PlayerModel _player = DataManager.Instance._player;
            if (currentReward.ID == _player.dayLogin)
            {
                imgTick.color = new Color(1f, 1f, 1f, 0f);
                imgTick.fillAmount = 0f;
                imgTick.gameObject.SetActive(true);

                imgTick.DOColor(new Color(1f, 1f, 1f, 1f), timeAnim);
                imgTick.DOFillAmount(1f, timeAnim);
            }
        }


    }
}