using Kamgam.BikeAndCharacter25D;
using Kamgam.BikeAndCharacter25D.Helpers;
using UnityEditor;
using UnityEngine;

public class CharacterCollider : MonoBehaviour, ITrigger2DReceiver
{
    BikeAndCharacter bikeControl;
    [SerializeField] ParticleSystem collisionParticle;
    [SerializeField] AudioSource collisionSound;

    private bool bikeDisconnect = false;

    private void Start()
    {
        bikeControl = GetComponentInParent<BikeAndCharacter>();
    }
    public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
    {
        if (!bikeDisconnect)
        {
            bikeDisconnect = true;
            if (bikeControl) bikeControl.DisconnectBike();
            if (collisionParticle) collisionParticle.Play();
            if (collisionSound) collisionSound.Play();
            if (LevelController.Instance)
            {
                StartCoroutine(LevelController.Instance.CrashCar());
            }
        }
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
