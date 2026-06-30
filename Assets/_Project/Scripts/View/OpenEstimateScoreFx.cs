using GTest.OldUI;
using UnityEngine;

/// <summary>
/// 开果逐房估价：封装 LegacyScoreFX，纯数字 + 旁路「金币」文案。
/// </summary>
public class OpenEstimateScoreFx : MonoBehaviour
{
    [SerializeField] private LegacyScoreFXManager scoreFx;

    public void BindIfNeeded(LegacyScoreFXManager manager)
    {
        if (scoreFx == null && manager != null)
        {
            scoreFx = manager;
        }
    }

    /// <summary>开果重置，无动画。</summary>
    public void ResetEstimate(int value)
    {
        scoreFx?.SetValue(value);
    }

    /// <summary>逐房揭示后播放估价增减动画。</summary>
    public void AnimateTo(int oldValue, int newValue)
    {
        if (scoreFx == null)
        {
            return;
        }

        if (oldValue == newValue)
        {
            scoreFx.SetValue(newValue);
            return;
        }

        var delta = newValue - oldValue;
        if (delta > 0)
        {
            scoreFx.AddScore(delta, newValue);
        }
        else
        {
            scoreFx.SubtractScore(-delta, newValue);
        }
    }

    public void StopMotion()
    {
        scoreFx?.Animator?.Stop();
    }
}
