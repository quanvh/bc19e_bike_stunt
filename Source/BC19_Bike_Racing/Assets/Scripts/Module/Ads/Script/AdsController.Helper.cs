using DG.Tweening;
using System;
using UnityEngine;

#if USE_ADMOB
using GoogleMobileAds.Common;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
        //Test ID
        //banner_id_android: ca-app-pub-3940256099942544/6300978111
        //inter_id_android: ca-app-pub-3940256099942544/1033173712
        //reward_id_android: ca-app-pub-3940256099942544/5224354917
        //openad_id_android: ca-app-pub-3940256099942544/3419835294
        //banner_collapse_android: ca-app-pub-3940256099942544/2014213617
        //native_id_android: ca-app-pub-3940256099942544/2247696110

        //banner_id_ios: ca-app-pub-3940256099942544/2934735716
        //inter_id_ios: ca-app-pub-3940256099942544/4411468910
        //reward_id_ios: ca-app-pub-3940256099942544/1712485313
        //openad_id_ios: ca-app-pub-3940256099942544/5662855259
        //banner_collapse_ios: ca-app-pub-3940256099942544/8388050270
        //native_id_ios: ca-app-pub-3940256099942544/3986624511


        #region HELPER_FUNCTION

        public void ExecuteAction(Action action)
        {
#if USE_ADMOB
            if (ExecuteInUpdate)
            {
                MobileAdsEventExecutor.ExecuteInUpdate(action);
            }
            else
            {
                action();
            }
#else
                action();
#endif
        }

        public bool IsDuration()
        {
            if (firstStart)
            {
                firstStart = false;
                return (DateTime.Now - lastAdTime).TotalSeconds > TimeFirstInter;
            }
            else
            {
                return (DateTime.Now - lastAdTime).TotalSeconds > DurationBetweenAds;
            }
        }
        public void MuteGameSound()
        {
            //if (AudioController.Instance)
            //    AudioController.Instance.PauseAllSound(true);
            ShowAds = true;
            SetVolumeFade(0, 0.25f);
#if !UNITY_EDITOR
        Time.timeScale = 0;
#endif
        }
        public void OpenGameSound()
        {
            //if (AudioController.Instance)
            //    AudioController.Instance.ResumeSound();
            SetVolumeFade(1f, 1f);
            Invoke(nameof(OpenAdsListen), 1.0f);
#if !UNITY_EDITOR
        Time.timeScale = 1f;
#endif
        }

        private void OpenAdsListen()
        {
            ShowAds = false;
        }

        public void SetVolumeFade(float endValue, float duration)
        {
            if (logDebug)
                Debug.Log(logPrefix + "SetVolumeFade: " + endValue);
            DOTween.Kill(nameof(SetVolumeFade), true);
            var volume = AudioListener.volume;
            DOVirtual.Float(volume, endValue, duration, (v) =>
            {
                AudioListener.volume = v;
            })
            .SetId("SetVolumeFade")
            .SetUpdate(true);
        }
        #endregion
    }
}