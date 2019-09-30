using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public ClickerItem[] clickers;
    public AutoClickerItem[] autoClickers;
    public OfflineClickerItem[] offlineClickers;


    private void Awake() => Instance = this;

    private void Start()
    {
        Area51Controller.mainEvent.OnBuy += Buy;
    }

    private void Buy(Clicker clicker, Action<bool> success = default)
    {
        if (clicker == null)
        {
            MyDebug.LogError($"Clicker {clicker.name} not found");
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

        if (!data.isDefend) data.soldiersCount -= clicker.price;
        else Area51Controller.mainEvent.ChangeHp(-clicker.price);

        clicker.hasBought = true;

        clicker.allClickPower += clicker.clickPowerDefault;
        clicker.price += (clicker.priceDefault / 2) + (clicker.priceDefault * clicker.level);
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
