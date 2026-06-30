using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GTest.OldUI
{
    /// <summary>
    /// 旧版 Text 分数动画公共基类：绑定、数值状态、复位与音效。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public abstract class LegacyScoreMotionBase : MonoBehaviour
    {
        [Header("绑定")]
        [SerializeField] protected Text _displayText;
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected AudioSource _audioSource;
        [SerializeField] protected string _numberFormat = "N0";
        [SerializeField] protected Color _defaultColor = Color.white;

        protected float _currentValue;
        protected Sequence _currentSequence;
        protected Vector3 _originalScale;
        protected Vector2 _originalAnchoredPos;

        public float CurrentValue => _currentValue;

        protected virtual void Awake()
        {
            if (_displayText == null)
                _displayText = GetComponent<Text>();
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _originalScale = transform.localScale;
            _originalAnchoredPos = _rectTransform.anchoredPosition;
            _displayText.color = _defaultColor;
            _displayText.alignment = TextAnchor.MiddleCenter;
            _displayText.resizeTextForBestFit = false;
            SyncValueFromDisplayText();
        }

        /// <summary>
        /// 将内部数值与 Text 当前显示对齐（按当前文化的 N0 千分位规则解析整数）。
        /// </summary>
        protected void SyncValueFromDisplayText()
        {
            if (TryParseDisplayText(out float parsed))
                _currentValue = parsed;
        }

        /// <summary>
        /// 解析 Text 上的分数字符串（仅 N0 整数，先剥离千分位再解析，避免 "3,500"→3.5）。
        /// </summary>
        protected bool TryParseDisplayText(out float value)
        {
            value = 0f;
            if (_displayText == null || string.IsNullOrWhiteSpace(_displayText.text))
                return false;

            string text = _displayText.text.Trim();
            string groupSep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            if (!string.IsNullOrEmpty(groupSep))
                text = text.Replace(groupSep, "");

            text = text.Replace("\u00A0", "").Replace(" ", "");

            if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
            {
                value = integer;
                return true;
            }

            return false;
        }

        public virtual void SetValue(float value)
        {
            _currentValue = value;
            UpdateDisplayText(value);
        }

        protected void UpdateDisplayText(float value)
        {
            _displayText.text = FormatScore(value);
        }

        protected string FormatScore(float value)
        {
            return Mathf.FloorToInt(value).ToString(_numberFormat, CultureInfo.CurrentCulture);
        }

        protected void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
                _audioSource.PlayOneShot(clip);
        }

        protected void ResetVisual()
        {
            transform.localScale = _originalScale;
            _rectTransform.anchoredPosition = _originalAnchoredPos;
            _displayText.color = _defaultColor;
        }

        protected void FinishMotion(Action onComplete)
        {
            _rectTransform.anchoredPosition = _originalAnchoredPos;
            transform.localScale = _originalScale;
            _displayText.color = _defaultColor;
            onComplete?.Invoke();
        }

        public void Stop()
        {
            _currentSequence?.Kill();
            ResetVisual();
        }

        protected virtual void OnDestroy()
        {
            _currentSequence?.Kill();
        }
    }
}
