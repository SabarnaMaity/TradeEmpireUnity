[System.Serializable]
public class PlayerProfileData
{
    public string username;
    public int coins;
    public InventoryData inventory;
    public AchievementData achievements;
}

[System.Serializable]
public class InventoryData
{
    public int wood;
    public int stone;
    public int gold;
}

[System.Serializable]
public class AchievementData
{
    public bool firstPurchase;
    public bool firstSale;
    public int totalTrades;
}
