# 《llopen MVP》- Unity项目架构文档

> 版本：MVP v1.0 | 日期：2026-06-15
> 原则：**只保留MVP必需的系统，砍掉加工、顾客竞价、Spine**
> 参考：完整版 Architecture.md（3156行）

---

## 1. 技术栈（MVP不变）

| 技术 | 版本 | MVP状态 |
|------|------|---------|
| Unity | 2022.3 LTS | ✅ 使用 |
| 渲染管线 | 2D URP | ✅ 使用 |
| DI容器 | VContainer 1.16.8 | ✅ 使用 |
| 异步编程 | UniTask 2.5.10 | ✅ 使用 |
| 事件总线 | 自研 EventBus | ✅ 使用 |
| UI动画 | DOTween Pro | ✅ 使用 |
| 骨骼动画 | Spine | ❌ MVP砍掉 |
| 资源管理 | Addressables | ✅ 使用 |
| JSON | Newtonsoft.Json | ✅ 使用 |
| 响应式 | UniRx 7.1.0 | ✅ 使用 |

---

## 2. 架构分层（MVP简化版）

```
┌─────────────────────────────────────┐
│           View Layer (MVP)          │
│  MarketPage  OpenPage  SellPage     │
│  BagPage  ShopPage                  │
└──────────────┬──────────────────────┘
               │ EventBus
┌──────────────┴──────────────────────┐
│         Manager Layer (MVP)         │
│  MarketManager  BagManager          │
│  SellManager    AdManager           │
│  ShopManager                        │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│         System Layer (MVP)          │
│  DurianGeneratorSystem              │
│  AppearanceProbabilitySystem        │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│         Model Layer                 │
│  DurianData  VarietyData            │
│  AppearanceType  ShopData           │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│         Core Layer                  │
│  EventBus  VContainer  UniTask      │
│  Addressables  DOTween              │
└─────────────────────────────────────┘
```

**对比完整版，MVP砍掉的层**：
- ❌ ProcessManager（加工管理器）
- ❌ CustomerManager（顾客竞价管理器）
- ❌ ProcessingSystem（加工逻辑系统）
- ❌ CustomerSystem（顾客逻辑系统）
- ❌ Spine 骨骼动画系统

---

## 3. 项目目录结构（MVP）

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   ├── MainScene.unity          # 主场景
│   │   └── LoadingScene.unity       # 加载场景
│   │
│   ├── Scripts/
│   │   ├── Core/                     # 核心基础设施
│   │   │   ├── EventBus.cs           # 事件总线
│   │   │   ├── GameBootstrapper.cs   # VContainer启动器
│   │   │   └── GameLifetimeScope.cs  # VContainer生命周期
│   │   │
│   │   ├── Model/                    # 数据模型
│   │   │   ├── DurianData.cs
│   │   │   ├── VarietyData.cs
│   │   │   ├── ShopData.cs
│   │   │   └── Enums.cs             # AppearanceType/VarietyType
│   │   │
│   │   ├── System/                   # 逻辑系统
│   │   │   ├── DurianGeneratorSystem.cs
│   │   │   └── AppearanceProbabilitySystem.cs
│   │   │
│   │   ├── Manager/                  # 业务管理器
│   │   │   ├── MarketManager.cs      # 市场管理
│   │   │   ├── BagManager.cs         # 背包管理
│   │   │   ├── SellManager.cs        # 售卖管理（固定价）
│   │   │   ├── AdManager.cs          # 广告管理
│   │   │   └── ShopManager.cs        # 商店管理
│   │   │
│   │   ├── View/                     # UI视图
│   │   │   ├── MarketPage.cs
│   │   │   ├── OpenPage.cs
│   │   │   ├── SellPage.cs
│   │   │   ├── BagPage.cs
│   │   │   └── ShopPage.cs
│   │   │
│   │   └── Component/                # 核心交互组件
│   │       ├── KnifeTool.cs          # 划刀工具
│   │       └── DurianOpener.cs       # 榴莲揭示
│   │
│   ├── Data/                         # 配置数据
│   │   ├── DurianConfig.asset        # 榴莲品种配置
│   │   ├── AppearanceConfig.asset    # 外观偏移配置
│   │   └── ShopConfig.asset          # 商店升级配置
│   │
│   ├── Resources/                    # Resources中的Addressables引用
│   │   └── AddressableAssetsData/
│   │
│   └── Art/                          # 美术资源
│       ├── Scenes/                   # 场景背景
│       ├── Durians/                  # 榴莲贴图
│       ├── UI/                       # UI资源
│       └── Effects/                  # 特效
│
├── Plugins/
│   └── WeChatSDK/                    # 微信小游戏SDK
│
└── AddressableAssetsData/            # Addressables配置
```

---

## 4. 核心数据模型

### 4.1 Enums.cs

```csharp
// 榴莲品种（MVP仅3种）
public enum VarietyType
{
    JinZheng = 0,   // 金枕
    GanYao = 1,     // 干尧
    MaoShanWang = 2 // 猫山王
}

