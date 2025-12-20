using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Bacon
{
    [Serializable]
    public class AdmobNativeUI : PopupBase
    {
        [Header("ELEMENT REQUIRED")]
        public Image adIcon;
        public Image adChoices;
        public GameObject adLabel;
        public Text adHeadline;
        public GameObject adCTA;
        public Text adCallToAction;
        public Text adAdvertiser;

        [Header("ELEMENT OPTIONAL")]
        public Image adImage;
        public Text adBody;
        public Image adRating;
        public Text adPrice;
        public Text adStore;
        public Text adMessage;


        [Header("ADS OPTION")]
        [SerializeField, Space] NativeUIName nativeName;

        [SerializeField, ShowIf("nativeName", NativeUIName.Native_Inter), Space] private int totalCountDown = 5;
        [ShowIf("nativeName", NativeUIName.Native_Inter), Space] public Transform adLoading;
        [ShowIf("nativeName", NativeUIName.Native_Inter), Space] public Button btnClose;
        [ShowIf("nativeName", NativeUIName.Native_Inter), Space] public Text txtClose;

        [Header("DEBUG"), ShowIf("nativeName", NativeUIName.Native_Banner)]
        [ShowIf("nativeName", NativeUIName.Native_Banner), Space] public bool isDebug = false;
        [ShowIf("isDebug"), Space] public Color color = Color.red;

        [HideInInspector] public Action OnInterSuccess;


        private bool loadingNativeInter = false;
        private readonly float rotateSpeed = 100;


        protected override void OnEnable()
        {
            base.OnEnable();
#if USE_NATIVE
            AdsController.Instance.OnNativeLoaded += OnNativeLoaded;
            if (nativeName == NativeUIName.Native_Inter)
            {
                loadingNativeInter = true;

                if (adLoading)
                {
                    adLoading.gameObject.SetActive(true);
                }
                StartCoroutine(ShowCloseButton());
            }
            else if (nativeName == NativeUIName.Native_Banner)
            {
                AdsController.Instance.OnNativeClick += OnClickBanner;
                if (isDebug)
                {
                    CalcPositons();
                    DrawBox();
                }
            }
#endif
        }

        protected override void OnDisable()
        {
#if USE_NATIVE
            AdsController.Instance.OnNativeLoaded -= OnNativeLoaded;
            if (nativeName == NativeUIName.Native_Banner)
            {
                AdsController.Instance.OnNativeClick -= OnClickBanner;
            }
#endif
            base.OnDisable();
        }

        private void Update()
        {
            if (nativeName == NativeUIName.Native_Inter && loadingNativeInter && adLoading)
            {
                adLoading.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime, Space.Self);
            }
        }

        private IEnumerator ShowCloseButton()
        {
            txtClose.text = totalCountDown.ToString();
            yield return new WaitForSeconds(1.0f);

            btnClose.interactable = false;
            for (int i = totalCountDown; i > 0; i--)
            {
                txtClose.text = i.ToString();
                yield return new WaitForSeconds(1.0f);
            }
            txtClose.text = "X";
            btnClose.interactable = true;
        }

        private void OnNativeLoaded()
        {
            if (nativeName == NativeUIName.Native_Inter)
            {
                loadingNativeInter = false;
                adLoading.gameObject.SetActive(false);
                //AdsController.Instance.HideSmallBanner();
            }
            else if (nativeName == NativeUIName.Native_Banner)
            {
                if (isDebug)
                {
                    CalcPositons();
                    DrawBox();
                }
                gameObject.SetActive(RouteController.Instance.ShowNativeBanner);
            }
        }

        public void OnNativeClose()
        {
            if (nativeName == NativeUIName.Native_Inter)
            {
                Hide(() =>
                {
                    //AdsController.Instance.ShowSmallBanner();
                    OnInterSuccess?.Invoke();
                    Destroy(gameObject);
                });

            }
        }

        public void OnClickBanner()
        {
#if USE_NATIVE
            if (nativeName == NativeUIName.Native_Banner)
            {
                adHeadline.GetComponent<Text>().raycastTarget = false;
                if (RouteController.Instance)
                    RouteController.Instance.ShowNativeDefault();
            }
#endif
        }

        #region DEBUG HELPER
        private Vector3 V3TopLeft;
        private Vector3 V3TopRight;
        private Vector3 V3BottomLeft;
        private Vector3 V3BottomRight;
        private void CalcPositons()
        {
            Bounds bounds;
            if (adHeadline.TryGetComponent<BoxCollider2D>(out var bc))
                bounds = bc.bounds;
            else
                return;


            Vector3 v3Center = bounds.center;
            Vector3 v3Extents = bounds.extents;

            V3TopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z);  //top left corner
            V3TopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z);  //top right corner
            V3BottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z);  //bottom left corner
            V3BottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z);  //bottom right corner

            //V2TopLeft = transform.TransformPoint(V2TopLeft);
            //V2TopRight = transform.TransformPoint(V2TopRight);
            //V2BottomLeft = transform.TransformPoint(V2BottomLeft);
            //V2BottomRight = transform.TransformPoint(V2BottomRight);
            if (isDebug)
                Debug.Log("===== " + V3TopLeft + " -- " + V3BottomRight);
        }


        private readonly string colliderName = "Headline_Collider";
        private void DrawBox()
        {
#if UNITY_EDITOR
            Debug.DrawLine(V3TopLeft, V3TopRight, color);
            Debug.DrawLine(V3TopRight, V3BottomRight, color);
            Debug.DrawLine(V3BottomRight, V3BottomLeft, color);
            Debug.DrawLine(V3BottomLeft, V3TopLeft, color);
#endif
            GameObject _object = null;
            LineRenderer adCollider = null;

            var _tranform = transform.Find(colliderName);
            if (_tranform == null)
            {
                _object = new GameObject(colliderName);
                _object.transform.parent = transform;
                _object.layer = LayerMask.NameToLayer("UI");
                adCollider = _object.AddComponent<LineRenderer>();
            }
            else
            {
                adCollider = _tranform.GetComponent<LineRenderer>();
            }

            adCollider.startWidth = 0.02f;
            adCollider.endWidth = 0.02f;
            adCollider.startColor = color;
            adCollider.endColor = color;
            adCollider.loop = true;
            adCollider.positionCount = 4;
            adCollider.SetPosition(0, V3TopLeft);
            adCollider.SetPosition(1, V3TopRight);
            adCollider.SetPosition(2, V3BottomRight);
            adCollider.SetPosition(3, V3BottomLeft);
        }
        #endregion
    }

    public enum NativeUIName
    {
        Default = 0,
        Native_Banner = 1,
        Native_Inter = 2,
    }
}