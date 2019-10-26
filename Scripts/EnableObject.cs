using UnityEngine;

public class EnableObject : MonoBehaviour
{
    private new string name;


    private void Start()
    {
        name = transform.parent.parent.GetChild(2).name;
    }

    private void OnBecameInvisible()
    {
        ClickerItem<Clicker> clickerItem = UI.GetClickerItem(name);

        if (clickerItem != null) clickerItem.Enable(false);

        else
        {
            BoosterItem<Booster> boosterItem = UI.GetBoosterItem(name);

            if (boosterItem != null) boosterItem.Enable(false);
        }
    }

    private void OnBecameVisible()
    {
        ClickerItem<Clicker> clickerItem = UI.GetClickerItem(name);

        if (clickerItem != null) clickerItem.Enable(true);

        else
        {
            BoosterItem<Booster> boosterItem = UI.GetBoosterItem(name);

            if (boosterItem != null) boosterItem.Enable(true);
        }
    }
}
