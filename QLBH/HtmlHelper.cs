using MyWebApp.Models;
using System.Net;
using System.Text;

namespace MyWebApp
{
    public static class HtmlHelper
    {

        public static string GenerateHTMLNotification(List<string> product, string Name, string phone, string email)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@$"<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <title>Thông báo Đặt Hàng Mới</title>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f5f5f5;
      margin: 0;
      padding: 20px;
    }}
    .container {{
      background-color: #ffffff;
      max-width: 600px;
      margin: 0 auto;
      border-radius: 5px;
      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
      overflow: hidden;
    }}
    .header {{
      background-color: #007BFF;
      color: #ffffff;
      padding: 20px;
      text-align: center;
    }}
    .content {{
      padding: 20px;
    }}
    .info {{
      margin-bottom: 15px;
      line-height: 1.6;
    }}
    .info span {{
      font-weight: bold;
    }}
    .footer {{
      background-color: #f1f1f1;
      text-align: center;
      font-size: 12px;
      color: #777777;
      padding: 10px;
    }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <h2>Thông báo Đặt Hàng Mới</h2>
    </div>
    <div class=""content"">
      <p>Chào Bạn,</p>
      <p>Đã có một khách hàng vừa đặt mua sản phẩm. Dưới đây là thông tin chi tiết:</p>
      <div class=""info"">
        <span>Tên khách hàng:</span> {Name}
      </div>
      <div class=""info"">
        <span>Số điện thoại:</span> {phone}
      </div>
      <div class=""info"">
        <span>Email:</span> {email}
      </div>
      <div class=""info"">
        <span>Thông tin sản phẩm:</span>
<ul>");
            foreach (var item in product)
            {
                sb.Append("<li>" + item + "</li>");
            }
            sb.Append(@$"</ul>
      </div>
      <p>Vui lòng kiểm tra và xác nhận đơn hàng.</p>
    </div>
    <div class=""footer"">
      © 2026 ECOShop. All rights reserved.
    </div>
  </div>
</body>
</html>
");
            return sb.ToString();
        }


public static string GenerateHTMLContent(List<ProductInfo> products, string clientName, string phoneNumber, string email)
    {
        var downloadRows = new StringBuilder();
        var orderRows = new StringBuilder();

        decimal subtotal = 0;

        foreach (var product in products)
        {
            subtotal += product.Total;

            downloadRows.Append($@"
<tr>
<td style=""border:1px solid #ddd;"">
<a href=""{product.Url}"" style=""color:#2a7ae2;text-decoration:none;"">{product.Name}</a>
</td>
<td style=""border:1px solid #ddd;"">Không bao giờ</td>
<td style=""border:1px solid #ddd;"">
<a href=""{product.DocumentId}"" style=""color:#2a7ae2;text-decoration:none;"">Click vào đây để lấy tài khoản!</a>
</td>
</tr>");

            orderRows.Append($@"
<tr>
<td style=""border:1px solid #ddd;"">{product.Name}</td>
<td style=""border:1px solid #ddd;"">{product.Quantity}</td>
<td style=""border:1px solid #ddd;"">{product.Total:N0} ₫</td>
</tr>");
        }

        decimal discount = 0;
        decimal total = subtotal - discount;

        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
<meta charset=""UTF-8"">
<title>Đơn hàng hoàn tất</title>
</head>

<body style=""margin:0;padding:0;background:#f3f3f3;font-family:Arial, Helvetica, sans-serif;"">

<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f3f3;padding:20px 0;"">
<tr>
<td align=""center"">

<table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border:1px solid #ddd;"">

<tr>
<td style=""background:#63c05b;color:#ffffff;font-size:24px;font-weight:bold;text-align:center;padding:18px;"">
Đơn hàng đã được hoàn tất
</td>
</tr>

<tr>
<td style=""padding:25px;font-size:14px;color:#333;line-height:1.6;"">

<p>Xin chào <strong>{clientName}</strong>,</p>
<p>Chúng tôi đã xử lý xong đơn hàng của bạn.</p>

<h3 style=""color:#63c05b;margin-top:20px;"">Tệp tải xuống</h3>

<table width=""100%"" cellpadding=""8"" cellspacing=""0"" style=""border-collapse:collapse;border:1px solid #ddd;"">
<tr style=""background:#f7f7f7;font-weight:bold;"">
<td style=""border:1px solid #ddd;"">Sản phẩm</td>
<td style=""border:1px solid #ddd;"">Hết hạn</td>
<td style=""border:1px solid #ddd;"">Tải xuống</td>
</tr>

{downloadRows}

</table>

<h3 style=""color:#63c05b;margin-top:25px;"">Chi tiết đơn hàng</h3>

<table width=""100%"" cellpadding=""8"" cellspacing=""0"" style=""border-collapse:collapse;border:1px solid #ddd;"">

<tr style=""background:#f7f7f7;font-weight:bold;"">
<td style=""border:1px solid #ddd;"">Sản phẩm</td>
<td style=""border:1px solid #ddd;"">Số lượng</td>
<td style=""border:1px solid #ddd;"">Giá</td>
</tr>

{orderRows}

<tr>
<td colspan=""2"" style=""border:1px solid #ddd;"">Tổng số phụ:</td>
<td style=""border:1px solid #ddd;"">{subtotal:N0} ₫</td>
</tr>

<tr>
<td colspan=""2"" style=""border:1px solid #ddd;"">Giảm giá:</td>
<td style=""border:1px solid #ddd;color:red;"">-{discount:N0} ₫</td>
</tr>

<tr style=""font-weight:bold;"">
<td colspan=""2"" style=""border:1px solid #ddd;"">Tổng cộng:</td>
<td style=""border:1px solid #ddd;"">{total:N0} ₫</td>
</tr>

</table>

<h3 style=""color:#63c05b;margin-top:25px;"">Địa chỉ thanh toán</h3>

<table width=""100%"" cellpadding=""10"" cellspacing=""0"" style=""border:1px solid #ddd;"">
<tr>
<td>
<strong>{clientName}</strong><br>
{phoneNumber}<br>
<a href=""mailto:{email}"">{email}</a>
</td>
</tr>
</table>

</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";
    }

    public static string GenerateHTMLContent(List<string> items, bool isPaid)
        {
            var sb = new StringBuilder();

            // Phần đầu của HTML
            sb.Append(@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>" + (isPaid ? "Thông Báo Thanh Toán Thành Công" : "Xác Nhận Đơn Hàng Thành Công") + @"</title>
  <style>
    body {
      margin: 0;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background: #f4f7f9;
      color: #333;
      padding: 20px;
    }
    .email-container {
      background: #fff;
      max-width: 600px;
      margin: auto;
      padding: 30px;
      border-radius: 8px;
      box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
    }
    .header {
      text-align: center;
      margin-bottom: 30px;
    }
    .header h1 {
      color: #4CAF50;
    }
    .content p {
      font-size: 1.1em;
      line-height: 1.6;
      margin: 20px 0;
    }
    .info-box {
      border-top: 1px solid #ddd;
      padding-top: 20px;
      margin-top: 30px;
    }
    .info-box h2 {
      font-size: 1.3em;
      margin-bottom: 15px;
      color: #333;
    }
    .contact-item {
      margin-bottom: 10px;
      display: flex;
      align-items: center;
    }
    .contact-item span {
      margin-left: 10px;
      font-size: 1em;
    }
    .contact-icon {
      width: 24px;
      height: 24px;
    }
    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 12px 25px;
      background: #4CAF50;
      color: #fff;
      text-decoration: none;
      border-radius: 5px;
      transition: background 0.3s ease;
    }
    .btn:hover {
      background: #43a047;
    }
  </style>
  <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"">
</head>
<body>
  <div class=""email-container"">
    <div class=""header"">
      <h1>Shop Xin Chào Quý Khách!" + @"</h1>
    </div>
    <div class=""content"">
      <p>" + (isPaid
                ? "Chúng tôi xin thông báo rằng giao dịch thanh toán của bạn đã được xử lý thành công. Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi."
                : "Chúng tôi xin thông báo rằng giao dịch đơn hàng của bạn đã được xử lý thành công. Vui lòng gửi ảnh thông tin đã chuyển khoản thành công để nhận sản phẩm.") + @"</p>
    </div>
    
    <!-- Phần hiển thị danh sách -->
    <div class=""list-content"">
      <h2>Sản phẩm đã mua:</h2>
      <ul>");

            // Lặp qua danh sách và thêm các mục vào HTML
            foreach (var item in items)
            {
                sb.Append("<li>" + item + "</li>");
            }

            sb.Append(@"</ul>
    </div>
    
    <div class=""info-box"">
      <h2>Thông Tin Liên Hệ Người Bán</h2>
      <div class=""contact-item"">
        <i class=""fa fa-phone"" aria-hidden=""true""></i>
        <span>Số điện thoại: 0358 223 929</span>
      </div>
      <div class=""contact-item"">
        <i class=""fa fa-facebook-square"" aria-hidden=""true""></i>
        <span>Facebook: <a href=""https://www.facebook.com/truong.luong.386820"" target=""_blank"">facebook.com</a></span>
      </div>
      <div class=""contact-item"">
        <i class=""fa fa-comments"" aria-hidden=""true""></i>
        <span>Zalo: 0358 223 929</span>
      </div>
    </div>
  </div>
</body>
</html>");

            return sb.ToString();
        }

        public static string GenerateHTMLContent(List<string> products, string name, string phone, string email, string note)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"vi\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\" />");
            sb.AppendLine("  <title>Thông báo đơn hàng mới</title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body style=\"margin:0;padding:0;background-color:#f2f4f6;\">");
            sb.AppendLine("  <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");
            sb.AppendLine("    <tr><td align=\"center\">");
            sb.AppendLine("      <table width=\"600\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"margin:40px 0;background:#fff;border-radius:8px;overflow:hidden;\">");

            // Header
            sb.AppendLine("        <tr><td style=\"background:#004aac;padding:20px;text-align:center;\">");
            sb.AppendLine("          <h1 style=\"color:#fff;font-family:Arial,sans-serif;font-size:24px;margin:0;\">TruongShop</h1>");
            sb.AppendLine("        </td></tr>");

            // Title
            sb.AppendLine("        <tr><td style=\"padding:30px 40px 0;\">");
            sb.AppendLine("          <h2 style=\"font-family:Arial,sans-serif;font-size:20px;color:#333;margin:0;\">📣 Đơn hàng mới vừa được đặt</h2>");
            sb.AppendLine("        </td></tr>");

            // Body
            sb.AppendLine("        <tr><td style=\"padding:20px 40px;\">");
            sb.AppendLine("          <p style=\"font-family:Arial,sans-serif;font-size:16px;color:#555;line-height:1.5;\">Chào bạn,</p>");
            sb.AppendLine("          <p style=\"font-family:Arial,sans-serif;font-size:16px;color:#555;line-height:1.5;\">Khách hàng vừa đặt hàng với thông tin:</p>");

            // Customer info
            sb.AppendLine("          <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:collapse;margin:20px 0;\">");
            sb.AppendLine($"            <tr><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#333;width:120px;\"><strong>Họ & tên:</strong></td><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;\">{WebUtility.HtmlEncode(name)}</td></tr>");
            sb.AppendLine($"            <tr><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#333;\"><strong>Điện thoại:</strong></td><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;\">{WebUtility.HtmlEncode(phone)}</td></tr>");
            sb.AppendLine($"            <tr><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#333;\"><strong>Email:</strong></td><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;\">{WebUtility.HtmlEncode(email)}</td></tr>");
            sb.AppendLine($"            <tr><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#333;vertical-align:top;\"><strong>Ghi chú:</strong></td><td style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;\">{(string.IsNullOrWhiteSpace(note) ? "– Không có –" : WebUtility.HtmlEncode(note))}</td></tr>");
            sb.AppendLine("          </table>");

            // Order details
            sb.AppendLine("          <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:collapse;\">");
            sb.AppendLine("            <tr><td style=\"font-family:Arial,sans-serif;font-size:16px;color:#333;padding-bottom:8px;\"><strong>🛒 Chi tiết đơn hàng</strong></td></tr>");
            foreach (var prod in products)
            {
                sb.AppendLine("            <tr>");
                sb.AppendLine($"              <td style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;padding:6px 0;\">{WebUtility.HtmlEncode(prod)}</td>");
                sb.AppendLine("            </tr>");
            }
            sb.AppendLine("          </table>");

            // Footer
            sb.AppendLine("        <tr><td style=\"background:#f2f4f6;padding:20px 40px;text-align:center;\">");
            sb.AppendLine("          <p style=\"font-family:Arial,sans-serif;font-size:12px;color:#888;line-height:1.4;margin:0;\">");
            sb.AppendLine("            © 2025 TRUONGSHOP. Địa chỉ: 33 Nguyễn Thái Học, Yết Kiêu, Hà Đông.<br/>");
            sb.AppendLine("            Hotline: 0358223929 | Email: luongnhattruong2004@gmail.com");
            sb.AppendLine("          </p>");
            sb.AppendLine("        </td></tr>");

            sb.AppendLine("      </table>");
            sb.AppendLine("    </td></tr>");
            sb.AppendLine("  </table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
