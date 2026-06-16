using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 市场选购页：选品种、买榴莲、试闻广告。
/// </summary>
public class MarketPage : MonoBehaviour
{
    [SerializeField] private Text goldText;
    [SerializeField] private Button[] varietyButtons;
    [SerializeField] private Image[] durianImages;
    [SerializeField] private Text[] priceTexts;
    [SerializeField] private Text[] appearanceTexts;
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Button[] smellButtons;
    [SerializeField] private Button bagButton;
    [SerializeField] private Button shopButton;

    private MarketManager _marketManager;
    private BagManager _bagManager;
    private AdManager _adManager;
    private GameUIRoot _uiRoot;
    private IDisposable _refreshedSub;
    private IDisposable _purchasedSub;
    private bool _initialized;
    private int _lastGold = -1;

    [Inject]
    public void Construct(
        MarketManager marketManager,
        BagManager bagManager,
        AdManager adManager,
        GameUIRoot uiRoot)
    {
        _marketManager = marketManager;
        _bagManager = bagManager;
        _adManager = adManager;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        Initialize();
        SubscribeEvents();
    }

    private void OnEnable()
    {
        if (_initialized)
        {
            SubscribeEvents();
            RefreshGold();
            RefreshCards();
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        KillTweens();
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();
        _refreshedSub = EventBus.Subscribe<MarketRefreshedEvent>(_ => RefreshCards());
        _purchasedSub = EventBus.Subscribe<DurianPurchasedEvent>(OnPurchased);
    }

    private void UnsubscribeEvents()
    {
        _refreshedSub?.Dispose();
        _purchasedSub?.Dispose();
        _refreshedSub = null;
        _purchasedSub = null;
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        EnsureReferences();
        BindButtons();
        RefreshGold();
        _marketManager?.RefreshMarket(VarietyType.JinZheng);
        _initialized = true;
    }

    private void EnsureReferences()
    {
        if (goldText == null)
        {
            goldText = transform.Find("Header/GoldText")?.GetComponent<Text>();
        }

        if (varietyButtons == null || varietyButtons.Length == 0)
        {
            var row = transform.Find("VarietyRow");
            if (row != null)
            {
                varietyButtons = row.GetComponentsInChildren<Button>(true);
            }
        }

        if (buyButtons == null || buyButtons.Length == 0)
        {
            buyButtons = FindCardButtons("Buy");
        }

        if (smellButtons == null || smellButtons.Length == 0)
        {
            smellButtons = FindCardButtons("Smell");
        }

        if (durianImages == null || durianImages.Length == 0)
        {
            durianImages = FindCardImages("DurianImage");
        }

        if (priceTexts == null || priceTexts.Length == 0)
        {
            priceTexts = FindCardTexts("Price");
        }

        if (appearanceTexts == null || appearanceTexts.Length == 0)
        {
            appearanceTexts = FindCardTexts("Appearance");
        }

        if (bagButton == null)
        {
            bagButton = transform.Find("Footer/Bag")?.GetComponent<Button>();
        }

        if (shopButton == null)
        {
            shopButton = transform.Find("Footer/Shop")?.GetComponent<Button>();
        }
    }

    private Button[] FindCardButtons(string childName)
    {
        var cardRow = transform.Find("CardRow");
        if (cardRow == null)
        {
            return Array.Empty<Button>();
        }

        var list = new System.Collections.Generic.List<Button>();
        for (var i = 0; i < cardRow.childCount; i++)
        {
            var btn = cardRow.GetChild(i).Find(childName)?.GetComponent<Button>();
            if (btn != null)
            {
                list.Add(btn);
            }
        }

        return list.ToArray();
    }

    private Image[] FindCardImages(string childName)
    {
        var cardRow = transform.Find("CardRow");
        if (cardRow == null)
        {
            return Array.Empty<Image>();
        }

        var list = new System.Collections.Generic.List<Image>();
        for (var i = 0; i < cardRow.childCount; i++)
        {
            var image = cardRow.GetChild(i).Find(childName)?.GetComponent<Image>();
            if (image != null)
            {
                list.Add(image);
            }
        }

        return list.ToArray();
    }

    private Text[] FindCardTexts(string childName)
    {
        var cardRow = transform.Find("CardRow");
        if (cardRow == null)
        {
            return Array.Empty<Text>();
        }

        var list = new System.Collections.Generic.List<Text>();
        for (var i = 0; i < cardRow.childCount; i++)
        {
            var text = cardRow.GetChild(i).Find(childName)?.GetComponent<Text>();
            if (text != null)
            {
                list.Add(text);
            }
        }

        return list.ToArray();
    }

    private void BindButtons()
    {
        if (_marketManager == null || _uiRoot == null)
        {
            Debug.LogError("[MarketPage] 依赖注入失败，按钮无法绑定。请确认 GameLifetimeScope 已 Build。");
            return;
        }

        var varieties = new[] { VarietyType.JinZheng, VarietyType.GanYao, VarietyType.MaoShanWang };
        if (varietyButtons != null)
        {
            for (var i = 0; i < varietyButtons.Length && i < varieties.Length; i++)
            {
                if (varietyButtons[i] == null)
                {
                    continue;
                }

                var variety = varieties[i];
                varietyButtons[i].onClick.RemoveAllListeners();
                varietyButtons[i].onClick.AddListener(() => _marketManager.RefreshMarket(variety));
            }
        }

        if (buyButtons != null)
        {
            for (var i = 0; i < buyButtons.Length; i++)
            {
                if (buyButtons[i] == null)
                {
                    continue;
                }

                var index = i;
                buyButtons[i].onClick.RemoveAllListeners();
                buyButtons[i].onClick.AddListener(() => TryBuy(index));
            }
        }

        if (smellButtons != null)
        {
            for (var i = 0; i < smellButtons.Length; i++)
            {
                if (smellButtons[i] == null)
                {
                    continue;
                }

                var index = i;
                smellButtons[i].onClick.RemoveAllListeners();
                smellButtons[i].onClick.AddListener(() => TrySmell(index));
            }
        }

        if (bagButton != null)
        {
            bagButton.onClick.RemoveAllListeners();
            bagButton.onClick.AddListener(() => _uiRoot.ShowBag());
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(() => _uiRoot.ShowShop());
        }
    }

    private void TryBuy(int index)
    {
        if (_marketManager == null)
        {
            return;
        }

        var durians = _marketManager.CurrentMarketDurians;
        if (durians == null || index < 0 || index >= durians.Length)
        {
            return;
        }

        var durian = durians[index];
        if (PlayerData.Instance.Gold < durian.finalPrice)
        {
            PlayInsufficientGoldFeedback(index);
            return;
        }

        _marketManager.BuyDurian(index);
    }

    private void OnPurchased(DurianPurchasedEvent e)
    {
        _bagManager?.AddDurian(e.Durian);
        RefreshGold();
        _uiRoot?.ShowOpen(e.Durian);
    }

    private async void TrySmell(int index)
    {
        if (_marketManager == null || _adManager == null)
        {
            return;
        }

        var durians = _marketManager.CurrentMarketDurians;
        if (durians == null || index < 0 || index >= durians.Length)
        {
            return;
        }

        var success = await _adManager.ShowRewardedAd("free_smell");
        if (!success)
        {
            return;
        }

        var hint = durians[index].yieldGrade switch
        {
            YieldGrade.Empty or YieldGrade.Low => "气味偏弱，可能出肉不多",
            YieldGrade.High or YieldGrade.Perfect => "香气浓郁，或许不错",
            _ => "气味一般，中规中矩"
        };

        if (appearanceTexts != null && index < appearanceTexts.Length && appearanceTexts[index] != null)
        {
            var hintText = appearanceTexts[index];
            hintText.text = hint;
            hintText.transform.DOKill();
            hintText.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 4, 0.5f);
        }
    }

