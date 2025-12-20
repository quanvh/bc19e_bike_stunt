using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
#if USE_ADMOB
using GoogleMobileAds.Ump.Api;
#endif

namespace Bacon
{
    public class UMP : MonoBehaviour
    {
#if USE_ADMOB
        protected string logPrefix = "UMP ";
        public bool HasUnknownError = false;
        public bool CanRequestAds => ConsentInformation.CanRequestAds();
        [Header("DEBUG")]
        [SerializeField]
        private DebugGeography debugGeography = DebugGeography.Disabled;
        [SerializeField, Tooltip("https://developers.google.com/admob/unity/test-ads")]
        private List<string> testDeviceIds;
        [SerializeField]
        private Button buttonReset;

        public static UMP Instance = null;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (buttonReset)
            {
                buttonReset.onClick.AddListener(ResetConsentInformation);
            }
        }

        protected IEnumerator OnCanRequestAd;
        protected bool isChecking = false;
        public IEnumerator DOGatherConsent(IEnumerator onCanRequestAd)
        {
            if (Instance == null)
                throw new Exception(logPrefix + "AdmobConsentController NULL");

            if (isChecking)
            {
                Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
                yield break;
            }

            OnCanRequestAd = onCanRequestAd;
            isChecking = true;

            var requestParameters = new ConsentRequestParameters();
            if (Instance.debugGeography != DebugGeography.Disabled)
            {
                requestParameters = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = new ConsentDebugSettings
                    {
                        DebugGeography = Instance.debugGeography,
                        TestDeviceHashedIds = Instance.testDeviceIds,
                    }
                };
            }


            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> UPDATE");

            ConsentInformation.Update(requestParameters, (FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                    FirebaseLogger.RegularEvent("ump_update_error_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;
                    HasUnknownError = true;
                    return;
                }

                if (CanRequestAds) // Determine the consent-related action to take based on the ConsentStatus.
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    Debug.Log(logPrefix + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                    FirebaseLogger.RegularEvent("ump_update_success_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;

                    if (onCanRequestAd != null)
                        UnityMainThreadDispatcher.Enqueue(OnCanRequestAd);
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");
                FirebaseLogger.RegularEvent("ump_loadshow");
#if UNITY_IOS
                if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED &&
                    ConsentInformation.ConsentStatus == ConsentStatus.Required)
                {
                    isChecking = false;
                }
                else
                {
                    ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
                    {
                        if (error != null) // Form load failed.
                        {
                            Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                            FirebaseLogger.RegularEvent("ump_loadshow_error");
                            HasUnknownError = true;
                        }
                        else  // Form showing succeeded.
                        {
                            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                            FirebaseLogger.RegularEvent("ump_loadshow_success");
                        }
                        isChecking = false;
                    });
                }
#else
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
                {
                    if (error != null) // Form load failed.
                    {
                        Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                        FirebaseLogger.RegularEvent("ump_loadshow_error");
                        HasUnknownError = true;
                    }
                    else  // Form showing succeeded.
                    {
                        Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                        FirebaseLogger.RegularEvent("ump_loadshow_success");
                    }
                    isChecking = false;
                });
#endif
            });

            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> WAIT!");
            while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
                yield return null;

            FirebaseLogger.RegularEvent("ump_status_" + ConsentInformation.ConsentStatus.ToString());
            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper());

            if (CanRequestAds && OnCanRequestAd != null)
            {
                yield return OnCanRequestAd;
            }
        }


        public void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
        {
            Debug.Log(logPrefix + "Showing privacy options form...");
            FirebaseLogger.RegularEvent("ump_option_show");
            ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogError(logPrefix + "Showing privacy options form - ERROR " + error.Message);
                    onComplete?.Invoke(error.Message);
                    FirebaseLogger.RegularEvent("ump_option_show_error");
                }
                else  // Form showing succeeded.
                {
                    if (privacyButton)
                        privacyButton.interactable = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
                    Debug.Log(logPrefix + "Showing privacy options form - SUCCESS");
                    onComplete?.Invoke(null);
                    FirebaseLogger.RegularEvent("ump_option_show_success");
                }
            });
        }


        public void ResetConsentInformation()
        {
            FirebaseLogger.RegularEvent("ump_reset");
            ConsentInformation.Reset();
        }
#else
        protected static string TAG = "UMP ";
        public static bool CanRequestAds => false;

        public static UMP Instance = null;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public IEnumerator DOGatherConsent(IEnumerator onCanRequestAd)
        {
            Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
            yield return onCanRequestAd;
        }

        public void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
        {
            Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
        }

        public void ResetConsentInformation()
        {
            Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
        }
#endif
    }
}