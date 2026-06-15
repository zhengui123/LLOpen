using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private VarietyConfig[] varietyConfigs;
    [SerializeField] private AppearanceConfig[] appearanceConfigs;
    [SerializeField] private ShopConfig shopConfig;

    protected override void Awake()
    {
        autoRun = false;
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        EnsureConfigsLoaded();

        builder.Register<EventBus>(Lifetime.Singleton);
        builder.Register<AppearanceProbabilitySystem>(Lifetime.Singleton);

        builder.RegisterInstance(varietyConfigs);
        builder.RegisterInstance(appearanceConfigs);
        builder.RegisterInstance(shopConfig);

        builder.Register<DurianGeneratorSystem>(Lifetime.Singleton);
        builder.Register<MarketManager>(Lifetime.Singleton);
        builder.Register<BagManager>(Lifetime.Singleton);
        builder.Register<SellManager>(Lifetime.Singleton);
        builder.Register<AdManager>(Lifetime.Singleton);
        builder.Register<ShopManager>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<GameUIRoot>();
        builder.RegisterComponentInHierarchy<MarketPage>();
        builder.RegisterComponentInHierarchy<OpenPage>();
        builder.RegisterComponentInHierarchy<SellPage>();
        builder.RegisterComponentInHierarchy<BagPage>();
        builder.RegisterComponentInHierarchy<ShopPage>();
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
