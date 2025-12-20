using Bacon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIFreeItem : PopupBase
{
    public Button btnAds;
    public Button btnDrive;
    public Button btnClose;

    public Image imgThumb;

    [SerializeField] SoundFx claimAdsSound;

    private CUSTOM_TYPE itemType;

    private readonly List<CUSTOM_TYPE> customTypes = new List<CUSTOM_TYPE>
    {
        CUSTOM_TYPE.NONE,
        CUSTOM_TYPE.WHEEL,
        CUSTOM_TYPE.HELMET,
        CUSTOM_TYPE.CHARACTER,
    };

    private readonly float carThumbScale = 0.9f;
    private readonly float charThumbScale = 2;
    private readonly float customThumbScale = 0.6f;

    private CarModel CurrentCar;
    private CustomModel CurrentCustom;
    protected override void ShowStart()
    {
        base.ShowStart();
        if (btnAds)
        {
            btnAds.gameObject.SetActive(true);
            btnAds.GetComponent<Animator>().enabled = false;
        }
        if (btnDrive) btnDrive.gameObject.SetActive(false);
        if (btnClose) btnClose.gameObject.SetActive(false);

        itemType = customTypes[Random.Range(0, 4)];
        if (itemType == CUSTOM_TYPE.NONE)
        {
            CurrentCar = DataManager.Instance.GetRandomCar();
            imgThumb.sprite = CurrentCar.Thumb;
            imgThumb.transform.localScale = carThumbScale * Vector3.one;
        }
        else if (itemType == CUSTOM_TYPE.CHARACTER)
        {
            CurrentCustom = DataManager.Instance.GetRandomItem(itemType);
            imgThumb.sprite = CurrentCustom.Thumb;
            imgThumb.transform.localScale = charThumbScale * Vector3.one;
        }
        else
        {
            CurrentCustom = DataManager.Instance.GetRandomItem(itemType);
            imgThumb.sprite = CurrentCustom.Thumb;
            imgThumb.transform.localScale = customThumbScale * Vector3.one;
        }
        imgThumb.SetNativeSize();
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_Default);

            if (AdsController.Instance.gameLoaded
            && DataManager.Instance.Remote.DecreaseNativeBanner)
            {
                RouteController.Instance.ShowNativeBanner = false;
            }
        }
        if (btnAds)
        {
            btnAds.GetComponent<Animator>().enabled = true;
        }
        Invoke(nameof(ShowCloseButton), 2.0f);
    }

    private void ShowCloseButton()
    {
        btnClose.gameObject.SetActive(true);
    }

    protected override void HideStart()
    {
        base.HideStart();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_Default);

            if (AdsController.Instance.gameLoaded
            && DataManager.Instance.Remote.DecreaseNativeBanner)
            {
                RouteController.Instance.ShowNativeBanner = true;
            }
        }
    }

    public void OnWatchAds()
    {
        AdsController.Instance.ShowReward(ADTYPE.CLAIM_DISCOUNT, () =>
        {
            if (itemType == CUSTOM_TYPE.NONE)
            {
                UIToast.Instance.Toast("Thanks for watching video. You got a new car!");
                CurrentCar.Unlock = true;
                _player.currentCar = CurrentCar.ID;
            }
            else if (itemType == CUSTOM_TYPE.WHEEL)
            {
                UIToast.Instance.Toast("Thanks for watching video. You got a new wheel!");
                CurrentCustom.Unlock = true;
                _player.currentWheel = CurrentCustom.ID;
            }
            else if (itemType == CUSTOM_TYPE.CHARACTER)
            {
                UIToast.Instance.Toast("Thanks for watching video. You got a new charactor!");
                CurrentCustom.Unlock = true;
                _player.currentChar = CurrentCustom.ID;
            }
            else if (itemType == CUSTOM_TYPE.HELMET)
            {
                UIToast.Instance.Toast("Thanks for watching video. You got a new helmet!");
                CurrentCustom.Unlock = true;
                _player.currentHelmet = CurrentCustom.ID;
            }
            if (claimAdsSound) claimAdsSound.PlaySound();
            DataManager.Instance.Save();
            btnDrive.gameObject.SetActive(true);
            btnAds.gameObject.SetActive(false);
        });
    }

    public void OnDrive()
    {
        RouteController.Instance.StartLevel();
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UIGarage>();
    }
}
