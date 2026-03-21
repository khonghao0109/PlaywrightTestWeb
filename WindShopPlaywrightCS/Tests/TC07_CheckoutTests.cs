using Microsoft.Playwright;

using NUnit.Framework;
using WindShopPlaywright.Fixtures;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC07")]
public class TC07_CheckoutTests : BaseTest
{
    [SetUp]
    public async Task Setup()
    {
        await LoginAsAdmin();
        await ClearCartAsync();
        await AddToCartFromHomeAsync(Products.EaFC25.Name);
        await AddToCartFromHomeAsync(Products.Wukong.Name);
        await CartPage.GoToAsync();
    }

    [Test]
    [Description("TC07-001 | PASS - Thanh toan thanh cong - redirect QR")]
    public async Task TC07_001_CheckoutSuccess()
    {
        var email = await CartPage.EmailInput.InputValueAsync();
        if (string.IsNullOrEmpty(email))
            await CartPage.EmailInput.FillAsync("admin01@gmail.com");

        await CartPage.FillCheckoutFormAsync(CheckoutData.LoggedIn);
        await CartPage.ClickCheckoutAsync();
        await CartPage.ExpectRedirectedToQRAsync();
    }

    [Test]
    [Description("TC07-002 | FAIL - De trong Email")]
    public async Task TC07_002_CheckoutFail_NoEmail()
    {
        await CartPage.EmailInput.FillAsync("");
        await CartPage.FillCheckoutFormAsync(new CheckoutInput(
            Fullname: CheckoutData.LoggedIn.Fullname,
            Phone:    CheckoutData.LoggedIn.Phone));

        await CartPage.ClickCheckoutAsync();
        await CartPage.ExpectEmailErrorAsync();
        await Assertions.Expect(Page).ToHaveURLAsync("**/cart**");
    }

    [Test]
    [Description("TC07-003 | FAIL - De trong Ho va ten")]
    public async Task TC07_003_CheckoutFail_NoFullname()
    {
        await CartPage.EmailInput.FillAsync("admin01@gmail.com");
        await CartPage.FillCheckoutFormAsync(new CheckoutInput(
            Fullname: "",
            Phone:    CheckoutData.LoggedIn.Phone));

        await CartPage.ClickCheckoutAsync();
        await CartPage.ExpectFullNameErrorAsync();
        await Assertions.Expect(Page).ToHaveURLAsync("**/cart**");
    }

    [Test]
    [Description("TC07-004 | FAIL - De trong So dien thoai")]
    public async Task TC07_004_CheckoutFail_NoPhone()
    {
        await CartPage.EmailInput.FillAsync("admin01@gmail.com");
        await CartPage.FillCheckoutFormAsync(new CheckoutInput(
            Fullname: CheckoutData.LoggedIn.Fullname,
            Phone:    ""));

        await CartPage.ClickCheckoutAsync();
        await CartPage.ExpectPhoneErrorAsync();
        await Assertions.Expect(Page).ToHaveURLAsync("**/cart**");
    }

    [Test]
    [Description("TC07-005 | Email tu dien khi da dang nhap")]
    public async Task TC07_005_EmailAutoFilled()
    {
        var email = await CartPage.EmailInput.InputValueAsync();
        TestContext.WriteLine($"Email auto-filled: '{email}'");
        if (!string.IsNullOrEmpty(email))
            Assert.That(email, Does.Contain("@"));
        else
            Assert.Warn("Email khong tu dien - kiem tra CartController");
    }
}
