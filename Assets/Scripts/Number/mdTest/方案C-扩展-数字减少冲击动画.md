# 方案 C 扩展：数字减少冲击动画

> **基于**：方案 C Balatro-Feel 参考复刻 | **费用**：免费 | **难度**：中等 | **耗时**：20 分钟移植

---

## 一、设计理念

扣分不等于"动画反过来放"。

加分的视觉语言是"获得"——弹跳、膨胀、金色。扣分的视觉语言是"失去"——坠落、紧缩、红色。两者的缓动曲线、时序节奏、色彩语言完全不同。

以下设计参考了 Balatro 中盲注失败、Roguelike 游戏扣血的通用做法。

**核心差异：**

| 维度 | 加分 | 扣分 |
|------|------|------|
| 方向 | 向上弹跳 | 向下坠落 |
| 颜色 | 金色/亮白 | 血红/暗红 |
| 缓动 | OutBack（超出回弹） | InBack（先缩再坠） |
| 震动 | 轻颤 | 重击 |
| 缩放 | 膨胀（1.2x） | 紧缩（0.8x）再弹回 |
| 音效 | 清脆"叮" | 低沉"咚" |
| 时序 | 蓄力→滚动→冲击→回弹 | 惊恐→坠落→撞击→喘息→恢复 |

---

## 二、扣分 5 阶段时序

```
┌─────────────────────────────────────────────────┐
│ 阶段 1：惊恐定格（0-100ms）                       │
│  数字瞬间放大 1.15x，颜色变血红                    │
│  短暂定格，制造"即将失去"的紧张感                  │
│  屏幕微震（预警）                                 │
├─────────────────────────────────────────────────┤
│ 阶段 2：快速坠落（100-500ms）                     │
│  数字快速滚向目标值                               │
│  缓动曲线：InBack — 先向后缩再加速坠落             │
│  数字向下位移 8-15px                             │
│  红色保持                                        │
├─────────────────────────────────────────────────┤
│ 阶段 3：撞击（500ms 瞬间）                        │
│  数字触底，缩放紧缩到 0.85x                        │
│  颜色瞬间变暗红/黑红                              │
│  触发重击音效                                    │
│  屏幕剧烈震动（比加分强 2-3 倍）                   │
├─────────────────────────────────────────────────┤
│ 阶段 4：喘息回弹（500-800ms）                     │
│  缩放从 0.85x 弹回 1.0x（弹性动画）               │
│  颜色从暗红渐变回默认色                            │
│  数字从下方回弹到原位                              │
│  震动渐止                                       │
├─────────────────────────────────────────────────┤
│ 阶段 5：恢复（800ms+）                            │
│  正常显示                                         │
│  可选的暗色残留（短暂保持略暗色调再恢复）           │
└─────────────────────────────────────────────────┘
```

---

## 三、Cursor 指令：逐步实现

