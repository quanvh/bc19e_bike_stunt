using UnityEngine;
using Kamgam.BikeAndCharacter25D.Helpers;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Bike // .Brake
    {
        protected InputASR brakeForceASR;

        protected void updateBraking()
        {
            if (brakeForceASR == null)
            {
                this.brakeForceASR = new InputASR(0.0f, 1.0f, Config.BrakeForceAttackDuration, 0.0f, false);
            }

            brakeForceASR.On = isBraking;
            brakeForceASR.Update();
        }

        protected void fixedUpdateBraking()
        {
            // Brake automatically if no input was made and the speed is low.
            // lower limit (rollback and > -5, upper limit < 0.7f)
            // Debug.Log(LastRollBackTimeDelta + " < " + LastInputActionTimeDelta);
            bool noInputSinceLastRollBack = LastRollBackTimeDelta < LastInputActionTimeDelta;
            bool isRollingBackwards = VelocityVectorLocal.x > -5 && VelocityVectorLocal.x < 0f;
            bool isRollingForwards = VelocityVectorLocal.x > 0 && VelocityVectorLocal.x < 0.7f;
            bool backwardCheck = isRollingBackwards && LastRollBackTimeDelta > 0.1f && noInputSinceLastRollBack;
            bool forwardCheck = isRollingForwards && IsTouchingGround;
            bool slowSpeedBrake = !isSpeedingUp && (backwardCheck || forwardCheck);

            // brake
            if (isBraking)
            {
                if (this.brakeForceASR != null)
                {
                    nextMotorSpeedIncrement = 0;

                    JointMotor2D m = BackWheelJoint.motor;
                    m.motorSpeed = 0;
                    BackWheelJoint.motor = m;
                    if (brakeForceASR.Value <= 0.15f)
                    {
                        // light braking
                        if (BackWheelGroundTouchTrigger.IsTouching && BikeBody.gameObject.transform.InverseTransformVector(BikeBody.velocity.x, BikeBody.velocity.y, 0).x > 3)
                        {
                            // high speed
                            if (BackWheelGroundTouchTrigger.IsTouching)
                            {
                                BikeBody.AddRelativeForce(new Vector2(-Config.BrakeForce, 0f)); // slow down the bike without wheel blocking
                            }
                        }
                        else
                        {
                            // low speed breaking (to avoid being pushed backwards by the brake force)
                            BackWheelBody.freezeRotation = true;
                        }
                    }
                    else if (brakeForceASR.Value > 0.15f && brakeForceASR.Value <= 0.3f)
                    {
                        // medium braking
                        BackWheelBody.freezeRotation = true;
                    }
                    else
                    {
                        // strong braking
                        FrontWheelBody.freezeRotation = true;
                        BackWheelBody.freezeRotation = true;
                    }
                }
            }
            else if(slowSpeedBrake && AutoBrakeAtLowSpeed)
            {
                // strong braking
                FrontWheelBody.freezeRotation = true;
                BackWheelBody.freezeRotation = true;
            }
            else
            {
                FrontWheelBody.freezeRotation = false;
                BackWheelBody.freezeRotation = false;
            }
        }
    }
}
