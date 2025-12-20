using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UILuckyCard : IGacha
    {
        [SerializeField] private GameObject CardPrefab;

        [SerializeField] private Transform content;

        [SerializeField] private int numCard = 10;

        public GameObject btnClose;

        public GameObject btnClaim;

        public CustomData cardData;

        public CarData carData;

        private List<RewardModel> lstBlueprint;

        public int totalGold;
        public int totalBlueprint;
        public int freeSpin;
        private readonly int maxFreeSpin = 1;
        private int countBlueprint;
        private bool checkEnoughBlueprint;

        public override void InitGacha()
        {
            cardShowing = false;
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            lstBlueprint = new List<RewardModel>();

            freeSpin = maxFreeSpin;

            List<CustomModel> _lst = cardData.listItems.Where(t => t.TargetType == REWARD_TYPE.CAR).Take(6).ToList();

            if (btnClaim) btnClaim.SetActive(false);
            if (btnClose) btnClose.SetActive(false);

            //rule to set reward: first to sixth  spin - 100% blueprint
            // seventh spin toward - 50% blueprint - 50% gold
            foreach (var item in _lst)
            {
                var _reward = new RewardModel();
                _reward.Type = REWARD_TYPE.CARD;
                _reward.TargetID = item.TargetID;
                _reward.Thumb = item.Thumb;
                _reward.Icon = item.Thumb;
                _reward.Name = item.Name;
                _reward.Value = 1;
                _reward.TargetType = item.TargetType;

                lstBlueprint.Add(_reward);
            }
            for (int i = 0; i < _lst.Count; i++)
            {
                var carModel = DataManager.Instance.GetCar((_lst[i].TargetID));
                if (carModel.Unlock == true)
                {
                    var itemRemove = lstBlueprint.Single(r => r.TargetID == carModel.ID);
                    lstBlueprint.Remove(itemRemove);
                    countBlueprint++;
                    if (countBlueprint == _lst.Count) checkEnoughBlueprint = true;
                }
            }
            if (DataManager.Instance._player.gachaCount > 6)
            {
                lstBlueprint.AddRange(rewards.listItems);
            }
            if (!checkEnoughBlueprint)
            {
                lstReward = lstBlueprint.OrderBy(i => Guid.NewGuid()).Take(3).ToList();
            }
            else
            {
                lstReward = rewards.listItems.OrderBy(i => Guid.NewGuid()).Take(3).ToList();
            }
        }


        private IEnumerator SpawnCard()
        {
            for (int i = 0; i < numCard; i++)
            {
                GameObject _item = Instantiate(CardPrefab, content);
                if (lstReward.Count > i)
                {
                    _item.GetComponent<UICardItem>().SetCardItem(lstReward[i], this);
                    _item.transform.localScale = Vector3.zero;
                }


                _item.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        public override void StartSpin()
        {
            StartCoroutine(SpawnCard());
        }

        [HideInInspector] public bool cardShowing = false;

        public void OnShowCardComplete()
        {
            cardShowing = false;
            foreach (Transform item in content)
            {
                item.GetComponent<UICardItem>().ShowAdsButton();
            }
            DOVirtual.DelayedCall(1f, () =>
            {
                if (btnClaim) btnClaim.SetActive(true);
                if (btnClose) btnClose.SetActive(true);
            });
        }

        public override int FixedReward()
        {
            return 0;
        }
        public void OnClose()
        {
            if (totalGold > 0)
            {
                RouteController.Instance.goldWinLevel += totalGold;
                DataManager.Instance._player.currentGold += totalGold;
            }
            UIInit.Instance.OnClaimRewardLuckyBox(totalGold, totalBlueprint);

            totalGold = 0;
            totalBlueprint = 0;
        }

    }
}