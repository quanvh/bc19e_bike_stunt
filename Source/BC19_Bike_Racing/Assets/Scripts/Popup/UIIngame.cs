using Bacon;
using BC.Parking;
using DG.Tweening;
using Kamgam.BikeAndCharacter25D;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIIngame : PopupBase, IBikeTouchInput
{
    [Header("POPUP PARAM"), Space]
    public Text txtTime;
    public Text txtLevel;

    public Animator animTime;

    private int minute, second, milisecond;

    public GameObject ControlGroup;
    public GameObject btnPause;

    public GameObject Win, Fail;
    public Transform txtWin;
    public Image txtFail;
    public GameObject imgFail;

    [Header("TOUCH INPUT"), Space]
    public BC_UIController SpeedUpButton;
    public BC_UIController BrakeButton;
    public BC_UIController RotateRightButton;
    public BC_UIController RotateLeftButton;
    private BikeAndCharacter _bikeAndCharacter;

    [Header("FLIP"), Space]
    private bool canFlip;
    private int totalFlip;
    public Transform txtFlipParent;
    public GameObject txtFlipPf;
    private GameObject txtInsFlip;


    [SerializeField] SoundFx missionSound;

    private bool isPause = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        isPause = false;
        btnPause.SetActive(true);
        ControlGroup.SetActive(true);
        Fail.SetActive(false);
        Win.SetActive(false);
    }

    bool IBikeTouchInput.IsSpeedUpPressed()
    {
        return SpeedUpButton.pressing;
    }

    bool IBikeTouchInput.IsBrakePressed()
    {
        return BrakeButton.pressing;
    }

    public bool IsRotateCWPressed()
    {
        if (RotateRightButton) return RotateRightButton.pressing;
        else return Input.acceleration.x > 0;
    }

    public bool IsRotateCCWPressed()
    {
        if (RotateLeftButton) return RotateLeftButton.pressing;
        else return Input.acceleration.x < 0;
    }

    public void ResetUI()
    {
        totalFlip = 0;
        canFlip = false;
        txtTime.text = "00:00:00";
        txtLevel.text = "Level " + _player.currentLevel;
    }


    private float timeLevel = 0f;
    private void FixedUpdate()
    {
        if (LevelController.Instance.IsPlay)
        {
            timeLevel = LevelController.Instance.timeLevel;

            if (LevelController.Instance.UseTimeUp)
            {
                timeLevel = LevelController.Instance.TimeUpValue - timeLevel;
                if (timeLevel < 0f) timeLevel = 0f;

                animTime.enabled = timeLevel < 10f;


                if (timeLevel < 10f)
                {
                    if (!animTime.GetComponent<AudioSource>().isPlaying)
                    {
                        animTime.GetComponent<AudioSource>().volume = _player.sound;
                        animTime.GetComponent<AudioSource>().Play();
                    }
                }
                else
                {
                    animTime.GetComponent<AudioSource>().Stop();
                }
            }

            minute = (int)(timeLevel / 60);
            second = (int)timeLevel - 60 * minute;
            milisecond = (int)(100 * (timeLevel - 60 * minute - second));

            if (txtTime)
                txtTime.text = string.Format("{0}:{1}:{2}", minute.ToString("00"), second.ToString("00"), milisecond.ToString("00"));


            FlipBike();
        }
    }

    private void FlipBike()
    {

        if (_bikeAndCharacter == null)
        {
            _bikeAndCharacter = FindObjectOfType<BikeAndCharacter>();
        }
        if (_bikeAndCharacter && !_bikeAndCharacter.Grounded)
        {
            if (_bikeAndCharacter.Bike.BikeForward)
            {
                canFlip = true;
            }
            else
            {
                if (canFlip)
                {
                    canFlip = false;
                    totalFlip++;
                    LevelController.Instance.FlipCount++;
                    txtInsFlip = Instantiate(txtFlipPf, txtFlipParent);
                    txtInsFlip.GetComponent<Text>().text = "Flip +" + totalFlip;
                    StartCoroutine(DestroyTxtIns());
                }
            }
        }
        else
        {
            canFlip = false;
        }
    }

    IEnumerator DestroyTxtIns()
    {
        yield return new WaitForSeconds(1.5f);
        totalFlip = 0;
        Destroy(txtInsFlip);
    }

    public void CompleteLevel()
    {
        Vibration.Vibrate(200);
        if (missionSound) missionSound.PlaySound();

        DOTween.defaultEaseOvershootOrAmplitude = 1.02f;
        ControlGroup.SetActive(false);

        Win.SetActive(true);
        txtWin.localPosition = new Vector2(-1500f, 29f);
        txtWin.DOLocalMoveX(0f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            txtWin.DOLocalMoveX(1500f, .5f).OnComplete(() =>
                LevelController.Instance.CompleteGame())
            .SetDelay(0.25f).SetEase(Ease.Linear);
        }).SetDelay(0.2f);
    }

    public void FailLevel()
    {
        ControlGroup.SetActive(false);
        Fail.SetActive(true);
        imgFail.SetActive(true);
        txtFail.color = new Color(1f, 1f, 1f, 0f);
        txtFail.DOFade(1f, 0.5f).SetDelay(.3f).OnComplete(() =>
        {
            txtFail.DOFade(0f, 0.3f).SetDelay(.5f).OnComplete(() => LevelController.Instance.FailGame());
        });
    }

    public void OnPause()
    {
        LevelController.Instance.PauseGame(true);
        UIManager.Instance.ShowPopup<UIPause>();
    }

    public void OnZoom()
    {
        isPause = !isPause;
        btnPause.SetActive(!isPause);
        LevelController.Instance.ZoomMap();
    }
}
