using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;
    private static readonly string[] languages = { "ru", "en" };
    private const string PASSWORD = "ZgTA51OV199";
    public Calendar calendar;
    public RectTransform rect, description, debugClickerMenu, testVP;
    public Sprite[] langSpritesArr;
    public Image currentLanguage, toggleSFX, buttonSFX, sliderFPS, activePanelPointer, area51HpImage;
    public Slider sliderModifier, area51Hp;
    public Text soldiers, aliensHearts, textOfflineClickerBonus, textAutoclickerBonus, textClickBonus,
        prestigeClickBonus, prestigeAutoclickerBonus, prestigeOfflineClickerBonus, prClicker, prAutoclicker, prOfflineClicker,
        textModifier, activePanel, activeSection, prestige, prestigeLvl, gameVersion, debugLng;
    public Text[] localizedTexts;
    public GameObject debugLog, debugModifier, debugButtons, intro, beforeStorm, activeNotesList, modifier, prestigeBttn, passwordInput;
    public GameObject[] panelsArr, tutorials;
    private static Dictionary<string, ClickerItem<Clicker>> clickItems;
    private static Dictionary<string, BoosterItem<Booster>> boosters;
    private static Dictionary<string, Panel> panels;
    private static Dictionary<string, Sprite> langSprites;

    float speed = 0, heightVP, calendarDay = 60;


    private void Awake() => Instance = this;
    
    private void Start()
    {
        if (Application.isEditor) GameManager.data.passwordDebug = PASSWORD;

        #region SubscribeEvents
        Area51Controller.mainEvent.OnClick += OnClick;
        Area51Controller.mainEvent.OnAnyAction += OnChangeText;
        Application.lowMemory += OnLowMemory;
        #endregion

        #region Init
        clickItems = new Dictionary<string, ClickerItem<Clicker>>();
        boosters = new Dictionary<string, BoosterItem<Booster>>();
        panels = new Dictionary<string, Panel>();

        langSprites = new Dictionary<string, Sprite>();

        calendar.months = new Queue<string>(new string[]
        {
            "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"
        });
        #endregion

        #region Set
        gameVersion.text = $"v{Application.version}";

        prestigeLvl.text = GameManager.data.prestigeLvl.ToString();

        heightVP = -testVP.rect.height + 400;

        sliderModifier.minValue = 1;
        sliderModifier.value = 1;
        sliderModifier.maxValue = 5;
        textModifier.text = $"{sliderModifier.value}x";
        #endregion
        
        modifier.SetActive(false);

        prestigeBttn.SetActive(false);

        foreach (var i in langSpritesArr)
        {
            langSprites.Add(i.name, i);
        }

        ChangeLanguage(GameManager.data.language);

        calendar.number.text = GameManager.data.number;
        calendar.month.text = LanguageManager.GetLocalizedText(GameManager.data.month);
        calendar.year.text = GameManager.data.year;
        calendar.SynchronizeDate();

        foreach (var i in panelsArr)
        {
            Panel panel = new Panel(i, i.GetComponent<Animation>());
            panels.Add(i.name, panel);
            panels[i.name].panel.SetActive(false);
        }
        panels["Clickers"].panel.SetActive(true);

        foreach (var i in ShopManager.Instance.manualClickers)
        {
            i.Clicker = InitializeClicker(i.Clicker);
            InitializeClickerInfo(new ClickerItem<Clicker>(i.uiInfo, i.Clicker), clickItems);
        }
        foreach (var i in ShopManager.Instance.autoClickers)
        {
            i.Clicker = InitializeClicker(i.Clicker);
            InitializeClickerInfo(new ClickerItem<Clicker>(i.uiInfo, i.Clicker), clickItems);
        }
        foreach (var i in ShopManager.Instance.offlineClickers)
        {
            i.Clicker = InitializeClicker(i.Clicker);
            InitializeClickerInfo(new ClickerItem<Clicker>(i.uiInfo, i.Clicker), clickItems);
        }
        foreach (var i in ShopManager.Instance.universalClickers)
        {
            i.Clicker = InitializeClicker(i.Clicker);
            InitializeClickerInfo(new ClickerItem<Clicker>(i.uiInfo, i.Clicker), clickItems);
        }

        if (GameManager.data.debugEnabled)
        {
            debugButtons.SetActive(true);
            debugLog.SetActive(true);
        }

        if (!GameManager.data.wasTutorial) EnableIntro(true);
        else
        {
            foreach (var i in tutorials) Destroy(i);
            InvokeRepeating("NextDay", calendarDay, calendarDay);
        }

        if (GameManager.data.isDefend)
        {
            NextDay();
            GameManager.data.timeToWinLeft += 5;
        }

        if (GameManager.data.wasAttack && !GameManager.data.hasLost && !GameManager.data.hasRevertPrestige)
        {
            panels["Prestige"].panel.SetActive(true);
        }
        else if (GameManager.data.wasAttack && !GameManager.data.hasLost && GameManager.data.hasRevertPrestige)
        {
            prestigeBttn.SetActive(true);
        }

        if (GameManager.data.prestigeLvl > 0)
        {
            //prClicker.gameObject.SetActive(true);
            prAutoclicker.gameObject.SetActive(true);
            prOfflineClicker.gameObject.SetActive(true);
            //prestigeClickBonus.gameObject.SetActive(true);
            prestigeAutoclickerBonus.gameObject.SetActive(true);
            prestigeOfflineClickerBonus.gameObject.SetActive(true);
        }

        SFX(true);
        FPS(GameManager.data.fps);

        OnChangeText();
        
        T InitializeClicker<T>(T clicker) where T : Clicker
        {
            T savedClicker = null;
            Type type = null;

            if (GameManager.data.clickers.ContainsKey(clicker.name))
            {
                savedClicker = GameManager.data.clickers[clicker.name] as T;
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
                    GameManager.data.clickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.clickBonus += savedClicker.allClickPower;
                }
                else if(type == typeof(AutoClicker) || type == typeof(UniversalClicker))
                {
                    GameManager.data.autoClickerBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.autoClickerBonus += savedClicker.allClickPower;
                }
                else if (type == typeof(OfflineClicker) || type == typeof(UniversalClicker))
                {
                    GameManager.data.offlineClickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.offlineClickBonus += savedClicker.allClickPower;
                }
            }
            
            return savedClicker;
        }

        void InitializeClickerInfo<T>(T clickerItem, Dictionary<string, T> clickerItems) where T : ClickerItem<Clicker>
        {
            var clicker = clickerItem.Clicker;

            clickerItem.uiInfo.image = clickerItem.uiInfo.uiObject.GetChild(0).GetChild(0).GetComponent<Image>();
            clickerItem.uiInfo.bttnBuy = clickerItem.uiInfo.uiObject.GetChild(1).GetComponent<Button>();
            clickerItem.uiInfo.name = clickerItem.uiInfo.uiObject.GetChild(2).GetComponent<Text>();
            clickerItem.uiInfo.description = clickerItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            clickerItem.uiInfo.level = clickerItem.uiInfo.uiObject.GetChild(4).GetComponent<Text>();
            clickerItem.uiInfo.price = clickerItem.uiInfo.uiObject.GetChild(5).GetComponent<Text>();
            clickerItem.uiInfo.clickPower = clickerItem.uiInfo.uiObject.GetChild(6).GetComponent<Text>();

            clickerItems.Add(clicker.name, clickerItem);

            clickerItems[clicker.name].uiInfo.name.name = clicker.name;
            clickerItems[clicker.name].uiInfo.description.name = clicker.name + "D";

            clickerItems[clicker.name].uiInfo.image.sprite = clickerItem.uiInfo.avatar;
            clickerItems[clicker.name].uiInfo.name.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.name.name);
            clickerItems[clicker.name].uiInfo.description.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.description.name);
            clickerItems[clicker.name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
            clickerItems[clicker.name].uiInfo.price.text = FormatMoney(clicker.currentPrice);
            if (!(clicker is UniversalClicker))
            {
                string key = clicker.GetType() == typeof(Clicker) ? "Click" : "Sec";
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MovingObjList.GetObj("T").MoveToTarget();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            MovingObjList.GetObj("T").MoveToStartPos();
        }
    }

    public static T GetClickerItem<T>(string name) where T : Clicker
    {
        if (!clickItems.ContainsKey(name)) return null;
        return clickItems[name].Clicker as T;
    }
    public static Panel GetPanel(string name)
    {
        if (!panels.ContainsKey(name)) return null;
        return panels[name];
    }

    private void NextDay()
    {
        if (!GameManager.data.wasAttack && calendar.number.text == "20" && calendar.month.text == LanguageManager.GetLocalizedText("Sept"))
        {
            if (!GameManager.data.isDefend)
            {
                beforeStorm.SetActive(true);
                Time.timeScale = 0;
                GameManager.data.isDefend = true;
                return;
            }

            Area51Controller.mainEvent.OnHpHasChanged += OnHpHasChanged;
            Area51Controller.mainEvent.OnEndAttack += OnEndAttack;

            Area51Controller.BeginDefend();

            area51Hp.gameObject.SetActive(true);
            area51Hp.maxValue = GameManager.data.maxHp;

            OnHpHasChanged();

            EnemySpawner.SpawnEnemy();

            CancelInvoke("NextDay");
            return;
        }

        calendar++;
    }

    public void Panel_(Text text) => OpenOrClosePanel(text.name, text);
    public void Section(Text text) => OpenOrCloseSection(text.name, text);

    public void OffBeforeStorm()
    {
        beforeStorm.SetActive(false);
        Time.timeScale = 1;
        NextDay();
    }

    #region DebugModidier
    private void EnableModifier()
    {
        StartCoroutine(Modifier());
        StartCoroutine(Test());
    }
    private IEnumerator Test()
    {
        while (modifier.activeSelf)
        {
            if (GameManager.data.debugEnabled) textModifier.text = $"{string.Format("{0:0.00}", sliderModifier.value)}x  // скорость - {string.Format("{0:0.00}", speed)}";

            else textModifier.text = $"{string.Format("{0:0.00}", sliderModifier.value)}x";

            yield return new WaitForSeconds(0.04f);
        }
    }
    float checkTimer = 0;
    float timer = 0;
    float timerStart = 25;
    float koef_smoothness = 0.007f;
    float koef_1 = 0.001f;
    int koef_2 = 100;
    int maxBonus = 0;
    public Text koef1, koef2, koefSmoothness;
    private IEnumerator Modifier()
    {
        timer = timerStart;
        speed = 0;
        maxBonus = 0;
        modifier.SetActive(true);
        sliderModifier.minValue = 1;
        sliderModifier.value = 1;
        sliderModifier.maxValue = 5;
        textModifier.text = $"{sliderModifier.value}x";

        while (timer > 0)
        {
            if (sliderModifier.value >= 1)
            {
                checkTimer -= Time.fixedDeltaTime;
                if ((int)sliderModifier.value > maxBonus) maxBonus = (int)sliderModifier.value;

                if (speed > 0.3f) speed -= sliderModifier.value * koef_1;
                else speed -= sliderModifier.value * (koef_1 / 2);

                if (checkTimer < -1) speed -= sliderModifier.value * (-checkTimer * (koef_1 * 2));

                sliderModifier.value += speed * koef_smoothness;

                sliderModifier.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.HSVToRGB(sliderModifier.value / 20, 1, 1);

                if (sliderModifier.value <= sliderModifier.minValue && sliderModifier.minValue > 1)
                {
                    sliderModifier.maxValue /= 2;
                    sliderModifier.minValue /= 2;
                }
            }

            timer -= Time.fixedDeltaTime;

            yield return null;
        }

        MyDebug.Log($"Ваш бонус - {maxBonus}x");

        speed = 0;
        sliderModifier.minValue = 1;
        sliderModifier.value = 1;
        sliderModifier.maxValue = 5;
        textModifier.text = $"{sliderModifier.value}x";
        modifier.SetActive(false);
        Invoke("EnableModifier", UnityEngine.Random.Range(5, 6));
    }
    #endregion

    #region Tutorial

    public async void EnableIntro(bool enable)
    {
        if (enable)
        {
            await System.Threading.Tasks.Task.Delay(1500);
            intro.SetActive(true);
            intro.GetComponent<Animation>().Play("IntroOpen");
        }
        else
        {
            intro.GetComponent<Animation>().Play();
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
        ManualClicker clicker = GetClickerItem<ManualClicker>(name);

        Area51Controller.mainEvent.Buy(clicker, (bool success) =>
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

        GameManager.data.wasTutorial = true;

        InvokeRepeating("NextDay", calendarDay, calendarDay);
    }

    #endregion /Tutorial

    #region OnEvent

    private void OnClick(int clickCount)
    {
        if (GameManager.data.isDefend)
        {
            Area51Controller.mainEvent.ChangeHp(clickCount);
            return;
        }

        if (GameManager.data.soldiersCount + (int)(clickCount * sliderModifier.value) >= 0)
            GameManager.data.soldiersCount += (int)(clickCount * sliderModifier.value * Time.timeScale);
        else
            GameManager.data.soldiersCount = 0;

        if (GameManager.data.wasAttack && GameManager.data.hasLost && GameManager.data.soldiersCount >= 1000000)
        {
            panels["Prestige"].panel.transform.GetChild(2).GetComponent<Button>().interactable = true;
            GameManager.data.hasLost = false;
            OpenPrestigePanel();
        }
    }

    private void OnChangeText()
    {
        aliensHearts.text = FormatMoney(GameManager.data.aliensHearts);

        soldiers.text = FormatMoney(GameManager.data.soldiersCount);

        textClickBonus.text = $"+{FormatMoney(GameManager.data.clickBonus)}/{LanguageManager.GetLocalizedText("Click")}";

        textAutoclickerBonus.text = $"+{FormatMoney(GameManager.data.autoClickerBonus)}/{LanguageManager.GetLocalizedText("Sec")}";

        textOfflineClickerBonus.text = $"+{FormatMoney(GameManager.data.offlineClickBonus)}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeAutoclickerBonus.gameObject.activeSelf) prestigeAutoclickerBonus.text = $"+{FormatMoney((int)(GameManager.data.autoClickerBonus * (GameManager.data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeOfflineClickerBonus.gameObject.activeSelf) prestigeOfflineClickerBonus.text = $"+{FormatMoney((int)(GameManager.data.offlineClickBonus * (GameManager.data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        foreach (var i in clickItems) CheckPossibilityToBuy(i.Value);

        void CheckPossibilityToBuy(ClickerItem<Clicker> clickerItem)
        {
            if (clickerItem.Clicker.currency == Currency.Soldier)
            {
                if (GameManager.data.soldiersCount < clickerItem.Clicker.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
            }
            else
            {
                if (GameManager.data.aliensHearts < clickerItem.Clicker.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
            }
        }
    }

    private void OnHpHasChanged()
    {
        if (GameManager.data.soldiersCount < 0) GameManager.data.soldiersCount = 0;

        area51HpImage.color = Color.HSVToRGB(area51Hp.value / (GameManager.data.maxHp * 3), 1, 1);

        area51Hp.value = GameManager.data.soldiersCount;
        soldiers.text = FormatMoney(GameManager.data.soldiersCount);
    }

    private void OnEndAttack(bool isWin)
    {
        GameManager.data.isDefend = false;
        GameManager.data.wasAttack = true;

        area51Hp.gameObject.SetActive(false);

        if (isWin)
        {
            prestige.text = LanguageManager.GetLocalizedText("WinText");
        }
        else
        {
            GameManager.data.hasLost = true;
            panels["Prestige"].panel.transform.GetChild(2).GetComponent<Button>().interactable = false;
            prestige.text = LanguageManager.GetLocalizedText("DefeatText");
        }

        panels["Prestige"].panel.SetActive(true);

        InvokeRepeating("NextDay", calendarDay, calendarDay);

        Time.timeScale = 0;
    }
    
    #endregion /OnEvent

    #region Shop

    public void ShowDescription(Text name)
    {
        if (description.gameObject.activeSelf && description.position == name.transform.parent.GetChild(1).position)
        {
            description.gameObject.SetActive(false);
            return;
        }

        description.gameObject.SetActive(true);
        description.position = name.transform.parent.GetChild(1).position;
        description.GetChild(0).GetComponent<Text>().text = LanguageManager.GetLocalizedText(name.name + "D");
    }

    public void BuyClicker(Text name)
    {
        Clicker clicker = GetClicker();
        Area51Controller.mainEvent.Buy(clicker, (bool success) =>
        {
            if (success)
            {
                if (clickItems.ContainsKey(clicker.name)) ChangeInfo(clickItems[clicker.name]);
                else return;

                if (clicker is IAutocliker acl)
                {
                    acl.AutoClick();
                }

                SFXManager.PlaySound("Buy");

                void ChangeInfo(ClickerItem<Clicker> clickerItem)
                {
                    clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                    clickerItem.uiInfo.price.text = FormatMoney(clicker.currentPrice);
                    clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";          
                }
            }
            else MyDebug.LogError($"Clicker {clicker.name} not bought");
        
            OnChangeText();
        });
    
        Clicker GetClicker()
        {
            Clicker cl;

            if ((cl = GetClickerItem<Clicker>(name.name)) != null) return cl;

            return null;
        }
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

    #endregion /Shop

    #region More

    public void SFX(string name) => SFXManager.PlaySound(name);

    public void SFX(bool isStart = false)
    {
        if (isStart) SFXManager.EnableSound(GameManager.data.enableSFX);
        else SFXManager.EnableSound(!GameManager.data.enableSFX);

        if (GameManager.data.enableSFX)
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

    public void FPS(int fps)
    {
        Application.targetFrameRate = fps;
        GameManager.data.fps = fps;

        switch (GameManager.data.fps)
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
        switch (GameManager.data.fps)
        {
            case 30:
                Application.targetFrameRate = 60;
                GameManager.data.fps = 60;

                sliderFPS.rectTransform.anchoredPosition = new Vector2(0, 0);
                break;

            case 60:
                Application.targetFrameRate = 30;
                GameManager.data.fps = 30;

                sliderFPS.rectTransform.anchoredPosition = new Vector2(-145, 0);
                break;
        }
    }

    public void Languages()
    {
        if (!panels["LangSelect"].panel.activeSelf)
        {
            panels["LangSelect"].panel.SetActive(true);
            MovingObjList.GetObj("LangSelect").MoveToTarget();
            rect.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            MovingObjList.GetObj("LangSelect").MoveToStartPos(default, OnClose);
            rect.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public void ChangeLanguage(string lang)
    {
        LanguageManager.ChangeLanguage(lang);

        currentLanguage.sprite = langSprites[GameManager.data.language];

        ChangeShopItemsInfo();
        ChangeTexts();
        OnChangeText();
        
        MovingObjList.GetObj("LangSelect")?.MoveToStartPos(default, OnClose);
        rect.rotation = Quaternion.Euler(0, 0, 0);

        void ChangeShopItemsInfo()
        {
            foreach (var i in clickItems) ChangeShopItemInfo(i.Value);

            void ChangeShopItemInfo(ClickerItem<Clicker> clickerItem)
            {
                Clicker clicker = clickerItem.Clicker;
                clickerItem.uiInfo.name.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.name.name);
                clickerItem.uiInfo.description.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.description.name);
                clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";

                if (!(clicker is UniversalClicker))
                {
                    string key = clicker.GetType() == typeof(Clicker) ? "Click" : "Sec";
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

    #endregion /More

    #region Notes

    public void NoteList(string input)
    {
        var temp = input.Split();
        (string name, int numberInList) = (temp[0], int.Parse(temp[1]));

        var t = GameObject.Find("ContentTest").GetComponent<RectTransform>();

        if (panels[name].panel.activeSelf)
        {
            panels["NotesList"].panel.SetActive(false);
            panels[name].panel.SetActive(false);
            testVP.sizeDelta = new Vector2(0, 0);
        }
        else
        {
            if (activeNotesList != null) activeNotesList.SetActive(false);
            activeNotesList = panels[name].panel;
            panels["NotesList"].panel.SetActive(true);
            panels[name].panel.SetActive(true);
            testVP.sizeDelta = new Vector2(0, heightVP);
        }
        t.anchoredPosition = new Vector2(0, 350 * (numberInList - 1));
    }

    public GameObject note, panel;
    public void Note()
    {
        note.SetActive(true);
        panel.SetActive(true);
    }

    #endregion /Notes

    #region Prestige

    public void ApplyPrestige()
    {
        try
        {
            Time.timeScale = 1;

            int aliensHearts = GameManager.data.aliensHearts;
            int prestige = GameManager.data.prestigeLvl;
            float? timeToWinLeft = GameManager.data.timeToWinLeft;
            float? enemySpawnStep = GameManager.data.enemySpawnStep;
            string password = GameManager.data.passwordDebug;
            string language = GameManager.data.language;
            bool debugEnabled = GameManager.data.debugEnabled;
            bool wasTutorial = GameManager.data.wasTutorial;

            foreach (var acl in GameManager.data.clickers)
            {
                if (acl.Value is AutoClicker ac) ac.hasStart = false; 
                else if (acl.Value is UniversalClicker uc) uc.hasStart = false; 
            }

            GameManager.data = new GameData();

            prestige++;
            timeToWinLeft = 90 + (30 * prestige);
            if (enemySpawnStep >= 0.02f) enemySpawnStep *= 0.75f;

            GameManager.data.aliensHearts = aliensHearts + (prestige * 1000);
            GameManager.data.soldiersCount = 150000 * prestige + 51;
            GameManager.data.prestigeLvl = prestige;
            GameManager.data.timeToWinLeft = timeToWinLeft;
            GameManager.data.enemySpawnStep = enemySpawnStep;
            GameManager.data.passwordDebug = password;
            GameManager.data.language = language;
            GameManager.data.debugEnabled = debugEnabled;
            GameManager.data.wasTutorial = wasTutorial;

            SaveManager.Save(GameManager.data);
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
        catch (Exception e)
        {
            panels["Prestige"].panel.SetActive(false);
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
        }
    }

    public void RevertPrestige()
    {
        Time.timeScale = 1;
        GameManager.data.hasRevertPrestige = true;
        if (!GameManager.data.hasLost) prestigeBttn.SetActive(true);
        panels["Prestige"].panel.SetActive(false);
    }

    public void OpenPrestigePanel()
    {
        panels["Prestige"].panel.SetActive(true);
        prestige.text = LanguageManager.GetLocalizedText("GetPrestige");
    }

    #endregion Prestige

    #region Debug

    public void EnableDebug()
    {
        if (string.IsNullOrEmpty(GameManager.data.passwordDebug))
        {
            passwordInput.SetActive(true);
            return;
        }
        GameManager.data.debugEnabled = !GameManager.data.debugEnabled;

        debugLog.SetActive(GameManager.data.debugEnabled);
        debugButtons.SetActive(GameManager.data.debugEnabled);
        //debugModifier.SetActive(MyDebug.debugEnabled);
    }

    public void EnterPassword(Text password)
    {
        passwordInput.SetActive(false);

        if (password.text != PASSWORD) return;

        GameManager.data.passwordDebug = PASSWORD;
        EnableDebug();
    }

    public void IncreaseSoldiers()
    {
        if (GameManager.data.soldiersCount + 1000000000 > 0)
        {
            GameManager.data.soldiersCount += 1000000000;
            GameManager.data.aliensHearts += 1000;
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

    public void OffModifier()
    {
        if (timer > 0) timer = 0;
        MyDebug.Log("*** Модификатор выключен ***");
    }

    public void ChangeTimer()
    {
        if (timerStart < 120) timerStart += 5;
        else timerStart = 5;
        MyDebug.Log($"*** время действия модификатора равно {timerStart} сек ***");
    }

    public void ChangeKoef_1(Slider slider)
    {
        koef_1 = slider.value;
        koef1.text = slider.value.ToString();
    }
    public void ChangeKoef_2(Slider slider)
    {
        koef_2 = (int)slider.value;
        koef2.text = slider.value.ToString();
    }
    public void ChangeKoef_smoothness(Slider slider)
    {
        koef_smoothness = slider.value;
        koefSmoothness.text = slider.value.ToString();
    }

    public void DefaultSettings()
    {
        koef_1 = 0.001f;
        koef_2 = 100;
        koef_smoothness = 0.007f;

        koef1.text = koef_1.ToString();
        koef2.text = koef_2.ToString();
        koefSmoothness.text = koef_smoothness.ToString();

        GameObject.Find("Debug_Slider (0)").GetComponent<Slider>().value = 0.001f;
        GameObject.Find("Debug_Slider (1)").GetComponent<Slider>().value = 100;
        GameObject.Find("Debug_Slider (2)").GetComponent<Slider>().value = 0.007f;

        MyDebug.Log("*** Настройки по умолчанию назначены ***");
    }

    public Text debugCurrentPrice;
    private string currentClicker;
    public void ShowClickerMenu(Text name)
    {
        if (debugClickerMenu.gameObject.activeSelf && debugClickerMenu.position == name.transform.parent.GetChild(1).position)
        {
            debugClickerMenu.gameObject.SetActive(false);
            return;
        }
        debugClickerMenu.gameObject.SetActive(true);
        debugClickerMenu.position = name.transform.parent.GetChild(1).position;

        UpdateText(name.name);

        currentClicker = name.name;
    }

    public void NewPrice(Text newPrice)
    {
        if (clickItems.ContainsKey(currentClicker))
        {
            Clicker clicker = clickItems[currentClicker].Clicker;
            clicker.priceDefault = int.Parse(newPrice.text);
            clicker.currentPrice = clicker.priceDefault;
            clicker.level = 0;
        }
        else
        {
            MyDebug.LogError($"Clicker {currentClicker} not found");
            return;
        }

        UpdateText(currentClicker);
    }

    private void UpdateText(string name)
    {
        if (clickItems.ContainsKey(name))
        {
            debugCurrentPrice.text = clickItems[name].Clicker.currentPrice.ToString();

            clickItems[name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clickItems[name].Clicker.level}";
            clickItems[name].uiInfo.price.text = clickItems[name].Clicker.currentPrice.ToString();
        }
        else
        {
            MyDebug.LogError($"Clicker {name} not found");
            return;
        }
    }

    public void OnLowMemory()
    {
        MyDebug.LogWarning($"*** Low memory ***");
        MyDebug.LogWarning($"*** Low memory ***");
        MyDebug.LogWarning($"*** Low memory ***");
    }

    #endregion /Debug

    private string FormatMoney(float money)
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

    private void OpenOrClosePanel(string name, Text text)
    {
        if (!panels[name].panel.activeSelf)
        {
            if (activePanel != null)
            {
                activePanel.color = Color.white;
                panels[activePanel.name].animation.Play();
            }
            else activePanelPointer.color = Color.green;

            activePanel = text;
            activePanel.color = Color.green;
            activePanelPointer.transform.position = activePanel.transform.position;

            panels[name].panel.SetActive(true);
            panels[name].animation?.Play($"{name}Open");

            if (name == "LangSelect")
            {
                rect.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else
        {
            if (activePanel == null) return;

            activePanel.color = Color.white;
            activePanel = null;
            activePanelPointer.color = Color.clear;

            panels[name].animation?.Play();
            if (name == "More")
            {
                if (panels["LangSelect"].panel.activeSelf) MovingObjList.GetObj("LangSelect").MoveToStartPos(default, OnClose);
                rect.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void OpenOrCloseSection(string name, Text text)
    {
        activeSection.color = Color.white;
        panels[activeSection.name].panel.SetActive(false);

        activeSection = text;

        panels[name].panel.SetActive(true);
        activeSection.color = Color.green;
    }

    public class Panel
    {
        public GameObject panel;
        public Animation animation;

        public Panel(GameObject panel, Animation animation)
        {
            this.panel = panel;
            this.animation = animation;
        }
    }

    [Serializable]
    public struct Calendar
    {
        public Text month, number, year;
        public Queue<string> months;
        public List<string> months30Ending, months31Ending;

        public static Calendar operator ++(Calendar calendar)
        {
            if ((calendar.number.text == "30" && calendar.months30Ending.Contains(calendar.months.Peek())) ||
                (calendar.number.text == "31" && calendar.months31Ending.Contains(calendar.months.Peek())) ||
                (calendar.number.text == (GameManager.data.leapCounter != 4 ? "28" : "29") && calendar.months.Peek() == "Feb"))
            {
                calendar.NextMonth();
            }
            calendar.number.text = (int.Parse(calendar.number.text) + 1).ToString();

            GameManager.data.number = calendar.number.text;

            return calendar;
        }

        public void SynchronizeDate()
        {
            while (LanguageManager.GetLocalizedText(GameManager.data.month) != LanguageManager.GetLocalizedText(months.Peek()))
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
            number.text = "0";

            if (currentMonth == "Dec")
            {
                year.text = (int.Parse(year.text) + 1).ToString();
                GameManager.data.year = year.text;

                if (GameManager.data.leapCounter < 4) GameManager.data.leapCounter++;
                else GameManager.data.leapCounter = 1;
            }

            GameManager.data.month = months.Peek();
        }
    }
}

