using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 商店升级页（v1.5 3 级）+ 去广告卡展示 + 升级光柱特效。
/// </summary>
public class ShopPage : MonoBehaviour
{
    private static readonly Color NormalTextColor = Color.white;
    private static readonly Color MutedTextColor = new(0.65f, 0.65f, 0.65f);

    [SerializeField] private Text levelText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private GameObject removeAdsCard;
    [SerializeField] private Text removeAdsBadgeText;
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private Transform upgradeEffectRoot;

    private ShopManager _shopManager;
    private GameUIRoot _uiRoot;
    private IDisposable _upgradedSub;
    private Tween _badgePulseTween;
    private bool _isUpgrading;

    [Inject]
    public void Construct(ShopManager shopManager, GameUIRoot uiRoot)
    {
        _shopManager = shopManager;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        EnsureReferences();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => OnUpgradeAsync().Forget());
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }

        if (removeAdsButton != null)
        {
            removeAdsButton.onClick.RemoveAllListeners();
            removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
        }

        SharedUiSpriteUtil.ApplyBackIcon(backButton, spriteConfig);
        StartBadgePulse();
    }

    private void OnEnable()
    {
        _upgradedSub?.Dispose();
        _upgradedSub = EventBus.Subscribe<ShopUpgradedEvent>(_ => Refresh());
        Refresh();
        StartBadgePulse();
    }

    private void OnDisable()
    {
        _upgradedSub?.Dispose();
        _upgradedSub = null;
        StopBadgePulse();

        if (effectText != null)
        {
            effectText.transform.DOKill();
        }
    }

    private void EnsureReferences()
    {
        if (removeAdsCard == null)
        {
            removeAdsCard = transform.Find("RemoveAdsCard")?.gameObject;
        }

        if (removeAdsBadgeText == null)
        {
            removeAdsBadgeText = transform.Find("RemoveAdsCard/Badge")?.GetComponent<Text>();
        }

        if (removeAdsButton == null)
        {
            removeAdsButton = transform.Find("RemoveAdsCard")?.GetComponent<Button>();
        }

        if (upgradeEffectRoot == null)
        {
            upgradeEffectRoot = transform;
        }
    }

    public void Refresh()
    {
        if (_shopManager == null)
        {
            return;
        }

        var isMaxLevel = _shopManager.CurrentLevel >= _shopManager.MaxLevel;

        if (levelText != null)
        {
            levelText.text = $"商店 Lv.{_shopManager.CurrentLevel}";
            levelText.color = NormalTextColor;
        }

        if (effectText != null)
        {
            if (isMaxLevel)
            {
                var bonus = _shopManager.GetSellBonus();
                effectText.text = $"已满级 · 回收价 +{bonus * 100f:F0}%";
                effectText.color = MutedTextColor;
            }
            else
            {
                var nextLevel = _shopManager.GetNextLevel();
                var currentBonus = _shopManager.GetSellBonus();
                var nextBonus = _shopManager.GetNextLevelSellBonus();
                effectText.text =
                    $"当前回收 +{currentBonus * 100f:F0}% → Lv.{nextLevel} +{nextBonus * 100f:F0}%";
                effectText.color = NormalTextColor;
            }
        }

        if (upgradeButton == null)
        {
            return;
        }

        var label = upgradeButton.GetComponentInChildren<Text>();
        if (isMaxLevel)
        {
            upgradeButton.interactable = false;
            if (label != null)
            {
                label.text = "已满级";
            }

            return;
        }

        var cost = _shopManager.GetUpgradeCost();
        var next = _shopManager.GetNextLevel();
        upgradeButton.interactable = _shopManager.CanUpgrade() && !_isUpgrading;
        if (label != null)
        {
            label.text = $"升级到 Lv.{next}（{cost}金币）";
        }
    }

    private async UniTaskVoid OnUpgradeAsync()
    {
        if (_shopManager == null || !_shopManager.CanUpgrade() || _isUpgrading)
        {
            return;
        }

        _isUpgrading = true;
        upgradeButton.interactable = false;

        await PlayUpgradeEffectAsync();
        _shopManager.Upgrade();
        Refresh();
        PlayUpgradeFeedback();
        _isUpgrading = false;
    }

    private async UniTask PlayUpgradeEffectAsync()
    {
        if (spriteConfig == null || spriteConfig.upgradeEffect == null)
        {
            await UniTask.Delay(300);
            return;
        }

        var effectGo = new GameObject("UpgradeEffect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        effectGo.transform.SetParent(upgradeEffectRoot, false);

        var rect = effectGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.35f, 0f);
        rect.anchorMax = new Vector2(0.65f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(120f, 280f);
        rect.anchoredPosition = new Vector2(0f, 80f);

        var image = effectGo.GetComponent<Image>();
        image.sprite = spriteConfig.upgradeEffect;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = true;

        var canvasGroup = effectGo.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        var sequence = DOTween.Sequence();
        sequence.Join(rect.DOLocalMoveY(500f, 0.8f).SetEase(Ease.OutQuad));
        sequence.Join(canvasGroup.DOFade(0f, 0.8f));
        await sequence.AsyncWaitForCompletion();

        Destroy(effectGo);
    }

    private void PlayUpgradeFeedback()
    {
        if (effectText == null)
        {
            return;
        }

        effectText.transform.DOKill();
        effectText.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 5, 0.5f);
    }

    private void OnRemoveAdsClicked()
    {
        Debug.Log("[ShopPage] 去广告卡为 MVP 展示样式，支付功能尚未接入。");
    }

    private void StartBadgePulse()
    {
        if (removeAdsBadgeText == null)
        {
            return;
        }

        StopBadgePulse();
        removeAdsBadgeText.color = new Color(1f, 0.85f, 0.35f);
        _badgePulseTween = removeAdsBadgeText
            .DOColor(new Color(1f, 0.65f, 0.1f), 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBadgePulse()
    {
        _badgePulseTween?.Kill();
        _badgePulseTween = null;
    }
}
