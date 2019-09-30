using UnityEngine;

public class Area51Controller : MonoBehaviour
{
    public static EventManager mainEvent;

    private void Awake()
    {
        mainEvent = new EventManager();
    }

    public void Click()
    {
        mainEvent.Click(GameManager.data.clickBonus);
    }

    public static void BeginDefend()
    {
        mainEvent.OnHpChange += OnHpChanged;

        if (GameManager.data.maxHp == 0)
        {
            GameManager.data.maxHp = GameManager.data.soldiersCount;
            mainEvent.ChangeHp(GameManager.data.maxHp);
        }
        
        GameManager.data.isDefend = true;

        MyDebug.Log("Начало битвы");
    }

    public static void OnHpChanged(int hp)
    {
        if (!GameManager.data.isDefend) return;

        if (hp > 0)
        {
            if (GameManager.data.soldiersCount + hp <= GameManager.data.maxHp)
                GameManager.data.soldiersCount += hp;
            else
                GameManager.data.soldiersCount = GameManager.data.maxHp;
        }
        else if (hp < 0 && GameManager.data.soldiersCount > 0)
        {
            GameManager.data.soldiersCount += hp;
            if (GameManager.data.soldiersCount <= 0)
                mainEvent.EndAttack(false);
        }
    }
}
