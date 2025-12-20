namespace Kamgam.BikeAndCharacter25D
{
    public interface IBikeTouchInput
    {
        bool IsSpeedUpPressed();
        bool IsBrakePressed();
        bool IsRotateCCWPressed();
        bool IsRotateCWPressed();
    }
}