using System.Linq;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    [CreateAssetMenu(fileName = "LevelDataProvider", menuName = "BikeRacingTemplate2.5D/LevelDataProvider", order = 2)]
    public class LevelDataProvider : ScriptableObject
    {
        public LevelData[] Levels;

        public LevelData GetLevelData(int levelNr)
        {
            return Levels.FirstOrDefault(l => l.LevelNr == levelNr);
        }

        public bool IsLastLevel(int levelNr)
        {
            if (Levels.Length == 0)
                return true;

            return Levels[Levels.Length - 1].LevelNr == levelNr;
        }
    }
}

