using Bacon;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITimeUp : PopupBase
{
    public Button btnContinue, btnReplay;

    public Text txtTime;

    private readonly float timeUpExtra = 30f;

    protected override void OnEnable()
    {
        base.OnEnable();
        txtTime.text = "+" + timeUpExtra;
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        StartCoroutine(AnimButton());

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

    IEnumerator AnimButton()
    {
        btnReplay.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        btnContinue.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(2f);
        btnReplay.gameObject.SetActive(true);
    }

    public void OnContinue()
    {
        FirebaseLogger.ClickButton("Timeup_Ads", _player.currentLevel);

        AdsController.Instance.ShowReward(ADTYPE.TIME_UP, () =>
        {
            FirebaseLogger.LevelRevive(_player.currentMode, _player.currentLevel, _player.currentCar, "time_up");
            LevelController.Instance.ContinueGame();
        });
    }

    public void OnReplay()
    {
        FirebaseLogger.ClickButton("Timeup_Replay", _player.currentLevel);
        FirebaseLogger.LevelRestart(_player.currentMode, _player.currentLevel, _player.currentCar);

        //AdsController.Instance.ShowInter("Timeup_Replay", () =>
        //{
        //    RouteController.Instance.PrepareGarage();
        //});
        RouteController.Instance.PrepareGarage();
    }

}
