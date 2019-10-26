using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public ManualClickerItem[] manualClickers;
    public AutoClickerItem[] autoClickers;
    public OfflineClickerItem[] offlineClickers;
    public UniversalClickerItem[] universalClickers;
    public ModifierClickerItem[] modifierClickers;
    public SoldierBoosterItem[] soldierBoosters;
    public TimeBoosterItem[] timeBoosters;
    public RegenerationBoosterItem[] regenBoosters;
    public DefenceBoosterItem[] defenceBoosters;

    public static List<ClickerItem<Clicker>> clickerItems;
    public static List<BoosterItem<Booster>> boosterItems;

    private void Awake()
    {
        clickerItems = new List<ClickerItem<Clicker>>();
        boosterItems = new List<BoosterItem<Booster>>();

        AddClickerToList(manualClickers);
        AddClickerToList(autoClickers);
        AddClickerToList(offlineClickers);
        AddClickerToList(universalClickers);
        AddClickerToList(modifierClickers);
        AddBoosterToList(timeBoosters);
        AddBoosterToList(soldierBoosters);
        AddBoosterToList(regenBoosters);
        AddBoosterToList(defenceBoosters);

        void AddClickerToList<T>(ClickerItem<T>[] clickerItemT) where T : Clicker
        {
            foreach (var item in clickerItemT)
            {
                ClickerItem<Clicker> clickerItem = new ClickerItem<Clicker>(item.uiInfo, item.Clicker);

                clickerItems.Add(clickerItem);
            }
        }
        void AddBoosterToList<T>(BoosterItem<T>[] boosterItemT) where T : Booster
        {
            foreach (var item in boosterItemT)
            {
                BoosterItem<Booster> boosterItem = new BoosterItem<Booster>(item.uiInfo, item.Booster);

                boosterItems.Add(boosterItem);
            }
        }
    }

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
            if (clicker.GetType() == typeof(ModifierClicker))
            {
                ModifierClicker mcl = clicker as ModifierClicker;
                data.modifierValue += mcl.modifierValue;
            }

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
    public BoosterIcon boosterIcon;
    [HideInInspector] public Text amount, functional, use;
    [HideInInspector] public Button bttnUse;

    [Serializable] public struct BoosterIcon
    {
        public GameObject iconObject;
        public Sprite picture;
        [HideInInspector] public Text text;
    }
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

    public void Enable(bool enabled)
    {
        uiInfo.clickPower.enabled = enabled;
        uiInfo.currency.enabled = enabled;
        uiInfo.level.enabled = enabled;
        uiInfo.name.enabled = enabled;
        uiInfo.price.enabled = enabled;
        uiInfo.avatarImage.enabled = enabled;
        uiInfo.bttnBuy.GetComponent<Image>().enabled = enabled;
        uiInfo.avatarImage.transform.parent.GetComponent<Image>().enabled = enabled;
        uiInfo.avatarImage.transform.GetChild(0).GetComponent<Image>().enabled = enabled;
        uiInfo.uiObject.GetComponent<Image>().enabled = enabled;
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
[Serializable] public class ModifierClickerItem : ClickerItem<ModifierClicker>
{
    public ModifierClickerItem(ClickerShopItem uiInfo, ModifierClicker clicker) : base(uiInfo, clicker)
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

[Serializable] public abstract class SpecialClicker : Clicker
{
    public abstract void UseAbility();
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

[Serializable] public class ModifierClicker : SpecialClicker
{
    public float modifierValue;

    public override void UseAbility()
    {
        Debug.Log("Use");
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

    public void Enable(bool enabled)
    {
        uiInfo.amount.enabled = enabled;
        uiInfo.currency.enabled = enabled;
        uiInfo.functional.enabled = enabled;
        uiInfo.name.enabled = enabled;
        uiInfo.price.enabled = enabled;
        uiInfo.avatarImage.enabled = enabled;
        uiInfo.bttnBuy.GetComponent<Image>().enabled = enabled;
        uiInfo.bttnUse.GetComponent<Image>().enabled = enabled;
        uiInfo.avatarImage.transform.parent.GetComponent<Image>().enabled = enabled;
        uiInfo.avatarImage.transform.GetChild(0).GetComponent<Image>().enabled = enabled;
        uiInfo.uiObject.GetComponent<Image>().enabled = enabled;
    }

    public virtual void UpdateIconInfo(bool enableIcon)
    {
        uiInfo.boosterIcon.text.text = $"{Booster.abilityModifier}xxx";

        uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
    }
}

[Serializable] public class SoldierBoosterItem : BoosterItem<SoldierBooster>
{
    public SoldierBoosterItem(BoosterShopItem uiInfo, SoldierBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }

    public override void UpdateIconInfo(bool enableIcon)
    {
        uiInfo.boosterIcon.text.text = $"{SoldierBooster.SoldierModifier}x";

        uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
    }
}
[Serializable] public class TimeBoosterItem : BoosterItem<TimeBooster>
{
    public TimeBoosterItem(BoosterShopItem uiInfo, TimeBooster booster) : base (uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }

    public override void UpdateIconInfo(bool enableIcon)
    {
        uiInfo.boosterIcon.text.text = $"{TimeBooster.TimeModifier}x";

        uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
    }
}
[Serializable] public class RegenerationBoosterItem : BoosterItem<RegenerationBooster>
{
    public RegenerationBoosterItem(BoosterShopItem uiInfo, RegenerationBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }

    public override void UpdateIconInfo(bool enableIcon)
    {
        uiInfo.boosterIcon.text.text = $"+{RegenerationBooster.RegenModifier * 60}/{LanguageManager.GetLocalizedText("S")}";

        uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
    }
}
[Serializable] public class DefenceBoosterItem : BoosterItem<DefenceBooster>
{
    public DefenceBoosterItem(BoosterShopItem uiInfo, DefenceBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.booster = booster;
    }

    public override void UpdateIconInfo(bool enableIcon)
    {
        uiInfo.boosterIcon.text.text = $"{DefenceBooster.DefenceModifier * 100}%";

        uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
    }
}

[Serializable] public abstract class Booster : Product
{
    public bool IsUsing { get; protected set; }
    public int useTime;
    public float abilityModifier;
    [HideInInspector] public int amount;
    [HideInInspector] public float useTimeRemained;

    public abstract void Use();
}

[Serializable] public class TimeBooster : Booster
{
    public static float TimeModifier { get; set; } = 0;

    public override async void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }
        
        EventManager.eventManager.UseBooster(name, false);

        IsUsing = true;

        TimeModifier += abilityModifier;

        Time.timeScale += abilityModifier;

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;
            await System.Threading.Tasks.Task.Delay(1000);
        }

        Time.timeScale -= abilityModifier;

        TimeModifier -= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(name, true);

        useTimeRemained = useTime;
    }
}

[Serializable] public class SoldierBooster : Booster
{
    public static float SoldierModifier { get; set; } = 1;

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

[Serializable] public class RegenerationBooster : Booster
{
    public static float RegenModifier { get; set; } = 0;

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        EventManager.eventManager.UseBooster(name, false);

        IsUsing = true;

        RegenModifier += abilityModifier;

        while (useTimeRemained > 0)
        {
            useTimeRemained -= Time.deltaTime;

            GameDataManager.OnHpChange((int)abilityModifier);

            await System.Threading.Tasks.Task.Yield();
        }

        RegenModifier -= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(name, true);

        useTimeRemained = useTime;
    }
}

[Serializable] public class DefenceBooster : Booster
{
    public static float DefenceModifier { get; set; } = 0;

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        EventManager.eventManager.UseBooster(name, false);

        IsUsing = true;

        DefenceModifier += abilityModifier;

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;

            await System.Threading.Tasks.Task.Delay(1000);
        }

        DefenceModifier -= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(name, true);

        useTimeRemained = useTime;
    }
}

public interface IAutocliker
{
    void AutoClick();
}

public interface IOfflineClicker
{
    void RememberTime();

    void CalculateProduction();
}