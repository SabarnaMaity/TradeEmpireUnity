using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListingItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text itemText;
    public TMP_Text amountText;
    public TMP_Text priceText;
    public TMP_Text sellerText;
    public Button buyButton;

    // data
    private string listingId;
    private string sellerUid;
    private int price;
    private int amount;
    private MarketplaceManager manager;

   
   
   
    public void Setup(string listingId, string item, int amount, int price,
                      string sellerName, string sellerUid, MarketplaceManager manager)
    {
        this.listingId = listingId;
        this.sellerUid = sellerUid;
        this.price = price;
        this.amount = amount;
        this.manager = manager;

        
        itemText.text = item;
        amountText.text = "x" + amount.ToString();
        priceText.text = price.ToString() + " coins";
        sellerText.text = "Seller: " + sellerName;
       


        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

    }

    
    public void OnBuyClicked()
    {
        buyButton.interactable = false; // avoid double click

        // Call manager → you will add BuyItem() in manager next
        manager.StartCoroutine(BuyItemRoutine());
    }

    private System.Collections.IEnumerator BuyItemRoutine()
    {
        manager.ShowStatus("Processing purchase...");

        // Let manager handle purchase
        yield return manager.StartCoroutine(
            BuyItemFirebase(listingId, sellerUid, amount, price)
        );
    }

   
    // Firebase purchase logic
       private System.Collections.IEnumerator BuyItemFirebase(string listingId, string sellerUid, int amount, int price)
    {
        yield return manager.StartCoroutine(
            manager.ProcessPurchase(listingId, sellerUid, amount, price)
        );
    }
}
