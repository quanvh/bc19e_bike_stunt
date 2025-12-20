using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "SaveGame", menuName = "BikeRacingTemplate2.5D/SaveGame", order = 1)]
    public class SaveGame : ScriptableObject
    {
        #region Data

        public bool Initialized;
        public string GameVersion;
        public int Increment;
        public List<SaveGameLevelData> LevelData = new List<SaveGameLevelData>();

        #endregion



        #region Code

        public void InitAfterLoad()
        {
            // Do stuff like save game upgrades in here.
            Initialized = true;
        }

        public void PrepareForSaving()
        {
            // Do stuff right before saving here.
            GameVersion = Main.GameVersion;
            AddToIncrement(1);
        }

        public SaveGameLevelData GetLevelData(int levelNr)
        {
            if (LevelData != null && LevelData.Count > 0)
            {
                foreach (var level in LevelData)
                {
                    if (level.LevelNr == levelNr)
                        return level;
                }
            }

            var newLevelData = new SaveGameLevelData(levelNr, false);
            LevelData.Add(newLevelData);

            return newLevelData;
        }

        public void SetLevelCompleted(int levelNr)
        {
            var level = GetLevelData(levelNr);
            level.Completed = true;
        }

        public bool IsLevelUnlocked(int levelNr)
        {
            // The very first level is always unlocked.
            if (levelNr <= 1)
                return true;

            // If the previous level (levelNr-1) is completed then the level is playable.
            var prevLevelData = GetLevelData(levelNr - 1);
            return prevLevelData.Completed;
        }

        public void AddToIncrement(int delta)
        {
            if (delta < 0)
            {
                Debug.Log("Decrementing the increment is not allowed!");
                return;
            }
            
            Increment += delta;
        }

        #endregion
    }
}
