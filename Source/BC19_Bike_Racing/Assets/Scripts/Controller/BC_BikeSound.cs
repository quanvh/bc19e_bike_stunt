using Bacon;
using Kamgam.BikeAndCharacter25D;
using UnityEngine;

public class BC_BikeSound : MonoBehaviour
{
    [Range(0.25f, 1f)]
    public float minEnginePitch = 0.75f;

    [Range(1.25f, 2f)]
    public float maxEnginePitch = 2.0f;

    public Bike bike;
    public AudioSource engineSoundOn;

    private float PlayerSound
    {
        get
        {
            if (DataManager.Instance)
                return DataManager.Instance._player.sound;
            return 1;
        }
    }

    private void Start()
    {
        bike = GetComponent<Bike>();
    }

    public void ApplyEngineSound()
    {

        if (!engineSoundOn.isPlaying)
        {
            engineSoundOn.Play();
        }

        if (engineSoundOn)
        {
            engineSoundOn.volume = Mathf.Clamp(bike.Velocity, 0, bike.GetMaxVelocity() * PlayerSound);
            engineSoundOn.pitch = Mathf.Lerp(engineSoundOn.pitch,
                Mathf.Lerp(minEnginePitch, maxEnginePitch, bike.Velocity / bike.GetMaxVelocity()), Time.fixedDeltaTime * 50f);
        }
    }

    private void Update()
    {
        if (!bike.Crashed)
        {
            ApplyEngineSound();
        }
    }
}
