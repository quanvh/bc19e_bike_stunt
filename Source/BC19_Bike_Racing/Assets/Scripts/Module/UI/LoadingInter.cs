using System;
using System.Collections;
using UnityEngine;

namespace Bacon
{
    public class LoadingInter : MonoBehaviour
    {
        public static LoadingInter Instance;

        private readonly float defaultTimeWait = 6f;

        private string CurrentPlacement;
        private Action OnInterSuccess;
        private float TimeLoadInter
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.TimeLoadInter;
                else return defaultTimeWait;
            }
        }

        private bool UseAdmobInter
        {
            get
            {
                if (DataManager.Instance)
                    return DataManager.Instance.Remote.UseAdmobInter;
                else return false;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnInterLoaded += ShowInterDelay;
                AdsController.Instance.OnInterFailed += ShowInterDelay;
            }
        }

        private void OnDestroy()
        {
            if (AdsController.Instance)
            {
                AdsController.Instance.OnInterLoaded -= ShowInterDelay;
                AdsController.Instance.OnInterFailed -= ShowInterDelay;
            }
        }

        public bool RemoveAds => DataManager.Instance.VipUser || !DataManager.Instance.Remote.ShowInter
            || !DataManager.Instance.Remote.UseAdmobInter;
        public void ShowInter(string placement, Action OnSuccess, bool startLevel = false)
        {
            if (RemoveAds)
            {
                OnSuccess?.Invoke();
                return;
            }

            if (AdsController.Instance && AdsController.Instance.PreloadInterDefault)
            {
                if (startLevel && !DataManager.Instance.IsInterStart)
                {
                    OnSuccess?.Invoke();
                }
                else
                {
                    AdsController.Instance.ShowInter(placement, OnSuccess);
                }
            }
            else
            {
                showInter = false;
                CurrentPlacement = placement;
                if (!startLevel || (startLevel && DataManager.Instance.IsInterStart))
                {
                    OnInterSuccess = OnSuccess;
                    if (AdsController.Instance.IsInterReady())
                    {
                        ShowInterDelay();
                    }
                    else if (UseAdmobInter)
                    {
#if USE_ADMOB
                        UIManager.Instance.ShowPopup<UILoadingAds>(false);

                        AdsController.Instance.LoadAdmobInter(InterName.Inter_Default);

                        Invoke(nameof(ShowInterDelay), TimeLoadInter);
#else
                        OnSuccess?.Invoke();
#endif
                    }
                    else OnSuccess?.Invoke();
                }
                else OnSuccess?.Invoke();
            }
        }


        private bool showInter = false;
        private void ShowInterDelay()
        {
            if (AdsController.Instance && !AdsController.Instance.PreloadInterDefault)
            {
                if (showInter) return;
                showInter = true;
                CancelInvoke();
                UIManager.Instance.HidePopup<UILoadingAds>();
                AdsController.Instance.ShowInter(CurrentPlacement, () =>
                {
                    OnInterSuccess?.Invoke();
                });

            }
        }

    }
}
