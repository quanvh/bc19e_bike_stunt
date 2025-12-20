using Bacon;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIAdsItem : MonoBehaviour
{
    [SerializeField] Image ImgBg;
    [SerializeField] GameObject Active;
    [SerializeField] GameObject Tick;
    [SerializeField] GameObject NumOrder;

    public Sprite sprActive, sprInactive;

    public int Order;
    public REWARD_TYPE type;
    public int rewardValue;

    public void SetData(int current)
    {
        ImgBg.sprite = Order <= current ? sprActive : sprInactive;
        Active.SetActive(current == Order);
        Tick.SetActive(Order < current);
        NumOrder.SetActive(Order >= current);
    }

    public void UpdateData(int current)
    {
        if (Order == current)
        {
            Active.SetActive(false);
            NumOrder.SetActive(false);
            Tick.SetActive(true);
            Tick.GetComponent<Image>().fillAmount = 0;
            Tick.GetComponent<Image>().DOFillAmount(1f, 0.5f);
            switch (type)
            {
                case REWARD_TYPE.GOLD:
                    DataManager.Instance._player.currentGold += rewardValue;
                    UIToast.Instance.Toast("Thanks for watching video. You get " + rewardValue + " golds");
                    break;
                case REWARD_TYPE.HELMET:
                    DataManager.Instance.UnlockHelmet(rewardValue);
                    UIToast.Instance.Toast("Thanks for watching video. You get new helmet");
                    break;
                case REWARD_TYPE.CHARACTER:
                    DataManager.Instance.UnlockChar(rewardValue);
                    UIToast.Instance.Toast("Thanks for watching video. You get new character");
                    break;
                case REWARD_TYPE.CAR:
                    DataManager.Instance.UnlockCar(rewardValue);
                    UIToast.Instance.Toast("Thanks for watching video. You get new car");
                    break;
            }
            DataManager.Instance.Save();
        }
        else if (Order == current + 1)
        {
            ImgBg.sprite = sprActive;
            Active.SetActive(true);
        }
    }
}
