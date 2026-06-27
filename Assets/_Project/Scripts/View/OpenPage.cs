using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 开榴莲页：滑动划刀、揭示、卖出、复活广告。
/// </summary>
public class OpenPage : MonoBehaviour
{
    [SerializeField] private KnifeTool knifeTool;
    [SerializeField] private DurianOpener durianOpener;
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Image durianImage;
    [SerializeField] private Text guideText;
    [SerializeField] private Image swipeGuideImage;
    [SerializeField] private Text estimateText;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button reviveButton;
    [SerializeField] private Button backButton;

    private SellManager _sellManager;
    private AdManager _adManager;
    private BagManager _bagManager;
    private DurianGeneratorSystem _durianGenerator;
    private GameUIRoot _uiRoot;
    private DurianData _currentDurian;
    private string _lastRating;
    private IDisposable _openedSub;
    private Tween _guidePulseTween;
    private Tween _guideImagePulseTween;
    private bool _staticUiApplied;

    [Inject]
    public void Construct(
        SellManager sellManager,
        AdManager adManager,
        BagManager bagManager,
        DurianGeneratorSystem durianGenerator,
        GameUIRoot uiRoot)
    {
        _sellManager = sellManager;
        _adManager = adManager;
        _bagManager = bagManager;
        _durianGenerator = durianGenerator;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        ApplyStaticUiSprites();

        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(false);
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }

        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(false);
            reviveButton.onClick.RemoveAllListeners();
            reviveButton.onClick.AddListener(OnReviveClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }
    }

    public void Show(DurianData durian)
    {
        KillTweens();
        _currentDurian = durian;
        _lastRating = string.Empty;
        _openedSub?.Dispose();
        _openedSub = EventBus.Subscribe<DurianOpenedEvent>(OnDurianOpened);

        durianOpener?.ResetVisualState();
        HideActionButtons();
        ApplyDurianVisual(durian);
        BindKnifeEvents();

        if (guideText != null)
        {
            guideText.gameObject.SetActive(true);
            guideText.text = "在榴莲顶部滑动开果";
            StartGuidePulse();
        }

        ShowSwipeGuide();
        RefreshEstimateText();

        if (knifeTool != null)
        {
            knifeTool.Setup(durian);
        }
    }

    private void OnDurianOpened(DurianOpenedEvent e)
    {
        if (e.Durian.id != _currentDurian.id)
        {
            return;
        }

        _currentDurian = e.Durian;
        _lastRating = e.Rating;
        RefreshEstimateText();

        if (guideText != null)
        {
            StopGuidePulse();
            guideText.gameObject.SetActive(false);
        }

        HideSwipeGuide();
        ShowOpenedActionsAsync().Forget();
    }

    private async UniTaskVoid ShowOpenedActionsAsync()
    {
        // 评级动画由 DurianOpener 播放完毕后再弹出操作按钮
        await UniTask.Delay(300);
        ShowButtonPop(sellButton);
        if (_lastRating == "空壳")
        {
            ShowButtonPop(reviveButton);
        }
    }

    private void OnSellClicked()
    {
        _uiRoot?.ShowSell(_currentDurian, _lastRating);
    }

    private async void OnReviveClicked()
    {
        var success = await _adManager.ShowRewardedAd("revive");
        if (!success)
        {
            return;
        }

        _currentDurian = _durianGenerator.RerollOpenResult(_currentDurian);
        _bagManager.ReplaceDurian(_currentDurian);
        _lastRating = string.Empty;

        durianOpener?.ResetVisualState();
        ApplyDurianVisual(_currentDurian);
        RefreshEstimateText();
        HideActionButtons();

        if (guideText != null)
        {
            guideText.gameObject.SetActive(true);
            guideText.text = "在榴莲顶部滑动开果";
            StartGuidePulse();
        }

        ShowSwipeGuide();

        if (knifeTool != null)
        {
            knifeTool.Setup(_currentDurian);
        }
    }

    private void OnSwipeStarted()
    {
        HideSwipeGuide();

        if (guideText != null)
        {
            StopGuidePulse();
            guideText.gameObject.SetActive(false);
        }
    }

    private void BindKnifeEvents()
    {
        if (knifeTool == null)
        {
            return;
        }

        knifeTool.SwipeStarted -= OnSwipeStarted;
        knifeTool.SwipeStarted += OnSwipeStarted;
    }

    private void UnbindKnifeEvents()
    {
        if (knifeTool == null)
        {
            return;
        }

        knifeTool.SwipeStarted -= OnSwipeStarted;
    }

    private void ApplyDurianVisual(DurianData durian)
    {
        if (durianOpener != null)
        {
            durianOpener.ApplyUnopenedSprite(durian);
        }

        // durianImage 与 wholeDurianImage 在场景中常为同一节点，必须保持显示
        if (durianImage != null)
        {
            durianImage.gameObject.SetActive(true);
            durianImage.transform.DOKill();
            durianImage.transform.localScale = Vector3.one * 0.92f;
            durianImage.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }
    }

    private void ApplyStaticUiSprites()
    {
        if (_staticUiApplied || spriteConfig == null)
        {
            return;
        }

        if (swipeGuideImage != null && spriteConfig.swipeGuideIcon != null)
        {
            swipeGuideImage.sprite = spriteConfig.swipeGuideIcon;
            swipeGuideImage.color = Color.white;
            swipeGuideImage.preserveAspect = true;
        }

        if (backButton != null)
        {
            SharedUiSpriteUtil.ApplyBackIcon(backButton, spriteConfig);
        }

        if (reviveButton != null)
        {
            SharedUiSpriteUtil.ApplyAdIcon(reviveButton, spriteConfig);
        }

        _staticUiApplied = true;
    }

    private void ShowSwipeGuide()
    {
        if (swipeGuideImage == null)
        {
            return;
        }

        swipeGuideImage.gameObject.SetActive(true);
        swipeGuideImage.transform.localScale = Vector3.one;
    }

    private void HideSwipeGuide()
    {
        if (swipeGuideImage == null)
        {
            return;
        }

        swipeGuideImage.transform.DOKill();
        swipeGuideImage.gameObject.SetActive(false);
        swipeGuideImage.transform.localScale = Vector3.one;
    }

    private void RefreshEstimateText()
    {
        if (estimateText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_lastRating))
        {
            estimateText.text = "在顶部滑动开果 · 出肉率开果后揭晓";
            return;
        }

        var price = _sellManager.CalculateSellPrice(_currentDurian);
        estimateText.text = $"出肉率 {_currentDurian.yieldRate:F1}% · {_lastRating} · 估价 {price} 金币";
    }

    private void HideActionButtons()
    {
        KillButtonTweens(sellButton);
        KillButtonTweens(reviveButton);

        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(false);
        }

        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(false);
        }
    }

    private static void ShowButtonPop(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(true);
        var rect = button.transform;
        rect.DOKill();
        rect.localScale = Vector3.zero;
        rect.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

    private void StartGuidePulse()
    {
        if (guideText == null)
        {
            return;
        }

        StopGuidePulse();
        guideText.transform.localScale = Vector3.one;
        _guidePulseTween = guideText.transform
            .DOScale(1.06f, 0.85f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        if (swipeGuideImage != null && swipeGuideImage.gameObject.activeSelf)
        {
            swipeGuideImage.transform.localScale = Vector3.one;
            _guideImagePulseTween = swipeGuideImage.transform
                .DOScale(1.06f, 0.85f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void StopGuidePulse()
    {
        _guidePulseTween?.Kill();
        _guidePulseTween = null;
        _guideImagePulseTween?.Kill();
        _guideImagePulseTween = null;

        if (guideText != null)
        {
            guideText.transform.DOKill();
            guideText.transform.localScale = Vector3.one;
        }

        if (swipeGuideImage != null)
        {
            swipeGuideImage.transform.DOKill();
            if (swipeGuideImage.gameObject.activeSelf)
            {
                swipeGuideImage.transform.localScale = Vector3.one;
            }
        }
    }

    private static void KillButtonTweens(Button button)
    {
        button?.transform.DOKill();
    }

    private void KillTweens()
    {
        StopGuidePulse();
        KillButtonTweens(sellButton);
        KillButtonTweens(reviveButton);

        if (durianImage != null)
        {
            durianImage.transform.DOKill();
        }
    }

    private void OnDisable()
    {
        _openedSub?.Dispose();
        UnbindKnifeEvents();
        KillTweens();
    }
}
