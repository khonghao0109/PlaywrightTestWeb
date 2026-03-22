using NUnit.Framework;
using WindShopPlaywright.Fixtures;
using Microsoft.Playwright;

namespace WindShopPlaywright.Tests;

[TestFixture]
[Category("TC04")]
public class TC04_SearchTests : BaseTest
{
    [SetUp]
    public async Task GoHome() => await HomePage.GoToAsync();

    [Test]
    [Description("TC04-001 | Tim kiem san pham ton tai")]
    public async Task TC04_001_Search_Found()
    {
        await HomePage.SearchAsync(SearchData.Existing);

        // ✅ Dùng Assert.That thay ToHaveURLAsync
        Assert.That(Page.Url, Does.Contain("Product/Search"));
        await SearchPage.ExpectResultContainsAsync("Wukong");
    }

    [Test]
    [Description("TC04-002 | Tim kiem khong co ket qua")]
    public async Task TC04_002_Search_NotFound()
    {
        await HomePage.SearchAsync(SearchData.NotFound);

        // ✅ Dùng Assert.That
        Assert.That(Page.Url, Does.Contain("Product/Search"));
        await SearchPage.ExpectNoResultsAsync();
    }

    [Test]
    [Description("TC04-003 | Tim kiem tu khoa trong - redirect ve Index")]
    public async Task TC04_003_Search_EmptyKeyword()
    {
        await HomePage.SearchInput.FillAsync("");
        await HomePage.SearchSubmitBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(Page.Url, Does.Not.Contain("Product/Search"));
    }

    [Test]
    [Description("TC04-004 | Tim kiem khong phan biet hoa thuong")]
    public async Task TC04_004_Search_CaseInsensitive()
    {
        await HomePage.SearchAsync(SearchData.CaseInsensitive);

        // ✅ Dùng Assert.That
        Assert.That(Page.Url, Does.Contain("Product/Search"));
        await SearchPage.ExpectResultContainsAsync("ARC Raiders");
    }
}