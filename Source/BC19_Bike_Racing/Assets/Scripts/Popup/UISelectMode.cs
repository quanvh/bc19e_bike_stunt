using Bacon;
using DG.Tweening;
using UnityEngine;

public class UISelectMode : PopupBase
{
    public ThemeData themes = null;
    [SerializeField] Transform Content;
    [SerializeField] UISelectModeItem SelectItem;


    protected override void ShowStart()
    {
        base.ShowStart();
        SetData();
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        ShowData();
    }


    private readonly float duration = 0.5f;
    private void ShowData()
    {
        for (int i = 0; i < Content.childCount; i++)
        {
            float delayTime = i * 0.15f;
            Content.GetChild(i).DOScale(Vector3.one, duration)
            .SetEase(Ease.OutBack)
            .SetDelay(delayTime);
        }
    }

    private void SetData()
    {
        int i = 0;
        foreach (var item in themes.listthemes)
        {
            UISelectModeItem newMode;
            if (Content.childCount > i)
            {
                newMode = Content.GetChild(i).GetComponent<UISelectModeItem>();
            }
            else
            {
                newMode = Instantiate(SelectItem, Content);
            }
            newMode.SetData(item);
            newMode.transform.localScale = Vector3.zero;
            i++;
        }
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }
}
