using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 售卖页：固定回收价、看广告加价、确认卖出。
/// </summary>
public class SellPage : MonoBehaviour
{
    [SerializeField] private Text summaryText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text goldText;
    [SerializeField] private Button adBonusButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    private SellManager _sellManager;
    private AdManager _adManager;
    private BagManager _bagManager;
    private GameUIRoot _uiRoot;

    private DurianData _currentDurian;
    private string _currentRating;

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
        if (adBonusButton != null)
        {
            adBonusButton.onClick.AddListener(OnAdBonusClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmSell);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(() => _uiRoot.ShowMarket());
        }
    }

    public void Show(DurianData durian, string rating)
    {
        _currentDurian = durian;
        _currentRating = rating;
        RefreshPrice();
    }

    private async void OnAdBonusClicked()
    {
        var success = await _adManager.ShowRewardedAd("customer_bonus");
        if (!success)
        {
            return;
        }

        _sellManager.ApplyAdBonus();
        RefreshPrice();
    }

    private void OnConfirmSell()
    {
        var price = _sellManager.CalculateSellPrice(_currentDurian);
        _sellManager.SellDurian(_currentDurian);
        RemoveFromBag(_currentDurian);

        if (goldText != null)
        {
            goldText.text = $"+{price}";
        }

        _uiRoot.ShowMarket();
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

    public DurianData CurrentDurian => _currentDurian;
    public string CurrentRating => _currentRating;
}
