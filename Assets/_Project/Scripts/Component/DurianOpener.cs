using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 榴莲揭示：中央开裂叠加 + 左右壳划开，果肉与评级在同一区域展示。
/// </summary>
public class DurianOpener : MonoBehaviour
{
    [SerializeField] private DurianSpriteConfig spriteConfig;

    [Header("画面中央元素（同锚点叠放）")]
    [FormerlySerializedAs("durianImage")]
    [SerializeField] private Image wholeDurianImage;
    [SerializeField] private Image crackOverlayImage;
    [SerializeField] private Image openedDurianImage;
    [SerializeField] private Image shellLeftImage;
    [SerializeField] private Image shellRightImage;

    [Header("评级UI")]
    [FormerlySerializedAs("ratingIcon")]
    [SerializeField] private Image ratingBadgeImage;
    [SerializeField] private Text ratingText;
    [SerializeField] private CanvasGroup ratingCanvasGroup;

    [Header("果肉揭示区（v1.2 默认关闭底部房位弹出）")]
    [FormerlySerializedAs("roomsRoot")]
    [SerializeField] private Transform fleshGridParent;
    [FormerlySerializedAs("roomMeatPrefab")]
    [SerializeField] private GameObject fleshRoomPrefab;
    [SerializeField] private GameObject floatTextPrefab;
    [SerializeField] private bool revealFleshRoomsAtBottom = false;

    [Header("左右划壳动画")]
    [SerializeField] private float shellSplitDistance = 160f;
    [SerializeField] private float shellSplitDuration = 0.55f;
    [SerializeField] private float shellSplitRotation = 22f;
    [SerializeField] private float openedRevealDuration = 0.28f;

    [SerializeField] private float roomSpacing = 80f;
    [SerializeField] private float roomPopDuration = 0.3f;
    [SerializeField] private int roomRevealDelayMs = 150;
    [SerializeField] private float floatTextDuration = 0.8f;

    private float _currentCrackProgress;
    private bool _isOpening;
    private readonly List<GameObject> _spawnedRooms = new();

    private void Awake()
    {
        if (ratingCanvasGroup == null && ratingBadgeImage != null)
        {
            ratingCanvasGroup = ratingBadgeImage.GetComponent<CanvasGroup>();
        }

        SyncDurianLayerRects();
        ResetVisualState();
    }

    public void ResetVisualState()
    {
        KillTweens();
        _isOpening = false;
        _currentCrackProgress = 0f;
        ClearRooms();
        ResetCrackOverlay();
        ResetOpenedImage();
        ResetWholeDurianImage();
        ResetShellImages();
        ResetRatingDisplay();

        if (fleshGridParent != null)
        {
            fleshGridParent.gameObject.SetActive(false);
        }
    }

    public void UpdateCrackProgress(float progress)
    {
        if (spriteConfig == null || crackOverlayImage == null)
        {
            return;
        }

        progress = Mathf.Clamp01(progress);
        _currentCrackProgress = progress;
        ResetShellImages();

        if (progress <= 0f)
        {
            crackOverlayImage.enabled = false;
            return;
        }

        crackOverlayImage.enabled = true;
        var sprite = spriteConfig.GetCrackStage(progress);
        if (sprite != null)
        {
            crackOverlayImage.sprite = sprite;
        }

        SetImageAlpha(crackOverlayImage, 1f);
    }

    public void ApplyUnopenedSprite(DurianData durian)
    {
        if (wholeDurianImage == null)
        {
            return;
        }

        SyncDurianLayerRects();

        if (spriteConfig != null)
        {
            wholeDurianImage.sprite = spriteConfig.GetUnopenedSprite(durian.variety, durian.appearance);
            wholeDurianImage.color = Color.white;
        }

        wholeDurianImage.preserveAspect = true;
        wholeDurianImage.gameObject.SetActive(true);
        SetImageAlpha(wholeDurianImage, 1f);
    }