// 外观品质
public enum AppearanceType
{
    Poor = 0,    // 劣质外表
    Normal = 1,  // 普通外表
    Good = 2,    // 优质外表
    Premium = 3  // 极品外表
}

// 出肉率档位（MVP简化，去掉"榴莲之王"）
public enum YieldGrade
{
    Empty = 0,  // 空壳 0-15%
    Low = 1,    // 少肉 15-30%
    Normal = 2, // 正常 30-45%
    High = 3,   // 满肉 45-60%
    Perfect = 4 // 爆肉 60%+
}
```

### 4.2 DurianData.cs

```csharp
[System.Serializable]
public struct DurianData
{
    public int id;                          // 唯一ID
    public VarietyType variety;             // 品种
    public AppearanceType appearance;       // 外观等级
    public float appearancePriceMultiplier; // 外观价格系数（0.8/1.0/1.5/2.2）
    public int basePrice;                   // 品种基础价格
    public int finalPrice;                  // 最终标价（basePrice × 系数）
    public float yieldRate;                 // 出肉率（0-100%）
    public int roomCount;                   // 房数（3-7）
    public bool[] roomResults;              // 每房是否有肉
    public YieldGrade yieldGrade;           // 出肉率档位
}
```

### 4.3 VarietyData.cs（ScriptableObject）

```csharp
[CreateAssetMenu(fileName = "DurianConfig", menuName = "llopen/品种配置")]
public class VarietyConfig : ScriptableObject
{
    public VarietyType type;
    public string varietyName;          // 金枕/干尧/猫山王
    public int basePrice;               // 基础价格
    public float baseYieldRate;         // 基础出肉率（用于计算波动）
    public int minRooms;                // 最小房数
    public int maxRooms;                // 最大房数
    public float[] baseWeights;         // 5档基础权重 [空壳,少肉,正常,满肉,爆肉]
}
```

### 4.4 外观配置（ScriptableObject）

```csharp
[CreateAssetMenu(fileName = "AppearanceConfig", menuName = "llopen/外观配置")]
public class AppearanceConfig : ScriptableObject
{
    public AppearanceType type;
    public string appearanceName;       // 劣质外表/普通外表/...
    public float priceMultiplier;       // 价格系数（0.8/1.0/1.5/2.2）
    public float spawnWeight;           // 出现权重（用于随机出现概率）
    public float[] probabilityOffsets;  // 5档概率偏移 [空壳,少肉,正常,满肉,爆肉]
}
```

---

## 5. 核心系统设计

### 5.1 AppearanceProbabilitySystem（外观概率叠加系统）

```csharp
public class AppearanceProbabilitySystem
{
    // 计算最终概率分布（权重偏移 + 归一化）
    public float[] CalculateFinalProbabilities(
        float[] baseWeights,           // 品种基础权重 [5档]
        float[] appearanceOffsets)     // 外观偏移权重 [5档]
    {
        float[] finalWeights = new float[5];
        
        // 权重叠加
        for (int i = 0; i < 5; i++)
        {
            finalWeights[i] = Mathf.Max(0, baseWeights[i] + appearanceOffsets[i]);
        }
        
        // 归一化
        float total = 0f;
        for (int i = 0; i < 5; i++) total += finalWeights[i];
        if (total == 0) return baseWeights; // 防御
        
        for (int i = 0; i < 5; i++)
        {
            finalWeights[i] = finalWeights[i] / total * 100f;
        }
        
        return finalWeights;
    }
    
