using Bacon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.LegacyInputHelpers;

public class UIClaimCar : PopupBase
{
    [Header("POPUP PARAM"), Space]
    public Image imgCar;

    public GameObject txtNotEnough;
    public GameObject txtClaim;
    public GameObject btnDrive;
    public GameObject btnAdsClaim;
    public GameObject btnGetBluePrint;


    public Text titlePopup;
    public Text txtNameButton;
    public Text txtAdsCount;

    [SerializeField] SoundFx claimSound;
    [SerializeField] SoundFx claimAdsSound;

    private CarModel currentCar;
    private CustomModel currentCustomModel;

    CUSTOMIZE_SELECT CurrentState => GarageController.Instance.currentState;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (btnAdsClaim) btnAdsClaim.SetActive(false);
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_Default);
            AdsController.Instance.OnRewardComplete += OnRewardAds;
        }
    }


    protected override void HideStart()
    {
        base.HideStart();
        DataManager.Instance.Save();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_Default);
            AdsController.Instance.OnRewardComplete -= OnRewardAds;
        }
    }

    public void SetCurrentCar()
    {
        if (GarageController.Instance)
        {
            switch (CurrentState)
            {
                case CUSTOMIZE_SELECT.Car:
                    SetCarData(GarageController.Instance.GetCarActive());
                    break;

                case CUSTOMIZE_SELECT.Char:
                case CUSTOMIZE_SELECT.Helmet:
                case CUSTOMIZE_SELECT.Wheel:
                    SetCustomModel(GarageController.Instance.CustomModel);
                    break;

                default:
                    break;
            }
        }
    }

    private int currentAds = 0;

    private void SetCustomModel(CustomModel customModel)
    {
        switch (customModel.Type)
        {
            case CUSTOM_TYPE.CHARACTER:
                titlePopup.text = "GET CHARACTER";
                txtNameButton.text = "Claim Char";
                imgCar.transform.localScale = 1.5f * Vector3.one;
                if (customModel.UnlockType != PRICE_TYPE.CARD)
                    btnAdsClaim.SetActive(!customModel.Unlock);
                break;
            case CUSTOM_TYPE.HELMET:
                titlePopup.text = "GET HELMET";
                txtNameButton.text = "Claim Helmet";
                imgCar.transform.localScale = 0.65f * Vector3.one;
                if (customModel.UnlockType != PRICE_TYPE.CARD)
                    btnAdsClaim.SetActive(!customModel.Unlock);
                break;
            case CUSTOM_TYPE.WHEEL:
                titlePopup.text = "GET WHEEL";
                txtNameButton.text = "Claim Wheel";
                imgCar.transform.localScale = 0.65f * Vector3.one;
                if (customModel.UnlockType != PRICE_TYPE.CARD)
                    btnAdsClaim.SetActive(!customModel.Unlock);
                break;
        }
        currentCustomModel = customModel;
        imgCar.sprite = currentCustomModel.Thumb;
        imgCar.SetNativeSize();


        if (customModel.UnlockType != PRICE_TYPE.CARD)
        {
            currentAds = currentCustomModel.CurrentAds;
            //totalAds = Mathf.CeilToInt((float)customModel.Price / basePrice);
            txtAdsCount.gameObject.SetActive(true);
            txtAdsCount.text = currentAds + "/" + customModel.PriceAds;
            txtNotEnough.SetActive(!customModel.Unlock);
            txtNotEnough.GetComponent<Text>().text = "Not enough gold, Do you want to unlock for free?";

            btnAdsClaim.SetActive(!customModel.Unlock);
            btnGetBluePrint.SetActive(!btnAdsClaim);

            txtClaim.SetActive(customModel.Unlock);
            btnDrive.SetActive(customModel.Unlock);
        }
        else
        {
            titlePopup.text = "NOT ENOUGH BLUEPRINTS";
            txtAdsCount.gameObject.SetActive(false);
            btnAdsClaim.SetActive(false);
            btnGetBluePrint.SetActive(true);
            txtNotEnough.GetComponent<Text>().text = "Get blueprints in lucky spin!";
        }
    }
    public void SetCarData(CarModel carModel)
    {
        imgCar.transform.localScale = 0.65f * Vector3.one;
        currentCar = carModel;
        imgCar.sprite = currentCar.Thumb;
        imgCar.SetNativeSize();
        txtNotEnough.SetActive(!carModel.Unlock);
        if (carModel.UnlockType != PRICE_TYPE.CARD)
        {
            currentAds = currentCar.CurrentAds;
            titlePopup.text = "GET BIKE";
            txtNameButton.text = "Claim Bike";

            //totalAds = Mathf.CeilToInt((float)carModel.Price / basePrice);
            txtAdsCount.gameObject.SetActive(true);
            txtAdsCount.text = currentAds + "/" + currentCar.PriceAds;
            txtNotEnough.GetComponent<Text>().text = "Not enough gold, Do you want to unlock for free?";
            btnAdsClaim.SetActive(!carModel.Unlock);
            btnGetBluePrint.SetActive(!btnAdsClaim);

            txtClaim.SetActive(carModel.Unlock);
            btnDrive.SetActive(carModel.Unlock);
        }
        else
        {
            titlePopup.text = "NOT ENOUGH BLUEPRINTS";
            txtAdsCount.gameObject.SetActive(false);
            btnAdsClaim.SetActive(false);
            btnGetBluePrint.SetActive(true);
            txtNotEnough.GetComponent<Text>().text = "Get blueprints in lucky spin!";
        }

    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }

    public void OnDrive()
    {
        RouteController.Instance.StartLevel();
    }

    public void OnWatchAds()
    {
        switch (CurrentState)
        {
            case CUSTOMIZE_SELECT.Car:
                AdsController.Instance.ShowReward(ADTYPE.CLAIM_CAR);
                break;
            case CUSTOMIZE_SELECT.Char:
                AdsController.Instance.ShowReward(ADTYPE.CLAIM_CHAR);
                break;
            case CUSTOMIZE_SELECT.Helmet:
                AdsController.Instance.ShowReward(ADTYPE.CLAIM_HELMET);
                break;
            case CUSTOMIZE_SELECT.Wheel:
                AdsController.Instance.ShowReward(ADTYPE.CLAIM_WHEEL);
                break;
        }
    }

    private void SetAds()
    {
        UIToast.Instance.Toast("Thanks for watching video.");
        currentAds++;
        currentCar.CurrentAds = currentAds;
    }

    private void ClaimAds()
    {
        if (claimAdsSound) claimAdsSound.PlaySound();
        txtAdsCount.text = currentAds + "/" + currentCustomModel.PriceAds;
    }

    private void SetButton(string _txt)
    {
        txtNotEnough.SetActive(false);
        txtClaim.GetComponent<Text>().text = "You get a new " + _txt;
        txtClaim.SetActive(true);
        btnAdsClaim.SetActive(false);

        btnDrive.SetActive(true);
    }

    private void OnRewardAds(ADTYPE _adType)
    {
        if (_adType == ADTYPE.CLAIM_CAR)
        {
            SetAds();
            if (currentAds >= currentCar.PriceAds)
            {
                currentAds = 0;
                if (claimSound) claimSound.PlaySound();

                FirebaseLogger.BuyCar(_player.currentMode, _player.currentLevel, currentCar.ID, "Ads");

                DataManager.Instance.UnlockCar(currentCar.ID);
                _player.currentCar = currentCar.ID;
                GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Car, true);

                SetButton("bike");
            }
            else
            {

                if (claimAdsSound) claimAdsSound.PlaySound();
                txtAdsCount.text = currentAds + "/" + currentCar.PriceAds;
            }
        }
        else if (_adType == ADTYPE.CLAIM_CHAR)
        {
            SetAds();

            if (currentAds >= currentCustomModel.PriceAds)
            {
                currentAds = 0;
                if (claimSound) claimSound.PlaySound();

                FirebaseLogger.BuyItem(currentCustomModel.Name, "Ads", _player.currentLevel);

                DataManager.Instance.UnlockChar(currentCustomModel.ID);
                _player.currentChar = currentCustomModel.ID;
                GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Char, true);


                SetButton("charactor");
            }
            else
            {
                ClaimAds();
            }
        }
        else if (_adType == ADTYPE.CLAIM_HELMET)
        {
            SetAds();

            if (currentAds >= currentCustomModel.PriceAds)
            {
                currentAds = 0;
                if (claimSound) claimSound.PlaySound();

                FirebaseLogger.BuyItem(currentCustomModel.Name, "Ads", _player.currentLevel);

                DataManager.Instance.UnlockHelmet(currentCustomModel.ID);
                _player.currentHelmet = currentCustomModel.ID;
                GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Helmet, true);

                SetButton("helmet");
            }
            else
            {
                ClaimAds();
            }
        }
        else if (_adType == ADTYPE.CLAIM_WHEEL)
        {
            SetAds();

            if (currentAds >= currentCustomModel.PriceAds)
            {
                currentAds = 0;
                if (claimSound) claimSound.PlaySound();

                FirebaseLogger.BuyItem(currentCustomModel.Name, "Ads", _player.currentLevel);

                DataManager.Instance.UnlockWheel(currentCustomModel.ID);
                _player.currentWheel = currentCustomModel.ID;
                GarageController.Instance.ChangeState(CUSTOMIZE_SELECT.Wheel, true);

                SetButton("wheel");
            }
            else
            {
                ClaimAds();
            }
        }
    }

    public void OpenLuckySpinCustom()
    {
        DataManager.Instance.isLuckySpinCustom = true;
        UIManager.Instance.ShowPopup<UIGacha>();
    }
}
