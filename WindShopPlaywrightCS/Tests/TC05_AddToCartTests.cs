using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC05")]
public class TC05_AddToCartTests : BaseTest
{
    [SetUp]
    public async Task Setup()
    {
        await LoginAsAdmin();
        await ClearCartAsync();
        await HomePage.GoToAsync();
    }

    [Test]
    [Description("TC05-001 | Them tu card trang chu - badge tang")]
    public async Task TC05_001_AddFromCard()
    {
        var before = await HomePage.GetCartBadgeCountAsync();
        var btn = HomePage.AddToCartBtn(Products.EaFC25.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var after = await HomePage.GetCartBadgeCountAsync();
        Assert.That(after, Is.GreaterThan(before));
    }

    [Test]
    [Description("TC05-002 | Xem chi tiet - dat so luong 2 - Them vao gio")]
    public async Task TC05_002_AddFromDetail_Qty2()
    {
        var card = HomePage.ProductCard(Products.EaFC25.Name);
        await Assertions.Expect(card).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        var link = card.Locator("a.product-link, a[href*='Detail']").First;
        await link.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ProductDetailPage.ExpectOnDetailPageAsync();

        var before = await HomePage.GetCartBadgeCountAsync();
        await ProductDetailPage.IncreaseQtyAsync(1);
        var qty = await ProductDetailPage.GetQuantityValueAsync();
        Assert.That(qty, Is.EqualTo(2));

        await ProductDetailPage.ClickAddToCartAsync();
        var after = await HomePage.GetCartBadgeCountAsync();
        Assert.That(after, Is.GreaterThanOrEqualTo(before + 2));
    }

    [Test]
    [Description("TC05-003 | Nhan MUA NGAY - redirect toi /cart")]
    public async Task TC05_003_BuyNow()
    {
        var card = HomePage.ProductCard(Products.Wukong.Name);
        await Assertions.Expect(card).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        await card.Locator("a.product-link, a[href*='Detail']").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ProductDetailPage.ExpectBuyNowVisibleAsync();
        await ProductDetailPage.ClickBuyNowAsync();
        Assert.That(Page.Url, Does.Contain("Cart").Or.Contain("cart"));
        await CartPage.ExpectItemPresentAsync(Products.Wukong.Name);
    }

    [Test]
    [Description("TC05-004 | Them cung san pham lan 2 - chi 1 dong qty tang")]
    public async Task TC05_004_AddSameProduct_Twice()
    {
        var btn = HomePage.AddToCartBtn(Products.EaFC25.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await CartPage.GoToAsync();
        var rows = await Page.Locator(".cart-item")
            .Filter(new LocatorFilterOptions { HasText = Products.EaFC25.Name })
            .CountAsync();
        Assert.That(rows, Is.EqualTo(1));
    }
}
