﻿using System.Collections.Generic;
using System;

[System.Serializable]
public class GameData
{
    public GameData()
    {
        enemySpawnStep = 0.2f;
        timeToWinLeft = 90f;
        dayStep = 60f;
        timerIncreasingValue = 0f;
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
        inversedScale = false;
        products = new Dictionary<string, Product>();
        giftTimer = new GiftTimer(60, 0);
    }

    public DateTime calendar;
    public float? enemySpawnStep, timeToWinLeft, dayStep;
    public float modifierValue, timerIncreasingValue;
    public int soldiersCount, aliensHearts, clickBonus, autoClickerBonus, offlineClickBonus, fps, leapCounter, maxHp, prestigeLvl;
    public string language, number, month, year, exitTime, passwordDebug;
    public bool enableSFX, wasTutorial, wasAttack, isDefend, hasLost, hasRevertPrestige, debugEnabled, inversedScale;
    public GiftTimer giftTimer;
    public Dictionary<Currency, int> currencies;
    public Dictionary<string, Product> products;
    public Dictionary<string, Clicker> clickers;
    public Dictionary<string, Booster> boosters;
    public Dictionary<string, SpecialAmplification> specAmplifications;
}
