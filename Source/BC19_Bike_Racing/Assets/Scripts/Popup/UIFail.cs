using Bacon;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFail : PopupBase
{
    private readonly float levelReward = 0f;
    private readonly float timeReward = 0;

    private float totalReward;

    public Text txtLevel;
    public Text txtTime;
    public Text txtTotal;
    public Text txtFlip;

    public GameObject TimeUpButton, CrashButton;
    public Button btnContinue, btnReplay;

    [SerializeField] SoundFx failSound;

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_End_Level);
        }
    }

    protected override void HideStart()
    {
        base.HideStart();

        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_End_Level);
        }
    }

    public void SetLevelData()
    {
        _player ??= DataManager.Instance._player;

        if (LevelController.Instance.failType == FAIL_TYPE.TIME_UP)
        {
            FirebaseLogger.LevelFail(_player.currentMode, _player.currentLevel, _player.currentCar, "time_up");
            TimeUpButton.SetActive(true);
            CrashButton.SetActive(false);
        }
        else if (LevelController.Instance.failType == FAIL_TYPE.CRASH)
        {
            FirebaseLogger.LevelFail(_player.currentMode, _player.currentLevel, _player.currentCar, "crash");
            TimeUpButton.SetActive(false);
            CrashButton.SetActive(true);
            StartCoroutine(AnimButton());
        }


        totalReward = levelReward + timeReward;

        txtLevel.text = levelReward.ToString();
        txtTime.text = timeReward.ToString();
        txtTotal.text = totalReward.ToString();
        txtFlip.text = 0 + "";
    }

    public void FailSound()
    {
        failSound.PlaySound();
    }

    IEnumerator AnimButton()
    {
        btnReplay.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        btnContinue.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(2f);
        btnReplay.gameObject.SetActive(true);
    }

    public void OnTryAgain()
    {
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowReward(ADTYPE.REVIVE, () => OnContinue());
        }
    }


    public void OnContinue()
    {
        UIToast.Instance.Toast("Thanks for watching video. Continue your game");
        FirebaseLogger.LevelRevive(_player.currentMode, _player.currentLevel, _player.currentCar, "crash");
        LevelController.Instance.ContinueGame();
    }


    public void OnReplay()
    {
        FirebaseLogger.ClickButton("Fail_Replay", _player.currentLevel);
        FirebaseLogger.LevelRestart(_player.currentMode, _player.currentLevel, _player.currentCar);

        RouteController.Instance.StartLevel();
    }

}
