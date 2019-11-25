using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;
    public Calendar calendar;
    public GiftBlackout giftBlackout;
    public RectTransform langArrow, debugClickerMenu, scale, medalInfoParent;
    public RectTransform[] descriptions, productsParent;
    public Sprite[] langSpritesArr, prestigeAvatars;
    public Image currentLanguage, toggleSFX, buttonSFX, buttonScale, toggleScale, sliderFPS, buttonNotify, toggleNotify, sliderModifierColor, activePanelPointer, area51HpImage, prestigeImage, notify;
    public Slider sliderModifier, area51Hp;
    public Button gift, heartsSkip;
    public Text soldiers, soldiersFull, aliensHearts, aliensHeartsFull, textOfflineClickerBonus, textAutoclickerBonus, textClickBonus,
        prestigeClickBonus, prestigeAutoclickerBonus, prestigeOfflineClickerBonus, prClicker, prAutoclicker, prOfflineClicker,
        activePanelText, activeSection, prestige, gameVersion, debugLng, debugMaxCount, giftTimer;
    public Text[] localizedTexts;
    public GameObject debugLog, debugModifier, debugButtons, intro, beforeStorm, activeNotesList, prestigeBttn, passwordInput, separatorPrefab, bcgrPrefab, medalInfoPrefab;
    public GameObject[] panelsArr, tutorials, modifiers, productsPrefab;
    public static List<Type> productsCount;
    private List<Text> medalNames;
    private static Dictionary<string, ProductItem<Product>> products;
    private static Dictionary<string, GameObject> panels;
    private static Dictionary<string, Sprite> langSprites;
    private static readonly string[] languages = { "ru", "en" };
    private const string PASSWORD = "ZgTA51OV199";

    private void Awake()
    {
        Instance = this;
        productsCount = new List<Type>();

        Check();
    }

    private void Start()
    {
        if (Application.isEditor) GameDataManager.data.passwordDebug = PASSWORD;

        #region SubscribeEvents
        EventManager.eventManager.OnAnyAction += OnChangeText;
        EventManager.eventManager.OnBoosterUsed += OnBoosterUse;
        EventManager.eventManager.OnTimer += OnGiftTimer;
        EventManager.eventManager.OnFinishBuy += OnFinishBuy;
        EventManager.eventManager.OnAnimationEnd += OnGiftOpen;
        EventManager.eventManager.OnAdFinished += OnAdFinished;
        Application.lowMemory += OnLowMemory;
        #endregion

        #region Init
        products = new Dictionary<string, ProductItem<Product>>();

        panels = new Dictionary<string, GameObject>();

        langSprites = new Dictionary<string, Sprite>();

        medalNames = new List<Text>();

        calendar.months = new Queue<string>(new string[]
        {
            "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"
        });
        #endregion

        foreach (var i in langSpritesArr) langSprites.Add(i.name, i);

        ChangeLanguage(GameDataManager.data.language);

        #region Set
        gameVersion.text = $"v{Application.version}";
        
        prestigeImage.sprite = prestigeAvatars[GameDataManager.data.prestigeAvatarIndex];
        prestigeImage.preserveAspect = true;

        gift.interactable = GameDataManager.data.giftTimer.IsFinished;
        giftTimer.text = gift.interactable ? LanguageManager.GetLocalizedText("Open") : GameDataManager.data.giftTimer.ToString();

        heartsSkip.transform.GetChild(0).GetComponent<Text>().text = GameDataManager.data.SkipCost.ToString();
        #endregion

        prestigeBttn.SetActive(false);

        giftBlackout.SetActive(false);

        calendar.day.text = GameDataManager.data.number;
        calendar.month.text = LanguageManager.GetLocalizedText(GameDataManager.data.month);
        calendar.year.text = GameDataManager.data.year;
        calendar.SynchronizeDate();

        Notify(gift.interactable);

        foreach (var i in panelsArr)
        {
            panels.Add(i.name, i);
            panels[i.name].SetActive(false);
        }
        panels["Clickers"].SetActive(true);
        
        Type lastType = null;
        foreach (var clickerItem in ShopManager.clickerItems)
        {
            CheckToSeparate(clickerItem.Product.GetType(), 0);
            
            clickerItem.Product = InitializeClicker(clickerItem.Product);
            var item = InitializeClickerInfo(new ClickerItem<Clicker>(clickerItem.uiInfo, clickerItem.Product));

            products.Add(item.Product.name, new ProductItem<Product>(item.uiInfo, item.Product));
        }
        foreach (var boosterItem in ShopManager.boosterItems)
        {
            CheckToSeparate(boosterItem.Product.GetType(), 1);
            
            boosterItem.Product = InitializeBooster(boosterItem.Product);

            var item = InitializeBoosterInfo(new BoosterItem<Booster>(boosterItem.uiInfo, boosterItem.Product));

            products.Add(item.Product.name, new ProductItem<Product>(item.uiInfo, item.Product));

            if (products[item.Product.name].Product is IDefendBooster defendBooster) defendBooster.CheckAvailabilityUse();

            if (item.Product.IsUsing) item.Product.Use();
        }
        foreach (var specItem in ShopManager.specAmplificationItems)
        {
            CheckToSeparate(specItem.Product.GetType(), 2);
            
            specItem.Product = InitializeSpecialAmplification(specItem.Product);

            var item = InitializeSpecialAmplificationInfo(new SpecialAmplificationItem<SpecialAmplification>(specItem.uiInfo, specItem.Product));

            products.Add(specItem.Product.name, new ProductItem<Product>(specItem.uiInfo, specItem.Product));
        }
        foreach (var offerItem in ShopManager.offerItems)
        {
            CheckToSeparate(offerItem.Product.GetType(), 3);

            offerItem.Product = InitializeOffer(offerItem.Product);

            var item = InitializeOfferInfo(new OfferItem<Offer>(offerItem.uiInfo, offerItem.Product));

            products.Add(offerItem.Product.name, new ProductItem<Product>(offerItem.uiInfo, offerItem.Product));
        }

        foreach (var d in descriptions) d.SetAsLastSibling();
        
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

        for (int i = 0; i <= GameDataManager.data.prestigeLvl; i++)
        {
            CreateMedalInfo(medalInfoPrefab, medalInfoParent, i);
        }
        for (int i = GameDataManager.data.prestigeLvl + 1; i < prestigeAvatars.Length; i++)
        {
            MedalInfo medalInfo = CreateMedalInfo(medalInfoPrefab, medalInfoParent, i);

            medalInfo.medalButton.interactable = false;
        }

        SFX(true);
        InverseScale(true);
        Notifications(true);
        FPS(GameDataManager.data.fps);
        
        OnChangeText();


        T InitializeClicker<T>(T defaultClicker) where T : Clicker
        {
            T savedClicker = null;
            Type type = null;

            if (GameDataManager.data.products.ContainsKey(defaultClicker.name))
            {
                savedClicker = GameDataManager.data.products[defaultClicker.name] as T;
                type = typeof(T);
            }
            else
            {
                savedClicker = defaultClicker;
                savedClicker.currentPrice = savedClicker.priceDefault;
            }

            if (defaultClicker.priceDefault != savedClicker.priceDefault)
            {
                int level = 0;
                int price = defaultClicker.priceDefault;
                for (int x = 0; x < savedClicker.level; x++)
                {
                    price += (defaultClicker.priceDefault / 2) + (defaultClicker.priceDefault * level);
                    level++;
                }
                defaultClicker.currentPrice = price;
                MyDebug.LogWarning($"{defaultClicker.name}'s price has changed: {savedClicker.currentPrice} -> {defaultClicker.currentPrice}");
                savedClicker.currentPrice = defaultClicker.currentPrice;

                savedClicker.priceDefault = defaultClicker.priceDefault;
            }
            if (defaultClicker.clickPowerDefault != savedClicker.clickPowerDefault)
            {
                int allClickPower = 0;
                for (int x = 0; x < savedClicker.level; x++)
                {
                    allClickPower += defaultClicker.clickPowerDefault;
                }
                MyDebug.LogWarning($"{defaultClicker.name}'s click power has changed: {savedClicker.allClickPower} -> {allClickPower}");

                savedClicker.clickPowerDefault = defaultClicker.clickPowerDefault;
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
            if (defaultClicker.currency != savedClicker.currency)
            {
                savedClicker.currency = defaultClicker.currency;
            }

            return savedClicker;
        }

        T InitializeBooster<T>(T defaultBooster) where T : Booster
        {
            T savedBooster = null;

            if (GameDataManager.data.products.ContainsKey(defaultBooster.name))
            {
                savedBooster = GameDataManager.data.products[defaultBooster.name] as T;
            }
            else
            {
                savedBooster = defaultBooster;
                savedBooster.useTimeRemained = savedBooster.useTime;
            }

            if (defaultBooster.priceDefault != savedBooster.priceDefault) savedBooster.priceDefault = defaultBooster.priceDefault;
            if (defaultBooster.currency != savedBooster.currency) savedBooster.currency = defaultBooster.currency;
            if (defaultBooster.abilityModifier != savedBooster.abilityModifier) savedBooster.abilityModifier = defaultBooster.abilityModifier;
            if (defaultBooster.useTime != savedBooster.useTime)
            {
                savedBooster.useTime = defaultBooster.useTime;
                savedBooster.useTimeRemained = defaultBooster.useTime;
            }

            return savedBooster;
        }

        T InitializeSpecialAmplification<T>(T defaultAmplification) where T : SpecialAmplification
        {
            T savedAmplification = null;

            if (GameDataManager.data.products.ContainsKey(defaultAmplification.name))
            {
                savedAmplification = GameDataManager.data.products[defaultAmplification.name] as T;
            }
            else
            {
                savedAmplification = defaultAmplification;
                savedAmplification.currentPrice = defaultAmplification.priceDefault;
            }

            if (defaultAmplification.priceDefault != savedAmplification.priceDefault)
            {
                int priceDefault = defaultAmplification.priceDefault;
                int price = defaultAmplification.priceDefault;
                int level = 0;

                for (int i = 0; i < savedAmplification.level; i++)
                {
                    level++;
                    if (savedAmplification is PermanentSoldierBoost) price += priceDefault + (int)(priceDefault * (float)level / 10);
                    else price += priceDefault * (level / 10);
                }

                MyDebug.LogWarning($"{defaultAmplification.name}'s default price has changed: {savedAmplification.priceDefault} -> {defaultAmplification.priceDefault}");

                savedAmplification.currentPrice = price;
                savedAmplification.priceDefault = priceDefault;
            }
            if (defaultAmplification.currency != savedAmplification.currency) savedAmplification.currency = defaultAmplification.currency;
            if (defaultAmplification.defaultModifierValue != savedAmplification.defaultModifierValue)
            {
                float modifierValue = 0;

                for (int i = 0; i < savedAmplification.level; i++)
                {
                    modifierValue += defaultAmplification.defaultModifierValue;
                }

                MyDebug.LogWarning($"{defaultAmplification.name}'s amplification power has changed: {savedAmplification.defaultModifierValue} -> {defaultAmplification.defaultModifierValue}. " +
                    $"All power: {savedAmplification.modifierValue} -> {modifierValue}");

                savedAmplification.defaultModifierValue = defaultAmplification.defaultModifierValue;
                savedAmplification.modifierValue = modifierValue;
            }
            if (savedAmplification.modifierValue == 0) savedAmplification.modifierValue = savedAmplification.defaultModifierValue;

            return savedAmplification;
        }

        T InitializeOffer<T>(T defaultOffer) where T : Offer
        {
            T savedOffer = null;

            if (GameDataManager.data.products.ContainsKey(defaultOffer.name))
            {
                savedOffer = GameDataManager.data.products[defaultOffer.name] as T;
            }
            else
            {
                savedOffer = defaultOffer;
            }

            if (defaultOffer.priceDefault != savedOffer.priceDefault) savedOffer.priceDefault = defaultOffer.priceDefault;
            if (defaultOffer.currency != savedOffer.currency) savedOffer.currency = defaultOffer.currency;
            if (defaultOffer.productAmount != savedOffer.productAmount) savedOffer.productAmount = defaultOffer.productAmount;

            return savedOffer;
        }

        T InitializeClickerInfo<T>(T clickerItem) where T : ClickerItem<Clicker>
        {
            var clicker = clickerItem.Product;
            
            clickerItem.uiInfo.uiObject = CreateUIObject(productsPrefab[0], 0);

            InitializeShopInfo(clickerItem.uiInfo);

            clickerItem.uiInfo.level = clickerItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            clickerItem.uiInfo.clickPower = clickerItem.uiInfo.uiObject.GetChild(5).GetComponent<Text>();

            ProductItem<Product> item = new ProductItem<Product>(clickerItem.uiInfo, clickerItem.Product);

            InitializeProductInfo(item);

            clickerItem.uiInfo = item.uiInfo as ClickerShopItem;
            clickerItem.Product = item.Product as Clicker;

            clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
            clickerItem.uiInfo.price.text = FormatMoney(clicker.currentPrice);

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

            return clickerItem;
        }

        T InitializeBoosterInfo<T>(T boosterItem) where T : BoosterItem<Booster>
        {
            var booster = boosterItem.Product;

            string ability = $"{booster.abilityModifier}x";
            if (booster is DefenceBooster || booster is RegenBooster)
            {
                ability = booster is DefenceBooster ? $"{booster.abilityModifier * 100}%" : $"{booster.abilityModifier * 60}/{LanguageManager.GetLocalizedText("S")}";
            }

            boosterItem.uiInfo.uiObject = CreateUIObject(productsPrefab[1], 1);

            InitializeShopInfo(boosterItem.uiInfo);

            boosterItem.uiInfo.amount = boosterItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            boosterItem.uiInfo.use = boosterItem.uiInfo.uiObject.GetChild(5).GetChild(1).GetComponent<Text>();
            boosterItem.uiInfo.functional = boosterItem.uiInfo.uiObject.GetChild(5).GetChild(2).GetComponent<Text>();
            boosterItem.uiInfo.bttnUse = boosterItem.uiInfo.uiObject.GetChild(5).GetComponent<Button>();
            if (boosterItem.uiInfo.boosterIcon.iconObject != null)
            {
                boosterItem.uiInfo.boosterIcon.text = boosterItem.uiInfo.boosterIcon.iconObject.transform.GetChild(2).GetComponent<Text>();
                boosterItem.uiInfo.boosterIcon.iconObject.transform.GetChild(0).GetComponent<Image>().sprite = boosterItem.uiInfo.boosterIcon.picture;
            }
            
            ProductItem<Product> productItem = new ProductItem<Product>(boosterItem.uiInfo, boosterItem.Product);

            InitializeProductInfo(productItem);

            boosterItem.uiInfo = productItem.uiInfo as BoosterShopItem;
            boosterItem.Product = productItem.Product as Booster;

            boosterItem.uiInfo.amount.text = $"{booster.amount}x";
            boosterItem.uiInfo.functional.text = $"{LanguageManager.GetLocalizedText(booster.GetType().ToString())} {ability} - {booster.useTime}{LanguageManager.GetLocalizedText("Sec")}";
            boosterItem.uiInfo.use.text = LanguageManager.GetLocalizedText("Use");
            boosterItem.uiInfo.bttnUse.onClick.AddListener(() => SFX(SoundTypes.Button));
            boosterItem.uiInfo.bttnUse.onClick.AddListener(() => Use(boosterItem.uiInfo.name));
            boosterItem.uiInfo.bttnUse.interactable = !booster.IsUsing && booster.amount > 0;
            boosterItem.uiInfo.price.text = FormatMoney(booster.priceDefault);

            return boosterItem;
        }

        T InitializeSpecialAmplificationInfo<T>(T specItem) where T : SpecialAmplificationItem<SpecialAmplification>
        {
            var specAmplif = specItem.Product;

            specItem.uiInfo.uiObject = CreateUIObject(productsPrefab[2], 2);

            InitializeShopInfo(specItem.uiInfo);

            specItem.uiInfo.level = specItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            specItem.uiInfo.modifierValue = specItem.uiInfo.uiObject.GetChild(5).GetComponent<Text>();
            
            ProductItem<Product> productItem = new ProductItem<Product>(specItem.uiInfo, specItem.Product);

            specItem.uiInfo = productItem.uiInfo as SpecialAmplificationShopItem;
            specItem.Product = productItem.Product as SpecialAmplification;

            InitializeProductInfo(productItem);

            specItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {specAmplif.level}";
            specItem.uiInfo.price.text = FormatMoney(specAmplif.currentPrice);
            specItem.uiInfo.modifierValue.text = $"+{specAmplif.defaultModifierValue * 100}%";

            if (specItem.Product is PermanentSoldierBoost) specItem.uiInfo.modifierValue.text = $"{specAmplif.modifierValue}x";

            return specItem;
        }

        T InitializeOfferInfo<T>(T offerItem) where T : OfferItem<Offer>
        {
            var offer = offerItem.Product;
            
            offerItem.uiInfo.uiObject = CreateUIObject(productsPrefab[3], 3);

            InitializeShopInfo(offerItem.uiInfo);

            offerItem.uiInfo.productAmount = offerItem.uiInfo.uiObject.GetChild(3).GetComponent<Text>();
            offerItem.uiInfo.productImage = offerItem.uiInfo.uiObject.GetChild(5).GetComponent<Image>();

            ProductItem<Product> productItem = new ProductItem<Product>(offerItem.uiInfo, offerItem.Product);

            InitializeProductInfo(productItem);

            offerItem.uiInfo = productItem.uiInfo as OfferShopItem;
            offerItem.Product = productItem.Product as Offer;
            
            offerItem.uiInfo.price.text = $"{FormatMoney(offer.priceDefault)} {LanguageManager.GetLocalizedText(offerItem.Product.currency.ToString())}";
            offerItem.uiInfo.productImage.sprite = offerItem.uiInfo.productIcon;
            offerItem.uiInfo.productAmount.text = $"+{offerItem.Product.productAmount}";

            return offerItem;
        }

        void InitializeShopInfo(ShopItem shopItem)
        {
            shopItem.avatarImage = shopItem.uiObject.GetChild(0).GetChild(0).GetComponent<Image>();
            shopItem.bttnBuy = shopItem.uiObject.GetChild(1).GetComponent<Button>();
            shopItem.name = shopItem.uiObject.GetChild(2).GetComponent<Text>();
            shopItem.price = shopItem.uiObject.GetChild(4).GetComponent<Text>();
            shopItem.currency = shopItem.uiObject.GetChild(shopItem.uiObject.childCount - 1).GetComponent<Image>();
            
            shopItem.bttnBuy.onClick.AddListener(() => BuyProduct(shopItem.name));
            
            shopItem.avatarImage.GetComponent<Button>().onClick.AddListener(() => SFX(SoundTypes.Button));
            shopItem.avatarImage.GetComponent<Button>().onClick.AddListener(() => ShowDescription(shopItem.name));
        }

        void InitializeProductInfo(ProductItem<Product> productItem)
        {
            productItem.uiInfo.name.name = productItem.Product.name;
            productItem.uiInfo.name.text = LanguageManager.GetLocalizedText(productItem.uiInfo.name.name);
            productItem.uiInfo.avatarImage.sprite = productItem.uiInfo.avatar;
            productItem.uiInfo.currency.sprite = Resources.Load<Sprite>(productItem.Product.currency.ToString());
        }

        void CheckToSeparate(Type productType, int parentNum)
        {
            if (!productsCount.Contains(productType)) return;

            if (lastType != productType)
            {
                GameObject prefab = Instantiate(separatorPrefab) as GameObject;

                prefab.transform.SetParent(productsParent[parentNum]);
                prefab.transform.localScale = new Vector3(1, 1, 1);

                var section = prefab.transform.GetChild(0).GetComponent<Text>();

                section.name += $"{productType.ToString()}s";
                section.text = LanguageManager.GetLocalizedText(section.name);

                Text[] temp = new Text[localizedTexts.Length + 1];
                for (int i = 0; i < localizedTexts.Length; i++) temp[i] = localizedTexts[i];
                temp[temp.Length - 1] = section;
                localizedTexts = temp;

                lastType = productType;
            }
        }

        Transform CreateUIObject(GameObject productPrefab, int parentNum)
        {
            GameObject uiObject = Instantiate(productPrefab) as GameObject;

            uiObject.transform.SetParent(productsParent[parentNum]);
            uiObject.transform.localScale = new Vector3(1, 1, 1);

            return uiObject.transform;
        }

        MedalInfo CreateMedalInfo(GameObject prefab, Transform parent, int lvl)
        {
            GameObject prebab = Instantiate(prefab) as GameObject;

            prebab.transform.SetParent(parent);
            prebab.transform.localScale = new Vector3(1, 1, 1);

            MedalInfo medalInfo = new MedalInfo(prebab.transform.GetChild(0).GetComponent<Image>(), prebab.transform.GetChild(1).GetComponent<Text>(), prebab.transform.GetChild(0).GetComponent<Button>());

            medalInfo.medalButton.onClick.AddListener(() => SFX(SoundTypes.Button));
            medalInfo.medalButton.onClick.AddListener(() => SelectMedal(lvl));

            medalInfo.medalImage.sprite = prestigeAvatars[lvl];

            medalNames.Add(medalInfo.medalLvl);

            medalInfo.medalLvl.text = $"{LanguageManager.GetLocalizedText("Lvl")} {lvl}";

            return medalInfo;
        }
    }

    //
    int clickCount = 0;
    int maxClickCount = (int)(1 / 0.02f) / 2;

    bool b = false;
    async void Check()
    {
        while (Application.isPlaying)
        {
            await System.Threading.Tasks.Task.Delay(1000);

            if (clickCount > maxClickCount)
            {
                MyDebug.LogWarning("Pressing too often");
                b = true;
            }
            else b = false;

            clickCount = 0;
        }
    }
    //

    public static List<T> GetProducts<T>() where T : Product
    {
        List<T> productsT = new List<T>();

        foreach (var p in products)
        {
            if (p.Value.Product is T) productsT.Add(p.Value.Product as T);
        }

        if (productsT.Count == 0) return null;

        return productsT;
    }
    public static T GetProduct<T>(string name) where T : Product
    {
        if (products.ContainsKey(name))
        {
            if (products[name].Product is T productT) return productT;
            return null;
        }
        return null;
    }
    public static bool TryGetProduct<T>(string name, out T product) where T : Product
    {
        product = null;

        if (products.ContainsKey(name))
        {
            if (products[name].Product is T productT)
            {
                product = productT;
                return true;
            }
        }

        return false;
    }
    public static bool HasProduct<T>(string name) where T : Product
    {
        if (products.ContainsKey(name)) return products[name].Product is T;

        return false;
    }

    public static ProductItem<T> GetProductItem<T>(string name) where T : Product
    {
        if (products.ContainsKey(name))
        {
            T t = products[name].Product as T;

            ProductItem<T> product = new ProductItem<T>(products[name].uiInfo, t);

            return product;
        }
        return null;
    }
    public static bool TryGetProductItem<T>(string name, out ProductItem<T> productItem) where T : Product
    {
        productItem = null;

        if (products.ContainsKey(name))
        {
            T t = products[name].Product as T;

            ProductItem<T> productItemT = new ProductItem<T>(products[name].uiInfo, t);

            productItem = productItemT;
        }

        return productItem != null && productItem.Product != null && productItem.uiInfo != null;
    }
    public static bool HasProductItem<T>(string name) where T : ProductItem<Product>
    {
        if (products.ContainsKey(name)) return products[name] is T;

        return false;
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
        clickCount++;
        if (b) return;

        EventManager.eventManager.Click(GameDataManager.data.clickBonus);
        IncreaseSpeed();
    }

    public async void Notify(bool enable)
    {
        notify.gameObject.SetActive(enable);

        while (notify.gameObject.activeSelf && Application.isPlaying)
        {
            Color color = new Color(1, 0.65f, 0, (float)Math.Abs(Math.Sin(Time.time)));

            notify.color = color;

            await System.Threading.Tasks.Task.Yield();
        }
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

    public void ShowOrHideMedalsList()
    {
        string name = "Medals";
        if (!panels[name].activeSelf)
        {
            panels[name].SetActive(true);
            MoveToTarget(name);
        }
        else MoveToStartPos(name, default, OnClose);
    }
    public void SelectMedal(int avatarIndex)
    {
        prestigeImage.sprite = prestigeAvatars[avatarIndex];
        GameDataManager.data.prestigeAvatarIndex = avatarIndex;

        ShowOrHideMedalsList();
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

            foreach (var boosterItem in products)
            {
                if (boosterItem.Value.Product is IDefendBooster booster) booster.CheckAvailabilityUse();
            }

            CancelInvoke("NextDay");
            return;
        }

        calendar++;
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

            if (i > 0 && Math.Truncate(money) != money) return $"{money.ToString("F2")}{names[i]}";

            return $"{(int)money}{names[i]}";
        }

        string SplitUp() => money > 0 ? $"{money:# ### ### ###}" : "0";
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
    private float speed = 0, speedAcceleration = 0.0005f, speedLimit = 0.005f, timer = 0.15f;

    public void OnChangeValue()
    {
        sliderModifierColor.color = Color.HSVToRGB((sliderModifier.value - 1) / 8, 1, 1);
    }

    float deltaClickTime = 0;
    private void IncreaseSpeed()
    {
        float clickPower = 1 + GameDataManager.data.timerIncreasingValue;
        float clickKoef = (float)Math.Pow(0.55f, sliderModifier.value - 1);

        clickKoef += clickKoef * GameDataManager.data.timerIncreasingValue;

        timer = clickPower / (sliderModifier.value * 2);

        if (deltaClickTime > clickKoef) deltaClickTime = clickKoef;

        float speedBoost = speedAcceleration * (clickKoef - deltaClickTime);

        if (speed < 0) speedBoost *= (sliderModifier.maxValue / sliderModifier.value);

        if (speedBoost < speedLimit && speed < speedLimit) speed += speedBoost;
        else speed = speedLimit;
        
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
                if (speed > -speedLimit * (sliderModifier.value / 2)) speed -= speedLimit / 10 * (sliderModifier.value / 2);
                else speed = -speedLimit * (sliderModifier.value / 2);
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
                ClickerItem<Clicker> clickerItem;
                if (TryGetProductItem(name, out ProductItem<Clicker> productItem))
                {
                    clickerItem = new ClickerItem<Clicker>(productItem.uiInfo as ClickerShopItem, productItem.Product);
                }
                else return;

                clickerItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {clicker.level}";
                clickerItem.uiInfo.price.text = clicker.currentPrice.ToString();
                clickerItem.uiInfo.clickPower.text = $"+{clicker.clickPowerDefault}/{LanguageManager.GetLocalizedText("Click")}";

                tutorials[1].transform.GetChild(1).gameObject.SetActive(false);

                tutorials[0].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialGame_2");
                tutorials[1].transform.GetChild(2).GetComponent<Text>().text = LanguageManager.GetLocalizedText("TutorialShop_2");

                tutorials[2].gameObject.SetActive(true);
                tutorials[3].gameObject.SetActive(true);

                SFXManager.PlaySound(SoundTypes.Buy);
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

        GameData data = GameDataManager.data;

        soldiers.text = FormatMoney(data.soldiersCount);
        soldiersFull.text = FormatMoney(data.soldiersCount, FormatType.SplitUp);

        aliensHearts.text = FormatMoney(data.aliensHearts);
        aliensHeartsFull.text = FormatMoney(data.aliensHearts, FormatType.SplitUp);

        textClickBonus.text = $"+{FormatMoney(data.clickBonus * data.permanentSoldierModifier * SoldierBooster.SoldierModifier)}/{LanguageManager.GetLocalizedText("Click")}";

        textAutoclickerBonus.text = $"+{FormatMoney(data.autoClickerBonus * data.permanentSoldierModifier * SoldierBooster.SoldierModifier)}/{LanguageManager.GetLocalizedText("Sec")}";

        textOfflineClickerBonus.text = $"+{FormatMoney(data.offlineClickBonus * data.permanentSoldierModifier)}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeAutoclickerBonus.gameObject.activeSelf) prestigeAutoclickerBonus.text = $"+{FormatMoney((int)(data.autoClickerBonus * (data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        if (prestigeOfflineClickerBonus.gameObject.activeSelf) prestigeOfflineClickerBonus.text = $"+{FormatMoney((int)(data.offlineClickBonus * (data.prestigeLvl * 0.1f)))}/{LanguageManager.GetLocalizedText("Sec")}";

        if (data.wasAttack && data.hasLost && data.soldiersCount >= 1000000)
        {
            panels["Prestige"].transform.GetChild(2).GetComponent<Button>().interactable = true;
            data.hasLost = false;
            OpenPrestigePanel();
        }

        foreach (var p in products)
        {
            if (p.Value.Product is Clicker clicker) CheckPossibilityToBuy(clicker);
            else if (p.Value.Product is Booster booster)
            {
                CheckPossibilityToBuy(booster);
                CheckPossibillityToUse(booster);
            }
            else CheckPossibilityToBuy(p.Value.Product);
        }

        heartsSkip.interactable = data.aliensHearts >= data.SkipCost;

        void CheckPossibilityToBuy(Product product)
        {
            if (product is Clicker)
            {
                ClickerItem<Clicker> clickerItem;
                if (TryGetProductItem(product.name, out ProductItem<Clicker> productItem))
                {
                    clickerItem = new ClickerItem<Clicker>(productItem.uiInfo as ClickerShopItem, productItem.Product);
                }
                else return;

                if (clickerItem.Product.currency == Currency.Soldier)
                {
                    if (data.soldiersCount < clickerItem.Product.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                    else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
                }
                else
                {
                    if (data.aliensHearts < clickerItem.Product.currentPrice) clickerItem.uiInfo.bttnBuy.interactable = false;
                    else if (!clickerItem.uiInfo.bttnBuy.interactable) clickerItem.uiInfo.bttnBuy.interactable = true;
                }
            }
            else if (product is Booster)
            {
                BoosterItem<Booster> boosterItem;
                if (TryGetProductItem(product.name, out ProductItem<Booster> productItem))
                {
                    boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
                }
                else return;

                if (boosterItem.Product.currency == Currency.Soldier)
                {
                    if (data.soldiersCount < boosterItem.Product.priceDefault) boosterItem.uiInfo.bttnBuy.interactable = false;
                    else if (!boosterItem.uiInfo.bttnBuy.interactable) boosterItem.uiInfo.bttnBuy.interactable = true;
                }
                else
                {
                    if (data.aliensHearts < boosterItem.Product.priceDefault) boosterItem.uiInfo.bttnBuy.interactable = false;
                    else if (!boosterItem.uiInfo.bttnBuy.interactable) boosterItem.uiInfo.bttnBuy.interactable = true;
                }
            }
            else if (product is SpecialAmplification)
            {
                SpecialAmplificationItem<SpecialAmplification> specItem;
                if (TryGetProductItem(product.name, out ProductItem<SpecialAmplification> productItem))
                {
                    specItem = new SpecialAmplificationItem<SpecialAmplification>(productItem.uiInfo as SpecialAmplificationShopItem, productItem.Product);
                }
                else return;

                if (data.aliensHearts < specItem.Product.currentPrice) specItem.uiInfo.bttnBuy.interactable = false;
                else if (!specItem.uiInfo.bttnBuy.interactable) specItem.uiInfo.bttnBuy.interactable = true;
            }
        }

        void CheckPossibillityToUse(Booster booster)
        {
            BoosterItem<Booster> boosterItem;
            if (TryGetProductItem(booster.name, out ProductItem<Booster> productItem))
            {
                boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
            }
            else return;

            BoosterShopItem shopItem = boosterItem.uiInfo;

            if (booster.amount <= 0) shopItem.bttnUse.interactable = false;
            else if (!shopItem.bttnUse.interactable && !booster.IsUsing)
            {
                if (booster is IDefendBooster defendBooster) defendBooster.CheckAvailabilityUse();
                else shopItem.bttnUse.interactable = true;
            }
        }
    }

    private void OnHpHasChanged()
    {
        int soldiersCount = GameDataManager.data.soldiersCount;

        if (soldiersCount < 0)
        {
            GameDataManager.data.soldiersCount = 0;
            soldiersCount = 0;
        }

        area51Hp.value = soldiersCount;
        soldiers.text = FormatMoney(soldiersCount);
        soldiersFull.text = FormatMoney(soldiersCount, FormatType.SplitUp);

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

        foreach (var boosterItem in products)
        {
            if (boosterItem.Value.Product is IDefendBooster booster) booster.CheckAvailabilityUse();
        }
    }

    private void OnBoosterUse<T>(T booster, bool hasEnded) where T : Booster
    {
        if (booster == null)
        {
            MyDebug.LogError($"Booster not found (null)");
            return;
        }

        BoosterItem<Booster> boosterItem;
        if (TryGetProductItem(booster.name, out ProductItem<Booster> productItem))
        {
            boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
        }
        else return;

        boosterItem.uiInfo.bttnUse.interactable = hasEnded && boosterItem.Product.amount > 0;
        
        boosterItem.UpdateIconInfo(!hasEnded);

        boosterItem.uiInfo.amount.text = $"{boosterItem.Product.amount}x";
    }

    private void OnGiftTimer(GiftTimer timer)
    {
        if(!timer.IsFinished) giftTimer.text = timer.ToString();
        else
        {
            giftTimer.text = LanguageManager.GetLocalizedText("Open");

            gift.interactable = true;

            Notify(!panels["More"].activeSelf);
        }

        heartsSkip.interactable = GameDataManager.data.aliensHearts >= GameDataManager.data.SkipCost;
        heartsSkip.transform.GetChild(0).GetComponent<Text>().text = GameDataManager.data.SkipCost.ToString();
    }

    private void OnFinishBuy(Product product, bool success)
    {
        if (success)
        {
            ChangeInfo();

            if (product is IAutocliker acl) acl.AutoClick();

            void ChangeInfo()
            {
                if (product is Clicker clicker)
                {
                    ClickerItem<Clicker> clickerItem;
                    if (TryGetProductItem(product.name, out ProductItem<Clicker> productItem))
                    {
                        clickerItem = new ClickerItem<Clicker>(productItem.uiInfo as ClickerShopItem, productItem.Product);
                    }
                    else return;

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
                    BoosterItem<Booster> boosterItem;
                    if (TryGetProductItem(product.name, out ProductItem<Booster> productItem))
                    {
                        boosterItem = new BoosterItem<Booster>(productItem.uiInfo as BoosterShopItem, productItem.Product);
                    }
                    else return;

                    boosterItem.uiInfo.amount.text = $"{booster.amount}x";
                    boosterItem.uiInfo.price.text = FormatMoney(booster.priceDefault);
                }
                else if (product is SpecialAmplification spec)
                {
                    SpecialAmplificationItem<SpecialAmplification> specItem;
                    if (TryGetProductItem(product.name, out ProductItem<SpecialAmplification> productItem))
                    {
                        specItem = new SpecialAmplificationItem<SpecialAmplification>(productItem.uiInfo as SpecialAmplificationShopItem, productItem.Product);
                    }
                    else return;

                    specItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {spec.level}";
                    specItem.uiInfo.price.text = FormatMoney(spec.currentPrice);
                    specItem.uiInfo.modifierValue.text = $"{specItem.Product.modifierValue}x"; ;
                }
                else MyDebug.LogWarning($"Product not found");
            }
        }
        else MyDebug.LogError($"Product not bought");

        OnChangeText();
    }

    private void OnGiftOpen(GameObject obj)
    {
        obj.SetActive(false);

        giftBlackout.giftBox.SetActive(true);
        giftBlackout.giftCap.SetActive(true);

        EventManager.eventManager.OnAnimationEnd -= OnGiftOpen;
        EventManager.eventManager.OnAnimationEnd += OnGiftCapOpen;

        giftBlackout.giftCap.GetComponent<Animation>().Play();
    }
    private void OnGiftCapOpen(GameObject obj)
    {
        EventManager.eventManager.OnAnimationEnd += OnGiftOpen;
        EventManager.eventManager.OnAnimationEnd -= OnGiftCapOpen;

        ShowReward();
    }

    private void OnAdFinished(RewardBonus rewardBonus)
    {
        switch (rewardBonus.bonus)
        {
            case RewardBonus.Bonus.SkipTimer:
                int skipSeconds = rewardBonus.amount * 60;
                GameDataManager.data.giftTimer.DecreaseTime(skipSeconds);
                break;
            case RewardBonus.Bonus.HeartsBonus:
                GameDataManager.data.aliensHearts += rewardBonus.amount;
                break;
            default:
                MyDebug.LogWarning("Reward bonus not found");
                break;
        }

        OnChangeText();
    }

    #endregion /OnEvent

    #region Shop

    public void ShowDescription(Text name)
    {
        RectTransform description = null;

        if (HasProduct<Clicker>(name.name)) description = descriptions[0];
        else if (HasProduct<Booster>(name.name)) description = descriptions[1];
        else if (HasProduct<SpecialAmplification>(name.name)) description = descriptions[2];
        else if (HasProduct<Offer>(name.name)) description = descriptions[3];
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
                SFXManager.PlaySound(SoundTypes.Buy);
            }
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

    public void Use(Text name)
    {
        if (TryGetProduct(name.name, out Booster booster))
            booster.Use();
        else MyDebug.LogError($"Booster {name.name} not found");
    }

    #endregion

    #endregion /Shop

    #region More

    #region Gift

    public void OpenGift()
    {
        giftBlackout.root.SetActive(true);
        giftBlackout.imageGift.SetActive(true);
        giftBlackout.imageGift.GetComponent<Animation>().Play();

        gift.gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        Animation giftAnim = giftBlackout.imageGift.GetComponent<Animation>();
        Animation giftCapAnim = giftBlackout.giftCap.GetComponent<Animation>();

        if (giftAnim.isPlaying)
        {
            giftAnim.Stop();
            giftBlackout.imageGift.SetActive(false);
            giftBlackout.giftCap.SetActive(false);
            giftBlackout.giftBox.SetActive(true);

            ShowReward();
            return;
        }
        else if (giftCapAnim.isPlaying)
        {
            giftCapAnim.Stop();
            giftBlackout.giftCap.SetActive(false);

            ShowReward();
            return;
        }

        giftBlackout.rewardInfo.SetActive(false);
        giftBlackout.SetActive(false);

        gift.gameObject.SetActive(true);
        gift.interactable = false;

        GameDataManager.data.giftTimer.Reset(60,0);
        GameDataManager.data.giftTimer.Start();

        giftTimer.text = GameDataManager.data.giftTimer.ToString();
    }

    public void ShowReward()
    {
        EventManager.eventManager.GenerateReward((reward) =>
        {
            giftBlackout.rewardInfo.SetActive(true);

            if (reward.rewardType != RewardTypes.AlienHearts && reward.rewardType != RewardTypes.Soldiers)
            {
                giftBlackout.rewardInfo.rewardImage.sprite = GetProductItem<Product>(reward.name)?.uiInfo?.avatar;
                giftBlackout.rewardInfo.rewardText.text = LanguageManager.GetLocalizedText("Upgrading");
            }
            else
            {
                giftBlackout.rewardInfo.rewardImage.sprite = Resources.Load<Sprite>(reward.name);
                giftBlackout.rewardInfo.rewardText.text = reward.amount.ToString();
            }

            OnChangeText();
        });
    }

    public void SkipMenu(Image skipMenuImage)
    {
        bool active = panels["SkipMenu"].activeSelf;

        panels["SkipMenu"].SetActive(!active);

        skipMenuImage.color = !active ? Color.green : Color.white;
    }

    public void AdSkip(Image skipMenuImage)
    {
        SkipMenu(skipMenuImage);
    }
    public void HeartsSkip(Image skipMenuImage)
    {
        GameData data = GameDataManager.data;
        if (data.aliensHearts < data.SkipCost)
        {
            MyDebug.LogError("Not enough hearts");
            return;
        }

        data.aliensHearts -= data.SkipCost;

        data.giftTimer.DecreaseTime(-1);

        OnChangeText();

        SkipMenu(skipMenuImage);
    }

    #endregion /Gift

    #region Settings

    public void Settings(Text text)
    {
        string name = "Settings";

        if (!panels[name].activeSelf)
        {
            panels[name].SetActive(true);

            MoveToTarget(name);

            text.color = Color.green;
        }
        else
        {
            MoveToStartPos(name, default, OnClose);

            text.color = Color.white;
        }
    }

    public void SFX(string name) => SFXManager.PlaySound(Enum.TryParse(name, out SoundTypes sound) ? sound : default);
    public void SFX(SoundTypes sound) => SFXManager.PlaySound(sound);

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

    public void Notifications(bool isStart = false)
    {
        bool enableNotify = GameDataManager.data.enableNotificaions;

        if (!isStart) enableNotify = !enableNotify;

        if (enableNotify)
        {
            buttonNotify.color = new Color(0, 0.75f, 0);
            toggleNotify.rectTransform.anchoredPosition = new Vector2(150, 0);
        }
        else
        {
            buttonNotify.color = new Color(0.75f, 0, 0);
            toggleNotify.rectTransform.anchoredPosition = new Vector2(0, 0);
        }

        GameDataManager.data.enableNotificaions = enableNotify;
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
            foreach (var p in products)
            {
                if (TryGetProductItem(p.Key, out ProductItem<Clicker> productItemC))
                {
                    ClickerItem<Clicker> clickerItem = new ClickerItem<Clicker>(productItemC.uiInfo as ClickerShopItem, productItemC.Product);
                    ChangeClickerItemInfo(clickerItem);
                }
                else if (TryGetProductItem(p.Key, out ProductItem<Booster> productItemB))
                {
                    BoosterItem<Booster> boosterItem = new BoosterItem<Booster>(productItemB.uiInfo as BoosterShopItem, productItemB.Product);
                    ChangeBoosterItemInfo(boosterItem);
                }
                else if (TryGetProductItem(p.Key, out ProductItem<SpecialAmplification> productItemS))
                {
                    SpecialAmplificationItem<SpecialAmplification> specItem = new SpecialAmplificationItem<SpecialAmplification>(productItemS.uiInfo as SpecialAmplificationShopItem, productItemS.Product);
                    ChangeSpecAmplificationItemInfo(specItem);
                }
                else if (TryGetProductItem(p.Key, out ProductItem<Offer> productItemO))
                {
                    OfferItem<Offer> offerItem = new OfferItem<Offer>(productItemO.uiInfo as OfferShopItem, productItemO.Product);
                    ChangeOfferItemInfo(offerItem);
                }
            }

            void ChangeClickerItemInfo(ClickerItem<Clicker> clickerItem)
            {
                Clicker clicker = clickerItem.Product;
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
                Booster booster = boosterItem.Product;
                string ability = $"{booster.abilityModifier}x";
                if (booster is DefenceBooster || booster is RegenBooster)
                {
                    ability = booster is DefenceBooster ? $"{booster.abilityModifier * 100}%" : $"{booster.abilityModifier * 60}/{LanguageManager.GetLocalizedText("S")}";
                }

                boosterItem.uiInfo.name.text = LanguageManager.GetLocalizedText(boosterItem.uiInfo.name.name);
                boosterItem.uiInfo.functional.text = $"{LanguageManager.GetLocalizedText(booster.GetType().ToString())} {ability} - {booster.useTime}{LanguageManager.GetLocalizedText("Sec")}";
                boosterItem.uiInfo.use.text = LanguageManager.GetLocalizedText("Use");
            }

            void ChangeSpecAmplificationItemInfo(SpecialAmplificationItem<SpecialAmplification> specItem)
            {
                SpecialAmplification specAmplif = specItem.Product;

                specItem.uiInfo.name.text = LanguageManager.GetLocalizedText(specItem.uiInfo.name.name);
                specItem.uiInfo.level.text = $"{LanguageManager.GetLocalizedText("Level")} {specAmplif.level}";
            }

            void ChangeOfferItemInfo(OfferItem<Offer> offerItem)
            {
                Offer offer = offerItem.Product;

                offerItem.uiInfo.name.text = LanguageManager.GetLocalizedText(offerItem.uiInfo.name.name);
                offerItem.uiInfo.price.text = $"{offer.productAmount} {LanguageManager.GetLocalizedText(offer.currency.ToString())}";
            }
        }
        void ChangeTexts()
        {
            foreach (var t in localizedTexts) if (t != null) t.text = LanguageManager.GetLocalizedText(t.name);

            calendar.month.text = LanguageManager.GetLocalizedText(calendar.months.Peek());

            for (int i = 0; i < medalNames.Count; i++)
            {
                medalNames[i].text = $"{LanguageManager.GetLocalizedText("Lvl")} {i}";
            }
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

        GameData data = GameDataManager.data;

        int aliensHearts = data.aliensHearts;
        int prestige = data.prestigeLvl;
        int clickBonus = 0;
        int autoclickerBonus = 0;
        int offlineClickerBonus = 0;
        float? timeToWinLeft = data.timeToWinLeft;
        float? enemySpawnStep = data.enemySpawnStep;
        float timerIncreasingValue = 0;
        string password = data.passwordDebug;
        string language = data.language;
        bool debugEnabled = data.debugEnabled;
        bool wasTutorial = data.wasTutorial;
        bool inversedScale = data.inversedScale;

        List<Product> products = new List<Product>();
        List<Clicker> clickers = new List<Clicker>();
        List<Booster> boosters = new List<Booster>();
        List<SpecialAmplification> specs = new List<SpecialAmplification>();

        foreach (var p in data.products)
        {
            if (p.Value is AutoClicker) Product.DownCast<AutoClicker>(p.Value).hasStart = false;
            else if (p.Value is UniversalClicker) Product.DownCast<UniversalClicker>(p.Value).hasStart = false;

            if (p.Value is Booster booster) Product.DownCast<Booster>(p.Value).useTimeRemained = 0;

            if (p.Value.currency == Currency.AlienHeart) products.Add(p.Value);
        }

        data = new GameData();

        prestige++;
        timeToWinLeft = 90 + (30 * prestige);
        if (enemySpawnStep >= 0.02f) enemySpawnStep *= 0.75f;

        int heartsBonus = 1000;
        data.aliensHearts = aliensHearts + heartsBonus + (prestige * heartsBonus / 10);
        data.soldiersCount = 1000000 * prestige + 51;
        data.prestigeLvl = prestige;
        data.timeToWinLeft = timeToWinLeft;
        data.enemySpawnStep = enemySpawnStep;
        data.passwordDebug = password;
        data.language = language;
        data.debugEnabled = debugEnabled;
        data.wasTutorial = wasTutorial;
        data.inversedScale = inversedScale;

        foreach (var p in products)
        {
            if (p is Clicker)
            {
                if (p is ManualClicker mclicker) clickBonus += mclicker.allClickPower;
                else if (p is AutoClicker aclicker) autoclickerBonus += aclicker.allClickPower;
                else if (p is OfflineClicker oclicker) offlineClickerBonus += oclicker.allClickPower;
                else if (p is UniversalClicker uclicker)
                {
                    clickBonus += uclicker.allClickPower;
                    autoclickerBonus += uclicker.allClickPower * 5;
                    offlineClickerBonus += uclicker.allClickPower * 5;
                }
            }
            else if (p is SpecialAmplification sa) timerIncreasingValue += sa.modifierValue;

            data.products.Add(p.name, p);
        }
    
        data.clickBonus = clickBonus;
        data.autoClickerBonus = autoclickerBonus;
        data.offlineClickBonus = offlineClickerBonus;

        data.timerIncreasingValue = timerIncreasingValue;

        GameDataManager.data = data;

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

    public void ForceOpenGift() => GameDataManager.data.giftTimer.Reset(0, 1);

    public void ChangeMaxClickCount(Slider slider)
    {
        maxClickCount = (int)slider.value;
        debugMaxCount.text = maxClickCount.ToString();
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

    [Serializable]
    public struct GiftBlackout
    {
        public GameObject root, imageGift, giftBox, giftCap;
        public RewardInfo rewardInfo;

        public void SetActive(bool enable)
        {
            root.SetActive(enable);
            imageGift.SetActive(enable);
            giftBox.SetActive(enable);
            giftCap.SetActive(enable);
        }

        [Serializable]
        public struct RewardInfo
        {
            public GameObject root;
            public Text rewardText;
            public Image rewardImage;

            public void SetActive(bool enable)
            {
                rewardText.gameObject.SetActive(enable);
                rewardImage.gameObject.SetActive(enable);
                root.SetActive(enable);
            }
        }
    }

    public class MedalInfo
    {
        public MedalInfo(Image medalImage, Text medalLvl, Button medalButton)
        {
            this.medalImage = medalImage;
            this.medalLvl = medalLvl;
            this.medalButton = medalButton;
        }

        public Image medalImage;
        public Text medalLvl;
        public Button medalButton;
    }

    private enum FormatType
    {
        Truncate,
        SplitUp
    }
}
