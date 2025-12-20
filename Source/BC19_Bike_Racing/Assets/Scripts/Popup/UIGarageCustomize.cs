using Bacon;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIGarageCustomize : MonoBehaviour
{
    public GameObject UIItemPrefab;

    public List<GameObject> UIListItem;

    public List<Transform> UIListContent;

    public Transform Buttons;

    public CUSTOMIZE_SELECT currentState;

    private int currentCarId;

    private int currentItemId;

    private PlayerModel _player;

    private readonly float duration = 0.3f;


    private void OnEnable()
    {
        if (GarageController.Instance)
        {
            GarageController.Instance.OnCarLoaded += OnCarLoaded;
            GarageController.Instance.OnStateChange += OnChangeState;
            GarageController.Instance.OnCustomLoaded += OnCustomLoaded;
        }
        if (DataManager.Instance && DataManager.Instance.dataLoaded)
        {
            _player = DataManager.Instance._player;
            currentCarId = _player.currentCar;
            currentState = CUSTOMIZE_SELECT.Car;
            SelectCustomize(currentState);
        }
    }

    private void OnDisable()
    {
        if (GarageController.Instance)
        {
            GarageController.Instance.OnCarLoaded -= OnCarLoaded;
            GarageController.Instance.OnStateChange -= OnChangeState;
            GarageController.Instance.OnCustomLoaded -= OnCustomLoaded;
        }
    }

    public void OnChangeState(CUSTOMIZE_SELECT newState, bool force = false)
    {
        if (force || currentState != newState)
        {
            currentState = newState;
            UnityMainThreadDispatcher.Enqueue(() => SelectCustomize(newState));
        }
    }

    public void OnCarLoaded(int carId)
    {
        currentCarId = carId;
        foreach (Transform item in UIListContent[0])
        {
            item.GetComponent<UIListItem>().SetActiveCar(carId);
        }
        StartCoroutine(ScrollViewFocus.FocusOnItemCoroutine(UIListItem[0].GetComponent<ScrollRect>(),
                UIListContent[0].GetChild(carId - 1)));
    }

    public void OnCustomLoaded(CustomModel _model)
    {
        if ((_model.Type == CUSTOM_TYPE.CHARACTER && currentState == CUSTOMIZE_SELECT.Char)
            || (_model.Type == CUSTOM_TYPE.HELMET && currentState == CUSTOMIZE_SELECT.Helmet)
            || (_model.Type == CUSTOM_TYPE.WHEEL && currentState == CUSTOMIZE_SELECT.Wheel))
        {
            currentItemId = _model.ID;
        }

        foreach (Transform item in UIListContent[(int)currentState - 1])
        {
            item.GetComponent<UIListItem>().SetActiveItem(currentItemId);
        }
        if (UIListItem.Count >= (int)currentState && UIListContent.Count >= (int)currentState
            && UIListContent[(int)currentState - 1].childCount >= _model.ID)
        {
            StartCoroutine(ScrollViewFocus.FocusOnItemCoroutine(UIListItem[(int)currentState - 1].GetComponent<ScrollRect>(),
                UIListContent[(int)currentState - 1].GetChild(_model.ID - 1)));
        }
    }

    public void ChangeGarageState(int newState)
    {
        GarageController.Instance.ChangeState((CUSTOMIZE_SELECT)newState);
    }

    private Transform ListActive;
    public void SelectCustomize(CUSTOMIZE_SELECT _index)
    {
        int buttonIndex = (int)_index - 1;
        ListActive = UIListContent[buttonIndex];
        foreach (Transform t in Buttons)
        {
            t.GetComponent<UIButtonCustomize>().ActiveButton(t.GetSiblingIndex() == buttonIndex);
        }
        foreach (var t in UIListItem)
        {
            t.SetActive(UIListItem.IndexOf(t) == buttonIndex);
        }

        if (_index == CUSTOMIZE_SELECT.Car)
        {
            for (int i = 0; i < DataManager.Instance.TotalCar; i++)
            {
                float delayTime = i * 0.1f;

                if (ListActive.childCount > i)
                {
                    ListActive.GetChild(i).GetComponent<UIListItem>().SetActiveCar(currentCarId);
                }
                else
                {
                    GameObject _carGarage = Instantiate(UIItemPrefab, ListActive);
                    _carGarage.GetComponent<UIListItem>().SetCarData(i + 1);
                }

                ListActive.GetChild(i).localScale = Vector3.zero;
                ListActive.GetChild(i).DOScale(Vector3.one, duration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(delayTime).OnComplete(() =>
                    {
                        if (i == DataManager.Instance.TotalCar - 1)
                        {
                            FocusOnItem(buttonIndex, _player.currentCar);
                        }
                    });
            }

        }
        else if (_index == CUSTOMIZE_SELECT.Char)
        {
            for (int i = 0; i < DataManager.Instance.TotalChar; i++)
            {
                float delayTime = i * 0.1f;
                var _item = DataManager.Instance.GetChar(i + 1);
                if (ListActive.childCount > i)
                {
                    ListActive.GetChild(i).GetComponent<UIListItem>().SetActiveItem(currentItemId);
                }
                else
                {
                    GameObject _carGarage = Instantiate(UIItemPrefab, ListActive);
                    _carGarage.GetComponent<UIListItem>().SetCustomData(_item, _player.currentChar);
                }

                ListActive.GetChild(i).localScale = Vector3.zero;
                ListActive.GetChild(i).DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack)
                .SetDelay(delayTime).OnComplete(() =>
                {
                    if (i == DataManager.Instance.TotalChar - 1)
                    {
                        FocusOnItem(buttonIndex, _player.currentChar);
                    }
                });

            }
            FocusOnItem(buttonIndex, _player.currentChar);
        }
        else if (_index == CUSTOMIZE_SELECT.Helmet)
        {
            for (int i = 0; i < DataManager.Instance.TotalHelmet; i++)
            {
                float delayTime = i * 0.1f;

                var _item = DataManager.Instance.GetHelmet(i + 1);

                if (ListActive.childCount > i)
                {
                    ListActive.GetChild(i).GetComponent<UIListItem>().SetActiveItem(currentItemId);
                }
                else
                {
                    GameObject _carGarage = Instantiate(UIItemPrefab, ListActive);
                    _carGarage.GetComponent<UIListItem>().SetCustomData(_item, _player.currentHelmet);
                }

                ListActive.GetChild(i).localScale = Vector3.zero;
                ListActive.GetChild(i).DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack)
                .SetDelay(delayTime).OnComplete(() =>
                {
                    if (i == DataManager.Instance.TotalHelmet - 1)
                    {
                        FocusOnItem(buttonIndex, _player.currentHelmet);
                    }
                });

            }
        }
        else if (_index == CUSTOMIZE_SELECT.Wheel)
        {
            for (int i = 0; i < DataManager.Instance.TotalWheel; i++)
            {
                float delayTime = i * 0.1f;
                var _item = DataManager.Instance.GetWheel(i + 1);

                if (ListActive.childCount > i)
                {
                    ListActive.GetChild(i).GetComponent<UIListItem>().SetActiveItem(currentItemId);
                }
                else
                {
                    GameObject _carGarage = Instantiate(UIItemPrefab, ListActive);
                    _carGarage.GetComponent<UIListItem>().SetCustomData(_item, _player.currentWheel);

                }

                ListActive.GetChild(i).localScale = Vector3.zero;
                ListActive.GetChild(i).DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack)
                .SetDelay(delayTime).OnComplete(() =>
                {
                    if (i == DataManager.Instance.TotalWheel - 1)
                    {
                        FocusOnItem(buttonIndex, _player.currentWheel);
                    }
                });
            }
        }
    }


    public void FocusOnItem(int _indexList, int _indexItem)
    {
        if (UIListItem.Count > _indexList && UIListContent.Count > _indexList
            && UIListContent[_indexList].childCount >= _indexItem)
        {
            StartCoroutine(ScrollViewFocus.FocusOnItemCoroutine(UIListItem[_indexList].GetComponent<ScrollRect>(),
                        UIListContent[_indexList].GetChild(_indexItem - 1)));
        }
    }
}


public enum CUSTOMIZE_SELECT
{
    None = 0,
    Car = 1,
    Char = 2,
    Helmet = 3,
    Wheel = 4,
}