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
        EventManager.eventManager.OnBuy += Buy;
    }

    private void Buy(Product product, Action<bool> success = default)
    {
        if (product == null)
        {
            MyDebug.LogError($"Clicker not found");
            SFXManager.PlaySound("ErrorBuy");
            success(false);
            return;
        }

        GameData data = GameDataManager.data;

        if (product is Clicker clicker)
        {
            if (clicker.currentPrice < 0)
            {
                clicker.currentPrice = -clicker.currentPrice;
                SaveManager.Save(GameDataManager.data);
                success(false);
                return;
            }

            if (!data.isDefend)
            {
                if (clicker.currency == Currency.Soldier) data.soldiersCount -= clicker.currentPrice;
                else data.aliensHearts -= clicker.currentPrice;
            }
            else EventManager.eventManager.ChangeHp(-clicker.currentPrice);

            clicker.hasBought = true;

            clicker.allClickPower += clicker.clickPowerDefault;

            if (clicker.currency == Currency.Soldier)
                clicker.currentPrice += (clicker.priceDefault / 2) + (clicker.priceDefault / 10 * clicker.level);
            else
                clicker.currentPrice += clicker.priceDefault / 5 * (clicker.level + 1);

            clicker.level++;

            if (clicker.GetType() == typeof(UniversalClicker))
            {
                data.clickBonus += clicker.clickPowerDefault;
                data.autoClickerBonus += clicker.clickPowerDefault * 5;
                data.offlineClickBonus += clicker.clickPowerDefault * 5;
            }
            if (clicker.GetType() == typeof(ManualClicker)) data.clickBonus += clicker.clickPowerDefault;
            if (clicker.GetType() == typeof(AutoClicker)) data.autoClickerBonus += clicker.clickPowerDefault;
            if (clicker.GetType() == typeof(OfflineClicker)) data.offlineClickBonus += clicker.clickPowerDefault;

            if (!data.clickers.ContainsKey(clicker.name))
            {
                data.clickers.Add(clicker.name, clicker);
                data.clickersCount++;
            }

            if (clicker.currentPrice < 0) clicker.currentPrice = int.MaxValue;

        }
        else if (product is Booster booster)
        {
            if (!data.isDefend)
            {
                if (booster.currency == Currency.Soldier) data.soldiersCount -= booster.priceDefault;
                else data.aliensHearts -= booster.priceDefault;
            }
            booster.amount++;
            if (!data.boosters.ContainsKey(booster.name)) data.boosters.Add(booster.name, booster);
        }

        success(true);

        SaveManager.Save(data);
    }
}

public enum Currency
{
    Soldier,
    AlienHeart
}

[Serializable] public abstract class Product
{
    public Currency currency = Currency.Soldier;
    public string name;
    public int priceDefault;
}


[Serializable] public abstract class ShopItem
{
    public Transform uiObject;
    public Sprite avatar;
    [HideInInspector] public Text name, price;
    [HideInInspector] public Button bttnBuy;
    [HideInInspector] public Image avatarImage, currency;
}

[Serializable] public class ClickerShopItem : ShopItem
{
    [HideInInspector] public Text level, clickPower;
}

[Serializable] public class BoosterShopItem : ShopItem
{
    [HideInInspector] public Text amount;
    [HideInInspector] public Button bttnUse;
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
            await System.Threading.Tasks.Task.Delay((int)(1000 / (Time.timeScale + 0.001f)));

            if (EventManager.eventManager != null && hasStart)
            {
                EventManager.eventManager.Click(!GameDataManager.data.isDefend
                    ? allClickPower + (int)(allClickPower * (GameDataManager.data.prestigeLvl * 0.1f))
                    : (int)(allClickPower * (GameDataManager.data.prestigeLvl * 0.1f)));
            }
        }
    }
}

[Serializable] public class OfflineClicker : Clicker, IOfflineClicker
{
    public void RememberTime()
    {
        GameDataManager.data.exitTime = DateTime.Now.ToString();
    }

    public void CalculateProduction()
    {
        if (GameDataManager.data.isDefend)
        {
            MyDebug.LogWarning("Во время битвы оффлайн кликеры не вырабатывают солдат");
            return;
        }
        DateTime exitTime = DateTime.Parse(GameDataManager.data.exitTime);

        TimeSpan offlineTime = DateTime.Now - exitTime;

        int offlineSecs = (int)offlineTime.TotalSeconds;
        
        int totalProduction = offlineSecs * allClickPower;
        totalProduction += (int)(totalProduction * (GameDataManager.data.prestigeLvl * 0.1f));
        
        GameDataManager.data.soldiersCount += totalProduction;
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

            if (EventManager.eventManager != null && hasStart)
            {
                EventManager.eventManager.Click(!GameDataManager.data.isDefend
                    ? allClickPower * 5 + (int)(allClickPower * 5 * (GameDataManager.data.prestigeLvl * 0.1f))
                    : (int)(allClickPower * 5 * (GameDataManager.data.prestigeLvl * 0.1f)));
            }
        }
    }

    public void RememberTime()
    {
        GameDataManager.data.exitTime = DateTime.Now.ToString();
    }

    public void CalculateProduction()
    {
        if (GameDataManager.data.isDefend) return;

        DateTime exitTime = DateTime.Parse(GameDataManager.data.exitTime);

        TimeSpan offlineTime = DateTime.Now - exitTime;

        int offlineSecs = (int)offlineTime.TotalSeconds;

        int totalProduction = offlineSecs * (int)(allClickPower * 1.5f);
        totalProduction += (int)(totalProduction * (GameDataManager.data.prestigeLvl * 0.1f));

        GameDataManager.data.soldiersCount += totalProduction;
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
    public bool IsUsing { get; protected set; }
    public int useTime, abilityModifier;
    [HideInInspector] public int amount, useTimeRemained;

    public abstract void Use();
}

[Serializable] public class TimeBooster : Booster
{
    public override async void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }
        
        EventManager.eventManager.UseBooster(name, false);

        IsUsing = true;

        Time.timeScale *= abilityModifier;

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;
            await System.Threading.Tasks.Task.Delay(1000);
        }

        Time.timeScale /= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(name, true);

        useTimeRemained = useTime;
    }
}

[Serializable] public class SoldierBooster : Booster
{
    public static int SoldierModifier { get; set; } = 1;

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        EventManager.eventManager.UseBooster(name, false);

        IsUsing = true;

        SoldierModifier *= abilityModifier;

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;
            await System.Threading.Tasks.Task.Delay(1000);
        }

        SoldierModifier /= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(name, true);

        useTimeRemained = useTime;
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