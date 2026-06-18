using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using TMPro;

public class MarketplaceManager : MonoBehaviour
{
    [Header("Firebase")]
    private DatabaseReference db;

    [Header("UI References")]
    public TMP_Text coinsText;
    public GameObject listingPrefab;
    public Transform listingsContent;  // ScrollRect Content parent
    public TMP_Text statusText;

    [Header("Player Data")]
    public string playerUID;
    public int playerCoins;

    void Start()
    {
        Debug.Log("Current Player UID = " + Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId);
    }

    private void OnEnable()
    {
        InitializeFirebase();
        LoadPlayerCoins();
        LoadMarketplaceListings();
    }

    private void OnDisable()
    {
        db.Child("marketplace").ValueChanged -= HandleMarketplaceChanged;
    }

    
    private void InitializeFirebase()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        playerUID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    }

    

    private void LoadPlayerCoins()
    {
        Debug.Log("LoadPlayerCoins() called!");

        db.Child("users").Child(playerUID).Child("coins").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Firebase LoadPlayerCoins error: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Firebase coins value = " + task.Result.Value);

                if (task.Result.Value == null)
                {
                    Debug.LogError("Coins node missing in database!");
                    return;
                }

                playerCoins = int.Parse(task.Result.Value.ToString());
                //Debug.Log("playercoins:"+playerCoins);
                // coinsText.text = "Coins: " + playerCoins;

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    coinsText.text = "" + playerCoins;
                    //  coinsText.text = "Coins: " + task.Result.Value;

                    Debug.Log("Coins text updated: " + playerCoins);
                });
            }
        });
    }


   
    private void LoadMarketplaceListings()
    {
        db.Child("marketplace").ValueChanged += HandleMarketplaceChanged;
    }

    private void HandleMarketplaceChanged(object sender, ValueChangedEventArgs e)
    {
        // Clear existing
        foreach (Transform child in listingsContent)
            Destroy(child.gameObject);

        if (!e.Snapshot.Exists)
            return;

        // Instantiate each listing
        foreach (var child in e.Snapshot.Children)
        {
            string listingId = child.Key;
            string item = child.Child("item").Value.ToString();
            int amount = int.Parse(child.Child("amount").Value.ToString());
            int price = int.Parse(child.Child("price").Value.ToString());
            string sellerUid = child.Child("sellerUid").Value.ToString();
            string sellerName = child.Child("sellerName").Value.ToString();

            GameObject obj = Instantiate(listingPrefab, listingsContent);
            ListingItemUI ui = obj.GetComponent<ListingItemUI>();

            ui.Setup(listingId, item, amount, price, sellerName, sellerUid, this);
        }
    }

    
    public void RefreshCoins(int newCoins)
    {
        playerCoins = newCoins;
        coinsText.text = "Coins: " + playerCoins;
    }

    public void ShowStatus(string msg)
    {
        statusText.text = msg;
    }

   
    public IEnumerator ProcessPurchase(string listingId, string sellerUid, int amount, int price)
    {
        statusText.text = "Checking purchase...";
       
        if (sellerUid == playerUID)
        {
            statusText.text = "You cannot buy your own item!";
            yield break;
        }
        
        var buyerCoinsTask = db.Child("users").Child(playerUID).Child("coins").GetValueAsync();
        yield return new WaitUntil(() => buyerCoinsTask.IsCompleted);

        if (!buyerCoinsTask.Result.Exists)
        {
            statusText.text = "Error: no player coins!";
            yield break;
        }

        int latestBuyerCoins = int.Parse(buyerCoinsTask.Result.Value.ToString());

        if (latestBuyerCoins < price)
        {
            statusText.text = "Not enough coins!";
            yield break;
        }

        
        var listingTask = db.Child("marketplace").Child(listingId).GetValueAsync();
        yield return new WaitUntil(() => listingTask.IsCompleted);

        if (!listingTask.Result.Exists)
        {
            statusText.text = "Listing already sold!";
            yield break;
        }

        string item = listingTask.Result.Child("item").Value.ToString().ToLower();

        
        int updatedBuyerCoins = latestBuyerCoins - price;
        var updateBuyer = db.Child("users").Child(playerUID).Child("coins").SetValueAsync(updatedBuyerCoins);
        yield return new WaitUntil(() => updateBuyer.IsCompleted);

       
        var sellerCoinsTask = db.Child("users").Child(sellerUid).Child("coins").GetValueAsync();
        yield return new WaitUntil(() => sellerCoinsTask.IsCompleted);

        int sellerCoins = sellerCoinsTask.Result.Exists
            ? int.Parse(sellerCoinsTask.Result.Value.ToString())
            : 0;

        int updatedSellerCoins = sellerCoins + price;
        var updateSeller = db.Child("users").Child(sellerUid).Child("coins").SetValueAsync(updatedSellerCoins);
        yield return new WaitUntil(() => updateSeller.IsCompleted);
        // // Seller achievements
        // AchievementManager ach = FindFirstObjectByType<AchievementManager>();

        // if (ach != null)
        // {
        //     ach.UnlockFirstSale();
        //     ach.IncrementTrades();
        // }



        // 5. Add item to buyer inventory
        var buyerItemTask = db.Child("users").Child(playerUID).Child("inventory").Child(item.ToLower()).GetValueAsync();
        yield return new WaitUntil(() => buyerItemTask.IsCompleted);

        int buyerItemCount = buyerItemTask.Result.Exists
            ? int.Parse(buyerItemTask.Result.Value.ToString())
            : 0;

        int updatedItems = buyerItemCount + amount;

        var updateItem = db.Child("users").Child(playerUID).Child("inventory").Child(item).SetValueAsync(updatedItems);
        yield return new WaitUntil(() => updateItem.IsCompleted);

        // 6. Remove listing from marketplace
        var deleteListing = db.Child("marketplace").Child(listingId).RemoveValueAsync();
        yield return new WaitUntil(() => deleteListing.IsCompleted);

        // 7. UI update
        RefreshCoins(updatedBuyerCoins);
        statusText.text = "Purchase successful!";


        // ---- ACHIEVEMENTS: BUYER ----
        var buyerAchRef = db.Child("users").Child(playerUID).Child("achievements");

        // First purchase
        var buyerFirstPurchaseTask = buyerAchRef.Child("firstPurchase").GetValueAsync();
        yield return new WaitUntil(() => buyerFirstPurchaseTask.IsCompleted);

        // if (!buyerFirstPurchaseTask.Result.Exists ||
        //     buyerFirstPurchaseTask.Result.Value.ToString() == "false")
        // {
        //     yield return buyerAchRef.Child("firstPurchase").SetValueAsync(true);
        // }
        // ---- FIX: Safe boolean check for firstPurchase ----
        bool buyerFirstPurchase = false;

        if (buyerFirstPurchaseTask.Result.Exists)
        {
            bool.TryParse(buyerFirstPurchaseTask.Result.Value.ToString(), out buyerFirstPurchase);
        }

        if (!buyerFirstPurchase)
        {
            yield return buyerAchRef.Child("firstPurchase").SetValueAsync(true);
            // Update UI immediately
            AchievementManager ach = FindAnyObjectByType<AchievementManager>();
            if (ach != null)
                ach.UnlockFirstPurchase();
        }



        // Increment buyer totalTrades
        var buyerTradesTask = buyerAchRef.Child("totalTrades").GetValueAsync();
        yield return new WaitUntil(() => buyerTradesTask.IsCompleted);

        int buyerTrades = 0;
        if (buyerTradesTask.Result.Exists)
            int.TryParse(buyerTradesTask.Result.Value.ToString(), out buyerTrades);

        yield return buyerAchRef.Child("totalTrades").SetValueAsync(buyerTrades + 1);
        // UI trades update


        // ---- ACHIEVEMENTS: SELLER ----
        var sellerAchRef = db.Child("users").Child(sellerUid).Child("achievements");

        // First sale
        var sellerFirstSaleTask = sellerAchRef.Child("firstSale").GetValueAsync();
        yield return new WaitUntil(() => sellerFirstSaleTask.IsCompleted);

        // if (!sellerFirstSaleTask.Result.Exists ||
        //     sellerFirstSaleTask.Result.Value.ToString() == "false")
        // {
        //     yield return sellerAchRef.Child("firstSale").SetValueAsync(true);
        // }

        // ---- FIX: Safe boolean check for firstSale ----
        bool sellerFirstSale = false;

        if (sellerFirstSaleTask.Result.Exists)
        {
            bool.TryParse(sellerFirstSaleTask.Result.Value.ToString(), out sellerFirstSale);
        }

        if (!sellerFirstSale)
        {
            yield return sellerAchRef.Child("firstSale").SetValueAsync(true);
            // UI update
            AchievementManager ach = FindAnyObjectByType<AchievementManager>();
            if (ach != null)
                ach.UnlockFirstSale();
        }


        // Increment seller trades
        var sellerTradesTask = sellerAchRef.Child("totalTrades").GetValueAsync();
        yield return new WaitUntil(() => sellerTradesTask.IsCompleted);

        int sellerTrades = 0;
        if (sellerTradesTask.Result.Exists)
            int.TryParse(sellerTradesTask.Result.Value.ToString(), out sellerTrades);

        yield return sellerAchRef.Child("totalTrades").SetValueAsync(sellerTrades + 1);


    }



    public void CreateListing(string item, int amount, int price, string sellerName)
    {
        string itemKey = item.ToLower();

        // STEP 1 — Fetch player's inventory for this item
        db.Child("users").Child(playerUID).Child("inventory").Child(itemKey)
            .GetValueAsync()
            .ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    statusText.text = "Error checking inventory!");
                return;
            }

            // Player does not have the item at all
            if (!task.Result.Exists)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    statusText.text = "You don’t have this item to sell!");
                return;
            }

            int currentQty = int.Parse(task.Result.Value.ToString());

            // STEP 2 — Check quantity
            if (currentQty < amount)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    statusText.text = "Not enough items to create listing!");
                return;
            }

            // STEP 3 — Reduce player's inventory
            int newQty = currentQty - amount;

            db.Child("users").Child(playerUID).Child("inventory").Child(itemKey)
                .SetValueAsync(newQty)
                .ContinueWith(setTask =>
            {
                if (!setTask.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        statusText.text = "Inventory update failed!");
                    return;
                }

                // STEP 4 — Create marketplace listing
                string listingId = db.Child("marketplace").Push().Key;

                ListingData newListing = new ListingData
                {
                    item = itemKey,
                    amount = amount,
                    price = price,
                    sellerUid = playerUID,
                    sellerName = sellerName
                };

                string json = JsonUtility.ToJson(newListing);

                db.Child("marketplace").Child(listingId)
                    .SetRawJsonValueAsync(json)
                    .ContinueWith(pushTask =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (pushTask.IsCompleted)
                            statusText.text = "Listing created!";
                        else
                            statusText.text = "Failed to create listing!";
                    });
                });
            });
        });
    }




    [System.Serializable]
    public class ListingData
    {
        public string item;
        public int amount;
        public int price;
        public string sellerUid;
        public string sellerName;
    }


}
