using Microsoft.Playwright;
using NUnit.Framework;

namespace WindShopPlaywright.Pages;

public class SearchPage(IPage page)
{
    public ILocator ProductCards => page.Locator(".product-card");

    public async Task<int> GetResultCountAsync() => await ProductCards.CountAsync();

    public async Task ExpectNoResultsAsync()
    {
        var count = await ProductCards.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    public async Task ExpectResultContainsAsync(string name)
        => await Assertions.Expect(
               ProductCards.Filter(new LocatorFilterOptions { HasText = name }))
           .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 8000 });
}
