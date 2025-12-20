using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UIGachaBase : PopupBase
    {
        [Header("POPUP PARAM"), Space]
        public Button TurnButton;

        public Button CloseButton;

        public GameObject iconVideo;

        public ScoreText scoreText;

        public ParticleSystem animReward;

        public IGacha luckyGacha;

        public CustomData cardData;

        [Header("TIME TICK")]
        [SerializeField] private bool ShowTick = false;
        [SerializeField, ShowIf("ShowTick")] private bool logDebug = false;
        [SerializeField, ShowIf("ShowTick")] private float TimeTick = 30f;
        [SerializeField, ShowIf("ShowTick")] private Text txtTick;


        [Header("Reward Panel")]
        public GameObject RewardPanel;
        public Transform Reward;

        public Image imgThumb, imgIcon;
        public Text txtValue;


        private int totalGoldCollect;


        private bool spinAvaiable;
        protected override void ShowStart()
        {
            base.ShowStart();
            SetAdsImage();

            if (Reward != null)
            {
                RewardPanel.SetActive(false);
                Reward.localScale = Vector3.zero;
                Reward.localEulerAngles = Vector3.down * 180f;
            }

            spinAvaiable = true;
        }

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            if (AdsController.Instance)
            {
                AdsController.Instance.ShowMrec(MrecName.Mrec_Default);
                AdsController.Instance.OnRewardComplete += OnRewardAds;

                if (AdsController.Instance.gameLoaded
                && DataManager.Instance.Remote.DecreaseNativeBanner)
                {
                    RouteController.Instance.ShowNativeBanner = false;
                }
            }

            luckyGacha.OnSpinStart += OnSpinStart;
            luckyGacha.OnSpinComplete += OnSpinComplete;
        }

        protected override void HideStart()
        {
            base.HideStart();
            if (AdsController.Instance)
            {
                AdsController.Instance.HideMrec(MrecName.Mrec_Default);
                AdsController.Instance.OnRewardComplete -= OnRewardAds;

                if (AdsController.Instance.gameLoaded
                && DataManager.Instance.Remote.DecreaseNativeBanner)
                {
                    RouteController.Instance.ShowNativeBanner = true;
                }
            }

            luckyGacha.OnSpinStart -= OnSpinStart;
            luckyGacha.OnSpinComplete -= OnSpinComplete;
        }

        private TimeSpan timeRemain;
        private void Update()
        {
            if (isShow && ShowTick)
            {
                timeRemain = timeSpin.AddSeconds(TimeTick) - DateTime.UtcNow;
                if (timeRemain <= TimeSpan.Zero)
                {
                    spinAvaiable = true;
                    timeRemain = TimeSpan.Zero;
                }
                else
                {
                    if (logDebug)
                    {
                        Debug.Log("[UIGache] Time spin remain: " + timeRemain.Hours + " hours, " +
                            timeRemain.Minutes + " minutes, " + timeRemain.Seconds + " seconds.");
                    }
                }
                if (txtTick)
                {
                    txtTick.text = timeRemain.ToString();
                }
            }
        }

        public void OnShowStart()
        {
            if (!luckyGacha.InitAtStart) luckyGacha.InitGacha();

        }

        private void SetAdsImage()
        {
            if (_player != null && iconVideo != null)
                iconVideo.SetActive(_player.dailyGacha <= 0);
        }

        private void OnSpinStart()
        {
            if (TurnButton)
                TurnButton.interactable = false;
            if (CloseButton)
                CloseButton.interactable = false;
        }

        private void OnSpinComplete(RewardModel _reward)
        {
            if (TurnButton)
                TurnButton.interactable = true;
            if (CloseButton)
                CloseButton.interactable = true;
            StartCoroutine(GiveAward(_reward));
        }


        private DateTime timeSpin;
        public void OnSpin()
        {
            if (!spinAvaiable) return;
            if (ShowTick) spinAvaiable = false;

            timeSpin = DateTime.UtcNow;
            if (_player.dailyGacha > 0)
            {
                _player.dailyGacha--;
                _player.spinCount++;
                luckyGacha.StartSpin();
            }
            else
            {
                AdsController.Instance.ShowReward(ADTYPE.SPIN);
            }

        }

        private void OnRewardAds(ADTYPE adType)
        {
            if (adType == ADTYPE.SPIN)
            {
                OnStartSpinAds();
            }
            else if (adType == ADTYPE.SPIN_CAR || adType == ADTYPE.SPIN_CHAR || adType == ADTYPE.SPIN_HELMET)
            {

                UIToast.Instance.Toast("Thanks for watching video. Got lucky spin.");
                Invoke(nameof(OnSpinCustom), 1.5f);
            }
        }

        public void OnSpinCustom()
        {
            luckyGacha.StartSpin();
        }
        public void OnStartSpinAds()
        {
            _player.spinCount++;
            UIToast.Instance.Toast("Thanks for watching video. Got lucky spin.");
            Invoke(nameof(OnSpinCustom), 1.5f);
        }

        private bool runningAward = false;
        private IEnumerator GiveAward(RewardModel _reward)
        {
            if (runningAward) yield return null;
            else
            {
                runningAward = true;
                if (AudioController.Instance)
                {
                    AudioController.Instance.StopSound();
                    AudioController.Instance.ClaimReward();

                }
                SetAdsImage();

                switch (_reward.Type)
                {
                    case REWARD_TYPE.GOLD:
                        totalGoldCollect += _reward.Value;
                        break;
                    case REWARD_TYPE.CARD:
                        if (_reward.TargetType == REWARD_TYPE.CAR)
                        {
                            var _car = DataManager.Instance.GetCar(_reward.TargetID);
                            if (_car != null && _car.UnlockByCard)
                            {
                                _car.CurrentCard += _reward.Value;
                                //if (_car.CurrentCard >= _car.PriceCard)
                                if (_car.Unlock == true)
                                {
                                    var _cardItem = cardData.listItems
                                        .Where(t => t.TargetType == REWARD_TYPE.CAR && t.TargetID == _reward.TargetID)
                                        .FirstOrDefault();
                                    if (_cardItem != null)
                                    {
                                        _cardItem.Unlock = true;
                                    }
                                }
                            }
                        }
                        if (_reward.TargetType == REWARD_TYPE.HELMET)
                        {
                            var _item = DataManager.Instance.GetCustom(CUSTOM_TYPE.HELMET, _reward.TargetID);
                            if (_item != null && _item.UnlockByCard)
                            {
                                _item.CurrentCard += _reward.Value;
                                //if (_item.CurrentCard >= _item.PriceCard)
                                if (_item.Unlock == true)
                                {
                                    var _cardItem = cardData.listItems
                                        .Where(t => t.TargetType == REWARD_TYPE.HELMET && t.TargetID == _reward.TargetID)
                                        .FirstOrDefault();
                                    if (_cardItem != null)
                                    {
                                        _cardItem.Unlock = true;
                                    }
                                }
                            }
                        }
                        if (_reward.TargetType == REWARD_TYPE.CHARACTER)
                        {
                            var _item = DataManager.Instance.GetCustom(CUSTOM_TYPE.CHARACTER, _reward.TargetID);
                            if (_item != null && _item.UnlockByCard)
                            {
                                _item.CurrentCard += _reward.Value;
                                //if (_item.CurrentCard >= _item.PriceCard)
                                if (_item.Unlock == true)
                                {
                                    var _cardItem = cardData.listItems
                                        .Where(t => t.TargetType == REWARD_TYPE.CHARACTER && t.TargetID == _reward.TargetID)
                                        .FirstOrDefault();
                                    if (_cardItem != null)
                                    {
                                        _cardItem.Unlock = true;
                                    }
                                }
                            }
                        }
                        break;
                }
                yield return new WaitForSeconds(0.25f);

                if (Reward && RewardPanel)
                {
                    if (imgThumb)
                    {
                        if (_reward.TargetType == REWARD_TYPE.CHARACTER)
                        {
                            imgThumb.transform.localScale = new Vector3(2.3f, 2.3f, 2.3f);
                        }
                        else if (_reward.Type == REWARD_TYPE.GOLD)
                        {
                            imgThumb.transform.localScale = new Vector3(2f, 2f, 2f);
                        }
                        else
                        {
                            imgThumb.transform.localScale = Vector3.one;

                        }
                        imgThumb.sprite = _reward.Thumb;
                        imgThumb.SetNativeSize();
                        //imgThumb.transform.localScale = Vector3.one;

                    }
                    if (imgIcon)
                    {
                        if (_reward.Type == REWARD_TYPE.CARD)
                        {
                            imgIcon.enabled = false;
                        }
                        else
                        {
                            imgIcon.enabled = true;
                            imgIcon.sprite = _reward.Icon;
                            imgIcon.SetNativeSize();
                        }
                    }
                    if (txtValue)
                    {
                        if (_reward.Type == REWARD_TYPE.CARD)
                        {
                            txtValue.text = "X" + _reward.Value.ToString();
                        }
                        else
                        {
                            txtValue.text = _reward.Value.ToString();
                        }
                    }

                    RewardPanel.SetActive(true);
                    Reward.DOScale(1f, .8f).SetEase(Ease.OutBack);
                    Reward.DOLocalRotate(Vector3.zero, .25f).SetDelay(0.25f).OnComplete(() =>
                    {
                        if (animReward) animReward.Play();
                    });

                    yield return new WaitForSeconds(2.5f);
                    Reward.DOScale(0f, 0.25f).OnComplete(() =>
                    {
                        RewardPanel.SetActive(false);
                        Reward.localScale = Vector3.zero;
                        Reward.localEulerAngles = Vector3.down * 180f;
                        StartCoroutine(scoreText.StartAnim(totalGoldCollect));
                        runningAward = false;
                        luckyGacha.InitGacha();
                    });
                }
            }
        }

        public void OnClose()
        {
            if (totalGoldCollect > 0)
            {
                RouteController.Instance.goldWinLevel = totalGoldCollect;
                _player.currentGold += totalGoldCollect;
            }
            totalGoldCollect = 0;
            scoreText.txtValue.text = 0.ToString();
            DataManager.Instance.Save();

            Hide(() =>
            {
                UIManager.Instance.ShowPopup<UIGarage>();
            });

        }
    }
}