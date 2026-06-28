using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// v1.5 主界面壳层：全局顶栏（商店等级 / 金币 / 每日目标）+ 底栏导航。
/// </summary>
public class MainShellUI : MonoBehaviour
{
    public enum HubPage
    {
        Market,
        Bag,
        Shop,
        Collection
    }

    [SerializeField] private GameObject topBar;
    [SerializeField] private GameObject navBar;
    [SerializeField] private Text shopLevelText;
    [SerializeField] private Image goldIconImage;
    [SerializeField] private Text goldText;
    [SerializeField] private Text dailyTargetText;
    [SerializeField] private Button dailyTargetClaimButton;
    [SerializeField] private Button marketButton;
    [SerializeField] private Button bagButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button collectionButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Text questBadgeText;
    [SerializeField] private DurianSpriteConfig spriteConfig;

    private static readonly Color NavSelectedColor = new(1f, 0.82f, 0.35f);
    private static readonly Color NavNormalColor = new(0.35f, 0.45f, 0.55f);

    private GameUIRoot _uiRoot;
    private ShopManager _shopManager;
    private DailyTarget _dailyTarget;

    private IDisposable _purchasedSub;
    private IDisposable _soldSub;
    private IDisposable _upgradedSub;
    private IDisposable _dailyTargetSub;
    private IDisposable _marketRefreshedSub;
    private IDisposable _adRewardSub;

    private Tween _dailyTargetPulseTween;
    private int _lastGold = -1;
    private HubPage _activeHub = HubPage.Market;

    [Inject]
    public void Construct(GameUIRoot uiRoot, ShopManager shopManager, DailyTarget dailyTarget)
    {
        _uiRoot = uiRoot;
        _shopManager = shopManager;
        _dailyTarget = dailyTarget;
    }

    private void Start()
    {
        EnsureReferences();
        ApplyStaticSprites();
        BindButtons();
        TrySubscribeEvents();
        RefreshAll();
    }

    private void OnEnable()
    {
        TrySubscribeEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        StopDailyTargetPulse();
    }

    public void SetHubShellVisible(bool visible)
    {
        if (topBar != null)
        {
            topBar.SetActive(visible);
        }

        if (navBar != null)
        {
            navBar.SetActive(visible);
        }

        if (visible)
        {
            RefreshAll();
        }
    }

    public void SetActiveHub(HubPage page)
    {
        _activeHub = page;
        UpdateNavHighlight();
    }

    private void EnsureReferences()
    {
        if (topBar == null)
        {
            topBar = transform.Find("TopBar")?.gameObject;
        }

        if (navBar == null)
        {
            navBar = transform.Find("NavBar")?.gameObject;
        }

        if (shopLevelText == null)
        {
            shopLevelText = transform.Find("TopBar/ShopLevelText")?.GetComponent<Text>();
        }

        if (goldIconImage == null)
        {
            goldIconImage = transform.Find("TopBar/GoldIcon")?.GetComponent<Image>();
        }

        if (goldText == null)
        {
            goldText = transform.Find("TopBar/GoldText")?.GetComponent<Text>();
        }

        if (dailyTargetText == null)
        {
            dailyTargetText = transform.Find("TopBar/DailyTargetText")?.GetComponent<Text>();
        }

        if (dailyTargetClaimButton == null)
        {
            dailyTargetClaimButton = transform.Find("TopBar/DailyTargetClaim")?.GetComponent<Button>();
        }

        if (marketButton == null)
        {
            marketButton = transform.Find("NavBar/Market")?.GetComponent<Button>();
        }

        if (bagButton == null)
        {
            bagButton = transform.Find("NavBar/Bag")?.GetComponent<Button>();
        }

        if (shopButton == null)
        {
            shopButton = transform.Find("NavBar/Shop")?.GetComponent<Button>();
        }

        if (collectionButton == null)
        {
            collectionButton = transform.Find("NavBar/Collection")?.GetComponent<Button>();
        }

        if (questButton == null)
        {
            questButton = transform.Find("NavBar/Quest")?.GetComponent<Button>();
        }

        if (questBadgeText == null)
        {
            questBadgeText = transform.Find("NavBar/Quest/Badge")?.GetComponent<Text>();
        }
    }

    private void ApplyStaticSprites()
    {
        if (spriteConfig == null)
        {
            return;
        }

        if (goldIconImage != null && spriteConfig.goldCoinIcon != null)
        {
            goldIconImage.sprite = spriteConfig.goldCoinIcon;
            goldIconImage.color = Color.white;
        }
    }

    private void BindButtons()
    {
        if (marketButton != null)
        {
            marketButton.onClick.RemoveAllListeners();
            marketButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }

        if (bagButton != null)
        {
            bagButton.onClick.RemoveAllListeners();
            bagButton.onClick.AddListener(() => _uiRoot?.ShowBag());
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(() => _uiRoot?.ShowShop());
        }

        if (collectionButton != null)
        {
            collectionButton.onClick.RemoveAllListeners();
            collectionButton.onClick.AddListener(() => _uiRoot?.ShowCollection());
        }

        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(OnQuestClicked);
        }

