using UnityEngine;


public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
  //  public GameObject achievementsPanel;
    public GameObject profilePanel;
    public GameObject marketplacePanel;
    public GameObject createListPanel;
    public GameObject achievementsPanel;


     [Header("Main Menu Section (Buttons)")]
    public GameObject mainMenuSection;
    private GameObject currentOpenPanel;


   void Start()
    {
        CloseAllPanels();
    }

    public void OpenInventory()
    {
        OpenPanel(inventoryPanel);
    }

    public void OpenProfile()
    {
        OpenPanel(profilePanel);
    }
    public void OpenMarketPanel()
    {
        OpenPanel(marketplacePanel);
    }
    public void OpenCreateListPanel()
    {
        OpenPanel(createListPanel);
        marketplacePanel.SetActive(true);

    }
    public void OpenAchivementsPanel()
    {
        OpenPanel(achievementsPanel);
       // marketplacePanel.SetActive(true);
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        // Close Previous Panel
        if (currentOpenPanel != null && currentOpenPanel != panel)
            currentOpenPanel.SetActive(false);

        // Open New Panel
        panel.SetActive(true);
        currentOpenPanel = panel;

        // Hide main menu buttons
        if (mainMenuSection != null)
            mainMenuSection.SetActive(false);
    }

    public void CloseAllPanels()
    {
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (profilePanel) profilePanel.SetActive(false);
        if(marketplacePanel) marketplacePanel.SetActive(false);
        if(achievementsPanel) achievementsPanel.SetActive(false);

        currentOpenPanel = null;

        // Show main menu again
        if (mainMenuSection != null)
            mainMenuSection.SetActive(true);
    }

    
    public void BackToMenu()
    {
        CloseAllPanels();
    }
}
