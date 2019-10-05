using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public ManualClickerItem[] manualClickers;
    public AutoClickerItem[] autoClickers;
    public OfflineClickerItem[] offlineClickers;
    public UniversalClickerItem[] universalClickers;
    public TimeBoosterItem[] timeBoosters;
    public SoldierBoosterItem[] soldierBoosters;

    private void Awake() => Instance = this;

    private void Start()
    {
        Area51Controller.mainEvent.OnBuy += Buy;
    }

    private void Buy(Clicker clicker, Action<bool> success = default)
    {
        if (clicker == null)
        {
            MyDebug.LogError($"Clicker not found");
            SFXManager.PlaySound("ErrorBuy");
            success(false);
            return;
        }

        if (clicker.currentPrice < 0)
        {
            clicker.currentPrice = -clicker.currentPrice;
            SaveManager.Save(GameManager.data);
            success(false);
            return;
        }

        GameData data = GameManager.data;

        if (!data.isDefend)
        {
            if (clicker.currency == Currency.Soldier) data.soldiersCount -= clicker.currentPrice;
            else data.aliensHearts -= clicker.currentPrice;
        }
        else Area51Controller.mainEvent.ChangeHp(-clicker.currentPrice);

        clicker.hasBought = true;

        clicker.allClickPower += clicker.clickPowerDefault;

        if (clicker.currency == Currency.Soldier)
            clicker.currentPrice += (clicker.priceDefault / 2) + (clicker.priceDefault * clicker.level);
        else
            clicker.currentPrice += clicker.priceDefault / 5 * (clicker.level + 1);

        clicker.level++;

        if (clicker.GetType() == typeof(UniversalClicker))
        {
            data.clickBonus += clicker.clickPowerDefault;
            data.autoClickerBonus += clicker.clickPowerDefault * 5;
            data.offlineClickBonus += clicker.clickPowerDefault * 5;
        }

        data.clickBonus += clicker.clickPowerDefault;
        if (!data.clickers.ContainsKey(clicker.name)) data.clickers.Add(clicker.name, clicker);


        if (clicker.currentPrice < 0) clicker.currentPrice = int.MaxValue;

        SaveManager.Save(data);

        success(true);
    }
}

public enum Currency
{
    Soldier,
    AliensHeart
}

[Serializable] public abstract class Product
{
    public Currency currency = Currency.Soldier;
    public string name;
    public int priceDefault;
    [HideInInspector] public string description;
}


[Serializable] public abstract class ShopItem
{
    public Transform uiObject;
    public Sprite avatar;
    [HideInInspector] public Text name, description, price;
    [HideInInspector] public Button bttnBuy;
    [HideInInspector] public Image image;
}

[Serializable] public class ClickerShopItem : ShopItem
{
    [HideInInspector] public Text level, clickPower;
}

[Serializable] public class BoosterShopItem : ShopItem
{
    [HideInInspector] public Text amount;
}


[Serializable] public class ClickerItem<T> where T : Clicker
{
    public ClickerShopItem uiInfo;

    [SerializeField] protected T clicker;
    [HideInInspector] public T Clicker
    {
        get => clicker;
        set => clicker = value;
    }
    
    public ClickerItem(ClickerShopItem uiInfo, T clicker)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}

