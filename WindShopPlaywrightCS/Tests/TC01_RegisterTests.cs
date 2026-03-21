using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;
namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC01")]
public class TC01_RegisterTests : BaseTest
{
    [SetUp]
    public async Task NavigateToRegister() => await RegisterPage.GoToAsync();

    [Test]
    [Description("TC01-001 | PASS - Dang ky thanh cong voi du lieu hop le")]
    public async Task TC01_001_RegisterSuccess()
    {
        await RegisterPage.RegisterAsync(
            TestHelpers.UniqueEmail("tc01"),
            TestHelpers.UniqueUsername("tc01"),
            RegisterData.Valid.Password);

        await RegisterPage.ExpectRedirectedToLoginAsync();
    }

    [Test]
    [Description("TC01-002 | FAIL - Email sai dinh dang")]
    public async Task TC01_002_RegisterFail_InvalidEmail()
    {
        await RegisterPage.FillFormAsync(
            email:    RegisterData.BadEmail.Email,
            username: RegisterData.BadEmail.Username,
            password: RegisterData.BadEmail.Password,
            confirm:  RegisterData.BadEmail.Password);

        var isValid = await RegisterPage.EmailInput
            .EvaluateAsync<bool>("el => el.validity.valid");
        Assert.That(isValid, Is.False);
    }

    [Test]
    [Description("TC01-003 | FAIL - Username da ton tai")]
    public async Task TC01_003_RegisterFail_DuplicateUsername()
    {
        await RegisterPage.RegisterAsync(
            TestHelpers.UniqueEmail("tc01dup"),
            RegisterData.DuplicateUsername.Username,
            RegisterData.DuplicateUsername.Password);

        await RegisterPage.ExpectValidationErrorAsync();
    }

    [Test]
    [Description("TC01-004 | FAIL - Password va Confirm khong khop")]
    public async Task TC01_004_RegisterFail_PasswordMismatch()
    {
        await RegisterPage.FillFormAsync(
            email:    TestHelpers.UniqueEmail("tc01mis"),
            username: TestHelpers.UniqueUsername("tc01mis"),
            password: "Pass@123",
            confirm:  "Pass@999");
        await RegisterPage.SubmitAsync();

        await Assertions.Expect(RegisterPage.PasswordMatchError).ToBeVisibleAsync();
        await Assertions.Expect(RegisterPage.PasswordMatchError)
            .ToContainTextAsync("không trùng khớp");
    }

    [Test]
    [Description("TC01-005 | FAIL - De trong tat ca truong")]
    public async Task TC01_005_RegisterFail_AllEmpty()
    {
        await RegisterPage.SubmitAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Nếu form trống → server validate → vẫn ở trang create hoặc có lỗi
        var url = Page.Url;
        Assert.That(url, Does.Contain("create"),
            "Form trống phải bị từ chối, không redirect đi nơi khác");
    }

    [Test]
    [Description("TC01-006 | FAIL - Password qua yeu")]
    public async Task TC01_006_RegisterFail_WeakPassword()
    {
        await RegisterPage.RegisterAsync(
            TestHelpers.UniqueEmail("tc01weak"),
            TestHelpers.UniqueUsername("tc01weak"),
            RegisterData.WeakPassword.Password);

        await RegisterPage.ExpectValidationErrorAsync();
    }
}
