using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class UISplash : PopupBase
    {
        [Header("NATIVE AD")]
        [SerializeField] AdmobNativeUI nativeAds;

        public bool ForceAutoClose = false;

        public GameObject btnDone;

        public Text txtAutoClose;

        protected float currentTime;

        protected bool isSelected = false;

        public float TimeAutoClose
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeAutoCloseAge;
                else return 15f;
            }
        }

        public float TimeDelayDone
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeShowCloseAge;
                else return 5f;
            }
        }

        public bool UseMrecLanguage
        {
            get
            {
                if (RouteController.Instance)
                    return RouteController.Instance.UseMrecLanguage;
                else return true;
            }
        }



        protected override void ShowStart()
        {
            base.ShowStart();
            waitClose = true;
            currentTime = 0f;
            isSelected = false;
            btnDone.SetActive(false);
        }

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            if (AdsController.Instance)
            {
                if (UseMrecLanguage)
                {
                    AdsController.Instance.ShowMrec(MrecName.Mrec_Open, false);
                }
                else
                {
                    if (nativeAds)
                    {
                        AdsController.Instance.SetNativeAds(NativeName.Native_Language, nativeAds);
                    }
                }
            }
        }

        protected override void HideStart()
        {
            base.HideStart();
            if (AdsController.Instance && UseMrecLanguage)
            {
                AdsController.Instance.HideMrec(MrecName.Mrec_Open);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
        }


        protected bool waitClose;
        private void Update()
        {
            if (currentTime > TimeAutoClose)
            {
                if (!waitClose) return;
                waitClose = false;
                FirebaseLogger.RegularEvent("On_wait_over_time");
                LoadGame();
            }
            else
            {
                txtAutoClose.text = "Auto close and show later in " + Mathf.FloorToInt(TimeAutoClose - currentTime) + "s";
                if (!isSelected || ForceAutoClose)
                    currentTime += Time.deltaTime;
            }
        }

        protected IEnumerator DelayDone()
        {
            yield return new WaitForSeconds(TimeDelayDone);
            btnDone.SetActive(true);
        }

        protected void LoadGame()
        {
            RouteController.Instance.InitGame();
        }
    }
}