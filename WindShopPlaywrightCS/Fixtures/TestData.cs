using Microsoft.Extensions.Configuration;

namespace WindShopPlaywright.Fixtures;

public static class TestConfig
{
    private static readonly IConfigurationRoot _cfg;
    static TestConfig()
    {
        _cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }
    public static string BaseUrl      => _cfg["TestSettings:BaseUrl"]      ?? "https://localhost:7173";
    public static string AdminUser    => _cfg["TestSettings:AdminUsername"] ?? "Admin01";
    public static string AdminPass    => _cfg["TestSettings:AdminPassword"] ?? "Admin@123456";
    public static string TestEmail    => _cfg["TestSettings:TestEmail"]     ?? "testuser_pw@gmail.com";
    public static string TestUsername => _cfg["TestSettings:TestUsername"]  ?? "testuser_pw";
    public static string TestPassword => _cfg["TestSettings:TestPassword"]  ?? "TestPass@123";
    public static string GuestEmail   => _cfg["TestSettings:GuestEmail"]    ?? "guest_pw@gmail.com";
    public static string GuestName    => _cfg["TestSettings:GuestFullname"] ?? "Nguyen Guest Playwright";
    public static string GuestPhone   => _cfg["TestSettings:GuestPhone"]    ?? "0909999888";
    public static bool   Headless     => bool.Parse(_cfg["TestSettings:Headless"] ?? "true");
}

public record RegisterInput(string Email, string Username, string Password, string? Confirm = null);

public static class RegisterData
{
    public static RegisterInput Valid             => new(TestConfig.TestEmail, TestConfig.TestUsername, TestConfig.TestPassword);
    public static RegisterInput BadEmail          => new("notanemail",         "user_bad_email",        "Pass@123");
    public static RegisterInput DuplicateUsername => new("another@gmail.com",  TestConfig.AdminUser,    "Pass@123");
    public static RegisterInput WeakPassword      => new("weak@gmail.com",     "weak_user",             "123");
}

public record LoginInput(string Username, string Password);

public static class LoginData
{
    public static LoginInput Valid       => new(TestConfig.AdminUser, TestConfig.AdminPass);
    public static LoginInput WrongPass   => new(TestConfig.AdminUser, "WrongPass!!");
    public static LoginInput NonExistent => new("ghost_user",         "AnyPass");
}

public record CheckoutInput(
    string? Email    = null,
    string  Fullname = "",
    string  Phone    = "",
    string  Country  = "",
    string  State    = "",
    string  Note     = "");

public static class CheckoutData
{
    public static CheckoutInput LoggedIn => new(
        Fullname: "Nguyen Van Playwright",
        Phone:    "0901234567",
        Country:  "Viet Nam",
        State:    "Ha Noi",
        Note:     "Playwright test order");

    public static CheckoutInput Guest => new(
        Email:    TestConfig.GuestEmail,
        Fullname: TestConfig.GuestName,
        Phone:    TestConfig.GuestPhone,
        Country:  "Viet Nam",
        State:    "TP Ho Chi Minh");
}

public record ProductInfo(string Name, decimal Price, string Category);

public static class Products
{
    // ✅ Tên và giá đúng theo ảnh thực tế
    public static ProductInfo EaFC25 => new("EA SPORTS FC 25 – Steam Offline", 500_000m, "Thể thao");
    public static ProductInfo Wukong => new("Black Myth: Wukong", 100_000m, "3D");
    public static ProductInfo Arc => new("ARC Raiders", 200_000m, "Sinh Tồn");
}
public static class SearchData
{
    public const string Existing        = "Wukong";
    public const string NotFound        = "xyzabc999notexist";
    public const string CaseInsensitive = "arc raiders";
}

public static class TestHelpers
{
    public static string UniqueEmail(string prefix = "pw")
        => $"{prefix}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@test.com";

    public static string UniqueUsername(string prefix = "user")
        => $"{prefix}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    public static decimal ParsePrice(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var digits = new string(text.Where(char.IsDigit).ToArray());
        return decimal.TryParse(digits, out var v) ? v : 0;
    }
}
