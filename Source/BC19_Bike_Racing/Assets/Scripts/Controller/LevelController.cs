using Bacon;
using Kamgam.BikeAndCharacter25D;
using System;
using System.Collections;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public static Action OnLevelLoaded;

    public static Action<int> OnCarLoaded;

    public static Action<int> OnColorSelect;

    [HideInInspector] public float timeLevel;

    private GameObject CarObject;

    private PlayerModel _player;

    [HideInInspector] public bool isStart, isComplete, isPause, isFail;

    private int currentRevive;
    [HideInInspector] public FAIL_TYPE failType;

    [SerializeField] private UIIngame levelUI;

    [HideInInspector] public LevelModel CurrentLevel;

    [SerializeField] SoundFx timeupSound;
    [SerializeField] SoundFx engineStartSound;

    private Vector3 savePos;

    [HideInInspector] public int failCount = 0;
    [HideInInspector] public int FlipCount = 0;
    [HideInInspector] public int CoinCollected = 0;

    public bool IsPlay
    {
        get
        {
            return isStart && !isComplete && !isPause && !isFail;
        }
    }

    private void Awake()
    {
        Instance = this;
        if (levelUI == null)
        {
            levelUI = UIManager.Instance.GetUI<UIIngame>();
        }
    }

    private void ResetVariable()
    {
        currentRevive = 0;
        isStart = false;
        isFail = false;
        isComplete = false;
        isPause = false;
        timeLevel = 0;
        FlipCount = 0;
        CoinCollected = 0;
    }

    protected BikeAndCharacter bikeAndCharacter;
    protected Level level;
    protected void RespawnBike(Vector3 pos)
    {
        savePos = pos;
        ClearBike();

        // create new  bike
        CarObject = Instantiate(DataManager.Instance.CurrentCar.Source, transform);
        bikeAndCharacter = CarObject.GetComponent<BikeAndCharacter>();
        bikeAndCharacter.transform.position = savePos + 1.5f * Vector3.up;
        bikeAndCharacter.gameObject.SetActive(true);

        bikeAndCharacter.TouchInput = levelUI;
        bikeAndCharacter.HandleUserInput = true;
        bikeAndCharacter.Bike.IsBraking = true;
        bikeAndCharacter.Bike.Crashed = false;

        // inform cameraman
        level.GetCamera().gameObject.SetActive(true);
        level.SetupCamera(bikeAndCharacter.Character.TorsoBody);
    }

    public void ClearBike()
    {
        if (bikeAndCharacter != null)
        {
            Destroy(bikeAndCharacter.gameObject);
            bikeAndCharacter = null;
        }
    }

    public void RespawnBot(Vector3 pos)
    {
        Debug.Log("============== spawn bot");
    }

    public IEnumerator StartLevel()
    {
        _player = DataManager.Instance._player;
        if (CurrentLevel != null && CurrentLevel.ID != _player.currentLevel)
        {
            failCount = 0;
        }
        CurrentLevel = DataManager.Instance.CurrentLevel;

        ResetVariable();

        level = FindObjectOfType<Level>();
        level.terrainData = CurrentLevel.TerrainData;

        yield return level.GenMapCoroutine();
        level.GetGoal().OnGoalReached += OnGoalReached;
        level.GenMesh(RouteController.Instance.CurrentTheme, true);
        do
        {
            yield return new WaitForSeconds(0.1f);
        }
        while (!level.IsReady());

        RespawnBike(level.GetBikeSpawnPosition().position);
        if (_player.currentMode == 3)
        {
            RespawnBot(level.GetBikeSpawnPosition().position);
        }
        OnLevelLoaded?.Invoke();

        UIManager.Instance.ShowPopup<UIIngame>((t) => levelUI.ResetUI());
        FirebaseLogger.LevelStart(_player.currentMode, _player.currentLevel, _player.currentCar);

        if (engineStartSound) engineStartSound.PlaySound();

        yield return new WaitForSeconds(0.2f);
        isStart = true;

        AdsController.Instance.LoadMrec(MrecName.Mrec_End_Level);
    }

    public void ZoomMap()
    {
        isPause = !isPause;
        PauseGame(isPause);
        level.ZoomeMap(!isPause);
    }

    public void SavePoint()
    {
        if (!isStart || isPause || isComplete || isFail) return;
        savePos = bikeAndCharacter.GetPosition();
    }

    private void OnGoalReached()
    {
        if (!isStart || isPause || isComplete || isFail) return;

        isComplete = true;
        failCount = 0;
        if (bikeAndCharacter)
        {
            bikeAndCharacter.HandleUserInput = false;
            bikeAndCharacter.Bike.IsBraking = true;
        }

        levelUI.CompleteLevel();
    }

    public void PauseGame(bool active = true)
    {
        isPause = active;
        if (bikeAndCharacter)
        {
            bikeAndCharacter.HandleUserInput = !active;
            bikeAndCharacter.Bike.IsBraking = active;
        }
    }

    private readonly float TimeUpExtra = 30f;
    public void ContinueGame()
    {
        if (failType == FAIL_TYPE.TIME_UP)
        {
            timeLevel -= TimeUpExtra;
            isPause = false;
            UIManager.Instance.ShowPopup<UIIngame>();
        }
        else if (failType == FAIL_TYPE.CRASH)
        {
            isFail = false;
            RespawnBike(savePos);
            UIManager.Instance.ShowPopup<UIIngame>();
        }
    }

    public bool UseTimeUp = false;
    private readonly int MaxRevive = 3;
    public float TimeUpValue = 45f;
    private void FixedUpdate()
    {
        if (isStart && !isComplete && !isPause && !isFail)
        {
            timeLevel += Time.fixedDeltaTime;
            if (UseTimeUp && timeLevel >= TimeUpValue)
            {
                currentRevive++;
                failType = FAIL_TYPE.TIME_UP;
                if (currentRevive <= MaxRevive)
                {
                    isPause = true;
                    if (timeupSound) timeupSound.PlaySound();
                    UIManager.Instance.ShowPopup<UITimeUp>(-1f);
                }
                else
                {
                    isFail = true;
                    levelUI.FailLevel();
                }
            }
        }
    }

    public IEnumerator CrashCar()
    {
        if (isStart && !isPause && !isFail && !isComplete)
        {
            Vibration.Vibrate(200);
            bikeAndCharacter.Bike.Crashed = true;
            _player.failCount++;
            isFail = true;
            failType = FAIL_TYPE.CRASH;
            levelUI.FailLevel();
        }
        yield return null;
    }

    public void CompleteGame()
    {
        StartCoroutine(CompleteCoroutine());
    }

    private IEnumerator CompleteCoroutine()
    {
        CurrentLevel.Time = timeLevel;
        CurrentLevel.Star = CalculateStar();
        //Play Audio, wait for sound and parking icon animation
        yield return new WaitForSeconds(0.6f);
        GetComponent<LoadingInter>().ShowInter("Level_Complete", () => UIManager.Instance.ShowPopup<UIComplete>());
    }


    public void FailGame()
    {
        StartCoroutine(FailCoroutine());
    }

    private IEnumerator FailCoroutine()
    {
        failCount++;
        UIManager.Instance.HidePopup<UIIngame>();

        //Play Audio, wait for sound and parking icon animation
        yield return new WaitForSeconds(0.6f);

        if (_player.currentLevel > 1 && failCount > 1)
        {
            UIManager.Instance.ShowPopup<UISkipLevel>();
        }
        else
        {
            ShowAdsFail();
        }
    }

    public void ShowAdsFail()
    {
        GetComponent<LoadingInter>().ShowInter("Level_Fail", () =>
        {
            if (_player.currentLevel == 1)
            {
                FirebaseLogger.LevelRevive(_player.currentMode, _player.currentLevel, _player.currentCar);
                ContinueGame();
            }
            else
            {
                UIManager.Instance.ShowPopup<UIFail>();
            }
        });
    }

    private const float time3Star = 5f;
    private const float time2Star = 20f;
    public int CalculateStar()
    {
        PlayerModel _player = DataManager.Instance._player;
        if (UseTimeUp)
        {
            if (Instance.timeLevel <= TimeUpValue / 2f)
            {
                return 3;
            }
            else if (Instance.timeLevel <= 2f * TimeUpValue / 3f)
            {
                return 2;
            }
            else return 1;
        }
        else
        {
            if (Instance.timeLevel <= DataManager.Instance.CurrentLevel.BaseTime + time3Star)
            {
                return 3;
            }
            else if (Instance.timeLevel <= DataManager.Instance.CurrentLevel.BaseTime + time2Star)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }
}


public enum FAIL_TYPE
{
    CRASH, TIME_UP, LAZE, BOMB
}