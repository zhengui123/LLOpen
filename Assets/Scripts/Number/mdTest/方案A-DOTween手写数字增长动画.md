# 方案 A：DOTween 手写数字增长动画

> **费用**：免费 | **难度**：中等 | **耗时**：30 分钟 | **效果**：完全自定义

---

## 一、方案概览

用 DOTween（Unity 动画事实标准）手写数字增长逻辑。核心思路：

- 用 `DOTween.To()` 对 float 做补间动画，驱动数字从当前值滚到目标值
- 用 `DOPunchScale` / `DOShakePosition` 叠加视觉冲击
- 用不同缓动曲线（Ease）控制"手感"——小丑牌的核心秘密就在这

**优点**：零依赖（DOTween 你的项目大概率已有）、完全掌控、效果上限极高  
**缺点**：需要自己写代码、要调参找手感

---

## 二、前置准备

### Cursor 指令：检查 DOTween 是否已安装

```
在 Unity 中打开 Package Manager（Window → Package Manager）。
搜索 DOTween。如果已安装，跳过此步。

如果未安装：
1. 打开 Window → Package Manager
2. 点击左上角 + 号 → Add package from git URL
3. 输入：https://github.com/Demigiant/dotween.git
4. 等待安装完成，Unity 会自动弹出 DOTween 设置窗口
5. 点击 "Open DOTween Utility Panel" → "Setup DOTween"
```

### Cursor 指令：确认项目已有 TMP

```
检查项目中是否已有 TextMeshPro。
如果没有，在 Package Manager 中搜索 TextMeshPro 并安装。
第一次使用时需要导入 TMP Essential Resources：
Window → TextMeshPro → Import TMP Essential Resources
```

---

## 三、Cursor 指令：逐步实现

### 步骤 1：创建基础数字动画组件

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 ScoreCounter.cs。
删除默认内容，粘贴下方完整代码。
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
    /// 通用数字增长动画组件。
    /// 挂在带 TMP_Text 的 GameObject 上即可使用。
    /// </summary>
    public class ScoreCounter : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] private TMP_Text _displayText;

        [Header("动画参数")]
        [SerializeField] private float _defaultDuration = 0.6f;
        [SerializeField] private Ease _defaultEase = Ease.OutBack;
        [SerializeField] private string _numberFormat = "N0"; // N0 = 千分位

        [Header("视觉反馈")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _punchDuration = 0.2f;
        [SerializeField] private int _punchVibrato = 3;

        private float _currentValue;
        private Tween _countTween;
        private Tween _punchTween;
        private Vector3 _originalScale;

        // 公开的当前值
        public float CurrentValue => _currentValue;

        private void Awake()
        {
            if (_displayText == null)
                _displayText = GetComponent<TMP_Text>();

            _originalScale = transform.localScale;
        }

        /// <summary>
        /// 设置初始值（不会触发动画）
        /// </summary>
        public void SetValue(float value)
        {
            _currentValue = value;
            _displayText.text = value.ToString(_numberFormat);
        }

        /// <summary>
        /// 数字增长到目标值
        /// </summary>
        public void AnimateTo(float target, float? duration = null,
            Ease? ease = null, Action onComplete = null)
        {
            float dur = duration ?? _defaultDuration;
            Ease easeType = ease ?? _defaultEase;

            // 终止上一个计数动画，从当前位置开始
            _countTween?.Kill();

            _countTween = DOTween.To(
                () => _currentValue,
                x =>
                {
                    _currentValue = x;
                    _displayText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                },
                target,
                dur
            )
            .SetEase(easeType)
            .OnComplete(() =>
            {
                // 确保最终值精确
                _currentValue = target;
                _displayText.text = target.ToString(_numberFormat);

                // 数字停稳后弹一下
                PlayPunchEffect();
                onComplete?.Invoke();
            })
            .SetUpdate(true); // 无视 Time.timeScale
        }

        /// <summary>
        /// 数字减少到目标值
        /// </summary>
        public void AnimateDown(float target, float? duration = null,
            Ease? ease = null, Action onComplete = null)
        {
            AnimateTo(target, duration, ease ?? Ease.InOutCubic, onComplete);
        }

        /// <summary>
        /// 缩放弹跳反馈
        /// </summary>
        private void PlayPunchEffect()
        {
            _punchTween?.Kill();
            transform.localScale = _originalScale;

            _punchTween = transform
                .DOPunchScale(Vector3.one * (_punchScale - 1f),
                    _punchDuration, _punchVibrato, 0.5f)
                .SetUpdate(true);
        }

        /// <summary>
        /// 立即停止所有动画
        /// </summary>
        public void Stop()
        {
            _countTween?.Kill();
            _punchTween?.Kill();
            transform.localScale = _originalScale;
        }

        private void OnDestroy()
        {
            _countTween?.Kill();
            _punchTween?.Kill();
        }
    }
}
```

### 步骤 2：挂载组件

```
1. 在 Canvas 下创建一个 TextMeshPro - Text (UI)，命名为 "ScoreText"
2. 设置字体大小、颜色、对齐方式
3. 给 ScoreText 添加 ScoreCounter 组件
4. 把 ScoreText 的 TMP_Text 拖到 ScoreCounter 的 Display Text 字段
5. 在 Inspector 中调整动画参数（或保持默认值）
```

### 步骤 3：创建缓动曲线配置表

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 EasePresets.cs。
删除默认内容，粘贴下方代码。
```

