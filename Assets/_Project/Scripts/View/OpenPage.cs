using System;
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
    [SerializeField] private Image durianImage;
    [SerializeField] private Text guideText;
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

        if (durianImage != null)
        {
            durianImage.color = GetAppearanceColor(durian.appearance);
            durianImage.transform.DOKill();
            durianImage.transform.localScale = Vector3.one * 0.92f;
            durianImage.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }

        if (guideText != null)
        {
            guideText.gameObject.SetActive(true);
            guideText.text = "在榴莲顶部滑动开果";
            StartGuidePulse();
        }

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

        ShowButtonPop(sellButton);
        if (e.Rating == "空壳")
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

        durianOpener?.ResetVisualState();
        RefreshEstimateText();
        HideActionButtons();

        if (guideText != null)
        {
            guideText.gameObject.SetActive(true);
            guideText.text = "在榴莲顶部滑动开果";
            StartGuidePulse();
        }

        if (knifeTool != null)
        {
            knifeTool.Setup(_currentDurian);
        }
    }

    private void RefreshEstimateText()
    {
        if (estimateText == null)
        {
            return;
        }

        var price = _sellManager.CalculateSellPrice(_currentDurian);
        estimateText.text = $"出肉率约 {_currentDurian.yieldRate:F1}% · 估价 {price} 金币";
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

    private void OnDisable()
    {
        _openedSub?.Dispose();
        KillTweens();
    }

    private static Color GetAppearanceColor(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => new Color(0.45f, 0.35f, 0.25f),
            AppearanceType.Good => new Color(0.9f, 0.55f, 0.1f),
            AppearanceType.Premium => new Color(1f, 0.84f, 0.2f),
            _ => new Color(0.3f, 0.65f, 0.35f)
        };
    }
}
