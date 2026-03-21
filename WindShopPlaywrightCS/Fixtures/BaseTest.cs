using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using WindShopPlaywright.Pages;

namespace WindShopPlaywright.Fixtures;

[TestFixture]
public abstract class BaseTest : PageTest
{
    protected RegisterPage RegisterPage { get; private set; } = null!;
    protected LoginPage LoginPage { get; private set; } = null!;
    protected HomePage HomePage { get; private set; } = null!;
    protected ProductDetailPage ProductDetailPage { get; private set; } = null!;
    protected CartPage CartPage { get; private set; } = null!;
    protected SearchPage SearchPage { get; private set; } = null!;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = TestConfig.BaseUrl,
        IgnoreHTTPSErrors = true,
        Locale = "vi-VN",
        ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
    };

    [SetUp]
    public virtual async Task SetUpPageObjects()
    {
        Page.SetDefaultTimeout(20_000);
        Page.SetDefaultNavigationTimeout(25_000);

        RegisterPage = new RegisterPage(Page);
        LoginPage = new LoginPage(Page);
        HomePage = new HomePage(Page);
        ProductDetailPage = new ProductDetailPage(Page);
        CartPage = new CartPage(Page);
        SearchPage = new SearchPage(Page);

        // Dừng trước khi bắt đầu test
        await Page.WaitForTimeoutAsync(1500);
    }

    protected async Task LoginAsAdmin()
    {
        await LoginPage.GoToAsync();
        await Page.WaitForTimeoutAsync(500);
        await LoginPage.LoginAsync(TestConfig.AdminUser, TestConfig.AdminPass);
        await Page.WaitForTimeoutAsync(1000);
        await Expect(Page.Locator(".account-text"))
            .ToContainTextAsync(TestConfig.AdminUser);
    }

    protected async Task ClearCartAsync()
    {
        await Page.GotoAsync("/Cart/Clear/0",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(500);
    }

    protected async Task AddToCartFromHomeAsync(string productName)
    {
        await HomePage.GoToAsync();
        await Page.WaitForTimeoutAsync(500);
        var btn = HomePage.AddToCartBtn(productName);
        await btn.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        await btn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(800);
    }

    [TearDown]
    public async Task TakeScreenshotOnFailure()
    {
        // Dừng sau khi test kết thúc để kịp quan sát kết quả
        await Page.WaitForTimeoutAsync(2000);

        if (TestContext.CurrentContext.Result.Outcome.Status ==
            NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var name = TestContext.CurrentContext.Test.Name
                           .Replace(" ", "_").Replace("|", "-");
            var dir = Path.Combine("TestResults", "Screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{name}.png");
            await Page.ScreenshotAsync(
                new PageScreenshotOptions { Path = path, FullPage = true });
            TestContext.AddTestAttachment(path, "Screenshot on failure");
        }
    }
}