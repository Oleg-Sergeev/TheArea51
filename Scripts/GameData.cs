using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public GameData()
    {
        enemySpawnStep = 0.2f;
        timeToWinLeft = 90f;
        soldiersCount = 51;
        aliensHearts = 0;
        autoClickerBonus = 0;
        clickBonus = 0;
        offlineClickBonus = 0;
        maxHp = 0;
        prestigeLvl = 0;
        fps = 60;
        leapCounter = 3;
        language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        number = "27";
        month = "June";
        year = "2019";
        exitTime = "";
        passwordDebug = "";
        enableSFX = true;
        wasTutorial = false;
        isDefend = false;
        wasAttack = false;
        hasLost = false;
        hasRevertPrestige = false;
        clickers = new Dictionary<string, Clicker>();
        autoClickers = new Dictionary<string, AutoClicker>();
        offlineClickers = new Dictionary<string, OfflineClicker>();
    }

    public float? enemySpawnStep, timeToWinLeft;
    public int soldiersCount, aliensHearts, clickBonus, autoClickerBonus, offlineClickBonus, fps, leapCounter, maxHp, prestigeLvl;
    public string language, number, month, year, exitTime, passwordDebug;
    public bool enableSFX, wasTutorial, wasAttack, isDefend, hasLost, hasRevertPrestige, debugEnabled;

    public Dictionary<string, Clicker> clickers;
    public Dictionary<string, AutoClicker> autoClickers;
    public Dictionary<string, OfflineClicker> offlineClickers;
}
