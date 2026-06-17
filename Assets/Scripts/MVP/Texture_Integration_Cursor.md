# 《llopen MVP》- 贴图集成指令（项目对齐版）

> 版本：MVP v1.1 | 日期：2026-06-17  
> 前置：`Cursor_Prompts_MVP.md` 中 **Phase 0–6 + Ads Mock** 代码已基本落地  
> 贴图目录：`Assets/_Project/Art/Textures/`（41 张，按 AI_Art_Prompts 命名）  
> 本文档用途：在**现有 uGUI + VContainer 架构**上挂载贴图，**不推翻**已实现的色块占位逻辑

---

## 一、当前项目架构（梳理）

### 1.1 场景与启动

```
Launch.unity
  └─ GameBootstrap（60fps、DontDestroyOnLoad）
  └─ MvpSceneLoader → LoadScene("Main")

Main.unity
  └─ GameRoot
       ├─ GameLifetimeScope（autoRun=false，VContainer 注册）
       └─ GameBootstrapper（Awake 时 Build 容器）
  └─ MVP_Canvas
       └─ UIRoot / GameUIRoot（单页激活：Market/Open/Sell/Bag/Shop）
  └─ EventSystem
```

搭建入口：**Tools → llopen → Setup MVP Scene**（`MvpSceneSetupEditor.cs`）

### 1.2 依赖注入（GameLifetimeScope）

| 类型 | 注册方式 | 说明 |
|------|----------|------|
| EventBus、System、Manager | `Register Singleton` | 纯 C# 服务 |
| VarietyConfig[]、AppearanceConfig[]、ShopConfig | `RegisterInstance` | 来自 `Resources/` |
| GameUIRoot、各 Page | `RegisterComponent` | 场景已有 MonoBehaviour |
| **待增** DurianSpriteConfig | `RegisterInstance` | 贴图集成后注册 |

配置加载：`Resources/Variety`、`Resources/Appearance`、`Resources/ShopConfig`

### 1.3 事件流（EventBus + UniRx Subject）

```
MarketManager.RefreshMarket → MarketRefreshedEvent → MarketPage.RefreshCards
MarketManager.BuyDurian     → DurianPurchasedEvent → BagManager + OpenPage
DurianOpener.OpenAsync      → DurianOpenedEvent    → OpenPage（卖出/复活按钮）
SellManager.SellDurian      → DurianSoldEvent
ShopManager.Upgrade         → ShopUpgradedEvent    → ShopPage.Refresh
AdManager.ShowRewardedAd    → AdRewardEvent（Mock 2s）
```

### 1.4 UI 与表现（与原版文档的差异）

| 模块 | 文档假设 | **本项目实际** |
|------|----------|----------------|
| UI 体系 | Prefab + 可能 TMP | **uGUI `Image`/`Text`**，字体 `GeourceAltCHT-Medium` |
| 榴莲展示 | SpriteRenderer 世界坐标 | **Canvas `Image` 色块**，`DurianDisplayUtil.GetAppearanceColor` |
| 开果壳裂 | SH-01/02 左右半壳分离 | **RectTransform `DOScale` 挤压**（`DurianOpener.shellTransform`） |
| 房位揭示 | FL-01/02 SpriteRenderer | **UI Prefab** `RoomMeat`/`RoomEmpty` + `FloatText` |
| 划刀 | 刀 SpriteRenderer 跟随 | **`LineRenderer` 裂缝线** + `DOShakePosition` 震屏 |
| 开果后跳转 | 自动进 SellPage | **OpenPage「卖出」按钮** → SellPage |
| 商店 | 含去广告 IAP | **仅 2 级升级，无去广告** |
| 场景名 | MainScene | **`Main`** |
| 菜单 | 榴莲开了 | **`Tools/llopen/`** |

### 1.5 已有工具类

- `DurianDisplayUtil`：品种/外观**中文名**、色块颜色、背包简要信息  
- `YieldRatingUtil`：按 `yieldRate` 返回「空壳/小亏/…/榴莲之王」**文案**（非 YieldGrade 枚举直显）

贴图集成时：**文案继续用 DurianDisplayUtil，贴图用 DurianSpriteConfig，二者互补。**

---

## 二、软件开发指令完成度（Cursor_Prompts_MVP.md）