    public async UniTask OnSwipeComplete(DurianData durian)
    {
        if (_isOpening)
        {
            return;
        }

        _isOpening = true;
        ClearRooms();
        SyncDurianLayerRects();

        if (spriteConfig != null && crackOverlayImage != null && spriteConfig.crackStage5 != null)
        {
            crackOverlayImage.enabled = true;
            crackOverlayImage.sprite = spriteConfig.crackStage5;
            SetImageAlpha(crackOverlayImage, 1f);
        }

        await UniTask.Delay(200);

        PrepareOpenedLayer(durian);
        PrepareShellHalves();

        if (crackOverlayImage != null)
        {
            await crackOverlayImage.DOFade(0f, 0.2f).AsyncWaitForCompletion();
            crackOverlayImage.enabled = false;
        }

        if (openedDurianImage != null)
        {
            await openedDurianImage.DOFade(1f, openedRevealDuration).AsyncWaitForCompletion();
        }

        if (wholeDurianImage != null)
        {
            wholeDurianImage.DOFade(0f, openedRevealDuration);
        }

        await PlayShellSplitAnimation();

        if (wholeDurianImage != null)
        {
            wholeDurianImage.gameObject.SetActive(false);
            SetImageAlpha(wholeDurianImage, 1f);
        }

        await UniTask.Delay(150);

        if (revealFleshRoomsAtBottom)
        {
            await RevealFleshRooms(durian);
        }

        await ShowRating(durian);

        var rating = YieldRatingUtil.GetRating(durian.yieldRate);
        EventBus.Publish(new DurianOpenedEvent
        {
            Durian = durian,
            Rating = rating,
            YieldRate = durian.yieldRate
        });

        _isOpening = false;
    }

    public async UniTask OpenAsync(DurianData durian)
    {
        await OnSwipeComplete(durian);
    }

    private void PrepareOpenedLayer(DurianData durian)
    {
        if (openedDurianImage == null || spriteConfig == null)
        {
            return;
        }

        openedDurianImage.sprite = spriteConfig.GetOpenedSprite(durian.variety, durian.yieldGrade);
        openedDurianImage.preserveAspect = true;
        openedDurianImage.enabled = true;
        SetImageAlpha(openedDurianImage, 0f);
    }

    private void PrepareShellHalves()
    {
        if (spriteConfig == null)
        {
            return;
        }

        if (shellLeftImage != null && spriteConfig.shellLeftHalf != null)
        {
            shellLeftImage.sprite = spriteConfig.shellLeftHalf;
            shellLeftImage.preserveAspect = true;
            shellLeftImage.enabled = true;
            SetImageAlpha(shellLeftImage, 1f);
            ResetShellTransform(shellLeftImage.rectTransform);
        }

        if (shellRightImage != null && spriteConfig.shellRightHalf != null)
        {
            shellRightImage.sprite = spriteConfig.shellRightHalf;
            shellRightImage.preserveAspect = true;
            shellRightImage.enabled = true;
            SetImageAlpha(shellRightImage, 1f);
            ResetShellTransform(shellRightImage.rectTransform);
        }
    }

    private async UniTask PlayShellSplitAnimation()
    {
        if (shellLeftImage == null || shellRightImage == null)
        {
            await UniTask.Delay(Mathf.RoundToInt(shellSplitDuration * 1000f));
            return;
        }

        var leftRect = shellLeftImage.rectTransform;
        var rightRect = shellRightImage.rectTransform;
        leftRect.DOKill();
        rightRect.DOKill();
        shellLeftImage.DOKill();
        shellRightImage.DOKill();

        var sequence = DOTween.Sequence();
        sequence.Join(leftRect.DOAnchorPosX(-shellSplitDistance, shellSplitDuration).SetEase(Ease.OutCubic));
        sequence.Join(rightRect.DOAnchorPosX(shellSplitDistance, shellSplitDuration).SetEase(Ease.OutCubic));
        sequence.Join(leftRect.DORotate(new Vector3(0f, 0f, -shellSplitRotation), shellSplitDuration).SetEase(Ease.OutQuad));
        sequence.Join(rightRect.DORotate(new Vector3(0f, 0f, shellSplitRotation), shellSplitDuration).SetEase(Ease.OutQuad));
        sequence.Join(shellLeftImage.DOFade(0f, shellSplitDuration));
        sequence.Join(shellRightImage.DOFade(0f, shellSplitDuration));

        await sequence.AsyncWaitForCompletion();

        shellLeftImage.enabled = false;
        shellRightImage.enabled = false;
        ResetShellTransform(leftRect);
        ResetShellTransform(rightRect);
        SetImageAlpha(shellLeftImage, 1f);
        SetImageAlpha(shellRightImage, 1f);
    }

