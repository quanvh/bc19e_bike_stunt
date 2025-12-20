using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    public class UIGamePause : UIBase
    {
        public System.Action OnReturnToMenu;
        public System.Action OnRestart; 
        public System.Action OnUnpause;

        public Button ReturnToMenuBtn;
        public Button RestartBtn;
        public Button UnpauseBtn;

        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => false;

        public override void OnClick(GameObject obj, BaseEventData evt)
        {
            if (obj == ReturnToMenuBtn.gameObject)
                OnReturnToMenu?.Invoke();

            else if (obj == RestartBtn.gameObject)
                OnRestart?.Invoke();

            else if (obj == UnpauseBtn.gameObject)
                OnUnpause?.Invoke();
        }
    }
}
