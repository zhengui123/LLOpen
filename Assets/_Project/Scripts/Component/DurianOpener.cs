using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// v1.5 逐房揭示：RC 壳盖 + RF 果肉，支持中途卖与全开评级。
/// </summary>
public class DurianOpener : MonoBehaviour
{
    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private DurianRoomConfig roomConfig;
    [SerializeField] private Image wholeDurianImage;
    [SerializeField] private Transform roomSlotsParent;
    [SerializeField] private GameObject roomSlotPrefab;

    [Header("划刀")]
    [SerializeField] private KnifeTool knifeTool;

    [Header("中途卖")]
    [SerializeField] private GameObject sellMidwayGroup;
    [SerializeField] private CanvasGroup sellMidwayCanvas;
    [SerializeField] private OpenEstimateScoreFx estimateScoreFx;
    [SerializeField] private Button sellMidwayButton;
    [SerializeField] private Button continueButton;

    [Header("评级")]
    [SerializeField] private Image ratingBadgeImage;
    [SerializeField] private Text ratingText;
    [SerializeField] private CanvasGroup ratingCanvasGroup;

    [Header("分享（占位）")]
    [SerializeField] private GameObject shareButtonGroup;
    [SerializeField] private Button shareButton;

    private StreakCounter _streakCounter;
    private ShopManager _shopManager;

    private readonly List<RoomSlot> _roomSlots = new();
    private readonly YieldGrade[] _roomGrades = new YieldGrade[5];

    private DurianData _currentDurian;
    private int _openedRoomCount;
    private int _lastEstimate;
    private bool _hasSoldMidway;
    private bool _allOpened;

    [Inject]
    public void Construct(StreakCounter streakCounter, ShopManager shopManager)
    {
        _streakCounter = streakCounter;
        _shopManager = shopManager;
    }

    private void Awake()
    {
        if (ratingCanvasGroup == null && ratingBadgeImage != null)
        {
            ratingCanvasGroup = ratingBadgeImage.GetComponent<CanvasGroup>();
        }

        if (sellMidwayCanvas == null && sellMidwayGroup != null)
        {
            sellMidwayCanvas = sellMidwayGroup.GetComponent<CanvasGroup>();
        }

        if (sellMidwayButton != null)
        {
            sellMidwayButton.onClick.RemoveAllListeners();
            sellMidwayButton.onClick.AddListener(OnSellMidwayClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => OpenRemainingRoomsAsync().Forget());
        }

        if (shareButton == null && shareButtonGroup != null)
        {
            shareButton = shareButtonGroup.transform.Find("ShareButton")?.GetComponent<Button>();
        }

        if (shareButton != null)
        {
            shareButton.onClick.RemoveAllListeners();
            shareButton.onClick.AddListener(OnShareClicked);
        }

        HideTransientUi();
    }

    private void OnShareClicked()
    {
        Debug.Log("[Share] 炫耀一下 — 微信分享占位（v1.5 MVP）");
    }

    public void ResetVisualState()
    {
        Reset();
    }

    public void ApplyUnopenedSprite(DurianData durian)
    {
        if (wholeDurianImage == null)
        {
            return;
        }

        DurianDisplayUtil.ApplyUnopenedDurianVisual(
            wholeDurianImage, spriteConfig, durian.variety, durian.appearance);
        wholeDurianImage.gameObject.SetActive(true);
    }

    public void PrepareOpen(DurianData durian)
    {
        Reset();
        EnsureReferences();
        _currentDurian = durian;
        _streakCounter?.Reset();

        ApplyUnopenedSprite(durian);

        if (wholeDurianImage != null)
        {
            wholeDurianImage.gameObject.SetActive(true);
            wholeDurianImage.raycastTarget = false;
        }

        AssignRoomGrades(durian.yieldRate);
        SpawnRoomSlots();
        PrepareKnifeForRooms();

        if (sellMidwayGroup != null)
        {
            sellMidwayGroup.SetActive(false);
        }

        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.alpha = 0f;
        }

        if (shareButtonGroup != null)
        {
            shareButtonGroup.SetActive(false);
        }

