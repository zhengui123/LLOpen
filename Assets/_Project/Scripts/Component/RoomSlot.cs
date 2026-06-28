using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单房交互：点击或垂直滑动捅开壳盖，露出果肉。
/// </summary>
public class RoomSlot : MonoBehaviour
{
    private static readonly Color HighlightColor = new Color(1.15f, 1.12f, 0.75f, 1f);

    [SerializeField] private Image coverImage;
    [SerializeField] private Image fleshImage;

    private bool _isOpened;
    private bool _isDragging;
    private float _pointerDownY;
    private Tween _highlightTween;
    private RoomSlotPointerRelay _pointerRelay;

    public bool IsOpened => _isOpened;
    public int RoomIndex { get; private set; }
    public YieldGrade RoomGrade { get; private set; }

    public event System.Action<RoomSlot> OnOpened;

    public void ConfigureImages(Image cover, Image flesh)
    {
        coverImage = cover;
        fleshImage = flesh;
        EnsurePointerRelay();
    }

    public void Init(int index, Sprite cover, Sprite flesh, YieldGrade grade)
    {
        RoomIndex = index;
        RoomGrade = grade;
        EnsurePointerRelay();

        if (coverImage != null)
        {
            coverImage.sprite = cover;
            coverImage.gameObject.SetActive(cover != null);
            coverImage.color = Color.white;
            SetImageAlpha(coverImage, 1f);
            coverImage.rectTransform.localScale = Vector3.one;
            coverImage.rectTransform.anchoredPosition = Vector2.zero;
            coverImage.raycastTarget = true;
        }

        if (fleshImage != null)
        {
            fleshImage.sprite = flesh;
            fleshImage.gameObject.SetActive(false);
            fleshImage.rectTransform.localScale = Vector3.one;
        }

        _isOpened = false;
        _isDragging = false;
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

        if (enabled)
        {
            coverImage.color = HighlightColor;
            _highlightTween = coverImage.rectTransform
                .DOScale(1.08f, 0.55f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            coverImage.color = Color.white;
            coverImage.rectTransform.localScale = Vector3.one;
        }
    }

    internal void HandlePointerDown(PointerEventData eventData)
    {
        if (_isOpened)
        {
            return;
        }

        _pointerDownY = eventData.position.y;
        _isDragging = true;
    }

    internal void HandlePointerDrag(PointerEventData eventData)
    {
        if (!_isDragging || coverImage == null)
        {
            return;
        }

        var deltaY = eventData.position.y - _pointerDownY;
        coverImage.rectTransform.anchoredPosition = new Vector2(0f, deltaY * 0.35f);
    }

    internal void HandlePointerUp(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        var delta = eventData.position.y - _pointerDownY;

        if (Mathf.Abs(delta) > 50f || Mathf.Abs(delta) < 10f)
        {
            TryOpen();
        }
        else if (coverImage != null)
        {
            coverImage.rectTransform.DOKill();
            coverImage.rectTransform.DOAnchorPos(Vector2.zero, 0.15f);
        }
    }

    internal void HandlePointerClick(PointerEventData eventData)
    {
        if (!_isOpened && !_isDragging)
        {
            TryOpen();
        }
    }

    public bool TryOpen()
    {
        if (_isOpened)
        {
            return false;
        }

        _isOpened = true;
        SetOpenableHighlight(false);

        if (coverImage != null)
        {
            coverImage.DOKill();
            var flyDir = coverImage.rectTransform.anchoredPosition.normalized;
            if (flyDir.sqrMagnitude < 0.01f)
            {
                flyDir = Random.insideUnitCircle;
            }

            coverImage.rectTransform.DOAnchorPos(
                coverImage.rectTransform.anchoredPosition + flyDir * 80f,
                0.3f).SetEase(Ease.OutQuad);
            coverImage.DOFade(0f, 0.2f).OnComplete(() => coverImage.gameObject.SetActive(false));
        }

        if (fleshImage != null)
        {
            fleshImage.gameObject.SetActive(true);
            fleshImage.rectTransform.localScale = Vector3.zero;
            fleshImage.rectTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        OnOpened?.Invoke(this);
        return true;
    }

    private void EnsurePointerRelay()
    {
        if (coverImage == null)
        {
            return;
        }

        _pointerRelay = coverImage.GetComponent<RoomSlotPointerRelay>();
        if (_pointerRelay == null)
        {
            _pointerRelay = coverImage.gameObject.AddComponent<RoomSlotPointerRelay>();
        }

        _pointerRelay.Bind(this);
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
