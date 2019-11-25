using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using IAPProduct = UnityEngine.Purchasing.Product;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;
    private List<string> C_Products;
    private List<string> NC_Products;
    private static bool IsInitialized => storeController != null && storeExtensionProvider != null;
    private static string currentProductId;

    public static event SuccessfullPurchase OnSuccessfullPurchase;


    private void Start()
    {
        C_Products = new List<string>();
        NC_Products = new List<string>();

        foreach (var offer in UI.GetProducts<Offer>())
        {
            if (offer.isConsumable) C_Products.Add(offer.productId);
            if (!offer.isConsumable) NC_Products.Add(offer.productId);
        }

        InitializeIAP();
    }

    private async void InitializeIAP()
    {
        while (storeController == null && Application.isPlaying)
        {
            InitializePurchasing();
            await System.Threading.Tasks.Task.Yield();
        }

        void InitializePurchasing()
        {
            if (IsInitialized) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var c_p in C_Products) builder.AddProduct(c_p, ProductType.Consumable);
            foreach (var nc_p in NC_Products) builder.AddProduct(nc_p, ProductType.NonConsumable);

            MyDebug.Log("Initializing IAP...");

            UnityPurchasing.Initialize(this, builder);
        }
    }  
    
    public static void BuyConsumable(string productId)
    {
        BuyProductID(productId);
    }

    public static void BuyNonConsumable(string productId)
    {
        BuyProductID(productId);
    }

    private static void BuyProductID(string productId)
    {
        if (!IsInitialized)
        {
            MyDebug.LogWarning("BuyProductID FAIL. Not initialized.");
            return;
        }
        if (string.IsNullOrEmpty(productId))
        {
            MyDebug.LogError("BuyProductID FAIL. Empty productId");
            return;
        }

        IAPProduct product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            MyDebug.Log($"Purchasing product asychronously: {product.definition.id}");

            currentProductId = productId;

            storeController.InitiatePurchase(product);
        }
        else
        {
            MyDebug.LogError("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
        }
    }


    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        MyDebug.Log("OnInitialized: PASS");

        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        MyDebug.LogError($"OnInitializeFailed InitializationFailureReason: {error}");
    }

    public delegate void SuccessfullPurchase(PurchaseEventArgs args);
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == currentProductId)
        {
            MyDebug.Log($"ProcessPurchase: PASS. Product: {args.purchasedProduct.definition.id}");
        }
        else
        {
            MyDebug.LogError($"ProcessPurchase: FAIL. Unrecognized product: {args.purchasedProduct.definition.id}");
        }

        OnSuccessfullPurchase(args);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(IAPProduct product, PurchaseFailureReason failureReason)
    {
        MyDebug.LogError($"Purchase failed. Product {product.definition.storeSpecificId}, failure reasonn: {failureReason}");
    }
}
