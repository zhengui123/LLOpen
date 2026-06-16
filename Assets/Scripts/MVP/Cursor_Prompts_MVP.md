# 《llopen MVP》- Cursor 开发流程提示词

> 版本：MVP v1.0 | 日期：2026-06-15
> 对比完整版：Phase 0-8（43步）→ MVP仅Phase 0-5 + 广告（~20步）
> 技术栈：Unity 2022.3 LTS + 2D URP | VContainer + EventBus + UniTask + UniRx + DOTween
> 使用方式：将每个步骤的Cursor提示词复制粘贴到Cursor AI编辑器

---

# Phase 0: 项目初始化（MVP）

## Step 0.1: 创建Unity项目与基础配置

### Cursor提示词
```
请帮我完成Unity 2022.3 LTS项目的基础配置：

1. 项目使用2D URP渲染管线，确保URP Asset已创建并设为默认

2. Player Settings：
   - Company Name: DurianGame
   - Product Name: llopenMVP
   - Scripting Backend: IL2CPP
   - API Compatibility Level: .NET Standard 2.1
   - Color Space: Linear
   - Target Frame Rate: 60

3. 安装以下UPM包（修改Packages/manifest.json）：
   - VContainer: https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.8
   - UniTask: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10
   - UniRx: https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx#7.1.0
   - DOTween: https://github.com/Demigiant/dotween.git#1.2.765
   - Newtonsoft.Json: com.unity.nuget.newtonsoft-json
   - Addressables: com.unity.addressables

4. MVP不安装：Spine（砍掉）

5. 目录结构：
   Assets/_Project/Scripts/{Core,Model,System,Manager,View,Component}/
   Assets/_Project/Data/
   Assets/_Project/Art/{Scenes,Durians,UI,Effects}/
```

---

## Step 0.2: 创建GameBootstrapper和GameLifetimeScope

### Cursor提示词
```
请在 Assets/_Project/Scripts/Core/ 下创建以下文件：

1. GameBootstrapper.cs
   - MonoBehaviour，场景中挂载
   - Awake时调用VContainer启动
   - 使用 Find<GameLifetimeScope>().Build()

2. GameLifetimeScope.cs
   - 继承 VContainer.Unity.LifetimeScope
   - Configure方法中注册所有服务（见Architecture_MVP.md第8节）
   - MVP注册清单：
     - EventBus (Singleton)
     - AppearanceProbabilitySystem (Singleton)
     - DurianGeneratorSystem (Singleton)
     - MarketManager (Singleton)
     - BagManager (Singleton)
     - SellManager (Singleton)
     - AdManager (Singleton)
     - ShopManager (Singleton)
     - 所有View和Component (Transient)
```

---

# Phase 1: 核心框架搭建（MVP）

## Step 1.1: 创建EventBus事件总线

### Cursor提示词
```
请在 Assets/_Project/Scripts/Core/EventBus.cs 中实现事件总线：

1. 使用 UniRx Subject 实现发布/订阅模式
2. 支持泛型事件类型
3. 提供 Publish<T>(T eventData) 和 Subscribe<T>(Action<T> handler) 方法
4. 返回 IDisposable 用于取消订阅
5. 线程安全（可选，MVP阶段可以先不做）

MVP需要的事件类型（见Architecture_MVP.md第7节）：
- MarketRefreshedEvent
- DurianPurchasedEvent
- BagUpdatedEvent / BagFullEvent
- DurianSoldEvent
- ShopUpgradedEvent
- AdRewardEvent
```

---

## Step 1.2: 创建PlayerData单例

### Cursor提示词
```
请在 Assets/_Project/Scripts/Model/PlayerData.cs 中创建玩家数据：

public class PlayerData
{
    public static PlayerData Instance;
    
    public int Gold { get; set; } = 200;       // 初始金币
    public int DailyAdCount { get; set; } = 0;  // 今日看广告次数
    public float DailyBuff { get; set; } = 0f;   // 每日Buff（出肉率加成）
}
```

---

# Phase 2: 数据模型与配置（MVP）

## Step 2.1: 创建核心枚举和数据结构

### Cursor提示词
```
请在 Assets/_Project/Scripts/Model/ 下创建：

1. Enums.cs - 包含：
   - VarietyType: { JinZheng, GanYao, MaoShanWang }
   - AppearanceType: { Poor, Normal, Good, Premium }
   - YieldGrade: { Empty, Low, Normal, High, Perfect }
   - AdRewardType: { SellBonus, Revive, DailyBuff, Clue }

2. DurianData.cs - 结构体：
   - int id, VarietyType variety, AppearanceType appearance
   - float appearancePriceMultiplier, int basePrice, int finalPrice
   - float yieldRate, int roomCount, bool[] roomResults, YieldGrade yieldGrade
```

