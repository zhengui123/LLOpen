using UnityEngine;
using GTest.UI;

namespace GTest.OldUI
{
    /// <summary>
    /// 旧版 Text 数字增减唯一入口（单 LegacyScoreAnimator）。
    /// </summary>
    [RequireComponent(typeof(LegacyScoreAnimator))]
    public class LegacyScoreFXManager : MonoBehaviour
    {
        public static LegacyScoreFXManager Instance { get; private set; }

        [SerializeField] private ScreenShake _screenShake;

        private LegacyScoreAnimator _animator;

        public LegacyScoreAnimator Animator => _animator;
        public float CurrentScore => _animator != null ? _animator.CurrentValue : 0f;

        private void Awake()
        {
            Instance = this;
            _animator = GetComponent<LegacyScoreAnimator>();

            if (_screenShake == null)
                _screenShake = GetComponentInParent<ScreenShake>();
        }

        public void SetValue(float value) => _animator?.SetValue(value);

        public void AddScore(float amount, float newTotal, bool isCritical = false)
        {
            if (_screenShake != null)
                _screenShake.ShakeByScore(amount);

            _animator?.AnimateGain(newTotal, isCritical);
        }

        public void SubtractScore(float amount, float newTotal,
            LossSeverity severity = LossSeverity.Normal)
        {
            if (severity == LossSeverity.Normal)
            {
                if (amount >= 2000f) severity = LossSeverity.Critical;
                else if (amount >= 500f) severity = LossSeverity.Heavy;
            }

            if (_screenShake != null)
                _screenShake.ShakeLoss(severity);

            _animator?.AnimateLoss(newTotal, severity);
        }

        public void PlayImpact(bool isCritical = false) => _animator?.PlayGainImpact(isCritical);

        public void PlayLossImpact(LossSeverity severity = LossSeverity.Normal)
        {
            _screenShake?.ShakeLoss(severity);
            _animator?.PlayLossImpact(severity);
        }
    }
}
