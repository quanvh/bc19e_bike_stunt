using Bacon;
using DG.Tweening;
using Kamgam.BikeAndCharacter25D.Helpers;
using UnityEngine;

public class Coin : MonoBehaviour, ITrigger2DReceiver
{
    [SerializeField] private SoundFx coinSound;
    public void OnCustomTriggerEnter2D(Trigger2D trigger, Collider2D other)
    {
        if (coinSound) coinSound.PlaySound();
        LevelController.Instance.SavePoint();
        transform.DOBlendableLocalMoveBy(2f * Vector3.up, 0.25f).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                LevelController.Instance.CoinCollected++;
                Destroy(gameObject);
            });
    }

    public void OnCustomTriggerExit2D(Trigger2D trigger, Collider2D other)
    {
    }

    public void OnCustomTriggerStay2D(Trigger2D trigger, Collider2D other)
    {
    }
}
