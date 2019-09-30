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
    private static Dictionary<string, ClickerItem> clickItems;
    private static Dictionary<string, AutoClickerItem> autoClickItems;
    private static Dictionary<string, OfflineClickerItem> offlineClickItems;
    private static Dictionary<string, Panel> panels;
    private static Dictionary<string, Sprite> langSprites;

    float speed = 0, heightVP, calendarDay = 60;


    private void Awake() => Instance = this;

    private Dictionary<string, Clicker> clickers = new Dictionary<string, Clicker>();

    private void Start()
    {
        #region SubscribeEvents
        Area51Controller.mainEvent.OnClick += OnClick;
        Area51Controller.mainEvent.OnAnyAction += OnChangeText;
        Application.lowMemory += OnLowMemory;
        #endregion

        #region Init
        clickItems = new Dictionary<string, ClickerItem>();
        autoClickItems = new Dictionary<string, AutoClickerItem>();
        offlineClickItems = new Dictionary<string, OfflineClickerItem>();

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

        foreach (var i in panelsArr)
        {
            Panel panel = new Panel(i, i.GetComponent<Animation>());
            panels.Add(i.name, panel);
            panels[i.name].panel.SetActive(false);
        }
        panels["Clickers"].panel.SetActive(true);
        activeSection.color = Color.green;

        foreach (var i in langSpritesArr)
        {
            langSprites.Add(i.name, i);
        }

        ChangeLanguage(GameManager.data.language);

        calendar.number.text = GameManager.data.number;
        calendar.month.text = LanguageManager.GetLocalizedText(GameManager.data.month);
        calendar.year.text = GameManager.data.year;
        calendar.SynchronizeDate();

        foreach (var i in ShopManager.Instance.clickers)
        {
            i.Clicker = InitializeClicker(i.Clicker);
            InitializeClickerInfo(i, clickItems);

            clickers.Add(i.Clicker.name, i.Clicker);
        }
        foreach (var i in ShopManager.Instance.autoClickers)
        {
            i.Clicker = InitializeClicker(i.AutoClicker);
            i.AutoClicker = InitializeClicker(i.AutoClicker);
            InitializeClickerInfo(i, autoClickItems);

            clickers.Add(i.Clicker.name, i.AutoClicker);
        }
        foreach (var i in ShopManager.Instance.offlineClickers)
        {
            i.Clicker = InitializeClicker(i.OfflineClicker);
            i.OfflineClicker = InitializeClicker(i.OfflineClicker);
            InitializeClickerInfo(i, offlineClickItems);

            clickers.Add(i.Clicker.name, i.OfflineClicker);
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
                type = typeof(Clicker);
            }
            else if (GameManager.data.autoClickers.ContainsKey(clicker.name))
            {
                savedClicker = GameManager.data.autoClickers[clicker.name] as T;
                type = typeof(AutoClicker);
            }
            else if (GameManager.data.offlineClickers.ContainsKey(clicker.name))
            {
                savedClicker = GameManager.data.offlineClickers[clicker.name] as T;
                type = typeof(OfflineClicker);
            }
            else
            {
                savedClicker = clicker;
                savedClicker.price = savedClicker.priceDefault;
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
                clicker.price = price;
                MyDebug.LogWarning($"{clicker.name}'s price has changed: {savedClicker.price} -> {clicker.price}");
                savedClicker.price = clicker.price;

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
                if (type == typeof(Clicker))
                {
                    GameManager.data.clickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.clickBonus += savedClicker.allClickPower;
                }
                else if(type == typeof(AutoClicker))
                {
                    GameManager.data.autoClickerBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.autoClickerBonus += savedClicker.allClickPower;
                }
                else if (type == typeof(OfflineClicker))
                {
                    GameManager.data.offlineClickBonus -= savedClicker.allClickPower;
                    savedClicker.allClickPower = allClickPower;
                    GameManager.data.offlineClickBonus += savedClicker.allClickPower;
                }
            }
            
            return savedClicker;
        }

        void InitializeClickerInfo<T>(T clickerItem, Dictionary<string, T> clickerItems) where T : ClickerItem
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
            clickerItems[clicker.name].uiInfo.price.text = FormatMoney(clicker.price);
            clickerItems[clicker.name].uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";
        }
    }

    public static ClickerItem GetClickItem(string name)
    {
        if (!clickItems.ContainsKey(name)) return null;
        return clickItems[name];
    }
    public static AutoClickerItem GetAutoClickItem(string name)
    {
        if (!autoClickItems.ContainsKey(name)) return null;
        return autoClickItems[name];
    }
    public static OfflineClickerItem GetOfflineClickItem(string name)
    {
        if (!offlineClickItems.ContainsKey(name)) return null;
        return offlineClickItems[name];
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
        Clicker clicker = GetClickItem(name)?.Clicker;

        Area51Controller.mainEvent.Buy(clicker, (bool success) =>
        {
            if (success)
            {
                clickItems[name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                clickItems[name].uiInfo.price.text = clicker.price.ToString();
                clickItems[name].uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";

                tutorials[1].transform.GetChild(1).gameObject.SetActive(false);

                tutorials[0].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialGame_2");
                tutorials[1].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialShop_2");

                tutorials[2].gameObject.SetActive(true);
                tutorials[3].gameObject.SetActive(true);

                SFXManager.PlaySound("Buy");
            }
            else MyDebug.LogError($"Clicker {clicker.name} not bought");

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
        foreach (var i in autoClickItems) CheckPossibilityToBuy(i.Value);
        foreach (var i in offlineClickItems) CheckPossibilityToBuy(i.Value);

        void CheckPossibilityToBuy(ClickerItem clickerItem)
        {
            if (GameManager.data.soldiersCount < clickerItem.Clicker.price) clickerItem.uiInfo.bttnBuy.interactable = false;
            else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
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
                else if (autoClickItems.ContainsKey(clicker.name)) ChangeInfo(autoClickItems[clicker.name]);
                else if (offlineClickItems.ContainsKey(clicker.name)) ChangeInfo(offlineClickItems[clicker.name]);
                else return;
            
                SFXManager.PlaySound("Buy");

                void ChangeInfo(ClickerItem clickerItem)
                {
                    clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                    clickerItem.uiInfo.price.text = FormatMoney(clicker.price);
                    clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";          
                }
            }
            else MyDebug.LogError($"Clicker {clicker.name} not bought");
        
            OnChangeText();
        });
    
        Clicker GetClicker()
        {
            Clicker cl;

            if ((cl = GetClickItem(name.name)?.Clicker) != null) return cl;
            if ((cl = GetAutoClickItem(name.name)?.Clicker) != null) return cl;
            if ((cl = GetOfflineClickItem(name.name)?.Clicker) != null) return cl;

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
            panels["LangSelect"].animation.Play("LanguagesOpen");
            rect.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            panels["LangSelect"].animation.Play();
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

        panels["LangSelect"].animation.Play("LanguagesClose");
        rect.rotation = Quaternion.Euler(0, 0, 0);

        void ChangeShopItemsInfo()
        {
            foreach (var i in clickItems) ChangeShopItemInfo(i.Value);
            foreach (var i in autoClickItems) ChangeShopItemInfo(i.Value);
            foreach (var i in offlineClickItems) ChangeShopItemInfo(i.Value);

            void ChangeShopItemInfo(ClickerItem clickerItem)
            {
                clickerItem.uiInfo.name.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.name.name);
                clickerItem.uiInfo.description.text = LanguageManager.GetLocalizedText(clickerItem.uiInfo.description.name);
                clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clickerItem.Clicker.level}";
                clickerItem.uiInfo.clickPower.text = $"+{clickerItem.Clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";
            }
        }
        void ChangeTexts()
        {
            foreach (var t in localizedTexts) if (t != null) t.text = LanguageManager.GetLocalizedText(t.name);

            calendar.month.text = LanguageManager.GetLocalizedText(calendar.months.Peek());
        }
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
        // ???
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

            foreach (var acl in GameManager.data.autoClickers)
            {
                acl.Value.hasStart = false;
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
            clicker.price = clicker.priceDefault;
            clicker.level = 0;
        }
        else if (autoClickItems.ContainsKey(currentClicker))
        {
            AutoClicker autoclicker = autoClickItems[currentClicker].AutoClicker;
            autoclicker.priceDefault = int.Parse(newPrice.text);
            autoclicker.price = autoclicker.priceDefault;
            autoclicker.level = 0;
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
        if (autoClickItems.ContainsKey(name))
        {
            debugCurrentPrice.text = autoClickItems[name].AutoClicker.price.ToString();

            autoClickItems[name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {autoClickItems[name].AutoClicker.level}";
            autoClickItems[name].uiInfo.price.text = autoClickItems[name].AutoClicker.price.ToString();
        }
        else if (clickItems.ContainsKey(name))
        {
            debugCurrentPrice.text = clickItems[name].Clicker.price.ToString();

            clickItems[name].uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clickItems[name].Clicker.level}";
            clickItems[name].uiInfo.price.text = clickItems[name].Clicker.price.ToString();
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

            if (name == "LangSelect") rect.rotation = Quaternion.Euler(0, 0, 0);
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
                if (panels["LangSelect"].panel.activeSelf) panels["LangSelect"].animation?.Play();
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

[Serializable]
public class ShopItem
{
    public Transform uiObject;
    public Sprite avatar;
    [HideInInspector]
    public Text name, description, level, price, clickPower;
    [HideInInspector] public Button bttnBuy;
    [HideInInspector] public Image image;
}

[Serializable]
public class ClickerItem
{
    public ShopItem uiInfo;
    [SerializeField] private Clicker clicker;
    [HideInInspector] public Clicker Clicker
    {
        get => clicker;
        set => clicker = value;
    }

    public ClickerItem(Clicker clicker, ShopItem uiInfo)
    {
        this.clicker = clicker;
        this.uiInfo = uiInfo;
    }
}

[Serializable]
public class AutoClickerItem : ClickerItem
{
    [SerializeField] private AutoClicker autoClicker;
    [HideInInspector] public AutoClicker AutoClicker
    {
        get => autoClicker;
        set => autoClicker = value;
    }
    
    public AutoClickerItem(AutoClicker autoClicker, ShopItem uiInfo) : base(autoClicker, uiInfo)
    {
        this.autoClicker = autoClicker;
        this.uiInfo = uiInfo;
    }
}

[Serializable]
public class OfflineClickerItem : ClickerItem
{
    [SerializeField] private OfflineClicker offlineClicker;
    [HideInInspector] public OfflineClicker OfflineClicker
    {
        get => offlineClicker;
        set => offlineClicker = value;
    }
    
    public OfflineClickerItem(OfflineClicker offlineClicker, ShopItem uiInfo) : base(offlineClicker, uiInfo)
    {
        this.offlineClicker = offlineClicker;
        this.uiInfo = uiInfo;
    }
}

[Serializable]
public class Clicker
{
    public string name;
    public int clickPowerDefault;
    public int priceDefault;
    public int level;
    [HideInInspector] public string description;
    [HideInInspector] public int allClickPower;
    [HideInInspector] public int price;
    [HideInInspector] public bool hasBought;
}

[Serializable]
public class AutoClicker : Clicker, IAutocliker
{
    [NonSerialized]
    public bool hasStart;

    async public void AutoClick()
    {
        if (hasStart) return;

        hasStart = true;

        while (hasStart)
        {
            await System.Threading.Tasks.Task.Delay(1000);

            if (Area51Controller.mainEvent != null && hasStart)
            {
                Area51Controller.mainEvent.Click(!GameManager.data.isDefend
                    ? allClickPower + (int)(allClickPower * (GameManager.data.prestigeLvl * 0.1f))
                    : (int)(allClickPower * (GameManager.data.prestigeLvl * 0.1f)));
            }
        }
    }
}

[Serializable]
public class OfflineClicker : Clicker, IOfflineClicker
{
    public void RememberTime()
    {
        GameManager.data.exitTime = DateTime.Now.ToString();
    }

    public void CalculateProduction()
    {
        if (GameManager.data.isDefend)
        {
            MyDebug.LogWarning("Во время битвы оффлайн кликеры не вырабатывают солдат");
            return;
        }
        DateTime exitTime = DateTime.Parse(GameManager.data.exitTime);

        TimeSpan offlineTime = DateTime.Now - exitTime;

        int offlineSecs = (int)offlineTime.TotalSeconds;

        MyDebug.Log($"exit - {exitTime} /// now - {DateTime.Now} /// offline time - {offlineTime} /// offline secs - {offlineSecs}");

        int totalProduction = offlineSecs * allClickPower;
        totalProduction += (int)(totalProduction * (GameManager.data.prestigeLvl * 0.1f));

        MyDebug.Log($"{name} has produced - {totalProduction}");

        GameManager.data.soldiersCount += totalProduction;
    }
}

[Serializable]
public class UniversalClicker : Clicker, IAutocliker, IOfflineClicker
{
    [NonSerialized] public bool hasStart;

    public void AutoClick()
    {
        throw new NotImplementedException();
    }

    public void CalculateProduction()
    {
        throw new NotImplementedException();
    }

    public void RememberTime()
    {
        throw new NotImplementedException();
    }
}

interface IAutocliker
{
    void AutoClick();
}

interface IOfflineClicker
{
    void RememberTime();

    void CalculateProduction();
}
