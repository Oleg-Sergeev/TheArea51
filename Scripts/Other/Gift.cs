using System;
using System.Collections.Generic;
using UnityEngine;

public class Gift : MonoBehaviour
{
    public Reward[] rewardsArr;
    private static List<Reward> rewards;

    private void Start()
    {
        rewards = new List<Reward>();
        foreach (var r in rewardsArr) rewards.Add(r);

        EventManager.eventManager.OnGenerateReward += OnGenerateReward;

        GameDataManager.data.giftTimer.Start();
    }

    private static int GetRewardIndex()
    {
        float allChance = 0;
        for (int i = 0; i < rewards.Count; i++) allChance += rewards[i].dropChance;

        float randPercent = UnityEngine.Random.Range(0f, allChance);

        while (true)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                randPercent -= rewards[i].dropChance;
                if (randPercent <= 0) return i;
            }
        }
    }
    private static int GetAmount(Reward.Range[] ranges)
    {
        float allChances = 0;

        for (int i = 0; i < ranges.Length; i++) allChances += ranges[i].dropChance;

        float randPercent = UnityEngine.Random.Range(0f, allChances);

        while (true)
        {
            for (int i = 0; i < ranges.Length; i++)
            {
                randPercent -= ranges[i].dropChance;
                if (randPercent <= 0) return UnityEngine.Random.Range(ranges[i].min, ranges[i].max);
            }
        }
    }

    private void OnGenerateReward(Action<Reward> onReward)
    {
        Reward reward = rewards[GetRewardIndex()];

        reward.amount = GetAmount(reward.amountRanges);

        GameData data = GameDataManager.data;

        switch (reward.rewardType)
        {
            case RewardTypes.Soldiers:
                data.soldiersCount += reward.amount;
                break;

            case RewardTypes.AlienHearts:
                data.aliensHearts += reward.amount;
                break;

            case RewardTypes.ClickerUp:
                EventManager.eventManager.Buy(UI.GetProduct<Clicker>(reward.name));
                break;

            case RewardTypes.BoosterUp:
                EventManager.eventManager.Buy(UI.GetProduct<Booster>(reward.name));
                break;

            case RewardTypes.SpecAmplifUp:
                EventManager.eventManager.Buy(UI.GetProduct<SpecialAmplification>(reward.name));
                break;
        }

        onReward(reward);
    }
}

[Serializable]
public class Reward
{
    public RewardTypes rewardType;
    public string name;
    [Range(0, 100)] public float dropChance;
    public Range[] amountRanges;
    [HideInInspector] public int amount;

    [Serializable]
    public struct Range
    {
        public int min, max;
        [Range(0,100)] public float dropChance;
    }
}

public enum RewardTypes
{
    Soldiers,
    AlienHearts,
    ClickerUp,
    BoosterUp,
    SpecAmplifUp
}

[Serializable]
public class GiftTimer
{
    public GiftTimer(int min, int sec)
    {
        minute = min;
        second = sec;
    }

    private int second, minute;
    public int Second
    {
        get => second;

        private set
        {
            second = value;

            if (second < 0)
            {
                second = 59;
                Minute--;
            }
        }
    }
    public int Minute
    {
        get => minute;

        private set
        {
            minute = value;

            if (minute < 0) minute = 0;
        }
    }
    public bool IsFinished { get; private set; }
    [NonSerialized] private bool isRunning;


    public void Reset(int min = 60, int sec = 0)
    {
        Minute = min;
        Second = sec;

        IsFinished = false;
    }

    public void Start() => Tick();

    private async void Tick()
    {
        if (isRunning) return;
        isRunning = true;
        
        while ((Minute > 0 || Second > 0) && Application.isPlaying)
        {
            await System.Threading.Tasks.Task.Delay((int)(1000 / Time.timeScale));

            DecreaseSecond();

            EventManager.eventManager.Timer(this);
        }

        isRunning = false;

        void DecreaseSecond()
        {
            Second--;

            if (Minute <= 0 && Second <= 0) IsFinished = true;
        }
    }

    public override string ToString() => $"{Minute:D2}:{Second:D2}";
}
