using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 商店升级页（MVP 2 级）。
/// </summary>
public class ShopPage : MonoBehaviour
{
    private static readonly Color NormalTextColor = Color.white;
    private static readonly Color MutedTextColor = new(0.65f, 0.65f, 0.65f);

    [SerializeField] private Text levelText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button backButton;

    private ShopManager _shopManager;
    private GameUIRoot _uiRoot;
    private IDisposable _upgradedSub;

    [Inject]
    public void Construct(ShopManager shopManager, GameUIRoot uiRoot)
    {
        _shopManager = shopManager;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgrade);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => _uiRoot?.ShowMarket());
        }
    }

    private void OnEnable()
    {
        _upgradedSub?.Dispose();
        _upgradedSub = EventBus.Subscribe<ShopUpgradedEvent>(_ => Refresh());
        Refresh();
    }

    private void OnDisable()
    {
        _upgradedSub?.Dispose();
        _upgradedSub = null;

        if (effectText != null)
        {
            effectText.transform.DOKill();
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
                var nextBonus = _shopManager.GetNextLevelSellBonus();
                effectText.text = $"升级到 Lv.{nextLevel} 后回收价 +{nextBonus * 100f:F0}%";
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
        upgradeButton.interactable = _shopManager.CanUpgrade();
        if (label != null)
        {
            label.text = $"升级到 Lv.{next}（{cost}金币）";
        }
    }

    private void OnUpgrade()
    {
        if (_shopManager == null || !_shopManager.CanUpgrade())
        {
            return;
        }

        _shopManager.Upgrade();
        Refresh();
        PlayUpgradeFeedback();
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
}
