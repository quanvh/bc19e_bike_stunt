using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class MoreGameButton : MonoBehaviour
    {
        private Button _button;

        [SerializeField]
        private CustomModel gameModel;

        public Text gameName;
        public Image gameThumb;


        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnMoreGame());
        }


        public void SetData(CustomModel customModel)
        {
            gameModel = customModel;
            gameName.text = gameModel.Name;
            gameThumb.sprite = gameModel.Thumb;
        }


        private void OnMoreGame()
        {
#if UNITY_IOS
            if (gameModel.Price > 0)
            {
                Application.OpenURL("http://apps.apple.com/app/id" + gameModel.Price);
            }
#else
            if (!string.IsNullOrEmpty(gameModel.Description))
            {
                Application.OpenURL("http://play.google.com/store/apps/details?id=" + gameModel.Description);
            }
#endif
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}