#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The only part used from this is the .position of the transform.<br />
    /// The rest is just a dummy do visualize the spawn position in the editor
    /// </summary>
    public class SpawnPosition : MonoBehaviour
    {

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new  Color(0f, 0f, 1f, 0.3f);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 1f);
            }
            else
            {
                Gizmos.DrawSphere(transform.position + Vector3.up, 1f);

                // While we are at it, constrain the z pos to 0.
                var pos = transform.position;
                pos.z = 0;
                transform.position = pos;
            }
        }
#endif

    }
}
