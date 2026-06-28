using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 页面导航根节点，控制各页面的显示/隐藏与过渡动画。
/// </summary>
public class GameUIRoot : MonoBehaviour
{
    [SerializeField] private Image marketBgImage;
    [SerializeField] private Image openBgImage;
    [SerializeField] private MarketPage marketPage;
    [SerializeField] private OpenPage openPage;
    [SerializeField] private SellPage sellPage;
    [SerializeField] private BagPage bagPage;
    [SerializeField] private ShopPage shopPage;
    [SerializeField] private CollectionPage collectionPage;
    [SerializeField] private MainShellUI mainShell;

    public MarketPage Market => marketPage;
    public OpenPage Open => openPage;
    public SellPage Sell => sellPage;
    public BagPage Bag => bagPage;
    public ShopPage Shop => shopPage;
    public CollectionPage Collection => collectionPage;

    private GameObject _currentPage;
    private bool _firstPageShown;

    private void Start()
    {
        ShowMarket();
    }

    public void ShowMarket()
    {
        ShowPage(marketPage != null ? marketPage.gameObject : null, PageTransition.Direction.FromLeft);
        UpdateMainShell(marketPage?.gameObject, MainShellUI.HubPage.Market);
    }

    public void ShowOpen(DurianData durian)
    {
        if (openPage == null)
        {
            return;
        }

        ShowPage(openPage.gameObject, PageTransition.Direction.FromRight);
        openPage.Show(durian);
        UpdateMainShell(openPage.gameObject, MainShellUI.HubPage.Market);
    }

    public void ShowSell(DurianData durian, string rating, int overridePrice = -1)
    {
        if (sellPage == null)
        {
            return;
        }

        ShowPage(sellPage.gameObject, PageTransition.Direction.FromRight);
        sellPage.Show(durian, rating, overridePrice);
        UpdateMainShell(sellPage.gameObject, MainShellUI.HubPage.Market);
    }

    public void ShowBag()
    {
        ShowPage(bagPage != null ? bagPage.gameObject : null, PageTransition.Direction.FromRight);
        bagPage?.Refresh();
        UpdateMainShell(bagPage?.gameObject, MainShellUI.HubPage.Bag);
    }

    public void ShowShop()
    {
        ShowPage(shopPage != null ? shopPage.gameObject : null, PageTransition.Direction.FromRight);
        shopPage?.Refresh();
        UpdateMainShell(shopPage?.gameObject, MainShellUI.HubPage.Shop);
    }

    public void ShowCollection()
    {
        ShowPage(collectionPage != null ? collectionPage.gameObject : null, PageTransition.Direction.FromRight);
        collectionPage?.Refresh();
        UpdateMainShell(collectionPage?.gameObject, MainShellUI.HubPage.Collection);
    }

    private void ShowPage(GameObject targetPage, PageTransition.Direction direction)
    {
        if (targetPage == null)
        {
            return;
        }

        if (!_firstPageShown)
        {
            SetActivePageImmediate(targetPage);
            _firstPageShown = true;
            return;
        }

        if (_currentPage == targetPage)
        {
            return;
        }

        TransitionToPageAsync(targetPage, direction).Forget();
    }

    private async UniTaskVoid TransitionToPageAsync(GameObject targetPage, PageTransition.Direction direction)
    {
        var fromGroup = PageTransition.GetOrAddCanvasGroup(_currentPage);
        var toGroup = PageTransition.GetOrAddCanvasGroup(targetPage);

        await PageTransition.TransitionTo(fromGroup, toGroup, direction);

        _currentPage = targetPage;
        UpdateBackground(targetPage);
        UpdateMainShellForPage(targetPage);
    }

    private void SetActivePageImmediate(GameObject activePage)
    {
        if (marketPage != null)
        {
            var active = marketPage.gameObject == activePage;
            marketPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(marketPage.gameObject));
            }
        }

        if (openPage != null)
        {
            var active = openPage.gameObject == activePage;
            openPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(openPage.gameObject));
            }
        }

        if (sellPage != null)
        {
            var active = sellPage.gameObject == activePage;
            sellPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(sellPage.gameObject));
            }
        }

        if (bagPage != null)
        {
            var active = bagPage.gameObject == activePage;
            bagPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(bagPage.gameObject));
            }
        }

        if (shopPage != null)
        {
            var active = shopPage.gameObject == activePage;
            shopPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(shopPage.gameObject));
            }
        }

        if (collectionPage != null)
        {
            var active = collectionPage.gameObject == activePage;
            collectionPage.gameObject.SetActive(active);
            if (active)
            {
                PageTransition.ShowImmediate(PageTransition.GetOrAddCanvasGroup(collectionPage.gameObject));
            }
        }

        _currentPage = activePage;
        UpdateBackground(activePage);
        UpdateMainShellForPage(activePage);
    }

    private void UpdateMainShellForPage(GameObject activePage)
    {
        if (activePage == null)
        {
            return;
        }

        if (marketPage != null && activePage == marketPage.gameObject)
        {
            UpdateMainShell(activePage, MainShellUI.HubPage.Market);
            return;
        }

        if (bagPage != null && activePage == bagPage.gameObject)
        {
            UpdateMainShell(activePage, MainShellUI.HubPage.Bag);
            return;
        }

        if (shopPage != null && activePage == shopPage.gameObject)
        {
            UpdateMainShell(activePage, MainShellUI.HubPage.Shop);
            return;
        }

        if (collectionPage != null && activePage == collectionPage.gameObject)
        {
            UpdateMainShell(activePage, MainShellUI.HubPage.Collection);
            return;
        }

        UpdateMainShell(activePage, MainShellUI.HubPage.Market);
    }

    private void UpdateMainShell(GameObject activePage, MainShellUI.HubPage hubPage)
    {
        if (mainShell == null)
        {
            return;
        }

        var isHubPage = activePage != null &&
            (marketPage != null && activePage == marketPage.gameObject ||
             bagPage != null && activePage == bagPage.gameObject ||
             shopPage != null && activePage == shopPage.gameObject ||
             collectionPage != null && activePage == collectionPage.gameObject);

        mainShell.SetHubShellVisible(isHubPage);
        if (isHubPage)
        {
            mainShell.SetActiveHub(hubPage);
        }
    }

    private void UpdateBackground(GameObject activePage)
    {
        var showOpenBg = openPage != null && openPage.gameObject == activePage;

        if (marketBgImage != null)
        {
            marketBgImage.enabled = !showOpenBg;
        }

        if (openBgImage != null)
        {
            openBgImage.enabled = showOpenBg;
        }
    }
}
