using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class ButtonClick : MonoBehaviour
    {
        private Button mButton;

        private void Awake()
        {
            if (TryGetComponent(out mButton))
            {
                mButton.onClick.AddListener(() =>
                {
                    if (AudioController.Instance)
                        AudioController.Instance.Click();
                });
            }
        }
    }
}