using UnityEngine;

namespace Kamgam.BridgeBuilder25D
{
    public interface IBridge25DEdgePart
    {
        GameObject GetGameObject();
        void Clear();
        void UpdateEdge(Bridge25D bridge, bool isFirst, float offset, float depth, Vector3 Scale);
        void OnBreak(Vector3 position, Rigidbody2D rigidbody);
    }
}