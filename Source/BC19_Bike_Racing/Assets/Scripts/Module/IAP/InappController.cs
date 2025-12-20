using System;
using System.Collections;
using UnityEngine;


#if USE_INAPP
using Bacon;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif


namespace Bacon
{
#if USE_INAPP
    public class InappController : MonoBehaviour, IDetailedStoreListener
    {
        public event Action<string> OnPurchaseComplete;

        public static InappController Instance = null;

        private string currentProduct;

        [SerializeField] private bool InitAtStart = false;

        [SerializeField] private ShopData shopData;

        [Space, Header("DEBUG"), SerializeField]
        private bool logDebug = false;


        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (InitAtStart)
            {
                StartCoroutine(InitIAP());
            }
        }

        public IEnumerator InitIAP()
        {
            InitializationOptions options = new InitializationOptions();
            yield return UnityServices.InitializeAsync();
            InitializePurchasing();
        }

        public void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }

            try
            {
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

                //add product
                if (shopData != null)
                {
                    foreach (InappProduct product in shopData.listItems)
                    {
                        builder.AddProduct(product.ProductId, product.Type);
                    }
                }

                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception e)
            {
                // An error occurred during initialization.
                if (logDebug)
                    Debug.Log("[InappPurchase] initial fail: " + e.Message);
            }
        }


        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void BuyProductIndex(int _index)
        {
            var product = shopData.GetProduct(_index);
            if (product != null)
            {
                BuyProductID(product.ProductId);
            }
        }

        public void BuyProductID(string productId)
        {
            currentProduct = productId;
            if (IsInitialized())
            {
                if (AdsController.Instance) AdsController.Instance.hideApp = true;
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    if (logDebug)
                        Debug.Log(string.Format("[InappPurchase] Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    if (logDebug)
                        Debug.Log("[InappPurchase] BuyProductID: FAIL. Not purchasing product" +
                            ", either is not found or is not available for purchase");
                }
            }
            else
            {
                if (logDebug)
                    Debug.Log("[InappPurchase] BuyProductID FAIL. Not initialized.");
            }
        }

        public void RestorePurchases()
        {
            if (!IsInitialized())
            {
                if (logDebug)
                    Debug.Log("[InappPurchase] RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                if (logDebug)
                    Debug.Log("[InappPurchase] RestorePurchases started ...");

                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result, _str) =>
                {
                    if (logDebug)
                        Debug.Log("[InappPurchase] RestorePurchases continuing: " + result
                            + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                if (logDebug)
                    Debug.Log("[InappPurchase] RestorePurchases FAIL. Not supported on this platform. Current = "
                        + Application.platform);
            }
        }


        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            if (logDebug)
                Debug.Log("[InappPurchase] OnInitialized: PASS");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            OnInitSub();
        }

        private void OnInitSub()
        {
            if (shopData)
            {
                foreach (InappProduct product in shopData.listItems)
                {
                    if (product.Type == ProductType.Subscription)
                    {
                        var productSub = m_StoreController.products.WithID(product.ProductId);
                        if (productSub != null)
                        {
                            if (productSub.hasReceipt)
                            {
                                SubscriptionManager p = new SubscriptionManager(productSub, productSub.receipt);
                                SubscriptionInfo info = p.getSubscriptionInfo();
                                if (info.isSubscribed().Equals(Result.True))
                                {
                                    //Receipt Subscription
                                }
                            }
                            else
                            {
                                // Subscription fail, or expire
                            }
                        }
                    }
                }
            }
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            if (logDebug)
                Debug.Log("[InappPurchase] OnInitializeFailed InitializationFailureReason:" + error);
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (currentProduct == args.purchasedProduct.definition.id)
            {
                OnPurchaseComplete?.Invoke(currentProduct);
                m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
            }
            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (logDebug)
                Debug.Log(string.Format("[InappPurchase] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
                    product.definition.storeSpecificId, failureReason));
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            if (logDebug)
                Debug.Log("[InappPurchase] OnInitializeFailed InitializationFailureReason:" + message);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (logDebug)
                Debug.Log(string.Format("[InappPurchase] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
                    product.definition.storeSpecificId, failureDescription.message));
        }

    }


#else
    public class InappController : MonoBehaviour
    {
        public event Action<string> OnPurchaseComplete;

        public static InappController Instance = null;

        [SerializeField] private ShopData shopData;

        private void Awake()
        {
            Instance = this;
        }

        public IEnumerator InitIAP()
        {
            yield return new WaitForEndOfFrame();
        }

        public void BuyProductIndex(int _index)
        {
            var product = shopData.GetProduct(_index);
            if (product != null)
            {
                BuyProductID(product.ProductId);
            }
        }

        public void BuyProductID(string productId)
        {
            OnPurchaseComplete?.Invoke(productId);
        }

        public void RestorePurchases()
        {
        }
    }
#endif
}