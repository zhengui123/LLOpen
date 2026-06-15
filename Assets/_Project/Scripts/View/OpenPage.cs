using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 开榴莲页：滑动划刀、揭示、复活广告。
/// </summary>
public class OpenPage : MonoBehaviour
{
    [SerializeField] private KnifeTool knifeTool;
    [SerializeField] private DurianOpener durianOpener;
    [SerializeField] private Image durianImage;
    [SerializeField] private Text guideText;
    [SerializeField] private Text estimateText;
    [SerializeField] private Button reviveButton;
    [SerializeField] private Button backButton;

    private SellManager _sellManager;
    private AdManager _adManager;
    private GameUIRoot _uiRoot;
    private DurianData _currentDurian;
    private IDisposable _openedSub;

    [Inject]
    public void Construct(SellManager sellManager, AdManager adManager, GameUIRoot uiRoot)
    {
        _sellManager = sellManager;
        _adManager = adManager;
        _uiRoot = uiRoot;
    }

    private void Awake()
    {
        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(false);
            reviveButton.onClick.AddListener(OnReviveClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(() => _uiRoot.ShowMarket());
        }

        if (durianOpener != null)
        {
            durianOpener.SetNavigationTarget(_uiRoot);
        }
    }

    public void Show(DurianData durian)
    {
        _currentDurian = durian;
        _openedSub?.Dispose();
        _openedSub = EventBus.Subscribe<DurianOpenedEvent>(OnDurianOpened);

        if (durianImage != null)
        {
            durianImage.color = GetAppearanceColor(durian.appearance);
        }

        if (guideText != null)
        {
            guideText.text = "在榴莲顶部滑动开果";
        }

        if (estimateText != null)
        {
            var price = _sellManager.CalculateSellPrice(durian);
            estimateText.text = $"估价约 {price} 金币";
        }

        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(false);
        }

        if (knifeTool != null)
        {
            knifeTool.Setup(durian);
        }
    }

    private void OnDurianOpened(DurianOpenedEvent e)
    {
        if (e.Rating == "空壳" && reviveButton != null)
        {
            reviveButton.gameObject.SetActive(true);
        }
    }

    private async void OnReviveClicked()
    {
        await _adManager.ShowRewardedAd("revive");
        if (knifeTool != null)
        {
            knifeTool.Setup(_currentDurian);
        }

        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        _openedSub?.Dispose();
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
