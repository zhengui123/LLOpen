# 游戏经济配置说明

配置文件路径：`Assets/_Project/Resources/GameEconomyConfig.json`（运行时通过 `Resources.Load` 加载）。

**注意：说明文档不要放在 `Resources` 目录**，否则会与 JSON 同名冲突导致加载失败。

## 字段说明

| 字段 | 说明 |
|------|------|
| `initialGold` | 新局初始金币 |
| `marketRefreshCost` | 市场「换一批」金币费用；不足时走激励广告 |
| `sellYieldMultipliers` | 卖出价倍率对象：`empty` / `low` / `normal` / `high` / `perfect` |
| `varietyBasePrices` | 品种基础价对象：`jinZheng` / `ganYao` / `maoShanWang` |

注意：Unity `JsonUtility` 不支持 `[0.2, 0.6]` 这种数组写法，必须用嵌套对象字段。

## 卖出价公式

```
卖出价 = Round(购买价 finalPrice × 出肉档位倍率 × (1 + 商店加成 + 临时广告加成))
```

购买价已包含外观系数（劣质 0.8 / 普通 1.0 / 优质 1.5 / 极品 2.2）。

## 当前初始数值（v1.2）

- 初始金币：400
- 换一批：25 金币
- 品种基础价：50 / 100 / 180
- 出肉倍率：empty 0.2 / low 0.6 / normal 1.0 / high 1.35 / perfect 1.85

示例：金枕普通外观购买约 50 金币，Normal 出肉卖出约 50；High 约 68；Perfect 约 93。
