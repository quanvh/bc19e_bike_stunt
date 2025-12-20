using UnityEngine;
using Kamgam.BikeAndCharacter25D.Helpers;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Bike // .Input
    {
        protected float lastInputTime;

        protected bool isSpeedingUp = false;
        public bool IsSpeedingUp
        {
            get => isSpeedingUp;
            set
            {
                if (value)
                    updateLastInputTime();

                if (isSpeedingUp == value)
                    return;

                isSpeedingUp = value;
                if (isSpeedingUp)
                    isBraking = false;
            }
        }

        protected bool isBraking = false;
        public bool IsBraking
        {
            get => isBraking;
            set
            {
                if (value)
                    updateLastInputTime();

                if (isBraking == value)
                    return;

                isBraking = value;
                if (isBraking)
                {
                    isSpeedingUp = false;
                }
            }
        }

        protected int rotationDirection = 0;

        protected bool isRotatingCW = false;
        public bool IsRotatingCW
        {
            get => isRotatingCW;
            set
            {
                if (value)
                    updateLastInputTime();

                if (isRotatingCW == value)
                    return;

                isRotatingCW = value;
                if (isRotatingCW)
                {
                    isRotatingCCW = false;
                    rotationDirection = -1;
                    rotationInAirASR.Reset();
                    rotationInAirASR.On = true;
                }
                else
                {
                    rotationDirection = 0;
                    rotationInAirASR.On = false;
                }
            }
        }

        protected bool isRotatingCCW = false;

        public bool IsRotatingCCW
        {
            get => isRotatingCCW;
            set
            {
                if (value)
                    updateLastInputTime();

                if (isRotatingCCW == value)
                    return;

                isRotatingCCW = value;
                if (isRotatingCCW)
                {
                    isRotatingCW = false;
                    rotationDirection = 1;
                    rotationInAirASR.Reset();
                    rotationInAirASR.On = true;
                }
                else
                {
                    rotationDirection = 0;
                    rotationInAirASR.On = false;
                }
            }
        }

        public float LastInputActionTimeDelta
        {
            get => Time.realtimeSinceStartup - lastInputTime;
        }

        protected InputASR rotationInAirASR = new InputASR( 
            offValue: 0f,
            onValue: 1f,
            attackDuration: 0.3f,
            releaseDuration: 0.3f);

        protected void updateInput()
        {
            rotationInAirASR.Update();
            updateNextTorque();
        }

        public bool ReactsToInput()
        {
            return paused == false;
        }

        public void StopAllInput()
        {
            isBraking = false;
            isSpeedingUp = false;
            isRotatingCCW = false;
            isRotatingCW = false;
        }

        protected void updateNextTorque()
        {
            if (IsTouchingGround)
            {
                // don't tilt forward if only the front wheel is thouching the ground (it looks unrealistic)
                if (rotationDirection == -1 && FrontWheelGroundTouchTrigger.IsTouching && BackWheelGroundTouchTrigger.IsTouching == false)
                {
                    nextNormalizedInputTorque = 0;
                }
                else
                {
                    nextNormalizedInputTorque = rotationDirection;
                }
            }
            else
            {
                if(rotationDirection == 0)
                {
                    nextNormalizedInputTorque = (nextNormalizedInputTorque < 0 ? -1f : 1f) * rotationInAirASR.Value * 0.8f;
                }
                else
                {
                    nextNormalizedInputTorque =  rotationDirection * rotationInAirASR.Value * 0.8f;
                }
            }
        }

        protected void updateLastInputTime()
        {
            lastInputTime = Time.realtimeSinceStartup;
        }
    }
}