| Phase | 步骤 | 状态 | 备注 |
|-------|------|------|------|
| 0 | 项目配置 / Bootstrap / LifetimeScope | ✅ | URP、VContainer、UniTask、UniRx、DOTween |
| 1 | EventBus、PlayerData | ✅ | `RegisterBuildCallback` 强制 Resolve EventBus |
| 2 | Enums、DurianData、SO 配置 | ✅ | Resources 下 Variety/Appearance/ShopConfig |
| 3 | 5 个 Manager | ✅ | AdManager 为 Mock |
| 4 | 概率系统 + 生成器 | ✅ | 含 `RerollOpenResult`（复活） |
| 5 | KnifeTool、DurianOpener | ✅ | DOTween；API 为 `OpenAsync` |
| 6.1 | MarketPage | ✅ | 无「返回」；购买为独立按钮；DOTween 卡片动效 |
| 6.2 | OpenPage | ✅ | 卖出按钮在 OpenPage；复活重 roll |
| 6.3 | SellPage | ✅ | DOTween 金币滚动 + PunchScale |
| 6.4 | BagPage | ✅ | 网格、`DurianDisplayUtil`、空背包引导 |
| 6.5 | ShopPage | ✅ | 无去广告；动态升级费用 |
| 7 | 微信打包 | ⬜ | 未做 |
| Ads | WeChatAdSDK 独立类 | ⬜ | 逻辑合并在 `AdManager` Mock |

**结论**：贴图集成是在**可运行 MVP** 上换皮，不是从零写玩法。

---

## 三、贴图资源清单（41 张）

| 类别 | 编号 | 挂载目标（本项目） |
|------|------|-------------------|
| 场景背景 | S-01~02 | `GameUIRoot` 下背景 `Image`（市场/开果切换） |
| 榴莲未开 | D-01~03, D-A01~09 | Market/Open/Bag 的 `Image.sprite` |
| 已开壳 | D-O01~06 | 可选：开果后 `durianImage` 替换（非必须 MVP） |
| 开果动画 | K-01, SH-01~02, FL-01~02 | **可选增强**；默认保留 LineRenderer + 现有 Room Prefab |
| 外观图标 | U-01~04 | Market 卡片、`BagCard/Block` 旁小图标 |
| UI 图标 | UI-05~09 | 金币栏、广告按钮、引导、返回、品种底图 |
| 评级图标 | R-01~06 | `DurianOpener` 或 OpenPage 的 `ratingIcon` Image |
| 市场框架 | F-01 | `CardRow` 底板或单卡边框 |

---

# Phase T0: 贴图导入配置

## Step T0.1: 批量 Texture Import Settings

### Cursor 提示词
```
请在 Unity Editor 中创建菜单 Tools/llopen/Configure Texture Import Settings，
对 Assets\Art\MVP\下所有贴图批量设置：

1. S-*.jpg：Sprite、Max 2048、Bilinear
2. D-*/D-A*/D-O*/SH-*：Sprite、Max 1024、Pixels Per Unit 512
3. K/FL/U/UI/R/F-*：Sprite、Max 512、Pixels Per Unit 256

使用 TextureImporter，按扩展名区分压缩。完成后输出处理文件数量。
```

---

# Phase T1: 场景背景（挂到 GameUIRoot）

## Step T1.1: 背景图与页面切换

### Cursor 提示词
```
本项目 UI 在 Main 场景 MVP_Canvas 下，不要用 SpriteRenderer。

1. 在 GameUIRoot 同层或 Canvas 底层增加：
   - marketBgImage（S-01_market_bg）
   - openBgImage（S-02_cuttingboard_bg）
   全屏 Stretch，置于各 Page 背后。

2. 修改 GameUIRoot：
   - ShowMarket/ShowBag/ShowShop/Sell → 显示 marketBg，隐藏 openBg
   - ShowOpen → 显示 openBg，隐藏 marketBg

3. 在 GameLifetimeScope 无需注册；SerializeField 拖引用即可。

4. 更新 MvpSceneSetupEditor.BuildMainScene：创建两个背景 Image 并连线。

Canvas Scaler：Scale With Screen Size，Reference 1080×1920（若尚未设置）。
```

**勿用**原文档中的 `GameManager.ShowMarketBg()`——本项目无 GameManager。

---

# Phase T2: DurianSpriteConfig（核心）

## Step T2.1: 创建贴图配置 SO

