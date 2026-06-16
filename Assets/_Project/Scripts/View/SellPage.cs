using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 售卖页：固定回收价、看广告加价、确认卖出与金币 DOTween 动画。
/// </summary>
public class SellPage : MonoBehaviour
{
    [SerializeField] private Text summaryText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text goldText;
    [SerializeField] private CanvasGroup goldCanvasGroup;
    [SerializeField] private Button adBonusButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    [SerializeField] private float priceRollDuration = 0.4f;
    [SerializeField] private float goldAnimDuration = 0.6f;
    [SerializeField] private float goldFloatOffset = 80f;

    private SellManager _sellManager;
    private AdManager _adManager;
    private BagManager _bagManager;
    private GameUIRoot _uiRoot;

    private DurianData _currentDurian;
    private string _currentRating;
    private Vector2 _goldOriginAnchoredPos;
    private Sequence _activeSequence;
    private bool _isSelling;
    private bool _adBonusApplied;

    [Inject]
    public void Construct(
        SellManager sellManager,
        AdManager adManager,
        BagManager bagManager,
        GameUIRoot uiRoot)
    {
        _sellManager = sellManager;
        _adManager = adManager;
        _bagManager = bagManager;
        _uiRoot = uiRoot;
    }

    private void Awake()
    {
        EnsureGoldCanvasGroup();

        if (goldText != null)
        {
            _goldOriginAnchoredPos = goldText.rectTransform.anchoredPosition;
        }
    }

    private void Start()
    {
        if (adBonusButton != null)
        {
            adBonusButton.onClick.RemoveAllListeners();
            adBonusButton.onClick.AddListener(OnAdBonusClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmSell);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    public void Show(DurianData durian, string rating)
    {
        KillTweens();
        _isSelling = false;
        _adBonusApplied = false;
        _sellManager.ClearTemporaryBonus();
        _currentDurian = durian;
        _currentRating = rating;

        ResetGoldVisual();
        SetButtonsInteractable(true);
        ResetAdBonusButton();
        RefreshPrice();
    }

    private async void OnAdBonusClicked()
    {
        if (adBonusButton != null && !adBonusButton.interactable)
        {
            return;
        }

        var success = await _adManager.ShowRewardedAd("sel_bonus");
        if (!success)
        {
            return;
        }

        var oldPrice = _sellManager.CalculateSellPrice(_currentDurian);
        _sellManager.ApplyAdBonus();
        var newPrice = _sellManager.CalculateSellPrice(_currentDurian);

        AnimatePriceRoll(oldPrice, newPrice);
        SetAdBonusApplied();
    }

    private async void OnConfirmSell()
    {
        if (_isSelling)
        {
            return;
        }

        _isSelling = true;
        SetButtonsInteractable(false);

        var price = _sellManager.CalculateSellPrice(_currentDurian);
        _sellManager.SellDurian(_currentDurian);
        RemoveFromBag(_currentDurian);

        await PlayGoldAnimAsync(price);
        _uiRoot.ShowMarket();
        _isSelling = false;
    }

    private void OnBackClicked()
    {
        _sellManager.ClearTemporaryBonus();
        KillTweens();
        _uiRoot?.ShowMarket();
    }

    private async UniTask PlayGoldAnimAsync(int targetPrice)
    {
        if (goldText == null)
        {
            return;
        }

        EnsureGoldCanvasGroup();
        var goldRect = goldText.rectTransform;
        goldRect.anchoredPosition = _goldOriginAnchoredPos;
        goldText.gameObject.SetActive(true);
        goldCanvasGroup.alpha = 0f;

        var displayValue = 0f;
        KillTweens();

        _activeSequence = DOTween.Sequence();
        _activeSequence.Append(goldCanvasGroup.DOFade(1f, 0.25f));
        _activeSequence.Join(DOTween.To(
            () => displayValue,
            value =>
            {
                displayValue = value;
                goldText.text = $"+{Mathf.RoundToInt(value)}";
            },
            targetPrice,
            goldAnimDuration).SetEase(Ease.OutCubic));
        _activeSequence.Join(goldRect
            .DOAnchorPosY(_goldOriginAnchoredPos.y + goldFloatOffset, goldAnimDuration)
            .SetEase(Ease.OutCubic));
        _activeSequence.Join(goldText.transform
            .DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 0.5f));
        _activeSequence.AppendInterval(0.4f);

        await _activeSequence.AsyncWaitForCompletion();
    }

    private void AnimatePriceRoll(int fromPrice, int toPrice)
    {
        if (priceText == null)
        {
            RefreshPrice();
            return;
        }

        priceText.transform.DOKill();
        var displayValue = (float)fromPrice;

        DOTween.To(
            () => displayValue,
            value =>
            {
                displayValue = value;
                priceText.text = $"固定回收价 {Mathf.RoundToInt(value)} 金币";
            },
            toPrice,
            priceRollDuration)
            .SetEase(Ease.OutCubic)
            .SetTarget(priceText);

        priceText.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 5, 0.5f);
    }

    private void RemoveFromBag(DurianData durian)
    {
        for (var i = 0; i < _bagManager.Durians.Count; i++)
        {
            if (_bagManager.Durians[i].id == durian.id)
            {
                _bagManager.RemoveDurian(i);
                break;
            }
        }
    }

    private void RefreshPrice()
    {
        if (summaryText != null)
        {
            summaryText.text = $"出肉率 {_currentDurian.yieldRate:F1}% · {_currentRating}";
        }

        if (priceText != null)
        {
            priceText.text = $"固定回收价 {_sellManager.CalculateSellPrice(_currentDurian)} 金币";
        }
    }

    private void ResetGoldVisual()
    {
        if (goldText == null)
        {
            return;
        }

        EnsureGoldCanvasGroup();
        goldText.text = string.Empty;
        goldText.rectTransform.anchoredPosition = _goldOriginAnchoredPos;
        goldText.transform.localScale = Vector3.one;
        goldCanvasGroup.alpha = 0f;
        goldText.gameObject.SetActive(false);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = interactable;
        }

        if (backButton != null)
        {
            backButton.interactable = interactable;
        }

        if (adBonusButton != null)
        {
            adBonusButton.interactable = interactable && !_adBonusApplied;
        }
    }

    private void ResetAdBonusButton()
    {
        if (adBonusButton == null)
        {
            return;
        }

        adBonusButton.interactable = true;
        var label = adBonusButton.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = "看广告加价+20%";
        }
    }

    private void SetAdBonusApplied()
    {
        _adBonusApplied = true;

        if (adBonusButton == null)
        {
            return;
        }

        adBonusButton.interactable = false;
        var label = adBonusButton.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = "已加价 +20%";
        }
    }

    private void EnsureGoldCanvasGroup()
    {
        if (goldText == null)
        {
            return;
        }

        if (goldCanvasGroup == null)
        {
            goldCanvasGroup = goldText.GetComponent<CanvasGroup>();
            if (goldCanvasGroup == null)
            {
                goldCanvasGroup = goldText.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void KillTweens()
    {
        _activeSequence?.Kill();
        _activeSequence = null;

        if (goldText != null)
        {
            goldText.transform.DOKill();
            goldText.rectTransform.DOKill();
        }

        if (goldCanvasGroup != null)
        {
            goldCanvasGroup.DOKill();
        }

        if (priceText != null)
        {
            priceText.DOKill();
            priceText.transform.DOKill();
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public DurianData CurrentDurian => _currentDurian;
    public string CurrentRating => _currentRating;
}
