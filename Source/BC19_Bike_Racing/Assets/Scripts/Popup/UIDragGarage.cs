using Bacon;
using UnityEngine.EventSystems;

public class UIDragGarage : PopupBase, IDragHandler, IEndDragHandler
{
    private CameraFreeLookGarage freeLookGarage;

    public void OnDrag(PointerEventData eventData)
    {
        freeLookGarage = FindObjectOfType<CameraFreeLookGarage>();
        if (freeLookGarage != null)
        {
            freeLookGarage.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        freeLookGarage = FindObjectOfType<CameraFreeLookGarage>();
        if (freeLookGarage != null)
        {
            freeLookGarage.OnEndDrag(eventData);
        }
    }
}
