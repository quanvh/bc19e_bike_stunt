using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bacon
{
    public class ButtonInter : MonoBehaviour
    {
        private Button _button;

        [SerializeField] private bool startLevel = false;

        [SerializeField, Space] UnityEvent OnInterSuccess;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(ShowInter);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(ShowInter);
            }
        }


        private void ShowInter()
        {
            if (LoadingInter.Instance)
            {
                LoadingInter.Instance.ShowInter(_button.name, () => OnInterSuccess?.Invoke(), startLevel);
            }
            else OnInterSuccess?.Invoke();
        }
    }
}
