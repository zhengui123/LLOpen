# 方案 B：TMP-Text-Effects 文字特效系统

> **费用**：免费 MIT | **难度**：低 | **耗时**：15 分钟 | **效果**：文字层特效，配合数字滚动使用

---

## 一、方案概览

[TMP-Text-Effects](https://github.com/SmellyCat2002/TMP-Text-Effects) 是一个轻量级开源库，灵感来自游戏 SNKRX 的标签式文本动画系统，专为 Unity TextMeshPro 重新实现。

核心思路：用类似 `[bounce,rainbow]+999[/bounce]` 的标签语法，直接在 TMP 文本上叠加特效。它直接操作 TMP 顶点数组，性能极高，不需要额外的 GameObject。

**优点**：轻量（3 个文件）、标签语法直观、性能好、MIT 协议随便用  
**缺点**：特效种类有限（6 种内置）、不能做数字滚动（需要配合方案 A）  
**最佳定位**：作为"视觉特效层"，叠加在方案 A 的数字滚动之上

---

## 二、前置准备

### Cursor 指令：下载并导入

```
1. 打开浏览器，访问 https://github.com/SmellyCat2002/TMP-Text-Effects
2. 点击绿色 Code 按钮 → Download ZIP
3. 解压后，将 TextEffects 文件夹拖入 Unity 的 Assets/Plugins/ 目录
4. 或者在终端执行：
```

```bash
# 在项目根目录执行
cd Assets/Plugins
git clone https://github.com/SmellyCat2002/TMP-Text-Effects.git TempClone
# 只保留 TextEffects 文件夹
cp -r TempClone/Assets/TextEffects .
rm -rf TempClone
```

### Cursor 指令：确认导入成功

```
在 Unity Project 窗口中检查：
Assets/Plugins/TextEffects/
  ├── TMPEffectText.cs
  ├── TextEffectSystem.cs
  └── TextTag.cs

3 个文件存在即可。
```

---

## 三、Cursor 指令：逐步实现

### 步骤 1：创建带特效的文本对象

```
1. 在 Canvas 下右键 → UI → Text - TextMeshPro（创建 TMP 文本）
2. 重命名为 "EffectText"
3. 给 EffectText 添加 TMPEffectText 组件（Add Component → 搜索 TMPEffectText）
4. TMPEffectText 会自动绑定同 GameObject 上的 TMP_Text
```

### 步骤 2：基础特效测试

在 Inspector 中，找到 TMPEffectText 组件的 `Raw Text` 字段，输入：

```
[bounce]+999[/bounce]
```

点 Play 运行，文字应该会弹跳。

### 步骤 3：写一个结合数字滚动的管理器

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 TMPEffectScoreDisplay.cs。
删除默认内容，粘贴下方代码。
```

**完整代码：**

```csharp
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

namespace GTest.UI
{
    /// <summary>
    /// TMP-Text-Effects + DOTween 数字滚动的组合封装。
    /// 数字用 DOTween 滚动，特效用标签语法叠加。
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(TMPEffectText))]
    public class TMPEffectScoreDisplay : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] private TMPEffectText _effectText;

        [Header("动画参数")]
        [SerializeField] private float _defaultDuration = 0.6f;
        [SerializeField] private Ease _defaultEase = Ease.OutBack;
        [SerializeField] private string _numberFormat = "N0";

        [Header("特效标签预设")]
        [SerializeField] private string _normalTags = "bounce";           // 普通得分
        [SerializeField] private string _mediumTags = "bounce,pulse";    // 中等得分
        [SerializeField] private string _criticalTags = "shake,rainbow,pulse"; // 暴击
        [SerializeField] private string _lossTags = "shake";             // 扣分

        private TMP_Text _tmpText;
        private float _currentValue;
        private Tween _countTween;

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            if (_effectText == null)
                _effectText = GetComponent<TMPEffectText>();
        }

        /// <summary>
        /// 设置初始值（无动画）
        /// </summary>
        public void SetValue(float value)
        {
            _currentValue = value;
            _effectText.SetText(value.ToString(_numberFormat));
        }

        /// <summary>
        /// 带特效标签的数字增长动画
        /// </summary>
        public void AnimateScore(float target, ScoreImpact impact = ScoreImpact.Normal,
            float? duration = null)
        {
            float dur = duration ?? _defaultDuration;
            string tags = GetTags(impact, target);
            _countTween?.Kill();

            _countTween = DOTween.To(
                () => _currentValue,
                x =>
                {
                    _currentValue = x;
                    // 滚动中：显示纯数字（不加特效标签，避免性能抖动）
                    _tmpText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                },
                target, dur
            )
            .SetEase(_defaultEase)
            .OnComplete(() =>
            {
                _currentValue = target;
                // 停稳后：加上特效标签
                string finalText = target.ToString(_numberFormat);
                if (!string.IsNullOrEmpty(tags))
                {
                    finalText = $"[{tags}]{finalText}[/{tags.Split(',')[0]}]";
                }
                _effectText.SetText(finalText);

                // 延迟一段后去掉特效标签（恢复正常显示）
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    if (this != null)
                        _effectText.SetText(target.ToString(_numberFormat));
                });
            })
            .SetUpdate(true);
        }

        /// <summary>
        /// 扣分
        /// </summary>
        public void AnimateLoss(float target)
        {
            AnimateScore(target, ScoreImpact.Loss, _defaultDuration * 0.8f);
        }

        /// <summary>
        /// 立即显示数值+特效标签（无滚动）
        /// </summary>
        public void ShowWithEffect(float value, ScoreImpact impact)
        {
            string tags = GetTags(impact, value);
            string text = value.ToString(_numberFormat);
            if (!string.IsNullOrEmpty(tags))
            {
                text = $"[{tags}]{text}[/{tags.Split(',')[0]}]";
            }
            _effectText.SetText(text);
            _currentValue = value;
        }

        private string GetTags(ScoreImpact impact, float target)
        {
            float delta = target - _currentValue;

            return impact switch
            {
                ScoreImpact.Loss => _lossTags,
                ScoreImpact.Critical => _criticalTags,
                ScoreImpact.Normal when Mathf.Abs(delta) >= 500 => _mediumTags,
                ScoreImpact.Normal when Mathf.Abs(delta) >= 1000 => _criticalTags,
                _ => _normalTags
            };
        }

        public void Stop()
        {
            _countTween?.Kill();
            _effectText.SetText(_currentValue.ToString(_numberFormat));
        }

        private void OnDestroy()
        {
            _countTween?.Kill();
        }
    }

    /// <summary>
    /// 得分类型枚举
    /// </summary>
    public enum ScoreImpact
    {
        Normal,     // 普通
        Critical,   // 暴击
        Loss        // 扣分
    }
}
```

### 步骤 4：自定义特效标签（可选）

```
如果想添加自己的特效（比如"金色闪烁"），
打开 TextEffectSystem.cs，在 GenerateDefaultTags() 方法中添加。
```

**Cursor 指令：添加自定义特效**

```
用 IDE 打开 Assets/Plugins/TextEffects/TextEffectSystem.cs。
找到 GenerateDefaultTags() 方法，在末尾添加以下代码：
```

```csharp
// 金色闪烁特效
RegisterTag(new TextTag("golden", (c, dt, i, text) =>
{
    // 缩放脉冲
    float scale = 1f + Mathf.Sin(Time.time * 10f + i * 0.5f) * 0.15f;
    c.scale = scale;
    // 金色渐变
    float t = (Mathf.Sin(Time.time * 5f + i) + 1f) / 2f;
    c.color = Color.Lerp(Color.yellow, new Color(1f, 0.8f, 0f), t);
}));

