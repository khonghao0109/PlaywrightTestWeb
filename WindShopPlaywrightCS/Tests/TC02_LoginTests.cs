using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC02")]
public class TC02_LoginTests : BaseTest
{
    // ✅ Bỏ [SetUp] NavigateToLogin – mỗi test tự gọi GoToAsync()

    [Test]
    [Description("TC02-001 | PASS - Dang nhap thanh cong")]
    public async Task TC02_001_LoginSuccess()
    {
        await LoginPage.GoToAsync();
        await LoginPage.LoginAsync(LoginData.Valid.Username, LoginData.Valid.Password);
        await LoginPage.ExpectLoggedInAsync(TestConfig.AdminUser);
    }

    [Test]
    [Description("TC02-002 | FAIL - Sai mat khau")]
    public async Task TC02_002_LoginFail_WrongPassword()
    {
        await LoginPage.GoToAsync();
        await LoginPage.LoginAsync(LoginData.WrongPass.Username, LoginData.WrongPass.Password);
        await LoginPage.ExpectValidationErrorAsync();
        await Assertions.Expect(LoginPage.ValidationSummary)
            .ToContainTextAsync("không đúng");
        Assert.That(Page.Url, Does.Contain("Account/Login"));
    }

    [Test]
    [Description("TC02-003 | FAIL - Tai khoan khong ton tai")]
    public async Task TC02_003_LoginFail_NonExistent()
    {
        await LoginPage.GoToAsync();
        await LoginPage.LoginAsync(LoginData.NonExistent.Username, LoginData.NonExistent.Password);
        await LoginPage.ExpectValidationErrorAsync();
        Assert.That(Page.Url, Does.Contain("Account/Login"));
    }

    [Test]
    [Description("TC02-004 | FAIL - De trong ca hai truong")]
    public async Task TC02_004_LoginFail_EmptyFields()
    {
        await LoginPage.GoToAsync();
        await LoginPage.SubmitBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var url = Page.Url;
        Assert.That(url, Does.Contain("Login").Or.Contain("login"),
            "Submit trong phai o lai trang login");
    }

    [Test]
    [Description("TC02-005 | PASS - Dang nhap thanh cong")]
    public async Task TC02_005_LogoutSuccess()
    {
        await LoginPage.GoToAsync();
        await LoginPage.LoginAsync(LoginData.Valid.Username, LoginData.Valid.Password);
        await LoginPage.ExpectLoggedInAsync(TestConfig.AdminUser);
        await LoginPage.LogoutAsync();
        await LoginPage.ExpectLoggedOutAsync();
    }
}