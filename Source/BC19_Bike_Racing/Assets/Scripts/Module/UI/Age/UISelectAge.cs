using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UISelectAge : UISplash
    {
        [Header("PARAM"), Space]
        public Text txtAge;

        public Button btnNext, btnPre;


        private readonly int defaultAge = 18;

        private int currentAge;

        private readonly int minAge = 13, maxAge = 35;

        protected override void Awake()
        {
            base.Awake();
            //txtAge.text = defaultAge.ToString();
            currentAge = defaultAge;
        }


        public void SetAge(int offset)
        {
            if (!isSelected)
            {
                StartCoroutine(DelayDone());
            }
            isSelected = true;

            if (ForceAutoClose)
                currentTime = 0;
            else
                txtAutoClose.transform.parent.gameObject.SetActive(false);

            currentAge += offset;
            btnPre.interactable = currentAge >= minAge;
            btnNext.interactable = currentAge <= maxAge;
            if (currentAge < minAge)
            {
                txtAge.text = "<" + minAge;
            }
            else if (currentAge > maxAge)
            {
                txtAge.text = ">" + maxAge;
            }
            else
            {
                txtAge.text = currentAge.ToString();
            }
        }

        public void OnChooseAge()
        {
            PlayerModel _player = DataManager.Instance._player;
            _player.age = currentAge;
            _player.chooseAge = true;
            DataManager.Instance.Save();

            LoadGame();
        }

    }
}