[Serializable] public class ManualClickerItem : ClickerItem<ManualClicker>
{
    public ManualClickerItem(ClickerShopItem uiInfo, ManualClicker clicker) : base(uiInfo, clicker)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class AutoClickerItem : ClickerItem<AutoClicker>
{
    public AutoClickerItem(ClickerShopItem uiInfo, AutoClicker clicker) : base (uiInfo, clicker)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class OfflineClickerItem : ClickerItem<OfflineClicker>
{
    public OfflineClickerItem(ClickerShopItem uiInfo, OfflineClicker clicker) : base(uiInfo, clicker)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class UniversalClickerItem : ClickerItem<UniversalClicker>
{
    public UniversalClickerItem(ClickerShopItem uiInfo, UniversalClicker clicker) : base(uiInfo, clicker)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}


[Serializable] public abstract class Clicker : Product
{
    public int clickPowerDefault;
    public int level;
    [HideInInspector] public int allClickPower;
    [HideInInspector] public int currentPrice;
    [HideInInspector] public bool hasBought;
}

[Serializable] public class ManualClicker : Clicker
{

}

[Serializable] public class AutoClicker : Clicker, IAutocliker
{
    [NonSerialized]
    public bool hasStart;

    async public void AutoClick()
    {
        if (hasStart) return;

        hasStart = true;

        while (hasStart)
        {
            await System.Threading.Tasks.Task.Delay(1000);

            if (Area51Controller.mainEvent != null && hasStart)
            {
                Area51Controller.mainEvent.Click(!GameManager.data.isDefend
                    ? allClickPower + (int)(allClickPower * (GameManager.data.prestigeLvl * 0.1f))
                    : (int)(allClickPower * (GameManager.data.prestigeLvl * 0.1f)));
            }
        }
    }
}

[Serializable] public class OfflineClicker : Clicker, IOfflineClicker
{
    public void RememberTime()
    {
        GameManager.data.exitTime = DateTime.Now.ToString();
    }

    public void CalculateProduction()
    {
        if (GameManager.data.isDefend)
        {
            MyDebug.LogWarning("Во время битвы оффлайн кликеры не вырабатывают солдат");
            return;
        }
        DateTime exitTime = DateTime.Parse(GameManager.data.exitTime);

        TimeSpan offlineTime = DateTime.Now - exitTime;

        int offlineSecs = (int)offlineTime.TotalSeconds;

        MyDebug.Log($"exit - {exitTime} /// now - {DateTime.Now} /// offline time - {offlineTime} /// offline secs - {offlineSecs}");

        int totalProduction = offlineSecs * allClickPower;
        totalProduction += (int)(totalProduction * (GameManager.data.prestigeLvl * 0.1f));

        MyDebug.Log($"{name} has produced - {totalProduction}");

        GameManager.data.soldiersCount += totalProduction;
    }
}

[Serializable] public class UniversalClicker : Clicker, IAutocliker, IOfflineClicker
{
    [NonSerialized] public bool hasStart;

    async public void AutoClick()
    {
        if (hasStart) return;

        hasStart = true;

        while (hasStart)
        {
            await System.Threading.Tasks.Task.Delay(1000);

            if (Area51Controller.mainEvent != null && hasStart)
            {
                Area51Controller.mainEvent.Click(!GameManager.data.isDefend
                    ? allClickPower * 5 + (int)(allClickPower * 5 * (GameManager.data.prestigeLvl * 0.1f))
                    : (int)(allClickPower * 5 * (GameManager.data.prestigeLvl * 0.1f)));
            }
        }
    }

    public void RememberTime()
    {
        GameManager.data.exitTime = DateTime.Now.ToString();
    }

    public void CalculateProduction()
    {
        if (GameManager.data.isDefend) return;

        DateTime exitTime = DateTime.Parse(GameManager.data.exitTime);

        TimeSpan offlineTime = DateTime.Now - exitTime;

        int offlineSecs = (int)offlineTime.TotalSeconds;

        int totalProduction = offlineSecs * (int)(allClickPower * 1.5f);
        totalProduction += (int)(totalProduction * (GameManager.data.prestigeLvl * 0.1f));

        GameManager.data.soldiersCount += totalProduction;
    }
}


[Serializable] public class BoosterItem<T> where T : Booster
{
    public BoosterShopItem uiInfo;
    [SerializeField] protected T booster;
    [HideInInspector] public T Booster
    {
        get => booster;
        set => booster = value;
    }
    
    public BoosterItem(BoosterShopItem uiInfo, T booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }
}

[Serializable] public class TimeBoosterItem : BoosterItem<TimeBooster>
{
    public TimeBoosterItem(BoosterShopItem uiInfo, TimeBooster booster) : base (uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }
}
[Serializable] public class SoldierBoosterItem : BoosterItem<SoldierBooster>
{
    public SoldierBoosterItem(BoosterShopItem uiInfo, SoldierBooster booster) : base (uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }
}


[Serializable] public abstract class Booster : Product
{
    [HideInInspector] public int amount;
}

[Serializable] public class TimeBooster : Booster
{
    public int times;
    public void SpeedUpTime()
    {
        Debug.Log("Speed up");
        amount--;
    }

    public void SlowDownTime()
    {
        Debug.Log("Slow down");
        amount--;
    }
}

[Serializable] public class SoldierBooster : Booster
{
    public int soldiers;
    public void IncreaseSoldiers()
    {
        Debug.Log("Increase soldiers");
        amount--;
    }
}


interface IAutocliker
{
    void AutoClick();
}

interface IOfflineClicker
{
    void RememberTime();

    void CalculateProduction();
}