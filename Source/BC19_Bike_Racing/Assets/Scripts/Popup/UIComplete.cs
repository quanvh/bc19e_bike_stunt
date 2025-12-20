using Bacon;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIComplete : PopupBase
{
    private float levelReward = 150f;
    private float timeReward = 150f;

    private int multiplyAds = 3;

    private int star;
    [HideInInspector] public float totalReward;

    [Header("POPUP PARAM"), Space]
    public Text txtLevel;
    public Text txtTime;
    public Text txtTotal;
    public Text txtRewardAds;
    public Text txtRewardTitle;
    public Text txtTotalCoinFlip;

    public Transform Stars;
    public Transform Golds;

    public GameObject btnNext;

    public Button AdsButton;

    public ThemeData themeData;

    public ParticleSystem winParticle;


    [SerializeField] SoundFx completeSound;
    [SerializeField] SoundFx starSound;

    private bool adsFree = false;

    protected override void ShowStart()
    {
        base.ShowStart();
        AdsButton.interactable = true;
        SetLevelData();
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();
        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_End_Level, false);
        }
        StartCoroutine(DoStarAnimation());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DOTween.CompleteAll();
        if (winParticle)
        {
            winParticle.gameObject.SetActive(false);
        }
    }

    protected override void HideStart()
    {
        DataManager.Instance.Save();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_End_Level);
        }
        base.HideStart();
    }

    private readonly float flipBase = 50f;
    private readonly float coinBase = 10f;
    private float goldInLevel = 0;
    private LevelModel CurrentLevel;
    public void SetLevelData()
    {
        ResetData();
        goldInLevel = flipBase * LevelController.Instance.FlipCount + coinBase * LevelController.Instance.CoinCollected;
        CurrentLevel = DataManager.Instance.CurrentLevel;

        if (winParticle)
        {
            winParticle.gameObject.SetActive(true);
        }
        adsFree = _player.completeCount <= 0;



        levelReward = CurrentLevel.BaseGold;
        star = CurrentLevel.Star;

        FirebaseLogger.LevelSuccess(_player.currentMode, _player.currentLevel,
            _player.currentCar, star, LevelController.Instance.timeLevel);

        if (star == 3)
        {
            timeReward = levelReward / 2f;
        }
        else if (star == 2)
        {
            timeReward = levelReward / 2f - 100f;
        }
        else
        {
            timeReward = Math.Max(levelReward / 2f - 200f, 0);
        }

        if (_player.completeCount > 0 && _player.completeCount % 5 == 0)
        {
            multiplyAds = 5;
        }
        else multiplyAds = 3;

        totalReward = levelReward + timeReward + goldInLevel;
        txtRewardTitle.text = "X" + multiplyAds + " REWARD";
        txtRewardAds.text = (multiplyAds * totalReward).ToString();

        DataManager.Instance.SetLevelData();

    }

    void ResetData()
    {
        btnNext.SetActive(false);
        AdsButton.GetComponent<Animator>().enabled = false;

        txtLevel.text = "0";
        txtTime.text = "0";
        txtTotal.text = "0";
        txtTotalCoinFlip.text = "0";

        foreach (Transform _star in Stars)
        {
            _star.GetChild(0).gameObject.SetActive(false);
        }
    }

    IEnumerator ShowButton()
    {
        foreach (var item in themeData.listthemes)
        {
            if (!item.Unlock && item.Price == _player.currentLevel && item.Price == _player.completeCount + 1)
            {
                RouteController.Instance.CurrentTheme = item._name;
                UIToast.Instance.Toast("Congratulation. You unlock new theme");
            }
        }
        yield return new WaitForSeconds(1f);
        btnNext.SetActive(true);
    }

    private readonly float timeWaitStar = 0.5f;
    private readonly float timeAnim = 0.4f;
    public IEnumerator DoStarAnimation()
    {
        if (completeSound)
            completeSound.PlaySound();

        yield return new WaitForSeconds(timeWaitStar);
        int i = 0;
        foreach (Transform _star in Stars)
        {
            if (i < star)
            {
                if (starSound) starSound.PlaySound();
                _star.GetChild(0).gameObject.SetActive(true);
                _star.GetChild(0).localScale = Vector3.zero;
                _star.GetChild(0).localPosition = Vector3.zero;
                _star.GetChild(0).DOScale(Vector3.one, timeAnim).SetEase(Ease.OutBack);
                _star.GetChild(0).DOLocalMove(Vector3.zero, timeAnim).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(timeAnim);
            }
            i++;
        }
        yield return GoldAnimation();
        AdsButton.GetComponent<Animator>().enabled = !adsFree;
        StartCoroutine(ShowButton());

    }

    IEnumerator GoldAnimation()
    {
        int i = 0;
        float targetValue = 0;
        foreach (Transform goldReward in Golds)
        {
            if (i == 0)
            {
                targetValue = goldInLevel;
            }
            else if (i == 1)
            {
                targetValue = levelReward;
            }
            else if (i == 2)
            {
                targetValue = timeReward;
            }
            else if (i == 3)
            {
                targetValue = totalReward;
            }
            yield return goldReward.GetComponent<ScoreText>().StartAnim((int)targetValue, true);

            i++;
        }
        yield return null;
    }


    private readonly int levelRate = 5;
    public void OnNext()
    {
        FirebaseLogger.ClickButton("Complete_Next", _player.currentLevel);

        RouteController.Instance.goldWinLevel += (int)totalReward;
        _player.currentGold += (int)totalReward;

        if (!_player.showRate && _player.currentLevel == levelRate + 1)
        {
            UIManager.Instance.ShowPopup<UIRate>();
        }
        else if ((_player.currentLevel - 1) % 2 == 0)
        {
            UIManager.Instance.ShowPopup<UILuckyBox>();
        }
        else
        {
            RouteController.Instance.StartLevel();
        }
    }


    public void OnX3Reward()
    {
        UIToast.Instance.Toast("Thanks for watching video. You got " + multiplyAds * totalReward + "coins.");
        RouteController.Instance.goldWinLevel += multiplyAds * totalReward;
        _player.currentGold += (int)(multiplyAds * totalReward);
        //txtTotal.text = totalReward * multiplyAds + "";
        StartCoroutine(Golds.GetChild(3).GetComponent<ScoreText>().StartAnim((int)totalReward * multiplyAds, true));
        AdsButton.interactable = false;

        //RouteController.Instance.PrepareGarage();
    }

    public void OnClickX3()
    {
        AdsController.Instance.ShowReward(ADTYPE.CLAIM_X3, () =>
        {
            OnX3Reward();
        });
    }


}
