using Bacon;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    public Image imgBg;
    public Sprite levelOn, levelOff;
    public Sprite starOn, starOff;

    public Text txtLevel;

    public GameObject Current;
    public GameObject Stars;
    public GameObject LockIcon;

    private int currentId;
    private LevelModel CurrentLevel;

    PlayerModel _player;

    public void SetLevelData(int levelId)
    {
        currentId = levelId;
        CurrentLevel = DataManager.Instance.GetLevel(levelId);

        _player = DataManager.Instance._player;
        txtLevel.text = levelId.ToString();
        if (CurrentLevel.Unlock)
        {
            imgBg.sprite = levelOn;
            GetComponent<Button>().interactable = true;
            Current.SetActive(_player.currentLevel == levelId);
            Stars.SetActive(true);
            LockIcon.SetActive(false);
            for (int i = 0; i < Stars.transform.childCount; i++)
            {
                if (i < CurrentLevel.Star)
                    Stars.transform.GetChild(i).GetComponent<Image>().sprite = starOn;
                else Stars.transform.GetChild(i).GetComponent<Image>().sprite = starOff;
            }
        }
        else
        {
            imgBg.sprite = levelOff;
            GetComponent<Button>().interactable = false;
            Current.SetActive(false);
            Stars.SetActive(false);
            LockIcon.SetActive(true);
        }

    }

    public void OnSelectLevel()
    {
        _player.currentLevel = currentId;
        RouteController.Instance.StartLevel();
    }
}