**完整代码：**

```csharp
using DG.Tweening;

namespace GTest.UI
{
    /// <summary>
    /// 预设缓动曲线配置。
    /// 不同场景用不同 Ease，手感差异巨大。
    /// </summary>
    public static class EasePresets
    {
        // 普通得分 — 超出弹回，有"叮"的感觉
        public const Ease Normal = Ease.OutBack;

        // 中额得分 — 物理弹跳，更活泼
        public const Ease Medium = Ease.OutBounce;

        // 大额得分 / 暴击 — 弹簧震荡，最大冲击力
        public const Ease Critical = Ease.OutElastic;

        // 极速滚动 — 指数衰减，先快后慢，适合倒计时
        public const Ease Fast = Ease.OutExpo;

        // 线性 — 匀速，不推荐（太僵硬）
        public const Ease Linear = Ease.Linear;

        // 根据分数大小自动选择缓动
        public static Ease AutoSelect(float delta, float threshold1 = 100f,
            float threshold2 = 1000f)
        {
            if (delta >= threshold2) return Critical;
            if (delta >= threshold1) return Medium;
            return Normal;
        }
    }
}
```

### 步骤 4：创建增强型数字动画组件（可选）

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 ScoreAnimatorAdvanced.cs。
这个版本增加了颜色闪烁功能，效果更接近小丑牌。
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
    /// 增强型数字动画组件。
    /// 增加颜色闪烁、震动效果。
    /// </summary>
    public class ScoreAnimatorAdvanced : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] private TMP_Text _displayText;

        [Header("数字滚动")]
        [SerializeField] private float _defaultDuration = 0.6f;
        [SerializeField] private string _numberFormat = "N0";

        [Header("颜色")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _flashColor = Color.yellow;
        [SerializeField] private Color _criticalColor = new Color(1f, 0.3f, 0f); // 橙红

        [Header("缩放")]
        [SerializeField] private float _punchScale = 1.15f;
        [SerializeField] private float _punchDuration = 0.25f;
        [SerializeField] private int _punchVibrato = 5;

        [Header("震动")]
        [SerializeField] private float _shakeStrength = 5f;
        [SerializeField] private int _shakeVibrato = 10;
        [SerializeField] private float _shakeDuration = 0.3f;

        private float _currentValue;
        private Vector3 _originalPos;
        private Vector3 _originalScale;
        private Sequence _currentSeq;

        private void Awake()
        {
            if (_displayText == null)
                _displayText = GetComponent<TMP_Text>();
            _originalPos = ((RectTransform)transform).anchoredPosition;
            _originalScale = transform.localScale;
            _displayText.color = _defaultColor;
        }

        /// <summary>
        /// 设置初始值
        /// </summary>
        public void SetValue(float value)
        {
            _currentValue = value;
            _displayText.text = value.ToString(_numberFormat);
        }

        /// <summary>
        /// 普通得分动画：滚动 + 弹跳 + 颜色闪烁
        /// </summary>
        public void AnimateScore(float target, float? duration = null)
        {
            float dur = duration ?? _defaultDuration;
            _currentSeq?.Kill();
            ResetVisual();

            Color flashColor = (target - _currentValue) > 1000
                ? _criticalColor : _flashColor;

            _currentSeq = DOTween.Sequence()
                // 阶段1：数字滚动
                .Append(
                    DOTween.To(
                        () => _currentValue,
                        x =>
                        {
                            _currentValue = x;
                            _displayText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                        },
                        target, dur
                    ).SetEase(Ease.OutBack)
                )
                // 阶段2：停稳瞬间 — 颜色闪烁 + 缩放弹跳
                .AppendCallback(() =>
                {
                    _currentValue = target;
                    _displayText.text = target.ToString(_numberFormat);
                })
                .Append(
                    _displayText.DOColor(flashColor, 0.08f)
                )
                .Join(
                    transform.DOPunchScale(Vector3.one * (_punchScale - 1f),
                        _punchDuration, _punchVibrato, 0.5f)
                )
                // 阶段3：颜色渐回
                .Append(
                    _displayText.DOColor(_defaultColor, 0.3f)
                )
                .SetUpdate(true);
        }

        /// <summary>
        /// 暴击/大额得分动画：滚动 + 震动 + 弹跳 + 颜色闪烁
        /// </summary>
        public void AnimateCritical(float target, float? duration = null)
        {
            float dur = duration ?? _defaultDuration * 1.2f;
            _currentSeq?.Kill();
            ResetVisual();

            _currentSeq = DOTween.Sequence()
                // 数字滚动（更慢，更有分量）
                .Append(
                    DOTween.To(
                        () => _currentValue,
                        x =>
                        {
                            _currentValue = x;
                            _displayText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                        },
                        target, dur
                    ).SetEase(Ease.OutElastic)
                )
                // 停稳瞬间 — 多重反馈叠加
                .AppendCallback(() =>
                {
                    _currentValue = target;
                    _displayText.text = target.ToString(_numberFormat);
                })
                .Append(
                    _displayText.DOColor(_criticalColor, 0.08f)
                )
                .Join(
                    transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 10, 1f)
                )
                .Join(
                    ((RectTransform)transform).DOShakeAnchorPos(
                        _shakeDuration, _shakeStrength, _shakeVibrato, 90f, false, true)
                )
                // 颜色渐回
                .Append(
                    _displayText.DOColor(_defaultColor, 0.5f)
                )
                .SetUpdate(true);
        }

        private void ResetVisual()
        {
            transform.localScale = _originalScale;
            ((RectTransform)transform).anchoredPosition = _originalPos;
            _displayText.color = _defaultColor;
        }

        public void Stop()
        {
            _currentSeq?.Kill();
            ResetVisual();
        }

        private void OnDestroy()
        {
            _currentSeq?.Kill();
        }
    }
}
```

---

## 四、使用示例

### Cursor 指令：在 GameManager 中调用

```csharp
// 挂载引用
[SerializeField] private ScoreCounter _scoreCounter;
[SerializeField] private ScoreAnimatorAdvanced _scoreAdvanced;

