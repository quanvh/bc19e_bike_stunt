using Kamgam.BridgeBuilder25D.Helpers;
using UnityEngine;

namespace Kamgam.BridgeBuilder25D
{
    public class Bridge25DEdgePartWithRope : MonoBehaviour, IBridge25DEdgePart
    {
        [Tooltip("Specifies how far inward (z-axis) the rope will connect to the part.")]
        public float RopeDepthOffset = 0.1f;

        [Tooltip("Distance in counted parts from the edge part inwards (x-axis).")]
        [Range(1, 15)]
        public int RopeDistance = 7;

        [Header("References")]
        public GameObject PillarFront;
        public GameObject PillarBack;
        public GameObject RopeFront;
        public GameObject RopeBack;

        public GameObject GetGameObject()
        {
            if (this == null)
                return null;

            return gameObject;
        }

        public void Clear()
        {
            RopeFront = null;
            RopeBack = null;
        }

        public void UpdateEdge(Bridge25D bridge, bool isFirst, float offset, float depth, Vector3 scale)
        {
            if (GetGameObject() == null)
                return;

            if (bridge == null)
                return;

            if (bridge.Parts != null && bridge.Parts.Count > 0 && bridge.Spline != null && bridge.Spline.PointCount > 0)
            {
                // update position and rope infos
                Transform ropeConnection = null;
                int ropeDistance = Mathf.Min(RopeDistance, (int)(bridge.Parts.Count * 0.5f)); // ensure rope do not intersect
                for (int i = 0; i < bridge.Parts.Count; i++)
                {
                    // positions
                    if (isFirst && i == 0)
                        transform.position = bridge.Spline.GetPointAt(-0.01f) + new Vector3(-offset, 0f, 0f);

                    if (!isFirst && i == bridge.Parts.Count-1)
                        transform.position = bridge.Spline.GetPointAt(1.01f) + new Vector3(offset, 0f, 0f);

                    // rope end transforms
                    if (isFirst && i == ropeDistance)
                        ropeConnection = bridge.Parts[i].transform;

                    if (!isFirst && i == bridge.Parts.Count - ropeDistance)
                        ropeConnection = bridge.Parts[i].transform;
                }

                // update rope end pos, rot, scale
                if (ropeConnection != null && RopeFront != null && RopeBack != null)
                {
                    RopeFront.transform.SetLocalPositionZ(-(depth - RopeDepthOffset) * 0.5f * scale.z);
                    RopeBack.transform.SetLocalPositionZ((depth - RopeDepthOffset) * 0.5f * scale.z);

                    Vector2 ropeVector = ropeConnection.position - RopeFront.transform.position;
                    RopeFront.transform.SetLocalScaleZ(ropeVector.magnitude);
                    RopeFront.transform.LookAt(new Vector3(ropeConnection.position.x, ropeConnection.position.y, -(depth - RopeDepthOffset) * 0.5f * scale.z));
                    RopeBack.transform.SetLocalScaleZ(ropeVector.magnitude);
                    RopeBack.transform.LookAt(new Vector3(ropeConnection.position.x, ropeConnection.position.y, (depth - RopeDepthOffset) * 0.5f * scale.z));
                }

                if (PillarFront != null && PillarBack != null)
                {
                    PillarFront.transform.SetLocalPositionZ(-(depth - RopeDepthOffset) * 0.5f * scale.z);
                    PillarBack.transform.SetLocalPositionZ((depth - RopeDepthOffset) * 0.5f * scale.z);
                }
            }
        }

        public void OnBreak(Vector3 position, Rigidbody2D rigidbody)
        {
            RopeFront.gameObject.SetActive(false);
            RopeBack.gameObject.SetActive(false);
        }
    }
}