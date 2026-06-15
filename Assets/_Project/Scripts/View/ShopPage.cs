using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 商店升级页（MVP 2 级）。
/// </summary>
public class ShopPage : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button backButton;

    private ShopManager _shopManager;
    private GameUIRoot _uiRoot;

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

    public void Refresh()
    {
        if (levelText != null)
        {
            levelText.text = $"商店 Lv.{_shopManager.CurrentLevel}";
        }

        if (effectText != null)
        {
            var bonus = _shopManager.GetSellBonus();
            effectText.text = bonus > 0f
                ? $"回收价 +{bonus * 100f:F0}%"
                : "升级到 Lv.2 后回收价 +20%";
        }

        if (upgradeButton == null)
        {
            return;
        }

        if (_shopManager.CurrentLevel >= _shopManager.MaxLevel)
        {
            upgradeButton.interactable = false;
            var label = upgradeButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "已满级";
            }
            return;
        }

        upgradeButton.interactable = _shopManager.CanUpgrade();
        var text = upgradeButton.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = "升级到 Lv.2（500金币）";
        }
    }

    private void OnUpgrade()
    {
        _shopManager.Upgrade();
        Refresh();
    }
}
