﻿using System;

public class EventManager
{
    public static EventManager eventManager;

    public event Action<bool> OnEndAttack;
    public event Action<int> OnClick, OnHpChange;
    public event Action<Product, Action<bool>> OnBuy;
    public event Action<string, bool> OnBoosterUsed;
    public event Action OnAnyAction, OnHpHasChanged;

    public void EndAttack(bool isWin)
    {
        OnEndAttack(isWin);
    }
    public void Click(int clickCount = 1)
    {
        OnClick(clickCount);
        OnAnyAction();
    }
    public void ChangeHp(int hp)
    {
        if (OnHpChange != null && OnHpHasChanged != null)
        {
            OnHpChange(hp);
            OnHpHasChanged();
        }
    }
    public void UseBooster(string name, bool hasEnded)
    {
        OnBoosterUsed(name, hasEnded);
    }
    public void Buy<T>(T clicker, Action<bool> success = default) where T : Product
    {
        OnBuy(clicker, success);
        OnAnyAction();
    }
}
