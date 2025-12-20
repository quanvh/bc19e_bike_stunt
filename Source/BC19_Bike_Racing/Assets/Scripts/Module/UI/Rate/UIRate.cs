using System;
using System.Collections;
using UnityEngine;


#if UNITY_ANDROID && USE_RATE
using Google.Play.Review;
#endif

namespace Bacon
{
    public class UIRate : PopupBase
    {
        [Header("POPUP PARAM")]
        [SerializeField] private int BonusRate = 5000;

        public string AppID_iOS => DataManager.Instance.appID_ios;

        public string Bundle_Android => DataManager.Instance.bundle_android;

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            if (AdsController.Instance)
            {
                AdsController.Instance.ShowMrec(MrecName.Mrec_Default);
            }
            _player.showRate = true;
        }

        protected override void HideStart()
        {
            base.HideStart();
            if (AdsController.Instance)
            {
                AdsController.Instance.HideMrec(MrecName.Mrec_Default);
            }
        }

        public void OnClickRate()
        {

#if UNITY_IOS
        var canReview = UnityEngine.iOS.Device.RequestStoreReview();
        if (canReview == false)
            Application.OpenURL("http://apps.apple.com/app/id" + AppID_iOS);
#elif UNITY_ANDROID
            StartCoroutine(RequestReview(onDone =>
            {
                if (onDone == false)
                {
                    Application.OpenURL("http://play.google.com/store/apps/details?id=" + Bundle_Android);
                }
            }));
#endif

            //if (AdsController.Instance) AdsController.Instance.hideApp = true;

            _player.currentGold += BonusRate;
            UIToast.Instance.Toast("Thanks for rating! You've just got " + BonusRate + " golds");

            BackGarage();
        }

        public void OnClickIgnore()
        {
            BackGarage();
        }

        private void BackGarage()
        {
            DataManager.Instance.Save();
            Hide(() => RouteController.Instance.PrepareGarage());
        }


        public IEnumerator RequestReview(Action<bool> onDone)
        {
#if UNITY_ANDROID && USE_RATE
            var reviewManager = new ReviewManager();
            var playReviewInfoAsyncOperation = reviewManager.RequestReviewFlow();
            playReviewInfoAsyncOperation.Completed += playReviewInfoAsync =>
            {
                if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
                {
                    // display the review prompt
                    var playReviewInfo = playReviewInfoAsync.GetResult();
                    var launchReviewFlow = reviewManager.LaunchReviewFlow(playReviewInfo);
                    launchReviewFlow.Completed += (result) =>
                    {
                        onDone?.Invoke(result.IsSuccessful);
                        Debug.Log("LaunchReviewFlow: " + result.IsSuccessful
                            + ", done: " + result.IsDone + ", error: " + playReviewInfoAsync.Error.ToString());
                    };
                }
                else
                {
                    Debug.LogError("Handle error when loading review prompt");
                    onDone?.Invoke(false);
                }
            };
#else
            onDone(false);
#endif
            yield return null;
        }
    }
}