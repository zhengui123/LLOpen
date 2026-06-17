using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 榴莲揭示：壳裂开动画、房位展示、评级发布；由 OpenPage 控制后续卖出跳转。
/// </summary>
public class DurianOpener : MonoBehaviour
{
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Transform shellTransform;
    [SerializeField] private Image durianImage;
    [SerializeField] private Image shellLeftImage;
    [SerializeField] private Image shellRightImage;
    [SerializeField] private Transform roomsRoot;
    [SerializeField] private GameObject roomMeatPrefab;
    [SerializeField] private GameObject roomEmptyPrefab;
    [SerializeField] private GameObject floatTextPrefab;
    [SerializeField] private Text ratingText;
    [SerializeField] private Image ratingIcon;
    [SerializeField] private float shellOpenDuration = 0.35f;
    [SerializeField] private float shellSplitDistance = 100f;
    [SerializeField] private float roomPopDuration = 0.25f;
    [SerializeField] private float roomSpacing = 1.2f;
    [SerializeField] private float floatTextDuration = 0.8f;

    private bool _isOpening;
    private Vector3 _shellOriginalScale = Vector3.one;
    private Vector2 _shellLeftStartPos;
    private Vector2 _shellRightStartPos;
    private readonly List<GameObject> _spawnedRooms = new();

    private void Awake()
    {
        if (shellTransform != null)
        {
            _shellOriginalScale = shellTransform.localScale;
        }

        CacheShellHalfPositions();
    }

    /// <summary>
    /// 重置壳体与房位展示，供重新开果或复活后使用。
    /// </summary>
    public void ResetVisualState()
    {
        KillTweens();
        _isOpening = false;
        ClearRooms();

        if (shellTransform != null)
        {
            shellTransform.localScale = _shellOriginalScale;
        }

        if (durianImage != null)
        {
            durianImage.gameObject.SetActive(true);
        }

        ResetShellHalfImages();
        ResetRatingDisplay();
    }

    public async UniTask OpenAsync(DurianData durian)
    {
        if (_isOpening)
        {
            return;
        }

        _isOpening = true;

        ClearRooms();
        await PlayShellCrackAsync();
        await RevealRoomsAsync(durian);

        var rating = YieldRatingUtil.GetRating(durian.yieldRate);
        PlayRatingRevealAsync(rating, durian.yieldRate);

        EventBus.Publish(new DurianOpenedEvent
        {
            Durian = durian,
            Rating = rating,
            YieldRate = durian.yieldRate
        });

        _isOpening = false;
    }

    private async UniTask PlayShellCrackAsync()
    {
        if (HasShellHalfImages())
        {
            await PlayShellHalfCrackAsync();
            return;
        }

        if (shellTransform == null)
        {
            return;
        }

        var from = shellTransform.localScale;
        var to = new Vector3(from.x * 1.15f, from.y * 0.82f, from.z);
        await shellTransform
            .DOScale(to, shellOpenDuration)
            .SetEase(Ease.OutBack)
            .AsyncWaitForCompletion();
    }

    private async UniTask PlayShellHalfCrackAsync()
    {
        CacheShellHalfPositions();

        if (durianImage != null)
        {
            durianImage.gameObject.SetActive(false);
        }

        if (spriteConfig != null)
        {
            if (shellLeftImage != null && spriteConfig.shellLeftHalf != null)
            {
                shellLeftImage.sprite = spriteConfig.shellLeftHalf;
            }

            if (shellRightImage != null && spriteConfig.shellRightHalf != null)
            {
                shellRightImage.sprite = spriteConfig.shellRightHalf;
            }
        }

        var leftRect = shellLeftImage.rectTransform;
        var rightRect = shellRightImage.rectTransform;
        leftRect.anchoredPosition = _shellLeftStartPos;
        rightRect.anchoredPosition = _shellRightStartPos;
        shellLeftImage.gameObject.SetActive(true);
        shellRightImage.gameObject.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Join(leftRect
            .DOAnchorPosX(_shellLeftStartPos.x - shellSplitDistance, shellOpenDuration)
            .SetEase(Ease.OutBack));
        sequence.Join(rightRect
            .DOAnchorPosX(_shellRightStartPos.x + shellSplitDistance, shellOpenDuration)
            .SetEase(Ease.OutBack));
        await sequence.AsyncWaitForCompletion();
    }

    private async UniTask RevealRoomsAsync(DurianData durian)
    {
        if (roomsRoot == null || durian.roomResults == null)
        {
            return;
        }

        var count = durian.roomResults.Length;
        var startX = -(count - 1) * roomSpacing * 0.5f;
        var revealSequence = DOTween.Sequence();

        for (var i = 0; i < count; i++)
        {
            var hasMeat = durian.roomResults[i];
            var prefab = hasMeat ? roomMeatPrefab : roomEmptyPrefab;
            if (prefab == null)
            {
                continue;
            }

            var room = Instantiate(prefab, roomsRoot);
            ApplyRoomSprite(room, hasMeat);
            room.transform.localPosition = new Vector3(startX + i * roomSpacing, 0f, 0f);
            room.transform.localScale = Vector3.zero;
            _spawnedRooms.Add(room);

            SpawnFloatText(room.transform.position, hasMeat ? "满房" : "空房");
            revealSequence.Join(room.transform
                .DOScale(Vector3.one, roomPopDuration)
                .SetEase(Ease.OutBack));
        }

        if (revealSequence.Duration() > 0f)
        {
            await revealSequence.AsyncWaitForCompletion();
        }
    }

