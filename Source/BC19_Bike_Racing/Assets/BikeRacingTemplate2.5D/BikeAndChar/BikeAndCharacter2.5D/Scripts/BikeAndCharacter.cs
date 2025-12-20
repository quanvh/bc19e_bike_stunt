using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    public class BikeAndCharacter : MonoBehaviour
    {
        public Bike Bike;
        public Character Character;

        [System.NonSerialized]
        public bool HandleUserInput = true;

        [System.NonSerialized]
        public IBikeTouchInput TouchInput;

        public bool Grounded => Bike.FrontWheelGroundTouchTrigger.IsTouching || Bike.BackWheelGroundTouchTrigger.IsTouching;

        public void Update()
        {
            if (!HandleUserInput)
                return;

            if (TouchInput != null)
            {
                Bike.IsBraking = TouchInput.IsBrakePressed() || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                Bike.IsSpeedingUp = TouchInput.IsSpeedUpPressed() || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space);
                Bike.IsRotatingCW = TouchInput.IsRotateCWPressed() || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
                Bike.IsRotatingCCW = TouchInput.IsRotateCCWPressed() || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
            }

            if (Bike.IsRotatingCW)
                Character.TiltForward();
            else if (Bike.IsRotatingCCW)
                Character.TiltBackward();
            else
                Character.StopTilt();
        }

        public void StopAllInput()
        {
            Bike.IsBraking = false;
            Bike.IsSpeedingUp = false;
            Bike.IsRotatingCW = false;
            Bike.IsRotatingCCW = false;

            Character.StopTilt();
        }

        public void DisconnectBike()
        {
            DisconnectCharacterFromBike(Bike, Character);
        }

        public void DisconnectCharacterFromBike(Bike bike, Character character, bool addImpulse = true)
        {
            // disconnect char if needed
            if (character.IsConnectedToBike)
            {
                character.DisconnectFromBike(bike, addImpulse);
                bike.StopAllInput();
            }
        }

        public void ConnectCharacterToBike(Bike bike, Character character)
        {
            if (!character.IsConnectedToBike)
            {
                character.ConnectToBike(Bike);
                bike.StopAllInput();
            }
        }

        public Vector3 GetPosition()
        {
            return Bike.transform.position;
        }
    }
}