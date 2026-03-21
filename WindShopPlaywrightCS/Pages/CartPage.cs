using Microsoft.Playwright;
using WindShopPlaywright.Fixtures;
using NUnit.Framework;

namespace WindShopPlaywright.Pages;

public class CartPage(IPage page)
{
    public ILocator CartItems     => page.Locator(".cart-item");
    public ILocator GrandTotal    => page.Locator("#grandTotal");
    public ILocator EmailInput    => page.Locator("input[name='Address.Email']");
    public ILocator FullNameInput => page.Locator("input[name='Address.FullName']");
    public ILocator PhoneInput    => page.Locator("input[name='Address.Phone']");
    public ILocator CountryInput  => page.Locator("input[name='Address.Country']");
    public ILocator StateInput    => page.Locator("input[name='Address.State']");
    public ILocator NoteTextarea  => page.Locator("textarea[name='Address.Note']");
    public ILocator CheckoutBtn   => page.Locator("button[type='submit']:has-text('Thanh toán')");
    public ILocator EmailError    => page.Locator("span[data-valmsg-for='Address.Email']");
    public ILocator FullNameError => page.Locator("span[data-valmsg-for='Address.FullName']");
    public ILocator PhoneError    => page.Locator("span[data-valmsg-for='Address.Phone']");

    public ILocator IncreaseBtn(string pid) => page.Locator($".increase-btn[data-id='{pid}']");
    public ILocator DecreaseBtn(string pid) => page.Locator($".decrease-btn[data-id='{pid}']");
    public ILocator RemoveBtn(string pid)   => page.Locator($".remove-btn[data-id='{pid}']");
    public ILocator QtyDisplay(string pid)  => page.Locator($"#qty-{pid}");

    public async Task GoToAsync()
        => await page.GotoAsync("/cart", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

    public async Task<string> GetFirstProductIdAsync()
    {
        var id = await CartItems.First.GetAttributeAsync("id");
        return id?.Replace("row-", "") ?? "1";
    }

    public async Task ClickIncreaseAsync(string pid)
    {
        await IncreaseBtn(pid).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickDecreaseAsync(string pid)
    {
        await DecreaseBtn(pid).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickRemoveAsync(string pid)
    {
        await RemoveBtn(pid).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<decimal> GetGrandTotalAsync()
    {
        var text = await GrandTotal.TextContentAsync();
        return TestHelpers.ParsePrice(text);
    }

    public async Task FillCheckoutFormAsync(CheckoutInput data)
    {
        if (data.Email    is not null)            await EmailInput.FillAsync(data.Email);
        if (!string.IsNullOrEmpty(data.Fullname)) await FullNameInput.FillAsync(data.Fullname);
        if (!string.IsNullOrEmpty(data.Phone))    await PhoneInput.FillAsync(data.Phone);
        if (!string.IsNullOrEmpty(data.Country))  await CountryInput.FillAsync(data.Country);
        if (!string.IsNullOrEmpty(data.State))    await StateInput.FillAsync(data.State);
        if (!string.IsNullOrEmpty(data.Note))     await NoteTextarea.FillAsync(data.Note);
    }

    public async Task ClickCheckoutAsync()
    {
        await CheckoutBtn.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ExpectItemPresentAsync(string name)
        => await Assertions.Expect(
               CartItems.Filter(new LocatorFilterOptions { HasText = name })).ToBeVisibleAsync();

    public async Task ExpectItemAbsentAsync(string name)
        => await Assertions.Expect(
               CartItems.Filter(new LocatorFilterOptions { HasText = name }))
           .Not.ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 5000 });

    public async Task ExpectGrandTotalAsync(decimal expected)
    {
        var actual = await GetGrandTotalAsync();
        Assert.That(actual, Is.EqualTo(expected));
    }

    public async Task ExpectEmailErrorAsync()    => await Assertions.Expect(EmailError).ToBeVisibleAsync();
    public async Task ExpectFullNameErrorAsync() => await Assertions.Expect(FullNameError).ToBeVisibleAsync();
    public async Task ExpectPhoneErrorAsync()    => await Assertions.Expect(PhoneError).ToBeVisibleAsync();

    public async Task ExpectRedirectedToQRAsync()
        => await Assertions.Expect(page)
               .ToHaveURLAsync("**/VietQR/GenerateQrCode**",
                   new PageAssertionsToHaveURLOptions { Timeout = 15_000 });
}
