using Bacon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlockMode : PopupBase
{
    public Image imgBg;
    public ThemeData themes = null;

    private LevelMode ModeUnlocked;

    protected override void ShowStart()
    {
        base.ShowStart();
        SetData();
    }

    private void SetData()
    {
        ModeUnlocked = DataManager.Instance.GetMode(DataManager.Instance.modeToUnlock);
        if (ModeUnlocked != null)
        {
            DataManager.Instance.modeToUnlock = 0;
            DataManager.Instance.isUnlockMode = false;
            var themeMode = ModeUnlocked.levels.First()?.Theme;
            foreach (var _theme in themes.listthemes)
            {
                if (_theme._name == themeMode)
                {
                    imgBg.sprite = _theme.Thumb;
                    break;
                }
            }
        }
    }

    public void OnRace()
    {
        DataManager.Instance.CurrentMode = ModeUnlocked.ID;
        RouteController.Instance.StartLevel();
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }
}
