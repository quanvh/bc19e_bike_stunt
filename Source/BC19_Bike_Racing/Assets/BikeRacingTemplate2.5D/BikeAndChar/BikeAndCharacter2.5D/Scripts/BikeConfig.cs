using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    [CreateAssetMenu(fileName = "BikeConfig", menuName = "BikeAndCharacter25D/BikeConfig", order = 1)]
    public class BikeConfig : ScriptableObject
    {
        [Header("Bike")]

        public float BikeGravityScale = 1.0f;
        public float BikeAngularDrag = 30.0f;
        public float BikeAngularDragInAir = 60.0f;
        public float BikeBodyMass = 90.0f;
        public float BikeWheelMass = 5.0f;

        // Speed - modified by SetBikeValues(...)
        public float BikeMaxMotorSpeed = 20200;
        public float BikeMaxMotorTorque = 580;
        public float BikeMaxMotorTorqueOnSlopes = 590;
        public float BikeMotorSpeedIncrement = 200;

        // Brake
        public float BrakeForceAttackDuration = 0.5f;
        public float BrakeForce = 1500;

        // Limits - modified by SetBikeValues(...)
        public float BikeMaxVelocity = 35; // limits the max velocity of the bike

        // Rotation - modified by SetBikeValues(...)
        public float BikeTorqueForce = 1200;
        public float BikeMoveRotation = 50f;
        [Tooltip("Add some extra downward force on the back wheel on steep slopes if leaning forward and speeding up.")]
        public float BikeBackWheelDownForceOnSlopes = 2000;

        public BikeConfig ShallowCopy()
        {
            return (BikeConfig) this.MemberwiseClone();
        }
    }
}