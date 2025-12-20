using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class UISelectModeItem : MonoBehaviour
{
    [SerializeField] Image imgBg;
    [SerializeField] Text txtName;
    [SerializeField] GameObject Lock;
    [SerializeField] Text txtLock;

    private ThemeModel _theme;

    public void SetData(ThemeModel themeModel)
    {
        _theme = themeModel;
        bool themeUnlock = themeModel.Unlock || DataManager.Instance.TotalLevelUnlock >= themeModel.Price;

        imgBg.sprite = themeModel.Thumb;
        txtName.text = themeModel.Name;
        Lock.SetActive(!themeUnlock);
        txtLock.text = "YOU NEED: " + (themeModel.Price - 1) + " LEVEL";
    }

    public void OnSelectMode()
    {
        DataManager.Instance.CurrentMode = 0;
        RouteController.Instance.CurrentTheme = _theme._name;
        UIManager.Instance.ShowPopup<UILevel>();
    }
}
