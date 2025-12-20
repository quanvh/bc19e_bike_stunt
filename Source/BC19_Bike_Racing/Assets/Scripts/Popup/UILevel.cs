using Bacon;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILevel : PopupBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("POPUP PARAM"), Space]
    [SerializeField] private GameObject LevelPrefab;
    [SerializeField] private Transform levelContent;
    [SerializeField] private Transform btnRace;


    private readonly float startX = 180;
    private readonly float startY = -100;
    private readonly float sizeX = 220f;
    private readonly float sizeY = 256f;
    private int currentPage;
    private int maxPage;

    private float clickPosX = 0;
    private readonly float deltaMoveX = 1100f;
    private readonly float timeMove = 0.35f;
    private readonly float timeMoveStop = .15f;
    private bool isMoved;


    private float btnRacePos;

    private LevelMode CurrentMode;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void ShowStart()
    {
        base.ShowStart();

        if (btnRace)
        {
            btnRacePos = btnRace.localPosition.x;
            btnRace.DOLocalMoveX(btnRacePos + 600, 0);
        }
        if (DataManager.Instance && DataManager.Instance.dataLoaded)
        {
            currentPage = (_player.currentLevel - 1) / maxItem + 1;
            CurrentMode = DataManager.Instance.GetMode(_player.currentMode);
            maxPage = 1 + (CurrentMode.levels.Count - 1) / maxItem;
            LoadData();
            foreach (Transform level in levelContent)
            {
                level.localScale = new Vector3(0.1f, 0f, 1f);
            }
        }
    }

    protected override void ShowCompleted()
    {
        base.ShowCompleted();

        if (AdsController.Instance)
        {
            AdsController.Instance.ShowMrec(MrecName.Mrec_Start_Level);
        }
        ShowData();
    }

    protected override void HideStart()
    {
        base.HideStart();
        if (AdsController.Instance)
        {
            AdsController.Instance.HideMrec(MrecName.Mrec_Start_Level);
        }
    }

    private readonly int maxItem = 9;
    private void LoadData()
    {
        for (int i = 0; i < maxItem; i++)
        {
            if (levelContent.childCount > i)
            {
                if (maxItem * (currentPage - 1) + i + 1 > CurrentMode.levels.Count)
                {
                    levelContent.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    levelContent.GetChild(i).gameObject.SetActive(true);
                    levelContent.GetChild(i).GetComponent<LevelItem>().SetLevelData(6 * (currentPage - 1) + i + 1);
                }
            }
            else if (maxItem * (currentPage - 1) + i < CurrentMode.levels.Count)
            {
                GameObject item = Instantiate(LevelPrefab, levelContent);
                item.transform.localPosition = new Vector2(startX + sizeX * (i % 3), startY - (i / 3) * sizeY);
                item.GetComponent<LevelItem>().SetLevelData(maxItem * (currentPage - 1) + i + 1);
            }
        }
    }

    private readonly float timeAnim = 0.25f;
    private readonly float timeDelay = 0.1f;

    private void ShowData()
    {
        int i = 0;
        foreach (Transform level in levelContent)
        {
            i++;
            level.DOScale(1f, timeAnim).SetEase(Ease.OutCubic).SetDelay(timeDelay * i);
        }

        if (btnRace)
        {
            btnRace.DOLocalMoveX(btnRacePos, timeAnim).SetEase(Ease.OutBack).SetDelay(timeDelay * i + timeAnim);
        }
    }


    public void OnSelectLevel()
    {
        RouteController.Instance.StartLevel();
    }


    public void NextPage(bool sound = false)
    {
        if (isMoved) return;
        isMoved = true;

        if (currentPage == maxPage)
        {
            levelContent.DOLocalMoveX(-100f, timeMoveStop).OnComplete(() =>
            {
                levelContent.DOLocalMoveX(0, timeMoveStop).SetEase(Ease.OutBack).OnComplete(() => isMoved = false);
            });
        }
        else
        {
            levelContent.DOLocalMoveX(-deltaMoveX, timeMove).OnComplete(() =>
            {
                levelContent.DOLocalMoveX(deltaMoveX, 0f).OnComplete(() =>
                {
                    currentPage++;
                    LoadData();
                    //set new level data
                    levelContent.DOLocalMoveX(0, timeMove).OnComplete(() => isMoved = false); ;
                });
            });
        }
    }

    public void PrevPage(bool sound = false)
    {
        if (isMoved) return;
        isMoved = true;

        if (currentPage == 1)
        {
            levelContent.DOLocalMoveX(100f, timeMoveStop).OnComplete(() =>
            {
                levelContent.DOLocalMoveX(0, timeMoveStop).SetEase(Ease.OutBack).OnComplete(() => isMoved = false); ;
            });
        }
        else
        {
            levelContent.DOLocalMoveX(deltaMoveX, timeMove).OnComplete(() =>
            {
                levelContent.DOLocalMoveX(-deltaMoveX, 0f).OnComplete(() =>
                {
                    //set new level data
                    currentPage--;
                    LoadData();
                    levelContent.DOLocalMoveX(0, timeMove).OnComplete(() => isMoved = false); ;
                });
            });
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        clickPosX = eventData.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMoved) return;

        //check currrent page, face move when limit page
        if (eventData.position.x - clickPosX > 15f)
        {
            PrevPage();
        }
        else if (eventData.position.x - clickPosX < -15f)
        {
            NextPage();
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isMoved = false;
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPopup<UISelectMode>();
    }
}