### Cursor 提示词
```
创建 Assets/_Project/Scripts/Model/DurianSpriteConfig.cs：

[CreateAssetMenu(fileName = "DurianSpriteConfig", menuName = "llopen/贴图配置")]

字段与查询方法与 AI_Art 清单一致：
- 未开/变体/已开 Sprite 字段
- knifeSprite, shellLeftHalf, shellRightHalf, fleshPiece, emptyPiece
- poor/normal/good/premium 图标，UI-05~09，R-01~06，marketFrame

方法：
- GetUnopenedSprite(VarietyType, AppearanceType)
- GetOpenedSprite(VarietyType, YieldGrade)  // MVP 空壳/爆肉两极端
- GetAppearanceIcon(AppearanceType)
- GetRatingSprite(string ratingText)  // 对接 YieldRatingUtil 文案：空壳/小亏/…/榴莲之王
- GetRatingSprite(YieldGrade grade)   // 可选重载

在 Assets/_Project/Data/ 创建 DurianSpriteConfig.asset 并拖入 41 张 Sprite。

GameLifetimeScope 增加：
[SerializeField] private DurianSpriteConfig durianSpriteConfig;
builder.RegisterInstance(durianSpriteConfig);
EnsureConfigsLoaded 中 Resources.Load 兜底。
```

---

# Phase T3: 开果相关（在现有组件上增强）

## Step T3.1: KnifeTool — 保留 LineRenderer，可选刀图标

### Cursor 提示词
```
不要删除现有 LineRenderer 划刀逻辑。

在 KnifeTool 中：
1. [SerializeField] Image knifeImage（Canvas 下，可选）
2. [Inject] 或 SerializeField DurianSpriteConfig
3. 开始滑动：knifeImage.sprite = knifeSprite，跟随手指 anchoredPosition，DOTween 轻微震动
4. 开果触发：knifeImage.DOFade(0, 0.3f) 后隐藏

若未挂 knifeImage，行为与现在一致（仅裂缝线）。
```

## Step T3.2: DurianOpener — UI 壳裂 + 房位贴图

### Cursor 提示词
```
不要改为世界空间 SpriteRenderer 壳裂；保持 shellTransform（RectTransform）DOScale 挤压。

1. 注入 DurianSpriteConfig
2. roomMeatPrefab / roomEmptyPrefab：将 Image 色块改为 FL-01 / FL-02 Sprite（改 Prefab 或 Spawn 时赋值）
3. 可选：shellLeftImage / shellRightImage（SH-01/02）在 PlayShellCrackAsync 时
   - 隐藏 durianImage/wholeImage
   - 左右 Image DOLocalMoveX 分离（UI 坐标 ±80~120）
   若不挂则沿用现有 Scale 动画
4. ratingText 旁增加 ratingIcon Image：
   - sprite = config.GetRatingSprite(rating)
   - 已有 DOTween 缩放，图标与文字同步
5. 仍发布 DurianOpenedEvent，不自动跳 SellPage（OpenPage 负责）
```

---

# Phase T4: MarketPage

## Step T4.1: 榴莲卡片区贴图

### Cursor 提示词
```
MarketPage 已有 durianImages[]、appearanceTexts[]、priceTexts[]。

1. 注入 DurianSpriteConfig
2. RefreshCards 中：
   durianImages[i].sprite = config.GetUnopenedSprite(durian.variety, durian.appearance);
   durianImages[i].color = Color.white;  // 贴图后不再用纯色
   appearanceTexts[i].text = DurianDisplayUtil.GetAppearanceName(durian.appearance);

3. 新增（可选）[SerializeField] Image[] appearanceIcons：
   appearanceIcons[i].sprite = config.GetAppearanceIcon(durian.appearance);

4. goldText 左侧增加 goldIconImage → UI-05

5. varietyButtons 背景图 → UI-09（保留按钮上中文品种名）

6. CardRow 下可加 marketFrameImage（F-01）作为底板

更新 MvpSceneSetupEditor.BuildMarketPage 创建 appearanceIcons、goldIcon。
```

---

# Phase T5: OpenPage / BagPage / SellPage

## Step T5.1: OpenPage

### Cursor 提示词
```
1. durianImage.sprite = GetUnopenedSprite（Show 时）
2. 新增 swipeGuideImage（UI-07），与 guideText 并存；StartGuidePulse 时引导图同步呼吸；滑动开始后隐藏
3. 返回按钮可换 UI-08 贴图（backButton 子 Image）
4. 不在此页处理壳裂贴图（DurianOpener 负责）
```

## Step T5.2: BagPage

### Cursor 提示词
```
BagPage.SpawnCard 中：
- Block Image：GetUnopenedSprite 或仅 GetAppearanceIcon 角标 + 未开图
- 文案仍用 DurianDisplayUtil（品种/购价/出肉率）

更新 BagCard Prefab：Block 改为 Durian 贴图 Image，preserveAspect=true。
```

## Step T5.3: SellPage

### Cursor 提示词
```
SellPage summary 区域可选增加 ratingIcon（按 CurrentRating 字符串取 R-01~06）。
广告按钮旁小图标 UI-06（SerializeField，Refresh 时赋值）。
无需大改 DOTween 金币动画。
```

