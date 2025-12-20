using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    public class UIMainMenu : UIBaseFade
    {
        // Dependencies
        public SaveGameProvider SaveGameProvider;
        public LevelDataProvider LevelDataProvider;

        // Callbacks
        public Action<int> OnLoadLevel;

        // Scene Refs
        public TMPro.TextMeshProUGUI VersionTf;

        public void OnEnable()
        {
            var levelButtons = GetComponentsInChildren<LevelButton>();
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelNr = i + 1;
                var levelData = LevelDataProvider.GetLevelData(levelNr);
                var unlocked = SaveGameProvider.SaveGame.IsLevelUnlocked(levelNr);
                levelButtons[i].SetData(levelData, unlocked);
            }

            VersionTf.text = "Version " + Main.GameVersion;
        }

        public override void OnClick(GameObject obj, BaseEventData eventData)
        {
            var levelButton = obj.GetComponent<LevelButton>();
            OnLoadLevel?.Invoke(levelButton.LevelNr);
        }
    }
}
