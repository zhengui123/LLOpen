# 方案 C：Balatro-Feel 参考复刻

> **费用**：免费 MIT | **难度**：中高 | **耗时**：2 小时（学习+移植） | **效果**：最接近小丑牌原生手感

---

## 一、方案概览

[mixandjam/balatro-feel](https://github.com/mixandjam/balatro-feel) 是 Mix and Jam 频道第 32 期节目的官方代码仓库，由 André Cardoso 制作，719 Star，MIT 开源。

它是一个**完整示例项目**（不是插件），在 Unity 中精确复刻了 Balatro 的：

- 卡牌拖拽、旋转、回弹手感
- **数字分数滚动动画**
- 屏幕震动
- 粒子特效时序
- UI 缩放反馈

**优点**：直接看 Balatro 级别的实现细节、有时序参考、有配套视频教程  
**缺点**：不能当插件拖入项目、需要自己理解后移植核心代码  
**最佳定位**：作为"手感参考"和"时序模板"，提取核心逻辑嫁接到你自己的项目

---

## 二、前置准备

### Cursor 指令：下载项目

```bash
# 在任意位置（非你的项目内）克隆
cd ~/Downloads
git clone https://github.com/mixandjam/balatro-feel.git
```

### Cursor 指令：用 Unity 打开参考项目

```
1. 打开 Unity Hub → Add → 选择 ~/Downloads/balatro-feel 文件夹
2. Unity 版本要求：2022.3+（你的 2022.3 LTS 可以直接打开）
3. 打开后，找到 Scenes 文件夹中的示例场景，点 Play 看效果
```

### Cursor 指令：找到核心脚本

```
在参考项目的 Assets 目录中，重点关注以下脚本：
- 与 "score" / "points" / "chip" 相关的 C# 文件
- 使用 DOTween 的脚本（搜索 "DOTween" / "DO"）
- 音频触发脚本
```

---

## 三、从 Balatro-Feel 提取的核心时序

经过对 Balatro 实际游戏和参考项目的分析，数字增长的标准时序如下：

```
┌─────────────────────────────────────────────────┐
│ 阶段 1：预备（0-50ms）                           │
│  数字略微缩小（0.95x），制造"蓄力"感              │
├─────────────────────────────────────────────────┤
│ 阶段 2：滚动（50ms-600ms）                       │
│  数字从当前值滚向目标值                           │
│  缓动曲线：OutBack（小数）/ OutElastic（大数）    │
│  高位数字先停，低位数字后停（小程序风格）           │
├─────────────────────────────────────────────────┤
│ 阶段 3：冲击（600ms 瞬间）                       │
│  数字瞬间放大到 1.2x                              │
│  颜色变为亮金色/亮白色                            │
│  触发音效（叮）                                  │
├─────────────────────────────────────────────────┤
│ 阶段 4：回弹（600-800ms）                        │
│  缩放回弹到 1.0x（弹性动画）                       │
│  颜色渐变回默认色                                 │
│  屏幕微震（大额得分时）                           │
├─────────────────────────────────────────────────┤
│ 阶段 5：保持（800ms+）                           │
│  恢复正常显示                                     │
│  粒子特效淡出                                    │
└─────────────────────────────────────────────────┘
```

---

## 四、Cursor 指令：移植核心时序到你的项目

### 步骤 1：创建 Balatro 风格动画组件

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 BalatroScoreAnimator.cs。
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
    /// 参考 mixandjam/balatro-feel 的时序实现的分数动画组件。
    /// 精确复刻 5 阶段时序：蓄力 → 滚动 → 冲击 → 回弹 → 保持
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class BalatroScoreAnimator : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private RectTransform _rectTransform;

        [Header("阶段 1：蓄力")]
        [SerializeField] private float _anticipationScale = 0.95f;
        [SerializeField] private float _anticipationDuration = 0.05f;

        [Header("阶段 2：滚动")]
        [SerializeField] private float _scrollDuration = 0.6f;
        [SerializeField] private Ease _scrollEaseNormal = Ease.OutBack;
        [SerializeField] private Ease _scrollEaseCritical = Ease.OutElastic;
        [SerializeField] private string _numberFormat = "N0";

        [Header("阶段 3-4：冲击 + 回弹")]
        [SerializeField] private float _impactScale = 1.25f;
        [SerializeField] private float _impactDuration = 0.08f;
        [SerializeField] private float _recoverDuration = 0.3f;
        [SerializeField] private Ease _recoverEase = Ease.OutElastic;

        [Header("颜色")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _impactColor = new Color(1f, 0.9f, 0.3f); // 金色
        [SerializeField] private Color _criticalColor = new Color(1f, 0.3f, 0.05f); // 橙红

        [Header("震动（大额得分）")]
        [SerializeField] private float _shakeThreshold = 500f;
        [SerializeField] private float _shakeStrength = 8f;
        [SerializeField] private float _shakeDuration = 0.4f;
        [SerializeField] private int _shakeVibrato = 20;

        [Header("音效")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _tickSound;
        [SerializeField] private AudioClip _impactSound;
        [SerializeField] private AudioClip _criticalSound;

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
        /// 标准 Balatro 5 阶段动画
        /// </summary>
        public void AnimateTo(float target, bool isCritical = false,
            Action onComplete = null)
        {
            _currentSequence?.Kill();
            ResetVisual();

            float delta = target - _currentValue;
            bool useBigImpact = isCritical || Mathf.Abs(delta) >= _shakeThreshold;

            float scrollDur = useBigImpact ? _scrollDuration * 1.3f : _scrollDuration;
            Ease scrollEase = useBigImpact ? _scrollEaseCritical : _scrollEaseNormal;
            Color impactC = useBigImpact ? _criticalColor : _impactColor;

            AudioClip impactClip = useBigImpact ? _criticalSound : _impactSound;

            _currentSequence = DOTween.Sequence();

            // === 阶段 1：蓄力（微缩） ===
            _currentSequence.Append(
                transform.DOScale(_originalScale * _anticipationScale,
                    _anticipationDuration)
                .SetEase(Ease.InQuad)
            );

            // === 阶段 2：数字滚动 + 恢复缩放 ===
            _currentSequence.Append(
                DOTween.To(
                    () => _currentValue,
                    x =>
                    {
                        _currentValue = x;
                        _displayText.text = Mathf.FloorToInt(x).ToString(_numberFormat);
                    },
                    target, scrollDur
                ).SetEase(scrollEase)
            );
            // 滚动同时恢复缩放
            _currentSequence.Join(
                transform.DOScale(_originalScale, _anticipationDuration * 2)
                .SetEase(Ease.OutQuad)
            );

            // === 阶段 3：冲击 ===
            _currentSequence.AppendCallback(() =>
            {
                _currentValue = target;
                _displayText.text = target.ToString(_numberFormat);
            });
            _currentSequence.Append(
                _displayText.DOColor(impactC, _impactDuration)
            );
            _currentSequence.Join(
                transform.DOScale(_originalScale * _impactScale, _impactDuration)
                .SetEase(Ease.OutQuad)
            );
            _currentSequence.AppendCallback(() =>
            {
                PlaySound(impactClip);
            });

            // === 阶段 4：回弹 ===
            _currentSequence.Append(
                transform.DOScale(_originalScale, _recoverDuration)
                .SetEase(_recoverEase)
            );
            _currentSequence.Join(
                _displayText.DOColor(_defaultColor, _recoverDuration * 0.8f)
            );

            // 震动（大额得分）
            if (useBigImpact)
            {
                _currentSequence.Join(
                    _rectTransform.DOShakeAnchorPos(
                        _shakeDuration, _shakeStrength,
                        _shakeVibrato, 90f, false, true)
                );
            }

            // === 阶段 5：完成 ===
            _currentSequence.AppendCallback(() =>
            {
                _rectTransform.anchoredPosition = _originalAnchoredPos;
                onComplete?.Invoke();
            });

            _currentSequence.SetUpdate(true);
        }

        /// <summary>
        /// 仅播放冲击反馈（数字不变，纯粹的特效展示）
        /// 适合数字已经正确但需要强调的场景
        /// </summary>
        public void PlayImpactOnly(bool isCritical = false)
        {
            Color impactC = isCritical ? _criticalColor : _impactColor;
            AudioClip clip = isCritical ? _criticalSound : _impactSound;

            Sequence s = DOTween.Sequence();
            s.Append(
                _displayText.DOColor(impactC, _impactDuration * 1.5f)
            );
            s.Join(
                transform.DOPunchScale(Vector3.one * (_impactScale - 1f),
                    _recoverDuration, 5, 0.5f)
            );
            s.Append(
                _displayText.DOColor(_defaultColor, _recoverDuration)
            );
            s.SetUpdate(true);
            PlaySound(clip);
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
}
```

### 步骤 2：添加屏幕震动管理器

```
在 Assets/Scripts/UI/ 下新建 C# 脚本，命名为 ScreenShake.cs。
粘贴下方代码。
```

**完整代码：**

```csharp
using UnityEngine;
using DG.Tweening;

namespace GTest.UI
{
    /// <summary>
    /// 全局屏幕震动管理器。
    /// 挂在 Canvas 或 Camera 上。
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake _instance;
        public static ScreenShake Instance => _instance;

        [SerializeField] private RectTransform _canvasRect;
        private Vector3 _originalPos;
        private Tween _shakeTween;

        private void Awake()
        {
            _instance = this;
            if (_canvasRect == null)
                _canvasRect = GetComponent<RectTransform>();
            _originalPos = _canvasRect.anchoredPosition;
        }

        /// <summary>
        /// 触发屏幕震动
        /// </summary>
        public void Shake(float strength = 5f, float duration = 0.3f,
            int vibrato = 10)
        {
            _shakeTween?.Kill();
            _canvasRect.anchoredPosition = _originalPos;

            _shakeTween = _canvasRect
                .DOShakeAnchorPos(duration, strength, vibrato, 90f, false, true)
                .OnComplete(() => _canvasRect.anchoredPosition = _originalPos);
        }

        /// <summary>
        /// 根据分数增量自动选择震动强度
        /// </summary>
        public void ShakeByScore(float scoreDelta)
        {
            if (scoreDelta < 100) return;  // 太小不震
            if (scoreDelta < 500) Shake(3f, 0.2f, 8);
            else if (scoreDelta < 2000) Shake(6f, 0.35f, 15);
            else Shake(10f, 0.5f, 25);
        }

        private void OnDestroy()
        {
            _shakeTween?.Kill();
        }
    }
}
```

### 步骤 3：集成音频触发

Balatro 的数字动画有一个容易被忽视的关键：**声音是手感的一半**。

```
在场景中创建一个 AudioSource GameObject：
1. 右键 Hierarchy → Audio → Audio Source
2. 命名为 "SFX_Score"
3. 把 BalatroScoreAnimator 的 Audio Source 字段拖入
4. 准备 3 个音效文件（可暂时用免费音效替代）：
   - tickSound：短促的"嘀"声（数字滚动时）
   - impactSound：清脆的"叮"声（数值停稳）
   - criticalSound：厚重的"铛"声（暴击/大额）
```

**免费音效来源：**
- [freesound.org](https://freesound.org/) 搜索 "coin" "ding" "impact"
- [mixkit.co](https://mixkit.co/free-sound-effects/) 搜索 "game" "score"
- Unity Asset Store 搜索 "Free Sound Effects"

### 步骤 4：创建全局访问入口

```
在 Assets/Scripts/Managers/ 下新建 C# 脚本，命名为 ScoreFXManager.cs。
粘贴下方代码。
```

**完整代码：**

```csharp
using UnityEngine;

namespace GTest
{
    /// <summary>
    /// 全局分数特效管理器。
    /// 任何地方通过 ScoreFXManager.Instance.AnimateScore(...) 调用。
    /// </summary>
    public class ScoreFXManager : MonoBehaviour
    {
        public static ScoreFXManager Instance { get; private set; }

        [SerializeField] private BalatroScoreAnimator _scoreDisplay;
        [SerializeField] private ScreenShake _screenShake;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 加分动画（一站式调用）
        /// </summary>
        public void AddScore(float amount, float newTotal, bool isCritical = false)
        {
            // 屏幕震动
            if (_screenShake != null)
                _screenShake.ShakeByScore(amount);

            // 数字动画
            if (_scoreDisplay != null)
                _scoreDisplay.AnimateTo(newTotal, isCritical);
            else
                Debug.LogWarning("[ScoreFXManager] ScoreDisplay 未绑定");
        }

        /// <summary>
        /// 仅播放冲击特效（数字不变）
        /// </summary>
        public void PlayImpact(bool isCritical = false)
        {
            _scoreDisplay?.PlayImpactOnly(isCritical);
        }
    }
}
```

---

## 五、实际使用

### Cursor 指令：在场景中搭建

```
1. 创建 Canvas 下的 Score Text（TMP）
2. 给它挂 BalatroScoreAnimator 组件
3. 创建空 GameObject "ScoreFXManager"，挂 ScoreFXManager 组件
4. 把 Score Text 拖到 ScoreFXManager 的 Score Display 字段
5. 把 Canvas 拖到 ScoreFXManager 的 Screen Shake 字段（Canvas 需要挂 ScreenShake 组件）
```

### Cursor 指令：在游戏逻辑中调用

```csharp
// 普通加分
ScoreFXManager.Instance.AddScore(50, totalScore: 1050);

// 暴击加倍
ScoreFXManager.Instance.AddScore(500, totalScore: 6000, isCritical: true);

// 只震动不改变数字
ScoreFXManager.Instance.PlayImpact(isCritical: true);
```

---

## 六、配套视频教程

B 站有搬运的中文版，46 分钟讲透所有细节：

```
https://www.bilibili.com/video/BV1WJ4m1E7Ct
标题：【Unity教程搬运】重现Balatro（小丑牌）的游戏手感 | Mix and Jam
```

建议配合上面的代码一起看，理解每一步的设计意图。

---

## 七、从参考项目中还能学到什么

打开 `balatro-feel` 项目后，关注这些细节：

| 文件/场景 | 学什么 |
|----------|--------|
| 卡牌拖拽脚本 | `OnDrag` 中 DOTween 的用法 |
| 粒子特效 Prefab | 得分飘出的粒子系统结构 |
| DOTween 设置 | `.SetUpdate(true)`、`.SetAutoKill()` 等关键 API |
| 音频触发时机 | 音效与动画帧的精确对齐 |
| 屏幕震动参数 | `DOShakeAnchorPos` 的具体参数 |

---

## 八、常见问题

| 问题 | 解决 |
|------|------|
| Sequence 不播放 | 检查是否被 `.Kill()` 误杀；确保 `SetUpdate(true)` 即使暂停也播 |
| 震动后 Canvas 位置偏移 | `OnComplete` 中手动恢复 `originalPos` |
| 连续快速加分时动画重叠 | 用 `_currentSequence?.Kill()` 终止上一个 |
| 音效缺失 | 用免费的 placeholder 音效替代，后续再换 |
