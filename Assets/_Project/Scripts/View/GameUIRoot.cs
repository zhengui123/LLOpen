using UnityEngine;

/// <summary>
/// UI 页面导航根节点，控制各页面的显示/隐藏。
/// </summary>
public class GameUIRoot : MonoBehaviour
{
    [SerializeField] private MarketPage marketPage;
    [SerializeField] private OpenPage openPage;
    [SerializeField] private SellPage sellPage;
    [SerializeField] private BagPage bagPage;
    [SerializeField] private ShopPage shopPage;

    public MarketPage Market => marketPage;
    public OpenPage Open => openPage;
    public SellPage Sell => sellPage;
    public BagPage Bag => bagPage;
    public ShopPage Shop => shopPage;

    private void Start()
    {
        ShowMarket();
    }

    public void ShowMarket()
    {
        SetActivePage(marketPage != null ? marketPage.gameObject : null);
    }

    public void ShowOpen(DurianData durian)
    {
        if (openPage == null)
        {
            return;
        }

        SetActivePage(openPage.gameObject);
        openPage.Show(durian);
    }

    public void ShowSell(DurianData durian, string rating)
    {
        if (sellPage == null)
        {
            return;
        }

        SetActivePage(sellPage.gameObject);
        sellPage.Show(durian, rating);
    }

    public void ShowBag()
    {
        SetActivePage(bagPage != null ? bagPage.gameObject : null);
        bagPage?.Refresh();
    }

    public void ShowShop()
    {
        SetActivePage(shopPage != null ? shopPage.gameObject : null);
        shopPage?.Refresh();
    }

    private void SetActivePage(GameObject activePage)
    {
        if (marketPage != null) marketPage.gameObject.SetActive(marketPage.gameObject == activePage);
        if (openPage != null) openPage.gameObject.SetActive(openPage.gameObject == activePage);
        if (sellPage != null) sellPage.gameObject.SetActive(sellPage.gameObject == activePage);
        if (bagPage != null) bagPage.gameObject.SetActive(bagPage.gameObject == activePage);
        if (shopPage != null) shopPage.gameObject.SetActive(shopPage.gameObject == activePage);
    }
}
