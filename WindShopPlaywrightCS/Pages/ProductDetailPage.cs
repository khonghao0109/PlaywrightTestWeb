using Microsoft.Playwright;

namespace WindShopPlaywright.Pages;

public class ProductDetailPage(IPage page)
{
    public ILocator QuantityInput  => page.Locator("#quantityInput");
    public ILocator QtyDecreaseBtn => page.Locator(".quantity-input-group button").First;
    public ILocator QtyIncreaseBtn => page.Locator(".quantity-input-group button").Last;
    public ILocator AddToCartBtn   => page.Locator("#add-to-cart-button");
    public ILocator BuyNowBtn      => page.Locator("#buy-now-button");

    public async Task IncreaseQtyAsync(int times = 1)
    {
        for (var i = 0; i < times; i++) await QtyIncreaseBtn.ClickAsync();
    }

    public async Task ClickAddToCartAsync()
    {
        await AddToCartBtn.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ✅ Sửa
    public async Task ClickBuyNowAsync()
    {
        await BuyNowBtn.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);  // bỏ WaitForURL
    }

    public async Task ExpectOnDetailPageAsync()
    {
        // Dùng Assert.That thay ToHaveURLAsync
        NUnit.Framework.Assert.That(page.Url,
            NUnit.Framework.Does.Contain("Detail").Or.Contains("detail"));
    }

    public async Task<int> GetQuantityValueAsync()
    {
        var val = await QuantityInput.InputValueAsync();
        return int.TryParse(val, out var n) ? n : 1;
    }


    public async Task ExpectAddToCartVisibleAsync()
        => await Assertions.Expect(AddToCartBtn).ToBeVisibleAsync();

    public async Task ExpectBuyNowVisibleAsync()
        => await Assertions.Expect(BuyNowBtn).ToBeVisibleAsync();
}
