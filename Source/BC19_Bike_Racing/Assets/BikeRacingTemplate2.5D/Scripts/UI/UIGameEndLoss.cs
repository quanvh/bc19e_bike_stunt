using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    public class UIGameEndLoss : UIBaseFade
    {
        public System.Action OnReturnToMenu;
        public System.Action OnRestart;

        public Button ReturnToMenuBtn;
        public Button RestartBtn;
        public TextMeshProUGUI TimeTf;

        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => false;

        public void SetTime(float timeInSec, LevelData levelData)
        {
            TimeTf.text = string.Format(
                "{0} (Time to beat: {1})",
                UIGameTime.FormatRaceTime(timeInSec),
                UIGameTime.FormatRaceTime(levelData.TimeToBeat)
                );
        }

        public override void OnClick(GameObject obj, BaseEventData evt)
        {
            if (obj == ReturnToMenuBtn.gameObject)
                OnReturnToMenu?.Invoke();
            else if (obj == RestartBtn.gameObject)
                OnRestart?.Invoke();
        }
    }
}
