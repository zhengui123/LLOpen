using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private VarietyConfig[] varietyConfigs;
    [SerializeField] private AppearanceConfig[] appearanceConfigs;
    [SerializeField] private ShopConfig shopConfig;
    [SerializeField] private DurianSpriteConfig durianSpriteConfig;
    [SerializeField] private DurianRoomConfig jinzhengRoomConfig;
    [SerializeField] private GameUIRoot uiRoot;
    [SerializeField] private MarketPage marketPage;
    [SerializeField] private OpenPage openPage;
    [SerializeField] private SellPage sellPage;
    [SerializeField] private BagPage bagPage;
    [SerializeField] private ShopPage shopPage;
    [SerializeField] private CollectionPage collectionPage;

    protected override void Awake()
    {
        autoRun = false;
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        EnsureConfigsLoaded();
        EnsureUIReferences();

        var economyConfig = GameEconomyConfig.LoadDefault();
        PlayerData.Instance.Gold = economyConfig.InitialGold;

        builder.RegisterInstance(economyConfig);
        builder.RegisterInstance(PlayerProgression.Instance);
        builder.Register<EventBus>(Lifetime.Singleton);
        builder.Register<AppearanceProbabilitySystem>(Lifetime.Singleton);

        builder.RegisterInstance(varietyConfigs);
        builder.RegisterInstance(appearanceConfigs);
        builder.RegisterInstance(shopConfig);

        if (durianSpriteConfig != null)
        {
            builder.RegisterInstance(durianSpriteConfig);
        }

        if (jinzhengRoomConfig != null)
        {
            builder.RegisterInstance(jinzhengRoomConfig);
        }

        builder.Register<StreakCounter>(Lifetime.Singleton);
        builder.Register<DailyTarget>(Lifetime.Singleton);
        builder.Register<DurianGeneratorSystem>(Lifetime.Singleton);
        builder.Register<MarketManager>(Lifetime.Singleton);
        builder.Register<BagManager>(Lifetime.Singleton);
        builder.Register<SellManager>(Lifetime.Singleton);
        builder.Register<AdManager>(Lifetime.Singleton);
        builder.Register<ShopManager>(Lifetime.Singleton);

        RegisterPage(builder, uiRoot);
        RegisterPage(builder, marketPage);
        RegisterPage(builder, openPage);
        RegisterPage(builder, sellPage);
        RegisterPage(builder, bagPage);
        RegisterPage(builder, shopPage);
        RegisterPage(builder, collectionPage);

        if (uiRoot != null)
        {
            var shell = uiRoot.GetComponentInChildren<MainShellUI>(true);
            if (shell != null)
            {
                builder.RegisterComponent(shell);
            }
        }

        if (openPage != null)
        {
            var opener = openPage.GetComponentInChildren<DurianOpener>(true);
            if (opener != null)
            {
                builder.RegisterComponent(opener);
            }
        }

        // 强制创建 EventBus 单例，否则静态 Publish/Subscribe 时 _instance 为空
        builder.RegisterBuildCallback(resolver => resolver.Resolve<EventBus>());
    }

    private static void RegisterPage<T>(IContainerBuilder builder, T page) where T : Component
    {
        if (page != null)
        {
            builder.RegisterComponent(page);
        }
    }

    private void EnsureUIReferences()
    {
        if (uiRoot == null)
        {
            uiRoot = FindObjectOfType<GameUIRoot>(true);
        }

        if (marketPage == null)
        {
            marketPage = FindObjectOfType<MarketPage>(true);
        }

        if (openPage == null)
        {
            openPage = FindObjectOfType<OpenPage>(true);
        }

        if (sellPage == null)
        {
            sellPage = FindObjectOfType<SellPage>(true);
        }

        if (bagPage == null)
        {
            bagPage = FindObjectOfType<BagPage>(true);
        }

        if (shopPage == null)
        {
            shopPage = FindObjectOfType<ShopPage>(true);
        }

        if (collectionPage == null)
        {
            collectionPage = FindObjectOfType<CollectionPage>(true);
        }
    }

    private void EnsureConfigsLoaded()
    {
        if (varietyConfigs == null || varietyConfigs.Length == 0)
        {
            varietyConfigs = Resources.LoadAll<VarietyConfig>("Variety");
        }

        if (appearanceConfigs == null || appearanceConfigs.Length == 0)
        {
            appearanceConfigs = Resources.LoadAll<AppearanceConfig>("Appearance");
        }

        if (shopConfig == null)
        {
            shopConfig = Resources.Load<ShopConfig>("ShopConfig");
        }

        if (shopConfig == null)
        {
            shopConfig = ScriptableObject.CreateInstance<ShopConfig>();
        }

        if (jinzhengRoomConfig == null)
        {
            jinzhengRoomConfig = Resources.Load<DurianRoomConfig>("JinzhengRoomConfig");
        }
    }
}
