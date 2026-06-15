using System;
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

    private void OnEnable()
    {
        _refreshedSub = EventBus.Subscribe<MarketRefreshedEvent>(_ => RefreshCards());
        _purchasedSub = EventBus.Subscribe<DurianPurchasedEvent>(OnPurchased);
        BindButtons();
        RefreshGold();
        _marketManager.RefreshMarket(VarietyType.JinZheng);
    }

    private void OnDisable()
    {
        _refreshedSub?.Dispose();
        _purchasedSub?.Dispose();
    }

    private void BindButtons()
    {
        var varieties = new[] { VarietyType.JinZheng, VarietyType.GanYao, VarietyType.MaoShanWang };
        for (var i = 0; i < varietyButtons.Length && i < varieties.Length; i++)
        {
            var variety = varieties[i];
            varietyButtons[i].onClick.RemoveAllListeners();
            varietyButtons[i].onClick.AddListener(() =>
            {
                _marketManager.RefreshMarket(variety);
            });
        }

        for (var i = 0; i < buyButtons.Length; i++)
        {
            var index = i;
            buyButtons[i].onClick.RemoveAllListeners();
            buyButtons[i].onClick.AddListener(() => TryBuy(index));
        }

        for (var i = 0; i < smellButtons.Length; i++)
        {
            var index = i;
            smellButtons[i].onClick.RemoveAllListeners();
            smellButtons[i].onClick.AddListener(() => TrySmell(index));
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
        var durians = _marketManager.CurrentMarketDurians;
        if (durians == null || index < 0 || index >= durians.Length)
        {
            return;
        }

        var durian = durians[index];
        if (PlayerData.Instance.Gold < durian.finalPrice)
        {
            return;
        }

        _marketManager.BuyDurian(index);
    }

    private void OnPurchased(DurianPurchasedEvent e)
    {
        _bagManager.AddDurian(e.Durian);
        RefreshGold();
        _uiRoot.ShowOpen(e.Durian);
    }

    private async void TrySmell(int index)
    {
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
            appearanceTexts[index].text = hint;
        }
    }

    private void RefreshGold()
    {
        if (goldText != null)
        {
            goldText.text = $"金币 {PlayerData.Instance.Gold}";
        }
    }

    private void RefreshCards()
    {
        RefreshGold();
        var durians = _marketManager.CurrentMarketDurians;
        if (durians == null)
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
            if (i < durianImages.Length && durianImages[i] != null)
            {
                durianImages[i].color = GetAppearanceColor(durian.appearance);
            }

            if (i < appearanceTexts.Length && appearanceTexts[i] != null)
            {
                appearanceTexts[i].text = durian.appearance.ToString();
            }

            var canAfford = PlayerData.Instance.Gold >= durian.finalPrice;
            if (i < priceTexts.Length && priceTexts[i] != null)
            {
                priceTexts[i].text = $"{durian.finalPrice}";
                priceTexts[i].color = canAfford ? Color.white : Color.red;
            }

            if (i < buyButtons.Length && buyButtons[i] != null)
            {
                buyButtons[i].interactable = canAfford;
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
