using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 运行时创建 RoomSlot 层级（无 Prefab 时的回退方案）。
/// </summary>
public static class RoomSlotFactory
{
    public static RoomSlot Create(Transform parent)
    {
        var root = new GameObject("RoomSlot", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(140f, 140f);
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        var fleshImage = CreateStretchImage(root.transform, "FleshImage", raycast: false);
        fleshImage.gameObject.SetActive(false);

        var coverImage = CreateStretchImage(root.transform, "CoverImage", raycast: true);
        coverImage.preserveAspect = true;

        var slot = root.AddComponent<RoomSlot>();
        slot.ConfigureImages(coverImage, fleshImage);
        return slot;
    }

    private static Image CreateStretchImage(Transform parent, string name, bool raycast)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.raycastTarget = raycast;
        image.color = Color.white;
        return image;
    }
}
