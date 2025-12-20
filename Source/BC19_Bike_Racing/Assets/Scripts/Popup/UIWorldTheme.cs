using Bacon;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldTheme : MonoBehaviour
{
    private int currentId;
    PlayerModel _player;

    public Image imgBG;
    public Text txtName;
    public Text txtRequired;
    public GameObject Required;
    public Button btnRace;


    private LevelMode CurrentMode;

    public void SetThemeData(int themeID)
    {
        currentId = themeID;
        CurrentMode = DataManager.Instance.GetMode(themeID);

        _player = DataManager.Instance._player;

        txtName.text = CurrentMode.Name;
        imgBG.sprite = CurrentMode.Thumb;
        if (CurrentMode.Unlock || (CurrentMode.UnlockType == PRICE_TYPE.LEVEL && DataManager.Instance.TotalLevelUnlock >= CurrentMode.Price))
        {
            Required.SetActive(false);
            btnRace.gameObject.SetActive(true);
            btnRace.onClick.RemoveListener(OnSelectTheme);
            btnRace.onClick.AddListener(OnSelectTheme);
        }
        else
        {
            if (CurrentMode.UnlockType == PRICE_TYPE.LEVEL)
            {
                txtRequired.text = "Unlock at level " + CurrentMode.Price;
            }

            Required.SetActive(true);
            btnRace.gameObject.SetActive(false);
            imgBG.GetComponent<Button>().onClick.RemoveListener(OnUnlockTheme);
            imgBG.GetComponent<Button>().onClick.AddListener(OnUnlockTheme);
        }
    }

    public void OnSelectTheme()
    {
        if (AdsController.Instance)
        {
            AdsController.Instance.LoadMrec(MrecName.Mrec_Start_Level);
        }
        if (DataManager.Instance)
        {
            DataManager.Instance.CurrentMode = currentId;
        }
        UIManager.Instance.ShowPopup<UILevel>();
    }

    private void OnUnlockTheme()
    {
        UIToast.Instance.Toast("Game mode locked. Play more to unlock");
    }
}
