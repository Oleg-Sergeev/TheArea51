using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameData data;

    private async void Awake()
    {
        try
        {
            Time.timeScale = 1;

            if ((data = SaveManager.Load()) == null)
            {
                data = new GameData();
            }

            EventManager.eventManager = new EventManager();

            EventManager.eventManager.OnClick += OnClick;

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
        }
        catch (System.Exception e)
        {
            string logPath = Application.dataPath + "/Log.txt";
            string lastLogs = "";
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
            if (System.IO.File.Exists(logPath)) lastLogs = System.IO.File.ReadAllText(logPath);
            System.IO.File.WriteAllText(Application.dataPath + "/Log.txt", $"{lastLogs}\n***Error: {e.StackTrace} /// {e.Message} ***");
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

    public static void BeginDefend()
    {
        EventManager.eventManager.OnHpChange += OnHpChange;

        if (data.maxHp == 0)
        {
            data.maxHp = data.soldiersCount;
            EventManager.eventManager.ChangeHp(data.maxHp);
        }

        PoolManager.Instance.CreatePools();

        data.isDefend = true;
    }

    public static void OnHpChange(int hp)
    {
        if (!data.isDefend) return;

        if (hp > 0)
        {
            if (data.soldiersCount + hp <= data.maxHp)
                data.soldiersCount += hp;
            else
                data.soldiersCount = data.maxHp;
        }
        else if (hp <= 0 && data.soldiersCount >= 0)
        {
            data.soldiersCount += hp;
            if (data.soldiersCount <= 0)
                EventManager.eventManager.EndAttack(false);
        }
    }

    public static void OnClick(int clickCount)
    {
        if (data.isDefend)
        {
            EventManager.eventManager.ChangeHp(clickCount);
            return;
        }

        int lastSoldierCount = data.soldiersCount;
        data.soldiersCount += (int)(clickCount * SoldierBooster.SoldierModifier * UI.Instance.sliderModifier.value);
        if (data.soldiersCount <= 0 && lastSoldierCount < 0) data.soldiersCount = 0;
        else if (data.soldiersCount <= 0 && lastSoldierCount > 0) data.soldiersCount = int.MaxValue;
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
        try
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
        catch (System.Exception e)
        {
            string logPath = Application.dataPath + "/Log.txt";
            string lastLogs = "";
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
            if (System.IO.File.Exists(logPath)) lastLogs = System.IO.File.ReadAllText(logPath);
            System.IO.File.WriteAllText(Application.dataPath + "/Log.txt", $"{lastLogs}\n***Error: {e.StackTrace} /// {e.Message} ***");
        }
    }
}
