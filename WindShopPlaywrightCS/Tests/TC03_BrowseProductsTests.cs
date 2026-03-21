using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC03")]
public class TC03_BrowseProductsTests : BaseTest
{
    [SetUp]
    public async Task Setup()
    {
        await LoginAsAdmin();
        await HomePage.GoToAsync();
    }

    [Test]
    [Description("TC03-001 | Sidebar hien thi dung Danh muc & Thuong hieu")]
    public async Task TC03_001_Sidebar_ShowsCategoriesAndBrands()
    {
        await HomePage.ExpectCategoriesSidebarVisibleAsync();
        await Assertions.Expect(HomePage.CategoryLink("3D")).ToBeVisibleAsync();
        await Assertions.Expect(HomePage.CategoryLink("Sinh Tồn")).ToBeVisibleAsync();
        await Assertions.Expect(HomePage.CategoryLink("Thể thao")).ToBeVisibleAsync();

        await HomePage.ExpectBrandsSidebarVisibleAsync();
        await Assertions.Expect(HomePage.BrandLink("steam")).ToBeVisibleAsync();
        await Assertions.Expect(HomePage.BrandLink("Xbox")).ToBeVisibleAsync();
        await Assertions.Expect(HomePage.BrandLink("Netflix")).ToBeVisibleAsync();
        await Assertions.Expect(HomePage.BrandLink("EA")).ToBeVisibleAsync();

        // ✅ Dùng href thay vì text – tránh lỗi encoding tiếng Việt
        var phanMemLink = Page.Locator("a[href*='Ph%E1%BA%A7n-m%E1%BB%81m']").First;
        await Assertions.Expect(phanMemLink).ToBeVisibleAsync();
    }

    [Test]
    [Description("TC03-002 | Click danh muc The thao - loc san pham dung")]
    public async Task TC03_002_CategoryFilter_TheThao()
    {
        await Assertions.Expect(HomePage.CategoryLink("Thể thao")).ToBeVisibleAsync();
        await HomePage.CategoryLink("Thể thao").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(Page.Url, Does.Contain("/category/"));
        await Assertions.Expect(HomePage.ProductCard("EA SPORTS FC 25"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 8000 });
    }

    [Test]
    [Description("TC03-003 | Click thuong hieu EA - co san pham hien thi")]
    public async Task TC03_003_BrandFilter_EA()
    {
        await Assertions.Expect(HomePage.BrandLink("EA")).ToBeVisibleAsync();
        await HomePage.BrandLink("EA").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(Page.Url, Does.Contain("/brand/"));

        // ✅ Kiểm tra có ít nhất 1 sản phẩm trên trang brand EA
        var productCount = await Page.Locator(".product-card").CountAsync();
        Assert.That(productCount, Is.GreaterThan(0), "Trang brand EA phai co san pham");
    }

    [Test]
    [Description("TC03-004 | Them san pham vao gio tu card trang chu")]
    public async Task TC03_004_AddToCart_FromHomeCard()
    {
        var before = await HomePage.GetCartBadgeCountAsync();

        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var after = await HomePage.GetCartBadgeCountAsync();
        Assert.That(after, Is.GreaterThan(before));
    }

    [Test]
    [Description("TC03-005 | Loc theo gia - slider hoat dong")]
    public async Task TC03_005_PriceFilter()
    {
        await HomePage.PriceMaxRange.FillAsync("300000");
        await HomePage.PriceMaxRange.DispatchEventAsync("input");
        await HomePage.PriceMaxRange.DispatchEventAsync("change");

        await Assertions.Expect(HomePage.PriceFilterBtn).ToBeVisibleAsync();
        await HomePage.PriceFilterBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await HomePage.ExpectProductsVisibleAsync();
    }
}