using Bacon;

public class UINotify : PopupBase
{
    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_Default);
        }
    }

    public void OnDone()
    {

        _player = DataManager.Instance._player;
        _player.currentLevel = 1;
        DataManager.Instance.Save();
        Hide(() => RouteController.Instance.PrepareGarage());
    }


    protected override void HideStart()
    {
        base.HideStart();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_Default);
        }
    }
}
