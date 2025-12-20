using Bacon;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UIGarageButton : MonoBehaviour
{

    public Text txtAds;
    public Text txtPrice;
    public Text txtCard;

    public Button btnPlay, btnBuy, btnBuyAds, btnBuyCard;

    [SerializeField] SoundFx unlockSound;
    [SerializeField] SoundFx unlockAdsSound;
    [SerializeField] UIGarage uiGarage;

    private CarModel currentCar;
    public CustomModel currentCustom;

    [HideInInspector]
    public CUSTOMIZE_SELECT currentState;

    private PlayerModel _player;

    private int currentAds = 0;
    private int totalAds = 0;

    private void OnEnable()
    {
        if (DataManager.Instance && DataManager.Instance.dataLoaded)
        {
            _player = DataManager.Instance._player;
            OnCarLoaded(_player.currentCar);
        }
        if (GarageController.Instance)
        {
            currentState = CUSTOMIZE_SELECT.Car;
            GarageController.Instance.OnStateChange += OnChangeState;
            GarageController.Instance.OnCarLoaded += OnCarLoaded;
            GarageController.Instance.OnCustomLoaded += OnCustomLoaded;
        }

        if (AdsController.Instance)
        {
            AdsController.Instance.OnRewardComplete += OnRewardAds;
        }
    }

    private void OnDisable()
    {
        if (GarageController.Instance)
        {
            GarageController.Instance.OnStateChange -= OnChangeState;
            GarageController.Instance.OnCarLoaded -= OnCarLoaded;
            GarageController.Instance.OnCustomLoaded -= OnCustomLoaded;
        }

        if (AdsController.Instance)
        {
            AdsController.Instance.OnRewardComplete -= OnRewardAds;
        }
        DataManager.Instance.Save();
    }

    public void OnChangeState(CUSTOMIZE_SELECT newState, bool force = false)
    {
        if (force || currentState != newState)
        {
            currentState = newState;
        }
    }

    public void OnCarLoaded(int carId)
    {
        currentCar = DataManager.Instance.GetCar(carId);

        if (currentCar == null) return;

        UpdateCarButton(currentCar);

        if (!currentCar.Unlock)
        {
            txtPrice.text = currentCar.Price.ToString("N0",
                    CultureInfo.CreateSpecificCulture("en-US"));
        }
        else
        {
            _player.currentCar = carId;
        }
    }

    public void OnCustomLoaded(CustomModel customModel)
    {
        if (customModel == null) return;
        currentCustom = customModel;
        UpdateCustomizeItemButton(currentCustom);
        if (!currentCustom.Unlock)
        {
            txtPrice.text = currentCustom.Price.ToString("N0",
                    CultureInfo.CreateSpecificCulture("en-US"));
        }
        else
        {
            switch (customModel.Type)
            {
                case CUSTOM_TYPE.COLOR:
                    _player.currentColor = customModel.ID;
                    break;
                case CUSTOM_TYPE.HELMET:
                    _player.currentHelmet = customModel.ID;
                    break;
                case CUSTOM_TYPE.WHEEL:
                    _player.currentWheel = customModel.ID;
                    break;
                case CUSTOM_TYPE.CHARACTER:
                    _player.currentChar = customModel.ID;
                    break;
                default: break;
            }
        }
    }


    private void UpdateCarButton(CarModel _car)
    {
        if (btnPlay) btnPlay.gameObject.SetActive(_car.Unlock);
        if (_car.Unlock)
        {
            if (btnBuy) btnBuy.gameObject.SetActive(false);
            if (btnBuyAds) btnBuyAds.gameObject.SetActive(false);
            if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
        }
        else
        {
            if (currentCar.UnlockType == PRICE_TYPE.CARD)
            {
                //if (btnBuyCard) btnBuyCard.gameObject.SetActive(currentCar.CurrentCard >= currentCar.PriceCard);
                //txtCard.text = currentCar.CurrentCard + "/" + currentCar.PriceCard;3

                btnBuyCard.gameObject.SetActive(true);
                btnBuyAds.gameObject.SetActive(false);
                btnBuy.gameObject.SetActive(false);

                txtCard.text = currentCar.CurrentCard + "/" + currentCar.PriceCard;
            }
            if (currentCar.UnlockType == PRICE_TYPE.GOLD)
            {
                if (btnBuy) btnBuy.gameObject.SetActive(true);
                if (btnBuyAds) btnBuyAds.gameObject.SetActive(false);
                if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
            }
            else if (currentCar.UnlockType == PRICE_TYPE.ADS)
            {
                currentAds = currentCar.CurrentAds;
                totalAds = currentCar.Price;
                txtAds.text = currentAds + "/" + totalAds;
                if (btnBuy) btnBuy.gameObject.SetActive(false);
                if (btnBuyAds) btnBuyAds.gameObject.SetActive(true);
                if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateCustomizeItemButton(CustomModel _model)
    {
        if (_model == null) return;
        if (btnPlay && currentCar != null)
        {
            btnPlay.gameObject.SetActive(currentCar.Unlock && _model.Unlock);
        }
        if (_model.Unlock)
        {
            if (btnBuy) btnBuy.gameObject.SetActive(false);
            if (btnBuyAds) btnBuyAds.gameObject.SetActive(false);
            if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
        }
        else
        {
            if (_model.UnlockType == PRICE_TYPE.CARD)
            {
                btnBuyCard.gameObject.SetActive(true);
                if (btnBuy) btnBuy.gameObject.SetActive(false);
                if (btnBuyAds) btnBuyAds.gameObject.SetActive(false);
                txtCard.text = _model.CurrentCard + "/" + _model.PriceCard;
            }
            if (_model.UnlockType == PRICE_TYPE.GOLD)
            {
                if (btnBuy) btnBuy.gameObject.SetActive(true);
                if (btnBuyAds) btnBuyAds.gameObject.SetActive(false);
                if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
            }
            else if (_model.UnlockType == PRICE_TYPE.ADS)
            {
                currentAds = _model.CurrentAds;
                totalAds = _model.Price;
                txtAds.text = currentAds + "/" + totalAds;
                if (btnBuy) btnBuy.gameObject.SetActive(false);
                if (btnBuyAds) btnBuyAds.gameObject.SetActive(true);
                if (btnBuyCard) btnBuyCard.gameObject.SetActive(false);
            }
        }
    }

    public void OnBuyGold()
    {
        switch (currentState)
        {
            case CUSTOMIZE_SELECT.Car:
                FirebaseLogger.ClickButton("Garage_Buy_Car", _player.currentLevel);

                if (currentCar.UnlockType == PRICE_TYPE.GOLD && _player.currentGold >= currentCar.Price)
                {
                    _player.currentGold -= currentCar.Price;
                    currentCar.Unlock = true;
                    _player.currentCar = currentCar.ID;
                    if (uiGarage) uiGarage.UpdateCurrency();
                    if (unlockSound) unlockSound.PlaySound();

                    GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Car, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }
                break;
            case CUSTOMIZE_SELECT.Char:
                if (currentCustom.UnlockType == PRICE_TYPE.GOLD && _player.currentGold >= currentCustom.Price)
                {
                    _player.currentGold -= currentCustom.Price;
                    currentCustom.Unlock = true;
                    _player.currentChar = currentCustom.ID;
                    if (uiGarage) uiGarage.UpdateCurrency();
                    if (unlockSound) unlockSound.PlaySound();

                    GarageController.Instance.ChangeState(currentState, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }

                break;
            case CUSTOMIZE_SELECT.Helmet:
                if (currentCustom.UnlockType == PRICE_TYPE.GOLD && _player.currentGold >= currentCustom.Price)
                {
                    _player.currentGold -= currentCustom.Price;
                    currentCustom.Unlock = true;
                    _player.currentHelmet = currentCustom.ID;
                    if (uiGarage) uiGarage.UpdateCurrency();
                    if (unlockSound) unlockSound.PlaySound();

                    GarageController.Instance.ChangeState(currentState, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }
                break;

            case CUSTOMIZE_SELECT.Wheel:
                if (currentCustom.UnlockType == PRICE_TYPE.GOLD && _player.currentGold >= currentCustom.Price)
                {
                    _player.currentGold -= currentCustom.Price;
                    currentCustom.Unlock = true;
                    _player.currentWheel = currentCustom.ID;
                    if (uiGarage) uiGarage.UpdateCurrency();
                    if (unlockSound) unlockSound.PlaySound();

                    GarageController.Instance.ChangeState(currentState, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }
                break;
        }
    }

    public void OnBuyCard()
    {
        switch (currentState)
        {
            case CUSTOMIZE_SELECT.Car:
                FirebaseLogger.ClickButton("Garage_Buy_Car_Card", _player.currentLevel);

                if (currentCar.UnlockByCard && currentCar.CurrentCard >= currentCar.PriceCard)
                {
                    currentCar.CurrentCard = 0;
                    currentCar.Unlock = true;
                    _player.currentCar = currentCar.ID;
                    if (unlockSound) unlockSound.PlaySound();

                    GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Car, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }
                break;
            case CUSTOMIZE_SELECT.Char:
            case CUSTOMIZE_SELECT.Helmet:
            case CUSTOMIZE_SELECT.Wheel:
                if (currentCustom.UnlockByCard && currentCustom.CurrentCard >= currentCustom.PriceCard)
                {
                    currentCustom.CurrentCard = 0;
                    currentCustom.Unlock = true;
                    if (currentState == CUSTOMIZE_SELECT.Char)
                    {
                        _player.currentChar = currentCustom.ID;
                    }
                    else if (currentState == CUSTOMIZE_SELECT.Helmet)
                    {
                        _player.currentHelmet = currentCustom.ID;
                    }
                    else if (currentState == CUSTOMIZE_SELECT.Wheel)
                    {
                        _player.currentWheel = currentCustom.ID;
                    }

                    if (unlockSound) unlockSound.PlaySound();
                    GarageController.Instance.ChangeState(currentState, true);
                }
                else
                {
                    UIManager.Instance.ShowPopup<UIClaimCar>();
                }
                break;
        }
    }

    public void OnBuyAds()
    {
        switch (currentState)
        {
            case CUSTOMIZE_SELECT.Car:
                AdsController.Instance.ShowReward(ADTYPE.BUY_CAR);
                break;
            case CUSTOMIZE_SELECT.Char:
                AdsController.Instance.ShowReward(ADTYPE.BUY_CHAR);
                break;
            case CUSTOMIZE_SELECT.Helmet:
                AdsController.Instance.ShowReward(ADTYPE.BUY_HELMET);
                break;
            case CUSTOMIZE_SELECT.Wheel:
                AdsController.Instance.ShowReward(ADTYPE.BUY_WHEEL);
                break;
            default: break;

        }

    }

    private void OnRewardAds(ADTYPE _adType)
    {
        if (_adType == ADTYPE.BUY_CAR)
        {
            currentAds++;
            currentCar.CurrentAds = currentAds;
            if (currentAds >= totalAds)
            {
                currentAds = 0;
                if (unlockSound) unlockSound.PlaySound();
                FirebaseLogger.BuyCar(_player.currentMode, _player.currentLevel, currentCar.ID, "Ads");

                DataManager.Instance.UnlockCar(currentCar.ID);
                _player.currentCar = currentCar.ID;
                GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Car, true);

                UIToast.Instance.Toast("You've just unlock new bike");
                //ReloadCar();
            }
            else
            {
                UIToast.Instance.Toast("Thanks for watching. You need " + (totalAds - currentAds) + " more video to unlock car.");
                if (unlockAdsSound) unlockAdsSound.PlaySound();
                txtAds.text = currentAds + "/" + totalAds;
            }
        }
        else if (_adType == ADTYPE.BUY_CHAR || _adType == ADTYPE.BUY_HELMET || _adType == ADTYPE.BUY_WHEEL)
        {
            currentAds++;
            currentCustom.CurrentAds = currentAds;
            if (currentAds >= totalAds)
            {
                currentAds = 0;
                if (unlockSound) unlockSound.PlaySound();

                currentCustom.Unlock = true;
                if (_adType == ADTYPE.BUY_CHAR)
                {
                    _player.currentChar = currentCustom.ID;
                    UIToast.Instance.Toast("You've just unlock new character");
                }
                else if (_adType == ADTYPE.BUY_HELMET)
                {
                    _player.currentHelmet = currentCustom.ID;
                    UIToast.Instance.Toast("You've just unlock new helmet");
                }
                else if (_adType == ADTYPE.BUY_WHEEL)
                {
                    _player.currentWheel = currentCustom.ID;
                    UIToast.Instance.Toast("You've just unlock new wheel");
                }
                GarageController.Instance.LoadCustom(currentCustom);
            }
            else
            {
                if (unlockAdsSound) unlockAdsSound.PlaySound();
                txtAds.text = currentAds + "/" + totalAds;
                UIToast.Instance.Toast("Thanks for watching. You need " + (totalAds - currentAds) + " more video to unlock item.");
            }
        }
    }
}
