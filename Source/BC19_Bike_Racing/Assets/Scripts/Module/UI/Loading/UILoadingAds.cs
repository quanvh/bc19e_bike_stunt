using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingAds : PopupBase
{
    [Header("POPUP PARAM")]
    public Text txtTime;
    public Image imgFill;

    private float currentTime;

    private readonly float defaultTimeWait = 6f;
    private float TimeLoadInter
    {
        get
        {
            if (DataManager.Instance)
                return DataManager.Instance.Remote.TimeLoadInter;
            else return defaultTimeWait;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currentTime = TimeLoadInter;
    }


    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime < 0)
        {
            currentTime = 0;
            return;
        }
        if (txtTime)
        {
            txtTime.text = Mathf.FloorToInt(currentTime).ToString();
        }
        if (imgFill)
        {
            imgFill.fillAmount = currentTime / TimeLoadInter;
        }
    }
}
