using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    private DatabaseReference db;
    private string playerUID;

    [Header("Achievement Lock Icons")]
    public GameObject firstPurchaseLock;
    public GameObject firstSaleLock;
    public GameObject traderLevel1Lock;

    void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        playerUID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

       
       AddAchievementListener();
    }

    
    void AddAchievementListener()
    {
        db.Child("users").Child(playerUID).Child("achievements")
            .ValueChanged += HandleAchievementChanged;
    }

    
   
   
    private void HandleAchievementChanged(object sender, ValueChangedEventArgs e)
    {
        if (!e.Snapshot.Exists)
            return;

        var data = e.Snapshot;

        

        
    bool firstPurchaseUnlocked = data.Child("firstPurchase").Value != null &&
                                 (bool)data.Child("firstPurchase").Value;

   
    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
        if (firstPurchaseLock.activeSelf != !firstPurchaseUnlocked)
            firstPurchaseLock.SetActive(!firstPurchaseUnlocked);
    });

   
    bool firstSaleUnlocked = data.Child("firstSale").Value != null &&
                             (bool)data.Child("firstSale").Value;

    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
        if (firstSaleLock.activeSelf != !firstSaleUnlocked)
            firstSaleLock.SetActive(!firstSaleUnlocked);
    });




        
        int totalTrades = 0;
        if (data.Child("totalTrades").Value != null)
            int.TryParse(data.Child("totalTrades").Value.ToString(), out totalTrades);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            traderLevel1Lock.SetActive(totalTrades < 5);
        });
    }

    



public void UnlockFirstPurchase()
{
    UnityMainThreadDispatcher.Instance().Enqueue(() => firstPurchaseLock.SetActive(false));

    db.Child("users").Child(playerUID).Child("achievements").Child("firstPurchase")
      .SetValueAsync(true).ContinueWith(task =>
    {
        if (task.IsFaulted)
            Debug.LogError("Failed to update FirstPurchase in Firebase: " + task.Exception);
    });
}

public void UnlockFirstSale()
{
    UnityMainThreadDispatcher.Instance().Enqueue(() => firstSaleLock.SetActive(false));

    db.Child("users").Child(playerUID).Child("achievements").Child("firstSale")
      .SetValueAsync(true).ContinueWith(task =>
    {
        if (task.IsFaulted)
            Debug.LogError("Failed to update FirstSale in Firebase: " + task.Exception);
    });
}




    


    public void IncrementTrades()
{
    db.Child("users").Child(playerUID).Child("achievements").Child("totalTrades")
        .GetValueAsync().ContinueWith(task =>
    {
        int count = task.Result.Exists ? int.Parse(task.Result.Value.ToString()) : 0;
        count++;

        db.Child("users").Child(playerUID).Child("achievements").Child("totalTrades").SetValueAsync(count);

        if (count >= 5)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                traderLevel1Lock.SetActive(false));
        }
    });
}

}