### 步骤 1：创建扣分动画组件

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 ScoreLossAnimator.cs。
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
    /// 扣分/损失动画组件。
    /// 5阶段时序：惊恐定格 → 快速坠落 → 撞击 → 喘息回弹 → 恢复
    /// 参考方案C的加分动画体系，独立设计扣分的视觉语言。
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScoreLossAnimator : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private RectTransform _rectTransform;

        [Header("阶段 1：惊恐定格")]
        [SerializeField] private float _panicScale = 1.15f;
        [SerializeField] private float _panicDuration = 0.1f;
        [SerializeField] private Color _panicColor = new Color(1f, 0.15f, 0.1f); // 血红

        [Header("阶段 2：快速坠落")]
        [SerializeField] private float _fallDuration = 0.4f;
        [SerializeField] private Ease _fallEase = Ease.InBack;
        [SerializeField] private float _fallOffsetY = -12f;       // 向下位移
        [SerializeField] private string _numberFormat = "N0";

        [Header("阶段 3：撞击")]
        [SerializeField] private float _impactScale = 0.85f;
        [SerializeField] private float _impactDuration = 0.06f;
        [SerializeField] private Color _impactColor = new Color(0.6f, 0.05f, 0.05f); // 暗红

        [Header("阶段 4：喘息回弹")]
        [SerializeField] private float _recoverDuration = 0.35f;
        [SerializeField] private Ease _recoverEase = Ease.OutElastic;
        [SerializeField] private Color _defaultColor = Color.white;

        [Header("震动")]
        [SerializeField] private float _panicShakeStrength = 4f;
        [SerializeField] private float _impactShakeStrength = 12f;
        [SerializeField] private float _shakeDuration = 0.5f;
        [SerializeField] private int _shakeVibrato = 25;

        [Header("音效")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _panicSound;    // 预警音
        [SerializeField] private AudioClip _impactSound;   // 撞击音
        [SerializeField] private AudioClip _lossStartSound; // 开始扣分音

        private float _currentValue;
        private Sequence _currentSequence;
        private Vector3 _originalScale;
        private Vector2 _originalAnchoredPos;

        private void Awake()
        {
            if (_displayText == null) _displayText = GetComponent<TMP_Text>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            _originalScale = transform.localScale;
            _originalAnchoredPos = _rectTransform.anchoredPosition;
            _displayText.color = _defaultColor;
        }

        /// <summary>
        /// 设置初始值（无动画）
        /// </summary>
        public void SetValue(float value)
        {
            _currentValue = value;
            _displayText.text = value.ToString(_numberFormat);
        }

        /// <summary>
        /// 扣分动画（完整 5 阶段）
        /// </summary>
        /// <param name="target">扣分后的目标值</param>
        /// <param name="severity">损失严重程度，影响震动和缩放强度</param>
        public void AnimateLoss(float target, LossSeverity severity = LossSeverity.Normal,
            Action onComplete = null)
        {
            _currentSequence?.Kill();
            ResetVisual();

            float lossAmount = _currentValue - target;
            if (lossAmount <= 0)
            {
                // 没有实际扣分，只更新数字
                SetValue(target);
                onComplete?.Invoke();
                return;
            }

            // 根据严重程度调整参数倍率
            float severityMult = GetSeverityMultiplier(severity);

            float panicScale = 1f + (_panicScale - 1f) * severityMult;
            float impactScale = 1f - (1f - _impactScale) * severityMult;
            float fallOffset = _fallOffsetY * severityMult;
            float shakeStr = _impactShakeStrength * severityMult;

            _currentSequence = DOTween.Sequence();

            // ===== 阶段 1：惊恐定格 =====
            _currentSequence.AppendCallback(() =>
            {
                PlaySound(_panicSound);
            });
            _currentSequence.Append(
                _displayText.DOColor(_panicColor, _panicDuration * 0.5f)
            );
            _currentSequence.Join(
                transform.DOScale(_originalScale * panicScale, _panicDuration)
                .SetEase(Ease.OutQuad)
            );
            // 微震预警
            _currentSequence.Join(
                _rectTransform.DOShakeAnchorPos(
                    _panicDuration, _panicShakeStrength, 8, 90f, false, true)
            );

            // ===== 阶段 2：快速坠落 =====
            _currentSequence.AppendCallback(() =>
            {
                PlaySound(_lossStartSound);
            });
            _currentSequence.Append(
                DOTween.To(
                    () => _currentValue,
                    x =>
                    {
                        _currentValue = x;
                        _displayText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                    },
                    target,
                    _fallDuration
                ).SetEase(_fallEase)
            );
            // 坠落时数字向下位移
            _currentSequence.Join(
                _rectTransform.DOAnchorPos(
                    _originalAnchoredPos + new Vector2(0, fallOffset),
                    _fallDuration
                ).SetEase(Ease.InQuad)
            );
            // 坠落时逐渐变暗
            _currentSequence.Join(
                _displayText.DOColor(_panicColor, _fallDuration * 0.3f)
            );

            // ===== 阶段 3：撞击 =====
            _currentSequence.AppendCallback(() =>
            {
                _currentValue = target;
                _displayText.text = target.ToString(_numberFormat);
            });
            _currentSequence.Append(
                _displayText.DOColor(_impactColor, _impactDuration)
            );
            _currentSequence.Join(
                transform.DOScale(_originalScale * impactScale, _impactDuration)
                .SetEase(Ease.InQuad)
            );
            _currentSequence.Join(
                _rectTransform.DOShakeAnchorPos(
                    _shakeDuration, shakeStr, _shakeVibrato, 90f, false, true)
            );
            _currentSequence.AppendCallback(() =>
            {
                PlaySound(_impactSound);
            });

            // ===== 阶段 4：喘息回弹 =====
            _currentSequence.Append(
                transform.DOScale(_originalScale, _recoverDuration)
                .SetEase(_recoverEase)
            );
            _currentSequence.Join(
                _rectTransform.DOAnchorPos(
                    _originalAnchoredPos, _recoverDuration * 0.8f
                ).SetEase(Ease.OutBack)
            );
            _currentSequence.Join(
                _displayText.DOColor(_defaultColor, _recoverDuration)
            );

            // ===== 阶段 5：恢复 =====
            _currentSequence.AppendCallback(() =>
            {
                _rectTransform.anchoredPosition = _originalAnchoredPos;
                transform.localScale = _originalScale;
                _displayText.color = _defaultColor;
                onComplete?.Invoke();
            });

            _currentSequence.SetUpdate(true);
        }

        /// <summary>
        /// 仅播放撞击效果（数字不变，纯视觉反馈）
        /// 适合外部伤害、环境扣血等场景
        /// </summary>
        public void PlayImpactOnly(LossSeverity severity = LossSeverity.Normal)
        {
            float mult = GetSeverityMultiplier(severity);
            float impactScale = 1f - (1f - _impactScale) * mult;

            Sequence s = DOTween.Sequence();
            s.Append(
                _displayText.DOColor(_impactColor, _impactDuration * 1.5f)
            );
            s.Join(
                transform.DOPunchScale(
                    Vector3.one * (impactScale - 1f),
                    0.3f, 8, 0.5f)
            );
            s.Join(
                _rectTransform.DOShakeAnchorPos(
                    0.4f, _impactShakeStrength * mult, 20, 90f, false, true)
            );
            s.Append(
                _displayText.DOColor(_defaultColor, _recoverDuration)
            );
            s.Join(
                transform.DOScale(_originalScale, _recoverDuration)
            );
            s.SetUpdate(true);

            PlaySound(_impactSound);
        }

        /// <summary>
        /// 连续扣分（如中毒持续掉血）
        /// </summary>
        public void AnimateContinuousLoss(float target, float intervalSeconds,
            float lossPerTick, Action onComplete = null)
        {
            _currentSequence?.Kill();

            _currentSequence = DOTween.Sequence();
            float tempTarget = _currentValue;

            while (tempTarget > target)
            {
                tempTarget = Mathf.Max(tempTarget - lossPerTick, target);
                float capturedTarget = tempTarget;

                // 每次 Tick 只做快速撞击（不做完整 5 阶段）
                _currentSequence.AppendCallback(() =>
                {
                    _currentValue = capturedTarget;
                    _displayText.text = capturedTarget.ToString(_numberFormat);
                });
                _currentSequence.Append(
                    _displayText.DOColor(_impactColor, 0.05f)
                );
                _currentSequence.Join(
                    transform.DOPunchScale(Vector3.one * -0.05f, 0.1f, 3, 0.5f)
                );
                _currentSequence.Append(
                    _displayText.DOColor(_defaultColor, 0.1f)
                );
                _currentSequence.AppendInterval(intervalSeconds);
            }

            _currentSequence.AppendCallback(() =>
            {
                onComplete?.Invoke();
            });
            _currentSequence.SetUpdate(true);
        }

        private float GetSeverityMultiplier(LossSeverity severity)
        {
            return severity switch
            {
                LossSeverity.Light => 0.6f,
                LossSeverity.Normal => 1.0f,
                LossSeverity.Heavy => 1.4f,
                LossSeverity.Critical => 2.0f,
                _ => 1.0f
            };
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private void ResetVisual()
        {
            transform.localScale = _originalScale;
            _rectTransform.anchoredPosition = _originalAnchoredPos;
            _displayText.color = _defaultColor;
        }

        public void Stop()
        {
            _currentSequence?.Kill();
            ResetVisual();
        }

        private void OnDestroy()
        {
            _currentSequence?.Kill();
        }
    }

    /// <summary>
    /// 损失严重程度
    /// </summary>
    public enum LossSeverity
    {
        Light,      // 轻微（小额扣分）
        Normal,     // 普通
        Heavy,      // 重度（大额扣分）
        Critical    // 致命（濒死/破产级别）
    }
}
```

---

## 四、Cursor 指令：配套屏幕震动扩展

### 步骤 2：震动管理器增加扣分模式

```
打开方案C中创建的 Assets/Scripts/UI/ScreenShake.cs。
在类中添加以下方法：
```

```csharp
/// <summary>
/// 扣分专用震动（比加分更剧烈、频率更低，模拟重击感）
/// </summary>
public void ShakeLoss(LossSeverity severity = LossSeverity.Normal)
{
    _shakeTween?.Kill();
    _canvasRect.anchoredPosition = _originalPos;

    (float strength, float duration, int vibrato) = severity switch
    {
        LossSeverity.Light => (4f, 0.25f, 10),
        LossSeverity.Normal => (8f, 0.4f, 18),
        LossSeverity.Heavy => (14f, 0.55f, 25),
        LossSeverity.Critical => (22f, 0.7f, 35),
        _ => (8f, 0.4f, 18)
    };

    // 扣分震动使用更低的随机性（randomness: 60），
    // 制造"重击"感而非"抖动"感
    _shakeTween = _canvasRect
        .DOShakeAnchorPos(duration, strength, vibrato, 60f, false, true)
        .OnComplete(() => _canvasRect.anchoredPosition = _originalPos);
}
```

---

## 五、Cursor 指令：集成到全局管理器

### 步骤 3：扩展 ScoreFXManager

```
打开方案C中创建的 ScoreFXManager.cs，添加扣分相关方法：
```

```csharp
[Header("扣分")]
[SerializeField] private ScoreLossAnimator _scoreLossDisplay;

