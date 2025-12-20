using Bacon;
using UnityEngine;

public class UIWorldClass : PopupBase
{
    public GameObject ThemePrefab;
    public Transform Content;

    protected override void ShowStart()
    {
        base.ShowStart();
        SetData();
    }

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

    private void SetData()
    {
        for (int i = 0; i < 2; i++)
        {
            if (Content.childCount > i)
            {
                if (i > DataManager.Instance.TotalLevel)
                {
                    Content.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    Content.GetChild(i).gameObject.SetActive(true);
                    Content.GetChild(i).GetComponent<UIWorldTheme>().SetThemeData(i + 1);
                }
            }
            else if (i < DataManager.Instance.TotalMode - 1)
            {
                GameObject item = Instantiate(ThemePrefab, Content);
                item.GetComponent<UIWorldTheme>().SetThemeData(i + 1);
            }

        }
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }
}
