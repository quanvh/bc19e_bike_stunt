using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UICardItem : MonoBehaviour
    {
        public GameObject CardShow;
        public GameObject CardHide;
        public Text txtNumber;
        public Image Thumb;

        public GameObject AdsButton;

        private RewardModel currentReward;

        public Action OnCardShowComplete;

        private bool cardShow;
        private bool cardShowing;

        private UILuckyCard uiLuckyCard;


        private void Start()
        {
            DOVirtual.DelayedCall(UnityEngine.Random.Range(1.5f, 3f), () =>
            {
                CardShow.GetComponent<Animator>().enabled = true;
            });

        }
        public void SetCardItem(RewardModel _reward, UILuckyCard _uiLuckyCard)
        {
            uiLuckyCard = _uiLuckyCard;

            cardShow = false;
            cardShowing = false;
            AdsButton.SetActive(false);

            currentReward = _reward;
            CardShow.SetActive(true);
            CardHide.SetActive(false);

            txtNumber.gameObject.SetActive(false);

            Thumb.gameObject.SetActive(false);
            Thumb.sprite = _reward.Thumb;
            Thumb.SetNativeSize();
            if (_reward.Type == REWARD_TYPE.GOLD)
            {
                txtNumber.GetComponent<Text>().text = _reward.Value.ToString();
            }
            else if (_reward.Type == REWARD_TYPE.CARD)
            {
                txtNumber.GetComponent<Text>().text = "X" + _reward.Value.ToString();
            }
        }

        public void ShowCard(Action onComplete = null)
        {
            AdsButton.SetActive(false);
            OnCardShowComplete = onComplete;
            //if (AudioController.Instance) AudioController.Instance.OpenCard();
            GetComponent<Animator>().enabled = true;
            StartCoroutine(ShowCard(currentReward));
        }


        private readonly float timeShowCard = 1.5f;
        private IEnumerator ShowCard(RewardModel _reward)
        {
            AudioController.Instance.ClaimReward();
            switch (_reward.Type)
            {
                //case REWARD_TYPE.GOLD:
                //    totalGoldCollect = _reward.Value;
                //    break;
                case REWARD_TYPE.CARD:
                    var carModel = DataManager.Instance.GetCar(_reward.TargetID);
                    carModel.CurrentCard++;
                    break;
            }
            if (uiLuckyCard)
            {
                if (currentReward.Type == REWARD_TYPE.GOLD)
                {
                    uiLuckyCard.totalGold += currentReward.Value;
                }
                else if (currentReward.Type == REWARD_TYPE.CARD)
                {
                    uiLuckyCard.totalBlueprint += currentReward.Value;
                }
            }


            transform.DOScale(1.0f, 0.1f);
            yield return new WaitForSecondsRealtime(timeShowCard);
            cardShow = true;
            cardShowing = false;
            OnCardShowComplete?.Invoke();
        }


        public void OnClickCard()
        {
            if (!cardShowing && !uiLuckyCard.cardShowing)
            {
                if (GetComponent<Animator>().enabled == true) return;

                cardShowing = true;
                uiLuckyCard.cardShowing = true;

                if (uiLuckyCard.freeSpin > 0)
                {
                    uiLuckyCard.freeSpin--;
                    ShowCard(() => uiLuckyCard.OnShowCardComplete());
                }
                else
                {
                    AdsController.Instance.ShowReward(ADTYPE.LUCKY_BOX, () =>
                    {
                        ShowCard(() => uiLuckyCard.OnShowCardComplete());
                    });
                }
            }
        }

        public void ShowAdsButton()
        {
            AdsButton.SetActive(!cardShow);
        }

    }
}