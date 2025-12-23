using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UILoading : PopupBase
    {
        [Header("POPUP PARAM"), Space]
        public bool timerLoad = true;

        public Image imgFill;

        public List<string> lstTip;
        public Text txtTip;
        private int currentTip = 0;

        public GameObject imgLoadingAds;

        private float currentTime = 0f;
        private readonly float timeChangeText = 2.5f;
        private float currentTimeLoad = 0f;
        private float TimeLoadOpenAds
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeLoadOpenAds;
                else return 4f;
            }
        }

        [Header("STEP LOADING"), Space]
        public float targetLoad = 0f;

        public float maxTimeLoad = 30f;

        private readonly float speedLoad = 50f;

        [SerializeField] private Transform imgBg;
        private Vector3 bgPos;

        protected override void Awake()
        {
            base.Awake();
            if (imgBg != null)
            {
                bgPos = imgBg.localPosition;
            }
        }

        private void Start()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                targetLoad = 1f;
            }
#if UNITY_EDITOR
            targetLoad = 1f;
#endif

            if (AdsController.Instance)
            {
                AdsController.Instance.OnFirstAdsLoaded += OnFirstAdsLoaded;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (imgFill)
            {
                imgFill.fillAmount = 0f;
            }
            if (AdsController.Instance)
            {
                if (AdsController.Instance.loadOpenAd)
                {
                    Invoke(nameof(CancelLoadAd), TimeLoadOpenAds);
                }
                if (imgLoadingAds)
                    imgLoadingAds.SetActive(AdsController.Instance.loadOpenAd);
            }
        }

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            if (AdsController.Instance)
            {
                AdsController.Instance.HideMrec();
            }
        }

        private void OnDestroy()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnFirstAdsLoaded -= OnFirstAdsLoaded;
            }
        }

        private void CancelLoadAd()
        {
            AdsController.Instance.loadOpenAd = false;
            UIInit.Instance.LoadingAds(false);
            Hide();
        }

        private void OnFirstAdsLoaded()
        {
            targetLoad = 1f;
            FirebaseLogger.RegularEvent("First_ads_ready");
        }

        private readonly float moveSpeed = 50f;
        void Update()
        {
            if (timerLoad)
            {
                if (imgFill.fillAmount == 1 && RouteController.Instance.InitDone)
                {
                    timerLoad = false;
                    imgFill.transform.parent.gameObject.SetActive(false);

                    FirebaseLogger.RegularEvent("Finish_loading");
                    AdsController.Instance.ShowInterSplash(() => LoadGame());
                }
                else
                {
                    imgFill.fillAmount = currentTimeLoad / maxTimeLoad;
                }
            }
            else if (AdsController.Instance.loadOpenAd && AdsController.Instance.OpenAdsReady())
            {
                AdsController.Instance.loadOpenAd = false;
                AdsController.Instance.ShowAdmobOpenAds();
                Hide();
            }

            if (currentTimeLoad < maxTimeLoad)
            {
                currentTimeLoad += Time.deltaTime * (targetLoad >= 1f ? speedLoad : 1f);
            }


            currentTime += Time.deltaTime;
            if (currentTime >= timeChangeText)
            {
                currentTip = (currentTip + 1) % lstTip.Count;
                currentTime = 0f;
                txtTip.text = lstTip[currentTip];
            }

            if (imgBg != null)
            {
                imgBg.localPosition = bgPos + new Vector3(Mathf.Clamp(Input.acceleration.x, -moveSpeed, moveSpeed),
                    Mathf.Clamp(Input.acceleration.y, -moveSpeed, moveSpeed), 0f);
            }
        }

        private void LoadGame()
        {
            RouteController.Instance.GetUserInfo();
        }
    }
}