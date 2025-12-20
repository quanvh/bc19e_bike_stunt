using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "BikeRacingTemplate2.5D/LevelData", order = 2)]
    public class LevelData : ScriptableObject
    {
        public int LevelNr;
		public string SceneName;
        public float TimeToBeat;
    }
}

