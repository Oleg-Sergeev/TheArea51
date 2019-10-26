using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;
    public Calendar calendar;
    public RectTransform langArrow, debugClickerMenu, scale;
    public RectTransform[] descriptions;
    public Sprite[] langSpritesArr;
    public Image currentLanguage, toggleSFX, buttonSFX, buttonScale, toggleScale, sliderFPS, sliderModifierColor, activePanelPointer, area51HpImage;
    public Slider sliderModifier, area51Hp;
    public Text soldiers, soldiersFull, aliensHearts, aliensHeartsFull, textOfflineClickerBonus, textAutoclickerBonus, textClickBonus,
        prestigeClickBonus, prestigeAutoclickerBonus, prestigeOfflineClickerBonus, prClicker, prAutoclicker, prOfflineClicker,
        activePanelText, activeSection, prestige, prestigeLvl, gameVersion, debugLng;
    public Text[] localizedTexts;
    public GameObject debugLog, debugModifier, debugButtons, intro, beforeStorm, activeNotesList, prestigeBttn, passwordInput;
    public GameObject[] panelsArr, tutorials, modifiers;
    private static Dictionary<string, ClickerItem<Clicker>> clickItems;
    private static Dictionary<string, BoosterItem<Booster>> boosterItems;
    private static Dictionary<string, GameObject> panels;
    private static Dictionary<string, Sprite> langSprites;
    private static readonly string[] languages = { "ru", "en" };
    private const string PASSWORD = "ZgTA51OV199";


    private void Awake() => Instance = this;

    private void Start()
    {
        if (Application.isEditor) GameDataManager.data.passwordDebug = PASSWORD;

        #region SubscribeEvents
        EventManager.eventManager.OnAnyAction += OnChangeText;
        EventManager.eventManager.OnBoosterUsed += OnBoosterUse;
        Application.lowMemory += OnLowMemory;
        #endregion

        #region Init
        clickItems = new Dictionary<string, ClickerItem<Clicker>>();
        boosterItems = new Dictionary<string, BoosterItem<Booster>>();
        
        panels = new Dictionary<string, GameObject>();

        langSprites = new Dictionary<string, Sprite>();

        calendar.months = new Queue<string>(new string[]
        {
            "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"
        });
        #endregion

        #region Set
        gameVersion.text = $"v{Application.version}";

        prestigeLvl.text = GameDataManager.data.prestigeLvl.ToString();
        #endregion

        prestigeBttn.SetActive(false);

        foreach (var i in langSpritesArr)
        {
            langSprites.Add(i.name, i);
        }

        ChangeLanguage(GameDataManager.data.language);

        calendar.day.text = GameDataManager.data.number;
        calendar.month.text = LanguageManager.GetLocalizedText(GameDataManager.data.month);
        calendar.year.text = GameDataManager.data.year;
        calendar.SynchronizeDate();

        foreach (var i in panelsArr)
        {
            panels.Add(i.name, i);
            panels[i.name].SetActive(false);
        }
        panels["Clickers"].SetActive(true);        

        foreach(var clickerItem in ShopManager.clickerItems)
        {
            clickerItem.Clicker = InitializeClicker(clickerItem.Clicker);
            InitializeClickerInfo(new ClickerItem<Clicker>(clickerItem.uiInfo, clickerItem.Clicker), clickItems);

            clickItems[clickerItem.Clicker.name].Enable(false);
        }
        foreach (var boosterItem in ShopManager.boosterItems)
        {
            boosterItem.Booster = InitializeBooster(boosterItem.Booster);
            InitializeBoosterInfo(new BoosterItem<Booster>(boosterItem.uiInfo, boosterItem.Booster), boosterItems);

            if (boosterItem.Booster.IsUsing) boosterItem.Booster.Use();

            boosterItems[boosterItem.Booster.name].Enable(false);
        }

        if (GameDataManager.data.debugEnabled)
        {
            debugButtons.SetActive(true);
            debugLog.SetActive(true);
        }

        if (!GameDataManager.data.wasTutorial) EnableIntro(true);
        else
        {
            foreach (var i in tutorials) Destroy(i);
            Destroy(intro);
            InvokeRepeating("NextDay", (float)GameDataManager.data.dayStep, (float)GameDataManager.data.dayStep);
        }

        if (GameDataManager.data.isDefend)
        {
            NextDay();
            GameDataManager.data.timeToWinLeft += 5;
        }

        if (GameDataManager.data.wasAttack && !GameDataManager.data.hasLost && !GameDataManager.data.hasRevertPrestige)
        {
            panels["Prestige"].SetActive(true);
        }
        else if (GameDataManager.data.wasAttack && !GameDataManager.data.hasLost && GameDataManager.data.hasRevertPrestige)
        {
            prestigeBttn.SetActive(true);
        }

        if (GameDataManager.data.prestigeLvl > 0)
        {
            //prClicker.gameObject.SetActive(true);
            prAutoclicker.gameObject.SetActive(true);
            prOfflineClicker.gameObject.SetActive(true);
            //prestigeClickBonus.gameObject.SetActive(true);
            prestigeAutoclickerBonus.gameObject.SetActive(true);
            prestigeOfflineClickerBonus.gameObject.SetActive(true);
        }

        SFX(true);
        InverseScale(true);
        FPS(GameDataManager.data.fps);

        OnChangeText();

        GameDataManager.data.modifierValue = 2;

        T InitializeClicker<T>(T clicker) where T : Clicker
        {
            T savedClicker = null;
            Type type = null;

            if (GameDataManager.data.clickers.ContainsKey(clicker.name))
            {
                savedClicker = GameDataManager.data.clickers[clicker.name] as T;
                type = typeof(T);
            }
            else
            {
                savedClicker = clicker;
                savedClicker.currentPrice = savedClicker.priceDefault;
            }

            if (clicker.priceDefault != savedClicker.priceDefault)
            {
                int level = 0;
                int price = clicker.priceDefault;
                for (int x = 0; x < savedClicker.level; x++)
                {
                    price += (clicker.priceDefault / 2) + (clicker.priceDefault * level);
                    level++;
                }
                clicker.currentPrice = price;
                MyDebug.LogWarning($"{clicker.name}'s price has changed: {savedClicker.currentPrice} -> {clicker.currentPrice}");
                savedClicker.currentPrice = clicker.currentPrice;

                savedClicker.priceDefault = clicker.priceDefault;
            }
            if (clicker.clickPowerDefault != savedClicker.clickPowerDefault)
            {
                int allClickPower = 0;
                for (int x = 0; x < savedClicker.level; x++)
                {
                    allClickPower += clicker.clickPowerDefault;
                }
                MyDebug.LogWarning($"{clicker.name}'s click power has changed: {savedClicker.allClickPower} -> {allClickPower}");

                savedClicker.clickPowerDefault = clicker.clickPowerDefault;
                if (type == typeof(ManualClicker) || type == typeof(UniversalClicker))
                {
                    GameDataManager.data.clickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameDataManager.data.clickBonus += savedClicker.allClickPower;
                }
                else if (type == typeof(AutoClicker) || type == typeof(UniversalClicker))
                {
                    GameDataManager.data.autoClickerBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameDataManager.data.autoClickerBonus += savedClicker.allClickPower;
                }
                else if (type == typeof(OfflineClicker) || type == typeof(UniversalClicker))
                {
                    GameDataManager.data.offlineClickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameDataManager.data.offlineClickBonus += savedClicker.allClickPower;
                }
            }
            if (clicker.currency != savedClicker.currency)
            {
                savedClicker.currency = clicker.currency;
            }

            return savedClicker;
        }

        T InitializeBooster<T>(T booster) where T : Booster
        {
            T savedBooster = null;

            if (GameDataManager.data.boosters.ContainsKey(booster.name))
            {
                savedBooster = GameDataManager.data.boosters[booster.name] as T;
            }
            else
            {
                savedBooster = booster;
                savedBooster.useTimeRemained = savedBooster.useTime;
            }

            if (booster.priceDefault != savedBooster.priceDefault) savedBooster.priceDefault = booster.priceDefault;
            if (booster.currency != savedBooster.currency) savedBooster.currency = booster.currency;
            if (booster.abilityModifier != savedBooster.abilityModifier) savedBooster.abilityModifier = booster.abilityModifier;
            if (booster.useTime != savedBooster.useTime)
            {
                savedBooster.useTime = booster.useTime;
                savedBooster.useTimeRemained = booster.useTime;
            }

            return savedBooster;
        }

        void InitializeClickerInfo<T>(T clickerItem, Dictionary<string, T> clickerItems) where T : ClickerItem<Clicker>
        {
            var clicker = clickerItem.Clicker;

            InitializeShopInfo(clickerItem.uiInfo);
            clickerItem.uiInfo.level = clickerItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            clickerItem.uiInfo.clickPower = clickerItem.uiInfo.uiObject.GetChild(5).GetComponent<Text>();

            clickerItems.Add(clicker.name, clickerItem);

            clickerItems[clicker.name].uiInfo.name.name = clicker.name;

            clickerItems[clicker.name].uiInfo.avatarImage.sprite = clickerItem.uiInfo.avatar;
            clickerItems[clicker.name].uiInfo.name.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.name.name);
            clickerItems[clicker.name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
            clickerItems[clicker.name].uiInfo.price.text = FormatMoney(clicker.currentPrice);
            clickerItems[clicker.name].uiInfo.currency.sprite = Resources.Load<Sprite>(clickerItems[clicker.name].Clicker.currency.ToString());
            if (!(clicker is UniversalClicker))
            {
                string key = clicker is ManualClicker ? "Click" : "Sec";
                clickerItems[clicker.name].uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText(key)}";
            }
            else
            {
                clickerItems[clicker.name].uiInfo.clickPower.text =
                    $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}" +
                    $"\n+{clicker.clickPowerDefault * 5}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Auto")}" +
                    $"\n+{(int)(clicker.clickPowerDefault * 1.5f)}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Off_")}";
            }
        }

        void InitializeBoosterInfo<T>(T boosterItem, Dictionary<string, T> boosterItems) where T : BoosterItem<Booster>
        {
            var booster = boosterItem.Booster;

            string key = booster is TimeBooster ? "TimeBoost" : "SoldierBoost";

            InitializeShopInfo(boosterItem.uiInfo);
            boosterItem.uiInfo.amount = boosterItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            boosterItem.uiInfo.use = boosterItem.uiInfo.uiObject.GetChild(5).GetChild(1).GetComponent<Text>();
            boosterItem.uiInfo.functional = boosterItem.uiInfo.uiObject.GetChild(5).GetChild(2).GetComponent<Text>();
            boosterItem.uiInfo.bttnUse = boosterItem.uiInfo.uiObject.GetChild(5).GetComponent<Button>();
            boosterItem.uiInfo.boosterIcon.text = boosterItem.uiInfo.boosterIcon.iconObject.transform.GetChild(2).GetComponent<Text>();
            boosterItem.uiInfo.boosterIcon.iconObject.transform.GetChild(0).GetComponent<Image>().sprite = boosterItem.uiInfo.boosterIcon.picture;

            boosterItems.Add(booster.name, boosterItem);

            boosterItems[booster.name].uiInfo.name.name = booster.name;

            boosterItems[booster.name].uiInfo.avatarImage.sprite = boosterItem.uiInfo.avatar;
            boosterItems[booster.name].uiInfo.name.text = LanguageManager.GetLocalizedText(boosterItem.uiInfo.name.name);
            boosterItems[booster.name].uiInfo.amount.text = $"{booster.amount}x";
            boosterItems[booster.name].uiInfo.functional.text = $"{LanguageManager.GetLocalizedText(key)} {booster.abilityModifier}x - {booster.useTime}{LanguageManager.GetLocalizedText("Sec")}";
            boosterItems[booster.name].uiInfo.use.text = LanguageManager.GetLocalizedText("Use");
            boosterItems[booster.name].uiInfo.price.text = booster.priceDefault.ToString();
            boosterItems[booster.name].uiInfo.currency.sprite = Resources.Load<Sprite>(boosterItems[booster.name].Booster.currency.ToString());
        }

        void InitializeShopInfo(ShopItem shopItem)
        {
            shopItem.avatarImage = shopItem.uiObject.GetChild(0).GetChild(0).GetComponent<Image>();
            shopItem.bttnBuy = shopItem.uiObject.GetChild(1).GetComponent<Button>();
            shopItem.name = shopItem.uiObject.GetChild(2).GetComponent<Text>();
            shopItem.price = shopItem.uiObject.GetChild(4).GetComponent<Text>();
            shopItem.currency = shopItem.uiObject.GetChild(6).GetComponent<Image>();
        }
    }

    public static T GetProduct<T>(string name) where T : Product
    {
        if (clickItems.ContainsKey(name))
        {
            if (clickItems[name].Clicker is T productT) return productT;
            return null;
        }
        else if (boosterItems.ContainsKey(name))
        {
            if (boosterItems[name].Booster is T productT) return productT;
            return null;
        }
        return null;
    }
    public static bool TryGetProduct<T>(string name, out T product) where T : Product
    {
        product = null;

        if (clickItems.ContainsKey(name))
        {
            if (clickItems[name].Clicker is T productT)
            {
                product = productT;
                return true;
            }
        }
        else if (boosterItems.ContainsKey(name))
        {
            if (boosterItems[name].Booster is T productT)
            {
                product = productT;
                return true;
            }
        }

        return false;
    }

    public static ClickerItem<Clicker> GetClickerItem(string name)
    {
        if (clickItems.ContainsKey(name)) return clickItems[name];
        else return null;
    }
    public static BoosterItem<Booster> GetBoosterItem(string name)
    {
        if (boosterItems.ContainsKey(name)) return boosterItems[name];
        else return null;
    }
    public static GameObject GetPanel(string name)
    {
        if (!panels.ContainsKey(name)) return null;
        return panels[name];
    }

    public void Panel_(Text text) => OpenOrClosePanel(text.name, text);
    public void Section(Text text) => OpenOrCloseSection(text.name, text);

    public void OffBeforeStorm()
    {
        Debug.Log("Before");
        beforeStorm.SetActive(false);
        Time.timeScale = timeScale;
        NextDay();
    }

    public void Click()
    {
        EventManager.eventManager.Click(GameDataManager.data.clickBonus);
        IncreaseSpeed();
    }

    private float timeScale;
    private void NextDay()
    {
        if (!GameDataManager.data.wasAttack && calendar.day.text == "20" && calendar.month.text == LanguageManager.GetLocalizedText("Sept"))
        {
            if (!GameDataManager.data.isDefend)
            {
                beforeStorm.SetActive(true);
                timeScale = Time.timeScale;
                Time.timeScale = 0;
                GameDataManager.data.isDefend = true;
                return;
            }

            EventManager.eventManager.OnHpHasChanged += OnHpHasChanged;
            EventManager.eventManager.OnEndAttack += OnEndAttack;

            GameDataManager.BeginDefend();

            area51Hp.gameObject.SetActive(true);
            area51Hp.maxValue = GameDataManager.data.maxHp;

            OnHpHasChanged();

            EnemySpawner.SpawnEnemy();

            CancelInvoke("NextDay");
            return;
        }

        calendar++;
    }

    public void ShowFullCurrencyCount(GameObject name)
    {
        MovingObj obj = MovingObjList.GetObj(name.name);

        if (obj.isActiveAndEnabled) MoveToStartPos(name.name, default, OnClose);
        else
        {
            obj.gameObject.SetActive(true);
            MoveToTarget(name.name);
        }
    }

    private void MoveToTarget(string name, Vector2 newTarget = default, float newSpeed = default, Action<GameObject> onEnd = default)
    {
        MovingObjList.GetObj(name)?.MoveToTarget(newTarget, newSpeed, onEnd);
    }
    private void MoveToStartPos(string name, float newSpeed = default, Action<GameObject> onEnd = default)
    {
        MovingObjList.GetObj(name)?.MoveToStartPos(newSpeed, onEnd);
    }

    private string FormatMoney(double money, FormatType formatType = FormatType.Truncate)
    {
        switch (formatType)
        {
            case FormatType.Truncate:
                return Truncate();
            case FormatType.SplitUp:
                return SplitUp();
            default: return null;
        }

        string Truncate()
        {
            string[] names = { "", "K", "M" };
            int i = 0;

            while (i + 1 < names.Length && money >= 1000)
            {
                money /= 1000;
                i++;
            }
            if (i > 0 && Math.Truncate(money) != money)
            {
                return $"{money.ToString("F2")}{names[i]}";
            }
            return $"{(int)money}{names[i]}";
        }

        string SplitUp() => $"{money:# ### ### ###}";
    }

    private GameObject closingPanel;
    private void OpenOrClosePanel(string name, Text text)
    {
        if (!panels[name].activeSelf)
        {
            if (activePanelText != null)
            {
                activePanelText.color = Color.white;
                closingPanel = panels[activePanelText.name];

                MoveToStartPos(activePanelText.name, default, (obj) =>
                {
                    OnClose(obj);
                    closingPanel = null;
                });

                if (panels["LangSelect"].activeSelf)
                {
                    MoveToStartPos("LangSelect", default, OnClose);
                    langArrow.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
            else activePanelPointer.color = Color.green;

            activePanelText = text;
            activePanelText.color = Color.green;
            activePanelPointer.transform.position = activePanelText.transform.position;

            panels[name].SetActive(true);
            MoveToTarget(name);
        }
        else
        {
            if (activePanelText == null || closingPanel == panels[name]) return;

            activePanelText.color = Color.white;
            activePanelPointer.color = Color.clear;
            activePanelText = null;

            MoveToStartPos(name, default, (obj) =>
            {
                OnClose(obj);
                closingPanel = null;
            });

            if (panels["LangSelect"].activeSelf)
            {
                MoveToStartPos("LangSelect", default, OnClose);
                langArrow.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void OpenOrCloseSection(string name, Text text)
    {
        activeSection.color = Color.white;
        panels[activeSection.name].SetActive(false);

        activeSection = text;

        panels[name].SetActive(true);
        activeSection.color = Color.green;
    }


    #region Modifier
    private float speed = 0, speedAcceleration = 0.002f, speedLimit = 0.005f, timer = 0.2f, timerKoef = 0.1f;

    public void OnChangeValue()
    {
        sliderModifierColor.color = Color.HSVToRGB((sliderModifier.value - 1) / 8, 1, 1);
    }

    float deltaClickTime = 0;
    private void IncreaseSpeed()
    {
        float clickPower = 1 + GameDataManager.data.modifierValue;
        float clickKoef = (float)Math.Pow(0.55f, Math.Floor(sliderModifier.value) - 1);

        timer = (clickPower / sliderModifier.value * 2) * timerKoef;
        if (speedAcceleration * clickKoef <= deltaClickTime) deltaClickTime = 0;
        if (speed <= speedLimit) speed += ((speedAcceleration * clickKoef) - deltaClickTime);

        deltaClickTime = 0;
        ChangeValue();
    }

    private bool isRunning;
    private async void ChangeValue()
    {
        if (isRunning) return;
        isRunning = true;
        sliderModifier.value += speed;

        while (sliderModifier.value > 1)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0)
            {
                if (speed > -speedLimit * sliderModifier.value) speed -= speedLimit / 10;
                else speed = -speedLimit * sliderModifier.value;
            }
            sliderModifier.value += speed;


            deltaClickTime += Time.unscaledDeltaTime;
            await System.Threading.Tasks.Task.Yield();
        }
        speed = 0;
        isRunning = false;
    }

    #endregion /Modifier

    #region Tutorial

    public async void EnableIntro(bool enable)
    {
        if (enable)
        {
            await System.Threading.Tasks.Task.Delay(1500);

            intro.SetActive(true);

            MoveToTarget(intro.name);
        }
        else
        {
            MoveToStartPos(intro.name, default, OnClose);
        }
    }

    public void TutorialGame()
    {
        tutorials[0].SetActive(true);
        tutorials[1].SetActive(true);

        GameObject.Find("BottomUI").transform.GetChild(0).GetComponent<Button>().interactable = false;

        tutorials[0].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialGame_1");
        tutorials[1].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialShop_1");
    }

    public void TutorialBuy()
    {
        string name = "ClickPower";
        ManualClicker clicker = GetProduct<ManualClicker>(name);

        EventManager.eventManager.Buy(clicker, (bool success) =>
        {
            if (success)
            {
                clickItems[name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                clickItems[name].uiInfo.price.text = clicker.currentPrice.ToString();
                clickItems[name].uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";

                tutorials[1].transform.GetChild(1).gameObject.SetActive(false);

                tutorials[0].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialGame_2");
                tutorials[1].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialShop_2");

                tutorials[2].gameObject.SetActive(true);
                tutorials[3].gameObject.SetActive(true);

                SFXManager.PlaySound("Buy");
            }
            else MyDebug.LogError($"Clicker {name} not bought");

            OnChangeText();
        });
    }

    public void EndTutorial()
    {
        foreach (var i in tutorials) Destroy(i);

        GameObject.Find("BottomUI").transform.GetChild(0).GetComponent<Button>().interactable = true;

        GameDataManager.data.wasTutorial = true;

        InvokeRepeating("NextDay", (float)GameDataManager.data.dayStep, (float)GameDataManager.data.dayStep);
    }

    #endregion /Tutorial

    #region OnEvent

    private void OnChangeText()
    {
        if (soldiers == null) return;

        soldiers.text = FormatMoney(GameDataManager.data.soldiersCount);
        soldiersFull.text = FormatMoney(GameDataManager.data.soldiersCount, FormatType.SplitUp);

        aliensHearts.text = FormatMoney(GameDataManager.data.aliensHearts);
        aliensHeartsFull.text = FormatMoney(GameDataManager.data.aliensHearts, FormatType.SplitUp);

        textClickBonus.text = $"+{FormatMoney(GameDataManager.data.clickBonus)}/{LanguageManager.GetLocalizedText("Click")}";

        textAutoclickerBonus.text = $"+{FormatMoney(GameDataManager.data.autoClickerBonus)}/{LanguageManager.GetLocalizedText("Sec")}";

        textOfflineClickerBonus.text = $"+{FormatMoney(GameDataManager.data.offlineClickBonus)}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeAutoclickerBonus.gameObject.activeSelf) prestigeAutoclickerBonus.text = $"+{FormatMoney((int)(GameDataManager.data.autoClickerBonus * (GameDataManager.data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeOfflineClickerBonus.gameObject.activeSelf) prestigeOfflineClickerBonus.text = $"+{FormatMoney((int)(GameDataManager.data.offlineClickBonus * (GameDataManager.data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        if (GameDataManager.data.wasAttack && GameDataManager.data.hasLost && GameDataManager.data.soldiersCount >= 1000000)
        {
            panels["Prestige"].transform.GetChild(2).GetComponent<Button>().interactable = true;
            GameDataManager.data.hasLost = false;
            OpenPrestigePanel();
        }

        foreach (var i in clickItems) CheckPossibilityToBuy(i.Value.Clicker);
        foreach (var i in boosterItems)
        {
            CheckPossibilityToBuy(i.Value.Booster);
            CheckPossibillityToUse(i.Value.Booster);
        }

        void CheckPossibilityToBuy(Product product)
        {
            if (product is Clicker)
            {
                ClickerItem<Clicker> clickerItem = clickItems[product.name];

                if (clickerItem.Clicker.currency == Currency.Soldier)
                {
                    if (GameDataManager.data.soldiersCount < clickerItem.Clicker.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                    else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
                }
                else
                {
                    if (GameDataManager.data.aliensHearts < clickerItem.Clicker.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                    else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
                }
            }
            else if (product is Booster)
            {
                BoosterItem<Booster> boosterItem = boosterItems[product.name];

                if (boosterItem.Booster.currency == Currency.Soldier)
                {
                    if (GameDataManager.data.soldiersCount < boosterItem.Booster.priceDefault) boosterItem.uiInfo.bttnBuy.interactable = false;
                    else if (!boosterItem.uiInfo.bttnBuy.interactable) boosterItem.uiInfo.bttnBuy.interactable = true;
                }
                else
                {
                    if (GameDataManager.data.aliensHearts < boosterItem.Booster.priceDefault) boosterItem.uiInfo.bttnBuy.interactable = false;
                    else if (!boosterItem.uiInfo.bttnBuy.interactable) boosterItem.uiInfo.bttnBuy.interactable = true;
                }
            }
        }

        void CheckPossibillityToUse(Booster booster)
        {
            BoosterShopItem shopItem = boosterItems[booster.name].uiInfo;

            if (booster.amount <= 0) shopItem.bttnUse.interactable = false;
            else if (!shopItem.bttnUse.interactable && !booster.IsUsing) shopItem.bttnUse.interactable = true;
        }
    }

    private void OnHpHasChanged()
    {
        if (GameDataManager.data.soldiersCount < 0) GameDataManager.data.soldiersCount = 0;

        area51Hp.value = GameDataManager.data.soldiersCount;
        soldiers.text = FormatMoney(GameDataManager.data.soldiersCount);

        area51HpImage.color = Color.HSVToRGB(area51Hp.value / (GameDataManager.data.maxHp * 3), 1, 1);
    }

    private void OnEndAttack(bool isWin)
    {
        GameDataManager.data.isDefend = false;
        GameDataManager.data.wasAttack = true;

        area51Hp.gameObject.SetActive(false);

        if (isWin)
        {
            prestige.text = LanguageManager.GetLocalizedText("WinText");
        }
        else
        {
            GameDataManager.data.hasLost = true;
            panels["Prestige"].transform.GetChild(2).GetComponent<Button>().interactable = false;
            prestige.text = LanguageManager.GetLocalizedText("DefeatText");
        }

        panels["Prestige"].SetActive(true);

        InvokeRepeating("NextDay", (float)GameDataManager.data.dayStep, (float)GameDataManager.data.dayStep);

        Time.timeScale = 0;
    }

    private void OnBoosterUse(string name, bool hasEnded)
    {
        if (!boosterItems.ContainsKey(name))
        {
            MyDebug.LogError($"Booster {name} not found");
            return;
        }

        var boosterItem = boosterItems[name];

        boosterItem.uiInfo.bttnUse.interactable = hasEnded && boosterItem.Booster.amount > 0;
        
        boosterItem.UpdateIconInfo(!hasEnded);

        boosterItem.uiInfo.amount.text = $"{boosterItem.Booster.amount}x";
    }
    #endregion /OnEvent

    #region Shop

    public void ShowDescription(Text name)
    {
        RectTransform description = null;

        if (clickItems.ContainsKey(name.name)) description = descriptions[0];
        else if (boosterItems.ContainsKey(name.name)) description = descriptions[1];
        else return;

        if (description.gameObject.activeSelf && description.position == name.transform.parent.GetChild(1).position)
        {
            description.gameObject.SetActive(false);
            return;
        }

        description.gameObject.SetActive(true);
        description.position = name.transform.parent.GetChild(1).position;
        description.GetChild(0).GetComponent<Text>().text = LanguageManager.GetLocalizedText(name.name + "D");
    }

    public void BuyProduct(Text name)
    {
        Product product = GetProduct<Product>(name.name);
        EventManager.eventManager.Buy(product, (bool success) =>
        {
            if (success)
            {
                ChangeInfo();

                if (product is IAutocliker acl)
                {
                    acl.AutoClick();
                }

                SFXManager.PlaySound("Buy");

                void ChangeInfo()
                {
                    if (product is Clicker clicker)
                    {
                        ClickerItem<Clicker> clickerItem = clickItems[clicker.name];

                        clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                        clickerItem.uiInfo.price.text = FormatMoney(clicker.currentPrice);
                        if (clicker is UniversalClicker)
                        {
                            clickerItem.uiInfo.clickPower.text =
                            $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}" +
                            $"\n+{clicker.clickPowerDefault * 5}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Auto")}" +
                            $"\n+{(int)(clicker.clickPowerDefault * 1.5f)}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Off_")}";
                        }
                        else
                        {
                            string key = clicker is ManualClicker ? "Click" : "Sec";
                            clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText(key)}";
                        }
                    }
                    else if (product is Booster booster)
                    {
                        BoosterItem<Booster> boosterItem = boosterItems[booster.name];

                        boosterItem.uiInfo.amount.text = $"{booster.amount}x";
                        boosterItem.uiInfo.price.text = FormatMoney(booster.priceDefault);
                    }
                    else MyDebug.LogWarning($"Product {name.name} not found");
                }
            }
            else MyDebug.LogError($"Clicker {name.name} not bought");

            OnChangeText();
        });
    }

    public void Clickers(Text text)
    {
        OpenOrClosePanel(text.name, text);
    }
    public void Warriors(Text text)
    {
        OpenOrClosePanel(text.name, text);
    }
    public void Boosters(Text text)
    {
        OpenOrClosePanel(text.name, text);
    }

    #region BoosterAbilities

    public void SpeedUpTime(Text name)
    {
        if (TryGetProduct(name.name, out TimeBooster timeBooster))
            timeBooster.Use();
        else MyDebug.LogError($"Time booster {name.name} not found");
    }

    public void IncreaseSoldiers(Text name)
    {
        if (TryGetProduct(name.name, out SoldierBooster soldierBooster))
            soldierBooster.Use();
        else MyDebug.LogError($"Soldier booster {name.name} not found");
    }

    public void Regen(Text name)
    {
        if (TryGetProduct(name.name, out RegenerationBooster regenBooster))
            regenBooster.Use();
        else MyDebug.LogError($"Regen booster {name.name} not found");
    }

    public void Defence(Text name)
    {
        if (TryGetProduct(name.name, out DefenceBooster defenceBooster))
            defenceBooster.Use();
        else MyDebug.LogError($"Defence booster {name.name} not found");
    }

    #endregion

    #endregion /Shop

    #region More

    #region Settings

    public void SFX(string name) => SFXManager.PlaySound(name);

    public void SFX(bool isStart = false)
    {
        if (isStart) SFXManager.EnableSound(GameDataManager.data.enableSFX);
        else SFXManager.EnableSound(!GameDataManager.data.enableSFX);

        if (GameDataManager.data.enableSFX)
        {
            buttonSFX.color = new Color(0, 0.75f, 0);
            toggleSFX.rectTransform.anchoredPosition = new Vector2(150, 0);
        }
        else
        {
            buttonSFX.color = new Color(0.75f, 0, 0);
            toggleSFX.rectTransform.anchoredPosition = new Vector2(0, 0);
        }
    }

    public void InverseScale(bool isStart = false)
    {
        bool inverseScale = GameDataManager.data.inversedScale;
        if (isStart) ChangePosition(inverseScale);
        else
        {
            ChangePosition(!inverseScale);

            GameDataManager.data.inversedScale = !inverseScale;
        }

        if (GameDataManager.data.inversedScale)
        {
            buttonScale.color = new Color(0, 0.75f, 0);
            toggleScale.rectTransform.anchoredPosition = new Vector2(150, 0);
        }
        else
        {
            buttonScale.color = new Color(0.75f, 0, 0);
            toggleScale.rectTransform.anchoredPosition = new Vector2(0, 0);
        }

        void ChangePosition(bool inverse)
        {
            scale.anchoredPosition = inverse ? new Vector2(-750, 0) : new Vector2(0, 0);
        }
    }

    public void FPS(int fps)
    {
        Application.targetFrameRate = fps;
        GameDataManager.data.fps = fps;

        switch (GameDataManager.data.fps)
        {
            case 30:
                sliderFPS.rectTransform.anchoredPosition = new Vector2(-145, 0);
                break;

            case 60:
                sliderFPS.rectTransform.anchoredPosition = new Vector2(0, 0);
                break;
        }
    }
    public void FPS()
    {
        switch (GameDataManager.data.fps)
        {
            case 30:
                Application.targetFrameRate = 60;
                GameDataManager.data.fps = 60;

                sliderFPS.rectTransform.anchoredPosition = new Vector2(0, 0);
                break;

            case 60:
                Application.targetFrameRate = 30;
                GameDataManager.data.fps = 30;

                sliderFPS.rectTransform.anchoredPosition = new Vector2(-145, 0);
                break;
        }
    }

    public void Languages()
    {
        string name = "LangSelect";
        if (!panels[name].activeSelf)
        {
            panels[name].SetActive(true);

            MoveToTarget(name);

            langArrow.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            MoveToStartPos(name, default, OnClose);

            langArrow.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public void ChangeLanguage(string lang)
    {
        LanguageManager.ChangeLanguage(lang);

        currentLanguage.sprite = langSprites[GameDataManager.data.language];

        ChangeShopItemsInfo();
        ChangeTexts();
        OnChangeText();

        MoveToStartPos("LangSelect", default, OnClose);

        langArrow.rotation = Quaternion.Euler(0, 0, 0);

        void ChangeShopItemsInfo()
        {
            foreach (var i in clickItems) ChangeClickerItemInfo(i.Value);
            foreach (var i in boosterItems) ChangeBoosterItemInfo(i.Value);

            void ChangeClickerItemInfo(ClickerItem<Clicker> clickerItem)
            {
                Clicker clicker = clickerItem.Clicker;
                clickerItem.uiInfo.name.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.name.name);
                clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";

                if (!(clicker is UniversalClicker))
                {
                    string key = clicker is ManualClicker ? "Click" : "Sec";
                    clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText(key)}";
                }
                else
                {
                    clickerItem.uiInfo.clickPower.text =
                        $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}" +
                        $"\n+{clicker.clickPowerDefault * 5}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Auto")}" +
                        $"\n+{(int)(clicker.clickPowerDefault * 1.5f)}/{LanguageManager.GetLocalizedText("Sec")} {LanguageManager.GetLocalizedText("Off_")}";
                }
            }

            void ChangeBoosterItemInfo(BoosterItem<Booster> boosterItem)
            {
                Booster booster = boosterItem.Booster;
                string key = booster is TimeBooster ? "TimeBoost" : "SoldierBoost";

                boosterItem.uiInfo.name.text = LanguageManager.GetLocalizedText(boosterItem.uiInfo.name.name);
                boosterItem.uiInfo.functional.text = $"{LanguageManager.GetLocalizedText(key)} {booster.abilityModifier}x - {booster.useTime}{LanguageManager.GetLocalizedText("Sec")}";
                boosterItem.uiInfo.use.text = LanguageManager.GetLocalizedText("Use");
            }
        }
        void ChangeTexts()
        {
            foreach (var t in localizedTexts) if (t != null) t.text = LanguageManager.GetLocalizedText(t.name);

            calendar.month.text = LanguageManager.GetLocalizedText(calendar.months.Peek());
        }
    }

    private void OnClose(GameObject objToClose)
    {
        objToClose?.SetActive(false);
    }

    #endregion /Settings

    #endregion /More

    #region Prestige

    public void ApplyPrestige()
    {
        Time.timeScale = 1;

        MovingObjList.Clear();

        int aliensHearts = GameDataManager.data.aliensHearts;
        int prestige = GameDataManager.data.prestigeLvl;
        float? timeToWinLeft = GameDataManager.data.timeToWinLeft;
        float? enemySpawnStep = GameDataManager.data.enemySpawnStep;
        string password = GameDataManager.data.passwordDebug;
        string language = GameDataManager.data.language;
        bool debugEnabled = GameDataManager.data.debugEnabled;
        bool wasTutorial = GameDataManager.data.wasTutorial;

        List<Clicker> clickers = new List<Clicker>();
        List<Booster> boosters = new List<Booster>();

        foreach (var cl in GameDataManager.data.clickers)
        {
            if (cl.Value is AutoClicker ac) ac.hasStart = false;
            else if (cl.Value is UniversalClicker uc) uc.hasStart = false;

            if (cl.Value.currency == Currency.AlienHeart) clickers.Add(cl.Value);
        }
        foreach (var b in GameDataManager.data.boosters)
        {
            if (b.Value.currency == Currency.AlienHeart) boosters.Add(b.Value);
        }

        GameDataManager.data = new GameData();

        prestige++;
        timeToWinLeft = 90 + (30 * prestige);
        if (enemySpawnStep >= 0.02f) enemySpawnStep *= 0.75f;

        GameDataManager.data.aliensHearts = aliensHearts + (prestige * 1000);
        GameDataManager.data.soldiersCount = 1000000 * prestige + 51;
        GameDataManager.data.prestigeLvl = prestige;
        GameDataManager.data.timeToWinLeft = timeToWinLeft;
        GameDataManager.data.enemySpawnStep = enemySpawnStep;
        GameDataManager.data.passwordDebug = password;
        GameDataManager.data.language = language;
        GameDataManager.data.debugEnabled = debugEnabled;
        GameDataManager.data.wasTutorial = wasTutorial;

        foreach (var b in boosters) GameDataManager.data.boosters.Add(b.name, b);
        foreach (var cl in clickers) GameDataManager.data.clickers.Add(cl.name, cl);
        GameDataManager.data.clickersCount = clickers.Count;

        SaveManager.Save(GameDataManager.data);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void RevertPrestige()
    {
        Time.timeScale = 1;
        GameDataManager.data.hasRevertPrestige = true;
        if (!GameDataManager.data.hasLost) prestigeBttn.SetActive(true);
        panels["Prestige"].SetActive(false);
    }

    public void OpenPrestigePanel()
    {
        panels["Prestige"].SetActive(true);
        prestige.text = LanguageManager.GetLocalizedText("GetPrestige");
    }

    #endregion Prestige

    #region Debug

    public void EnableDebug()
    {
        if (string.IsNullOrEmpty(GameDataManager.data.passwordDebug))
        {
            passwordInput.SetActive(true);
            return;
        }
        GameDataManager.data.debugEnabled = !GameDataManager.data.debugEnabled;

        debugLog.SetActive(GameDataManager.data.debugEnabled);
        debugButtons.SetActive(GameDataManager.data.debugEnabled);
        //debugModifier.SetActive(MyDebug.debugEnabled);
    }

    public void EnterPassword(Text password)
    {
        passwordInput.SetActive(false);

        if (password.text != PASSWORD) return;

        GameDataManager.data.passwordDebug = PASSWORD;
        EnableDebug();
    }

    public void IncreaseSoldiers(int soldiers)
    {
        if (GameDataManager.data.soldiersCount + soldiers > 0)
        {
            GameDataManager.data.soldiersCount += soldiers;
            GameDataManager.data.aliensHearts += soldiers / 1000;
            OnChangeText();
        }
    }

    public void ChangeTimeScale()
    {
        Time.timeScale = Time.timeScale % 8 + 0.5f;
        MyDebug.Log($"time scale - {Time.timeScale}");
    }

    public void ResetGame()
    {
        SaveManager.deleteGame = true;
        SaveManager.Save(null);
        Application.Quit();
    }

    public void SkipToBattle()
    {
        while(calendar.day.text != "20" || calendar.month.text != LanguageManager.GetLocalizedText("Sept"))
        {
            NextDay();
        }
    }

    public void OnLowMemory()
    {
        MyDebug.LogWarning($"*** Low memory ***");
        MyDebug.LogWarning($"*** Low memory ***");
        MyDebug.LogWarning($"*** Low memory ***");
    }

    #endregion /Debug

    [Serializable]
    public struct Calendar
    {
        public Text month, day, year;
        public Queue<string> months;
        public List<string> months30Ending, months31Ending;

        public static Calendar operator ++(Calendar calendar)
        {
            if ((calendar.day.text == "30" && calendar.months30Ending.Contains(calendar.months.Peek())) ||
                (calendar.day.text == "31" && calendar.months31Ending.Contains(calendar.months.Peek())) ||
                (calendar.day.text == (GameDataManager.data.leapCounter != 4 ? "28" : "29") && calendar.months.Peek() == "Feb"))
            {
                calendar.NextMonth();
            }
            calendar.day.text = (int.Parse(calendar.day.text) + 1).ToString();

            GameDataManager.data.number = calendar.day.text;

            return calendar;
        }

        public void SynchronizeDate()
        {
            while (LanguageManager.GetLocalizedText(GameDataManager.data.month) != LanguageManager.GetLocalizedText(months.Peek()))
            {
                var tempMonth = months.Dequeue();
                months.Enqueue(tempMonth);
            }
        }

        private void NextMonth()
        {
            string currentMonth = months.Dequeue();
            month.text = LanguageManager.GetLocalizedText(months.Peek());
            months.Enqueue(currentMonth);
            day.text = "0";

            if (currentMonth == "Dec")
            {
                year.text = (int.Parse(year.text) + 1).ToString();
                GameDataManager.data.year = year.text;

                if (GameDataManager.data.leapCounter < 4) GameDataManager.data.leapCounter++;
                else GameDataManager.data.leapCounter = 1;
            }

            GameDataManager.data.month = months.Peek();
        }
    }

    private enum FormatType
    {
        Truncate,
        SplitUp
    }
}