    private void SyncDurianLayerRects()
    {
        if (wholeDurianImage == null)
        {
            return;
        }

        var source = wholeDurianImage.rectTransform;
        SyncRectTransform(crackOverlayImage, source);
        SyncRectTransform(openedDurianImage, source);
        SyncRectTransform(shellLeftImage, source);
        SyncRectTransform(shellRightImage, source);

        if (shellLeftImage != null)
        {
            shellLeftImage.rectTransform.pivot = new Vector2(1f, 0.5f);
        }

        if (shellRightImage != null)
        {
            shellRightImage.rectTransform.pivot = new Vector2(0f, 0.5f);
        }
    }

    private static void SyncRectTransform(Image image, RectTransform source)
    {
        if (image == null)
        {
            return;
        }

        var rect = image.rectTransform;
        rect.anchorMin = source.anchorMin;
        rect.anchorMax = source.anchorMax;
        rect.pivot = source.pivot;
        rect.anchoredPosition = source.anchoredPosition;
        rect.sizeDelta = source.sizeDelta;
        rect.offsetMin = source.offsetMin;
        rect.offsetMax = source.offsetMax;
        rect.localRotation = Quaternion.identity;
        rect.localScale = source.localScale;
    }

    private static void ResetShellTransform(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
    }

    private async UniTask RevealFleshRooms(DurianData durian)
    {
        if (fleshGridParent == null || durian.roomResults == null || fleshRoomPrefab == null)
        {
            return;
        }

        fleshGridParent.gameObject.SetActive(true);
        var roomCount = durian.roomResults.Length;
        for (var i = 0; i < roomCount; i++)
        {
            var hasMeat = durian.roomResults[i];
            var roomObj = Instantiate(fleshRoomPrefab, fleshGridParent);
            roomObj.transform.localPosition = GetRoomPosition(i, roomCount);

            var image = roomObj.GetComponent<Image>();
            if (image != null && spriteConfig != null)
            {
                var sprite = hasMeat ? spriteConfig.fleshPiece : spriteConfig.emptyPiece;
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.color = Color.white;
                }
            }

            roomObj.transform.localScale = Vector3.zero;
            _spawnedRooms.Add(roomObj);

            await roomObj.transform
                .DOScale(1f, roomPopDuration)
                .SetEase(Ease.OutBack)
                .AsyncWaitForCompletion();

            await UniTask.Delay(roomRevealDelayMs);
        }
    }

    private async UniTask ShowRating(DurianData durian)
    {
        var rating = YieldRatingUtil.GetRating(durian.yieldRate);

        if (ratingText != null)
        {
            ratingText.transform.DOKill();
            ratingText.text = $"出肉率 {durian.yieldRate:F1}% · {rating}";
            ratingText.transform.localScale = Vector3.one;
        }

        if (ratingBadgeImage != null)
        {
            ratingBadgeImage.transform.DOKill();
            ratingBadgeImage.gameObject.SetActive(true);

            if (spriteConfig != null)
            {
                ratingBadgeImage.sprite = rating == "榴莲之王"
                    ? spriteConfig.kingRating
                    : spriteConfig.GetRatingSprite(rating);
            }

            ratingBadgeImage.transform.localScale = Vector3.one;
        }

        var canvasGroup = ratingCanvasGroup ?? GetOrAddCanvasGroup(ratingBadgeImage);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, 0.3f).AsyncWaitForCompletion();
        }

        if (ratingBadgeImage != null)
        {
            await ratingBadgeImage.transform
                .DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.5f, 2, 0.5f)
                .AsyncWaitForCompletion();
        }

        if (rating == "榴莲之王" && ratingText != null)
        {
            await ratingText.transform
                .DOPunchScale(Vector3.one * 0.25f, 0.5f, 8, 0.5f)
                .AsyncWaitForCompletion();
        }

        await UniTask.Delay(500);
    }

    private Vector3 GetRoomPosition(int index, int total)
    {
        var startX = -(total - 1) * roomSpacing * 0.5f;
        return new Vector3(startX + index * roomSpacing, 0f, 0f);
    }

    private void ResetCrackOverlay()
    {
        if (crackOverlayImage == null)
        {
            return;
        }

        crackOverlayImage.DOKill();
        crackOverlayImage.enabled = false;
        SetImageAlpha(crackOverlayImage, 1f);
    }

    private void ResetOpenedImage()
    {
        if (openedDurianImage == null)
        {
            return;
        }

        openedDurianImage.DOKill();
        openedDurianImage.enabled = false;
        SetImageAlpha(openedDurianImage, 0f);
    }

    private void ResetWholeDurianImage()
    {
        if (wholeDurianImage == null)
        {
            return;
        }

        wholeDurianImage.DOKill();
        wholeDurianImage.gameObject.SetActive(true);
        SetImageAlpha(wholeDurianImage, 1f);
    }

    private void ResetShellImages()
    {
        if (shellLeftImage != null)
        {
            shellLeftImage.DOKill();
            shellLeftImage.enabled = false;
            SetImageAlpha(shellLeftImage, 1f);
            ResetShellTransform(shellLeftImage.rectTransform);
        }

        if (shellRightImage != null)
        {
            shellRightImage.DOKill();
            shellRightImage.enabled = false;
            SetImageAlpha(shellRightImage, 1f);
            ResetShellTransform(shellRightImage.rectTransform);
        }
    }

    private void ResetRatingDisplay()
    {
        if (ratingText != null)
        {
            ratingText.text = string.Empty;
            ratingText.transform.DOKill();
            ratingText.transform.localScale = Vector3.one;
        }

        if (ratingBadgeImage != null)
        {
            ratingBadgeImage.transform.DOKill();
            ratingBadgeImage.transform.localScale = Vector3.one;
            ratingBadgeImage.gameObject.SetActive(false);
        }

        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.DOKill();
            ratingCanvasGroup.alpha = 0f;
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        var color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private static CanvasGroup GetOrAddCanvasGroup(Image image)
    {
        if (image == null)
        {
            return null;
        }

        var canvasGroup = image.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = image.gameObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private void ClearRooms()
    {
        foreach (var room in _spawnedRooms)
        {
            if (room != null)
            {
                room.transform.DOKill();
                Destroy(room);
            }
        }

        _spawnedRooms.Clear();
    }

    private void KillTweens()
    {
        if (wholeDurianImage != null)
        {
            wholeDurianImage.DOKill();
        }

        if (crackOverlayImage != null)
        {
            crackOverlayImage.DOKill();
        }

        if (openedDurianImage != null)
        {
            openedDurianImage.DOKill();
        }

        if (shellLeftImage != null)
        {
            shellLeftImage.DOKill();
            shellLeftImage.rectTransform.DOKill();
        }

        if (shellRightImage != null)
        {
            shellRightImage.DOKill();
            shellRightImage.rectTransform.DOKill();
        }

        if (fleshGridParent != null)
        {
            fleshGridParent.DOKill(true);
        }

        if (ratingText != null)
        {
            ratingText.transform.DOKill();
        }

        if (ratingBadgeImage != null)
        {
            ratingBadgeImage.transform.DOKill();
        }

        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.DOKill();
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }
}
