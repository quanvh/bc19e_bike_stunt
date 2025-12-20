using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    public class LevelButton : MonoBehaviour
    {
        public int LevelNr;

        public Button Button;
        public TextMeshProUGUI LevelNrTf;
        public GameObject Img;

        public void SetData(LevelData levelData, bool unlocked)
        {
            LevelNr = levelData.LevelNr;
            LevelNrTf.text = LevelNr.ToString();

            Img.SetActive(!unlocked);
            Button.interactable = unlocked;
        }
    }
}
