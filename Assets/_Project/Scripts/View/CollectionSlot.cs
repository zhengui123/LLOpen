using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图鉴单格：未解锁显示锁格 + ？；已解锁显示品种、档位与最佳售价。
/// </summary>
public class CollectionSlot : MonoBehaviour
{
    [SerializeField] private GameObject lockedRoot;
    [SerializeField] private Image lockedBgImage;
    [SerializeField] private Text lockedQuestionText;
    [SerializeField] private GameObject unlockedRoot;
    [SerializeField] private Text infoText;
    [SerializeField] private Text priceText;

    public void SetLocked(DurianSpriteConfig config)
    {
        if (lockedRoot != null)
        {
            lockedRoot.SetActive(true);
        }

        if (unlockedRoot != null)
        {
            unlockedRoot.SetActive(false);
        }

        if (lockedBgImage != null && config != null && config.collectionLockedSlot != null)
        {
            lockedBgImage.sprite = config.collectionLockedSlot;
            lockedBgImage.color = Color.white;
            lockedBgImage.preserveAspect = true;
        }

        if (lockedQuestionText != null)
        {
            lockedQuestionText.text = "？";
        }
    }

    public void SetUnlocked(string varietyName, string gradeName, int bestPrice)
    {
        if (lockedRoot != null)
        {
            lockedRoot.SetActive(false);
        }

        if (unlockedRoot != null)
        {
            unlockedRoot.SetActive(true);
        }

        if (infoText != null)
        {
            infoText.text = $"{varietyName}\n{gradeName}";
        }

        if (priceText != null)
        {
            priceText.text = bestPrice > 0 ? $"最高 {bestPrice} 金" : "已解锁";
        }
    }
}
