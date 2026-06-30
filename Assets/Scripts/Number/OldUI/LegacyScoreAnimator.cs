using System;
using UnityEngine;
using DG.Tweening;
using GTest.UI;

namespace GTest.OldUI
{
    /// <summary>
    /// 旧版 Text 数字增减动画。
    /// 增减共用 LossMotionProfile 时序与振动；加分镜像缩放/位移，配色独立。
    /// </summary>
    public class LegacyScoreAnimator : LegacyScoreMotionBase
    {
        [Header("运动参数（增减共用）")]
        [SerializeField] private LossMotionProfile _loss = LossMotionProfile.CreateDefault();

        [Header("加分配色与音效")]
        [SerializeField] private GainColorProfile _gain = GainColorProfile.CreateDefault();

        public void AnimateGain(float target, bool isCritical = false, Action onComplete = null)
        {
            _currentSequence?.Kill();
            ResetVisual();

            float startValue = _currentValue;
            float gainAmount = target - startValue;
            if (gainAmount <= 0f)
            {
                SetValue(target);
                onComplete?.Invoke();
                return;
            }

            LossSeverity severity = ResolveSeverity(gainAmount, isCritical);
            PlayMotionSequence(startValue, target, BuildPlayParams(severity, isGain: true), onComplete);
        }

        public void AnimateLoss(float target, LossSeverity severity = LossSeverity.Normal,
            Action onComplete = null)
        {
            _currentSequence?.Kill();
            ResetVisual();

            float startValue = _currentValue;
            float lossAmount = startValue - target;
            if (lossAmount <= 0f)
            {
                SetValue(target);
                onComplete?.Invoke();
                return;
            }

            PlayMotionSequence(startValue, target, BuildPlayParams(severity, isGain: false), onComplete);
        }

        public void PlayGainImpact(bool isCritical = false)
        {
            LossSeverity severity = isCritical ? LossSeverity.Critical : LossSeverity.Normal;
            float mult = GetSeverityMultiplier(severity);
            float impactScale = MirrorScale(1f - (1f - _loss.impactScale) * mult);
            Color impactC = GetGainImpactColor(severity);
            AudioClip clip = ResolveGainSound(
                isCritical ? _gain.criticalSound : _gain.impactSound,
                isCritical ? null : _loss.impactSound);

            Sequence s = DOTween.Sequence();
            s.Append(_displayText.DOColor(impactC, _loss.impactDuration * 1.5f));
            s.Join(transform.DOPunchScale(
                Vector3.one * (impactScale - 1f), _loss.recoverDuration, 5, 0.5f));
            s.Append(_displayText.DOColor(_defaultColor, _loss.recoverDuration));
            s.SetUpdate(true);
            PlaySound(clip);
        }

        public void PlayLossImpact(LossSeverity severity = LossSeverity.Normal)
        {
            float mult = GetSeverityMultiplier(severity);
            float impactScale = 1f - (1f - _loss.impactScale) * mult;
            Color impactC = GetLossImpactColor(severity);

            Sequence s = DOTween.Sequence();
            s.Append(_displayText.DOColor(impactC, _loss.impactDuration * 1.5f));
            s.Join(transform.DOPunchScale(
                Vector3.one * (impactScale - 1f), 0.3f, 8, 0.5f));
            s.Join(_rectTransform.DOShakeAnchorPos(
                0.4f, _loss.impactShakeStrength * mult, 20, 90f, false, true));
            s.Append(_displayText.DOColor(_defaultColor, _loss.recoverDuration));
            s.Join(transform.DOScale(_originalScale, _loss.recoverDuration));
            s.SetUpdate(true);
            PlaySound(_loss.impactSound);
        }

        public void AnimateContinuousLoss(float target, float intervalSeconds,
            float lossPerTick, Action onComplete = null)
        {
            _currentSequence?.Kill();
            float startValue = _currentValue;
            _currentSequence = DOTween.Sequence();
            float tempTarget = startValue;

            while (tempTarget > target)
            {
                tempTarget = Mathf.Max(tempTarget - lossPerTick, target);
                float captured = tempTarget;

                _currentSequence.AppendCallback(() =>
                {
                    _currentValue = captured;
                    UpdateDisplayText(captured);
                });
                _currentSequence.Append(_displayText.DOColor(_loss.impactColor, 0.05f));
                _currentSequence.Join(transform.DOPunchScale(Vector3.one * -0.05f, 0.1f, 3, 0.5f));
                _currentSequence.Append(_displayText.DOColor(_defaultColor, 0.1f));
                _currentSequence.AppendInterval(intervalSeconds);
            }

            _currentSequence.AppendCallback(() => onComplete?.Invoke());
            _currentSequence.SetUpdate(true);
        }

