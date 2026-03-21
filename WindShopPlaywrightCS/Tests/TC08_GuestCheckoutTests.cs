using NUnit.Framework;
using Microsoft.Playwright;

using WindShopPlaywright.Fixtures;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC08")]
public class TC08_GuestCheckoutTests : BaseTest
{
    [SetUp]
    public async Task EnsureGuest()
    {
        await Page.GotoAsync("/account/logout",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await ClearCartAsync();
        await HomePage.GoToAsync();
    }

    [Test]
    [Description("TC08-001 | PASS - Them SP khi chua dang nhap - badge tang")]
    public async Task TC08_001_GuestAddToCart()
    {
        await Assertions.Expect(Page.Locator(".login-btn")).ToBeVisibleAsync();
        var before = await HomePage.GetCartBadgeCountAsync();

        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var after = await HomePage.GetCartBadgeCountAsync();
        Assert.That(after, Is.GreaterThan(before));
        await Assertions.Expect(Page.Locator(".login-btn")).ToBeVisibleAsync();
    }

    [Test]
    [Description("TC08-002 | PASS - Guest mo gio hang - Email KHONG tu dien")]
    public async Task TC08_002_GuestCart_EmailNotPrefilled()
    {
        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await CartPage.GoToAsync();

        await CartPage.ExpectItemPresentAsync(Products.Wukong.Name);
        await CartPage.ExpectGrandTotalAsync(100_000m);

        var email = await CartPage.EmailInput.InputValueAsync();
        Assert.That(email, Is.Empty);
    }

    [Test]
    [Description("TC08-003 | PASS - Guest dien du thong tin giao hang")]
    public async Task TC08_003_GuestFillForm()
    {
        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await CartPage.GoToAsync();

        await CartPage.FillCheckoutFormAsync(CheckoutData.Guest);

        await Assertions.Expect(CartPage.EmailInput).ToHaveValueAsync(CheckoutData.Guest.Email!);
        await Assertions.Expect(CartPage.FullNameInput).ToHaveValueAsync(CheckoutData.Guest.Fullname);
        await Assertions.Expect(CartPage.PhoneInput).ToHaveValueAsync(CheckoutData.Guest.Phone);
    }

    [Test]
    [Description("TC08-004 | PASS - Guest checkout - redirect QR")]
    public async Task TC08_004_GuestCheckout_Success()
    {
        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await CartPage.GoToAsync();

        await CartPage.FillCheckoutFormAsync(CheckoutData.Guest);
        await CartPage.ClickCheckoutAsync();
        await CartPage.ExpectRedirectedToQRAsync();
    }

    [Test]
    [Description("TC08-005 | FAIL - Guest checkout khong dien gi - hien 3 loi do")]
    public async Task TC08_005_GuestCheckout_AllEmpty()
    {
        var btn = HomePage.AddToCartBtn(Products.Wukong.Name);
        await Assertions.Expect(btn).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await CartPage.GoToAsync();

        await CartPage.ClickCheckoutAsync();

        await CartPage.ExpectEmailErrorAsync();
        await CartPage.ExpectFullNameErrorAsync();
        await CartPage.ExpectPhoneErrorAsync();
        await Assertions.Expect(Page).ToHaveURLAsync("**/cart**");
    }
}
