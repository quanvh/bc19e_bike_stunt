using Kamgam.InputHelpers;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    public class UILoadingScreen : UIBaseFade
    {
        public TMPro.TextMeshProUGUI LoadingMessageTf;
        public TMPro.TextMeshProUGUI VersionTf;

        public override UIStack GetUIStack()
        {
            return UIStack.Loading;
        }

        void OnEnable()
        {
            VersionTf.text = "Version " + Main.GameVersion;
        }

        public void SetText(string text)
        {
            LoadingMessageTf.text = text;
        }
    }
}
