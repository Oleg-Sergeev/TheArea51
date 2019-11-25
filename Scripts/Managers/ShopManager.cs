using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class ShopManager : MonoBehaviour
{
    public ManualClickerItem[] manualClickers;
    public AutoClickerItem[] autoClickers;
    public OfflineClickerItem[] offlineClickers;
    public UniversalClickerItem[] universalClickers;
    public SoldierBoosterItem[] soldierBoosters;
    public TimeBoosterItem[] timeBoosters;
    public RegenBoosterItem[] regenBoosters;
    public InstantHealthBoosterItem[] instantHealthBoosters;
    public DefenceBoosterItem[] defenceBoosters;
    public TimeSoldierModifierItem[] timeSoldierModifiers;
    public PermanentSoldierBoostItem[] permamentSoldierBoosts;
    public AlienHeartsItem[] alienHearts;

    public static List<ClickerItem<Clicker>> clickerItems;
    public static List<BoosterItem<Booster>> boosterItems;
    public static List<SpecialAmplificationItem<SpecialAmplification>> specAmplificationItems;
    public static List<OfferItem<Offer>> offerItems;

    private void Awake()
    {
        clickerItems = new List<ClickerItem<Clicker>>();
        boosterItems = new List<BoosterItem<Booster>>();
        specAmplificationItems = new List<SpecialAmplificationItem<SpecialAmplification>>();
        offerItems = new List<OfferItem<Offer>>();

        AddClickerToList(manualClickers);
        AddClickerToList(autoClickers);
        AddClickerToList(offlineClickers);
        AddClickerToList(universalClickers);
        AddBoosterToList(timeBoosters);
        AddBoosterToList(soldierBoosters);
        AddBoosterToList(regenBoosters);
        AddBoosterToList(instantHealthBoosters);
        AddBoosterToList(defenceBoosters);
        AddSpecAmplificationToList(timeSoldierModifiers);
        AddSpecAmplificationToList(permamentSoldierBoosts);
        AddOfferToList(alienHearts);

        void AddClickerToList<T>(ClickerItem<T>[] clickerItemT) where T : Clicker
        {
            foreach (var item in clickerItemT)
            {
                ClickerItem<Clicker> clickerItem = new ClickerItem<Clicker>(item.uiInfo, item.Product);

                clickerItems.Add(clickerItem);
            }

            UI.productsCount.Add(typeof(T));

        }
        void AddBoosterToList<T>(BoosterItem<T>[] boosterItemT) where T : Booster
        {
            foreach (var item in boosterItemT)
            {
                BoosterItem<Booster> boosterItem = new BoosterItem<Booster>(item.uiInfo, item.Product);

                boosterItems.Add(boosterItem);
            }

            UI.productsCount.Add(typeof(T));
        }
        void AddSpecAmplificationToList<T>(SpecialAmplificationItem<T>[] specialAmplificationItemT) where T : SpecialAmplification
        {
            foreach (var item in specialAmplificationItemT)
            {
                SpecialAmplificationItem<SpecialAmplification> samplificationItem = new SpecialAmplificationItem<SpecialAmplification>(item.uiInfo, item.Product);

                specAmplificationItems.Add(samplificationItem);
            }

            UI.productsCount.Add(typeof(T));
        }
        void AddOfferToList<T>(OfferItem<T>[] offerItemsT) where T : Offer
        {
            foreach (var item in offerItemsT)
            {
                OfferItem<Offer> offerItem = new OfferItem<Offer>(item.uiInfo, item.Product);

                offerItems.Add(offerItem);
            }

            UI.productsCount.Add(typeof(T));
        }
    }

    private void Start()
    {
        EventManager.eventManager.OnBuy += Buy;
        IAPManager.OnSuccessfullPurchase += OnSuccessfullPurchase;
    }
    
    private void Buy(Product product, Action<bool> success, bool isFree = false)
    {
        if (product == null)
        {
            MyDebug.LogError($"Clicker not found");

            EventManager.eventManager.FinishBuy(product, false);

            if (success != default) success(false);
            return;
        }

        GameData data = GameDataManager.data;

        if (product is Clicker clicker)
        {
            if (clicker.currentPrice < 0)
            {
                clicker.currentPrice = -clicker.currentPrice;
                SaveManager.Save(GameDataManager.data);

                EventManager.eventManager.FinishBuy(product, false);

                if (success != default) success(false);
                return;
            }

            if (data.isDefend)
            {
                if (clicker.currency == Currency.Soldier) EventManager.eventManager.ChangeHp(isFree ? -clicker.currentPrice : 0);
                else data.aliensHearts -= !isFree ? clicker.currentPrice : 0;
            }
            else
            {
                if (clicker.currency == Currency.Soldier) data.soldiersCount -= !isFree ? clicker.currentPrice : 0;
                else data.aliensHearts -= !isFree ? clicker.currentPrice : 0;
            }

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

            if (!data.products.ContainsKey(clicker.name))
            {
                data.products.Add(clicker.name, clicker);
            }

            if (clicker.currentPrice < 0) clicker.currentPrice = int.MaxValue;

        }
        else if (product is Booster booster)
        {
            if (booster.currency == Currency.Soldier) data.soldiersCount -= !isFree ? booster.priceDefault : 0;
            else if (booster.currency == Currency.AlienHeart) data.aliensHearts -= !isFree ? booster.priceDefault : 0;
            else
            {
                EventManager.eventManager.FinishBuy(product, false);

                if (success != default) success(false);
                return;
            }

            booster.amount++;

            if (booster is IDefendBooster defendBooster) defendBooster.CheckAvailabilityUse();

            if (!data.products.ContainsKey(booster.name)) data.products.Add(booster.name, booster);
        }
        else if (product is SpecialAmplification modifier)
        {
            if (modifier.currentPrice < 0)
            {
                modifier.currentPrice = -modifier.currentPrice;
                SaveManager.Save(GameDataManager.data);

                EventManager.eventManager.FinishBuy(product, false);

                if (success != default) success(false);
                return;
            }

            data.aliensHearts -= !isFree ? modifier.currentPrice : 0;

            modifier.level++;

            if (modifier is PermanentSoldierBoost boost)
            {
                modifier.currentPrice += modifier.priceDefault + (int)(modifier.priceDefault * (float)modifier.level / 10);
                Debug.Log($"{data.permanentSoldierModifier} {(int)modifier.modifierValue}");
                data.permanentSoldierModifier = (int)modifier.modifierValue++;
                Debug.Log($"{data.permanentSoldierModifier} {(int)modifier.modifierValue}");
            }
            else
            {
                modifier.currentPrice += (int)(modifier.priceDefault * ((float)modifier.level / 10));

                data.timerIncreasingValue += modifier.defaultModifierValue;
            }

            if (modifier.currentPrice < 0) modifier.currentPrice = int.MaxValue;

            if (!data.products.ContainsKey(modifier.name)) data.products.Add(modifier.name, modifier);
        }
        else if (product is Offer offer)
        {
            IAPManager.BuyConsumable(offer.productId);

            return;
        }
        else
        {
            if (success != default) success(false);

            EventManager.eventManager.FinishBuy(product, false);

            return;
        }

        if (success != default) success(true);

        EventManager.eventManager.FinishBuy(product, true);

        SaveManager.Save(data);
    }

    private void OnSuccessfullPurchase(PurchaseEventArgs args)
    {
        MyDebug.Log($"Purchased {args.purchasedProduct.definition.id}");

        if (args.purchasedProduct.definition.id.Contains("hearts"))
        {
            GameDataManager.data.aliensHearts += int.Parse(args.purchasedProduct.definition.id.Split(new char[] { '_' })[1]);

            SFXManager.PlaySound(SoundTypes.Buy);

            EventManager.eventManager.FinishBuy(null, true);
        }
    }
}

