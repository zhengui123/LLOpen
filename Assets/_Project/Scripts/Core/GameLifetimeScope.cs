using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private VarietyConfig[] varietyConfigs;
    [SerializeField] private AppearanceConfig[] appearanceConfigs;
    [SerializeField] private ShopConfig shopConfig;
    [SerializeField] private DurianSpriteConfig durianSpriteConfig;
    [SerializeField] private GameUIRoot uiRoot;
    [SerializeField] private MarketPage marketPage;
    [SerializeField] private OpenPage openPage;
    [SerializeField] private SellPage sellPage;
    [SerializeField] private BagPage bagPage;
    [SerializeField] private ShopPage shopPage;

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
        builder.Register<EventBus>(Lifetime.Singleton);
        builder.Register<AppearanceProbabilitySystem>(Lifetime.Singleton);

        builder.RegisterInstance(varietyConfigs);
        builder.RegisterInstance(appearanceConfigs);
        builder.RegisterInstance(shopConfig);

        if (durianSpriteConfig != null)
        {
            builder.RegisterInstance(durianSpriteConfig);
        }

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
    }
}