---

# Phase T6: 共用 UI（精简版，不用独立 CommonUIManager）

## Step T6.1: UiSpriteApplier 或编辑器一次性绑定

### Cursor 提示词
```
不创建庞大的 CommonUIManager。二选一：

方案 A（推荐）：扩展 MvpSceneSetupEditor
- 搭建场景时从 DurianSpriteConfig 读取 goldCoin、backArrow、watchAd
- 写入各页面对应 Image

方案 B：轻量 MonoBehaviour UiSpriteApplier.cs
- [SerializeField] DurianSpriteConfig config
- [SerializeField] Image[] goldIcons, backIcons, adIcons
- Awake 赋值 sprite

各 Page 的 backButton 子节点 Image 统一用 UI-08。
广告相关：Market smell、Sell adBonus、Open revive 按钮旁 48×48 UI-06。
```

---

# Phase T7: 替换色块与验证

## Step T7.1: 色块 → Sprite 扫描清单

### 需改动的文件

| 文件 | 替换点 |
|------|--------|
| `MarketPage.RefreshCards` | `durianImages[i].color` → `sprite` |
| `OpenPage.Show` | `durianImage.color` → `sprite` |
| `BagPage.SpawnCard` | `Block.color` → `sprite` |
| `DurianOpener` | Room Prefab Image、可选 shell/rating |
| `MvpSceneSetupEditor` | 搭建时默认 sprite 引用 |

**保留** `DurianDisplayUtil.GetAppearanceColor` 作为贴图缺失时的 Fallback。

## Step T7.2: 验证菜单

### Cursor 提示词
```
创建 Assets/Editor/TextureValidator.cs：

[MenuItem("Tools/llopen/验证贴图配置")]
检查 Assets/_Project/Data/DurianSpriteConfig.asset 所有 Sprite 字段非空，
输出 已挂载/缺失 数量。

与 DurianDisplayUtil 无冲突，仅验证 SO。
```

---

# Phase T8: 手动操作清单（约 20 分钟）

| 步骤 | 操作 |
|------|------|
| 1 | 将 41 张图放入 `Assets/_Project/Art/Textures/` |
| 2 | 执行 **Tools/llopen/Configure Texture Import Settings**（T0 脚本生成后） |
| 3 | Create → llopen → 贴图配置 → 保存到 `_Project/Data/` |
| 4 | 拖入全部 Sprite 字段 |
| 5 | `GameLifetimeScope` 拖入 `durianSpriteConfig` |
| 6 | 执行 **Tools/llopen/Setup MVP Scene**（重建 UI 引用） |
| 7 | **Tools/llopen/验证贴图配置** → 0 缺失 |
| 8 | Play：Launch → Main，走购买→开果→卖出→背包→商店 |

---

# 附录 A：原贴图文档中建议弃用/改写的条目

| 原指令 | 本项目处理 |
|--------|------------|
| SpriteRenderer 壳裂 + fleshGridParent 世界坐标 | 改为 **UI Image + RectTransform** 或保留 Scale 动画 |
| `Open()` 方法名 | 使用现有 **`OpenAsync`** |
| 自动跳转 SellPage | **OpenPage 卖出按钮** |
| CommonUIManager 每页挂载 | **Setup 编辑器绑定** 或轻量 Applier |
| GameManager 切背景 | **GameUIRoot** |
| `menuName "榴莲开了/..."` | **`llopen/...`** |
| WeChatAdSDK / ad_free | **不实现** |
| TMP 组件 | **UnityEngine.UI.Text** |
| MainScene | **Main** |

---

# 附录 B：推荐 Cursor 执行顺序

```
T0 导入设置
  ↓
T2 DurianSpriteConfig + LifetimeScope 注册
  ↓
T1 背景图 + GameUIRoot
  ↓
T4 MarketPage（最明显换皮）
  ↓
T5 Open/Bag/Sell
  ↓
T3 Knife/Opener 增强（可选壳半图）
  ↓
T6 共用图标
  ↓
T7 验证 + 删色块 Fallback 测试
```

**可并行**：T4/T5 各页面在 Config 就绪后互不阻塞。

---

**文档版本**：MVP v1.1（项目对齐）  
**依赖**：`Cursor_Prompts_MVP.md`（代码基线）、`AI_Art_Prompts_MVP.md`（贴图命名）  
**下一步**：按 T0 → T2 → T1 → T4 顺序将 Cursor 提示词逐条执行，并完成 Phase T8 手动清单
