using System.Collections;
using UnityEngine;


namespace Bacon
{
#if UNITY_IOS
/*
1. In the Unity Editor, select Window > Package Manager to open the Package Manager.
2. Select the iOS 14 Advertising Support package from the list, then select the most recent verified version.
3. Click the Install or Update button.
*/

using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
using System.Runtime.InteropServices;
#endif

#if USE_FACEBOOK && UNITY_IOS && !UNITY_EDITOR
public static class AudienceNetworkSettings
{
    [DllImport("__Internal")]
    private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

    public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
    {
        FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);

        Debug.Log("FBAdSettingsBridgeSetAdvertiserTrackingEnabled: " + advertiserTrackingEnabled);
    }
}
#endif

    public class ATTHelper : MonoBehaviour
    {
        public static bool isAppsFlyerAdRevenueStarted = false;
        public bool checkAtAwake = false;

        private void Awake()
        {
            if (checkAtAwake)
                StartCoroutine(DOCheckATT());
        }

        public static IEnumerator DOCheckATT()
        {
#if UNITY_IOS
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

        Debug.LogWarning("[ATTHelper] GetAuthorizationTrackingStatus: " + status);

        yield return new WaitForSeconds(0.1f);

        if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            //Change iOS in com.unity.ads.ios-support from  1.0.0 to 1.2.0 in "Packages/manifest.json" and "Packages/packages-lock.json"
            ATTrackingStatusBinding.RequestAuthorizationTracking((status) =>
            {
                var authorizationTrackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                Debug.LogWarning("[ATTHelper] RequestAuthorizationTracking: " + status + " " + authorizationTrackingStatus);

#if USE_FACEBOOK && !UNITY_EDITOR
                if (authorizationTrackingStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
                    AudienceNetworkSettings.SetAdvertiserTrackingEnabled(true);
                else
                    AudienceNetworkSettings.SetAdvertiserTrackingEnabled(false);
#endif
            });
        }

        float timeOut = 1.0f;
        while (timeOut > 0 && status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            timeOut -= Time.deltaTime;
            yield return null;
        }
#endif

            yield return new WaitForSeconds(0.1f);
        }
    }
}