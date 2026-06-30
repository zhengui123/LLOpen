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
        public void Shake(float strength = 5f, float duration = 0.3f, int vibrato = 10)
        {
            _shakeTween?.Kill();
            _canvasRect.anchoredPosition = _originalPos;

            _shakeTween = _canvasRect
                .DOShakeAnchorPos(duration, strength, vibrato, 90f, false, true)
                .OnComplete(() => _canvasRect.anchoredPosition = _originalPos)
                .SetUpdate(true);
        }

        /// <summary>
        /// 根据分数增量自动选择震动强度
        /// </summary>
        public void ShakeByScore(float scoreDelta)
        {
            if (scoreDelta < 100f) return;
            if (scoreDelta < 500f) Shake(3f, 0.2f, 8);
            else if (scoreDelta < 2000f) Shake(6f, 0.35f, 15);
            else Shake(10f, 0.5f, 25);
        }

        /// <summary>
        /// 扣分专用震动（比加分更剧烈，模拟重击感）
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

            _shakeTween = _canvasRect
                .DOShakeAnchorPos(duration, strength, vibrato, 60f, false, true)
                .OnComplete(() => _canvasRect.anchoredPosition = _originalPos)
                .SetUpdate(true);
        }

        private void OnDestroy()
        {
            _shakeTween?.Kill();
        }
    }
}
