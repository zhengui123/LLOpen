using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 背包页：网格展示已购榴莲。
/// </summary>
public class BagPage : MonoBehaviour
{
    [SerializeField] private Text capacityText;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject emptyHint;
    [SerializeField] private Button goMarketButton;
    [SerializeField] private Button backButton;

    private BagManager _bagManager;
    private GameUIRoot _uiRoot;

    [Inject]
    public void Construct(BagManager bagManager, GameUIRoot uiRoot)
    {
        _bagManager = bagManager;
        _uiRoot = uiRoot;
    }

    private void Start()
    {
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
    }

    public void Refresh()
    {
        if (capacityText != null)
        {
            capacityText.text = $"{_bagManager.Durians.Count}/{_bagManager.MaxCapacity}";
        }

        var isEmpty = _bagManager.Durians.Count == 0;
        if (emptyHint != null)
        {
            emptyHint.SetActive(isEmpty);
        }

        if (cardRoot == null || cardPrefab == null)
        {
            return;
        }

        for (var i = cardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardRoot.GetChild(i).gameObject);
        }

        for (var i = 0; i < _bagManager.Durians.Count; i++)
        {
            var durian = _bagManager.Durians[i];
            var card = Instantiate(cardPrefab, cardRoot);
            card.SetActive(true);

            var image = card.GetComponentInChildren<Image>();
            if (image != null)
            {
                image.color = GetAppearanceColor(durian.appearance);
            }

            var label = card.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = $"{durian.variety}\n{durian.appearance}";
            }

            var button = card.GetComponent<Button>();
            if (button != null)
            {
                var captured = durian;
                button.onClick.AddListener(() => _uiRoot.ShowOpen(captured));
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
