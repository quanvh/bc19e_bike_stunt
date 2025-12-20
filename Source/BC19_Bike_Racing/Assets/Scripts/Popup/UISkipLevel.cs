using Bacon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkipLevel : PopupBase
{
    public void OnSkipLevel()
    {
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowReward(ADTYPE.SKIP_LEVEL, () =>
            {
                OnSkipSuccess();
            });
        }
    }

    public void OnSkipSuccess()
    {
        UIToast.Instance.Toast("Skip level success!");
        DataManager.Instance.SetLevelData();
        RouteController.Instance.PrepareGarage();
    }


    public void OnClose()
    {
        LevelController.Instance.ShowAdsFail();
    }
}