[Serializable] public abstract class Product
{
    public Currency currency = Currency.Soldier;
    public string name;
    public int priceDefault;

    public static T DownCast<T>(Product product) where T : Product
    {
        if (product is T) return product as T;
        return null;
    }
    public static bool TryDownCast<T>(Product product, out T productT) where T : Product
    {
        productT = null;

        if (product is T)
        {
            productT = product as T;
            return true;
        }

        return false;
    }
}

[Serializable] public abstract class ShopItem
{
    public Sprite avatar;
    [HideInInspector] public Transform uiObject;
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

[Serializable] public class SpecialAmplificationShopItem : ShopItem
{
    [HideInInspector] public Text level, modifierValue;
}

[Serializable] public class OfferShopItem : ShopItem
{
    public Sprite productIcon;
    [HideInInspector] public Image productImage;
    [HideInInspector] public Text productAmount;
}

[Serializable] public class ProductItem<T> where T : Product
{
    [HideInInspector] public ShopItem uiInfo;

    [SerializeField] protected T product;
    [HideInInspector] public T Product { get => product; set => product = value; }

    public ProductItem(ShopItem uiInfo, T product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}

[Serializable] public class ClickerItem<T> : ProductItem<T> where T : Clicker
{
    public new ClickerShopItem uiInfo;
    
    public ClickerItem(ClickerShopItem uiInfo, T product) : base(uiInfo, product)
    {
        this.product = product;
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
        this.product = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class AutoClickerItem : ClickerItem<AutoClicker>
{
    public AutoClickerItem(ClickerShopItem uiInfo, AutoClicker clicker) : base (uiInfo, clicker)
    {
        this.product = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class OfflineClickerItem : ClickerItem<OfflineClicker>
{
    public OfflineClickerItem(ClickerShopItem uiInfo, OfflineClicker clicker) : base(uiInfo, clicker)
    {
        this.product = clicker;
        this.uiInfo = uiInfo;
    }
}
[Serializable] public class UniversalClickerItem : ClickerItem<UniversalClicker>
{
    public UniversalClickerItem(ClickerShopItem uiInfo, UniversalClicker clicker) : base(uiInfo, clicker)
    {
        this.product = clicker;
        this.uiInfo = uiInfo;
    }
}

 
[Serializable] public abstract class Clicker : Product
{
    public int clickPowerDefault;
    [HideInInspector] public int level;
    [HideInInspector] public int allClickPower;
    [HideInInspector] public int currentPrice;
    [HideInInspector] public bool hasBought;
}

[Serializable] public class ManualClicker : Clicker
{

}

[Serializable] public class AutoClicker : Clicker, IAutocliker
{
    [NonSerialized] public bool hasStart;

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
        totalProduction *= GameDataManager.data.permanentSoldierModifier;
        
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
            await System.Threading.Tasks.Task.Delay((int)(1000 / (Time.timeScale + 0.001f)));

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
        totalProduction *= GameDataManager.data.permanentSoldierModifier;

        GameDataManager.data.soldiersCount += totalProduction;
    }
}


[Serializable] public class BoosterItem<T> : ProductItem<T> where T : Booster
{
    public new BoosterShopItem uiInfo;
    
    public BoosterItem(BoosterShopItem uiInfo, T product) : base (uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
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

    public void UpdateIconInfo(bool enableIcon)
    {
        if (uiInfo.boosterIcon.iconObject != null)
        {
            uiInfo.boosterIcon.text.text = Product.CurrentBoosterValue;

            uiInfo.boosterIcon.iconObject.SetActive(enableIcon);
        }
    }
}
[Serializable] public class SoldierBoosterItem : BoosterItem<SoldierBooster>
{
    public SoldierBoosterItem(BoosterShopItem uiInfo, SoldierBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.product = booster;
    }
}
[Serializable] public class TimeBoosterItem : BoosterItem<TimeBooster>
{
    public TimeBoosterItem(BoosterShopItem uiInfo, TimeBooster booster) : base (uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.product = booster;
    }
}
[Serializable] public class RegenBoosterItem : BoosterItem<RegenBooster>
{
    public RegenBoosterItem(BoosterShopItem uiInfo, RegenBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.product = booster;
    }
}
[Serializable] public class InstantHealthBoosterItem : BoosterItem<InstantHealthBooster>
{
    public InstantHealthBoosterItem(BoosterShopItem uiInfo, InstantHealthBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.product = booster;
    }
}
[Serializable] public class DefenceBoosterItem : BoosterItem<DefenceBooster>
{
    public DefenceBoosterItem(BoosterShopItem uiInfo, DefenceBooster booster) : base(uiInfo, booster)
    {
        this.uiInfo = uiInfo;
        this.product = booster;
    }
}

[Serializable] public abstract class Booster : Product
{
    public bool IsUsing { get; protected set; }
    public int useTime;
    public float abilityModifier;
    protected string currentBoosterValue;
    [HideInInspector] public int amount;
    [HideInInspector] public float useTimeRemained;
    [HideInInspector] public abstract string CurrentBoosterValue { get; set; }
    
    public abstract void Use();
}

[Serializable] public class SoldierBooster : Booster
{
    public static float SoldierModifier { get; set; } = 1;
    
    public override string CurrentBoosterValue { get => currentBoosterValue = $"{SoldierModifier}x"; set => currentBoosterValue = value; }

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        IsUsing = true;

        SoldierModifier *= abilityModifier;
        
        EventManager.eventManager.UseBooster(this, false);

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;
            await System.Threading.Tasks.Task.Delay(1000);
        }

        SoldierModifier /= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(this, true);

        useTimeRemained = useTime;
    }
}

[Serializable] public class TimeBooster : Booster
{
    public static float TimeModifier { get; set; } = 0;
    
    public override string CurrentBoosterValue { get => currentBoosterValue = $"{TimeModifier}x"; set => currentBoosterValue = value; }

    public override async void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }
        
        IsUsing = true;

        TimeModifier += abilityModifier;

        Time.timeScale = TimeModifier;

        EventManager.eventManager.UseBooster(this, false);

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;
            await System.Threading.Tasks.Task.Delay(1000);
        }

        TimeModifier -= abilityModifier;

        Time.timeScale = TimeModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(this, true);

        useTimeRemained = useTime;

        if (Time.timeScale <= 0) Time.timeScale = 1;
    }
}

[Serializable] public class RegenBooster : Booster, IDefendBooster
{
    public static float RegenModifier { get; set; } = 0;
    
    public override string CurrentBoosterValue { get => currentBoosterValue = $"{RegenModifier}/{LanguageManager.GetLocalizedText("S")}"; set => currentBoosterValue = value; }

    public void CheckAvailabilityUse()
    {
        BoosterItem<Booster> boosterItem;
        if (UI.TryGetProductItem(name, out ProductItem<Booster> productItem))
        {
            boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
            boosterItem.uiInfo.bttnUse.interactable = GameDataManager.data.isDefend && !IsUsing && amount > 0;
        }
    }

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        IsUsing = true;

        RegenModifier += abilityModifier;

        EventManager.eventManager.UseBooster(this, false);

        while (useTimeRemained > 0)
        {
            useTimeRemained -= Time.deltaTime;

            GameDataManager.OnHpChange((int)abilityModifier);

            await System.Threading.Tasks.Task.Yield();
        }

        RegenModifier -= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(this, true);

        useTimeRemained = useTime;
    }
}

