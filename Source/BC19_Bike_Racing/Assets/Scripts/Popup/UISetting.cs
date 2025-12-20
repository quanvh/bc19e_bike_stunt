using Bacon;

public class UISetting : PopupBase
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


    public void OnClose()
    {
        Hide();
    }

}
