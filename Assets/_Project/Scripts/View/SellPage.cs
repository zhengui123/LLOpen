using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 售卖页：金币飞行动画、出肉回顾、看广告加价、确认卖出。
/// </summary>
public class SellPage : MonoBehaviour
{
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Text summaryText;
    [SerializeField] private Image ratingIcon;
    [SerializeField] private Image durianResultImage;
    [SerializeField] private Transform roomReviewRoot;
    [SerializeField] private Text priceText;
    [SerializeField] private Text goldText;
    [SerializeField] private CanvasGroup goldCanvasGroup;
    [SerializeField] private RectTransform coinFlyTarget;
    [SerializeField] private Transform coinParticleRoot;
    [SerializeField] private Button adBonusButton;
    [SerializeField] private Image adBonusIcon;
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
    private Tween _adPulseTween;
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
        EnsureReferences();
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

    private int _overrideSellPrice = -1;

    public void Show(DurianData durian, string rating, int overridePrice = -1)
    {
        KillTweens();
        _isSelling = false;
        _adBonusApplied = false;
        _overrideSellPrice = overridePrice;
        _sellManager.ClearTemporaryBonus();
        _currentDurian = durian;
        _currentRating = rating;

        ResetGoldVisual();
        SetButtonsInteractable(true);
        ResetAdBonusButton();
        ApplyStaticUiSprites();
        RefreshDurianVisual();
        RefreshRoomReview();
        RefreshPrice();
        StartAdBonusPulse();
    }

    private void EnsureReferences()
    {
        if (durianResultImage == null)
        {
            durianResultImage = transform.Find("DurianResultImage")?.GetComponent<Image>();
        }

        if (roomReviewRoot == null)
        {
            roomReviewRoot = transform.Find("RoomReviewRow");
        }

        if (coinParticleRoot == null)
        {
            coinParticleRoot = transform.Find("CoinParticleRoot");
        }

        if (coinFlyTarget == null)
        {
            coinFlyTarget = transform.Find("CoinFlyTarget")?.GetComponent<RectTransform>();
        }

        if (coinParticleRoot == null)
        {
            var rootGo = new GameObject("CoinParticleRoot", typeof(RectTransform));
            rootGo.transform.SetParent(transform, false);
            var rect = rootGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            coinParticleRoot = rootGo.transform;
        }
    }

    private void ApplyStaticUiSprites()
    {
        if (spriteConfig == null)
        {
            return;
        }

        if (adBonusIcon != null && spriteConfig.watchAdIcon != null)
        {
            adBonusIcon.sprite = spriteConfig.watchAdIcon;
            adBonusIcon.color = Color.white;
        }

        SharedUiSpriteUtil.ApplyBackIcon(backButton, spriteConfig);
    }

    private void RefreshDurianVisual()
    {
        if (durianResultImage == null || spriteConfig == null)
        {
            return;
        }

        durianResultImage.sprite = spriteConfig.GetRf(_currentDurian.yieldGrade);
        durianResultImage.color = Color.white;
        durianResultImage.preserveAspect = true;
        durianResultImage.gameObject.SetActive(durianResultImage.sprite != null);
    }

