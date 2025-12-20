using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class UIListItem : MonoBehaviour
{
    public Image Thumb;
    public Image Lock;
    public GameObject Card;
    public GameObject Active;
    public Text NumCard;


    private void OnEnable()
    {
        _player = DataManager.Instance._player;
    }

    private readonly float carThumbScale = 0.25f;
    public void SetCarData(int carId)
    {
        currentCar = DataManager.Instance.GetCar(carId);

        Thumb.sprite = currentCar.Thumb;
        Thumb.SetNativeSize();
        Thumb.transform.localScale = carThumbScale * Vector3.one;

        SetActiveCar(_player.currentCar);


        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClickCar);
    }

    public void SetActiveCar(int carId)
    {
        if (currentCar.Unlock)
        {
            Card.SetActive(false);
        }
        else
        {
            Card.SetActive(currentCar.UnlockByCard);
            if (currentCar.UnlockByCard)
            {
                NumCard.text = currentCar.CurrentCard + "/" + currentCar.PriceCard;
            }
        }
        Lock.gameObject.SetActive(!currentCar.Unlock);
        Active.SetActive(currentCar.ID == carId);
    }


    public void OnClickCar()
    {
        AudioController.Instance.Click();
        GarageController.Instance.LoadCar(currentCar);
    }

    private readonly float customCharScale = 0.8f;
    private readonly float customWheelScale = 0.25f;
    private readonly float customThumbScale = 0.3f;
    public void SetCustomData(CustomModel item, int _current)
    {
        currentCustom = item;
        Thumb.sprite = currentCustom.Thumb;
        Thumb.SetNativeSize();
        if (item.Type == CUSTOM_TYPE.CHARACTER)
        {
            Thumb.transform.localScale = customCharScale * Vector3.one;
        }
        else if (item.Type == CUSTOM_TYPE.WHEEL)
        {
            Thumb.transform.localScale = customWheelScale * Vector3.one;
        }
        else
        {
            Thumb.transform.localScale = customThumbScale * Vector3.one;
        }

        SetActiveItem(_current);

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClickCustom);
    }

    private void OnClickCustom()
    {
        AudioController.Instance.Click();
        GarageController.Instance.LoadCustom(currentCustom);
    }

    public void SetActiveItem(int itemId)
    {
        if (currentCustom == null) return;
        if (currentCustom.Unlock)
        {
            Card.SetActive(false);
        }
        else
        {
            Card.SetActive(currentCustom.UnlockByCard);
            if (currentCustom.UnlockByCard)
            {
                NumCard.text = currentCustom.CurrentCard + "/" + currentCustom.PriceCard;
            }
        }
        Lock.gameObject.SetActive(!currentCustom.Unlock);
        Active.SetActive(currentCustom.ID == itemId);
    }

    private CarModel currentCar;
    private CustomModel currentCustom;
    private PlayerModel _player;
}
