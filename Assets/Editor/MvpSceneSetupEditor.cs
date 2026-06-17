#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

/// <summary>
/// 一键搭建 MVP 场景：UI 色块占位、组件挂载、引用连线。
/// </summary>
public static class MvpSceneSetupEditor
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string LaunchScenePath = "Assets/Scenes/Launch.unity";
    private const string PrefabDir = "Assets/_Project/Prefabs/UI";
    private const string DefaultFontPath = "Assets/Font/GeourceAltCHT-Medium.ttf";
    private const string MarketBgSpritePath = "Assets/Art/MVP/Scenes/S-01_市场背景.png";
    private const string OpenBgSpritePath = "Assets/Art/MVP/Scenes/S-02_开果台背景.png";
    private const string DurianSpriteConfigPath = "Assets/_Project/Data/DurianSpriteConfig.asset";

    [MenuItem("Tools/llopen/Create DurianSpriteConfig Asset")]
    public static void CreateDurianSpriteConfigAsset()
    {
        EnsureFolder("Assets/_Project/Data");
        var existing = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(DurianSpriteConfigPath);
        if (existing != null)
        {
            Selection.activeObject = existing;
            Debug.Log("[MvpSceneSetup] DurianSpriteConfig 已存在，请在 Inspector 中拖入 41 张 Sprite。");
            return;
        }

        var asset = ScriptableObject.CreateInstance<DurianSpriteConfig>();
        AssetDatabase.CreateAsset(asset, DurianSpriteConfigPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        Debug.Log("[MvpSceneSetup] 已创建 DurianSpriteConfig.asset，请在 Inspector 中拖入 41 张 Sprite。");
    }

    [MenuItem("Tools/llopen/Apply Room Prefab Sprites")]
    public static void ApplyRoomPrefabSpritesMenu()
    {
        var spriteConfig = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(DurianSpriteConfigPath);
        var meat = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/RoomMeat.prefab");
        var empty = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/RoomEmpty.prefab");
        ApplyRoomPrefabSprites(meat, empty, spriteConfig);
        AssetDatabase.SaveAssets();
        Debug.Log("[MvpSceneSetup] RoomMeat / RoomEmpty 已应用 FL-01 / FL-02 贴图。");
    }

    [MenuItem("Tools/llopen/Setup MVP Scene")]
    public static void SetupMvpScene()
    {
        EnsureFolder(PrefabDir);
        var roomMeatPrefab = CreateBlockPrefab("RoomMeat", new Color(1f, 0.84f, 0.2f), new Vector2(80, 80));
        var roomEmptyPrefab = CreateBlockPrefab("RoomEmpty", new Color(0.55f, 0.55f, 0.55f), new Vector2(80, 80));
        var spriteConfig = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(DurianSpriteConfigPath);
        ApplyRoomPrefabSprites(roomMeatPrefab, roomEmptyPrefab, spriteConfig);
        var floatTextPrefab = CreateFloatTextPrefab();
        var bagCardPrefab = CreateBagCardPrefab();

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        var mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        BuildMainScene(roomMeatPrefab, roomEmptyPrefab, floatTextPrefab, bagCardPrefab);
        EditorSceneManager.SaveScene(mainScene);

        var launchScene = EditorSceneManager.OpenScene(LaunchScenePath, OpenSceneMode.Single);
        BuildLaunchScene();
        EditorSceneManager.SaveScene(launchScene);

        AssetDatabase.SaveAssets();
        Debug.Log("[MvpSceneSetup] Main + Launch 场景搭建完成。");

        EditorSceneManager.OpenScene("Assets/Scenes/Demo.unity", OpenSceneMode.Single);
    }

    private static void BuildMainScene(
        GameObject roomMeatPrefab,
        GameObject roomEmptyPrefab,
        GameObject floatTextPrefab,
        GameObject bagCardPrefab)
    {
        var oldRoot = GameObject.Find("GameRoot");
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot);
        }

        var oldUi = GameObject.Find("MVP_Canvas");
        if (oldUi != null)
        {
            Object.DestroyImmediate(oldUi);
        }

        var oldEvent = GameObject.Find("EventSystem");
        if (oldEvent != null)
        {
            Object.DestroyImmediate(oldEvent);
        }

        var oldBootstrap = GameObject.Find("GameBootstrap");
        if (oldBootstrap != null)
        {
            Object.DestroyImmediate(oldBootstrap);
        }

        var gameRoot = new GameObject("GameRoot");
        gameRoot.AddComponent<GameLifetimeScope>();
        gameRoot.AddComponent<GameBootstrapper>();

        var canvasGo = CreateCanvas();
        var uiRootGo = CreatePanel(canvasGo.transform, "UIRoot", fullStretch: true);
        var uiRootImage = uiRootGo.GetComponent<Image>();
        if (uiRootImage != null)
        {
            uiRootImage.color = new Color(0f, 0f, 0f, 0f);
            uiRootImage.raycastTarget = false;
        }

        var uiRoot = uiRootGo.AddComponent<GameUIRoot>();
        var (marketBgImage, openBgImage) = BuildBackgroundImages(uiRootGo.transform);

        var marketPage = BuildMarketPage(uiRootGo.transform);
        var openPage = BuildOpenPage(uiRootGo.transform, roomMeatPrefab, roomEmptyPrefab, floatTextPrefab);
        var sellPage = BuildSellPage(uiRootGo.transform);
        var bagPage = BuildBagPage(uiRootGo.transform, bagCardPrefab);
        var shopPage = BuildShopPage(uiRootGo.transform);

        WireUIRoot(uiRoot, marketPage, openPage, sellPage, bagPage, shopPage, marketBgImage, openBgImage);
        WireGameLifetimeScope(gameRoot, uiRoot, marketPage, openPage, sellPage, bagPage, shopPage);

        marketPage.SetActive(false);
        openPage.SetActive(false);
        sellPage.SetActive(false);
        bagPage.SetActive(false);
        shopPage.SetActive(false);
    }

    private static void BuildLaunchScene()
    {
        var launchRoot = GameObject.Find("GameRoot");
        if (launchRoot != null)
        {
            Object.DestroyImmediate(launchRoot);
        }

        var bootstrap = GameObject.Find("GameBootstrap");
        if (bootstrap == null)
        {
            bootstrap = new GameObject("GameBootstrap");
        }

        if (bootstrap.GetComponent<GameBootstrap>() == null)
        {
            bootstrap.AddComponent<GameBootstrap>();
        }

        if (bootstrap.GetComponent<MvpSceneLoader>() == null)
        {
            bootstrap.AddComponent<MvpSceneLoader>();
        }
    }

    private static GameObject BuildMarketPage(Transform parent)
    {
        var page = CreatePanel(parent, "MarketPage", fullStretch: true);
        page.AddComponent<MarketPage>();
        var mp = page.GetComponent<MarketPage>();

        var header = CreatePanel(page.transform, "Header", false, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 0));
        var goldText = CreateText(header.transform, "GoldText", "金币 200", 28, TextAnchor.MiddleLeft);
        Stretch(goldText.rectTransform, 20, 10, -20, -10);

        var varietyRow = CreatePanel(page.transform, "VarietyRow", false,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -140), new Vector2(0, -70));
        var varietyButtons = new Button[3];
        var names = new[] { "金枕", "干尧", "猫山王" };
        for (var i = 0; i < 3; i++)
        {
            varietyButtons[i] = CreateButton(varietyRow.transform, $"Variety_{i}", names[i],
                new Vector2(0.02f + i * 0.33f, 0.1f), new Vector2(0.31f + i * 0.33f, 0.9f),
                new Color(0.2f, 0.45f, 0.75f));
        }

        var cardRow = CreatePanel(page.transform, "CardRow", false,
            new Vector2(0, 0.25f), new Vector2(1, 0.85f), Vector2.zero, Vector2.zero);
        var images = new Image[3];
        var priceTexts = new Text[3];
        var appearanceTexts = new Text[3];
        var buyButtons = new Button[3];
        var smellButtons = new Button[3];

        for (var i = 0; i < 3; i++)
        {
            var card = CreatePanel(cardRow.transform, $"Card_{i}", false,
                new Vector2(0.02f + i * 0.32f, 0), new Vector2(0.3f + i * 0.32f, 1), Vector2.zero, Vector2.zero);
            SetImageColor(card, new Color(0.15f, 0.15f, 0.18f));

            images[i] = CreateImage(card.transform, "DurianImage", new Color(0.3f, 0.65f, 0.35f),
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.95f));
            appearanceTexts[i] = CreateText(card.transform, "Appearance", "Normal", 20, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.34f));
            priceTexts[i] = CreateText(card.transform, "Price", "80", 24, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.1f), new Vector2(0.45f, 0.22f));
            buyButtons[i] = CreateButton(card.transform, "Buy", "购买",
                new Vector2(0.5f, 0.08f), new Vector2(0.95f, 0.22f), new Color(0.85f, 0.35f, 0.15f));
            smellButtons[i] = CreateButton(card.transform, "Smell", "试闻",
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.08f), new Color(0.35f, 0.35f, 0.4f));
        }

        var footer = CreatePanel(page.transform, "Footer", false,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 120));
        var bagButton = CreateButton(footer.transform, "Bag", "背包",
            new Vector2(0.05f, 0.2f), new Vector2(0.3f, 0.8f), new Color(0.25f, 0.55f, 0.35f));
        var shopButton = CreateButton(footer.transform, "Shop", "商店",
            new Vector2(0.7f, 0.2f), new Vector2(0.95f, 0.8f), new Color(0.55f, 0.3f, 0.7f));

        var so = new SerializedObject(mp);
        so.FindProperty("goldText").objectReferenceValue = goldText;
        so.FindProperty("varietyButtons").arraySize = 3;
        for (var i = 0; i < 3; i++) so.FindProperty("varietyButtons").GetArrayElementAtIndex(i).objectReferenceValue = varietyButtons[i];
        WireArray(so, "durianImages", images);
        WireArray(so, "priceTexts", priceTexts);
        WireArray(so, "appearanceTexts", appearanceTexts);
        WireArray(so, "buyButtons", buyButtons);
        WireArray(so, "smellButtons", smellButtons);
        so.FindProperty("bagButton").objectReferenceValue = bagButton;
        so.FindProperty("shopButton").objectReferenceValue = shopButton;
        so.ApplyModifiedPropertiesWithoutUndo();

        return page;
    }

    private static GameObject BuildOpenPage(
        Transform parent,
        GameObject roomMeatPrefab,
        GameObject roomEmptyPrefab,
        GameObject floatTextPrefab)
    {
        var page = CreatePanel(parent, "OpenPage", fullStretch: true);
        page.AddComponent<OpenPage>();
        var op = page.GetComponent<OpenPage>();

        var durianImage = CreateImage(page.transform, "DurianImage", new Color(0.3f, 0.65f, 0.35f),
            new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
        var guideText = CreateText(page.transform, "Guide", "在榴莲顶部滑动开果", 26, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.24f));
        var estimateText = CreateText(page.transform, "Estimate", "出肉率约 --% · 估价 -- 金币", 22, TextAnchor.UpperRight,
            new Vector2(0.55f, 0.76f), new Vector2(0.95f, 0.92f));

        var swipeArea = CreatePanel(page.transform, "SwipeArea", false,
            new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.75f), Vector2.zero, Vector2.zero);
        swipeArea.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);

        var shell = new GameObject("DurianShell");
        shell.transform.SetParent(page.transform, false);
        var shellRect = shell.AddComponent<RectTransform>();
        Stretch(shellRect, 0, 0, 0, 0);
        shellRect.anchorMin = new Vector2(0.15f, 0.25f);
        shellRect.anchorMax = new Vector2(0.85f, 0.75f);
        shellRect.offsetMin = Vector2.zero;
        shellRect.offsetMax = Vector2.zero;

        var roomsRoot = new GameObject("RoomsRoot");
        roomsRoot.transform.SetParent(page.transform, false);
        var roomsRect = roomsRoot.AddComponent<RectTransform>();
        roomsRect.anchorMin = new Vector2(0.1f, 0.05f);
        roomsRect.anchorMax = new Vector2(0.9f, 0.2f);
        roomsRect.offsetMin = Vector2.zero;
        roomsRect.offsetMax = Vector2.zero;

        var spriteConfig = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(DurianSpriteConfigPath);
        var knifeImage = CreateImage(page.transform, "KnifeImage", Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        knifeImage.rectTransform.sizeDelta = new Vector2(140f, 140f);
        knifeImage.preserveAspect = true;
        knifeImage.raycastTarget = false;
        if (spriteConfig != null)
        {
            knifeImage.sprite = spriteConfig.knifeSprite;
        }
        knifeImage.gameObject.SetActive(false);

        var knifeGo = new GameObject("KnifeTool");
        knifeGo.transform.SetParent(page.transform, false);
        var knife = knifeGo.AddComponent<KnifeTool>();
        var line = knifeGo.GetComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.positionCount = 0;
        line.useWorldSpace = true;

        var openerGo = new GameObject("DurianOpener");
        openerGo.transform.SetParent(page.transform, false);
        var opener = openerGo.AddComponent<DurianOpener>();

        var ratingText = CreateText(page.transform, "Rating", "", 24, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.12f));
        var ratingIcon = CreateImage(page.transform, "RatingIcon", Color.white,
            new Vector2(0.12f, 0.05f), new Vector2(0.22f, 0.12f));
        ratingIcon.preserveAspect = true;
        ratingIcon.gameObject.SetActive(false);

        var shellLeftImage = CreateImage(page.transform, "ShellLeft", Color.white,
            new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
        shellLeftImage.preserveAspect = true;
        shellLeftImage.gameObject.SetActive(false);
        if (spriteConfig != null)
        {
            shellLeftImage.sprite = spriteConfig.shellLeftHalf;
        }

        var shellRightImage = CreateImage(page.transform, "ShellRight", Color.white,
            new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
        shellRightImage.preserveAspect = true;
        shellRightImage.gameObject.SetActive(false);
        if (spriteConfig != null)
        {
            shellRightImage.sprite = spriteConfig.shellRightHalf;
        }

        var sellButton = CreateButton(page.transform, "Sell", "卖出",
            new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.18f), new Color(0.85f, 0.55f, 0.1f));
        var reviveButton = CreateButton(page.transform, "Revive", "看广告复活",
            new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.08f), new Color(0.75f, 0.2f, 0.2f));
        var backButton = CreateButton(page.transform, "Back", "返回市场",
            new Vector2(0.02f, 0.92f), new Vector2(0.2f, 0.98f), new Color(0.35f, 0.35f, 0.4f));

        var knifeSo = new SerializedObject(knife);
        knifeSo.FindProperty("durianOpener").objectReferenceValue = opener;
        knifeSo.FindProperty("swipeArea").objectReferenceValue = swipeArea.GetComponent<RectTransform>();
        knifeSo.FindProperty("targetCamera").objectReferenceValue = Camera.main;
        knifeSo.FindProperty("crackLine").objectReferenceValue = line;
        knifeSo.FindProperty("knifeImage").objectReferenceValue = knifeImage;
        knifeSo.FindProperty("spriteConfig").objectReferenceValue = spriteConfig;
        knifeSo.FindProperty("shellWidth").floatValue = 4f;
        knifeSo.ApplyModifiedPropertiesWithoutUndo();

        var openerSo = new SerializedObject(opener);
        openerSo.FindProperty("spriteConfig").objectReferenceValue = spriteConfig;
        openerSo.FindProperty("shellTransform").objectReferenceValue = shell.transform;
        openerSo.FindProperty("durianImage").objectReferenceValue = durianImage;
        openerSo.FindProperty("shellLeftImage").objectReferenceValue = shellLeftImage;
        openerSo.FindProperty("shellRightImage").objectReferenceValue = shellRightImage;
        openerSo.FindProperty("roomsRoot").objectReferenceValue = roomsRoot.transform;
        openerSo.FindProperty("roomMeatPrefab").objectReferenceValue = roomMeatPrefab;
        openerSo.FindProperty("roomEmptyPrefab").objectReferenceValue = roomEmptyPrefab;
        openerSo.FindProperty("floatTextPrefab").objectReferenceValue = floatTextPrefab;
        openerSo.FindProperty("ratingText").objectReferenceValue = ratingText;
        openerSo.FindProperty("ratingIcon").objectReferenceValue = ratingIcon;
        openerSo.ApplyModifiedPropertiesWithoutUndo();

        var opSo = new SerializedObject(op);
        opSo.FindProperty("knifeTool").objectReferenceValue = knife;
        opSo.FindProperty("durianOpener").objectReferenceValue = opener;
        opSo.FindProperty("durianImage").objectReferenceValue = durianImage;
        opSo.FindProperty("guideText").objectReferenceValue = guideText;
        opSo.FindProperty("estimateText").objectReferenceValue = estimateText;
        opSo.FindProperty("sellButton").objectReferenceValue = sellButton;
        opSo.FindProperty("reviveButton").objectReferenceValue = reviveButton;
        opSo.FindProperty("backButton").objectReferenceValue = backButton;
        opSo.ApplyModifiedPropertiesWithoutUndo();

        return page;
    }

    private static GameObject BuildSellPage(Transform parent)
    {
        var page = CreatePanel(parent, "SellPage", fullStretch: true);
        page.AddComponent<SellPage>();
        var sp = page.GetComponent<SellPage>();

        var summary = CreateText(page.transform, "Summary", "出肉率 --", 28, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.8f));
        var price = CreateText(page.transform, "Price", "固定回收价 --", 26, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f));
        var goldAnim = CreateText(page.transform, "GoldAnim", "", 32, TextAnchor.MiddleCenter,
            new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.5f));
        var goldCanvasGroup = goldAnim.gameObject.AddComponent<CanvasGroup>();
        goldCanvasGroup.alpha = 0f;
        var adBtn = CreateButton(page.transform, "AdBonus", "看广告加价+20%",
            new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.35f), new Color(0.2f, 0.5f, 0.8f));
        var confirmBtn = CreateButton(page.transform, "Confirm", "确认卖出",
            new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.22f), new Color(0.85f, 0.55f, 0.1f));
        var backBtn = CreateButton(page.transform, "Back", "返回",
            new Vector2(0.02f, 0.92f), new Vector2(0.18f, 0.98f), new Color(0.35f, 0.35f, 0.4f));

        var so = new SerializedObject(sp);
        so.FindProperty("summaryText").objectReferenceValue = summary;
        so.FindProperty("priceText").objectReferenceValue = price;
        so.FindProperty("goldText").objectReferenceValue = goldAnim;
        so.FindProperty("goldCanvasGroup").objectReferenceValue = goldCanvasGroup;
        so.FindProperty("adBonusButton").objectReferenceValue = adBtn;
        so.FindProperty("confirmButton").objectReferenceValue = confirmBtn;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
        return page;
    }

    private static GameObject BuildBagPage(Transform parent, GameObject cardPrefab)
    {
        var page = CreatePanel(parent, "BagPage", fullStretch: true);
        page.AddComponent<BagPage>();
        var bp = page.GetComponent<BagPage>();

        var capacity = CreateText(page.transform, "Capacity", "0/10", 26, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.9f), new Vector2(0.4f, 0.98f));
        var cardRoot = CreatePanel(page.transform, "CardRoot", false,
            new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
        var grid = cardRoot.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(140, 160);
        grid.spacing = new Vector2(12, 12);

        var emptyHint = CreateText(page.transform, "EmptyHint", "去市场买榴莲吧", 28, TextAnchor.MiddleCenter,
            new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.55f));
        var goMarket = CreateButton(page.transform, "GoMarket", "去市场",
            new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.4f), new Color(0.3f, 0.65f, 0.35f));
        var backBtn = CreateButton(page.transform, "Back", "返回",
            new Vector2(0.02f, 0.92f), new Vector2(0.18f, 0.98f), new Color(0.35f, 0.35f, 0.4f));

        var so = new SerializedObject(bp);
        so.FindProperty("capacityText").objectReferenceValue = capacity;
        so.FindProperty("cardRoot").objectReferenceValue = cardRoot.transform;
        so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
        so.FindProperty("emptyHint").objectReferenceValue = emptyHint.gameObject;
        so.FindProperty("goMarketButton").objectReferenceValue = goMarket;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
        return page;
    }

    private static GameObject BuildShopPage(Transform parent)
    {
        var page = CreatePanel(parent, "ShopPage", fullStretch: true);
        page.AddComponent<ShopPage>();
        var sp = page.GetComponent<ShopPage>();

        var level = CreateText(page.transform, "Level", "商店 Lv.1", 30, TextAnchor.MiddleCenter,
            new Vector2(0.2f, 0.7f), new Vector2(0.8f, 0.8f));
        var effect = CreateText(page.transform, "Effect", "升级到 Lv.2 后回收价 +20%", 22, TextAnchor.MiddleCenter,
            new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.65f));
        var upgrade = CreateButton(page.transform, "Upgrade", "升级到 Lv.2（500金币）",
            new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.48f), new Color(0.55f, 0.3f, 0.7f));
        var backBtn = CreateButton(page.transform, "Back", "返回",
            new Vector2(0.02f, 0.92f), new Vector2(0.18f, 0.98f), new Color(0.35f, 0.35f, 0.4f));

        var so = new SerializedObject(sp);
        so.FindProperty("levelText").objectReferenceValue = level;
        so.FindProperty("effectText").objectReferenceValue = effect;
        so.FindProperty("upgradeButton").objectReferenceValue = upgrade;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
        return page;
    }

    private static void WireGameLifetimeScope(
        GameObject gameRoot,
        GameUIRoot uiRoot,
        GameObject market,
        GameObject open,
        GameObject sell,
        GameObject bag,
        GameObject shop)
    {
        var scope = gameRoot.GetComponent<GameLifetimeScope>();
        var so = new SerializedObject(scope);
        so.FindProperty("uiRoot").objectReferenceValue = uiRoot;
        so.FindProperty("marketPage").objectReferenceValue = market.GetComponent<MarketPage>();
        so.FindProperty("openPage").objectReferenceValue = open.GetComponent<OpenPage>();
        so.FindProperty("sellPage").objectReferenceValue = sell.GetComponent<SellPage>();
        so.FindProperty("bagPage").objectReferenceValue = bag.GetComponent<BagPage>();
        so.FindProperty("shopPage").objectReferenceValue = shop.GetComponent<ShopPage>();

        CreateDurianSpriteConfigAsset();
        var spriteConfig = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(DurianSpriteConfigPath);
        if (spriteConfig != null)
        {
            so.FindProperty("durianSpriteConfig").objectReferenceValue = spriteConfig;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireUIRoot(
        GameUIRoot uiRoot,
        GameObject market,
        GameObject open,
        GameObject sell,
        GameObject bag,
        GameObject shop,
        Image marketBgImage,
        Image openBgImage)
    {
        var so = new SerializedObject(uiRoot);
        so.FindProperty("marketBgImage").objectReferenceValue = marketBgImage;
        so.FindProperty("openBgImage").objectReferenceValue = openBgImage;
        so.FindProperty("marketPage").objectReferenceValue = market.GetComponent<MarketPage>();
        so.FindProperty("openPage").objectReferenceValue = open.GetComponent<OpenPage>();
        so.FindProperty("sellPage").objectReferenceValue = sell.GetComponent<SellPage>();
        so.FindProperty("bagPage").objectReferenceValue = bag.GetComponent<BagPage>();
        so.FindProperty("shopPage").objectReferenceValue = shop.GetComponent<ShopPage>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static (Image marketBgImage, Image openBgImage) BuildBackgroundImages(Transform parent)
    {
        var marketBgImage = CreateImage(parent, "MarketBgImage", Color.white, Vector2.zero, Vector2.one);
        var openBgImage = CreateImage(parent, "OpenBgImage", Color.white, Vector2.zero, Vector2.one);

        marketBgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MarketBgSpritePath);
        openBgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OpenBgSpritePath);

        marketBgImage.preserveAspect = false;
        openBgImage.preserveAspect = false;
        marketBgImage.enabled = true;
        openBgImage.enabled = false;

        marketBgImage.transform.SetSiblingIndex(0);
        openBgImage.transform.SetSiblingIndex(1);
        return (marketBgImage, openBgImage);
    }

    private static GameObject CreateCanvas()
    {
        var canvasGo = new GameObject("MVP_Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 10f;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasGo.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        return canvasGo;
    }

    private static GameObject CreatePanel(Transform parent, string name, bool fullStretch,
        Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);
        var rect = go.GetComponent<RectTransform>();
        if (fullStretch)
        {
            Stretch(rect, 0, 0, 0, 0);
        }
        else
        {
            rect.anchorMin = anchorMin ?? Vector2.zero;
            rect.anchorMax = anchorMax ?? Vector2.one;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }

        return go;
    }

    private static Text CreateText(Transform parent, string name, string text, int fontSize,
        TextAnchor align, Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var uiText = go.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.alignment = align;
        uiText.color = Color.white;
        uiText.font = GetDefaultFont();
        uiText.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        if (anchorMin.HasValue && anchorMax.HasValue)
        {
            rect.anchorMin = anchorMin.Value;
            rect.anchorMax = anchorMax.Value;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            Stretch(rect, 0, 0, 0, 0);
        }

        return uiText;
    }

    private static Font GetDefaultFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>(DefaultFontPath);
        if (font == null)
        {
            Debug.LogError($"[MvpSceneSetup] 未找到字体：{DefaultFontPath}，请确认资源存在。");
        }

        return font;
    }

    private static Image CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var btn = go.AddComponent<Button>();
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        CreateText(go.transform, "Label", label, 22, TextAnchor.MiddleCenter);
        return btn;
    }

    private static void ApplyRoomPrefabSprites(
        GameObject meatPrefab,
        GameObject emptyPrefab,
        DurianSpriteConfig config)
    {
        if (config == null)
        {
            return;
        }

        ApplySpriteToPrefab(meatPrefab, config.fleshPiece);
        ApplySpriteToPrefab(emptyPrefab, config.emptyPiece);
    }

    private static void ApplySpriteToPrefab(GameObject prefab, Sprite sprite)
    {
        if (prefab == null || sprite == null)
        {
            return;
        }

        var path = AssetDatabase.GetAssetPath(prefab);
        var root = PrefabUtility.LoadPrefabContents(path);
        var image = root.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static GameObject CreateBlockPrefab(string name, Color color, Vector2 size)
    {
        var path = $"{PrefabDir}/{name}.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            return existing;
        }

        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateFloatTextPrefab()
    {
        var path = $"{PrefabDir}/FloatText.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        var go = new GameObject("FloatText");
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 40);
        var uiText = go.AddComponent<Text>();
        uiText.font = GetDefaultFont();
        uiText.fontSize = 20;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        go.AddComponent<CanvasGroup>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateBagCardPrefab()
    {
        var path = $"{PrefabDir}/BagCard.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        var go = new GameObject("BagCard");
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140, 160);
        go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.24f);
        go.AddComponent<Button>();
        var block = new GameObject("Block");
        block.transform.SetParent(go.transform, false);
        var blockRect = block.AddComponent<RectTransform>();
        blockRect.anchorMin = new Vector2(0.1f, 0.25f);
        blockRect.anchorMax = new Vector2(0.9f, 0.9f);
        blockRect.offsetMin = Vector2.zero;
        blockRect.offsetMax = Vector2.zero;
        block.AddComponent<Image>().color = new Color(0.3f, 0.65f, 0.35f);
        CreateText(go.transform, "VarietyText", "金枕", 20, TextAnchor.MiddleCenter,
            new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.72f));
        CreateText(go.transform, "InfoText", "普通 · 购价 50 · 出肉约 40%", 14, TextAnchor.MiddleCenter,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.48f));
        go.SetActive(false);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void SetImageColor(GameObject go, Color color)
    {
        var image = go.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void WireArray<T>(SerializedObject so, string propertyName, T[] values) where T : Object
    {
        var prop = so.FindProperty(propertyName);
        prop.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
