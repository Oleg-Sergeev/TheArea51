using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameData data;

    private async void Awake()
    {
        if ((data = SaveManager.Load()) == null)
        {
            data = new GameData();
        }

        EventManager.eventManager = new EventManager();

        CheckForNull(data);

        StartCoroutine(Save());
        
        if (data.clickers.Count != data.clickersCount)
        {
            Debug.LogWarning($"Clickers count ({data.clickers.Count}) != data clickers count ({data.clickersCount})");
            data.hasChangedSaveStructure = true;
            data.clickersCount = 0;
            data.clickBonus = 0;
            data.autoClickerBonus = 0;
            data.offlineClickBonus = 0;
            foreach (var cl in data.clickers)
            {
                if (cl.Value is ManualClicker clicker) data.clickBonus += clicker.allClickPower;
                else if (cl.Value is AutoClicker aclicker) data.autoClickerBonus += aclicker.allClickPower;
                else if (cl.Value is OfflineClicker oclicker) data.offlineClickBonus += oclicker.allClickPower;
                else
                {
                    UniversalClicker uclicker = cl.Value as UniversalClicker;
                    data.clickBonus += uclicker.allClickPower;
                    data.autoClickerBonus += uclicker.allClickPower * 5;
                    data.offlineClickBonus += uclicker.allClickPower * 2;
                }
                data.clickersCount++;
            }
        }

        foreach (var cl in data.clickers)
        {
            IAutocliker autoClicker = cl.Value as IAutocliker;
            autoClicker?.AutoClick();

            await System.Threading.Tasks.Task.Delay(Random.Range(400, 600));
        }

        void CheckForNull(GameData data)
        {
            if (data.clickers == null) data.clickers = new Dictionary<string, Clicker>();
            if (data.boosters == null) data.boosters = new Dictionary<string, Booster>();
            if (data.timeToWinLeft == null) data.timeToWinLeft = 90f;
            if (data.enemySpawnStep == null) data.enemySpawnStep = 0.2f;
            if (data.clickersCount == null) data.clickersCount = data.clickers.Count;
        }
    }

    private IEnumerator Save()
    {
        while (true)
        {
            SaveManager.Save(data);

            yield return new WaitForSeconds(60);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            foreach (var ocl in data.clickers)
            {
                IOfflineClicker offlineClicker = ocl.Value as IOfflineClicker;
                offlineClicker?.RememberTime();
            }
        }
        else
        {
            foreach (var ocl in data.clickers)
            {
                IOfflineClicker offlineClicker = ocl.Value as IOfflineClicker;
                offlineClicker?.CalculateProduction();
            }
        }

        SaveManager.Save(data);
    }

    public static void BeginDefend()
    {
        EventManager.eventManager.OnHpChange += OnHpChanged;

        if (data.maxHp == 0)
        {
            data.maxHp = data.soldiersCount;
            EventManager.eventManager.ChangeHp(data.maxHp);
        }

        data.isDefend = true;
    }

    public static void OnHpChanged(int hp)
    {
        if (!data.isDefend) return;

        if (hp > 0)
        {
            if (data.soldiersCount + hp <= data.maxHp)
                data.soldiersCount += hp;
            else
                data.soldiersCount = data.maxHp;
        }
        else if (hp < 0 && data.soldiersCount > 0)
        {
            data.soldiersCount += hp;
            if (data.soldiersCount <= 0)
                EventManager.eventManager.EndAttack(false);
        }
    }
}
