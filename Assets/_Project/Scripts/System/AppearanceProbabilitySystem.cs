using UnityEngine;

/// <summary>
/// 外观概率叠加：品种基础权重 + 外观偏移 → 归一化概率 → 抽样档位。
/// </summary>
public class AppearanceProbabilitySystem
{
    public float[] CalculateFinalProbabilities(float[] baseWeights, float[] offsets)
    {
        var finalWeights = new float[5];

        for (var i = 0; i < 5; i++)
        {
            finalWeights[i] = Mathf.Max(0f, baseWeights[i] + offsets[i]);
        }

        var total = 0f;
        for (var i = 0; i < 5; i++)
        {
            total += finalWeights[i];
        }

        if (total <= 0f)
        {
            return (float[])baseWeights.Clone();
        }

        for (var i = 0; i < 5; i++)
        {
            finalWeights[i] = finalWeights[i] / total * 100f;
        }

        return finalWeights;
    }

    public YieldGrade SampleYieldGrade(float[] probabilities)
    {
        var random = Random.Range(0f, 100f);
        var cumulative = 0f;

        for (var i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (random <= cumulative)
            {
                return (YieldGrade)i;
            }
        }

        return YieldGrade.Normal;
    }
}
