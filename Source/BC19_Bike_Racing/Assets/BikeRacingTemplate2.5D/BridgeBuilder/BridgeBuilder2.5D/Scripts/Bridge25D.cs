using Kamgam.BridgeBuilder25D.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kamgam.BridgeBuilder25D
{
    [RequireComponent(typeof(BezierSpline))]
    public class Bridge25D : MonoBehaviour, ITrigger2DReceiver
    {
        /// <summary>
        /// The first parameter is the bridge object. The second parameter is the break location.
        /// </summary>
        public Action<Bridge25D, Vector3> OnBreak;

        [Header("Creation")]

        [Tooltip("Should the bridge be recreated automatically after a parameter has been changed in the Editor (has no effect during runtime or in play mode)?")]
        public bool AutoCreate = false;

        public bool RecreateOnReset = true;

        [Tooltip("Set the RidigdBodies to awake at start? If set to false then you will have to activate them by calling SetPhysicsActive(true) or by adding and configuring a ProximityTrigger.")]
        public bool StartAwake = true;

        [Tooltip("Use the DamageReceiver component of the parts to damage bridge parts. Parts receiving more than 'damageTilBreak' damage will break.")]
        public float damageTilBreak = 10;

        [Header("Prefabs & Presets")]
        public GameObject BridgeEdgePartPrefab;
        public GameObject BridgePartPrefab;
        public PhysicsMaterial2D PhysicsMaterial;

        [Tooltip("Set this value to the total with (x-axis) of your 'BridgePart' prefab.")]
        public float BridgePartWidthInPrefab = 0.5f;

        [Tooltip("Set this value to the total depth (z-axis) of your 'BridgePart' prefab.")]
        public float BridgePartDepthInPrefab = 2f;

        [Header("Trigger")]
        public Trigger2D ProximityTrigger;
        public float ProximityTriggerMargin = 30f;

        // Hidden memory of where the physics anchors originated (used to fix them if the object has been moved).
        [HideInInspector]
        public bool AnchorRefPosSet = false;
        [HideInInspector]
        public Vector3 AnchorRefPos;

        [Header("Parts")]
        [Tooltip("List of created elements (bridge parts)")]
        public List<Rigidbody2D> Parts;

        [Tooltip("The bridge parts are moved to this layer if broken.")]
        public int PartLayerIfBroken;

        [Header("Visuals (enable 'AutoCreate' and play around)")]
        [Range(0, 0.7f)]
        public float WidthVariation = 0f;

        [Tooltip("Scale the part prefab mesh to modify the visual size.")]
        public Vector3 Scale = Vector3.one;

        [Tooltip("Each part will be randomly rotated. Use this to make the bridge looks less uniform.")]
        [Range(0, 10f)]
        public float RandomRotationX = 0f;

        [Tooltip("Each part will be randomly rotated. Use this to make the bridge looks less uniform.")]
        [Range(0, 10f)]
        public float RandomRotationY = 0f;

        [Tooltip("If materials are added to this list then they will be applied to the middle parts of the bridge at random.")]
        public Material[] RandomMaterials;

        [Header("Physics (per part)")]
        public float Mass = 70;
        public float LinearDrag = 3;
        public float GravityScale = 1.0f;
        public float SpringDampingRatio = 0.1f;
        public float SpringFrequency = 0.5f;
        [Tooltip("Set to 0 to add no spring joints. Only necessary for stability of bridges with many (>10) parts"), Range(0, 5)]
        public int AddSpringJointsEvery = 1;

        [Header("Physics (if broken)")]
        public float MassBroken = 70;
        public float LinearDragBroken = 0.1f;


        [Header("Edge Parts")]
        [Tooltip("Edge parts are move outwards along the x-axis by this offet.")]
        public float BridgeEdgePartOffset = 0.1f;

        private const string EdgePart1Name = "BridgeEdgePart1";
        protected IBridge25DEdgePart edgePart1;
        public IBridge25DEdgePart EdgePart1
        {
            get
            {
                if (edgePart1 == null || edgePart1.GetGameObject() == null)
                {
                    var obj = transform.Find(EdgePart1Name);
                    if (obj != null)
                        edgePart1 = obj.GetComponent<IBridge25DEdgePart>();
                }
                return edgePart1;
            }
        }

        private const string EdgePart2Name = "BridgeEdgePart2";
        protected IBridge25DEdgePart edgePart2;
        public IBridge25DEdgePart EdgePart2
        {
            get
            {
                if (edgePart2 == null || edgePart2.GetGameObject() == null)
                {
                    var obj = transform.Find(EdgePart2Name);
                    if (obj != null)
                        edgePart2 = obj.GetComponent<IBridge25DEdgePart>();
                }
                return edgePart2;
            }
        }

        [Header("Events")]
        public UnityEvent OnBreakEvent;

        protected bool broken;

        // checkpoint memory
        protected bool tmpCheckpointBroken;
        protected Vector3 tmpCheckpointBrokenAt;

        // damage
        protected DamageReceiver damagedPart;
        protected float damageReceived;

        [SerializeField]
        protected BezierSpline spline;
        public BezierSpline Spline
        {
            get
            {
                if (spline == null)
                {
                    spline = this.gameObject.GetComponent<BezierSpline>();
                    if (spline == null)
                    {
                        spline = this.gameObject.AddComponent<BezierSpline>();
                        spline.Resolution = 300;
                        spline.Mode2D = true;
                        if (spline.transform.Find("Point 0") == null)
                        {
                            var p0 = new GameObject().AddComponent<BezierPoint>();
                            p0.name = "Point 0";
                            p0.transform.parent = spline.transform;
                            p0.handleType = BezierPoint.HandleType.Broken;
                            p0.Position = this.transform.position;
                            p0.Handle1Pos = p0.Position + Vector3.up;
                            p0.Handle2Pos = p0.Position + Vector3.right * 4;

                            var p1 = new GameObject().AddComponent<BezierPoint>();
                            p1.name = "Point 1";
                            p1.transform.parent = spline.transform;
                            p1.handleType = BezierPoint.HandleType.Broken;
                            p1.Position = this.transform.position + (Vector3.right * 10);
                            p1.Handle2Pos = p1.Position + Vector3.up;
                            p1.Handle1Pos = p1.Position + Vector3.left * 4;
                        }
                    }
                }
                return spline;
            }
        }

        public void RefreshElementsList()
        {
            if (Parts == null)
            {
                Parts = new List<Rigidbody2D>();
            }
            Parts.RemoveNullValues();

            if (Parts.Count == 0)
            {
                var elements = this.GetComponentsInChildren<Rigidbody2D>(true);
                foreach (var element in elements)
                {
                    if (element.name.ToLower().Contains("part"))
                    {
                        Parts.Add(element);
                    }
                }
            }

            updateEdges();

            foreach (var element in Parts)
            {
                var damageReceiver = element.transform.GetComponent<DamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.OnDamageReceived -= onDamageReceived;
                    damageReceiver.OnDamageReceived += onDamageReceived;
                }
            }
        }

        public void SetPhysicsActive(bool active)
        {
            foreach (var element in Parts)
            {
                if (active)
                {
                    element.simulated = true;
                    element.WakeUp();
                }
                else
                {
                    element.simulated = false;
                    element.Sleep();
                }
            }
        }

        public void Clear()
        {
            if (EdgePart1 != null)
            {
                EdgePart1.GetGameObject().name = "(Destroyed)";
                Utils.SmartDestroy(EdgePart1.GetGameObject());
                EdgePart1.Clear();
                edgePart1 = null;
            }

            if (EdgePart2 != null)
            {
                EdgePart2.GetGameObject().name = "(Destroyed)";
                Utils.SmartDestroy(EdgePart2.GetGameObject());
                EdgePart2.Clear();
                edgePart2 = null;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name.Contains("Part"))
                {
                    Utils.SmartDestroy(child.gameObject);
                }
            }
            Parts.Clear();

            damagedPart = null;
            damageReceived = 0;
        }

        protected Vector3 getBridgeCenter()
        {
            return Spline.GetPointAt(-0.1f) + (Spline.GetPointAt(1.1f) - Spline.GetPointAt(-0.1f)) * 0.5f;
        }

        public void Recreate()
        {
            Create(Spline, forceRebuild: true);
        }

        private float plankWidth => BridgePartWidthInPrefab;

        public void Create(GameObject bridgePartPrefab, GameObject bridgeEdgePartPrefab, PhysicsMaterial2D physicsMaterial, BezierSpline curve, bool forceRebuild = false)
        {
            if (bridgePartPrefab != null)
                BridgePartPrefab = bridgePartPrefab;

            if (bridgeEdgePartPrefab != null)
                BridgeEdgePartPrefab = bridgeEdgePartPrefab;

            PhysicsMaterial = physicsMaterial;
            Create(curve, forceRebuild);
        }

        public void Create(BezierSpline curve, bool forceRebuild = false)
        {

            if (BridgePartPrefab == null || BridgeEdgePartPrefab == null)
            {
#if UNITY_EDITOR
                if (BridgeBuilder25DSettings.GetOrCreateSettings().ShowLogs)
                    Debug.LogError("2.5D Bridge has no Prefabs assigned. Aborting creation! Please assign Prefabs to the BridgeElementPrefab slots first.");
#endif
                return;
            }

            AnchorRefPos = this.transform.position;
            AnchorRefPosSet = true;

            RefreshElementsList();

            if (forceRebuild)
            {
                Clear();
            }

            UpdateProximityTrigger();

            Vector2 plankVector; // the smallest vector I know ^^
            var segments = curve.GetSegmentPoints(plankWidth * Scale.x, 300, WidthVariation);
            int partsToCreate = segments.Count - 1;

            // add planks if needed
            for (int i = 0; i < Mathf.Max(partsToCreate, Parts.Count); i++)
            {
                if (i < partsToCreate && i >= Parts.Count)
                {
                    if (i >= Parts.Count)
                    {
                        // add new
                        var rb = Utils.SmartInstantiatePrefab<Rigidbody2D>(BridgePartPrefab, this.transform, true);
                        rb.sleepMode = StartAwake ? RigidbodySleepMode2D.StartAwake : RigidbodySleepMode2D.StartAsleep;
                        rb.gameObject.name = "BridgePart";
                        if (PhysicsMaterial != null && rb.sharedMaterial == null)
                            rb.sharedMaterial = PhysicsMaterial;
                        Parts.Add(rb);

                        var damageReceiver = rb.GetComponent<DamageReceiver>();
                        damageReceiver.OnDamageReceived -= onDamageReceived;
                        damageReceiver.OnDamageReceived += onDamageReceived;
                    }
                }
            }
            // remove planks if needed
            for (int i = Mathf.Max(partsToCreate, Parts.Count) - 1; i >= 0; i--)
            {
                if (i >= partsToCreate && i < Parts.Count)
                {
                    Utils.SmartDestroy(Parts[i].gameObject);
                    Parts.Remove(Parts[i]);
                }
            }

            var rnd = new System.Random();
            for (int i = 0; i < partsToCreate; i++)
            {
                var rb = Parts[i];
                rb.transform.position = segments[i];
                plankVector = segments[i + 1] - segments[i];
                rb.transform.SetLocalEulerAngleZ(Vector2.SignedAngle(Vector2.right, plankVector));
                rb.transform.SetLocalScaleX((plankVector.magnitude) / plankWidth);
                rb.transform.SetLocalScaleY(Scale.y);
                rb.transform.SetLocalScaleZ(Scale.z);
                rb.transform.GetChild(0).transform.localRotation = Quaternion.Euler(
                            UnityEngine.Random.Range(-RandomRotationX, RandomRotationX),
                            UnityEngine.Random.Range(-RandomRotationY, RandomRotationY),
                            0);
                if (RandomMaterials != null && RandomMaterials.Length > 0)
                {
                    var mat = Utils.PickRandomFrom(RandomMaterials, rnd);
                    rb.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
                rb.gravityScale = GravityScale;

                // remove old joints
                var joints = rb.gameObject.GetComponents<HingeJoint2D>();
                Utils.SmartDestroy(joints);

                // springs
                var springJoints = rb.gameObject.GetComponents<SpringJoint2D>();
                Utils.SmartDestroy(springJoints);
                if (AddSpringJointsEvery > 0)
                {
                    if (i % AddSpringJointsEvery == 0)
                    {
                        var springJoint = rb.gameObject.AddComponent<SpringJoint2D>();
                        var springConnectedAnchor = springJoint.connectedAnchor;
                        springConnectedAnchor.x = rb.transform.position.x;
                        springConnectedAnchor.y = rb.transform.position.y;
                        springJoint.connectedAnchor = springConnectedAnchor;
                        springJoint.distance = 0f;
                        springJoint.frequency = SpringFrequency;
                        springJoint.dampingRatio = SpringDampingRatio;
                    }
                }

                // mass
                rb.mass = Mass;
                rb.drag = LinearDrag;

                // add  new ones
                var joint = rb.gameObject.AddComponent<HingeJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                var anchor = joint.anchor;
                var connectedAnchor = joint.connectedAnchor;
                if (i == 0)
                {
                    joint.connectedBody = null;
                    connectedAnchor = Spline.GetPointAt(-0.1f);
                }
                else if (i == partsToCreate - 1)
                {
                    // end joint
                    joint.enabled = true;
                    joint.connectedBody = null;
                    anchor.x = plankWidth;
                    anchor.y = 0;
                    joint.anchor = anchor;
                    connectedAnchor = rb.transform.TransformPoint(anchor);
                    joint.connectedAnchor = connectedAnchor;
                    // connection joint
                    joint = rb.gameObject.AddComponent<HingeJoint2D>();
                    joint.enabled = true;
                    joint.autoConfigureConnectedAnchor = false;
                }

                if (i > 0)
                {
                    joint.connectedBody = Parts[i - 1];
                    connectedAnchor.x = plankWidth;
                    connectedAnchor.y = 0;
                }
                joint.connectedAnchor = connectedAnchor;
            }

            // add end points
            if (EdgePart1 == null || EdgePart1.GetGameObject() == null)
            {
                var obj = Utils.SmartInstantiatePrefab(BridgeEdgePartPrefab, this.transform, true);
                obj.name = EdgePart1Name;
                if (obj.GetComponent<IBridge25DEdgePart>() == null)
                {
                    Debug.LogError("Your EdgePart Prefab does not contain a IBridge25DEdgePart component. You need to add one.");
                }
            }
            if (EdgePart2 == null || EdgePart2.GetGameObject() == null)
            {
                var obj = Utils.SmartInstantiatePrefab(BridgeEdgePartPrefab, this.transform, true);
                obj.name = EdgePart2Name;
            }
            updateEdges();

            broken = false;

#if UNITY_EDITOR
            if (forceRebuild)
            {
                if (BridgeBuilder25DSettings.GetOrCreateSettings().ShowLogs)
                {
                    Debug.Log("Created " + Parts.Count + " bridge elements.");
                }
            }
#endif
        }

        public void UpdateProximityTrigger()
        {
            if (ProximityTrigger != null)
            {
                ProximityTrigger.transform.position = getBridgeCenter();
                if (Spline != null)
                {
                    ProximityTrigger.GetComponent<CircleCollider2D>().radius = ((Spline.GetPointAt(1.1f) - Spline.GetPointAt(-0.1f)) * 0.5f).magnitude + ProximityTriggerMargin;
                }
            }
        }

        protected void onDamageReceived(DamageReceiver receiver, float damage)
        {
            if (damage > damageReceived)
            {
                damagedPart = receiver;
                damageReceived = damage;
            }

            // breaking multiple parts
            if (damage > damageTilBreak)
            {
                BreakAt(receiver.transform.position);
                broken = false;
            }
        }

        /// <summary>
        /// If the bridge is moved at runtime this should be called to update the joint anchors. <br />
        /// Notice: You should not move any physics objects with transforms.
        /// </summary>
        public void UpdateJoints()
        {
            if (AnchorRefPosSet == false)
                return;

            var delta = this.transform.position - AnchorRefPos;
            AnchorRefPos = this.transform.position;
            if (delta.sqrMagnitude > 0.01f)
            {
                foreach (var element in Parts)
                {
                    var hinge = element.gameObject.GetComponent<HingeJoint2D>();
                    if (hinge != null)
                    {
                        if (element.IsFirstOf(Parts))
                        {
                            hinge.connectedAnchor += (Vector2)delta; // element.transform.TransformPoint(hinge.anchor);
                        }
                        else if (element.IsLastOf(Parts))
                        {
                            var hinges = element.gameObject.GetComponents<HingeJoint2D>();
                            foreach (var hingeJoint in hinges)
                            {
                                if (hingeJoint.connectedAnchor.y != 0)
                                {
                                    hingeJoint.connectedAnchor += (Vector2)delta; // element.transform.TransformPoint(hingeJoint.anchor);
                                }
                            }
                        }
                    }

                    var spring = element.gameObject.GetComponent<SpringJoint2D>();
                    if (spring != null)
                    {
                        var springConnectedAnchor = spring.connectedAnchor;
                        springConnectedAnchor += (Vector2)delta;
                        //springConnectedAnchor.x = element.transform.position.x;
                        //springConnectedAnchor.y = element.transform.position.y;
                        spring.connectedAnchor = springConnectedAnchor;
                    }

                    var body = element.gameObject.GetComponent<Rigidbody2D>();
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }
            }
        }

        public void AddProximityTrigger()
        {
            RemoveProximityTrigger();

            var trigger = new GameObject("ProximityTrigger");
            var collider = trigger.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            trigger.transform.parent = transform;
            ProximityTrigger = trigger.AddComponent<Trigger2D>();
            ProximityTrigger.TriggerReceiver = this;
            ProximityTrigger.BehaveLikeAnArea = true;
            UpdateProximityTrigger();

            StartAwake = false;
        }

        public void RemoveProximityTrigger()
        {
            var obj = transform.Find("ProximityTrigger");
            if (obj != null)
            {
                Utils.SmartDestroy(obj.gameObject);
            }
            ProximityTrigger = null;

            StartAwake = true;
        }

        protected void updateEdges()
        {
            if (EdgePart1 != null)
                EdgePart1.UpdateEdge(this, true, BridgeEdgePartOffset, BridgePartDepthInPrefab, Scale);

            if (EdgePart2 != null)
                EdgePart2.UpdateEdge(this, false, BridgeEdgePartOffset, BridgePartDepthInPrefab, Scale);
        }

        public void Awake()
        {
            AutoCreate = false;
            RefreshElementsList();
        }

        public void FixedUpdate()
        {
            if (Vector3.SqrMagnitude(AnchorRefPos - transform.position) > 0.01f)
            {
                UpdateJoints();
            }
        }

        public void LateUpdate()
        {
            if (Parts != null && Parts.Count > 0 && Parts[0] != null && Parts[0].IsAwake())
            {
                updateEdges();
            }

            if (damageReceived > 0)
            {
                if (damageReceived > damageTilBreak)
                {
                    BreakAt(damagedPart.transform.position);
                }
                damageReceived = 0;
                damagedPart = null;
            }
        }

        /// <summary>
        /// Breaks at the elements which is closest to the given position.
        /// Breaks only if not yet broken.
        /// </summary>
        /// <param name="t">normalized position from start to finish (0.0 to 1.0)</param>
        public void BreakAt(float t)
        {
            if (broken == false)
            {
                BreakAt(Vector3.Lerp(Parts[0].transform.position, Parts[Parts.Count - 1].transform.position, t));
            }
        }

        /// <summary>
        /// Break at the bridge part which is closes to the given position.
        /// Position is in global space.
        /// </summary>
        /// <param name="position"></param>
        public void BreakAt(Vector3 position)
        {
            if (Parts == null || Parts.Count == 0)
                return;

            Rigidbody2D closestElement = Parts[0];
            float sqrMinDistance = Mathf.Infinity;
            float sqrDistance;
            foreach (var ele in Parts)
            {
                var springJoint = ele.GetComponent<SpringJoint2D>();
                if (springJoint != null)
                {
                    springJoint.enabled = false;
                }
                sqrDistance = Vector3.SqrMagnitude(position - ele.transform.position);
                if (sqrDistance < sqrMinDistance)
                {
                    closestElement = ele;
                    sqrMinDistance = sqrDistance;
                }

                // mass
                ele.mass = MassBroken;
                ele.drag = LinearDragBroken;

                // layer
                ele.gameObject.layer = PartLayerIfBroken;
            }
            closestElement.gameObject.GetComponent<HingeJoint2D>().enabled = false;
            broken = true;
            tmpCheckpointBrokenAt = position;

            // disable ropes if broken
            if (EdgePart1 != null)
                EdgePart1.OnBreak(position, closestElement);

            if (EdgePart2 != null)
                EdgePart2.OnBreak(position, closestElement);

            OnBreak?.Invoke(this, position);
            OnBreakEvent?.Invoke();
        }

        public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
        {
            foreach (var ele in Parts)
            {
                ele.WakeUp();
            }
        }

        public void OnCustomTriggerStay2D(Trigger2D trigger, Collider2D other)
        {
        }

        public void OnCustomTriggerExit2D(Trigger2D trigger, Collider2D other)
        {
            foreach (var ele in Parts)
            {
                ele.Sleep();
            }
        }

        /// <summary>
        /// Use this to memorize the broken state of the bridge.
        /// </summary>
        public void OnCheckpointReached()
        {
            tmpCheckpointBroken = broken;
        }

        /// <summary>
        /// If restartFromBeginning is FALSE then the broken state will be restored.
        /// </summary>
        /// <param name="restartFromBeginning"></param>
        public void ResetState(bool restartFromBeginning)
        {
            if (RecreateOnReset)
            {
                if (restartFromBeginning)
                {
                    tmpCheckpointBroken = false;
                    tmpCheckpointBrokenAt = Vector3.zero;
                }

                Create(Spline, forceRebuild: true);

                // break
                if (tmpCheckpointBroken)
                {
                    BreakAt(tmpCheckpointBrokenAt);
                }
            }
        }
    }
}
