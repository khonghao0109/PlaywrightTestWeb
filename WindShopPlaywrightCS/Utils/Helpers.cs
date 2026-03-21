namespace WindShopPlaywright.Utils;

public static class PlaywrightHelpers
{
    public static decimal ParseVnd(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var digits = new string(text.Where(char.IsDigit).ToArray());
        return decimal.TryParse(digits, out var v) ? v : 0;
    }
}
