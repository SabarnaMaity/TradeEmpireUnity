using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Panels")]
    public GameObject startMenuPanel;
    public GameObject loginPanel;
    public GameObject signupPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Show start menu first
        ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        startMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        startMenuPanel.SetActive(false);
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
    }

    public void OpenSignupPanel()
    {
        startMenuPanel.SetActive(false);
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
    }
}
