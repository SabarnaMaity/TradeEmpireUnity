using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Collections;
using Firebase.Database;
using TMPro;
public class FireBaseAuthManager : MonoBehaviour
{

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;


    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;


    [Space]
    [Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public static FireBaseAuthManager Instance;

    private void Awake()
    {

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
            }
        });
    }



    void InitializeFirebase()
    {
        //Set the default instance object
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }


    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }


    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));

    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;


            string failedMessage = "Login Failed! Because ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage = "Login Failed";
                    break;
            }

            Debug.Log(failedMessage);
        }
        else
        {
            user = loginTask.Result.User;

            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);

            //  References.userName = user.DisplayName;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }



    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            Debug.LogError("User Name is empty");
        }
        else if (email == "")
        {
            Debug.LogError("email field is empty");
        }
        else if (passwordRegisterField.text != confirmPasswordRegisterField.text)
        {
            Debug.LogError("Password does not match");
        }
        else
        {

            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);
            }
            else
            {

                user = registerTask.Result.User;


                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);
                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    Debug.LogError(updateProfileTask.Exception);
                    user.DeleteAsync();
                }
                else
                {


                    string databaseURL = "https://tradeempire-441f2-default-rtdb.firebaseio.com/";
                    DatabaseReference db = FirebaseDatabase.GetInstance(databaseURL).RootReference;

                    // db.Child("users").Child(user.UserId).SetRawJsonValueAsync(json);

                    string uid = user.UserId;

                    PlayerProfileData newPlayer = new PlayerProfileData();
                    newPlayer.username = name;
                    newPlayer.coins = 50;
                    newPlayer.inventory = new InventoryData();
                    newPlayer.achievements = new AchievementData();

                    string json = JsonUtility.ToJson(newPlayer);

                    var saveTask = db.Child("users").Child(uid).SetRawJsonValueAsync(json);
                    yield return new WaitUntil(() => saveTask.IsCompleted);

                    if (saveTask.Exception != null)
                    {
                        Debug.LogError("Failed to save default user data: " + saveTask.Exception);
                    }
                    else
                    {
                        Debug.Log("Default user data saved for: " + uid);
                    }


                    Debug.Log("Registration Successful! Welcome " + user.DisplayName);
                    UiManager.Instance.OpenLoginPanel();
                }
            }
        }
    }



    public void GuestLogin()
    {
        StartCoroutine(GuestLoginAsync());
    }

    private IEnumerator GuestLoginAsync()
    {
        var guestTask = auth.SignInAnonymouslyAsync();
        yield return new WaitUntil(() => guestTask.IsCompleted);

        if (guestTask.Exception != null)
        {
            Debug.LogError("Guest Login Failed: " + guestTask.Exception);
            yield break;
        }


        user = guestTask.Result.User;
        string uid = user.UserId;
        Debug.Log("Guest Login Successful! UID = " + uid);


        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        var checkTask = db.Child("users").Child(uid).GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Exception != null)
        {
            Debug.LogError("Guest DB Check Failed: " + checkTask.Exception);
            yield break;
        }


        if (checkTask.Result.Exists)
        {
            Debug.Log("Guest already exists in database.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            yield break;
        }



        string guestName = "Guest" + Random.Range(1000, 9999);

        PlayerProfileData guestData = new PlayerProfileData();
        guestData.username = guestName;
        guestData.coins = 50;

        // inventory
        guestData.inventory = new InventoryData();
        guestData.inventory.wood = 0;
        guestData.inventory.stone = 0;
        guestData.inventory.gold = 0;

        // achievements
        guestData.achievements = new AchievementData();
        guestData.achievements.firstPurchase = false;
        guestData.achievements.firstSale = false;
        guestData.achievements.totalTrades = 0;

        string json = JsonUtility.ToJson(guestData);

        var saveTask = db.Child("users").Child(uid).SetRawJsonValueAsync(json);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            Debug.LogError("Failed to save guest data: " + saveTask.Exception);
            yield break;
        }

        Debug.Log("Guest profile created successfully in DB!");


        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }




}
