using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

public class InventoryManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text coinsText;

    public TMP_Text woodCountText;
    public TMP_Text stoneCountText;
    public TMP_Text goldCountText;

    [Header("Prices")]
    public int woodPrice = 5;
    public int stonePrice = 5;
    public int goldPrice = 5;

    // local values
    private int coins;
    private int wood;
    private int stone;
    private int gold;

    private string userId;
    private DatabaseReference dbRef;

    void OnEnable()
    {
        
        InitializeFirebase();
        AddInventoryListener();
        
    }

    void InitializeFirebase()
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
    void AddInventoryListener()
{
    dbRef.Child("users").Child(userId).ValueChanged += HandleInventoryChanged;
}
private void HandleInventoryChanged(object sender, ValueChangedEventArgs e)
{
    if (!e.Snapshot.Exists)
        return;

    var data = e.Snapshot;

    
    if (data.Child("coins").Value != null)
        coins = int.Parse(data.Child("coins").Value.ToString());

    
    var inv = data.Child("inventory");

    wood = inv.Child("wood").Exists ? int.Parse(inv.Child("wood").Value.ToString()) : 0;
    stone = inv.Child("stone").Exists ? int.Parse(inv.Child("stone").Value.ToString()) : 0;
    gold = inv.Child("gold").Exists ? int.Parse(inv.Child("gold").Value.ToString()) : 0;

    
    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
        UpdateUI();
    });
}


    

    public void LoadInventoryFromFirebase()
    {
        dbRef.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result;

                
                coins = data.Child("coins").Exists
        ? int.Parse(data.Child("coins").Value.ToString())
        : 0;


                
                var inv = data.Child("inventory");

                wood = inv.Child("wood").Exists
                    ? int.Parse(inv.Child("wood").Value.ToString())
                    : 0;

                stone = inv.Child("stone").Exists
                    ? int.Parse(inv.Child("stone").Value.ToString())
                    : 0;

                gold = inv.Child("gold").Exists
                    ? int.Parse(inv.Child("gold").Value.ToString())
                    : 0;

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    UpdateUI();
                });
            }
            else
            {
                Debug.LogError("Inventory load failed!");
            }
        });
    }



    
    void UpdateUI()
    {
        coinsText.text = coins.ToString();
        woodCountText.text = wood.ToString();
        stoneCountText.text = stone.ToString();
        goldCountText.text = gold.ToString();
    }

    
    public void BuyWood()
    {
        if (coins >= woodPrice)
        {
            coins -= woodPrice;
            wood += 1;
            SaveInventory();
        }
    }

    public void BuyStone()
    {
        if (coins >= stonePrice)
        {
            coins -= stonePrice;
            stone += 1;
            SaveInventory();
        }
    }

    public void BuyGold()
    {
        if (coins >= goldPrice)
        {
            coins -= goldPrice;
            gold += 1;
            SaveInventory();
        }
    }

    
    void SaveInventory()
    {
        dbRef.Child("users").Child(userId).Child("coins").SetValueAsync(coins);
        dbRef.Child("users").Child(userId).Child("inventory").Child("wood").SetValueAsync(wood);
        dbRef.Child("users").Child(userId).Child("inventory").Child("stone").SetValueAsync(stone);
        dbRef.Child("users").Child(userId).Child("inventory").Child("gold").SetValueAsync(gold);

        UpdateUI(); // Refresh UI
    }
}
