using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Kamgam.BikeAndCharacter25D;

namespace Kamgam.BikeRacing25D
{
    public class UIGameMenuForTouch : UIBaseFade, IBikeTouchInput
    {
        public System.Action OnPause;
        public System.Action OnRestart;

        public GameObject PauseButton;
        public GameObject RestartButton;

        public GameObject SpeedUpButton;
        public GameObject BrakeButton;
        public GameObject RotateCWButton;
        public GameObject RotateCCWButton;

        bool isSpeedUpBtnPressed;
        bool isBrakeBtnPressed;
        bool isRotateCWBtnPressed;
        bool isRotateCCWBtnPressed;

        PointerEventData pointerEventData;
        GraphicRaycaster raycaster;
        EventSystem eventSystem;
        List<RaycastResult> raycastResults;
        int frameCounter;

        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => true;

        void Start()
        {
            raycaster = GetComponent<GraphicRaycaster>();
            eventSystem = EventSystem.current;
            pointerEventData = new PointerEventData(eventSystem);
            raycastResults = new List<RaycastResult>();
        }

        public override void OnClick(GameObject obj, BaseEventData evt)
        {
            if (obj == PauseButton.gameObject)
            {
                OnPause?.Invoke();
            }

            if (obj == RestartButton.gameObject)
            {
                OnRestart?.Invoke();
            }
        }

        #region Continuous touch and mouse input detection

        void OnEnable()
        {
            frameCounter = 0;
        }

        // We do check whether or not a finge/mouse is pressing a button continuously every N frames.
        // This is done to simplify the Input code of the bike as with this we can check keyDown and touchDown
        // events in the same way. This uses the OLD input system.

        void Update()
        {
            // Do raycasts every nth frame if the ui is logically shown
            if (++frameCounter % 5 == 0 && isShown)
            {
                isSpeedUpBtnPressed = false;
                isBrakeBtnPressed = false;
                isRotateCWBtnPressed = false;
                isRotateCCWBtnPressed = false;

                if (Main.IsTouchEnabled() && Input.touchSupported)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        var touch = Input.GetTouch(i);
                        doRaycast(touch.position);
                    }
                }
                else if (Input.mousePresent && Input.GetMouseButton(0))
                {
                    doRaycast(Input.mousePosition);
                }
            }
        }

        protected void doRaycast(Vector2 position)
        {
            raycastResults.Clear();
            pointerEventData.position = position;
            raycaster.Raycast(pointerEventData, raycastResults);
            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject == SpeedUpButton)
                {
                    isSpeedUpBtnPressed = true;
                    break;
                }
                if (result.gameObject == BrakeButton)
                {
                    isBrakeBtnPressed = true;
                    break;
                }
                if (result.gameObject == RotateCWButton)
                {
                    isRotateCWBtnPressed = true;
                    break;
                }
                if (result.gameObject == RotateCCWButton)
                {
                    isRotateCCWBtnPressed = true;
                    break;
                }
            }
        }

        bool IBikeTouchInput.IsSpeedUpPressed()
        {
            return isSpeedUpBtnPressed;
        }

        bool IBikeTouchInput.IsBrakePressed()
        {
            return isBrakeBtnPressed;
        }

        public bool IsRotateCWPressed()
        {
            return isRotateCWBtnPressed;
        }

        public bool IsRotateCCWPressed()
        {
            return isRotateCCWBtnPressed;
        }

        #endregion
    }
}
