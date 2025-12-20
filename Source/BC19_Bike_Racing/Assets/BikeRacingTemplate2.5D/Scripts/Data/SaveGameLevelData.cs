namespace Kamgam.BikeRacing25D
{
    [System.Serializable]
    public class SaveGameLevelData
    {
        public int LevelNr;
        public bool Completed;

        public SaveGameLevelData(int levelNr, bool completed)
        {
            LevelNr = levelNr;
            Completed = completed;
        }
    }
}
