using UnityEngine;
using TMPro;

public class CreateListingPanel : MonoBehaviour
{
    [Header("UI Inputs")]
    public TMP_Dropdown itemDropdown;  
    public TMP_InputField amountInput;
    public TMP_InputField priceInput;

    [Header("References")]
    public MarketplaceManager marketplaceManager;
    public GameObject marketplacePanel;
    public GameObject createlistPanel;
    public TMP_Text creatorStatusText;

   
    public void OnCreateListing()
    {
       // marketplacePanel.SetActive(true);
        string item = itemDropdown.options[itemDropdown.value].text;

        if (!int.TryParse(amountInput.text, out int amount) || amount <= 0)
        {
            creatorStatusText.text = "Enter valid amount!";
            return;
        }

        if (!int.TryParse(priceInput.text, out int price) || price <= 0)
        {
            creatorStatusText.text = "Enter valid price!";
            return;
        }

        string sellerName = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;

        marketplaceManager.CreateListing(item, amount, price, sellerName);

       // creatorStatusText.text = "Listing created!";
        marketplacePanel.SetActive(true);
        createlistPanel.SetActive(false);
        amountInput.text = "";
        priceInput.text = "";
    }

    public void ClosePanel()
    {
       // this.gameObject.SetActive(false);
       createlistPanel.SetActive(false);
        marketplacePanel.SetActive(true);

    }
}