/// <summary>
/// 扣分动画
/// </summary>
public void SubtractScore(float amount, float newTotal,
    LossSeverity severity = LossSeverity.Normal)
{
    // 根据扣分金额自动判定严重程度
    if (severity == LossSeverity.Normal)
    {
        if (amount >= 2000) severity = LossSeverity.Critical;
        else if (amount >= 500) severity = LossSeverity.Heavy;
    }

    // 屏幕震动
    if (_screenShake != null)
        _screenShake.ShakeLoss(severity);

    // 数字动画
    if (_scoreLossDisplay != null)
        _scoreLossDisplay.AnimateLoss(newTotal, severity);
}

/// <summary>
/// 仅播放扣分冲击（不改变数值）
/// </summary>
public void PlayLossImpact(LossSeverity severity = LossSeverity.Normal)
{
    _screenShake?.ShakeLoss(severity);
    _scoreLossDisplay?.PlayImpactOnly(severity);
}
```

---

## 六、使用示例

### 基础调用

```csharp
// 普通扣血
ScoreFXManager.Instance.SubtractScore(50, newTotal: 950);

// 大额损失（自动判定为 Heavy/Critical）
ScoreFXManager.Instance.SubtractScore(3000, newTotal: 2000);

// 强制指定严重程度
ScoreFXManager.Instance.SubtractScore(100, newTotal: 900,
    severity: LossSeverity.Critical); // 100 分但用暴击级震动，适合剧情杀
