using Bacon;
using System.Collections;
using UnityEngine;

public class UILuckyBox : PopupBase
{
    [Header("POPUP PARAM")]
    public IGacha luckyGacha;

    public void OnShowStart()
    {
        if (!luckyGacha.InitAtStart) luckyGacha.InitGacha();
        Invoke(nameof(ShowCard), 0.5f);
    }

    private void ShowCard()
    {
        _player.gachaCount++;
        luckyGacha.StartSpin();
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance && AdsController.Instance.gameLoaded
        && DataManager.Instance.Remote.DecreaseNativeBanner)
        {
            RouteController.Instance.ShowNativeBanner = false;
        }
    }

    protected override void HideStart()
    {
        base.HideStart();
        if (AdsController.Instance && AdsController.Instance.gameLoaded
        && DataManager.Instance.Remote.DecreaseNativeBanner)
        {
            RouteController.Instance.ShowNativeBanner = true;
        }
    }


    public void OnClose()
    {
        StartCoroutine(ShowGarage());
    }

    IEnumerator ShowGarage()
    {
        yield return new WaitForSeconds(1f);
        Hide(() => RouteController.Instance.PrepareGarage());
    }
}
