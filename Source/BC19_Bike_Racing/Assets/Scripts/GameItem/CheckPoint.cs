using Kamgam.BikeAndCharacter25D.Helpers;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CheckPoint : MonoBehaviour, ITrigger2DReceiver
{
    public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
    {
        LevelController.Instance.SavePoint();
        gameObject.SetActive(false);
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
