using UnityEngine;
using System.Collections.Generic;
using Kamgam.BikeAndCharacter25D.Helpers;
using System;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Bike : MonoBehaviour
    {
        public const int AVERAGE_QUEUE_MAX = 300;
        public const float AVERAGE_DELTA_TIME = 1f / 50f;

        public BikeConfig Config;

        protected float tmpAverageDeltaCountdown;
        protected int numOfEntriesMax = 10;

        // Bike
        public GameObject BikeBack;
        public GameObject FrontWheel;
        public GameObject BackWheel;
        public GameObject BackWheelForceLocation;
        public GameObject CenterOfMass;
        public GameObject CenterOfMassInAir;

        public Rigidbody2D BikeBody;
        public Rigidbody2D FrontWheelBody;
        public Rigidbody2D BackWheelBody;

        public WheelJoint2D FrontWheelJoint;
        public WheelJoint2D BackWheelJoint;

        // wheel suspension
        public bool SuspensionImpactHelp = true;
        public Collider2D[] BikeBodySuspensionMaxedColliders;
        public Collider2D[] BikeBodyDefaultColliders;

        [Header("Triggers & Ground")]
        [System.NonSerialized]
        public bool HandleTriggers;
        public GroundTouchTrigger FrontWheelGroundTouchTrigger;
        public GroundTouchTrigger BackWheelGroundTouchTrigger;
        public GroundTouchTrigger BackWheelOuterGroundTouchTrigger; // currently not used (use later for wheely detection)

        [Tooltip("Only these layers are considered a valid ground which the bike can get traction on.\nIf teh bike stands on something else then no input will be taken into account.")]
        public LayerMask GroundLayers;

        [Tooltip("Add some helper forces to avoid the bike from rolling over. Makes driving the bike much easier.")]
        public bool RollOverProtection = true;

        [Tooltip("If the bike starts to roll backward/forward then brake automatically (as any real world bike drive would). It's a convenience feature. Tapping the brakes once will release them.")]
        public bool AutoBrakeAtLowSpeed = true;

        // Bike Rotation
        /// <summary>
        /// Four rotation quadrants starting facing right and turning up (1 = 0-90 degrees, 2 = 90-180, ...)
        /// </summary>
        protected int bikeRotationQuadrant = 1;

        public bool BikeForward;

        /// <summary>
        /// 1 means the bike is tilted upwards, 2 means it is tilted downwards<br />
        /// 1 or 2 / 1 = 270 - 360 & 0 - 90, 2 = the rest
        /// </summary>
        protected int bikeRotationHalf = 1;

        protected float bikeRotationAngle;
        /// <summary>
        /// Returns the rotation angle of the bike (0 = horizontal, 90 rotated backwards facing up, 180 being upside down, up to 360).<br />
        /// It is aware of the turned state of the bike. 90 will always mean leaning backward and facing up.
        /// </summary>
        public float BikeRotationAngle => bikeRotationAngle;

        protected float bikePitchAngle;
        /// <summary>
        /// Returns the rotation angle of the bike compared to the horizontal plane (0 = horizontal, 90 rotated backwards facing up, -90 rotated down).<br />
        /// It is aware of the turned state of the bike. 90 will always mean leaning backward and facing up.
        /// </summary>
        public float BikePitchAngle => bikePitchAngle;

        protected float bikePitchAngleAbs;
        /// <summary>
        /// Returns the rotation angle of the bike compared to the horizontal plane. The angle is always positive.<br />
        /// It is aware of the turned state of the bike.
        /// </summary>
        public float BikePitchAngleAbs => bikePitchAngleAbs;

        // Speed - modified by SetBikeValues(...)
        protected float configMaxMotorSpeed;
        protected float configMaxMotorTorque;
        protected float configMaxMotorTorqueOnSlopes;
        protected float configMotorSpeedIncrement;
        protected float configMaxVelocity; // if the bike is faster than this limit then boost or speed Up have no effect.
        protected float configTorqueForce;

        protected Vector2 centerOfMassPos;
        protected Vector2 centerOfMassInAirPos;

        protected float nextMotorSpeedIncrement;
        protected float motorSpeed;
        /// <summary>
        /// A value between -1 and +1, defines whether or not the user wants to rotate the bike in the next physics update.
        /// </summary>
        protected float nextNormalizedInputTorque;

        /// <summary>
        /// How long has the bike been in the air now? Is 0 if touching ground.
        /// </summary>
        protected float flightDuration;
        /// <summary>
        /// How long has the bike been in the air now? Is 0 if touching ground.
        /// </summary>
        public float FlightDuration { get => flightDuration; }
        /// <summary>
        /// How long did the last flight take? May be very small if hopping after landing. Is equal to flightDuration while flying.
        /// </summary>
        protected float lastFlightDuration;

        /// <summary>
        /// How long has the front wheel been in the air now? Is 0 if touching ground.
        /// </summary>
        protected float frontWheelFlightDuration;

        /// <summary>
        /// How long has the back wheel been in the air now? Is 0 if touching ground.
        /// </summary>
        protected float backWheelFlightDuration;

        /// <summary>
        /// How long has the bike been on the ground now? Is 0 if in mid air.
        /// </summary>
        protected float groundDuration; //TODO: add to Rewindable
        /// <summary>
        /// How long has the bike been in the ground now? Is 0 if touching ground.
        /// </summary>
        public float GroundDuration { get => groundDuration; }

        /// <summary>
        /// How long did the last ground touch take? May be very small if hopping after landing.
        /// </summary>
        protected float lastGroundDuration;

        /// <summary>
        /// How long has it been since the last contact to ground?<br />
        /// Calculated based on Time.realtimeSinceStartup.
        /// </summary>
        protected float lastGroundTime;

        /// <summary>
        /// Will be true if the bike is rolling backwards.
        /// </summary>
        protected bool lookingDirectionIsOppositeToVelocity;

        // some avg stats
        protected Vector2AverageQueue velocities;
        protected Vector2AverageQueue forwardVectors;
        protected Vector2 tmpLastVelocityVectorLocal = Vector2.zero;

        protected float lastRollBackTime = 0;

        /// <summary>
        /// A "roll back" is defined as the bike changing the localVelocity.x direction without turning.
        /// </summary>
        public float LastRollBackTimeDelta { get => Time.realtimeSinceStartup - lastRollBackTime; }

        protected Vector3 frontWheelDefaultPos;
        protected Vector3 backWheelDefaultPos;

        protected Vector3 frontWheelVectorToDefault;
        protected Vector3 backWheelVectorToDefault;

        [System.NonSerialized]
        public bool Crashed;

        [HideInInspector]
        public int RotationQuadrant
        {
            get
            {
                return this.bikeRotationQuadrant;
            }
        }

        public bool IsUpsideDown
        {
            get
            {
                return this.bikeRotationQuadrant == 2 || this.bikeRotationQuadrant == 3;
            }
        }

        [HideInInspector]
        public float Velocity
        {
            get
            {
                return BikeBody.velocity.magnitude;
            }
        }

        /// <summary>
        /// The bike rigidbody velocity in world space in units per second.
        /// </summary>
        [HideInInspector]
        public Vector2 VelocityVector
        {
            get
            {
                return BikeBody.velocity;
            }
        }

        [HideInInspector]
        public Vector2 VelocityVectorLocal
        {
            get
            {
                return this.transform.InverseTransformVector(BikeBody.velocity);
            }
        }

        /// <summary>
        /// Gives the average of the last N seconds.
        /// </summary>
        /// <param name="timeframeInSec"></param>
        /// <returns></returns>
        public Vector2 GetVelocityVectorAverage(float timeframeInSec = 0.5f)
        {
            int numOfEntriesToSample = Mathf.RoundToInt(timeframeInSec / AVERAGE_DELTA_TIME);
            if (numOfEntriesToSample > 1 && velocities.Count > 0)
            {
                velocities.UpdateAverage(numOfEntriesToSample);
                return velocities.Average();
            }
            else
            {
                return VelocityVector;
            }
        }

        /// <summary>
        /// The bikes local forward vector transformed into world space.
        /// </summary>
        [HideInInspector]
        public Vector3 ForwardVector
        {
            get
            {
                return this.transform.TransformVector(Vector3.right);
            }
        }

        /// <summary>
        /// Gives the average of the last N seconds.
        /// </summary>
        /// <param name="timeframeInSec"></param>
        /// <returns></returns>
        public Vector2 GetForwardVectorAverage(float timeframeInSec = 0.5f)
        {
            int numOfEntriesToSample = Mathf.RoundToInt(timeframeInSec / AVERAGE_DELTA_TIME);
            if (numOfEntriesToSample > 1 && forwardVectors.Count > 0)
            {
                forwardVectors.UpdateAverage(numOfEntriesToSample);
                return forwardVectors.Average();
            }
            else
            {
                return ForwardVector;
            }
        }

        /// <summary>
        /// The bikes local upward vector transformed into world space.
        /// </summary>
        [HideInInspector]
        public Vector3 UpwardVector
        {
            get
            {
                return this.transform.TransformVector(Vector3.up);
            }
        }

        [HideInInspector]
        public float SlopeInDegrees
        {
            get
            {
                var angle = this.transform.eulerAngles.z;
                if (angle > 180)
                {
                    return angle - 360;
                }
                else
                {
                    return angle;
                }
            }
        }

        /// <summary>
        /// The tilt angle of the bike independent of turn.
        /// Values are between -180 and +180, 0 means horizontal.
        /// Positive means tilted upwards, negative means tilted downwards.
        /// </summary>
        public float SlopeInDegreesAbsolute
        {
            get
            {
                var angle = this.transform.eulerAngles.z;
                if (angle > 180)
                {
                    angle = angle - 360;
                }
                return angle;
            }
        }

        /// <summary>
        /// Is any wheel of the bike touching the ground?
        /// </summary>
        [HideInInspector]
        public bool IsTouchingGround
        {
            get
            {
                return BackWheelOuterGroundTouchTrigger.IsTouching || BackWheelGroundTouchTrigger.IsTouching || FrontWheelGroundTouchTrigger.IsTouching;
            }
        }

        /// <summary>
        /// Is the front wheel of the bike touching the ground?
        /// </summary>
        [HideInInspector]
        public bool IsFrontTouchingGround
        {
            get
            {
                return FrontWheelGroundTouchTrigger.IsTouching;
            }
        }

        /// <summary>
        /// Is the back wheel of the bike touching the ground?
        /// The back wheel will report touch as true even if it's very close to the ground but does not really touch it.
        /// </summary>
        [HideInInspector]
        public bool IsBackTouchingGround
        {
            get
            {
                return BackWheelOuterGroundTouchTrigger.IsTouching || BackWheelGroundTouchTrigger.IsTouching;
            }
        }

        protected float standstillTime;

        /// <summary>
        /// How long the bike has been standing still in seconds (Time.deltatime based).
        /// </summary>
        [HideInInspector]
        public float StandstillTime
        {
            get
            {
                return standstillTime;
            }
        }

        protected bool paused;

        [HideInInspector]
        public bool IsWaitingToRace
        {
            get
            {
                return paused;
            }
        }

        protected bool awoken;

        void Awake()
        {
            loadConfig();

            // Set center of mass
            updateCenterOfMass();
            BikeBody.centerOfMass = centerOfMassPos;
            frontWheelDefaultPos = FrontWheel.transform.localPosition;
            backWheelDefaultPos = BackWheel.transform.localPosition;

            // averages
            velocities = new Vector2AverageQueue(numOfEntriesMax);
            forwardVectors = new Vector2AverageQueue(numOfEntriesMax);

            awakeParticles();

            awoken = true;
        }

        protected void updateCenterOfMass()
        {
            centerOfMassPos = new Vector2(CenterOfMass.transform.localPosition.x * BikeBody.transform.localScale.x,
                               CenterOfMass.transform.localPosition.y * BikeBody.transform.localScale.y);
            centerOfMassInAirPos = new Vector2(
                                                CenterOfMassInAir.transform.localPosition.x * BikeBody.transform.localScale.x,
                                                CenterOfMassInAir.transform.localPosition.y * BikeBody.transform.localScale.y);
        }

        public float GetMaxVelocity()
        {
            return this.configMaxVelocity;
        }

        /// <summary>
        /// Gets the distance to the bikes main body (z coordinates are ignored).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float GetDistanceTo(Vector3 position)
        {
            return Vector2.Distance(this.BikeBody.position, position);
        }

        /// <summary>
        /// Gets the distance to the bikes main body (z coordinates are ignored).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetDistanceVectorTo(Vector3 position)
        {
            return position - new Vector3(this.BikeBody.position.x, this.BikeBody.position.y, this.transform.position.z);
        }

        public bool OverlapsCollider2D(params Collider2D[] colliders)
        {
            if (colliders.Length == 1)
            {
                return colliders[0].OverlapPoint(BikeBody.position);
            }
            else
            {
                foreach (var collider in colliders)
                {
                    if (collider.OverlapPoint(BikeBody.position))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected void loadConfig()
        {
            BikeBody.gravityScale = Config.BikeGravityScale;
            BikeBody.angularDrag = Config.BikeAngularDrag;
            BikeBody.mass = Config.BikeBodyMass;
            FrontWheelBody.mass = Config.BikeWheelMass;
            FrontWheelBody.gravityScale = Config.BikeGravityScale;
            BackWheelBody.mass = Config.BikeWheelMass;
            BackWheelBody.gravityScale = Config.BikeGravityScale;

            configMaxMotorTorque = Config.BikeMaxMotorTorque;
            configMaxMotorTorqueOnSlopes = Config.BikeMaxMotorTorqueOnSlopes;
            configMaxMotorSpeed = Config.BikeMaxMotorSpeed;
            configMotorSpeedIncrement = Config.BikeMotorSpeedIncrement;
            configMaxVelocity = Config.BikeMaxVelocity;
            configTorqueForce = Config.BikeTorqueForce;
        }

        public void SetPaused(bool paused)
        {
            this.paused = paused;
            this.setParticlesPaused(paused);
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            updateInput();
            updateBraking();

            // update air time
            if (IsTouchingGround == false)
            {
                flightDuration += Time.deltaTime;
                lastFlightDuration = flightDuration;

                if (IsFrontTouchingGround == false) frontWheelFlightDuration += Time.deltaTime;
                if (IsBackTouchingGround == false) backWheelFlightDuration += Time.deltaTime;
            }
            else
            {
                if (flightDuration > 0)
                {
                    lastFlightDuration = flightDuration;
                }
                flightDuration = 0;

                if (IsFrontTouchingGround == true) frontWheelFlightDuration = 0f;
                if (IsBackTouchingGround == true) backWheelFlightDuration = 0f;
            }

            // update ground time
            if (IsTouchingGround == true)
            {
                lastGroundTime = Time.realtimeSinceStartup;
                groundDuration += Time.deltaTime;
                lastGroundDuration = groundDuration;
            }
            else
            {
                if (groundDuration > 0)
                {
                    lastGroundDuration = groundDuration;
                }
                groundDuration = 0;
            }

            // update stand still time
            if (VelocityVectorLocal.x < 0.1f)
            {
                standstillTime += Time.deltaTime;
            }
            else
            {
                standstillTime = 0;
            }

            updateParticles(nextMotorSpeedIncrement);

            // Back wheel visuals: rotate the bikes back part (where the back wheel is connected) according to the wheel position
            float bikeBackAngle = Vector2.SignedAngle(BackWheel.transform.localPosition - BikeBack.transform.localPosition, new Vector3(-1, 0, 0));
            Vector3 bikeBackRotation = BikeBack.transform.localEulerAngles;
            bikeBackRotation.z = -bikeBackAngle;
            BikeBack.transform.localEulerAngles = bikeBackRotation;
        }

        void FixedUpdate()
        {
            if (paused)
                return;

#if UNITY_EDITOR
            // update configs if necessary
            loadConfig();
#endif

            // update bike rotation quadrant
            fixedUpdateRotationInformation();

            // change the center for the mass in mid air to make rotations easier
            if (IsTouchingGround)
            {
                if (BikeBody.centerOfMass != centerOfMassPos)
                    BikeBody.centerOfMass = centerOfMassPos;
            }
            else
            {
                if (BikeBody.centerOfMass != centerOfMassInAirPos)
                    BikeBody.centerOfMass = centerOfMassInAirPos;
            }

            // calc averages
            tmpAverageDeltaCountdown -= Time.deltaTime;
            if (tmpAverageDeltaCountdown <= 0)
            {
                tmpAverageDeltaCountdown = AVERAGE_DELTA_TIME;

                // keep track of velocity over multiple physics updates (usually one Updated every 0.02 Sec.)
                if (velocities.Count >= AVERAGE_QUEUE_MAX)
                {
                    velocities.Dequeue();
                }
                velocities.Enqueue(VelocityVector, false);

                // keep track of forward vector over multiple physics updates (usually one Updated every 0.02 Sec.)
                if (forwardVectors.Count >= AVERAGE_QUEUE_MAX)
                {
                    forwardVectors.Dequeue();
                }
                forwardVectors.Enqueue(ForwardVector, false);
            }

            // motor speed and helper forces concerning speed
            fixedUpdateSpeed();

            // apply Rotation
            fixedUpdateRotation();

            // handle braking
            fixedUpdateBraking();

            fixedUpdateCollidersForSuspension();

            // Roll back
            if (VelocityVectorLocal.x < 0 && tmpLastVelocityVectorLocal.x >= 0)
            {
                if (Mathf.Abs(VelocityVectorLocal.x) > 0.1f && FlightDuration < 0.1f) // avoid spamming rollback events if the bike is standing still
                {
                    lastRollBackTime = Time.realtimeSinceStartup;
                }
            }
            if (Mathf.Abs(VelocityVectorLocal.x) > 0.2f) // avoid spamming rollback events if the bike is standing still
            {
                tmpLastVelocityVectorLocal = VelocityVectorLocal;
            }
        }

        protected void fixedUpdateCollidersForSuspension()
        {
            /**
             * Wheels should be configured to not collide with the bike colliders.
             * If they are configured to not collide then some of the bike colliders
             * are disable to allow the wheels to move upwards without hitting anything.
             * This avoids the wheel hitting the bike colliders (which would make the
             * wheels stop even if the bike has zero friction.)
             **/
            if (!SuspensionImpactHelp)
                return;

            frontWheelVectorToDefault = frontWheelDefaultPos - FrontWheel.transform.localPosition;
            // backWheelVectorToDefault = backWheelDefaultPos - BackWheel.transform.localPosition;
            if (frontWheelVectorToDefault.magnitude > 0.33f /*|| backWheelVectorToDefault.magnitude > 0.30f*/)
            {
                for (int i = 0; i < BikeBodyDefaultColliders.Length; i++)
                {
                    BikeBodyDefaultColliders[i].enabled = false;
                }
                for (int i = 0; i < BikeBodySuspensionMaxedColliders.Length; i++)
                {
                    BikeBodySuspensionMaxedColliders[i].enabled = true;
                }
            }
            else
            {
                for (int i = 0; i < BikeBodyDefaultColliders.Length; i++)
                {
                    BikeBodyDefaultColliders[i].enabled = true;
                }
                for (int i = 0; i < BikeBodySuspensionMaxedColliders.Length; i++)
                {
                    BikeBodySuspensionMaxedColliders[i].enabled = false;
                }
            }
        }

        private readonly float flipAngle = 30f;
        protected void fixedUpdateRotationInformation()
        {
            if (this.transform.eulerAngles.z >= 0 && this.transform.eulerAngles.z <= 90)
            {
                bikeRotationQuadrant = 1;
                bikeRotationHalf = 1;
            }
            if (this.transform.eulerAngles.z > 90 && this.transform.eulerAngles.z <= 180)
            {
                bikeRotationQuadrant = 2;
                bikeRotationHalf = 2;
            }
            if (this.transform.eulerAngles.z > 180 && this.transform.eulerAngles.z < 270)
            {
                bikeRotationQuadrant = 3;
                bikeRotationHalf = 2;
            }
            if (this.transform.eulerAngles.z >= 270 && this.transform.eulerAngles.z <= 360)
            {
                bikeRotationQuadrant = 4;
                bikeRotationHalf = 1;
            }

            BikeForward = Math.Abs(this.transform.eulerAngles.z - 180f) < flipAngle;

            bikeRotationAngle = transform.eulerAngles.z;
            if (bikeRotationAngle < 180)
            {
                bikePitchAngle = bikeRotationAngle;
                bikePitchAngleAbs = bikeRotationAngle;
            }
            else
            {
                bikePitchAngle = bikeRotationAngle - 360;
                bikePitchAngleAbs = -bikePitchAngle;
            }
        }

        protected void fixedUpdateSpeed()
        {
            var forwardDirection = (Vector2)this.transform.TransformVector(new Vector3(1, 0, 0));
            this.lookingDirectionIsOppositeToVelocity = (VelocityVector + forwardDirection).magnitude < VelocityVector.magnitude;
            bool isUnderSpeedLimit = lookingDirectionIsOppositeToVelocity || Velocity < configMaxVelocity;
            if (IsTouchingGround && isSpeedingUp && isUnderSpeedLimit)
            {
                nextMotorSpeedIncrement = configMotorSpeedIncrement;
            }
            else
            {
                // decrease slowly to be faster at speeding up if spped is hit again
                nextMotorSpeedIncrement = configMotorSpeedIncrement * -2.0f;
            }

            // increase Motor Speed
            // Motorspeed should not be smaller than the current rolling speed
            if (Velocity < configMaxVelocity && BackWheelBody.angularVelocity < -motorSpeed)
            {
                motorSpeed = -BackWheelBody.angularVelocity;
            }
            motorSpeed = Mathf.Clamp(motorSpeed + nextMotorSpeedIncrement, 0, configMaxMotorSpeed);

            // motor is set to off (default)
            BackWheelJoint.useMotor = false;

            // set motor on if the bike touches the ground
            if (nextMotorSpeedIncrement > 0)
            {
                if (isUnderSpeedLimit // if the bike faster than max speed, then don't speed up
                        && -motorSpeed < BackWheelBody.angularVelocity // "<" because values are negative. If back wheel rotates faster than motorSpeed, then don't speed up
                    )
                {
                    BackWheelJoint.useMotor = true;

                    // apply Speed
                    JointMotor2D m = BackWheelJoint.motor;
                    m.motorSpeed = -motorSpeed;
                    m.maxMotorTorque = configMaxMotorTorque;
                    BackWheelJoint.motor = m; // automatically sets useMotor to true

                    // Add some extra downward force on the back wheel on steep slopes if leaning forward and speeding up.
                    // This is to compensate for the wheel not touching the ground if leaning forward and thus not creating the desired forward force.
                    // We may also like to play around with Physics2D DefaultContactOffset (not done yet), but that's difficult to predict.
                    if (BackWheelGroundTouchTrigger.IsTouching
                        && nextMotorSpeedIncrement > 0
                        && nextNormalizedInputTorque != 0
                        && bikeRotationHalf == 1
                        && Velocity < 40)
                    {
                        // add downward force to back wheel
                        var tmpDirection = this.transform.TransformVector(new Vector3(0, -1, 0)).normalized;
                        Vector2 direction = new Vector2(tmpDirection.x, tmpDirection.y);
                        var tmpPosition = BackWheel.transform.TransformPoint(Vector3.zero);
                        Vector2 position = new Vector2(tmpPosition.x, tmpPosition.y);
                        BackWheelBody.AddForceAtPosition(direction * Config.BikeBackWheelDownForceOnSlopes, position);

                        // make motor torque force stronger on slopes
                        if (SlopeInDegrees > 30 && SlopeInDegrees < 90)
                        {
                            m = BackWheelJoint.motor;
                            m.maxMotorTorque = configMaxMotorTorqueOnSlopes;
                            BackWheelJoint.motor = m; // automatically sets useMotor to true
                        }
                    }
                }
            }
        }

        protected RaycastHit2D[] tmpRotationRayCastHits = new RaycastHit2D[1];

        protected void fixedUpdateRotation()
        {
            var torque = nextNormalizedInputTorque * configTorqueForce;

            BikeBody.angularDrag = (IsTouchingGround == false && torque == 0) ? Config.BikeAngularDragInAir : Config.BikeAngularDrag;

            // roll over protection
            if (RollOverProtection && Crashed == false && torque <= 0 && IsFrontTouchingGround == false && IsBackTouchingGround == true)
            {
                int hitsFront = Physics2D.RaycastNonAlloc(FrontWheelBody.position, Vector2.down, tmpRotationRayCastHits, 3, GroundLayers);
                if (hitsFront > 0)
                {
                    var frontWheelGroundPos = tmpRotationRayCastHits[0].point;
                    var angle = Vector2.Angle(FrontWheelBody.position - BackWheelBody.position, frontWheelGroundPos - BackWheelBody.position);
                    if (angle > 30)
                    {
                        BikeBody.angularVelocity = 0;
                        if (angle > 100)
                        {
                            BikeBody.AddTorque(-5000);
                        }
                        BikeBody.AddForceAtPosition(this.transform.TransformDirection(Vector3.down) * 1000f, FrontWheelBody.position, ForceMode2D.Force);

                        // debug
                        /*
                        if (angle > 100) {
                            UtilsDebug.DrawVector(FrontWheelBody.position, this.transform.TransformDirection(Vector3.down), Color.green, 1.0f);
                        } else {
                            UtilsDebug.DrawVector(FrontWheelBody.position, this.transform.TransformDirection(Vector3.down), Color.red, 1.0f);
                        }
                        UtilsDebug.DrawVectorPos(BackWheelBody.position, FrontWheelBody.position, Color.red);
                        UtilsDebug.DrawVectorPos(BackWheelBody.position, frontWheelGroundPos, Color.red);
                        //*/
                    }
                }
            }

            // rotate
            if (Mathf.Abs(torque) > 0)
            {
                if (Mathf.Abs(torque) > 0.001f)
                {
                    BikeBody.AddTorque(torque, ForceMode2D.Force);
                }

                // extra rotation force in free fall, small effect but it helps to rotate constantly in free fall
                if (IsTouchingGround == false && Crashed == false)
                {
                    if (Config.BikeMoveRotation > 0)
                    {
                        float moveRotation = Config.BikeMoveRotation;
                        moveRotation *= torque > 0 ? 1 : -1;
                        BikeBody.MoveRotation(BikeBody.rotation + (moveRotation * Time.fixedDeltaTime));
                    }
                }
            }
        }

        public void ClearAverageMemories()
        {
            velocities.Clear();
            forwardVectors.Clear();
            tmpAverageDeltaCountdown = 0;
        }
    }
}