        if (dailyTargetClaimButton != null)
        {
            dailyTargetClaimButton.onClick.RemoveAllListeners();
            dailyTargetClaimButton.onClick.AddListener(OnDailyTargetClaimClicked);
        }
    }

    private void TrySubscribeEvents()
    {
        if (!EventBus.IsReady)
        {
            return;
        }

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();
        _purchasedSub = EventBus.Subscribe<DurianPurchasedEvent>(_ => RefreshGold());
        _soldSub = EventBus.Subscribe<DurianSoldEvent>(_ => RefreshGold());
        _upgradedSub = EventBus.Subscribe<ShopUpgradedEvent>(_ => RefreshShopLevel());
        _dailyTargetSub = EventBus.Subscribe<DailyTargetUpdatedEvent>(_ => RefreshDailyTargetUi());
        _marketRefreshedSub = EventBus.Subscribe<MarketRefreshedEvent>(_ => RefreshGold());
        _adRewardSub = EventBus.Subscribe<AdRewardEvent>(_ => RefreshGold());
    }

    private void UnsubscribeEvents()
    {
        _purchasedSub?.Dispose();
        _soldSub?.Dispose();
        _upgradedSub?.Dispose();
        _dailyTargetSub?.Dispose();
        _marketRefreshedSub?.Dispose();
        _adRewardSub?.Dispose();
        _purchasedSub = null;
        _soldSub = null;
        _upgradedSub = null;
        _dailyTargetSub = null;
        _marketRefreshedSub = null;
        _adRewardSub = null;
    }

    private void RefreshAll()
    {
        RefreshShopLevel();
        RefreshGold();
        RefreshDailyTargetUi();
        UpdateNavHighlight();
    }

    private void RefreshShopLevel()
    {
        if (shopLevelText == null || _shopManager == null)
        {
            return;
        }

        shopLevelText.text = $"🛒 Lv.{_shopManager.CurrentLevel}";
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

    private void RefreshDailyTargetUi()
    {
        if (_dailyTarget == null)
        {
            return;
        }

        if (dailyTargetText != null)
        {
            if (_dailyTarget.IsClaimed)
            {
                dailyTargetText.text = "🎯 今日已领";
                dailyTargetText.color = Color.white;
                StopDailyTargetPulse();
            }
            else if (_dailyTarget.IsCompleted)
            {
                dailyTargetText.text = $"🎯 {DailyTarget.TargetGold}/{DailyTarget.TargetGold}";
                dailyTargetText.color = new Color(1f, 0.92f, 0.45f);
                StartDailyTargetPulse();
            }
            else
            {
                dailyTargetText.text = $"🎯 {_dailyTarget.EarnedToday}/{DailyTarget.TargetGold}";
                dailyTargetText.color = Color.white;
                StopDailyTargetPulse();
            }
        }

        if (dailyTargetClaimButton != null)
        {
            var canClaim = _dailyTarget.IsCompleted && !_dailyTarget.IsClaimed;
            dailyTargetClaimButton.gameObject.SetActive(canClaim);
            dailyTargetClaimButton.interactable = canClaim;
        }

        RefreshQuestBadge();
    }

    private void RefreshQuestBadge()
    {
        if (questBadgeText == null || _dailyTarget == null)
        {
            return;
        }

        if (_dailyTarget.IsClaimed)
        {
            questBadgeText.gameObject.SetActive(false);
            return;
        }

        questBadgeText.gameObject.SetActive(true);
        if (_dailyTarget.IsCompleted)
        {
            questBadgeText.text = "!";
        }
        else
        {
            questBadgeText.text = $"{_dailyTarget.EarnedToday}/{DailyTarget.TargetGold}";
        }
    }

    private void OnDailyTargetClaimClicked()
    {
        if (_dailyTarget == null || !_dailyTarget.ClaimReward())
        {
            return;
        }

        RefreshGold();
        RefreshDailyTargetUi();

        if (dailyTargetText != null)
        {
            dailyTargetText.transform.DOKill();
            dailyTargetText.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 0.5f);
        }
    }

    private void OnQuestClicked()
    {
        Debug.Log("[Quest] 每日任务入口占位（v1.5 MVP）— 当前进度见顶栏 🎯");
    }

    private void UpdateNavHighlight()
    {
        SetNavButtonColor(marketButton, _activeHub == HubPage.Market);
        SetNavButtonColor(bagButton, _activeHub == HubPage.Bag);
        SetNavButtonColor(shopButton, _activeHub == HubPage.Shop);
        SetNavButtonColor(collectionButton, _activeHub == HubPage.Collection);
    }

    private static void SetNavButtonColor(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = selected ? NavSelectedColor : NavNormalColor;
        }
    }

    private void StartDailyTargetPulse()
    {
        if (dailyTargetText == null)
        {
            return;
        }

        StopDailyTargetPulse();
        dailyTargetText.transform.localScale = Vector3.one;
        _dailyTargetPulseTween = dailyTargetText.transform
            .DOScale(1.08f, 0.55f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopDailyTargetPulse()
    {
        _dailyTargetPulseTween?.Kill();
        _dailyTargetPulseTween = null;

        if (dailyTargetText != null)
        {
            dailyTargetText.transform.DOKill();
            dailyTargetText.transform.localScale = Vector3.one;
        }
    }
}
