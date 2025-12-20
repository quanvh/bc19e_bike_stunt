using Kamgam.BikeAndCharacter25D.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Character : MonoBehaviour
    {
        public CharacterConfig Config;

        protected class RelativeTransformMemory
        {
            public Vector3 RelativePosition;
            public float RelativeRotation;

            public RelativeTransformMemory(GameObject go, GameObject relativeTo)
            {
                RelativePosition = go.transform.position - relativeTo.transform.position;
                RelativeRotation = go.transform.rotation.eulerAngles.z - relativeTo.transform.rotation.eulerAngles.z;
            }

            public void Restore(GameObject go, GameObject relativeTo)
            {
                var rotation = relativeTo.transform.rotation;
                var angles = rotation.eulerAngles;
                angles.z += RelativeRotation;
                rotation.eulerAngles = angles;
                go.transform.rotation = rotation;
                go.transform.position = relativeTo.transform.position + RelativePosition;
            }
        }

        public LayerMask DistanceToGroundLayers = ~0; // default to everything

        public GameObject Head;
        public GameObject Torso;
        public GameObject UpperArm;
        public GameObject LowerArm;
        public GameObject UpperLeg;
        public GameObject LowerLeg;

        public GameObject Helmet;
        public Transform HelmetSpawnPosition;

        public HingeJoint2D HeadJoint;
        public HingeJoint2D UpperArmJoint;
        public DistanceJoint2D UpperArmConnectionJointElbow;
        public HingeJoint2D LowerArmJoint;
        public HingeJoint2D UpperLegJoint;
        public DistanceJoint2D UpperLegtConnectionJointKnee;
        public HingeJoint2D LowerLegJoint;

        public Rigidbody2D HeadBody;
        public Rigidbody2D TorsoBody;
        public Rigidbody2D UpperArmBody;
        public Rigidbody2D LowerArmBody;
        public Rigidbody2D UpperLegBody;
        public Rigidbody2D LowerLegBody;

        public HingeJoint2D ArmConnectionJoint;
        public HingeJoint2D LegConnectionJoint;
        public DistanceJoint2D TorsoConnectionJointHip;
        public DistanceJoint2D TorsoConnectionJointChest;
        public SpringJoint2D SpringConnectionJointChest;
        public SpringJoint2D[] SpringConnectionJointsHead;

        public Transform CapeSpawnLocation;

        Dictionary<GameObject, RelativeTransformMemory> memoryOfBodyparts;

        public bool IsConnectedToBike
        {
            get { return ArmConnectionJoint.enabled; }
        }

        public enum Tilt { None = 0, Left = 1, Right = 2 }
        protected Tilt currentTilt = Tilt.None;

        // tmp
        protected float HeadBodyMass = -1;
        protected float TorsoBodyMass = -1;
        protected float UpperArmBodyMass = -1;
        protected float LowerArmBodyMass = -1;
        protected float UpperLegBodyMass = -1;
        protected float LowerLegBodyMass = -1;
        protected float TorsoLinearDrag = -1;
        protected float TorsoAngularDrag = -1;

        protected bool scaled = false;
        protected bool paused = false;

        protected Vector2AverageQueue positionsOnGround;
        protected float tmpAverageDeltaCountdown;

        [HideInInspector]
        public float Velocity
        {
            get
            {
                return TorsoBody.velocity.magnitude;
            }
        }

        [HideInInspector]
        public Vector2 VelocityVector
        {
            get
            {
                return TorsoBody.velocity;
            }
        }

        protected void loadConfig()
        {
            if (IsConnectedToBike == true)
            {
                HeadBody.mass = HeadBodyMass = Config.CharacterHeadBodyMass;
                TorsoBody.mass = TorsoBodyMass = Config.CharacterTorsoBodyMass;
                UpperArmBody.mass = UpperArmBodyMass = Config.CharacterUpperArmBodyMass;
                LowerArmBody.mass = LowerArmBodyMass = Config.CharacterLowerArmBodyMass;
                UpperLegBody.mass = UpperLegBodyMass = Config.CharacterUpperLegBodyMass;
                LowerLegBody.mass = LowerLegBodyMass = Config.CharacterLowerLegBodyMass;
            }
        }

        public void Awake()
        {
            loadConfig();

            // apply mass
            HeadBodyMass = HeadBody.mass;
            TorsoBodyMass = TorsoBody.mass;
            UpperArmBodyMass = UpperArmBody.mass;
            LowerArmBodyMass = LowerArmBody.mass;
            UpperLegBodyMass = UpperLegBody.mass;
            LowerLegBodyMass = LowerLegBody.mass;

            // apply torso drag
            TorsoLinearDrag = TorsoBody.drag;
            TorsoAngularDrag = TorsoBody.angularDrag;
        }

        public void SetPaused(bool paused)
        {
            this.paused = paused;
        }

        public void Update()
        {
            if (this.paused)
                return;

#if UNITY_EDITOR
            loadConfig();
#endif
        }

        protected RaycastHit2D[] tmpRayCastHitGroundResults = new RaycastHit2D[1];

        /// <summary>
        /// May return null if no ground position has been found.
        /// </summary>
        /// <returns></returns>
        public Vector2? GetPositionOnGround()
        {
            if (this.gameObject.activeInHierarchy)
            {
                int numResults = Physics2D.RaycastNonAlloc(
                    Torso.transform.TransformPoint(0, 0, 0),
                    Vector2.down,
                    tmpRayCastHitGroundResults,
                    200,
                    DistanceToGroundLayers.value
                );
                if (numResults > 0)
                {
                    return new Vector2(Torso.transform.position.x, tmpRayCastHitGroundResults[0].point.y);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public Vector2 GetLastKnownPositionOnGround()
        {
            if (positionsOnGround == null || positionsOnGround.Count == 0)
            {
                return this.Torso.transform.position + Vector3.down;
            }
            else
            {
                return positionsOnGround.Last();
            }
        }

        public float GetLastKnownDistanceToGround()
        {
            if (positionsOnGround == null || positionsOnGround.Count == 0)
            {
                return 1f;
            }
            else
            {
                return Vector2.Distance(positionsOnGround.Last(), this.Torso.transform.position);
            }
        }

        /// <summary>
        /// Gives the average of the last N seconds for Y. X is always the last known position.
        /// </summary>
        /// <param name="timeframeInSec"></param>
        /// <returns></returns>
        public Vector2? GetPositionOnGroundAverage(float timeframeInSec = 0.5f)
        {
            int numOfEntriesToSample = Mathf.RoundToInt(timeframeInSec / Bike.AVERAGE_DELTA_TIME);
            if (numOfEntriesToSample > 1 && positionsOnGround.Count > 0)
            {
                if (positionsOnGround.Count >= 0)
                {
                    positionsOnGround.UpdateAverage(numOfEntriesToSample);
                    var result = new Vector2(
                        // Do not average x, because position on ground x should always be where the character is now.
                        positionsOnGround.LastX(),
                        positionsOnGround.AverageY
                    );
                    // If the average y is above then clamp it to the char position.
                    // We do this by checking the dot product. If it is < 0 then the two vecors are facing different directions.
                    var currentPos = (Vector2)Torso.transform.TransformPoint(0, 0, 0);
                    if (Vector2.Dot(result - currentPos, positionsOnGround.Last() - currentPos) < 0)
                    {
                        result.y = currentPos.y;
                    }
                    return result;
                }
                else
                {
                    return GetPositionOnGround();
                }
            }
            else
            {
                return GetPositionOnGround();
            }
        }

        public Vector3 GetPositionInWorld()
        {
            return Torso.transform.TransformPoint(new Vector3(0, 0, 0));
        }

        public void DisconnectFromBike(Bike bike, bool addImpulse = true)
        {
            // save positions and rotations
            memoryOfBodyparts = new Dictionary<GameObject, RelativeTransformMemory>();
            memoryOfBodyparts.Add(Head, new RelativeTransformMemory(Head, bike.gameObject));
            memoryOfBodyparts.Add(Torso, new RelativeTransformMemory(Torso, bike.gameObject));
            memoryOfBodyparts.Add(UpperArm, new RelativeTransformMemory(UpperArm, bike.gameObject));
            memoryOfBodyparts.Add(LowerArm, new RelativeTransformMemory(LowerArm, bike.gameObject));
            memoryOfBodyparts.Add(UpperLeg, new RelativeTransformMemory(UpperLeg, bike.gameObject));
            memoryOfBodyparts.Add(LowerLeg, new RelativeTransformMemory(LowerLeg, bike.gameObject));

            ArmConnectionJoint.enabled = false;
            UpperLegtConnectionJointKnee.enabled = false;
            LegConnectionJoint.enabled = false;
            UpperArmConnectionJointElbow.enabled = false;
            TorsoConnectionJointHip.enabled = false;
            TorsoConnectionJointChest.enabled = false;
            SpringConnectionJointChest.enabled = false;
            SpringConnectionJointsHead.ToList().ForEach(joint => joint.enabled = false);

            // set mass
            HeadBody.mass = 4;
            TorsoBody.mass = 50;
            UpperArmBody.mass = 5;
            LowerArmBody.mass = 5;
            UpperLegBody.mass = 5;
            LowerLegBody.mass = 5;

            // enable colliders
            Head.GetComponent<CircleCollider2D>().enabled = true;
            TorsoBody.GetComponent<BoxCollider2D>().enabled = true;
            UpperArm.GetComponent<BoxCollider2D>().enabled = true;
            LowerArm.GetComponent<BoxCollider2D>().enabled = true;
            UpperLeg.GetComponent<BoxCollider2D>().enabled = true;
            LowerLeg.GetComponent<BoxCollider2D>().enabled = true;

            // change drag
            TorsoBody.drag = 0.1f;
            TorsoBody.angularDrag = 0.3f;

            // add a small upwards for on separation.
            if (addImpulse)
            {
                AddImpulse(new Vector2(0, 200));
            }
        }

        public void AddImpulse(Vector2 impulse)
        {
            TorsoBody.AddForce(impulse, ForceMode2D.Impulse);
        }

        public void ConnectToBike(Bike bike)
        {
            // save positions and rotations
            if (memoryOfBodyparts != null)
            {
                memoryOfBodyparts[Head].Restore(Head, bike.gameObject);
                memoryOfBodyparts[Torso].Restore(Torso, bike.gameObject);
                memoryOfBodyparts[UpperArm].Restore(UpperArm, bike.gameObject);
                memoryOfBodyparts[LowerArm].Restore(LowerArm, bike.gameObject);
                memoryOfBodyparts[UpperLeg].Restore(UpperLeg, bike.gameObject);
                memoryOfBodyparts[LowerLeg].Restore(LowerLeg, bike.gameObject);
                memoryOfBodyparts = null;
            }

            ArmConnectionJoint.connectedBody = bike.BikeBody;
            UpperLegtConnectionJointKnee.connectedBody = bike.BikeBody;
            LegConnectionJoint.connectedBody = bike.BikeBody;
            UpperArmConnectionJointElbow.connectedBody = bike.BikeBody;
            TorsoConnectionJointHip.connectedBody = bike.BikeBody;
            TorsoConnectionJointChest.connectedBody = bike.BikeBody;
            SpringConnectionJointChest.connectedBody = bike.BikeBody;
            SpringConnectionJointsHead.ToList().ForEach(joint => joint.connectedBody = bike.BikeBody);

            ArmConnectionJoint.enabled = true;
            UpperLegtConnectionJointKnee.enabled = true;
            LegConnectionJoint.enabled = true;
            UpperArmConnectionJointElbow.enabled = true;
            TorsoConnectionJointHip.enabled = true;
            TorsoConnectionJointChest.enabled = true;
            SpringConnectionJointChest.enabled = true;
            SpringConnectionJointsHead.ToList().ForEach(joint => joint.enabled = true);

            // reset mass
            HeadBody.mass = HeadBodyMass;
            TorsoBody.mass = TorsoBodyMass;
            UpperArmBody.mass = UpperArmBodyMass;
            LowerArmBody.mass = LowerArmBodyMass;
            UpperLegBody.mass = UpperLegBodyMass;
            LowerLegBody.mass = LowerLegBodyMass;

            // reset drag
            TorsoBody.drag = TorsoLinearDrag;
            TorsoBody.angularDrag = TorsoAngularDrag;

            // disable colliders
            Head.GetComponent<CircleCollider2D>().enabled = false;
            TorsoBody.GetComponent<BoxCollider2D>().enabled = false;
            UpperArm.GetComponent<BoxCollider2D>().enabled = false;
            LowerArm.GetComponent<BoxCollider2D>().enabled = false;
            UpperLeg.GetComponent<BoxCollider2D>().enabled = false;
            LowerLeg.GetComponent<BoxCollider2D>().enabled = false;

            Physics2D.SyncTransforms();
        }

        public void TiltBackward()
        {
            rotateCounterClockWise();
        }

        protected float tiltAnimDivisor = 5; // inverse speed

        protected void rotateCounterClockWise()
        {
            var anchor = SpringConnectionJointChest.connectedAnchor;
            anchor.x += (-1.10f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.5f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointChest.connectedAnchor = anchor;

            TorsoConnectionJointChest.distance += (0.5f - TorsoConnectionJointChest.distance) / tiltAnimDivisor;
            TorsoConnectionJointHip.distance += (0.1f - TorsoConnectionJointHip.distance) / tiltAnimDivisor;

            anchor = SpringConnectionJointsHead[1].connectedAnchor;
            anchor.x += (-0.5f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.5f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointsHead[1].connectedAnchor = anchor;

            this.currentTilt = Tilt.Left;
        }

        public void TiltForward()
        {
            rotateClockWise();
        }

        public void rotateClockWise()
        {
            var anchor = SpringConnectionJointChest.connectedAnchor;
            anchor.x += (1.0f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.6f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointChest.connectedAnchor = anchor;

            TorsoConnectionJointChest.distance += (0.5f - TorsoConnectionJointChest.distance) / tiltAnimDivisor;
            TorsoConnectionJointHip.distance += (0.25f - TorsoConnectionJointHip.distance) / tiltAnimDivisor;
            UpperArmConnectionJointElbow.distance += (0.45f - UpperArmConnectionJointElbow.distance) / tiltAnimDivisor;

            anchor = SpringConnectionJointsHead[1].connectedAnchor;
            anchor.x += (0.406f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.426f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointsHead[1].connectedAnchor = anchor;

            this.currentTilt = Tilt.Right;
        }

        public void StopTilt()
        {
            UpperArmConnectionJointElbow.distance += (0.16f - UpperArmConnectionJointElbow.distance) / tiltAnimDivisor;
            var anchor = SpringConnectionJointChest.connectedAnchor;
            anchor.x += (0.15f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.62f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointChest.connectedAnchor = anchor;
            TorsoConnectionJointChest.distance += (0.054f - TorsoConnectionJointChest.distance) / tiltAnimDivisor;
            TorsoConnectionJointHip.distance += (0.06f - TorsoConnectionJointHip.distance) / tiltAnimDivisor;

            anchor = SpringConnectionJointsHead[1].connectedAnchor;
            anchor.x += (0.406f - anchor.x) / tiltAnimDivisor;
            anchor.y += (1.426f - anchor.y) / tiltAnimDivisor;
            SpringConnectionJointsHead[1].connectedAnchor = anchor;

            this.currentTilt = Tilt.None;
        }

        public void StopMovement()
        {
            HeadBody.angularVelocity = 0;
            HeadBody.velocity = Vector2.zero;

            TorsoBody.angularVelocity = 0;
            TorsoBody.velocity = Vector2.zero;

            UpperArmBody.angularVelocity = 0;
            UpperArmBody.velocity = Vector2.zero;

            LowerArmBody.angularVelocity = 0;
            LowerArmBody.velocity = Vector2.zero;

            UpperLegBody.angularVelocity = 0;
            UpperLegBody.velocity = Vector2.zero;

            LowerLegBody.angularVelocity = 0;
            LowerLegBody.velocity = Vector2.zero;
        }

        public Vector3 GetWorldPosition()
        {
            return this.Torso.transform.position;
        }

        public void ClearAverageMemories()
        {
            positionsOnGround.Clear();
            tmpAverageDeltaCountdown = 0;
        }
    }
}