---

## Step 2.2: 创建ScriptableObject配置

### Cursor提示词
```
请在 Assets/_Project/Data/ 下创建3个ScriptableObject配置：

1. VarietyConfig.cs（品种配置）
   - VarietyType type, string varietyName
   - int basePrice, float baseYieldRate
   - int minRooms, maxRooms
   - float[] baseWeights = new float[5]（5档基础权重）

   品种数据（手动创建3个asset文件）：
   - 金枕: 价格80, 出肉率30%, 3-5房, 权重[30,35,25,8,2]
   - 干尧: 价格200, 出肉率35%, 3-6房, 权重[20,30,25,18,7]
   - 猫山王: 价格400, 出肉率42%, 4-7房, 权重[10,20,25,30,15]

2. AppearanceConfig.cs（外观配置）
   - AppearanceType type, string appearanceName
   - float priceMultiplier, float spawnWeight
   - float[] probabilityOffsets = new float[5]（5档偏移权重）

   外观数据（手动创建4个asset文件）：
   - 劣质: 价格×0.8, 权重35%, 偏移[+15,+10,-10,-10,-5]
   - 普通: 价格×1.0, 权重40%, 偏移[0,0,0,0,0]
   - 优质: 价格×1.5, 权重20%, 偏移[-10,-5,+10,+5,0]
   - 极品: 价格×2.2, 权重5%, 偏移[-15,-10,+5,+10,+5]

3. ShopConfig.cs（商店配置）
   - int[] upgradeCosts = {0, 500}
   - float[] sellBonuses = {0f, 0.2f}
```

---

# Phase 3: 业务管理器（MVP）

## Step 3.1: 实现MarketManager

### Cursor提示词
```
请在 Assets/_Project/Scripts/Manager/MarketManager.cs 中实现市场管理器：

1. 依赖注入 DurianGeneratorSystem
2. CurrentMarketDurians 属性：DurianData[3]
3. RefreshMarket(VarietyType variety)：调用生成器生成3颗榴莲，发布MarketRefreshedEvent
4. BuyDurian(int index)：扣金币，发布DurianPurchasedEvent
5. GetPriceMultiplier(AppearanceType)：返回对应外观的价格系数
```

---

## Step 3.2: 实现BagManager

### Cursor提示词
```
请在 Assets/_Project/Scripts/Manager/BagManager.cs 中实现背包管理器：

1. Durians: List<DurianData>，MaxCapacity = 10
2. AddDurian(DurianData)：容量检查 → 添加 → 发布BagUpdatedEvent
3. RemoveDurian(int index)：移除 → 发布BagUpdatedEvent
4. 背包满时发布BagFullEvent
```

---

## Step 3.3: 实现SellManager（MVP简化为固定价）

### Cursor提示词
```
请在 Assets/_Project/Scripts/Manager/SellManager.cs 中实现售卖管理器（MVP版固定价格回收）：

1. 依赖注入 ShopManager
2. CalculateSellPrice(DurianData)：根据出肉率档位返回固定价格
   - Empty: 0金币, Low: 10金币, Normal: 50金币, High: 100金币, Perfect: 250金币
   - 乘以商店加成系数
3. SellDurian(DurianData)：加金币 → 发布DurianSoldEvent
4. ApplyAdBonus()：看广告后临时+20%售价
```

---

## Step 3.4: 实现AdManager

### Cursor提示词
```
请在 Assets/_Project/Scripts/Manager/AdManager.cs 中实现广告管理器：

1. 每日广告上限10次，变量_dailyAdCount
2. 单类上限字典：
   - "sel_bonus": 5次（顾客加价）
   - "revive": 3次（复活）
   - "daily_buff": 1次（每日Buff）
   - "free_smell": -1（不限）
3. CanShowAd(string adType)：检查每日上限+单类上限
4. ShowRewardedAd(string adType)：调用微信SDK → 计数 → 发放奖励 → 发布AdRewardEvent

MVP暂用Mock实现（因为微信SDK需要在真实环境中测试）：
- 用 UniTask.Delay(2000) 模拟广告播放2秒
- 返回true模拟广告看完
```

---

## Step 3.5: 实现ShopManager

### Cursor提示词
```
请在 Assets/_Project/Scripts/Manager/ShopManager.cs 中实现商店管理器（MVP仅2级）：

1. CurrentLevel: 1-2, MaxLevel = 2
2. 升级费用：Lv1免费, Lv2需500金币
3. 售卖加成：Lv1=0%, Lv2=+20%
4. CanUpgrade()：级别检查+金币检查
5. Upgrade()：扣金币 → 升级 → 发布ShopUpgradedEvent
```

