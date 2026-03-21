using Microsoft.Playwright;

namespace WindShopPlaywright.Pages;

public class HomePage(IPage page)
{
    // ✅ Dùng selector chỉ lấy desktop, loại mobile panel
    public ILocator SidebarCategories
        => page.Locator(".sidebar-card-categories .filter-list").First;

    public ILocator SidebarBrands
        => page.Locator(".sidebar-card-brands .filter-list").First;

    public ILocator PriceMaxRange => page.Locator("#price-max-range").First;
    public ILocator PriceFilterBtn => page.Locator("#price-filter-form .btn-price-apply").First;
    public ILocator ProductCards => page.Locator(".product-card");
    public ILocator CartBadge => page.Locator(".cart-badge");
    public ILocator CartLink => page.Locator(".cart-btn");
    public ILocator SearchInput => page.Locator(".search_box input");
    public ILocator SearchSubmitBtn => page.Locator(".search-icon-btn");

    // ✅ Dùng href để tránh duplicate giữa desktop và mobile
    public ILocator CategoryLink(string name)
        => page.Locator($"section a[href*='/category/']:has-text(\"{name}\")").First;

    public ILocator BrandLink(string name)
        => page.Locator($"section a[href*='/brand/']:has-text(\"{name}\")").First;

    public ILocator ProductCard(string name)
        => page.Locator(".product-card").Filter(new LocatorFilterOptions { HasText = name });

    public ILocator AddToCartBtn(string name)
        => page.Locator(".product-card")
               .Filter(new LocatorFilterOptions { HasText = name })
               .Locator(".ajax-add-to-cart");

    public async Task GoToAsync()
        => await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

    public async Task SearchAsync(string keyword)
    {
        await SearchInput.FillAsync(keyword);
        await SearchSubmitBtn.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<int> GetCartBadgeCountAsync()
    {
        try
        {
            var t = await CartBadge.TextContentAsync(new LocatorTextContentOptions { Timeout = 3000 });
            return int.TryParse(t, out var n) ? n : 0;
        }
        catch { return 0; }
    }

    public async Task ExpectProductsVisibleAsync()
        => await Assertions.Expect(ProductCards.First).ToBeVisibleAsync(
               new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

    public async Task ExpectCategoriesSidebarVisibleAsync()
        => await Assertions.Expect(SidebarCategories).ToBeVisibleAsync();

    public async Task ExpectBrandsSidebarVisibleAsync()
        => await Assertions.Expect(SidebarBrands).ToBeVisibleAsync();
}