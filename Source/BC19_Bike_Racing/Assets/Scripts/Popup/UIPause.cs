using Bacon;

public class UIPause : PopupBase
{
    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_Default);
        }
    }

    protected override void HideStart()
    {
        base.HideStart();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_Default);
        }
    }


    public void OnHome()
    {
        RouteController.Instance.PrepareGarage();
        //FirebaseLogger.ClickButton("Pause_Home", _player.currentLevel);

        //AdsController.Instance.ShowInter("Pause_Home", () => RouteController.Instance.PrepareGarage());
    }

    public void OnContinue()
    {
        LevelController.Instance.PauseGame(false);
        UIManager.Instance.ShowPopup<UIIngame>();
    }


    public void OnReplay()
    {
        RouteController.Instance.StartLevel();
    }

}
