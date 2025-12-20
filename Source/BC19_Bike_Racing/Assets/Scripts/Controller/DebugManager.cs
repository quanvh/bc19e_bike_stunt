using Bacon;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    #region DEBUG_MODE
    [Header("Debug_Mode")]
    [SerializeField] private Dropdown ddLevel;
    public GameObject GameAdmin;

    public Action OnAddCurrency;
    public Action OnSelectLevel;

    private PlayerModel _player;

    public static DebugManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (DataManager.Instance)
        {
            _player = DataManager.Instance._player;
        }
        SetDebugMode();
    }

    public void OnCurrency()
    {
        _player.currentGold += 10000;
        OnAddCurrency?.Invoke();
    }

    public void OnLevel()
    {
        _player.currentLevel = ddLevel.value + 1;
        OnSelectLevel?.Invoke();
    }

    public void SetDebugMode()
    {
        if (DataManager.Instance)
        {
            GameAdmin.SetActive(DataManager.Instance.DebugMode);
        }
    }

    public void ChangeDebugMode()
    {
        if (DataManager.Instance)
        {
            DataManager.Instance.DebugMode = !DataManager.Instance.DebugMode;
            SetDebugMode();
        }
    }

    public void UnlockAll()
    {
        if (DataManager.Instance)
        {
            DataManager.Instance.UnlockAllCar();
            DataManager.Instance.UnlockAllLevel();
            UIToast.Instance.Toast("Unlock all game asset");
            DataManager.Instance.Save();
        }
    }
    #endregion
}
