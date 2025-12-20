using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Bacon
{
    [CreateAssetMenu(fileName = "LevelDataAsset", menuName = "DataAsset/LevelDataAsset")]
    public class LevelData : ScriptableObject
    {
        public List<LevelMode> Modes;

        public List<LevelModel> AllLevels
        {
            get
            {
                var list = new List<LevelModel>();
                foreach (var _mode in Modes)
                {
                    list.AddRange(_mode.levels);
                }
                return list;
            }
        }

        public void UpdateFromSaveData(List<LevelModelSave> _saveData)
        {
            foreach (var model in AllLevels)
            {
                model.Unlock = false;
                model.Time = 0;
                model.Coin = 0;
                model.BestTime = 0;
                model.Star = 0;
                model.FirstTime = true;
                if (_saveData != null)
                {
                    var temp = _saveData.FirstOrDefault(x => x.ID == model.ID && x.Mode == model.Mode);
                    if (temp != null)
                    {
                        model.Unlock = temp.Unlock;
                        model.Time = temp.Time;
                        model.Coin = temp.Coin;
                        model.BestTime = temp.BestTime;
                        model.Star = temp.Star;
                        model.FirstTime = temp.FirstTime;
                    }
                }
            }

            foreach (var _mode in Modes)
            {
                foreach (var level in _mode.levels)
                {
                    if (level.Unlock)
                    {
                        _mode.Unlock = true;
                        break;
                    }
                }
            }
        }

        public List<LevelModelSave> SaveDataList
        {
            get => AllLevels?.Select(x => new LevelModelSave
            {
                ID = x.ID,
                Mode = x.Mode,
                Unlock = x.Unlock,
                Time = x.Time,
                Coin = x.Coin,
                BestTime = x.BestTime,
                Star = x.Star,
                FirstTime = x.FirstTime,
            }).ToList();
        }

        public void Reset()
        {
            foreach (var level in AllLevels)
            {
                level.Time = 0;
                level.Coin = 0;
                level.BestTime = 0;
                level.Star = 0;
                level.FirstTime = false;
            }
        }

        public void UnlockAllMode(bool all = false)
        {
            foreach (var mode in Modes)
            {
                foreach (var level in mode.levels.OrderBy(t => t.ID))
                {
                    level.Unlock = true;
                    if (!all) break;
                }
            }
        }

        public void UnlockMode(int modeId, bool all = false)
        {
            foreach (var mode in Modes)
            {
                if (mode.ID == modeId)
                {
                    mode.Unlock = true;
                    foreach (var level in mode.levels.OrderBy(t => t.ID))
                    {
                        level.Unlock = true;
                        if (!all) return;
                    }
                }
            }
        }


        public LevelModel FirstLevel(int modeId)
        {
            return Modes.Where(t => t.Mode == (LEVEL_MODE)modeId).FirstOrDefault()
                .levels.OrderBy(t => t.ID).FirstOrDefault();
        }

        public int FirstMode
        {
            get
            {
                var item = Modes.FirstOrDefault();
                return item != null ? (int)item.Mode : 0;
            }
        }


        public int FirstUnlock
        {
            get
            {
                var item = AllLevels.Where(t => t.Unlock == true).OrderBy(t => t.ID).FirstOrDefault();
                return item != null ? item.ID : 0;
            }
        }

        public LevelModel GetLevel(int levelID)
        {
            return Modes.First()?.GetLevel(levelID);
        }

        public LevelModel GetLevel(int levelID, int modeID)
        {
            return Modes.Where(t => t.Mode == (LEVEL_MODE)modeID).FirstOrDefault()?
                .levels.Where(t => t.ID == levelID).FirstOrDefault();
        }
    }

    [Serializable]
    public class LevelMode
    {
        [FoldoutGroup("@Name", expanded: false), Header("===========    MODE    =============")]
        public int ID;
        [FoldoutGroup("@Name")] public string Name;
        [FoldoutGroup("@Name")] public bool Unlock;
        [FoldoutGroup("@Name")] public Sprite Thumb;
        [FoldoutGroup("@Name")] public int Price;
        [FoldoutGroup("@Name")] public PRICE_TYPE UnlockType;
        [FoldoutGroup("@Name")] public LEVEL_MODE Mode;
        [FoldoutGroup("@Name")] public List<LevelModel> levels;

        public bool IsUnlock
        {
            get
            {
                return Unlock || levels.Where(t => t.Unlock).Any();
            }
        }

        public int UnlockedLevel
        {
            get
            {
                return levels.Where(t => t.Unlock).Count();
            }
        }

        public int TotalLevel
        {
            get
            {
                return levels.Count;
            }
        }

        public LevelModel GetLevel(int levelId)
        {
            return levels.Where(t => t.ID == levelId).FirstOrDefault();
        }

        public LevelModel GetLevelByIndex(int _index)
        {
            if (levels.Count >= _index) return levels[_index - 1];
            else return null;
        }
    }

    [Serializable]
    public class LevelModel : LevelModelBase
    {
        [FoldoutGroup("@Name"), Header("CUSTOM FIELD")]

        public THEME_NAME Theme = THEME_NAME.AUTUMN;

        [FoldoutGroup("@Name")] public Kamgam.Terrain25DLib.TerrainLevelData TerrainData;

    }

    [Serializable]
    public class LevelModelBase
    {
        [FoldoutGroup("@Name", expanded: false), Header("LEVEL FIELD")]
        [FoldoutGroup("@Name")] public int ID;
        [FoldoutGroup("@Name")] public string Name;
        [FoldoutGroup("@Name")] public LEVEL_MODE Mode;
        [FoldoutGroup("@Name")] public GameObject Source;

        [FoldoutGroup("@Name")] public int BaseGold;
        [FoldoutGroup("@Name")] public int BaseTime;
        [FoldoutGroup("@Name")] public bool Unlock;
        [FoldoutGroup("@Name")] public bool Current;

        [FoldoutGroup("@Name")] public string Scene = "Level City";


        [HideInInspector] public float Time;
        [HideInInspector] public int Coin;
        [HideInInspector] public float BestTime;
        [HideInInspector] public int Star;
        [HideInInspector] public bool FirstTime;
    }


    [Serializable]
    public class LevelModelSave
    {
        public LevelModelSave()
        {
            ID = 1;
            Mode = LEVEL_MODE.CLASSIC;
            Unlock = true;
            Time = 0;
            Coin = 0;
            BestTime = 0;
            Star = 0;
            FirstTime = true;
        }
        public int ID;
        public float Time;
        public int Coin;
        public float BestTime;
        public int Star;
        public bool FirstTime;
        public bool Unlock;
        public LEVEL_MODE Mode;
    }

    public enum LEVEL_MODE
    {
        CLASSIC = 0, CANDY = 1, VILLAGE = 2, RACE = 3, HARDCORE = 4
    }
}