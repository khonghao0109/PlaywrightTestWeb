using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC06")]
public class TC06_CartManagementTests : BaseTest
{
    [SetUp]
    public async Task Setup()
    {
        await LoginAsAdmin();
        await ClearCartAsync();
    }

    [Test]
    [Description("TC06-001 | Gio hang hien 2 SP va GrandTotal dung")]
    public async Task TC06_001_CartView_TwoProducts()
    {
        await AddToCartFromHomeAsync(Products.EaFC25.Name);
        await AddToCartFromHomeAsync(Products.Wukong.Name);
        await CartPage.GoToAsync();

        await CartPage.ExpectItemPresentAsync(Products.EaFC25.Name);
        await CartPage.ExpectItemPresentAsync(Products.Wukong.Name);
        await CartPage.ExpectGrandTotalAsync(600_000m);
    }

    [Test]
    [Description("TC06-002 | Nhan + tang so luong - GrandTotal tang")]
    public async Task TC06_002_IncreaseQty()
    {
        await AddToCartFromHomeAsync(Products.EaFC25.Name);
        await CartPage.GoToAsync();

        var pid    = await CartPage.GetFirstProductIdAsync();
        var before = await CartPage.GetGrandTotalAsync();

        await CartPage.ClickIncreaseAsync(pid);

        await Assertions.Expect(CartPage.QtyDisplay(pid)).ToHaveValueAsync("2");
        var after = await CartPage.GetGrandTotalAsync();
        Assert.That(after, Is.GreaterThan(before));
    }

    [Test]
    [Description("TC06-003 | Nhan - giam so luong (qty>1) - SP van con")]
    public async Task TC06_003_DecreaseQty()
    {
        await AddToCartFromHomeAsync(Products.EaFC25.Name);
        await CartPage.GoToAsync();

        var pid = await CartPage.GetFirstProductIdAsync();
        await CartPage.ClickIncreaseAsync(pid);
        await Assertions.Expect(CartPage.QtyDisplay(pid)).ToHaveValueAsync("2");

        var before = await CartPage.GetGrandTotalAsync();
        await CartPage.ClickDecreaseAsync(pid);
        await Assertions.Expect(CartPage.QtyDisplay(pid)).ToHaveValueAsync("1");

        var after = await CartPage.GetGrandTotalAsync();
        Assert.That(after, Is.LessThan(before));
        await CartPage.ExpectItemPresentAsync(Products.EaFC25.Name);
    }

    [Test]
    [Description("TC06-004 | Nhan - khi qty=1 - SP tu dong xoa")]
    public async Task TC06_004_DecreaseToZero_AutoRemove()
    {
        await AddToCartFromHomeAsync(Products.Wukong.Name);
        await CartPage.GoToAsync();

        var pid = await CartPage.GetFirstProductIdAsync();
        await CartPage.ClickDecreaseAsync(pid);

        // ✅ Chờ AJAX xử lý
        await Page.WaitForTimeoutAsync(1500);

        await CartPage.ExpectItemAbsentAsync(Products.Wukong.Name);

        // ✅ Bỏ kiểm tra GrandTotal=0 (element ẩn khi giỏ rỗng)
        // Thay bằng kiểm tra số lượng item = 0
        var itemCount = await CartPage.CartItems.CountAsync();
        Assert.That(itemCount, Is.EqualTo(0), "Gio hang phai rong sau khi xoa SP cuoi cung");
    }

    [Test]
    [Description("TC06-005 | Nhan nut Xoa - SP bien khoi gio")]
    public async Task TC06_005_RemoveButton()
    {
        await AddToCartFromHomeAsync(Products.EaFC25.Name);
        await CartPage.GoToAsync();

        var pid = await CartPage.GetFirstProductIdAsync();
        await CartPage.ClickRemoveAsync(pid);

        // ✅ Bỏ WaitForTimeoutAsync – đã có trong ClickRemoveAsync và reload trong ExpectItemAbsentAsync
        await CartPage.ExpectItemAbsentAsync(Products.EaFC25.Name);
    }
}
