using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameData data;
    public static Notification giftNotification;

    private async void Awake()
    {
        Time.timeScale = 1;

        if ((data = SaveManager.Load()) == null)
        {
            data = new GameData();
        }

        EventManager.eventManager = new EventManager();

        EventManager.eventManager.OnClick += OnClick;

        CheckForNull(data);

        data.timerIncreasingValue = data.modifierValue;

        CheckDate(data);

        StartCoroutine(Save());

        foreach (var acl in data.products)
        {
            if (acl.Value is IAutocliker autocliker)
            {
                autocliker.AutoClick();

                await System.Threading.Tasks.Task.Delay(UnityEngine.Random.Range(400, 600));
            }
        }

        giftNotification = new Notification();

        giftNotification.CreateNotificationChannel("Gift", "OpenGift", "Open gift", Unity.Notifications.Android.Importance.Default);

        void CheckForNull(GameData data)
        {
            if (data.products == null) data.products = new Dictionary<string, Product>();
            if (data.products == null) data.products = new Dictionary<string, Product>();
            if (data.timeToWinLeft == null) data.timeToWinLeft = 90f;
            if (data.enemySpawnStep == null) data.enemySpawnStep = 0.2f;
            if (data.timerSkipKoef == null) data.timerSkipKoef = 2.5f;
            if (data.permanentSoldierModifier == 0) data.permanentSoldierModifier = 1;
            if (data.dayStep == null) data.dayStep = 60f;
            if (data.giftTimer == default) data.giftTimer = new GiftTimer(60, 0);
        }
        void CheckDate(GameData data)
        {
            if (DateTime.TryParse(data.exitTime, out DateTime exitTime))
            {
                DateTime currentTime = DateTime.Now;
                if (currentTime < exitTime)
                {
                    MyDebug.LogWarning("Перемотка времени detected");
                    data.soldiersCount /= 2;
                    data.aliensHearts /= 2;
                }
            }
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
            hp = -hp;
            data.soldiersCount -= hp - (int)(DefenceBooster.DefenceModifier * hp);
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
        data.soldiersCount += (int)(clickCount * SoldierBooster.SoldierModifier * data.permanentSoldierModifier * Mathf.Floor(UI.Instance.sliderModifier.value));
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
        if (!focus) data.exitTime = DateTime.Now.ToString();
        else
        {
            foreach (var p in data.products)
            {
                if (p.Value is IOfflineClicker offlineClicker) offlineClicker.CalculateProduction();
            }
        }

        SaveManager.Save(data);
    }
    private void OnApplicationPause(bool pause)
    {
        GameData data = GameDataManager.data;

        if (pause)
        {
            data.exitTime = DateTime.Now.ToString();

            TimeSpan ts = new TimeSpan(0, data.giftTimer.Minute, data.giftTimer.Second);

            if (ts.TotalSeconds > 60)
                giftNotification.SendNotification(giftNotification.CreateAndroidNotification("The Area 51", LanguageManager.GetLocalizedText("TimeToOpenGift"), DateTime.Now.Add(ts)), giftNotification.AndroidChannel.Id);
        }
        else
        {
            if (DateTime.TryParse(data.exitTime, out DateTime time))
            {
                data.giftTimer.DecreaseTime((int)(DateTime.Now - DateTime.Parse(data.exitTime)).TotalSeconds);
                
                Notification.CancelAllNotifications();
            }
        }
    }
}