```

### 特殊场景

```csharp
// 1. 中毒持续掉血（每秒扣 10，扣到 0）
_lossAnimator.AnimateContinuousLoss(
    target: 0,
    intervalSeconds: 1.0f,
    lossPerTick: 10,
    onComplete: () => Debug.Log("HP 归零")
);

// 2. 外部伤害（数字不变，只震+闪）
ScoreFXManager.Instance.PlayLossImpact(LossSeverity.Heavy);

// 3. 濒死状态（扣到 0 时用 Critical 震动 + 画面变红）
_scoreLossDisplay.AnimateLoss(0, LossSeverity.Critical, () =>
{
    // 扣完后触发死亡画面
    StartCoroutine(DeathSequence());
});
```

---

## 七、严重程度分级指南

| 等级 | 判定阈值 | 缩放 | 震动 | 位移 | 适用场景 |
|------|---------|------|------|------|---------|
| Light | < 5% 总资产 | 0.95x | 4 | -6px | 零钱扣除、小额手续费 |
| Normal | 5%-15% | 0.85x | 8 | -12px | 普通伤害、一般损失 |
| Heavy | 15%-40% | 0.75x | 14 | -18px | 大额伤害、关键资源丢失 |
| Critical | > 40% 或归零 | 0.65x | 22 | -24px | 濒死、破产、剧情杀 |

---

## 八、加分 vs 扣分对比速查

| 阶段 | 加分 | 扣分 |
|------|------|------|
| 预备 | 蓄力微缩（0.95x） | 惊恐放大（1.15x）+ 变红 |
| 滚动 | OutBack 弹跳，向上 | InBack 坠落，向下位移 |
| 冲击 | 膨胀（1.2x）+ 金色 | 紧缩（0.85x）+ 暗红 |
| 回弹 | 弹性回缩 | 弹性弹回 |
| 震动 | 频率高、幅度小（"抖动"） | 频率低、幅度大（"重击"） |
| 音效 | 清脆"叮" | 低沉"咚" |

---

## 九、音效推荐

扣分音效的关键词：（在 freesound.org / mixkit.co 搜索）

| 音效 | 搜索关键词 | 感觉 |
|------|-----------|------|
| 预警音 | "alarm" "warning beep" | 紧张的嘀嘀声 |
| 开始扣分 | "whoosh down" "descend" | 急速下落 |
| 撞击音 | "heavy thud" "impact deep" | 沉重的撞击 |
| 破碎音（Critical） | "glass break" "shatter" | 碎裂/破产 |

---

## 十、常见问题

| 问题 | 解决 |
|------|------|
| 扣分后数字位置偏移 | `OnComplete` 中强制还原 `anchoredPosition` |
| 连续扣分动画重叠 | `_currentSequence?.Kill()` 终止上一个 |
| 扣分为 0 时动画仍播放 | 开头判断 `lossAmount > 0` |
| 扣分震动太强不适 | 降低 `_impactShakeStrength`（默认 12 → 6） |
| 与加分动画共享同一个 Text | 加分和扣分各用独立组件实例，互不干扰 |
