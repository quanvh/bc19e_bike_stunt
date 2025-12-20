using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Bacon
{
    public class UIMoreGame : PopupBase
    {
        [Header("POPUP PARAM")]
        public Transform content;
        public CustomData gameData;
        public GameObject gamePrefab;

        protected override void Awake()
        {
            base.Awake();

            foreach (CustomModel gameModel in gameData.listItems)
            {
                bool isValid = false;
#if UNITY_IOS
                if (gameModel.Price > 0) isValid = true;
#else
                if (!string.IsNullOrEmpty(gameModel.Description)) isValid = true;
#endif
                if (isValid)
                {
                    var _moreGame = Instantiate(gamePrefab, content);
                    _moreGame.GetComponent<MoreGameButton>().SetData(gameModel);
                }
                else continue;
            }
        }

        protected override void ShowStart()
        {
            base.ShowStart();
            foreach (Transform level in content)
            {
                level.localScale = new Vector3(0.1f, 0f, 1f);
            }
        }

        protected override void ShowCompleted()
        {
            base.ShowCompleted();
            ShowData();
        }

        protected override void HideStart()
        {
            base.HideStart();
        }


        public void OnClose()
        {
            Hide();
        }

        private void ShowData()
        {
            int i = 0;
            foreach (Transform level in content)
            {
                i++;
                level.DOScale(1f, 0.2f).SetEase(Ease.OutCubic).SetDelay(0.15f * i);
            }
        }
    }
}