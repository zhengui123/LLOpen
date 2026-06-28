using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 开榴莲页：v1.5 逐房捅开 + 见好就收 + 全开后卖出。
/// </summary>
public class OpenPage : MonoBehaviour
{
    [SerializeField] private DurianOpener durianOpener;
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Image durianImage;
    [SerializeField] private Text guideText;
    [SerializeField] private Text estimateText;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button reviveButton;
    [SerializeField] private Button backButton;

    [Header("v1.5 连击")]
    [SerializeField] private GameObject comboDisplay;
    [SerializeField] private Image comboFlameImage;
    [SerializeField] private Text comboText;

    private SellManager _sellManager;
    private AdManager _adManager;
    private BagManager _bagManager;
    private DurianGeneratorSystem _durianGenerator;
    private GameUIRoot _uiRoot;

    private DurianData _currentDurian;
    private string _lastRating;
    private int _overrideSellPrice = -1;
    private IDisposable _openedSub;
    private IDisposable _midwaySoldSub;
    private IDisposable _streakSub;
    private Tween _guidePulseTween;
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
        _overrideSellPrice = -1;

        UnsubscribeEvents();
        _openedSub = EventBus.Subscribe<DurianOpenedEvent>(OnDurianOpened);
        _midwaySoldSub = EventBus.Subscribe<DurianMidwaySoldEvent>(OnMidwaySold);
        _streakSub = EventBus.Subscribe<StreakUpdatedEvent>(OnStreakUpdated);

        HideActionButtons();
        HideComboDisplay();
        DisableLegacySwipeBlockers();

        if (durianOpener != null)
        {
            durianOpener.PrepareOpen(durian);
        }

        if (durianImage != null)
        {
            durianImage.gameObject.SetActive(true);
            durianImage.transform.DOKill();
            durianImage.transform.localScale = Vector3.one * 0.92f;
            durianImage.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }

        if (guideText != null)
        {
            guideText.gameObject.SetActive(true);
            guideText.text = "点击或上下滑动壳盖，逐房开果";
            StartGuidePulse();
        }

        RefreshEstimateText();
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

        ShowOpenedActionsAsync().Forget();
    }

    private void OnMidwaySold(DurianMidwaySoldEvent e)
    {
        if (e.Durian.id != _currentDurian.id)
        {
            return;
        }

        _overrideSellPrice = e.EstimatePrice;
        _lastRating = GradeToRatingText(EstimateGradeFromPrice(e.EstimatePrice));
        _uiRoot?.ShowSell(_currentDurian, _lastRating, e.EstimatePrice);
    }

    private void OnStreakUpdated(StreakUpdatedEvent e)
    {
        if (comboDisplay == null)
        {
            return;
        }

        if (e.Combo < 2)
        {
            comboDisplay.SetActive(false);
            return;
        }

        comboDisplay.SetActive(true);
        if (comboText != null)
        {
            comboText.text = e.Combo.ToString();
        }

        if (comboFlameImage != null && spriteConfig != null && spriteConfig.comboFlameFx != null)
        {
            comboFlameImage.sprite = spriteConfig.comboFlameFx;
            comboFlameImage.color = Color.white;
        }
    }

    private async UniTaskVoid ShowOpenedActionsAsync()
    {
        await UniTask.Delay(300);
        ShowButtonPop(sellButton);
        if (_lastRating == "空壳")
        {
            ShowButtonPop(reviveButton);
        }
    }

    private void OnSellClicked()
    {
        _uiRoot?.ShowSell(_currentDurian, _lastRating, _overrideSellPrice);
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
        Show(_currentDurian);
    }

    private void RefreshEstimateText()
    {
        if (estimateText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_lastRating))
        {
            estimateText.text = "逐房捅开壳盖 · 首房后可见好就收";
            return;
        }

        var price = _overrideSellPrice >= 0
            ? _overrideSellPrice
            : _sellManager.CalculateSellPrice(_currentDurian);
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

    private void HideComboDisplay()
    {
        if (comboDisplay != null)
        {
            comboDisplay.SetActive(false);
        }
    }

    /// <summary>v1.5 逐房开果：关闭 v1.2 划刀区域对点击的拦截。</summary>
    private void DisableLegacySwipeBlockers()
    {
        var swipeArea = transform.Find("SwipeArea")?.GetComponent<Image>();
        if (swipeArea != null)
        {
            swipeArea.raycastTarget = false;
        }

        var knifeTool = transform.Find("KnifeTool");
        if (knifeTool != null)
        {
            knifeTool.gameObject.SetActive(false);
        }

        var crackOverlay = transform.Find("CrackOverlay")?.GetComponent<Image>();
        if (crackOverlay != null)
        {
            crackOverlay.raycastTarget = false;
        }
    }

    private void ApplyStaticUiSprites()
    {
        if (_staticUiApplied || spriteConfig == null)
        {
            return;
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
    }

    private void StopGuidePulse()
    {
        _guidePulseTween?.Kill();
        _guidePulseTween = null;

        if (guideText != null)
        {
            guideText.transform.DOKill();
            guideText.transform.localScale = Vector3.one;
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

    private void UnsubscribeEvents()
    {
        _openedSub?.Dispose();
        _midwaySoldSub?.Dispose();
        _streakSub?.Dispose();
        _openedSub = null;
        _midwaySoldSub = null;
        _streakSub = null;
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        KillTweens();
    }

    private static string GradeToRatingText(YieldGrade grade)
    {
        return grade switch
        {
            YieldGrade.Empty => "空壳",
            YieldGrade.Low => "小亏",
            YieldGrade.Normal => "回本",
            YieldGrade.High => "小赚",
            YieldGrade.Perfect => "大赚",
            _ => "回本"
        };
    }

    private static YieldGrade EstimateGradeFromPrice(int price)
    {
        if (price <= 0)
        {
            return YieldGrade.Empty;
        }

        if (price < 80)
        {
            return YieldGrade.Low;
        }

        if (price < 150)
        {
            return YieldGrade.Normal;
        }

        if (price < 220)
        {
            return YieldGrade.High;
        }

        return YieldGrade.Perfect;
    }
}
