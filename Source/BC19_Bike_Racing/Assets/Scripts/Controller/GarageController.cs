using Bacon;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GarageController : MonoBehaviour
{
    public static GarageController Instance;

    public Transform carHolder;

    public Action<int> OnCarLoaded;
    public Action<CustomModel> OnCustomLoaded;

    public Action<CUSTOMIZE_SELECT, bool> OnStateChange;

    [HideInInspector]
    public CUSTOMIZE_SELECT currentState;

    private CarModel CurrentCar;
    [HideInInspector]
    public CustomModel CustomModel
    {
        get
        {
            if (currentState == CUSTOMIZE_SELECT.Char) return CurrentChar;
            else if (currentState == CUSTOMIZE_SELECT.Helmet) return CurrentHelmet;
            else if (currentState == CUSTOMIZE_SELECT.Wheel) return CurrentWheels;
            else return null;
        }
    }
    private CustomModel CurrentChar;
    private CustomModel CurrentHelmet;
    private CustomModel CurrentWheels;
    private GameObject CarObject;

    private bool _isSpamPopup = false;
    public bool IsSpamPopup
    {
        get { return _isSpamPopup; }
        set
        {
            _isSpamPopup = value;
        }
    }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsSpamPopup = true;
        InitGarage();
    }

    public void InitGarage()
    {
        CurrentCar = DataManager.Instance.CurrentCar;
        CurrentChar = DataManager.Instance.CurrentChar;
        CurrentHelmet = DataManager.Instance.CurrentHelmet;
        CurrentWheels = DataManager.Instance.CurrentWheel;
        currentState = CUSTOMIZE_SELECT.Car;
        LoadCar(CurrentCar);
    }


    public void ChangeState(CUSTOMIZE_SELECT newState, bool force = false)
    {

        if (force || newState != currentState)
        {
            currentState = newState;

            OnStateChange?.Invoke(newState, force);

            if (!CurrentCar.Unlock)
                LoadCar(DataManager.Instance.CurrentCar);

            if (newState == CUSTOMIZE_SELECT.Char)
            {
                LoadCustom(DataManager.Instance.CurrentChar);
            }
            else if (newState == CUSTOMIZE_SELECT.Helmet)
            {
                LoadCustom(DataManager.Instance.CurrentHelmet);
            }
            else if (newState == CUSTOMIZE_SELECT.Wheel)
            {
                LoadCustom(DataManager.Instance.CurrentWheel);
            }
            else if (newState == CUSTOMIZE_SELECT.Car)
            {
                LoadCar(DataManager.Instance.CurrentCar);
            }
        }
    }

    public void LoadCar(CarModel carModel)
    {

        if (CarObject != null && CurrentCar.ID != carModel.ID)
        {
            CurrentCar = carModel;
            Destroy(CarObject);
            CarObject = null;
        }
        if (CarObject == null)
        {
            CarObject = Instantiate(CurrentCar.Source, Vector3.zero, Quaternion.identity, carHolder);
        }
        OnCarLoaded?.Invoke(CurrentCar.ID);
    }
    public void LoadCustom(CustomModel customModel)
    {
        OnCustomLoaded?.Invoke(customModel);

        if (currentState == CUSTOMIZE_SELECT.Char)
        {
            CurrentChar = customModel;
        }
        else if (currentState == CUSTOMIZE_SELECT.Helmet)
        {
            CurrentHelmet = customModel;
        }
        else if (currentState == CUSTOMIZE_SELECT.Wheel)
        {
            CurrentWheels = customModel;
        }
    }

    public CarModel GetCarActive()
    {
        return DataManager.Instance.GetCar(CurrentCar.ID);
    }

    public void NextCar()
    {
        if (currentState == CUSTOMIZE_SELECT.Car)
        {
            CarModel nextCar = DataManager.Instance.GetCar(CurrentCar.ID, 1);
            LoadCar(nextCar);
        }
        else if (currentState == CUSTOMIZE_SELECT.Char)
        {
            CustomModel nextChar = DataManager.Instance.GetCharItem(CurrentChar.ID, 1);
            LoadCustom(nextChar);
        }
        else if (currentState == CUSTOMIZE_SELECT.Helmet)
        {
            CustomModel nextHelmel = DataManager.Instance.GetHelmetItem(CurrentHelmet.ID, 1);
            LoadCustom(nextHelmel);
        }
        else if (currentState == CUSTOMIZE_SELECT.Wheel)
        {
            CustomModel nextWheel = DataManager.Instance.GetWheelItem(CurrentWheels.ID, 1);
            LoadCustom(nextWheel);
        }
    }

    public void PreCar()
    {
        if (currentState == CUSTOMIZE_SELECT.Car)
        {
            CarModel preCar = DataManager.Instance.GetCar(CurrentCar.ID, -1);
            LoadCar(preCar);
        }
        else if (currentState == CUSTOMIZE_SELECT.Char)
        {
            CustomModel preChar = DataManager.Instance.GetCharItem(CurrentChar.ID, -1);
            LoadCustom(preChar);
        }
        else if (currentState == CUSTOMIZE_SELECT.Helmet)
        {
            CustomModel preHelmel = DataManager.Instance.GetHelmetItem(CurrentHelmet.ID, -1);
            LoadCustom(preHelmel);
        }
        else if (currentState == CUSTOMIZE_SELECT.Wheel)
        {
            CustomModel preWheel = DataManager.Instance.GetWheelItem(CurrentWheels.ID, -1);
            LoadCustom(preWheel);
        }
    }
}
