namespace Kamgam.BikeAndCharacter25D
{
    public partial class Character // .Input
    {
        public bool ReactToInput()
        {
            if (paused)
                return false;

            return true;
        }
    }
}