[Serializable] public class InstantHealthBooster : Booster, IDefendBooster
{
    public void CheckAvailabilityUse()
    {
        BoosterItem<Booster> boosterItem;
        if (UI.TryGetProductItem(name, out ProductItem<Booster> productItem))
        {
            boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
            boosterItem.uiInfo.bttnUse.interactable = GameDataManager.data.isDefend && !IsUsing && amount > 0;
        }
    }

    public override string CurrentBoosterValue { get => currentBoosterValue = $"{abilityModifier}x"; set => currentBoosterValue = value; }

    public override void Use()
    {
        if (amount <= 0) return;

        amount--;

        GameDataManager.OnHpChange((int)abilityModifier);

        EventManager.eventManager.UseBooster(this, true);
    }
}

[Serializable] public class DefenceBooster : Booster, IDefendBooster
{
    public static float DefenceModifier { get; set; } = 0;
    
    public override string CurrentBoosterValue { get => currentBoosterValue = $"{DefenceModifier * 100}%"; set => currentBoosterValue = value; }

    public void CheckAvailabilityUse()
    {
        BoosterItem<Booster> boosterItem;
        if (UI.TryGetProductItem(name, out ProductItem<Booster> productItem))
        {
            boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
            boosterItem.uiInfo.bttnUse.interactable = GameDataManager.data.isDefend && !IsUsing && amount > 0;
        }
    }

    public async override void Use()
    {
        if (!IsUsing)
        {
            if (amount <= 0) return;

            amount--;
        }

        IsUsing = true;

        DefenceModifier += abilityModifier;

        EventManager.eventManager.UseBooster(this, false);

        while (useTimeRemained > 0)
        {
            useTimeRemained -= 1;

            await System.Threading.Tasks.Task.Delay(1000);
        }

        DefenceModifier -= abilityModifier;

        IsUsing = false;

        EventManager.eventManager.UseBooster(this, true);

        useTimeRemained = useTime;
    }
}


