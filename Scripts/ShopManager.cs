using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public ClickerItem[] clickers;
    public AutoClickerItem[] autoClickers;
    public OfflineClickerItem[] offlineClickers;
    public UniversalClickerItem[] universalClickers;


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

        if (clicker.price < 0)
        {
            clicker.price = -clicker.price;
            SaveManager.Save(GameManager.data);
            success(false);
            return;
        }

        GameData data = GameManager.data;

        if (!data.isDefend)
        {
            if (clicker.currency == Currency.Soldier) data.soldiersCount -= clicker.price;
            else data.aliensHearts -= clicker.price;
        }
        else Area51Controller.mainEvent.ChangeHp(-clicker.price);

        clicker.hasBought = true;

        clicker.allClickPower += clicker.clickPowerDefault;

        if (clicker.currency == Currency.Soldier)
            clicker.price += (clicker.priceDefault / 2) + (clicker.priceDefault * clicker.level);
        else
            clicker.price += clicker.priceDefault / 5 * (clicker.level + 1);

        clicker.level++;

        if (clicker.GetType() == typeof(AutoClicker))
        {
            data.autoClickerBonus += clicker.clickPowerDefault;

            if (!data.autoClickers.ContainsKey(clicker.name)) data.autoClickers.Add(clicker.name, (AutoClicker)clicker);
        }
        else if (clicker.GetType() == typeof(OfflineClicker))
        {
            data.offlineClickBonus += clicker.clickPowerDefault;

            if (!data.offlineClickers.ContainsKey(clicker.name)) data.offlineClickers.Add(clicker.name, (OfflineClicker)clicker);
        }
        else if (clicker.GetType() == typeof(UniversalClicker))
        {
            data.clickBonus += clicker.clickPowerDefault;
            data.autoClickerBonus += clicker.clickPowerDefault * 5;
            data.offlineClickBonus += clicker.clickPowerDefault * 5;

            if (!data.universalClickers.ContainsKey(clicker.name)) data.universalClickers.Add(clicker.name, (UniversalClicker)clicker);
        }
        else
        {
            data.clickBonus += clicker.clickPowerDefault;
            if (!data.clickers.ContainsKey(clicker.name)) data.clickers.Add(clicker.name, clicker);
        }

        if (clicker.price < 0) clicker.price = int.MaxValue;

        SaveManager.Save(data);

        success(true);
    }
}

public enum Currency
{
    Soldier,
    AliensHeart
}
