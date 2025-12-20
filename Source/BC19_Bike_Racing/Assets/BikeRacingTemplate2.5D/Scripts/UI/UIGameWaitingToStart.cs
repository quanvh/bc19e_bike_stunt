using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    public class UIGameWaitingToStart : UIBaseFade
    {
        public GameObject StartMessageForKeyboard;

        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => true;

        void OnEnable()
        {
            StartMessageForKeyboard.SetActive(!Main.IsTouchEnabled());
        }
    }
}
