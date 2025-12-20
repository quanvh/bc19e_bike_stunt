namespace Kamgam.BikeRacing25D
{
    public class UIGameMenuForKeyboard : UIBaseFade
    {
        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => true;
    }
}
