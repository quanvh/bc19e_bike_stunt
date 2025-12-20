#if USE_I2
using I2.Loc;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class UILanguage : UISplash
    {
        [Header("POPUP PARAM"), Space]
        public Transform Languages;

        public GameObject LanguagePrefab;

        public CustomData language = null;

        public Sprite tickOn, tickOff;

        public Sprite bgSelect, bgNormal;


        [Header("NATIVE AD")]

        private int currentLanguage;


        protected override void Awake()
        {
            base.Awake();
            foreach (var _langItem in language.listItems)
            {
                GameObject _lang = Instantiate(LanguagePrefab, Languages);
                _lang.transform.GetComponent<Image>().sprite = bgNormal;
                _lang.transform.GetChild(0).GetComponent<Image>().sprite = _langItem.Thumb;
                _lang.transform.GetChild(1).GetComponent<Text>().text = _langItem.Name;
                //_lang.transform.GetChild(2).GetComponent<Image>().sprite = tickOff;
                _lang.GetComponent<Button>().onClick.AddListener(() => OnSelectLanguage(_langItem.ID - 1));
            }
        }




        public void OnSelectLanguage(int _lang)
        {
            if (!isSelected)
            {
                StartCoroutine(DelayDone());
            }
            isSelected = true;

            if (ForceAutoClose)
                currentTime = 0;
            else
                txtAutoClose.gameObject.SetActive(false);

            currentLanguage = _lang;
            int i = 0;
            foreach (Transform child in Languages)
            {
                child.GetComponent<Image>().sprite = i == currentLanguage ? bgSelect : bgNormal;
                i++;
            }
            FirebaseLogger.RegularEvent("On_select_language");
        }

        public void OnSaveLanguage()
        {
            PlayerModel _player = DataManager.Instance._player;
            _player.chooseLang = true;
            DataManager.Instance.Save();

#if USE_I2
            if (language.listItems.Count > currentLanguage)
            {
                CustomModel _lang = language.listItems[currentLanguage];
                if (_lang != null && LocalizationManager.HasLanguage(_lang.Name))
                {
                    LocalizationManager.CurrentLanguage = _lang.Name;
                }
            }
#endif
            FirebaseLogger.RegularEvent("On_save_language");
            LoadGame();
        }

    }
}