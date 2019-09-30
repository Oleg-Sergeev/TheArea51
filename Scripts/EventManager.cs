using System;

public class EventManager
{
    public event Action<bool> OnEndAttack;
    public event Action<int> OnClick, OnHpChange;
    public event Action<Clicker, Action<bool>> OnBuy;
    public event Action OnAnyAction, OnHpHasChanged;

    public void Click(int clickCount = 1)
    {
        OnClick(clickCount);
        OnAnyAction();
    }
    public void Buy(Clicker clicker, Action<bool> success = default)
    {
        OnBuy(clicker, success);
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
    public void EndAttack(bool isWin)
    {
        OnEndAttack(isWin);
    }
}

