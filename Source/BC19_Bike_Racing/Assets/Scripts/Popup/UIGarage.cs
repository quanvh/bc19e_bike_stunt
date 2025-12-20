using Bacon;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGarage : PopupBase
{
    [Header("PARAM"), Space]
    public Text txtFree;
    public ScoreText playerGold;
    public TMP_Text txtName;
    public TMP_InputField inputField;
    public Button btnRace;
    public GameObject BtnMoreGame;

    protected override void ShowStart()
    {
        base.ShowStart();
        UnityMainThreadDispatcher.Enqueue(SetData);

        if (GarageController.Instance)
        {
            if (GarageController.Instance.IsSpamPopup)
            {
                AdsController.Instance.LoadMrec(MrecName.Mrec_Default);
            }
        }
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (DataManager.Instance && DataManager.Instance.dataLoaded)
        {
            DebugManager.Instance.OnAddCurrency += UpdateCurrency;
        }

        UIManager.Instance.ShowPopup<UIDragGarage>(false);

        float timeWait = 1.0f;
        if (RouteController.Instance.goldWinLevel > 0)
        {
            StartCoroutine(playerGold.AddCurrency((int)RouteController.Instance.goldWinLevel));
            timeWait = 3.5f;
        }
        else
        {
            UpdateCurrency();
        }

        Invoke(nameof(ShowPopup), timeWait);

    }

    protected override void HideStart()
    {
        base.HideStart();
        UIManager.Instance.HidePopup<UIDragGarage>();
        DebugManager.Instance.OnAddCurrency -= UpdateCurrency;
    }

    private void ShowPopup()
    {
        DataManager.Instance.isLuckySpinCustom = false;
        if (GarageController.Instance && GarageController.Instance.IsSpamPopup)
        {
            GarageController.Instance.IsSpamPopup = false;
            //First check to show unlock mode, then daily reward, forward by spin
            if (DataManager.Instance && DataManager.Instance.isUnlockMode)
            {
                UIManager.Instance.ShowPopup<UIUnlockMode>();
            }
            else if (DataManager.Instance.ShowDaily)
            {
                OnDaily();
            }
            else if (_player.dailyGacha > 0)
            {
                UIManager.Instance.ShowPopup<UIGacha>();
            }
            else
            {
                UIManager.Instance.ShowPopup<UIFreeItem>();

                //if (Random.Range(0, 2) == 1)
                //{
                //    UIManager.Instance.ShowPopup<UIGacha>();
                //}
                //else
                //{
                //    UIManager.Instance.ShowPopup<UIFreeItem>();
                //}
            }
        }
        btnRace.interactable = true;
    }

    public void SetData()
    {
        if (DataManager.Instance)
        {
            inputField.text = _player.name;
            SetFreeCoin();
            BtnMoreGame.SetActive(DataManager.Instance.Remote.MoreGame);
        }
        btnRace.interactable = false;
    }


    private int freeCoin = 2000;
    private void SetFreeCoin()
    {
        freeCoin = _player.freeCoin >= 3 ? 5000 : 2000 + 1000 * _player.freeCoin;
        txtFree.text = freeCoin.ToString();
    }

    public void UpdateCurrency()
    {
        StartCoroutine(playerGold.StartAnim(_player.currentGold));
    }

    public void OnFreeCoin()
    {
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowReward(ADTYPE.FREE_COIN, () =>
            {
                OnGetFreeCoin();
            });
        }
    }

    public void OnGetFreeCoin()
    {
        UIToast.Instance.Toast("Thanks for watching video. You got " + freeCoin + "coins!");
        StartCoroutine(playerGold.AddCurrency(freeCoin));
        _player.currentGold += freeCoin;
        _player.freeCoin++;

        DataManager.Instance.Save();
        SetFreeCoin();
    }

    public void OnNextCar()
    {
        GarageController.Instance.NextCar();
    }

    public void OnPreCar()
    {
        GarageController.Instance.PreCar();
    }

    public void OnSetting()
    {
        UIManager.Instance.ShowPopup<UISetting>(false);
    }

    private bool loadBanner = false;
    public void OnDrive()
    {
        if (loadBanner) return;
        loadBanner = true;

        DataManager.Instance.CurrentMode = 0;
        FirebaseLogger.ClickButton("Garage_Drive", _player.currentLevel);

        AdsController.Instance.LoadMrec(MrecName.Mrec_Start_Level);

        UIManager.Instance.ShowPopup<UILevel>(null, (t) =>
        {
            loadBanner = false;
        });
    }

    public void OnSelectMode()
    {
        AdsController.Instance.LoadMrec(MrecName.Mrec_Start_Level);

        UIManager.Instance.ShowPopup<UISelectMode>();
    }

    public void OnSpin()
    {
        FirebaseLogger.ClickButton("Garage_Spin", _player.currentLevel);
        UIManager.Instance.ShowPopup<UIGacha>();
    }

    public void OnDaily()
    {
        UIManager.Instance.ShowPopup<UIDaily>(null, null, () => Show());
    }

    public void OnShop()
    {
        UIManager.Instance.ShowPopup<UIShop>();
    }

    public void OnWorldClass()
    {
        FirebaseLogger.ClickButton("Garage_World", _player.currentLevel);
        UIManager.Instance.ShowPopup<UIWorldClass>();
    }

    public void OnChangeName()
    {
        if (inputField.text != "")
        {
            _player.name = inputField.text;
            DataManager.Instance.Save();
        }
        inputField.text = _player.name;
    }

    public void OnMoreGame()
    {
        FirebaseLogger.ClickButton("More_Game", _player.currentLevel);
        UIManager.Instance.ShowPopup<UIMoreGame>(false);
    }

    public void OnRace()
    {
        if (loadBanner) return;
        loadBanner = true;

        FirebaseLogger.ClickButton("Garage_Race", _player.currentLevel);
        DataManager.Instance.CurrentMode = 3;

        RouteController.Instance.StartLevel();
    }
}

