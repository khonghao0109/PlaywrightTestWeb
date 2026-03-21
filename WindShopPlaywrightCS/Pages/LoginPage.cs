using Microsoft.Playwright;

namespace WindShopPlaywright.Pages;

public class LoginPage(IPage page)
{
    public ILocator UsernameInput     => page.Locator("input[name='Username']");
    public ILocator PasswordInput     => page.Locator("#passwordInput");
    public ILocator SubmitBtn         => page.Locator("button.btn-login");
    public ILocator ValidationSummary => page.Locator(".validation-summary");
    public ILocator AccountText       => page.Locator(".account-text");
    public ILocator LoginBtn          => page.Locator(".login-btn");

    public async Task GoToAsync()
        => await page.GotoAsync("/account/login",
               new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await SubmitBtn.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task LogoutAsync()
        => await page.GotoAsync("/account/logout",
               new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

    public async Task ExpectLoggedInAsync(string username)
        => await Assertions.Expect(AccountText)
               .ToContainTextAsync(username, new LocatorAssertionsToContainTextOptions { Timeout = 8000 });

    public async Task ExpectLoggedOutAsync()
        => await Assertions.Expect(LoginBtn).ToBeVisibleAsync();

    public async Task ExpectValidationErrorAsync()
        => await Assertions.Expect(ValidationSummary).ToBeVisibleAsync();
}