// 从下方飞入
RegisterTag(new TextTag("flyup", (c, dt, i, text) =>
{
    float progress = Mathf.Clamp01(Time.time * 2f - i * 0.1f);
    c.oy = Mathf.Lerp(-30f, 0f, EaseOutBack(progress));
    c.scale = Mathf.Lerp(0.5f, 1f, progress);
}));

// 缓动函数辅助
static float EaseOutBack(float t)
{
    float c1 = 1.70158f;
    float c3 = c1 + 1;
    return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
}
```

使用：
```
[golden,flyup]+5200[/golden]
```

---

## 四、所有可用特效标签速查

| 标签 | 效果 | 适用场景 |
|------|------|---------|
| `[bounce]` | 垂直弹跳 | 普通得分 |
| `[wavy]` | 正弦波浪 | 货币变化 |
| `[wavy_mid]` | 中等幅度波浪 | 显示数值 |
| `[pulse]` | 脉冲缩放 | 强调数值 |
| `[shake]` | 随机震动 | 暴击、扣分 |
| `[rainbow]` | 彩虹渐变色 | 大额得分 |
| `[spin]` | 旋转 | 刷新/重置 |

标签可以逗号分隔组合：
```
[wavy,rainbow,pulse]+10000[/wavy]
```

注意：关闭标签只需要写第一个标签名。

---

## 五、与方案 A 的配合

方案 B 不包含数字滚动——它只负责"文字显示层的特效"。最佳用法：

```
方案 A（ScoreCounter）负责数字从 0 滚到 1000
方案 B（TMPEffectText + 标签）负责滚完后叠上 [pulse,rainbow] 特效
```

具体做法：在上面的 `TMPEffectScoreDisplay` 中已经封装好了——滚动阶段用纯 TMP 显示数字，停稳后切到 `TMPEffectText.SetText()` 加上标签特效。

---

## 六、性能注意

- 每个 `TMPEffectText` 每帧会遍历所有可见字符，修改顶点
- 10 个以内同时使用无压力
- 超过 20 个时考虑用对象池、或缩短特效持续时间（`DelayedCall` 中去掉特效）
- 适合用在得分飘字、货币计数器等特定 UI 元素，**不要全屏所有文字都挂**

---

## 七、常见问题

| 问题 | 解决 |
|------|------|
| 标签没效果 | 必须用 `TMPEffectText.SetText()` 设置文本，不能用 `TMP_Text.text` |
| 特效不消失 | 需要手动切回普通文本，或用 `DelayedCall` 自动切 |
| 与其他 TMP 插件冲突 | 它直接操作顶点，与修改顶点数组的其他插件可能冲突 |
| 字体太小特效不明显 | 增大 `scale` 或 `oy` 的乘数 |
