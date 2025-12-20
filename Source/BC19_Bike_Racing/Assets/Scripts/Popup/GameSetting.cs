using Bacon;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{
    [Header("QUALITY")]
    public Transform ListQuality;
    public Sprite sprOn, sprOff;

    [Header("SFX")]
    public Image imgSound;
    public Sprite soundOn, soundOff;

    public Image imgMusic;

    [Header("CONTROL")]
    public Image imgControl;
    public Sprite controlButton, controlSteer;

    public Image imgDirect;
    public Sprite directLeft, directRight;

    private List<string> lstQuality = new List<string> {
        "Low Quality", "Medium Quality", "High Quality", "Ultra Quality"
    };
    private readonly int maxQuality = 4;
    private int currentQuality = 0;
    private PlayerModel _player;
    public void SetCurrentData()
    {
        //set quality
        currentQuality = _player.quality - 1;
        SetCurrentQuality();

        //set sound
        imgSound.sprite = _player.sound > 0 ? soundOn : soundOff;
        imgMusic.sprite = _player.music > 0 ? soundOn : soundOn;

        imgSound.transform.DOScaleX(_player.sound > 0 ? 1f : -1f, 0);
        imgMusic.transform.DOScaleX(_player.music > 0 ? 1f : -1f, 0);
    }

    private void OnEnable()
    {
        if (DataManager.Instance)
            _player = DataManager.Instance._player;
    }

    private void OnDisable()
    {
        DataManager.Instance.Save();
    }


    public void ChangeQuality(int offset)
    {
        //currentQuality = (currentQuality + offset) % maxQuality;
        currentQuality = offset;
        QualitySettings.SetQualityLevel(currentQuality);

        SetCurrentQuality();

        _player.quality = currentQuality;
    }

    private void SetCurrentQuality()
    {
        for (int i = 0; i < ListQuality.childCount; i++)
        {
            ListQuality.GetChild(i).GetComponent<Image>().sprite = i == currentQuality ? sprOn : sprOff;
            ListQuality.GetChild(i).GetChild(0).GetComponent<Text>().color = i == currentQuality ? Color.black : Color.white;
        }
        //txtQuality.text = lstQuality[currentQuality];
        //btnNext.interactable = currentQuality != maxQuality - 1;
        //btnPrev.interactable = currentQuality != 0;
    }

    public void ChangeSound()
    {

        _player.sound = 1 - _player.sound;
        imgSound.sprite = _player.sound > 0 ? soundOn : soundOff;
        imgSound.transform.DOScaleX(_player.sound > 0 ? 1f : -1f, 0);


        if (AudioController.Instance)
        {
            AudioController.Instance.SetSoundVolume(_player.sound);
        }
    }

    public void ChangeMusic()
    {

        _player.music = 1 - _player.music;
        imgMusic.sprite = _player.music > 0 ? soundOn : soundOff;
        imgMusic.transform.DOScaleX(_player.music > 0 ? 1f : -1f, 0);
        if (AudioController.Instance)
        {
            AudioController.Instance.SetMusicVolume(_player.music);
        }
    }

    public void ChangeDriver()
    {
        _player.driveType = _player.driveType == DriveMobile.Button ? DriveMobile.Steer : DriveMobile.Button;
        imgControl.sprite = _player.driveType == DriveMobile.Button ? controlButton : controlSteer;
    }

    public void ChangeDirect()
    {
        _player.driveDirect = _player.driveDirect == DriveDirect.Left ? DriveDirect.Right : DriveDirect.Left;
        imgDirect.sprite = _player.driveDirect == DriveDirect.Left ? directLeft : directRight;
    }

    #region DEBUG_MODE
    private int countDebug = 0;

    public void CountDebug()
    {
        countDebug++;
        if (countDebug % 10 == 0)
        {
            countDebug = 0;
            if (DebugManager.Instance)
            {
                DebugManager.Instance.ChangeDebugMode();
            }

            string message = "Debug mode is " + (DataManager.Instance.DebugMode ? "on" : "off");
            UIToast.Instance.Toast(message);
        }
    }

    #endregion
}