---

# Phase 4: 逻辑系统（MVP）

## Step 4.1: 实现AppearanceProbabilitySystem

### Cursor提示词
```
请在 Assets/_Project/Scripts/System/AppearanceProbabilitySystem.cs 中实现概率叠加系统：

1. CalculateFinalProbabilities(float[] baseWeights, float[] offsets)
   - 权重叠加：final[i] = max(0, base[i] + offset[i])
   - 归一化：final[i] / sum * 100
   
2. SampleYieldGrade(float[] probabilities)
   - 按累积概率随机抽取，返回YieldGrade枚举

3. 测试用例（手动验证）：
   - 金枕基础权重[30,35,25,8,2] + 极品偏移[-15,-10,+5,+10,+5]
   - 期望：最终权重[15,25,30,18,7] → 空壳概率从30%降至~16%
```

---

## Step 4.2: 实现DurianGeneratorSystem

### Cursor提示词
```
请在 Assets/_Project/Scripts/System/DurianGeneratorSystem.cs 中实现榴莲生成系统：

1. 依赖注入AppearanceProbabilitySystem
2. 注入VarietyConfig[]和AppearanceConfig[]（通过ScriptableObject）

3. GenerateMarketDurians(VarietyType variety)：返回DurianData[3]
   对每颗榴莲：
   - 随机外观等级（按spawnWeight加权随机）
   - 调用概率系统计算最终概率
   - 抽取出肉率档位（SampleYieldGrade）
   - 在档位区间随机生成具体出肉率（±5%随机波动）
   - 随机房数（品种minRooms-maxRooms之间）
   - 按出肉率随机生成每房是否有肉
   - 计算finalPrice = basePrice × priceMultiplier

4. RandomAppearance()：按spawnWeight随机抽取外观等级
   劣质35% / 普通40% / 优质20% / 极品5%
```

---

# Phase 5: 核心交互组件（MVP）

## Step 5.1: 实现KnifeTool（划刀交互）

### Cursor提示词
```
请在 Assets/_Project/Scripts/Component/KnifeTool.cs 中实现划刀工具：

1. 检测手指在榴莲顶部区域的滑动
2. 沿滑动方向绘制裂缝线（LineRenderer或DOTween路径动画）
3. 滑动距离达到阈值（如80%壳宽度）→ 触发DurianOpener.Open()
4. 播放裂缝音效 + 轻微震屏
5. 使用UniTask管理异步动画

简化MVP版：一刀切开全部（不逐房揭示），全部房数一次性显示。
```

---

## Step 5.2: 实现DurianOpener（榴莲揭示）

### Cursor提示词
```
请在 Assets/_Project/Scripts/Component/DurianOpener.cs 中实现榴莲揭示：

1. Open(DurianData durian)：开始揭示流程
2. 壳裂开动画（DOTween Scale裂开效果）
3. 果肉揭示动画（逐房弹出，全部同时显示）
4. 根据roomResults显示每房的可视化状态：
   - 有肉：金色果肉图标 + 飘出"满房"文字
   - 无肉：灰色空壳图标 + 飘出"空房"文字
5. 计算总出肉率 → 显示"出肉率评级"（见GDD_MVP 3.2节）
6. 发布 DurianOpenedEvent（携带出肉率评级）
7. 完成动画后 → 自动跳转SellPage
```

---

# Phase 6: UI页面实现（MVP）

## Step 6.1: 实现MarketPage（市场选购页）

### Cursor提示词
```
请创建 Assets/_Project/Scripts/View/MarketPage.cs：

1. UI布局（Prefab）：
   - 顶部：玩家金币显示
   - 中部：品种选择按钮×3（金枕/干尧/猫山王）
   - 底部：3颗榴莲卡片（同品种，不同外观）
     - 每张卡片显示：榴莲外观图、外观等级图标、标价
     - "免费试闻"按钮（触发看广告）
   - 底部："返回"按钮

2. 交互逻辑：
   - 点击品种按钮 → 调用MarketManager.RefreshMarket() 刷新3颗榴莲
   - 点击榴莲卡片 → 调用MarketManager.BuyDurian() → 进入OpenPage
   - 点击"免费试闻" → 调用AdManager.ShowRewardedAd("free_smell") → 显示微弱线索
   - 如果金币不足 → 红色标价 + 无法点击
```

---

## Step 6.2: 实现OpenPage（开榴莲页）

