using UnityEngine;
using GTest.UI;

namespace GTest.OldUI
{
    /// <summary>
    /// 旧版数字增减键盘测试：Y~P 加分，- = [ ] 扣分。
    /// </summary>
    [RequireComponent(typeof(LegacyScoreFXManager))]
    public class LegacyScoreAnimatorDemo : MonoBehaviour
    {
        private LegacyScoreFXManager _manager;
        private float _totalScore;

        private void Awake() => _manager = GetComponent<LegacyScoreFXManager>();

        private void Start()
        {
            _totalScore = 1000f;
            _manager?.SetValue(_totalScore);
        }

        private void Update()
        {
            if (_manager == null) return;

            if (Input.GetKeyDown(KeyCode.Y)) AddScore(50f, false);
            if (Input.GetKeyDown(KeyCode.U)) AddScore(300f, false);
            if (Input.GetKeyDown(KeyCode.I)) AddScore(800f, false);
            if (Input.GetKeyDown(KeyCode.O)) AddScore(2000f, true);
            if (Input.GetKeyDown(KeyCode.P)) _manager.PlayImpact(true);

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                SubtractScore(50f, LossSeverity.Normal);
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadEquals))
                SubtractScore(300f, LossSeverity.Normal);
            if (Input.GetKeyDown(KeyCode.LeftBracket))
                SubtractScore(800f, LossSeverity.Heavy);
            if (Input.GetKeyDown(KeyCode.RightBracket))
                _manager.PlayLossImpact(LossSeverity.Heavy);
        }

        private void AddScore(float amount, bool critical)
        {
            _totalScore += amount;
            _manager.AddScore(amount, _totalScore, critical);
        }

        private void SubtractScore(float amount, LossSeverity severity)
        {
            _totalScore = Mathf.Max(0f, _totalScore - amount);
            _manager.SubtractScore(amount, _totalScore, severity);
        }
    }
}
