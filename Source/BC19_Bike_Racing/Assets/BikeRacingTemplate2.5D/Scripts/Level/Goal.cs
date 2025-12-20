using UnityEngine;
using Kamgam.BikeAndCharacter25D.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The goal. If the player reaches it then the race will end.
    /// </summary>
    public class Goal : MonoBehaviour, ITrigger2DReceiver
    {
        public System.Action OnGoalReached;

        public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
        {
            OnGoalReached?.Invoke();
        }

        public void OnCustomTriggerExit2D(Trigger2D trigger, Collider2D other) { }
        public void OnCustomTriggerStay2D(Trigger2D trigger, Collider2D other) { }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, new Vector3(0.5f, 3f, 1f));
            }
            else
            {
                Gizmos.DrawCube(transform.position + Vector3.up * 1.5f, new Vector3(0.5f, 3f, 1f));

                // While we are at it, constrain the z pos to 0.
                var pos = transform.position;
                pos.z = 0;
                transform.position = pos;
            }
        }
#endif

    }
}