### Cursor提示词
```
请创建 Assets/_Project/Scripts/View/OpenPage.cs：

1. UI布局：
   - 中部：榴莲未开状态图（大图展示）
   - 手势引导："在榴莲顶部滑动开果"
   - 右上角：出肉率实时估价

2. 交互流程：
   - 玩家滑动 → KnifeTool检测滑动 → DurianOpener.Open()
   - 揭示完成后显示：
     - 每房结果（果肉/空壳图标）
     - 总出肉率 + 评级（空壳/小亏/回本/小赚/大赚/榴莲之王）
     - "卖出"按钮（跳转SellPage）
   - 如果开出空壳 → 弹出"复活"广告按钮
```

---

## Step 6.3: 实现SellPage（售卖页）

### Cursor提示词
```
请创建 Assets/_Project/Scripts/View/SellPage.cs：

1. UI布局：
   - 榴莲结果回顾（出肉率+评级）
   - "固定回收价"显示
   - "看广告加价+20%"按钮（触发AdManager）
   - "确认卖出"按钮
   - 卖出后显示金币增加动画

2. 交互：
   - 点击"看广告加价" → AdManager.ShowRewardedAd("sel_bonus") → 价格更新
   - 点击"确认卖出" → SellManager.SellDurian() → 金币动画 → 返回MarketPage
```

---

## Step 6.4: 实现BagPage（背包页）

### Cursor提示词
```
请创建 Assets/_Project/Scripts/View/BagPage.cs：

1. UI布局：
   - 网格展示已购买的榴莲（卡片形式）
   - 每张卡片：品种名、外观等级图标、简要信息
   - 点击榴莲 → 进入OpenPage开果
   - 容量显示："5/10"

2. 如果背包为空：显示"去市场买榴莲吧"引导按钮
```

---

## Step 6.5: 实现ShopPage（商店页）

### Cursor提示词
```
请创建 Assets/_Project/Scripts/View/ShopPage.cs：

1. UI布局：
   - 当前等级显示："商店 Lv.1"
   - 升级按钮："升级到 Lv.2（500金币）"
   - 效果说明："回收价+20%"
   - 如果已满级：显示"已满级"灰色状态
   - 去广告卡购买入口："去广告 ¥6（首充）"

2. 交互：
   - 点击升级 → ShopManager.Upgrade() → 效果立即生效
   - 金币不足：按钮灰色
```

---

# Phase 7: 微信小游戏适配（MVP简化）

## Step 7.1: 微信SDK接入与打包

### Cursor提示词
```
请帮我完成微信小游戏适配：

1. 安装微信小游戏SDK：
   - UPM: com.qq.weixin.minigame（使用Unity WeChat Mini Game插件）

2. Player Settings调整：
   - 切换到WebGL平台
   - 设置微信小游戏相关宏定义
   - Stripping Level: High

3. MVP简化的文件系统：
   - 使用PlayerPrefs存储金币和商店等级
   - 不使用云存储

4. 打包检查：
   - 首包大小 <8MB（MVP贴图少，应该很容易满足）
   - 测试：Build → 微信开发者工具预览
```

---

# Phase Ads: 广告系统接入（MVP）

## Step Ads.1: 创建微信广告Mock + 接入点

### Cursor提示词
```
请创建微信广告的Mock实现（便于开发测试）：

1. 创建 Assets/_Project/Scripts/Manager/WeChatAdSDK.cs
   - MockShowRewardedVideo(string adUnitId)：
     用 UniTask.Delay(2000) 模拟广告播放
     返回true（模拟广告看完）
   - 添加宏定义：#if UNITY_EDITOR 使用Mock，#else 使用真实微信SDK

2. 在AdManager中接入5个广告位的广告单元ID映射：
   - "free_smell" → ad-unit-market
   - "sel_bonus" → ad-unit-sell
   - "revive" → ad-unit-open
   - "daily_buff" → ad-unit-daily
   - "ad_free" → ad-unit-shop（去广告卡，非激励视频）

3. 广告按钮UI规范：
   - 按钮面积 ≤ 屏幕5%
   - 不得遮挡核心玩法
   - 显示"看广告"图标 + 奖励文字
```

---

## MVP开发顺序总结

```
Phase 0（项目初始化）
  ↓
Phase 1（EventBus + PlayerData）
  ↓
Phase 2（数据模型 + ScriptableObject配置）
  ↓
Phase 3（5个Manager）
  ↓
Phase 4（2个System）
  ↓
Phase 5（核心交互组件）
  ↓
Phase 6（5个UI页面）
  ↓
Phase Ads（广告接入）
  ↓
Phase 7（微信打包）
```

**可并行节点**：
- Phase 6 的5个UI页面可以轮流开发，互不依赖
- Phase Ads 可以在Phase 3 AdManager完成后就开始
- 贴图生成与程序编写全程并行

---

**文档版本**：MVP v1.0 | 2026-06-15
**下一步**：阅读 AI_Art_Prompts_MVP.md → 开始贴图生成