[Serializable] public class SpecialAmplificationItem<T> : ProductItem<T> where T : SpecialAmplification
{
    public new SpecialAmplificationShopItem uiInfo;
    
    public SpecialAmplificationItem(SpecialAmplificationShopItem uiInfo, T product) : base (uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}
[Serializable] public class TimeSoldierModifierItem : SpecialAmplificationItem<TimeSoldierModifier>
{
    public TimeSoldierModifierItem(SpecialAmplificationShopItem uiInfo, TimeSoldierModifier product) : base(uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}
[Serializable] public class PermanentSoldierBoostItem : SpecialAmplificationItem<PermanentSoldierBoost>
{
    public PermanentSoldierBoostItem(SpecialAmplificationShopItem uiInfo, PermanentSoldierBoost product) : base(uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}

[Serializable] public abstract class SpecialAmplification : Product
{
    public float defaultModifierValue;
    [HideInInspector] public float modifierValue;
    [HideInInspector] public int currentPrice;
    [HideInInspector] public int level;
}

[Serializable] public class TimeSoldierModifier : SpecialAmplification
{

}

[Serializable] public class PermanentSoldierBoost : SpecialAmplification
{

}


[Serializable] public class OfferItem<T> : ProductItem<T> where T : Offer
{
    public new OfferShopItem uiInfo;

    public OfferItem(OfferShopItem uiInfo, T product) : base(uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}
[Serializable] public class AlienHeartsItem : OfferItem<AlienHearts>
{
    public AlienHeartsItem(OfferShopItem uiInfo, AlienHearts product) : base(uiInfo, product)
    {
        this.uiInfo = uiInfo;
        this.product = product;
    }
}

[Serializable] public abstract class Offer : Product
{
    public int productAmount;
    public string productId;
    public bool isConsumable = true;
}

[Serializable] public class AlienHearts : Offer
{

}

public enum Currency
{
    Soldier,
    AlienHeart,
    Rub
}

public interface IAutocliker
{
    void AutoClick();
}

public interface IOfflineClicker
{
    void CalculateProduction();
}

public interface IDefendBooster
{
    void CheckAvailabilityUse();
}