    // 按概率分布抽取结果
    public YieldGrade SampleYieldGrade(float[] probabilities)
    {
        float random = Random.Range(0f, 100f);
        float cumulative = 0f;
        
        for (int i = 0; i < 5; i++)
        {
            cumulative += probabilities[i];
            if (random <= cumulative) return (YieldGrade)i;
        }
        
        return YieldGrade.Normal; // fallback
    }
}
```

### 5.2 DurianGeneratorSystem（榴莲生成系统）

```csharp
public class DurianGeneratorSystem
{
    private AppearanceProbabilitySystem _probabilitySystem;
    private VarietyConfig[] _varietyConfigs;
    private AppearanceConfig[] _appearanceConfigs;
    
    // 为市场生成3颗同品种榴莲
    public DurianData[] GenerateMarketDurians(VarietyType variety)
    {
        DurianData[] durians = new DurianData[3];
        
        for (int i = 0; i < 3; i++)
        {
            // 1. 随机外观等级
            AppearanceType appearance = RandomAppearance();
            
            // 2. 计算最终概率
            float[] probs = _probabilitySystem.CalculateFinalProbabilities(
                GetBaseWeights(variety),
                GetAppearanceOffsets(appearance)
            );
            
            // 3. 抽取出肉率档位
            YieldGrade grade = _probabilitySystem.SampleYieldGrade(probs);
            
            // 4. 在档位区间内生成具体出肉率
            float yieldRate = GenerateYieldRate(variety, grade);
            
            // 5. 生成房数和每房结果
            int roomCount = RandomRoomCount(variety);
            bool[] rooms = GenerateRoomResults(roomCount, yieldRate);
            
            durians[i] = new DurianData
            {
                id = GenerateId(),
                variety = variety,
                appearance = appearance,
                appearancePriceMultiplier = GetPriceMultiplier(appearance),
                basePrice = GetBasePrice(variety),
                finalPrice = Mathf.RoundToInt(GetBasePrice(variety) * GetPriceMultiplier(appearance)),
                yieldRate = yieldRate,
                roomCount = roomCount,
                roomResults = rooms,
                yieldGrade = grade
            };
        }
        
        return durians;
    }
}
```

---

## 6. Manager层设计

### 6.1 MarketManager

```csharp
public class MarketManager
{
    private DurianGeneratorSystem _generator;
    
    public DurianData[] CurrentMarketDurians { get; private set; }
    
    // 刷新市场（选择品种后生成3颗榴莲）
    public void RefreshMarket(VarietyType variety)
    {
        CurrentMarketDurians = _generator.GenerateMarketDurians(variety);
        EventBus.Publish(new MarketRefreshedEvent(CurrentMarketDurians));
    }
    
    // 玩家购买某颗榴莲
    public void BuyDurian(int durianIndex)
    {
        if (durianIndex < 0 || durianIndex >= CurrentMarketDurians.Length) return;
        
        DurianData purchased = CurrentMarketDurians[durianIndex];
        int cost = purchased.finalPrice;
        
        if (PlayerData.Instance.Gold >= cost)
        {
            PlayerData.Instance.Gold -= cost;
            EventBus.Publish(new DurianPurchasedEvent(purchased));
        }
    }
}
```

### 6.2 BagManager（简化版）

```csharp
public class BagManager
{
    public List<DurianData> Durians { get; private set; } = new();
    public int MaxCapacity { get; private set; } = 10;
    
