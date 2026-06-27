using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 背包页：网格展示已购榴莲，空背包引导。
/// </summary>
public class BagPage : MonoBehaviour
{
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Text capacityText;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private Image emptyIllustImage;
    [SerializeField] private Text emptyHintText;
    [SerializeField] private Button goMarketButton;
    [SerializeField] private Button backButton;

    private BagManager _bagManager;
    private GameUIRoot _uiRoot;
    private IDisposable _bagUpdatedSub;

    [Inject]
    public void Construct(BagManager bagManager, GameUIRoot uiRoot)
    {
        _bagManager = bagManager;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        EnsureReferences();

        if (goMarketButton != null)
        {
            goMarketButton.onClick.RemoveAllListeners();
            goMarketButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }

        ApplyStaticUiSprites();
    }

    private void OnEnable()
    {
        _bagUpdatedSub?.Dispose();
        _bagUpdatedSub = EventBus.Subscribe<BagUpdatedEvent>(_ => Refresh());
    }

    private void OnDisable()
    {
        _bagUpdatedSub?.Dispose();
        _bagUpdatedSub = null;
        KillCardTweens();
    }

    private void EnsureReferences()
    {
        if (emptyStatePanel == null)
        {
            emptyStatePanel = transform.Find("EmptyStatePanel")?.gameObject;
        }

        if (emptyIllustImage == null)
        {
            emptyIllustImage = transform.Find("EmptyStatePanel/EmptyIllust")?.GetComponent<Image>();
        }

        if (emptyHintText == null)
        {
            emptyHintText = transform.Find("EmptyStatePanel/EmptyHintText")?.GetComponent<Text>();
        }

        if (goMarketButton == null)
        {
            goMarketButton = transform.Find("EmptyStatePanel/GoMarket")?.GetComponent<Button>()
                ?? transform.Find("GoMarket")?.GetComponent<Button>();
        }

        if (cardRoot == null)
        {
            cardRoot = transform.Find("CardRoot");
        }
    }

    private void ApplyStaticUiSprites()
    {
        if (spriteConfig == null || emptyIllustImage == null)
        {
            return;
        }

        if (spriteConfig.emptyBagIllust != null)
        {
            emptyIllustImage.sprite = spriteConfig.emptyBagIllust;
            emptyIllustImage.color = Color.white;
            emptyIllustImage.preserveAspect = true;
        }
    }

    public void Refresh()
    {
        if (_bagManager == null)
        {
            return;
        }

        EnsureReferences();
        UpdateBagDisplay();

        if (capacityText != null)
        {
            capacityText.text = $"{_bagManager.Durians.Count}/{_bagManager.MaxCapacity}";
        }

        if (cardRoot == null || cardPrefab == null || _bagManager.Durians.Count == 0)
        {
            return;
        }

        KillCardTweens();

        for (var i = cardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardRoot.GetChild(i).gameObject);
        }

        for (var i = 0; i < _bagManager.Durians.Count; i++)
        {
            SpawnCard(_bagManager.Durians[i], i);
        }
    }

    private void UpdateBagDisplay()
    {
        var isEmpty = _bagManager.Durians.Count == 0;

        if (emptyStatePanel != null)
        {
            emptyStatePanel.SetActive(isEmpty);
        }

        if (emptyHintText != null)
        {
            emptyHintText.text = "你的背包空空如也，去市场挑选榴莲吧！";
        }

        if (cardRoot != null)
        {
            cardRoot.gameObject.SetActive(!isEmpty);
        }
    }

    private void SpawnCard(DurianData durian, int index)
    {
        var card = Instantiate(cardPrefab, cardRoot);
        card.SetActive(true);
        card.transform.localScale = Vector3.zero;

        var blockImage = card.transform.Find("Block")?.GetComponent<Image>();
        if (blockImage != null)
        {
            if (spriteConfig != null)
            {
                blockImage.sprite = spriteConfig.GetUnopenedSprite(durian.variety, durian.appearance);
                blockImage.color = Color.white;
                blockImage.preserveAspect = true;
            }
            else
            {
                blockImage.color = DurianDisplayUtil.GetAppearanceColor(durian.appearance);
            }
        }

        var appearanceIcon = card.transform.Find("AppearanceIcon")?.GetComponent<Image>();
        if (appearanceIcon != null)
        {
            if (spriteConfig != null)
            {
                appearanceIcon.sprite = spriteConfig.GetAppearanceIcon(durian.appearance);
                appearanceIcon.gameObject.SetActive(true);
            }
            else
            {
                appearanceIcon.gameObject.SetActive(false);
            }
        }

        var varietyText = card.transform.Find("VarietyText")?.GetComponent<Text>();
        if (varietyText != null)
        {
            varietyText.text = DurianDisplayUtil.GetVarietyName(durian.variety);
        }

        var infoText = card.transform.Find("InfoText")?.GetComponent<Text>();
        if (infoText != null)
        {
            infoText.text = $"{DurianDisplayUtil.GetAppearanceName(durian.appearance)} · {DurianDisplayUtil.GetBagBriefInfo(durian)}";
        }

        var legacyLabel = card.transform.Find("Label")?.GetComponent<Text>();
        if (legacyLabel != null && varietyText == null && infoText == null)
        {
            legacyLabel.text = $"{DurianDisplayUtil.GetBagCardLabel(durian)}\n{DurianDisplayUtil.GetBagBriefInfo(durian)}";
        }

        var button = card.GetComponent<Button>();
        if (button != null)
        {
            var captured = durian;
            button.onClick.AddListener(() => _uiRoot.ShowOpen(captured));
        }

        card.transform
            .DOScale(1f, 0.28f)
            .SetDelay(index * 0.05f)
            .SetEase(Ease.OutBack);
    }

    private void KillCardTweens()
    {
        if (cardRoot == null)
        {
            return;
        }

        for (var i = 0; i < cardRoot.childCount; i++)
        {
            cardRoot.GetChild(i).DOKill();
        }
    }
}