        private Tween BuildNumberTween(float from, float target, float duration)
        {
            float min = Mathf.Min(from, target);
            float max = Mathf.Max(from, target);
            float animValue = from;

            var tween = DOTween.To(
                () => animValue,
                x =>
                {
                    animValue = x;
                    float shown = Mathf.Clamp(x, min, max);
                    _currentValue = shown;
                    UpdateDisplayText(shown);
                },
                target,
                duration);

            tween.OnStart(() =>
            {
                animValue = from;
                _currentValue = from;
                UpdateDisplayText(from);
            });

            return tween;
        }

        private void PlayMotionSequence(float startValue, float target, MotionPlayParams p,
            Action onComplete)
        {
            _currentSequence = DOTween.Sequence();

            _currentSequence.AppendCallback(() => PlaySound(p.panicSound));
            _currentSequence.Append(_displayText.DOColor(p.panicColor, p.panicDuration * 0.5f));
            _currentSequence.Join(
                transform.DOScale(_originalScale * p.panicScale, p.panicDuration).SetEase(Ease.OutQuad));
            _currentSequence.Join(_rectTransform.DOShakeAnchorPos(
                p.panicDuration, p.panicShakeStrength, 8, 90f, false, true));

            _currentSequence.AppendCallback(() => PlaySound(p.scrollSound));
            _currentSequence.Append(BuildNumberTween(startValue, target, p.scrollDuration).SetEase(p.scrollEase));
            _currentSequence.Join(_rectTransform.DOAnchorPos(
                _originalAnchoredPos + new Vector2(0f, p.scrollOffsetY), p.scrollDuration)
                .SetEase(Ease.InQuad));
            _currentSequence.Join(
                _displayText.DOColor(p.panicColor, p.scrollDuration * 0.3f));

            _currentSequence.AppendCallback(() =>
            {
                _currentValue = target;
                UpdateDisplayText(target);
            });
            _currentSequence.Append(_displayText.DOColor(p.impactColor, p.impactDuration));
            _currentSequence.Join(
                transform.DOScale(_originalScale * p.impactScale, p.impactDuration).SetEase(Ease.InQuad));
            _currentSequence.Join(_rectTransform.DOShakeAnchorPos(
                p.shakeDuration, p.impactShakeStrength, p.shakeVibrato, 90f, false, true));
            _currentSequence.AppendCallback(() => PlaySound(p.impactSound));

            _currentSequence.Append(
                transform.DOScale(_originalScale, p.recoverDuration).SetEase(p.recoverEase));
            _currentSequence.Join(_rectTransform.DOAnchorPos(
                _originalAnchoredPos, p.recoverDuration * 0.8f).SetEase(Ease.OutBack));
            _currentSequence.Join(
                _displayText.DOColor(_defaultColor, p.recoverDuration));
            _currentSequence.AppendCallback(() => FinishMotion(onComplete));
            _currentSequence.SetUpdate(true);
        }

        /// <summary>
        /// 扣分直用 _loss；加分镜像缩放/位移，配色用 _gain。
        /// </summary>
        private MotionPlayParams BuildPlayParams(LossSeverity severity, bool isGain)
        {
            float mult = GetSeverityMultiplier(severity);
            float panicScale = 1f + (_loss.panicScale - 1f) * mult;
            float impactScale = 1f - (1f - _loss.impactScale) * mult;
            float scrollOffsetY = _loss.fallOffsetY * mult;

            if (isGain)
            {
                panicScale = MirrorScale(panicScale);
                impactScale = MirrorScale(impactScale);
                scrollOffsetY = -scrollOffsetY;
            }

            return new MotionPlayParams
            {
                panicScale = panicScale,
                panicDuration = _loss.panicDuration,
                panicColor = isGain ? _gain.panicColor : _loss.panicColor,
                scrollDuration = _loss.fallDuration,
                scrollEase = _loss.fallEase,
                scrollOffsetY = scrollOffsetY,
                impactScale = impactScale,
                impactDuration = _loss.impactDuration,
                impactColor = isGain ? GetGainImpactColor(severity) : GetLossImpactColor(severity),
                recoverDuration = _loss.recoverDuration,
                recoverEase = _loss.recoverEase,
                panicShakeStrength = _loss.panicShakeStrength,
                impactShakeStrength = _loss.impactShakeStrength * mult,
                shakeDuration = _loss.shakeDuration,
                shakeVibrato = _loss.shakeVibrato,
                panicSound = isGain
                    ? ResolveGainSound(_gain.panicSound, _loss.panicSound)
                    : _loss.panicSound,
                scrollSound = isGain
                    ? ResolveGainSound(_gain.scrollSound, _loss.lossStartSound)
                    : _loss.lossStartSound,
                impactSound = isGain
                    ? ResolveGainSound(
                        severity is LossSeverity.Heavy or LossSeverity.Critical
                            ? _gain.criticalSound
                            : _gain.impactSound,
                        _loss.impactSound)
                    : _loss.impactSound
            };
        }

