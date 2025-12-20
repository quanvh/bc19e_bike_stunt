using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : PopupBase
{

    [Header("POPUP PARAM")]
    [SerializeField] Transform ListAdsItem;
    [SerializeField] Text txtAds;
    [SerializeField] Button btnAds;


    protected override void ShowStart()
    {
        base.ShowStart();
        InitData();
    }

    protected override void HideStart()
    {
        base.HideStart();
    }

    private readonly int MaxWatchAds = 5;

    private int CurrentAds
    {
        get { return RouteController.Instance.WatchAdsFree; }
        set { RouteController.Instance.WatchAdsFree = value; }
    }

    private void InitData()
    {
        txtAds.text = "WATCH VIDEOS AND GET REWARDS! [" + CurrentAds + "/" + MaxWatchAds + "]";
        btnAds.interactable = CurrentAds < MaxWatchAds;
        foreach (Transform t in ListAdsItem)
        {
            t.GetComponent<UIAdsItem>().SetData(CurrentAds + 1);
        }
    }


    public void OnWatchAds()
    {
        AdsController.Instance.ShowReward(ADTYPE.FREE_SHOP, () => AdsReward());
    }

    private void AdsReward()
    {
        CurrentAds++;
        btnAds.interactable = CurrentAds < MaxWatchAds;
        txtAds.text = "WATCH VIDEOS AND GET REWARDS! [" + CurrentAds + "/" + MaxWatchAds + "]";
        foreach (Transform t in ListAdsItem)
        {
            t.GetComponent<UIAdsItem>().UpdateData(CurrentAds);
        }
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }
}
