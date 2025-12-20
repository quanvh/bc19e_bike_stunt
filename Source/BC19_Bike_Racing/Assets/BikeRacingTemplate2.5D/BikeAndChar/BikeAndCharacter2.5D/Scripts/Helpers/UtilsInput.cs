using UnityEngine;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public static class UtilsInput
    {
        public const string AxisMouseX = "Mouse X";
        public const string AxisMouseY = "Mouse Y";
        public const KeyCode KeyboardBack = KeyCode.Escape;
        public const KeyCode KeyboardConfirm = KeyCode.Return;

        public static float GetAxisMouseX()
        {
            return Input.GetAxis(AxisMouseX);
        }

        public static float GetAxisMouseY()
        {
            return Input.GetAxis(AxisMouseY);
        }

        public static bool BackPressed()
        {
            return Input.GetKeyDown(KeyboardBack);
        }

        public static bool BackReleased()
        {
            return Input.GetKeyUp(KeyboardBack);
        }

        public static bool ConfirmPressed()
        {
            return Input.GetKeyDown(KeyboardConfirm);
        }

        public static bool ConfirmReleased()
        {
            return Input.GetKeyUp(KeyboardConfirm);
        }

        public static bool AnyPressed()
        {
            return Input.anyKeyDown;
        }
    }
}
