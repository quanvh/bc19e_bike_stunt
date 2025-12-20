using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class SelectCar : MonoBehaviour
{
    public Image carThumb;
    public Image carLock;


    public void SetData(CarModel carModel)
    {
        currentCar = carModel;
        carThumb.sprite = carModel.Thumb;

        _player = DataManager.Instance._player;

        if (carModel.Unlock)
        {
            carThumb.color = Color.white;
        }
        else
        {
            carThumb.color = new Color(100f / 255f, 100f / 255f, 100f / 255f);
        }
        carLock.gameObject.SetActive(!carModel.Unlock);
    }

    public void OnClickCar()
    {
        GarageController.Instance.LoadCar(currentCar);
    }

    public void ReloadData()
    {
        SetData(currentCar);
    }

    private CarModel currentCar;
    private PlayerModel _player;
}