    public void AddDurian(DurianData durian)
    {
        if (Durians.Count >= MaxCapacity)
        {
            EventBus.Publish(new BagFullEvent());
            return;
        }
        Durians.Add(durian);
        EventBus.Publish(new BagUpdatedEvent(Durians));
    }
    
    public DurianData RemoveDurian(int index)
    {
        var durian = Durians[index];
        Durians.RemoveAt(index);
        EventBus.Publish(new BagUpdatedEvent(Durians));
        return durian;
    }
}
```

### 6.3 SellManager（MVP简化为固定价）

```csharp
public class SellManager
{
    private ShopManager _shopManager;
    
    // MVP版：固定价格回收
    public int CalculateSellPrice(DurianData durian)
    {
        float basePrice = GetYieldGradePrice(durian.yieldGrade);
        float shopBonus = _shopManager.GetSellBonus(); // 商店升级加成
        return Mathf.RoundToInt(basePrice * (1f + shopBonus));
    }
    
    private float GetYieldGradePrice(YieldGrade grade) => grade switch
    {
        YieldGrade.Empty => 0f,
        YieldGrade.Low => 10f,
        YieldGrade.Normal => 50f,
        YieldGrade.High => 100f,
        YieldGrade.Perfect => 250f,
        _ => 0f
    };
    
    // 卖榴莲
    public void SellDurian(DurianData durian)
    {
        int price = CalculateSellPrice(durian);
        PlayerData.Instance.Gold += price;
        EventBus.Publish(new DurianSoldEvent(durian, price));
    }
}
```

### 6.4 AdManager

```csharp
public class AdManager
{
    private int _dailyAdCount = 0;
    private int _maxDailyAds = 10; // MVP上限10次
    
    private Dictionary<string, int> _adLimits = new()
    {
        {"customer_bonus", 5},  // 顾客加价每日5次
        {"revive", 3},          // 复活每日3次
        {"daily_buff", 1},      // 每日Buff 1次
        {"free_smell", -1},     // 免费试闻不限
    };
    
    public bool CanShowAd(string adType)
    {
        if (_dailyAdCount >= _maxDailyAds) return false;
        if (_adLimits.TryGetValue(adType, out int limit) && limit != -1)
        {
            // 检查单类上限
            return GetTypeCount(adType) < limit;
        }
        return true;
    }
    
    public async UniTask<bool> ShowRewardedAd(string adType)
    {
        if (!CanShowAd(adType)) return false;
        
        // 调用微信激励视频SDK
        bool success = await WeChatAdSDK.ShowRewardedVideo(adType);
        
        if (success)
        {
            _dailyAdCount++;
            IncrementTypeCount(adType);
            GrantReward(adType);
        }
        
        return success;
    }
    
    private void GrantReward(string adType)
    {
        switch (adType)
        {
            case "customer_bonus":
                EventBus.Publish(new AdRewardEvent(AdRewardType.SellBonus, 0.2f));
                break;
            case "revive":
                EventBus.Publish(new AdRewardEvent(AdRewardType.Revive, 0));
                break;
            case "daily_buff":
                EventBus.Publish(new AdRewardEvent(AdRewardType.DailyBuff, 
                    Random.Range(0f, 0.05f))); // 出肉率+0-5%
                break;
            case "free_smell":
                EventBus.Publish(new AdRewardEvent(AdRewardType.Clue, 0));
                break;
        }
    }
}
```

### 6.5 ShopManager（MVP仅2级）

```csharp
public class ShopManager
{
    public int CurrentLevel { get; private set; } = 1;
    public int MaxLevel => 2;
    
    private int[] _upgradeCosts = { 0, 500 };
    private float[] _sellBonus = { 0f, 0.2f }; // Lv1 0%, Lv2 +20%
    