    private void PlayInsufficientGoldFeedback(int index)
    {
        if (priceTexts != null && index < priceTexts.Length && priceTexts[index] != null)
        {
            var priceLabel = priceTexts[index];
            priceLabel.DOKill();
            priceLabel.color = Color.red;
            priceLabel.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.5f);
            priceLabel.DOColor(Color.red, 0.12f).SetLoops(4, LoopType.Yoyo);
        }

        if (goldText != null)
        {
            goldText.rectTransform.DOKill();
            goldText.rectTransform.DOShakeAnchorPos(0.35f, new Vector2(12f, 0f), 20, 90f, false, true);
        }
    }

    private void RefreshGold()
    {
        var gold = PlayerData.Instance.Gold;
        if (goldText != null)
        {
            goldText.text = $"金币 {gold}";

            if (_lastGold >= 0 && gold != _lastGold)
            {
                goldText.transform.DOKill();
                goldText.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 5, 0.5f);
            }
        }

        _lastGold = gold;
    }

    private void RefreshCards()
    {
        if (_marketManager == null)
        {
            return;
        }

        RefreshGold();
        var durians = _marketManager.CurrentMarketDurians;
        if (durians == null || buyButtons == null)
        {
            return;
        }

        for (var i = 0; i < durians.Length; i++)
        {
            if (i >= buyButtons.Length)
            {
                break;
            }

            var durian = durians[i];
            if (durianImages != null && i < durianImages.Length && durianImages[i] != null)
            {
                durianImages[i].color = GetAppearanceColor(durian.appearance);
            }

            if (appearanceTexts != null && i < appearanceTexts.Length && appearanceTexts[i] != null)
            {
                appearanceTexts[i].text = durian.appearance.ToString();
            }

            var canAfford = PlayerData.Instance.Gold >= durian.finalPrice;
            if (priceTexts != null && i < priceTexts.Length && priceTexts[i] != null)
            {
                priceTexts[i].text = $"{durian.finalPrice}";
                priceTexts[i].color = canAfford ? Color.white : Color.red;
            }

            if (buyButtons[i] != null)
            {
                buyButtons[i].interactable = canAfford;
            }
        }

        PlayCardRefreshAnimation();
    }

    private void PlayCardRefreshAnimation()
    {
        var cardRow = transform.Find("CardRow");
        if (cardRow == null)
        {
            return;
        }

        for (var i = 0; i < cardRow.childCount; i++)
        {
            var card = cardRow.GetChild(i);
            card.DOKill();
            card.localScale = Vector3.one * 0.85f;
            card.DOScale(1f, 0.28f)
                .SetDelay(i * 0.06f)
                .SetEase(Ease.OutBack);
        }
    }

    private void KillTweens()
    {
        if (goldText != null)
        {
            goldText.rectTransform.DOKill();
        }

        if (priceTexts != null)
        {
            foreach (var price in priceTexts)
            {
                if (price == null)
                {
                    continue;
                }

                price.DOKill();
                price.transform.DOKill();
            }
        }

        if (appearanceTexts != null)
        {
            foreach (var text in appearanceTexts)
            {
                text?.transform.DOKill();
            }
        }

        var cardRow = transform.Find("CardRow");
        if (cardRow != null)
        {
            for (var i = 0; i < cardRow.childCount; i++)
            {
                cardRow.GetChild(i).DOKill();
            }
        }
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