    private void ApplyRoomSprite(GameObject room, bool hasMeat)
    {
        if (spriteConfig == null)
        {
            return;
        }

        var image = room.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        var sprite = hasMeat ? spriteConfig.fleshPiece : spriteConfig.emptyPiece;
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }
    }

    private void SpawnFloatText(Vector3 worldPos, string message)
    {
        if (floatTextPrefab == null)
        {
            return;
        }

        var textGo = Instantiate(floatTextPrefab, roomsRoot != null ? roomsRoot : transform);
        textGo.transform.position = worldPos + Vector3.up * 0.5f;

        var label = textGo.GetComponent<Text>();
        if (label != null)
        {
            label.text = message;
            label.color = message == "满房" ? new Color(1f, 0.84f, 0f) : Color.gray;
        }

        var canvasGroup = textGo.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textGo.AddComponent<CanvasGroup>();
        }

        var endPos = textGo.transform.position + Vector3.up * 0.8f;
        DOTween.Sequence()
            .Join(textGo.transform.DOMove(endPos, floatTextDuration).SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(0f, floatTextDuration))
            .OnComplete(() => Destroy(textGo));
    }

    private void PlayRatingRevealAsync(string rating, float yieldRate)
    {
        if (ratingText == null)
        {
            return;
        }

        ratingText.transform.DOKill();
        ratingText.text = $"出肉率 {yieldRate:F1}% · {rating}";
        ratingText.transform.localScale = Vector3.zero;

        var textCanvasGroup = ratingText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = ratingText.gameObject.AddComponent<CanvasGroup>();
        }

        textCanvasGroup.alpha = 0f;

        if (ratingIcon != null)
        {
            ratingIcon.transform.DOKill();
            ratingIcon.gameObject.SetActive(true);
            if (spriteConfig != null)
            {
                ratingIcon.sprite = spriteConfig.GetRatingSprite(rating);
            }

            ratingIcon.transform.localScale = Vector3.zero;
        }

        var iconCanvasGroup = GetOrAddCanvasGroup(ratingIcon);

        var sequence = DOTween.Sequence();
        sequence.Join(ratingText.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        sequence.Join(textCanvasGroup.DOFade(1f, 0.3f));

        if (ratingIcon != null)
        {
            sequence.Join(ratingIcon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            if (iconCanvasGroup != null)
            {
                iconCanvasGroup.alpha = 0f;
                sequence.Join(iconCanvasGroup.DOFade(1f, 0.3f));
            }
        }

        if (rating == "榴莲之王")
        {
            sequence.Append(ratingText.transform
                .DOPunchScale(Vector3.one * 0.25f, 0.5f, 8, 0.5f));
            if (ratingIcon != null)
            {
                sequence.Join(ratingIcon.transform
                    .DOPunchScale(Vector3.one * 0.25f, 0.5f, 8, 0.5f));
            }
        }
    }

    private bool HasShellHalfImages()
    {
        return shellLeftImage != null && shellRightImage != null;
    }

    private void CacheShellHalfPositions()
    {
        if (shellLeftImage != null)
        {
            _shellLeftStartPos = shellLeftImage.rectTransform.anchoredPosition;
        }

        if (shellRightImage != null)
        {
            _shellRightStartPos = shellRightImage.rectTransform.anchoredPosition;
        }
    }

    private void ResetShellHalfImages()
    {
        if (shellLeftImage != null)
        {
            shellLeftImage.rectTransform.DOKill();
            shellLeftImage.rectTransform.anchoredPosition = _shellLeftStartPos;
            shellLeftImage.gameObject.SetActive(false);
        }

        if (shellRightImage != null)
        {
            shellRightImage.rectTransform.DOKill();
            shellRightImage.rectTransform.anchoredPosition = _shellRightStartPos;
            shellRightImage.gameObject.SetActive(false);
        }
    }

    private void ResetRatingDisplay()
    {
        if (ratingText != null)
        {
            ratingText.text = string.Empty;
            ratingText.transform.localScale = Vector3.one;
            var ratingCanvasGroup = ratingText.GetComponent<CanvasGroup>();
            if (ratingCanvasGroup != null)
            {
                ratingCanvasGroup.alpha = 1f;
            }
        }

        if (ratingIcon != null)
        {
            ratingIcon.transform.DOKill();
            ratingIcon.transform.localScale = Vector3.one;
            ratingIcon.gameObject.SetActive(false);
            var iconCanvasGroup = ratingIcon.GetComponent<CanvasGroup>();
            if (iconCanvasGroup != null)
            {
                iconCanvasGroup.alpha = 1f;
            }
        }
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
        if (shellTransform != null)
        {
            shellTransform.DOKill();
        }

        if (shellLeftImage != null)
        {
            shellLeftImage.rectTransform.DOKill();
        }

        if (shellRightImage != null)
        {
            shellRightImage.rectTransform.DOKill();
        }

        if (roomsRoot != null)
        {
            roomsRoot.DOKill(true);
        }

        if (ratingText != null)
        {
            ratingText.transform.DOKill();
            var ratingCanvasGroup = ratingText.GetComponent<CanvasGroup>();
            if (ratingCanvasGroup != null)
            {
                ratingCanvasGroup.DOKill();
            }
        }

        if (ratingIcon != null)
        {
            ratingIcon.transform.DOKill();
            var iconCanvasGroup = ratingIcon.GetComponent<CanvasGroup>();
            if (iconCanvasGroup != null)
            {
                iconCanvasGroup.DOKill();
            }
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }
}
