using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bacon
{
    public class HideUI : MonoBehaviour
    {
        [SerializeField] private KeyCode HideKey = KeyCode.P;

        bool currentState = true;


        void Update()
        {
            if (Input.GetKeyDown(HideKey))
            {
                if (currentState)
                {
                    foreach (var img in GetComponentsInChildren<Image>())
                    {
                        if (img != null)
                        {
                            img.enabled = false;
                        }
                    }
                    foreach (var text in GetComponentsInChildren<Text>())
                    {
                        if (text != null)
                        {
                            text.enabled = false;
                        }
                    }
                    foreach (var label in GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (label != null)
                        {
                            label.enabled = false;
                        }
                    }
                    currentState = false;
                }
                else
                {
                    foreach (var label in GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (label != null)
                        {
                            label.enabled = true;
                        }
                    }
                    foreach (var text in GetComponentsInChildren<Text>())
                    {
                        if (text != null)
                        {
                            text.enabled = true;
                        }
                    }
                    foreach (var img in GetComponentsInChildren<Image>())
                    {
                        if (img != null)
                        {
                            img.enabled = true;
                        }
                    }

                    currentState = true;

                }
            }

        }
    }
}