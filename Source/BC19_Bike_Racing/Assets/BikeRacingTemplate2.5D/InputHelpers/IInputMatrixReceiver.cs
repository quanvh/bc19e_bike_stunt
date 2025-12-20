namespace Kamgam.InputHelpers
{
    /// <summary>
    /// Implements some methods which will allow a listener to get notified 
    /// if it is added or removed from the input matrix´s active stack.
    /// </summary>
    public interface IInputMatrixReceiver
    {
        void OnActivatedInMatrix();
        void OnDeactivatedInMatrix();
    }
}
