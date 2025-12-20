using UnityEngine;

namespace Bacon
{
    public class UIUpdate : PopupBase
    {
        private string AppID_iOS => DataManager.Instance.appID_ios;

        public string Bundle_Android => DataManager.Instance.bundle_android;


        public void OnUpdate()
        {
#if UNITY_IOS
            Application.OpenURL("http://apps.apple.com/app/id" + AppID_iOS);
#elif UNITY_ANDROID
            Application.OpenURL("http://play.google.com/store/apps/details?id=" + Bundle_Android);
#endif
        }
    }
}