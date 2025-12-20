using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    public class UILuckyBoard : IGacha
    {
        public GameObject BoardItem;

        public int NumInRow = 4;
        public float ItemSpace = 10;

        public Transform BoardContent;

        private float itemWidth, itemHeight;

        public bool randomReward;

        private int gachaAward;

        public override void InitGacha()
        {
            lstReward = rewards.listItems;

            itemWidth = BoardItem.GetComponent<RectTransform>().sizeDelta.x;
            itemHeight = BoardItem.GetComponent<RectTransform>().sizeDelta.y;


            for (int i = 0; i < NumInRow; i++)
            {
                GameObject _item = Instantiate(BoardItem, BoardContent);
                float itemX = (itemWidth + ItemSpace) * (0.5f + i - NumInRow / 2);
                _item.transform.localPosition = new Vector2(itemX, itemHeight + ItemSpace);

                if (lstReward.Count > i)
                {
                    SetBoardItem(lstReward[i], _item.transform);
                }
            }

            GameObject _item2 = Instantiate(BoardItem, BoardContent);
            float itemX2 = (itemWidth + ItemSpace) * (NumInRow / 2 - 0.5f);
            _item2.transform.localPosition = new Vector2(itemX2, 0);
            if (lstReward.Count > NumInRow)
            {
                SetBoardItem(lstReward[NumInRow], _item2.transform);
            }

            for (int i = 0; i < NumInRow; i++)
            {
                GameObject _item = Instantiate(BoardItem, BoardContent);
                float itemX = (itemWidth + ItemSpace) * (NumInRow / 2 - 0.5f - i);
                _item.transform.localPosition = new Vector2(itemX, -(itemHeight + ItemSpace));

                if (lstReward.Count > NumInRow + 1 + i)
                {
                    SetBoardItem(lstReward[NumInRow + 1 + i], _item.transform);
                }
            }

            _item2 = Instantiate(BoardItem, BoardContent);
            itemX2 = (itemWidth + ItemSpace) * (0.5f - NumInRow / 2);
            _item2.transform.localPosition = new Vector2(itemX2, 0);
            if (lstReward.Count > 2 * NumInRow + 1)
            {
                SetBoardItem(lstReward[2 * NumInRow + 1], _item2.transform);
            }
        }

        public override void StartSpin()
        {
            if (randomReward)
            {
                gachaAward = Random.Range(0, BoardContent.childCount);
            }
            else
            {
                gachaAward = 1;
            }

            StartCoroutine(SpinCoroutine(4, gachaAward));
        }

        IEnumerator SpinCoroutine(int round, int result)
        {
            var currentRound = 0;
            var currentResult = 0;

            while (currentRound < round || currentResult != result)
            {
                if (currentResult == 0)
                    currentRound++;
                for (int i = 0; i < BoardContent.childCount; i++)
                {
                    if (currentResult == i)
                        SetFocus(true, BoardContent.GetChild(i));
                    else
                        SetFocus(false, BoardContent.GetChild(i));
                }

                currentResult++;
                if (currentResult > BoardContent.childCount - 1)
                    currentResult = 0;
                yield return new WaitForSecondsRealtime(0.05f);
            }

            // SPIN 1 STEP TO THE RESULT
            for (int i = 0; i < BoardContent.childCount; i++)
            {
                if (result == i)
                    SetFocus(true, BoardContent.GetChild(i));
                else
                    SetFocus(false, BoardContent.GetChild(i));
            }
            OnSpinComplete?.Invoke(lstReward[gachaAward]);
        }

        private void SetBoardItem(RewardModel _reward, Transform _root)
        {
            _root.GetChild(2).GetComponent<Text>().text = _reward.Value.ToString();
            _root.GetChild(1).GetComponent<Image>().sprite = _reward.Thumb;
            _root.GetChild(1).GetComponent<Image>().SetNativeSize();
        }

        private readonly Color focusColor = new Color(255f / 255f, 198f / 255f, 0f, 200f / 255f);
        private readonly Color normalColor = new Color(0, 0, 0, 100f / 255f);
        public void SetFocus(bool isFocus, Transform _root)
        {
            if (isFocus)
                _root.GetComponent<Image>().color = focusColor;
            else _root.GetComponent<Image>().color = normalColor;
        }

        public override int FixedReward()
        {
            throw new System.NotImplementedException();
        }
    }
}