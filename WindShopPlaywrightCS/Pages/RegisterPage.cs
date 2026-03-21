using Microsoft.Playwright;

namespace WindShopPlaywright.Pages;

public class RegisterPage(IPage page)
{
    public ILocator EmailInput           => page.Locator("input[name='Email']");
    public ILocator UsernameInput        => page.Locator("input[name='Username']");
    public ILocator PasswordInput        => page.Locator("#passwordInput");
    public ILocator ConfirmPasswordInput => page.Locator("#confirmPasswordInput");
    public ILocator SubmitBtn            => page.Locator("button.btn-signup");
    public ILocator ValidationSummary    => page.Locator(".validation-summary");
    public ILocator PasswordMatchError   => page.Locator("#passwordMatchError");

    public async Task GoToAsync()
        => await page.GotoAsync("/account/create",
               new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public async Task FillFormAsync(
        string? email = null, string? username = null,
        string? password = null, string? confirm = null)
    {
        if (email    is not null) await EmailInput.FillAsync(email);
        if (username is not null) await UsernameInput.FillAsync(username);
        if (password is not null) await PasswordInput.FillAsync(password);
        if (confirm  is not null) await ConfirmPasswordInput.FillAsync(confirm);
    }

    public async Task SubmitAsync() => await SubmitBtn.ClickAsync();

    public async Task RegisterAsync(string email, string username, string password, string? confirm = null)
    {
        await FillFormAsync(email, username, password, confirm ?? password);
        await SubmitAsync();
    }

    public async Task ExpectRedirectedToLoginAsync()
        => await Assertions.Expect(page).ToHaveURLAsync("https://localhost:7173/account/login");

    public async Task ExpectValidationErrorAsync(string? text = null)
    {
        var err = page.Locator(".text-danger:visible, .validation-summary:visible, #passwordMatchError").First;
        await Assertions.Expect(err).ToBeVisibleAsync();
        if (text is not null)
            await Assertions.Expect(err).ToContainTextAsync(text);
    }
}
