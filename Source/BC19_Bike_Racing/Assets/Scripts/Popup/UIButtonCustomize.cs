using UnityEngine;
using UnityEngine.UI;

public class UIButtonCustomize : MonoBehaviour
{
    [SerializeField] Image imgBg;
    [SerializeField] Sprite sprActive, sprInActive;
    [SerializeField] Text txtName;

    public void ActiveButton(bool active)
    {
        imgBg.sprite = active ? sprActive : sprInActive;
        txtName.color = active ? Color.black : Color.white;
    }
}
