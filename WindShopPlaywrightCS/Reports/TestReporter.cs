// Reports/TestReporter.cs
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Text;
using System.Text.Json;

namespace WindShopPlaywright.Reports;

[AttributeUsage(AttributeTargets.Assembly)]
public class TestReporterAttribute : Attribute, ITestAction
{
    private static readonly List<TcResult> _results = new();
    private static readonly object _lock = new();
    private static DateTime _start;
    private static DateTime _testStart;   // ← THÊM DÒNG NÀY


    public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

    // ✅ Sửa thành
    public void BeforeTest(ITest test)
    {
        if (test.IsSuite && test.Parent == null)
            _start = DateTime.Now;
        if (!test.IsSuite)
            _testStart = DateTime.Now;   // ← ghi thời điểm bắt đầu test
    }

    public void AfterTest(ITest test)
    {
        if (test.IsSuite) { if (test.Parent == null) GenerateReports(); return; }
        var ctx = TestContext.CurrentContext;
        lock (_lock)
        {
            _results.Add(new TcResult
            {
                SuiteId = ExtractSuite(test.FullName),
                TestId = test.Name,
                Description = test.Properties.Get("Description")?.ToString() ?? test.Name,
                Status = ctx.Result.Outcome.Status.ToString(),
                Duration = (DateTime.Now - _testStart).TotalSeconds,
                ErrorMsg = ctx.Result.Message ?? ""
            });
        }
    }

    private static string ExtractSuite(string full)
    {
        var m = System.Text.RegularExpressions.Regex.Match(full, @"TC\d+");
        return m.Success ? m.Value : "Other";
    }

    private static string Dir()
    {
        var d = Path.Combine(AppContext.BaseDirectory, "TestReports");
        Directory.CreateDirectory(d);
        return d;
    }

