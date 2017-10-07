using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class Purchaser : MonoBehaviour, IStoreListener
{
  private static IStoreController storeController;
  private static IExtensionProvider storeExtensionProvider;

  public static string kProductIDDonate = "com.rokuz.spaceshippers.donate";

  public delegate void OnDonatedEvent(bool success);
  public event OnDonatedEvent OnDonated;
  private bool isDonated = false;

  public bool IsDonated
  {
    get { return isDonated; }
  }

  void Start()
  {
    if (storeController == null)
      InitializePurchasing();
  }

  public void InitializePurchasing() 
  {
    if (IsInitialized())
      return;

    print("InitializePurchasing()");
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
    builder.AddProduct(kProductIDDonate, ProductType.NonConsumable);
    UnityPurchasing.Initialize(this, builder);
  }
    
  private bool IsInitialized()
  {
    return storeController != null && storeExtensionProvider != null;
  }

  public void Donate()
  {
    BuyProductID(kProductIDDonate);
  }

  public string GetDonateAmount()
  {
    if (!IsInitialized())
      return "";

    Product product = storeController.products.WithID(kProductIDDonate);
    return product.metadata.localizedPriceString;
  }

  void BuyProductID(string productId)
  {
    if (!IsInitialized())
      return;
    
    Product product = storeController.products.WithID(productId);
    if (product != null && product.availableToPurchase)
      storeController.InitiatePurchase(product);
  }

  public void RestorePurchases()
  {
    if (!IsInitialized())
      return;

    if (Application.platform == RuntimePlatform.IPhonePlayer || 
      Application.platform == RuntimePlatform.OSXPlayer)
    {
      var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
      apple.RestoreTransactions((result) => {});
    }
  }

  public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
  {
    storeController = controller;
    storeExtensionProvider = extensions;

    print("Purchaser initialized");
  }
    
  public void OnInitializeFailed(InitializationFailureReason error)
  {
    print("Purchaser not initialized");
  }

  public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
  {
    if (String.Equals(args.purchasedProduct.definition.id, kProductIDDonate, StringComparison.Ordinal))
    {
      isDonated = true;
      if (OnDonated != null)
        OnDonated(isDonated);
    }
    return PurchaseProcessingResult.Complete;
  }
    
  public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
  {
    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
      product.definition.storeSpecificId, failureReason));
    isDonated = false;
    if (OnDonated != null)
      OnDonated(isDonated);
  }
}
