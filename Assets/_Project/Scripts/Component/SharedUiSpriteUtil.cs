using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 共用 UI 小图标赋值：返回箭头、看广告图标（子节点需已存在）。
/// </summary>
public static class SharedUiSpriteUtil
{
    /// <summary>返回箭头：居中按比例显示，清空 Label 避免与文字重叠撑乱布局。</summary>
    public static void ApplyBackIcon(Button button, DurianSpriteConfig config)
    {
        if (button == null || config == null || config.backArrowIcon == null)
        {
            return;
        }

        var icon = button.transform.Find("BackIcon")?.GetComponent<Image>();
        if (icon == null)
        {
            return;
        }

        icon.sprite = config.backArrowIcon;
        icon.color = Color.white;
        icon.preserveAspect = true;
        ApplyBackIconLayout(icon);

        var label = button.transform.Find("Label")?.GetComponent<Text>();
        if (label != null)
        {
            label.text = string.Empty;
        }
    }

    /// <summary>BackIcon 居中，按 Sprite 比例显示并限制在按钮区域内。</summary>
    public static void ApplyBackIconLayout(Image icon)
    {
        if (icon == null)
        {
            return;
        }

        var rect = icon.rectTransform;
        var parent = rect.parent as RectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        icon.preserveAspect = true;

        if (icon.sprite == null)
        {
            return;
        }

        icon.SetNativeSize();
        var size = rect.sizeDelta;

        var maxSide = 72f;
        if (parent != null)
        {
            Canvas.ForceUpdateCanvases();
            var parentRect = parent.rect;
            if (parentRect.width > 0f && parentRect.height > 0f)
            {
                maxSide = Mathf.Min(parentRect.width, parentRect.height) * 0.72f;
            }
        }

        var longest = Mathf.Max(size.x, size.y);
        if (longest > maxSide && longest > 0f)
        {
            var scale = maxSide / longest;
            rect.sizeDelta = size * scale;
        }
    }

    public static void ApplyAdIcon(Button button, DurianSpriteConfig config)
    {
        if (button == null || config == null || config.watchAdIcon == null)
        {
            return;
        }

        var icon = button.transform.Find("AdIcon")?.GetComponent<Image>();
        if (icon == null)
        {
            return;
        }

        icon.sprite = config.watchAdIcon;
        icon.color = Color.white;
        icon.preserveAspect = true;
    }
}
