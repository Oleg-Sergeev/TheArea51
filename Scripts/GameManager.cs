using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameData data;

    private async void Awake()
    {
        if ((data = SaveManager.Load()) == null)
        {
            data = new GameData();
        }

        CheckForNull(data);

        foreach (var acl in data.clickers)
        {
            IAutocliker autoClicker = acl.Value as IAutocliker;
            autoClicker?.AutoClick();

            await System.Threading.Tasks.Task.Delay(Random.Range(400, 600));
        }

        StartCoroutine(Save());

        void CheckForNull(GameData data)
        {
            if (data.clickers == null) data.clickers = new Dictionary<string, Clicker>();
            if (data.timeToWinLeft == null) data.timeToWinLeft = 90f;
            if (data.enemySpawnStep == null) data.enemySpawnStep = 0.2f;
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
}