// 普通加分
_scoreCounter.AnimateTo(1000);

// 根据分数档位自动选缓动
float delta = newScore - _scoreCounter.CurrentValue;
Ease ease = EasePresets.AutoSelect(delta);
_scoreCounter.AnimateTo(newScore, ease: ease);

// 暴击加分
_scoreAdvanced.AnimateCritical(5000);

// 扣分
_scoreCounter.AnimateDown(200);
```

---

## 五、缓动曲线速查

在 Unity Editor 中可视化对比缓动曲线：

```
打开 DOTween Utility Panel（Tools → Demigiant → DOTween Utility Panel）
→ 点击 "Visual Editor" → 拖动曲线节点对比效果
```

推荐组合：

| 场景 | Ease | Duration | Punch |
|------|------|----------|-------|
| 小额得分 (+1~+99) | OutBack | 0.3s | 1.05x |
| 中等得分 (+100~+999) | OutBounce | 0.5s | 1.1x |
| 大额得分 (+1000+) | OutElastic | 0.8s | 1.2x |
| 暴击 | OutElastic + Shake | 1.0s | 1.3x |

---

## 六、进阶：叠加小程序计数器效果

小丑牌的数字效果中，有一个关键细节：**高位数字先停，低位数字后停**。

如果需要这种效果（仅适用于纯展示，不适用于实时分数）：

```csharp
// 逐位停止动画（小程序计数器风格）
private IEnumerator SlotMachineStyle(int target)
{
    int digits = target.ToString().Length;
    int current = 0;

    for (int d = digits - 1; d >= 0; d--)
    {
        int place = (int)Mathf.Pow(10, d);
        int targetDigit = (target / place) % 10;
        float spinTime = 0.15f * (d + 1);

        float elapsed = 0f;
        int spinCount = 0;
        while (elapsed < spinTime)
        {
            current = (current / place * place)
                + ((spinCount % 10) * place)
                + (current % place);
            _displayText.text = current.ToString(_numberFormat);
            spinCount++;
            elapsed += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        current = (current / place * place)
            + (targetDigit * place)
            + (current % place);
        _displayText.text = current.ToString(_numberFormat);
        yield return null;
    }
}
```

---

## 七、常见问题

| 问题 | 解决 |
|------|------|
| 数值为 0 时不显示 | 改用 `"N0"` 格式字符串 |
| 小数闪烁 | `Mathf.FloorToInt()` 确保整数 |
| Time.timeScale=0 时不动 | 加 `.SetUpdate(true)` |
| 连续调用动画冲突 | 用 `.Kill()` 终止上一个 Tween |
| 数字显示带小数 | 改为 `"F0"` 格式 |