        ShowEstimatePanel();
        SetMidwayActionButtonsVisible(false);
        _lastEstimate = 0;
        estimateScoreFx?.ResetEstimate(0);
    }

    public int GetLastEstimate() => _lastEstimate;

    public async UniTask OpenRemainingRoomsAsync()
    {
        if (_hasSoldMidway || _allOpened)
        {
            return;
        }

        for (var i = _openedRoomCount; i < _roomSlots.Count; i++)
        {
            await UniTask.Delay(200);
            _roomSlots[i].TryOpen();
        }
    }

    private void OnSellMidwayClicked()
    {
        if (_hasSoldMidway || _allOpened)
        {
            return;
        }

        _hasSoldMidway = true;
        knifeTool?.DisableInteraction();
        HideSellMidwayGroup();

        for (var i = _openedRoomCount; i < _roomSlots.Count; i++)
        {
            _roomSlots[i].TryOpen();
        }

        EventBus.Publish(new DurianMidwaySoldEvent
        {
            Durian = _currentDurian,
            EstimatePrice = _lastEstimate
        });
    }

    private void OnRoomOpened(RoomSlot slot)
    {
        if (_hasSoldMidway || _allOpened)
        {
            return;
        }

        _openedRoomCount++;
        var hasFlesh = slot.RoomGrade >= YieldGrade.High;
        _streakCounter?.OnRoomRevealed(hasFlesh);
        RefreshEstimateDisplay(animate: true);
        RefreshOpenableHighlights();

        if (_openedRoomCount == 1)
        {
            SetMidwayActionButtonsVisible(true);
        }

        if (_openedRoomCount >= _roomSlots.Count)
        {
            _allOpened = true;
            HideSellMidwayGroup();
            knifeTool?.DisableInteraction();
            OnAllRoomsOpenedAsync().Forget();
        }
    }

    private async UniTaskVoid OnAllRoomsOpenedAsync()
    {
        await UniTask.Delay(300);

        var overall = EstimateOverallGrade();
        PlayerProgression.Instance.UnlockCollection(_currentDurian.variety, overall, _lastEstimate);
        PlayerProgression.Instance.TotalOpens++;
        PlayerProgression.Instance.Save();

        await ShowRatingAsync(overall);

        if (overall == YieldGrade.Perfect || (_streakCounter != null && _streakCounter.CurrentStreak >= 3))
        {
            ShowShareButton();
        }

        var ratingTextValue = GradeToRatingText(overall);
        EventBus.Publish(new DurianOpenedEvent
        {
            Durian = _currentDurian,
            Rating = ratingTextValue,
            YieldRate = _currentDurian.yieldRate
        });
    }

    private void SpawnRoomSlots()
    {
        ClearRoomSlots();

        if (roomSlotsParent == null || spriteConfig == null)
        {
            Debug.LogError("[DurianOpener] 无法生成房位：roomSlotsParent 或 spriteConfig 缺失。");
            return;
        }

        var covers = spriteConfig.GetRcJz();
        var positions = roomConfig != null && roomConfig.roomPositions != null && roomConfig.roomPositions.Length >= 5
            ? roomConfig.roomPositions
            : GetDefaultRoomPositions();

        var missingCoverCount = 0;
        for (var i = 0; i < 5; i++)
        {
            RoomSlot slot;
            if (roomSlotPrefab != null)
            {
                var go = Instantiate(roomSlotPrefab, roomSlotsParent);
                slot = go.GetComponent<RoomSlot>();
                if (slot == null)
                {
                    slot = go.AddComponent<RoomSlot>();
                }
            }
            else
            {
                slot = RoomSlotFactory.Create(roomSlotsParent);
            }

            if (slot == null)
            {
                continue;
            }

            var rect = slot.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = positions[i];
            }

            var cover = covers != null && i < covers.Length ? covers[i] : null;
            if (cover == null)
            {
                missingCoverCount++;
            }

            slot.Init(i, cover, spriteConfig.GetRf(_roomGrades[i]), _roomGrades[i]);
            slot.OnOpened += OnRoomOpened;
            _roomSlots.Add(slot);
        }

        if (_roomSlots.Count == 0)
        {
            Debug.LogError("[DurianOpener] 未生成任何 RoomSlot，请检查 Prefab 或运行 Tools/llopen/Create RoomSlot Prefab。");
        }
        else if (missingCoverCount > 0)
        {
            Debug.LogWarning($"[DurianOpener] 有 {missingCoverCount} 个壳盖贴图未挂载，请执行 Tools/llopen/绑定 v1.5 贴图到 DurianSpriteConfig。");
        }
    }

    private void EnsureReferences()
    {
        if (wholeDurianImage == null)
        {
            wholeDurianImage = transform.parent?.Find("DurianImage")?.GetComponent<Image>();
        }

        if (roomConfig == null)
        {
            roomConfig = Resources.Load<DurianRoomConfig>("JinzhengRoomConfig");
        }

        if (roomSlotsParent == null)
        {
            roomSlotsParent = EnsureRoomSlotsParent();
        }

        if (roomSlotPrefab == null)
        {
            roomSlotPrefab = Resources.Load<GameObject>("Prefabs/RoomSlot");
        }

        if (sellMidwayGroup == null)
        {
            sellMidwayGroup = transform.parent?.Find("SellMidwayGroup")?.gameObject;
            sellMidwayCanvas = sellMidwayGroup?.GetComponent<CanvasGroup>();
            estimateScoreFx = sellMidwayGroup?.GetComponentInChildren<OpenEstimateScoreFx>(true);
            sellMidwayButton = sellMidwayGroup?.transform.Find("SellMidwayButton")?.GetComponent<Button>();
            continueButton = sellMidwayGroup?.transform.Find("ContinueButton")?.GetComponent<Button>();
        }

        if (ratingBadgeImage == null)
        {
            ratingBadgeImage = transform.parent?.Find("RatingIcon")?.GetComponent<Image>();
            ratingText = transform.parent?.Find("Rating")?.GetComponent<Text>();
            ratingCanvasGroup = ratingBadgeImage?.GetComponent<CanvasGroup>();
        }

        BindMidwayButtonsIfNeeded();
        EnsureKnifeTool();
    }

    private void EnsureKnifeTool()
    {
        if (knifeTool == null)
        {
            knifeTool = transform.parent?.Find("KnifeTool")?.GetComponent<KnifeTool>();
        }
    }

    private void PrepareKnifeForRooms()
    {
        EnsureKnifeTool();
        if (knifeTool != null)
        {
            knifeTool.gameObject.SetActive(true);
            knifeTool.PrepareForRooms(_roomSlots);
        }
    }

    private void BindMidwayButtonsIfNeeded()
    {
        if (sellMidwayButton != null)
        {
            sellMidwayButton.onClick.RemoveAllListeners();
            sellMidwayButton.onClick.AddListener(OnSellMidwayClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => OpenRemainingRoomsAsync().Forget());
        }
    }

    private Transform EnsureRoomSlotsParent()
    {
        var openPage = transform.parent;
        if (openPage == null)
        {
            return null;
        }

        var existing = openPage.Find("RoomSlotsParent");
        if (existing != null)
        {
            existing.SetAsLastSibling();
            return existing;
        }

        var go = new GameObject("RoomSlotsParent", typeof(RectTransform));
        go.transform.SetParent(openPage, false);

        var rect = go.GetComponent<RectTransform>();
        if (wholeDurianImage != null)
        {
            var durianRect = wholeDurianImage.rectTransform;
            rect.anchorMin = durianRect.anchorMin;
            rect.anchorMax = durianRect.anchorMax;
            rect.offsetMin = durianRect.offsetMin;
            rect.offsetMax = durianRect.offsetMax;
            rect.pivot = durianRect.pivot;
        }
        else
        {
            rect.anchorMin = new Vector2(0.15f, 0.25f);
            rect.anchorMax = new Vector2(0.85f, 0.75f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        go.transform.SetAsLastSibling();
        return go.transform;
    }

    private void RefreshOpenableHighlights()
    {
        foreach (var slot in _roomSlots)
        {
            if (slot != null && !slot.IsOpened)
            {
                slot.SetOpenableHighlight(true);
            }
        }
    }

    private static Vector2[] GetDefaultRoomPositions()
    {
        return new[]
        {
            new Vector2(0f, -80f),
            new Vector2(-110f, -30f),
            new Vector2(110f, -30f),
            new Vector2(-90f, 80f),
            new Vector2(90f, 80f)
        };
    }

    private void AssignRoomGrades(float yieldRate)
    {
        var fullRooms = Mathf.Clamp(Mathf.RoundToInt(yieldRate / 100f * 5f), 0, 5);
        for (var i = 0; i < 5; i++)
        {
            _roomGrades[i] = i < fullRooms ? YieldGrade.Perfect : YieldGrade.Empty;
        }

        for (var i = 4; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            var temp = _roomGrades[i];
            _roomGrades[i] = _roomGrades[j];
            _roomGrades[j] = temp;
        }
    }

    private void RefreshEstimateDisplay(bool animate)
    {
        var newEstimate = CalculateEstimate();
        if (animate)
        {
            estimateScoreFx?.AnimateTo(_lastEstimate, newEstimate);
        }
        else
        {
            estimateScoreFx?.ResetEstimate(newEstimate);
        }

        _lastEstimate = newEstimate;
    }

    /// <summary>
    /// 估价 = 已开房果肉累计 + 未开房按已观测满肉比例投影（含少量波动）；未开任何房时为 0。
    /// </summary>
    private int CalculateEstimate()
    {
        if (_currentDurian.id <= 0)
        {
            return 0;
        }

        var openedGold = SumOpenedRoomGold();
        var openedCount = CountOpenedRooms();
        if (openedCount == 0)
        {
            return 0;
        }

        var shopMult = 1f + (_shopManager != null ? _shopManager.GetSellBonus() : 0f);
        var remaining = 5 - openedCount;
        var projectedGold = 0;

        if (remaining > 0)
        {
            var fullOpened = CountOpenedFullRooms();
            var fullRatio = (float)fullOpened / openedCount;
            var uncertainty = remaining / 5f * Random.Range(-0.15f, 0.15f);
            projectedGold = Mathf.RoundToInt(remaining * fullRatio * 50f * (1f + uncertainty));
        }

        return Mathf.RoundToInt((openedGold + projectedGold) * shopMult);
    }

    private int SumOpenedRoomGold()
    {
        var sum = 0;
        foreach (var slot in _roomSlots)
        {
            if (slot != null && slot.IsOpened)
            {
                sum += GetGradeGoldValue(slot.RoomGrade);
            }
        }

        return sum;
    }

    private int CountOpenedRooms()
    {
        var count = 0;
        foreach (var slot in _roomSlots)
        {
            if (slot != null && slot.IsOpened)
            {
                count++;
            }
        }

        return count;
    }

    private int CountOpenedFullRooms()
    {
        var count = 0;
        foreach (var slot in _roomSlots)
        {
            if (slot != null && slot.IsOpened && slot.RoomGrade >= YieldGrade.High)
            {
                count++;
            }
        }

        return count;
    }

    private static int GetGradeGoldValue(YieldGrade grade)
    {
        return grade switch
        {
            YieldGrade.Empty => 0,
            YieldGrade.Low => 10,
            YieldGrade.Normal => 25,
            YieldGrade.High => 40,
            YieldGrade.Perfect => 50,
            _ => 0
        };
    }

    private YieldGrade EstimateOverallGrade()
    {
        var full = 0;
        for (var i = 0; i < 5; i++)
        {
            if (_roomGrades[i] >= YieldGrade.High)
            {
                full++;
            }
        }

        return full switch
        {
            0 => YieldGrade.Empty,
            1 => YieldGrade.Low,
            2 => YieldGrade.Normal,
            3 => YieldGrade.High,
            _ => YieldGrade.Perfect
        };
    }

    private async UniTask ShowRatingAsync(YieldGrade grade)
    {
        if (ratingBadgeImage == null || spriteConfig == null)
        {
            return;
        }

        ratingBadgeImage.sprite = spriteConfig.GetRatingIcon(grade);
        ratingBadgeImage.gameObject.SetActive(true);

        if (ratingText != null)
        {
            ratingText.text = GradeToRatingText(grade);
        }

        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.DOKill();
            ratingCanvasGroup.alpha = 0f;
            await ratingCanvasGroup.DOFade(1f, 0.3f).AsyncWaitForCompletion();
        }

        ratingBadgeImage.rectTransform.DOKill();
        ratingBadgeImage.rectTransform.localScale = Vector3.zero;
        await ratingBadgeImage.rectTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
    }

    private void ShowShareButton()
    {
        if (shareButtonGroup != null)
        {
            shareButtonGroup.SetActive(true);
        }
    }

    private void ShowEstimatePanel()
    {
        if (sellMidwayGroup == null)
        {
            return;
        }

        sellMidwayGroup.SetActive(true);
        if (sellMidwayCanvas != null)
        {
            sellMidwayCanvas.DOKill();
            sellMidwayCanvas.alpha = 1f;
        }
    }

    private void SetMidwayActionButtonsVisible(bool visible)
    {
        if (sellMidwayButton != null)
        {
            sellMidwayButton.gameObject.SetActive(visible);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(visible);
        }
    }

    private void ShowSellMidwayGroup()
    {
        ShowEstimatePanel();
        SetMidwayActionButtonsVisible(true);
    }

    private void HideSellMidwayGroup()
    {
        SetMidwayActionButtonsVisible(false);
        if (sellMidwayGroup != null)
        {
            sellMidwayGroup.SetActive(false);
        }
    }

    private void HideTransientUi()
    {
        HideSellMidwayGroup();
        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.alpha = 0f;
        }

        if (shareButtonGroup != null)
        {
            shareButtonGroup.SetActive(false);
        }
    }

    private void Reset()
    {
        KillTweens();
        ClearRoomSlots();
        _openedRoomCount = 0;
        _lastEstimate = 0;
        _hasSoldMidway = false;
        _allOpened = false;
        knifeTool?.DisableInteraction();
        HideTransientUi();

        if (wholeDurianImage != null)
        {
            wholeDurianImage.gameObject.SetActive(false);
        }
    }

    private void ClearRoomSlots()
    {
        foreach (var slot in _roomSlots)
        {
            if (slot != null)
            {
                slot.OnOpened -= OnRoomOpened;
                Destroy(slot.gameObject);
            }
        }

        _roomSlots.Clear();
    }

    private void KillTweens()
    {
        estimateScoreFx?.StopMotion();

        if (sellMidwayCanvas != null)
        {
            sellMidwayCanvas.DOKill();
        }

        if (ratingCanvasGroup != null)
        {
            ratingCanvasGroup.DOKill();
        }

        if (ratingBadgeImage != null)
        {
            ratingBadgeImage.rectTransform.DOKill();
        }
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
}
