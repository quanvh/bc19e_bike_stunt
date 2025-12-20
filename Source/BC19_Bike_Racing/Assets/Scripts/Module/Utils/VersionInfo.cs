using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class VersionInfo : MonoBehaviour
    {
        int bundleVersion = 230201100;

        public Text txtVersion;

        private void Start()
        {
            if (txtVersion == null)
            {
                txtVersion = GetComponent<Text>();
            }
            UpdateVersionInfo();
        }

        public void UpdateVersionInfo()
        {
#if UNITY_EDITOR
#if UNITY_ANDROID
            bundleVersion = UnityEditor.PlayerSettings.Android.bundleVersionCode;
#endif

#if UNITY_IOS
            bundleVersion = int.Parse(UnityEditor.PlayerSettings.iOS.buildNumber);
#endif
            UnityEditor.EditorUtility.SetDirty(this);
#else
        bundleVersion = DataManager.Instance.version;
#endif


            if (txtVersion != null)
            {
                txtVersion.text = "VERSION " + Application.version + " [" + bundleVersion + "]";
            }

        }
    }
}