    public bool CanUpgrade()
    {
        return CurrentLevel < MaxLevel 
            && PlayerData.Instance.Gold >= _upgradeCosts[CurrentLevel];
    }
    
    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        
        PlayerData.Instance.Gold -= _upgradeCosts[CurrentLevel];
        CurrentLevel++;
        EventBus.Publish(new ShopUpgradedEvent(CurrentLevel));
    }
    
    public float GetSellBonus()
    {
        return _sellBonus[CurrentLevel - 1];
    }
}
```

---

## 7. EventBus 事件定义（MVP）

```csharp
// 市场相关
public struct MarketRefreshedEvent { public DurianData[] Durians; }
public struct DurianPurchasedEvent { public DurianData Durian; }

// 背包相关
public struct BagUpdatedEvent { public List<DurianData> Durians; }
public struct BagFullEvent { }

// 售卖相关
public struct DurianSoldEvent { public DurianData Durian; public int Price; }

// 商店相关
public struct ShopUpgradedEvent { public int NewLevel; }

// 广告相关
public struct AdRewardEvent 
{ 
    public AdRewardType Type; 
    public float Value; 
}

public enum AdRewardType
{
    SellBonus,   // 售卖加成
    Revive,      // 复活
    DailyBuff,   // 每日Buff
    Clue         // 试闻线索
}
```

---

## 8. VContainer 依赖注入配置

```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Core
        builder.Register<EventBus>(Lifetime.Singleton);
        
        // Systems
        builder.Register<AppearanceProbabilitySystem>(Lifetime.Singleton);
        builder.Register<DurianGeneratorSystem>(Lifetime.Singleton);
        
        // Managers
        builder.Register<MarketManager>(Lifetime.Singleton);
        builder.Register<BagManager>(Lifetime.Singleton);
        builder.Register<SellManager>(Lifetime.Singleton);
        builder.Register<AdManager>(Lifetime.Singleton);
        builder.Register<ShopManager>(Lifetime.Singleton);
        
        // Views (Transient - 每次展示创建新实例)
        builder.Register<MarketPage>(Lifetime.Transient);
        builder.Register<OpenPage>(Lifetime.Transient);
        builder.Register<SellPage>(Lifetime.Transient);
        builder.Register<BagPage>(Lifetime.Transient);
        builder.Register<ShopPage>(Lifetime.Transient);
        
        // Components
        builder.Register<KnifeTool>(Lifetime.Transient);
        builder.Register<DurianOpener>(Lifetime.Transient);
    }
}
```

---

## 9. 性能方案（MVP简化版）

### 9.1 贴图管理

| 策略 | MVP实现 |
|------|---------|
| 贴图总数 | ~25张 |
| 场景背景 | 2张（JPG，1080×1920）|
| 榴莲贴图 | 18张（PNG，512×512）|
| UI图标 | 5张（PNG，256×256）|
| 打包方式 | 全部放入Resources，不做分包（MVP量小）|
| 加载方式 | Resources.Load（MVP不需要Addressables）|

> MVP贴图仅25张，不需要Texture2DArray和Addressables分包。完整版110张时才需要。

### 9.2 内存预算

| 项目 | MVP预算 |
|------|---------|
| 首包大小 | <8MB（25张JPG贴图仅~3MB）|
| 运行时内存 | <50MB |
| 首屏加载 | <1秒（Resources.Load同步加载）|

---

## 10. MVP架构总结

| 维度 | 完整版 | MVP版 | 简化原因 |
|------|---------|--------|---------|
| Manager数量 | 6个 | 5个 | 砍掉ProcessManager |
| System数量 | 4个 | 2个 | 砍掉ProcessingSystem、CustomerSystem |
| UI页面 | 6个 | 5个 | 砍掉ProcessingPage |
| 事件类型 | 12+ | 6个 | 仅核心事件 |
| ScriptableObject | 8+ | 3个 | 仅品种/外观/商店配置 |
| 贴图加载 | Addressables | Resources.Load | MVP贴图少 |

---

**文档版本**：MVP v1.0 | 2026-06-15
**下一步**：阅读 Cursor_Prompts_MVP.md → 开始Phase 0开发