        private static float MirrorScale(float scale) => 2f - scale;

        private static LossSeverity ResolveSeverity(float amount, bool isCritical)
        {
            if (isCritical) return LossSeverity.Critical;
            if (amount >= 2000f) return LossSeverity.Critical;
            if (amount >= 500f) return LossSeverity.Heavy;
            return LossSeverity.Normal;
        }

        private Color GetGainImpactColor(LossSeverity severity)
        {
            return severity is LossSeverity.Heavy or LossSeverity.Critical
                ? _gain.criticalColor
                : _gain.impactColor;
        }

        private Color GetLossImpactColor(LossSeverity severity)
        {
            return severity is LossSeverity.Heavy or LossSeverity.Critical
                ? _loss.criticalImpactColor
                : _loss.impactColor;
        }

        private static AudioClip ResolveGainSound(AudioClip gainClip, AudioClip fallback)
            => gainClip != null ? gainClip : fallback;

        private struct MotionPlayParams
        {
            public float panicScale;
            public float panicDuration;
            public Color panicColor;
            public float scrollDuration;
            public Ease scrollEase;
            public float scrollOffsetY;
            public float impactScale;
            public float impactDuration;
            public Color impactColor;
            public float recoverDuration;
            public Ease recoverEase;
            public float panicShakeStrength;
            public float impactShakeStrength;
            public float shakeDuration;
            public int shakeVibrato;
            public AudioClip panicSound;
            public AudioClip scrollSound;
            public AudioClip impactSound;
        }

        private static float GetSeverityMultiplier(LossSeverity severity)
        {
            return severity switch
            {
                LossSeverity.Light => 0.6f,
                LossSeverity.Normal => 1f,
                LossSeverity.Heavy => 1.4f,
                LossSeverity.Critical => 2f,
                _ => 1f
            };
        }

        [Serializable]
        public struct GainColorProfile
        {
            [Header("配色")]
            public Color panicColor;
            public Color impactColor;
            public Color criticalColor;
            [Header("音效（留空则沿用运动参数中的扣分音效）")]
            public AudioClip panicSound;
            public AudioClip scrollSound;
            public AudioClip impactSound;
            public AudioClip criticalSound;

            public static GainColorProfile CreateDefault() => new GainColorProfile
            {
                panicColor = new Color(1f, 0.95f, 0.65f),
                impactColor = new Color(1f, 0.9f, 0.3f),
                criticalColor = new Color(1f, 0.3f, 0.05f)
            };
        }

        [Serializable]
        public struct LossMotionProfile
        {
            [Header("阶段1 惊恐")]
            public float panicScale;
            public float panicDuration;
            public Color panicColor;
            [Header("阶段2 坠落")]
            public float fallDuration;
            public Ease fallEase;
            public float fallOffsetY;
            [Header("阶段3 撞击")]
            public float impactScale;
            public float impactDuration;
            public Color impactColor;
            public Color criticalImpactColor;
            [Header("阶段4 回弹")]
            public float recoverDuration;
            public Ease recoverEase;
            [Header("震动")]
            public float panicShakeStrength;
            public float impactShakeStrength;
            public float shakeDuration;
            public int shakeVibrato;
            [Header("音效")]
            public AudioClip panicSound;
            public AudioClip lossStartSound;
            public AudioClip impactSound;

            public static LossMotionProfile CreateDefault() => new LossMotionProfile
            {
                panicScale = 1.08f,
                panicDuration = 0.1f,
                panicColor = new Color(0.85f, 1f, 0.9f),
                fallDuration = 0.4f,
                fallEase = Ease.InBack,
                fallOffsetY = -12f,
                impactScale = 0.85f,
                impactDuration = 0.06f,
                impactColor = new Color(0.18f, 0.78f, 0.38f),
                criticalImpactColor = new Color(0.12f, 0.95f, 0.42f),
                recoverDuration = 0.35f,
                recoverEase = Ease.OutElastic,
                panicShakeStrength = 4f,
                impactShakeStrength = 12f,
                shakeDuration = 0.5f,
                shakeVibrato = 25
            };
        }
    }
}