    private void RefreshRoomReview()
    {
        if (roomReviewRoot == null || _currentDurian.roomResults == null || spriteConfig == null)
        {
            return;
        }

        for (var i = roomReviewRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(roomReviewRoot.GetChild(i).gameObject);
        }

        var roomResults = _currentDurian.roomResults;
        for (var i = 0; i < roomResults.Length; i++)
        {
            var iconGo = new GameObject($"Room_{i}", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(roomReviewRoot, false);
            var rect = iconGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(36f, 36f);

            var image = iconGo.GetComponent<Image>();
            image.sprite = roomResults[i] ? spriteConfig.fleshPiece : spriteConfig.emptyPiece;
            image.color = Color.white;
            image.preserveAspect = true;
        }
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
        StopAdBonusPulse();
    }

    private int GetDisplaySellPrice()
    {
        return _overrideSellPrice >= 0
            ? _overrideSellPrice
            : _sellManager.CalculateSellPrice(_currentDurian);
    }

    private async void OnConfirmSell()
    {
        if (_isSelling)
        {
            return;
        }

        _isSelling = true;
        SetButtonsInteractable(false);
        StopAdBonusPulse();

        var price = GetDisplaySellPrice();
        var goldBefore = PlayerData.Instance.Gold;

        var startPos = GetCoinStartWorldPosition();
        var targetPos = GetCoinTargetWorldPosition();
        await PlayCoinAnimation(price, startPos, targetPos);

        if (_overrideSellPrice >= 0)
        {
            _sellManager.SellAtPrice(_currentDurian, _overrideSellPrice);
        }
        else
        {
            _sellManager.SellDurian(_currentDurian);
        }

        RemoveFromBag(_currentDurian);

        await PlayGoldCounterAsync(goldBefore, PlayerData.Instance.Gold, price);
        _uiRoot.ShowMarket();
        _isSelling = false;
    }

    private void OnBackClicked()
    {
        _sellManager.ClearTemporaryBonus();
        KillTweens();
        StopAdBonusPulse();
        _uiRoot?.ShowMarket();
    }

    private Vector3 GetCoinStartWorldPosition()
    {
        if (durianResultImage != null)
        {
            return durianResultImage.rectTransform.position;
        }

        if (ratingIcon != null)
        {
            return ratingIcon.rectTransform.position;
        }

        return transform.position;
    }

    private Vector3 GetCoinTargetWorldPosition()
    {
        if (coinFlyTarget != null)
        {
            return coinFlyTarget.position;
        }

        return transform.position + new Vector3(0f, 300f, 0f);
    }

    private async UniTask PlayCoinAnimation(int amount, Vector3 startPos, Vector3 targetPos)
    {
        if (spriteConfig == null || coinParticleRoot == null)
        {
            return;
        }

        var coinSprite = spriteConfig.goldCoinParticle != null
            ? spriteConfig.goldCoinParticle
            : spriteConfig.goldCoinIcon;

        if (coinSprite == null)
        {
            return;
        }

        var coinCount = Mathf.Clamp(amount / 50, 1, 10);
        for (var i = 0; i < coinCount; i++)
        {
            var coinObj = new GameObject("CoinParticle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coinObj.transform.SetParent(coinParticleRoot, false);
            coinObj.transform.position = startPos + (Vector3)Random.insideUnitCircle * 12f;

            var image = coinObj.GetComponent<Image>();
            image.sprite = coinSprite;
            image.color = Color.white;
            image.raycastTarget = false;
            var rect = coinObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(48f, 48f);

            coinObj.transform.DOScale(0.3f, 0.5f).SetEase(Ease.InQuad);
            coinObj.transform.DOMove(targetPos, 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(coinObj));

            await UniTask.Delay(80);
        }

        await UniTask.Delay(350);
    }

    private async UniTask PlayGoldCounterAsync(int goldBefore, int goldAfter, int addedGold)
    {
        if (goldText == null)
        {
            return;
        }

        EnsureGoldCanvasGroup();
        var goldRect = goldText.rectTransform;
        goldRect.anchoredPosition = _goldOriginAnchoredPos;
        goldText.gameObject.SetActive(true);
        goldCanvasGroup.alpha = 1f;

        var displayGold = (float)goldBefore;
        var displayAdded = 0f;
        KillTweens();

        _activeSequence = DOTween.Sequence();
        _activeSequence.Append(DOTween.To(
            () => displayAdded,
            value =>
            {
                displayAdded = value;
                goldText.text = $"+{Mathf.RoundToInt(value)}";
            },
            addedGold,
            goldAnimDuration * 0.6f).SetEase(Ease.OutCubic));
        _activeSequence.Join(DOTween.To(
            () => displayGold,
            value =>
            {
                displayGold = value;
                if (summaryText != null)
                {
                    summaryText.text = $"金币 {Mathf.RoundToInt(value)}";
                }
            },
            goldAfter,
            goldAnimDuration).SetEase(Ease.OutCubic));
        _activeSequence.Join(goldRect
            .DOAnchorPosY(_goldOriginAnchoredPos.y + goldFloatOffset, goldAnimDuration)
            .SetEase(Ease.OutCubic));
        _activeSequence.Join(goldText.transform
            .DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 0.5f));
        _activeSequence.AppendInterval(0.35f);

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
                priceText.text = $"回收价 {Mathf.RoundToInt(value)} 金币";
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

        if (ratingIcon != null)
        {
            if (spriteConfig != null && !string.IsNullOrEmpty(_currentRating))
            {
                ratingIcon.sprite = spriteConfig.GetRatingSprite(_currentRating);
                ratingIcon.gameObject.SetActive(true);
            }
            else
            {
                ratingIcon.gameObject.SetActive(false);
            }
        }

        if (priceText != null)
        {
            priceText.text = $"回收价 {GetDisplaySellPrice()} 金币";
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

    private void StartAdBonusPulse()
    {
        if (adBonusButton == null || _adBonusApplied)
        {
            return;
        }

        StopAdBonusPulse();
        adBonusButton.transform.localScale = Vector3.one;
        _adPulseTween = adBonusButton.transform
            .DOScale(1.06f, 0.55f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopAdBonusPulse()
    {
        _adPulseTween?.Kill();
        _adPulseTween = null;

        if (adBonusButton != null)
        {
            adBonusButton.transform.localScale = Vector3.one;
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
        StopAdBonusPulse();
    }

    public DurianData CurrentDurian => _currentDurian;
    public string CurrentRating => _currentRating;
}
