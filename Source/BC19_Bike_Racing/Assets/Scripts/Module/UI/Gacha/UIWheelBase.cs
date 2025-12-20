using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class UIWheelBase : IGacha
    {
        public GameObject Circle;

        public Transform Arrow;

        public GameObject GachaItem;

        public Transform GachaContent;

        public CustomData cardData;

        public Sprite iconHelmet;
        public Sprite iconChar;
        public Sprite iconBike;

        private bool _isStarted;
        private List<float> _sectorsAngles = new List<float> { };

        private float _currentLerpRotationTime;

        private float _finalAngle;
        private float _startAngle = 0;

        [SerializeField] private float angleOffset = 0;

        private int gachaAward;

        public int fullCircles = 11;


        private List<RewardModel> shuffledList = new List<RewardModel>();


        private void InitNormalReward()
        {
            foreach (var item in rewards.listItems)
            {
                if (item.Type == REWARD_TYPE.CARD)
                {
                    if (item.TargetType == REWARD_TYPE.CAR)
                    {
                        var _car = DataManager.Instance.GetCar(item.TargetID);
                        if (_car != null && !_car.Unlock && _car.UnlockByCard && _car.CurrentCard < _car.PriceCard)
                        {
                            lstReward.Add(item);
                        }
                    }
                    else if (item.TargetType == REWARD_TYPE.HELMET)
                    {
                        var _helmet = DataManager.Instance.GetCustom(CUSTOM_TYPE.HELMET, item.TargetID);
                        if (_helmet != null && !_helmet.Unlock && _helmet.UnlockByCard && _helmet.CurrentCard < _helmet.PriceCard)
                        {
                            lstReward.Add(item);
                        }
                    }
                    else if (item.TargetType == REWARD_TYPE.CHARACTER)
                    {
                        var _char = DataManager.Instance.GetCustom(CUSTOM_TYPE.CHARACTER, item.TargetID);
                        if (_char != null && !_char.Unlock && _char.UnlockByCard && _char.CurrentCard < _char.PriceCard)
                        {
                            lstReward.Add(item);
                        }
                    }
                }
            }
            lstReward.AddRange(rewards.listItems.Take(fullCircles - lstReward.Count).OrderBy(i => Guid.NewGuid()).Take(fullCircles - lstReward.Count));
        }


        public override void InitGacha()
        {
            lstReward.Clear();
            shuffledList.Clear();

            InitNormalReward();


            //==============================
            _sectorsAngles.Clear();
            foreach (Transform item in GachaContent)
            {
                Destroy(item.gameObject);
            }
            for (int i = 0; i < lstReward.Count; i++)
            {
                float _angle = 360f * i / lstReward.Count + angleOffset;
                GameObject _item = Instantiate(GachaItem, GachaContent);
                _item.transform.localRotation = Quaternion.Euler(0f, 0f, _angle);
                if (lstReward[i].Type == REWARD_TYPE.GOLD)
                {
                    _item.transform.GetChild(1).GetComponent<Text>().text = lstReward[i].Value.ToString();
                    _item.transform.GetChild(1).GetComponent<Text>().color = lstReward[i].Color;
                    _item.transform.GetChild(1).gameObject.SetActive(true);
                    _item.transform.GetChild(0).gameObject.SetActive(false);
                }
                else if (lstReward[i].Type == REWARD_TYPE.CARD)
                {
                    if (DataManager.Instance.isLuckySpinCustom)
                    {
                        _item.transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
                    }
                    _item.transform.GetChild(0).GetComponent<Image>().sprite = lstReward[i].Icon;
                    _item.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
                    _item.transform.GetChild(0).transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0f);
                    _item.transform.GetChild(0).gameObject.SetActive(true);
                    _item.transform.GetChild(1).gameObject.SetActive(false);
                }
                _sectorsAngles.Add(_angle);
            }
        }

        public override void StartSpin()
        {
            StartCoroutine(SpinCoroutine());
        }

        public override int FixedReward()
        {
            if (RandomReward)
            {
                return UnityEngine.Random.Range(0, _sectorsAngles.Count);
            }
            else
            {
                return lstReward.IndexOf(lstReward.Where(t => t.Type == REWARD_TYPE.GOLD)
                    .OrderBy(i => Guid.NewGuid()).FirstOrDefault());
            }

        }

        public IEnumerator SpinCoroutine()
        {
            currentSector = 0;
            _currentLerpRotationTime = 0f;

            gachaAward = FixedReward();

            float randomFinalAngle = _sectorsAngles.Count > gachaAward ? _sectorsAngles[gachaAward] : 0;

            _finalAngle = -(fullCircles * 360 + randomFinalAngle);
            _isStarted = true;
            OnSpinStart?.Invoke();

            yield return new WaitForSeconds(0.25f);
            if (AudioController.Instance)
                AudioController.Instance.Spin();
        }

        readonly float maxLerpRotationTime = 4f;
        float currentSector;
        void Update()
        {
            if (!_isStarted)
                return;

            _currentLerpRotationTime += Time.deltaTime;
            if (_currentLerpRotationTime > maxLerpRotationTime || Circle.transform.eulerAngles.z == _finalAngle)
            {
                _currentLerpRotationTime = maxLerpRotationTime;
                _isStarted = false;
                _startAngle = _finalAngle % 360;

                if (lstReward.Count > gachaAward)
                {
                    OnSpinComplete?.Invoke(lstReward[gachaAward]);
                }
            }

            // Calculate current position using linear interpolation
            float t = _currentLerpRotationTime / maxLerpRotationTime;

            // This formulae allows to speed up at start and speed down at the end of rotation.
            // Try to change this values to customize the speed
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            float angle = Mathf.Lerp(_startAngle, _finalAngle, t);
            Circle.transform.eulerAngles = new Vector3(0, 0, angle);

            float _sector = (int)(angle / _sectorsAngles[1]);
            if (_sector != currentSector)
            {
                currentSector = _sector;
                if (Arrow) RotateArrow();
            }
        }

        private void RotateArrow()
        {
            Arrow.DOLocalRotate(new Vector3(0, 0, 30f), .25f).OnComplete(() =>
                Arrow.DOLocalRotate(new Vector3(0, 0, 0), .05f));
        }
    }
}