    private static void GenerateReports()
    {
        if (_results.Count == 0) return;

        var ts = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var dir = Dir();
        var json = Path.Combine(dir, $"report_{ts}.json");
        var html = Path.Combine(dir, $"report_{ts}.html");
        var latestHtml = Path.Combine(dir, "report_latest.html");
        var latestJson = Path.Combine(dir, "report_latest.json");

        lock (_lock)
        {
            WriteJson(json, ts);
            WriteHtml(html, ts);
            File.Copy(json, latestJson, true);
            File.Copy(html, latestHtml, true);
        }

        Console.WriteLine($"\n📊 HTML : {latestHtml}");
        Console.WriteLine($"📋 JSON : {latestJson}\n");

        // Mở HTML trong browser
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = latestHtml,
                UseShellExecute = true
            });
        }
        catch { }

        // Mở JSON trong VS Code
        try
        {
            // Đường dẫn mặc định của VS Code trên Windows
            var vscodePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Microsoft VS Code", "Code.exe");

            if (File.Exists(vscodePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = vscodePath,
                    Arguments = $"\"{latestJson}\"",
                    UseShellExecute = false
                });
            }
            else
            {
                // Fallback: mở bằng Notepad nếu không có VS Code
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{latestJson}\"",
                    UseShellExecute = false
                });
            }
        }
        catch { }
    }
    private static void WriteJson(string path, string ts)
    {
        var total = _results.Count;
        var passed = _results.Count(r => r.Status == "Passed");
        var failed = _results.Count(r => r.Status == "Failed");
        var obj = new
        {
            generatedAt = ts,
            totalSeconds = Math.Round((DateTime.Now - _start).TotalSeconds, 1),
            summary = new
            {
                total,
                passed,
                failed,
                skipped = total - passed - failed,
                passRate = total > 0 ? $"{(double)passed / total:P0}" : "N/A"
            },
            suites = _results.GroupBy(r => r.SuiteId).OrderBy(g => g.Key).Select(g => new {
                suiteId = g.Key,
                total = g.Count(),
                passed = g.Count(x => x.Status == "Passed"),
                failed = g.Count(x => x.Status == "Failed"),
                tests = g.OrderBy(x => x.TestId).Select(x => new {
                    x.TestId,
                    x.Description,
                    x.Status,
                    duration = $"{x.Duration:F2}s",
                    error = x.ErrorMsg.Length > 300 ? x.ErrorMsg[..300] + "..." : x.ErrorMsg
                })
            })
        };
        File.WriteAllText(path,
            JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
    }

    private static string E(string? s) =>
        (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static void WriteHtml(string path, string ts)
    {
        var total = _results.Count;
        var passed = _results.Count(r => r.Status == "Passed");
        var failed = _results.Count(r => r.Status == "Failed");
        var skipped = total - passed - failed;
        var pct = total > 0 ? (double)passed / total * 100 : 0;
        var dur = (DateTime.Now - _start).TotalSeconds;
        var sb = new StringBuilder();

        sb.Append($@"<!DOCTYPE html><html lang=""vi"">
<head><meta charset=""UTF-8""><title>WindShop Test Report – {ts}</title>
<style>
*{{box-sizing:border-box;margin:0;padding:0}}
body{{font-family:'Segoe UI',Arial,sans-serif;background:#f4f6f9;color:#333;font-size:14px}}
header{{background:#1a2535;color:#fff;padding:22px 32px}}
header h1{{font-size:20px;font-weight:700}}
.sub{{font-size:12px;opacity:.65;margin-top:4px}}
.wrap{{max-width:1200px;margin:24px auto;padding:0 24px}}
.grid{{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:14px;margin-bottom:24px}}
.card{{background:#fff;border-radius:10px;padding:18px;text-align:center;box-shadow:0 1px 4px rgba(0,0,0,.07);border-top:4px solid #dee2e6}}
.card.p{{border-color:#28a745}}.card.f{{border-color:#dc3545}}.card.s{{border-color:#ffc107}}.card.t{{border-color:#1a2535}}
.card .n{{font-size:34px;font-weight:700}}.card .l{{font-size:11px;color:#777;margin-top:5px;text-transform:uppercase}}
.bar{{background:#e9ecef;border-radius:99px;height:10px;overflow:hidden;margin-bottom:24px}}
.fill{{height:100%;border-radius:99px;background:linear-gradient(90deg,#28a745,#20c997)}}
.suite{{background:#fff;border-radius:10px;margin-bottom:18px;box-shadow:0 1px 4px rgba(0,0,0,.07);overflow:hidden}}
.sh{{background:#1a2535;color:#fff;padding:13px 18px;display:flex;justify-content:space-between;align-items:center;cursor:pointer}}
.sh:hover{{background:#243650}}.sh h3{{font-size:13px;font-weight:600}}
.sb{{font-size:11px;background:rgba(255,255,255,.15);padding:2px 10px;border-radius:99px}}
table{{width:100%;border-collapse:collapse}}
th{{background:#f1f3f5;padding:9px 14px;text-align:left;font-size:11px;color:#555;font-weight:600;text-transform:uppercase;border-bottom:2px solid #dee2e6}}
td{{padding:9px 14px;border-bottom:1px solid #f0f0f0;vertical-align:top;font-size:13px}}
tr:last-child td{{border-bottom:none}}tr:hover td{{background:#fafbfc}}
.badge{{display:inline-block;padding:2px 9px;border-radius:99px;font-size:11px;font-weight:600}}
.Passed{{background:#d4edda;color:#155724}}.Failed{{background:#f8d7da;color:#721c24}}.Skipped{{background:#fff3cd;color:#856404}}
.err{{background:#fff5f5;border-left:3px solid #dc3545;padding:5px 8px;margin-top:5px;font-size:11px;color:#b91c1c;white-space:pre-wrap;word-break:break-all;max-height:80px;overflow-y:auto}}
footer{{text-align:center;padding:20px;color:#aaa;font-size:12px}}
</style></head><body>
<header><h1>🧪 WindShop – Playwright Test Report</h1>
<div class=""sub"">Generated: {ts} | Duration: {dur:F1}s</div></header>
<div class=""wrap"">
<div class=""grid"">
<div class=""card t""><div class=""n"">{total}</div><div class=""l"">Total</div></div>
<div class=""card p""><div class=""n"">{passed}</div><div class=""l"">✅ Passed</div></div>
<div class=""card f""><div class=""n"">{failed}</div><div class=""l"">❌ Failed</div></div>
<div class=""card s""><div class=""n"">{skipped}</div><div class=""l"">⏭ Skipped</div></div>
<div class=""card t""><div class=""n"">{pct:F0}%</div><div class=""l"">Pass Rate</div></div>
</div>
<div class=""bar""><div class=""fill"" style=""width:{pct:F1}%""></div></div>
");
        foreach (var suite in _results.GroupBy(r => r.SuiteId).OrderBy(g => g.Key))
        {
            var sp = suite.Count(r => r.Status == "Passed");
            var sf = suite.Count(r => r.Status == "Failed");
            var st = suite.Count();
            var fb = sf > 0 ? $" | ❌ {sf} failed" : "";
            sb.Append($@"<div class=""suite"">
<div class=""sh"" onclick=""toggle('{suite.Key}')""><h3>{suite.Key}</h3><span class=""sb"">✅ {sp}/{st}{fb}</span></div>
<div id=""{suite.Key}""><table>
<thead><tr>
<th style=""width:160px"">Test ID</th><th>Mô tả</th>
<th style=""width:90px"">Kết quả</th><th style=""width:80px"">Thời gian</th>
</tr></thead><tbody>
");
            foreach (var tc in suite.OrderBy(r => r.TestId))
            {
                var err = string.IsNullOrEmpty(tc.ErrorMsg) ? "" :
                          $"<div class='err'>{E(tc.ErrorMsg.Trim())}</div>";
                sb.Append($"<tr><td><code>{E(tc.TestId)}</code></td><td>{E(tc.Description)}{err}</td>" +
                           $"<td><span class=\"badge {tc.Status}\">{tc.Status}</span></td>" +
                           $"<td>{tc.Duration:F2}s</td></tr>\n");
            }
            sb.Append("</tbody></table></div></div>\n");
        }
        sb.Append($@"</div>
<footer>WindShop Playwright | {ts}</footer>
<script>function toggle(id){{var e=document.getElementById(id);e.style.display=e.style.display===''?'none':'';}}
</script></body></html>");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}

internal class TcResult
{
    public string SuiteId { get; set; } = "";
    public string TestId { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public double Duration { get; set; }
    public string ErrorMsg { get; set; } = "";
}