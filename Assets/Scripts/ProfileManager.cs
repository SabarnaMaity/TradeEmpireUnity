using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text profileNameText;
    public TMP_Text profileCoinsText;

    private string userId;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    void OnEnable()
    {
        InitializeFirebase();
        AddProfileListener(); // Auto updates UI whenever data changes
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser == null)
        {
            SceneManager.LoadScene("GameStartScene");
            return;
        }
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void AddProfileListener()
    {
        dbRef.Child("users").Child(userId).ValueChanged += HandleProfileChanged;
    }
    void RemoveProfileListener()
{
    if (!string.IsNullOrEmpty(userId) && dbRef != null)
    {
        dbRef.Child("users").Child(userId).ValueChanged -= HandleProfileChanged;
    }
}


    private void HandleProfileChanged(object sender, ValueChangedEventArgs e)
    {
        if (!e.Snapshot.Exists) return;

        var data = e.Snapshot;

        // Read name
        string name = data.Child("username").Exists
            ? data.Child("username").Value.ToString()
            : "Unknown";

        // Read coins
        int coins = data.Child("coins").Exists
            ? int.Parse(data.Child("coins").Value.ToString())
            : 0;

        // Update UI safely
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            profileNameText.text = name;
            profileCoinsText.text = coins.ToString();
        });
    }
    public void Logout()
{
    Debug.Log("Logging out user...");

    //  Remove Firebase listeners
    RemoveProfileListener();

    // Sign out Firebase
    if (auth != null)
    {
        auth.SignOut();
    }

    // Clear local data (optional)
    PlayerPrefs.DeleteAll();

    //  Load Login Scene
    SceneManager.LoadScene("GameStartScene");
}

void OnDisable()
{
    RemoveProfileListener();
}


}
