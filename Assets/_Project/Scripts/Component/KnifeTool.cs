using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 划刀交互：检测顶部滑动、绘制裂缝线，达到阈值后触发开果。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class KnifeTool : MonoBehaviour
{
    [SerializeField] private DurianOpener durianOpener;
    [SerializeField] private RectTransform swipeArea;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LineRenderer crackLine;
    [SerializeField] private Image knifeImage;
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private float knifeShakeStrength = 12f;
    [SerializeField] private float knifeFadeDuration = 0.3f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip crackClip;
    [SerializeField] private float shellWidth = 4f;
    [SerializeField] private float swipeThresholdRatio = 0.8f;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.08f;
    [SerializeField] private float crackPulseDuration = 0.12f;

    private readonly List<Vector3> _crackPoints = new();
    private DurianData _currentDurian;
    private bool _isSwiping;
    private bool _hasOpened;
    private Vector3 _swipeStartWorld;
    private float _swipeDistance;
    private float _defaultLineWidth = 0.05f;

    private void Awake()
    {
        if (crackLine == null)
        {
            crackLine = GetComponent<LineRenderer>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (crackLine != null)
        {
            _defaultLineWidth = crackLine.startWidth;
        }

        ResetCrackLine();
        ResetKnifeImage();
    }

    public void Setup(DurianData durian)
    {
        KillTweens();
        _currentDurian = durian;
        _hasOpened = false;
        _isSwiping = false;
        _swipeDistance = 0f;
        ResetCrackLine();
        ResetKnifeImage();
        enabled = true;
    }

    private void Update()
    {
        if (_hasOpened)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            TryBeginSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && _isSwiping)
        {
            ContinueSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && _isSwiping)
        {
            EndSwipe().Forget();
        }
#else
        if (Input.touchCount == 0)
        {
            return;
        }

        var touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryBeginSwipe(touch.position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (_isSwiping)
                {
                    ContinueSwipe(touch.position);
                }
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (_isSwiping)
                {
                    EndSwipe().Forget();
                }
                break;
        }
#endif
    }

    private void TryBeginSwipe(Vector2 screenPosition)
    {
        if (!IsInSwipeArea(screenPosition))
        {
            return;
        }

        _isSwiping = true;
        _swipeStartWorld = ScreenToWorld(screenPosition);
        _swipeDistance = 0f;
        _crackPoints.Clear();
        _crackPoints.Add(_swipeStartWorld);
        UpdateCrackLine();
        ShowKnifeAt(screenPosition);
    }

    private void ContinueSwipe(Vector2 screenPosition)
    {
        UpdateKnifePosition(screenPosition);

        var worldPos = ScreenToWorld(screenPosition);
        _swipeDistance = Vector3.Distance(_swipeStartWorld, worldPos);

        if (_crackPoints.Count == 0 || Vector3.Distance(_crackPoints[^1], worldPos) > 0.05f)
        {
            _crackPoints.Add(worldPos);
            UpdateCrackLine();
        }
    }

    private async UniTaskVoid EndSwipe()
    {
        _isSwiping = false;

        if (_swipeDistance < shellWidth * swipeThresholdRatio)
        {
            HideKnifeImmediate();
            return;
        }

        _hasOpened = true;
        enabled = false;
        FadeOutKnife().Forget();
        PlayCrackFeedback().Forget();

        if (durianOpener != null)
        {
            await durianOpener.OpenAsync(_currentDurian);
        }
    }

    private async UniTaskVoid PlayCrackFeedback()
    {
        if (audioSource != null && crackClip != null)
        {
            audioSource.PlayOneShot(crackClip);
        }

        PlayCrackLinePulse();

        if (targetCamera != null)
        {
            await targetCamera.transform
                .DOShakePosition(shakeDuration, new Vector3(shakeMagnitude, shakeMagnitude, 0f), 20, 90f, false, true)
                .AsyncWaitForCompletion();
        }
    }

    private void PlayCrackLinePulse()
    {
        if (crackLine == null)
        {
            return;
        }

        crackLine.DOKill();
        var peakWidth = _defaultLineWidth * 2.5f;
        DOTween.To(
                () => crackLine.startWidth,
                width =>
                {
                    crackLine.startWidth = width;
                    crackLine.endWidth = width;
                },
                peakWidth,
                crackPulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                crackLine.startWidth = _defaultLineWidth;
                crackLine.endWidth = _defaultLineWidth;
            });
    }

    private bool IsInSwipeArea(Vector2 screenPosition)
    {
        if (swipeArea == null)
        {
            return true;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(swipeArea, screenPosition, targetCamera);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        var depth = targetCamera != null
            ? Mathf.Abs(targetCamera.transform.position.z)
            : 10f;
        var world = targetCamera != null
            ? targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth))
            : (Vector3)screenPosition;
        world.z = 0f;
        return world;
    }

    private void UpdateCrackLine()
    {
        if (crackLine == null)
        {
            return;
        }

        crackLine.positionCount = _crackPoints.Count;
        for (var i = 0; i < _crackPoints.Count; i++)
        {
            crackLine.SetPosition(i, _crackPoints[i]);
        }
    }

    private void ResetCrackLine()
    {
        _crackPoints.Clear();
        if (crackLine != null)
        {
            crackLine.positionCount = 0;
            crackLine.startWidth = _defaultLineWidth;
            crackLine.endWidth = _defaultLineWidth;
        }
    }

    private void ShowKnifeAt(Vector2 screenPosition)
    {
        if (knifeImage == null)
        {
            return;
        }

        if (spriteConfig != null && spriteConfig.knifeSprite != null)
        {
            knifeImage.sprite = spriteConfig.knifeSprite;
        }

        knifeImage.DOKill();
        var color = knifeImage.color;
        color.a = 1f;
        knifeImage.color = color;
        knifeImage.gameObject.SetActive(true);
        UpdateKnifePosition(screenPosition);

        var knifeRect = knifeImage.rectTransform;
        knifeRect.DOKill();
        knifeRect.DOShakeAnchorPos(shakeDuration, knifeShakeStrength, 20, 90f, false, true);
    }

    private void UpdateKnifePosition(Vector2 screenPosition)
    {
        if (knifeImage == null || !knifeImage.gameObject.activeSelf)
        {
            return;
        }

        var parent = knifeImage.rectTransform.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screenPosition, targetCamera, out var localPoint))
        {
            knifeImage.rectTransform.anchoredPosition = localPoint;
        }
    }

    private async UniTaskVoid FadeOutKnife()
    {
        if (knifeImage == null)
        {
            return;
        }

        knifeImage.DOKill();
        knifeImage.rectTransform.DOKill();
        await knifeImage.DOFade(0f, knifeFadeDuration).AsyncWaitForCompletion();
        HideKnifeImmediate();
    }

    private void HideKnifeImmediate()
    {
        if (knifeImage == null)
        {
            return;
        }

        knifeImage.DOKill();
        knifeImage.rectTransform.DOKill();
        knifeImage.gameObject.SetActive(false);
        var color = knifeImage.color;
        color.a = 1f;
        knifeImage.color = color;
    }

    private void ResetKnifeImage()
    {
        HideKnifeImmediate();
    }

    private void KillTweens()
    {
        if (targetCamera != null)
        {
            targetCamera.transform.DOKill();
        }

        if (crackLine != null)
        {
            crackLine.DOKill();
        }

        if (knifeImage != null)
        {
            knifeImage.DOKill();
            knifeImage.rectTransform.DOKill();
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }
}
