using UnityEngine;
using UnityEngine.EventSystems;


namespace BC.Parking
{
    public class BC_UIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private float Sensitivity
        {
            get
            {
                return 20f;
            }
        }

        private float Gravity
        {
            get
            {
                return 5f;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pressing = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressing = true;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            pressing = false;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            pressing = false;
        }


        private void Update()
        {
            if (pressing)
            {
                input += Time.deltaTime * Sensitivity;
            }
            else
            {
                input -= Time.deltaTime * Gravity;
            }
            if (input < 0f)
            {
                input = 0f;
            }
            if (input > 1f)
            {
                input = 1f;
            }
        }

        private void OnDisable()
        {
            input = 0f;
            pressing = false;
        }

        internal float input;

        public bool pressing;
    }
}