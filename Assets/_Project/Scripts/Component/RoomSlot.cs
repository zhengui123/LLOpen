using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单房展示：由 KnifeTool 划刀撬开壳盖后露出果肉。
/// </summary>
public class RoomSlot : MonoBehaviour
{
    private static readonly Color HighlightColor = new Color(1.15f, 1.12f, 0.75f, 1f);

    [SerializeField] private Image coverImage;
    [SerializeField] private Image fleshImage;

    private bool _isOpened;
    private bool _knifeActive;
    private Tween _highlightTween;

    public bool IsOpened => _isOpened;
    public int RoomIndex { get; private set; }
    public YieldGrade RoomGrade { get; private set; }

    public event System.Action<RoomSlot> OnOpened;

    public void ConfigureImages(Image cover, Image flesh)
    {
        coverImage = cover;
        fleshImage = flesh;
    }

    public void Init(int index, Sprite cover, Sprite flesh, YieldGrade grade)
    {
        RoomIndex = index;
        RoomGrade = grade;

        if (coverImage != null)
        {
            coverImage.sprite = cover;
            coverImage.gameObject.SetActive(cover != null);
            coverImage.color = Color.white;
            SetImageAlpha(coverImage, 1f);
            coverImage.rectTransform.localScale = Vector3.one;
            coverImage.rectTransform.anchoredPosition = Vector2.zero;
            coverImage.rectTransform.localRotation = Quaternion.identity;
            coverImage.raycastTarget = false;
        }

        if (fleshImage != null)
        {
            fleshImage.sprite = flesh;
            fleshImage.gameObject.SetActive(false);
            fleshImage.rectTransform.localScale = Vector3.one;
        }

        _isOpened = false;
        _knifeActive = false;
        SetOpenableHighlight(true);
    }

    public bool ContainsScreenPoint(Vector2 screenPosition, Camera camera)
    {
        if (coverImage == null || !coverImage.gameObject.activeInHierarchy)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(
            coverImage.rectTransform, screenPosition, camera);
    }

    public void ApplyKnifeProgress(float progress, Vector2 swipeDelta)
    {
        if (_isOpened || coverImage == null)
        {
            return;
        }

        _knifeActive = true;
        SetOpenableHighlight(false);

        var dir = swipeDelta.sqrMagnitude > 1f ? swipeDelta.normalized : Vector2.up;
        coverImage.rectTransform.anchoredPosition = dir * progress * 28f;
        coverImage.rectTransform.localScale = Vector3.one * (1f + progress * 0.07f);
        coverImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, dir.x * progress * 10f);
    }

    public void ResetKnifeProgress()
    {
        if (_isOpened || coverImage == null || !_knifeActive)
        {
            return;
        }

        _knifeActive = false;
        coverImage.rectTransform.DOKill();
        coverImage.rectTransform.DOAnchorPos(Vector2.zero, 0.12f);
        coverImage.rectTransform.DOScale(Vector3.one, 0.12f);
        coverImage.rectTransform.DOLocalRotate(Vector3.zero, 0.12f);
        SetOpenableHighlight(true);
    }

    public void SetOpenableHighlight(bool enabled)
    {
        if (coverImage == null || _isOpened)
        {
            return;
        }

        _highlightTween?.Kill();
        coverImage.rectTransform.DOKill();

        if (enabled && !_knifeActive)
        {
            coverImage.color = HighlightColor;
            _highlightTween = coverImage.rectTransform
                .DOScale(1.08f, 0.55f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else if (!enabled)
        {
            coverImage.color = Color.white;
            coverImage.rectTransform.localScale = Vector3.one;
        }
    }

    /// <summary>划刀撬开：沿滑动方向飞壳。</summary>
    public bool TryOpenFromKnife(Vector2 swipeDelta)
    {
        var flyDir = swipeDelta.sqrMagnitude > 1f ? swipeDelta.normalized : Vector2.up;
        return TryOpenWithDirection(flyDir);
    }

    /// <summary>程序自动开房（继续开 / 中途卖），无划刀过程。</summary>
    public bool TryOpen()
    {
        var flyDir = Random.insideUnitCircle;
        if (flyDir.sqrMagnitude < 0.01f)
        {
            flyDir = Vector2.up;
        }

        return TryOpenWithDirection(flyDir.normalized);
    }

    private bool TryOpenWithDirection(Vector2 flyDir)
    {
        if (_isOpened)
        {
            return false;
        }

        _isOpened = true;
        _knifeActive = false;
        SetOpenableHighlight(false);

        if (coverImage != null)
        {
            coverImage.DOKill();
            coverImage.rectTransform.DOKill();
            coverImage.rectTransform.DOAnchorPos(
                coverImage.rectTransform.anchoredPosition + flyDir * 90f,
                0.32f).SetEase(Ease.OutQuad);
            coverImage.DOFade(0f, 0.22f).OnComplete(() => coverImage.gameObject.SetActive(false));
        }

        if (fleshImage != null)
        {
            fleshImage.gameObject.SetActive(true);
            fleshImage.rectTransform.localScale = Vector3.zero;
            fleshImage.rectTransform.DOScale(1f, 0.32f).SetEase(Ease.OutBack);
        }

        OnOpened?.Invoke(this);
        return true;
    }

    private void OnDestroy()
    {
        _highlightTween?.Kill();
        if (coverImage != null)
        {
            coverImage.DOKill();
            coverImage.rectTransform.DOKill();
        }

        if (fleshImage != null)
        {
            fleshImage.rectTransform.DOKill();
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
