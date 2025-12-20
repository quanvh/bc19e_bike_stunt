using System.Collections;
using UnityEngine;

#if USE_ADVERTY
using Adverty;
#endif

namespace Bacon
{
    public partial class AdsController : MonoBehaviour
    {
#if USE_ADVERTY
    [Header("ADVERTY"), Space]
    [SerializeField] bool adverty_autoInit = false;
    [SerializeField] string adverty_android = "YWJmMTU5N2YtMWI1Ny00NWY2LWE0NjQtY2NkMzM1ZjNjNzM4JGh0dHBzOi8vYWRzZXJ2ZXIuYWR2ZXJ0eS5jb20=";
    [SerializeField] string adverty_ios = "";
    [SerializeField] bool allowLazyLoad = true;

    protected string apiKey = "";

    protected string TAG = "[AdsController] Adverty ";

    protected Camera gameCamera;

    protected bool IsInit = false;



    private void OnUnitActivated(BaseUnit obj)
    {
        FirebaseLogger.AdIngameActive(obj.Id, obj.name, CurrentLevel);
    }

    private void OnUnitActivationFailed(BaseUnit obj)
    {
        FirebaseLogger.AdIngameActive(obj.Id, obj.name, CurrentLevel);
    }

    private void OnUnitDeactivated(BaseUnit obj)
    {
        FirebaseLogger.AdIngameActive(obj.Id, obj.name, CurrentLevel);
    }

    private void OnUnitViewed(BaseUnit obj)
    {
        FirebaseLogger.AdIngameActive(obj.Id, obj.name, CurrentLevel);
    }

#endif

        public IEnumerator InitAdverty()
        {

#if USE_ADVERTY
        //AdvertyEvents.UnitViewed += OnUnitViewed;
        //AdvertyEvents.UnitActivated += OnUnitActivated;
        //AdvertyEvents.UnitActivationFailed += OnUnitActivationFailed;
        //AdvertyEvents.UnitDeactivated += OnUnitDeactivated;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            apiKey = adverty_ios;
        else
            apiKey = adverty_android;

        Debug.Log(TAG + "Init SDK: " + apiKey);

        //AdvertySettings.SandboxMode = DebugMode.IsOn;
        AdvertySettings.IsLazyLoadAllowed = allowLazyLoad;

        AdvertySettings.Mode platform = AdvertySettings.Mode.Mobile; //define target platform (Mobile, VR, AR)
        bool restrictUserDataCollection = false; //do you disallow collect extra user data?

        //Adverty.UserData userData = new Adverty.UserData(AgeSegment.Adult, Gender.Male); // define user as adult male
        UserData userData = new UserData(AgeSegment.Unknown, Gender.Unknown); //define user data (user and gender are unknown)

        AdvertySDK.Init(apiKey.Trim(), platform, restrictUserDataCollection, userData);

        Debug.Log(TAG + "Init SDK DONE IsAPIKeyValid " + AdvertySettings.IsAPIKeyValid + " IsLazyLoadAllowed " + AdvertySettings.IsLazyLoadAllowed + " SandboxMode " + AdvertySettings.SandboxMode);

        UpdateCamera();

        IsInit = true;
#endif
            yield return true;
        }

        public void UpdateCamera(Camera camera = null)
        {
#if USE_ADVERTY
        if (camera != null)
            gameCamera = camera;

        if (gameCamera == null)
            gameCamera = Camera.main;

        if (gameCamera != null)
        {
            AdvertySettings.SetMainCamera(gameCamera);
            Debug.Log(TAG + "SetMainCamera DONE");
        }
        else
            Debug.LogWarning(TAG + "SetMainCamera NULL");
#endif
        }
    }
}