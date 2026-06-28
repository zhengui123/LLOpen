using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 图鉴页：3 品种 × 5 档位，展示解锁进度与最佳售价。
/// </summary>
public class CollectionPage : MonoBehaviour
{
    private static readonly string[] GradeNames = { "空壳", "少肉", "正常", "满肉", "爆肉" };

    [SerializeField] private DurianSpriteConfig spriteConfig;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CollectionSlot[] slots = new CollectionSlot[PlayerProgression.CollectionSlotCount];
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image[] rowCompleteStamps = new Image[3];
    [SerializeField] private Button backButton;

    private GameUIRoot _uiRoot;
    private IDisposable _newEntrySub;

    [Inject]
    public void Construct(GameUIRoot uiRoot)
    {
        _uiRoot = uiRoot;
    }

    private void Awake()
    {
        EnsureSlotReferences();
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }

        SharedUiSpriteUtil.ApplyBackIcon(backButton, spriteConfig);
        ApplyBackgroundSprite();
    }

    private void OnEnable()
    {
        _newEntrySub?.Dispose();
        _newEntrySub = EventBus.Subscribe<CollectionNewEntryEvent>(_ => Refresh());
        Refresh();
    }

    private void OnDisable()
    {
        _newEntrySub?.Dispose();
        _newEntrySub = null;
    }

    public void Refresh()
    {
        EnsureSlotReferences();
        ApplyBackgroundSprite();

        var progression = PlayerProgression.Instance;
        if (progression == null)
        {
            return;
        }

        for (var varietyIndex = 0; varietyIndex < 3; varietyIndex++)
        {
            var variety = (VarietyType)varietyIndex;
            var varietyName = DurianDisplayUtil.GetVarietyName(variety);

            for (var gradeIndex = 0; gradeIndex < 5; gradeIndex++)
            {
                var grade = (YieldGrade)gradeIndex;
                var slotIndex = PlayerProgression.GetCollectionIndex(variety, grade);
                var slot = slotIndex >= 0 && slotIndex < slots.Length ? slots[slotIndex] : null;
                if (slot == null)
                {
                    continue;
                }

                if (progression.CollectionUnlocked[slotIndex])
                {
                    slot.SetUnlocked(
                        varietyName,
                        GradeNames[gradeIndex],
                        progression.CollectionBestPrice[slotIndex]);
                }
                else
                {
                    slot.SetLocked(spriteConfig);
                }
            }

            if (rowCompleteStamps != null && varietyIndex < rowCompleteStamps.Length && rowCompleteStamps[varietyIndex] != null)
            {
                var stamp = rowCompleteStamps[varietyIndex];
                var rowComplete = progression.IsRowComplete(variety);
                stamp.gameObject.SetActive(rowComplete);
                if (rowComplete && spriteConfig != null && spriteConfig.collectionCompleteStamp != null)
                {
                    stamp.sprite = spriteConfig.collectionCompleteStamp;
                    stamp.color = Color.white;
                    stamp.preserveAspect = true;
                }
            }
        }

        var count = progression.GetCollectionCount();
        if (progressText != null)
        {
            progressText.text = $"{count}/15";
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = count / (float)PlayerProgression.CollectionSlotCount;
        }
    }

    private void EnsureSlotReferences()
    {
        if (slots == null || slots.Length != PlayerProgression.CollectionSlotCount)
        {
            slots = new CollectionSlot[PlayerProgression.CollectionSlotCount];
        }

        var grid = transform.Find("GridRoot");
        if (grid == null)
        {
            return;
        }

        for (var i = 0; i < PlayerProgression.CollectionSlotCount; i++)
        {
            if (slots[i] != null)
            {
                continue;
            }

            var child = grid.Find($"Slot_{i}");
            if (child != null)
            {
                slots[i] = child.GetComponent<CollectionSlot>();
            }
        }
    }

    private void ApplyBackgroundSprite()
    {
        if (backgroundImage == null || spriteConfig == null)
        {
            return;
        }

        if (spriteConfig.collectionBookBg != null)
        {
            backgroundImage.sprite = spriteConfig.collectionBookBg;
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = true;
        }
    }
}
