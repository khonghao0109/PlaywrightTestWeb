using MailSender;
using MailSender.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApp;
using MyWebApp.Models;
using MyWebApp.Repository;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[AllowAnonymous]
[Route("VietQR")]
public class VietQRController : Controller
{
    private readonly IConfiguration _config;
    private readonly DataContext _context;
    private readonly IMailSender _mailSender;

    public VietQRController(IConfiguration config, DataContext context, IMailSender mailSender)
    {
        _config = config;
        _context = context;
        _mailSender = mailSender;
    }

    [HttpGet("GenerateQrCode")]
    public async Task<IActionResult> GenerateQrCode(string orderCode)
    {
        var order = _context.Orders.FirstOrDefault(x => x.OrderCode == orderCode);
        if (order == null)
            return NotFound($"Không tồn tại đơn hàng với OrderCode = {orderCode}");

        var orderDetails = _context.OrderDetails.Where(x => x.OrderId == order.Id).ToList();
        var orderAddress = _context.OrderAddresses.FirstOrDefault(x => x.OrderId == order.Id);

        var total = orderDetails.Sum(x => x.Price * x.Quantity);

        if (total < 1000)
            return Content("Số tiền tối thiểu là 1.000đ");

        var items = orderDetails.Select(od => new
        {
            name = _context.Products.FirstOrDefault(p => p.Id == od.ProductId)?.Name ?? "Sản phẩm",
            quantity = od.Quantity,
            price = (int)od.Price
        }).ToList();

        // Sử dụng DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() làm orderCode cho PayOS
        long payosOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // WEBHOOK ĐỐI SOÁT
        order.PayOSOrderCode = payosOrderCode;
        _context.SaveChanges();

        var roundedTotal = (int)Math.Round(total);
        var cancelUrlWithCode = $"{Request.Scheme}://{Request.Host}/Cart?payment=cancel&payosOrderCode={payosOrderCode}";

        var paymentData = new
        {
            orderCode = payosOrderCode,
            amount = roundedTotal,
            description = $"Windshop{new Random().Next(100000, 999999)}",
            items = items,
            cancelUrl = cancelUrlWithCode,
            returnUrl = $"{Request.Scheme}://{Request.Host}/VietQR/Success?payment=success",
            buyerName = orderAddress?.FullName,
            buyerEmail = orderAddress?.Email
        };

        var checksumKey = _config["PayOS:ChecksumKey"];

        var signatureParams = new SortedDictionary<string, string>
        {
            { "amount", roundedTotal.ToString() },
            { "cancelUrl", paymentData.cancelUrl },
            { "description", paymentData.description },
            { "orderCode", payosOrderCode.ToString() },
            { "returnUrl", paymentData.returnUrl }
        };

        var signatureData = string.Join(
            "&",
            signatureParams.Select(kvp => $"{kvp.Key}={kvp.Value}")
        );

        // ================== DEBUG SIGNATURE ==================
        Console.WriteLine("--- PAYOS SIGNATURE DEBUG ---");
        Console.WriteLine($"[RAW DATA TO SIGN]: {signatureData}");
        Console.WriteLine($"[CHECKSUM KEY USED]: {checksumKey}");
        var generatedSignature = PayOSHelper.CreateSignature(signatureData, checksumKey);
        Console.WriteLine($"[GENERATED SIGNATURE]: {generatedSignature}");
        Console.WriteLine("=================================");
        // ===================================================

        var signature = PayOSHelper.CreateSignature(signatureData, checksumKey);

        var body = new
        {
            orderCode = payosOrderCode,
            amount = roundedTotal,
            description = paymentData.description,
            items = paymentData.items,
            cancelUrl = paymentData.cancelUrl,
            returnUrl = paymentData.returnUrl,
            buyerName = paymentData.buyerName,
            buyerEmail = paymentData.buyerEmail,
            signature = signature
        };

        var debugJson = JsonSerializer.Serialize(body);
        Console.WriteLine("===== PAYOS REQUEST BODY =====");
        Console.WriteLine(debugJson);
        Console.WriteLine("==============================");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-client-id", _config["PayOS:ClientId"]);
        client.DefaultRequestHeaders.Add("x-api-key", _config["PayOS:ApiKey"]);

        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var res = await client.PostAsync(
            "https://api-merchant.payos.vn/v2/payment-requests",
            content
        );

        var json = await res.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;

        if (root.GetProperty("code").GetString() != "00")
        {
            return Content("PayOS lỗi: " + root.GetProperty("desc").GetString());
        }

        var data = root.GetProperty("data");

        // Log the response from PayOS to see if the full description is available
        Console.WriteLine("--- PAYOS CREATE-PAYMENT-LINK RESPONSE DATA ---");
        Console.WriteLine(data.ToString());
        Console.WriteLine("==============================================");

        ViewBag.QrCode = data.GetProperty("qrCode").GetString();
        ViewBag.AccountName = data.GetProperty("accountName").GetString();
        ViewBag.AccountNumber = data.GetProperty("accountNumber").GetString();
        ViewBag.Amount = total;
        ViewBag.OrderCode = data.GetProperty("description").GetString();
        ViewBag.PayOSOrderCode = payosOrderCode; 
        ViewBag.Email = paymentData.buyerEmail; 
        ViewBag.Phone = orderAddress?.Phone; 
        ViewBag.Name = paymentData.buyerName;

        return View();
    }


    [IgnoreAntiforgeryToken]
    [HttpPost("Webhook")]
    public async Task<IActionResult> Webhook()
    {
        Console.WriteLine("--- PAYOS WEBHOOK: INITIATED ---");
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("--- PAYOS WEBHOOK: RAW BODY ---");
            Console.WriteLine(body);
            Console.WriteLine("===============================");

            if (string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("--- PAYOS WEBHOOK: EMPTY BODY, EXITING. ---");
                return Ok();
            }

            PayOSWebhookResponse webhookResponse;
            try
            {
                webhookResponse = JsonSerializer.Deserialize<PayOSWebhookResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"--- PAYOS WEBHOOK: JSON DESERIALIZATION FAILED ---");
                Console.WriteLine(jsonEx.Message);
                Console.WriteLine("==================================================");
                return Ok();
            }


            if (webhookResponse == null || webhookResponse.data == null)
            {
                Console.WriteLine("--- PAYOS WEBHOOK: INVALID JSON PAYLOAD (missing 'data' or null response). ---");
                return Ok();
            }
            Console.WriteLine("--- PAYOS WEBHOOK: JSON DESERIALIZED SUCCESSFULLY. ---");


            var checksumKey = _config["PayOS:ChecksumKey"];
            var dataElement = JsonDocument.Parse(body).RootElement.GetProperty("data");

            var signatureParams = new SortedDictionary<string, string>();
            foreach (var property in dataElement.EnumerateObject())
            {
        

                string value;
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        value = property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        value = property.Value.GetRawText();
                        break;
                    case JsonValueKind.Null:
                        value = "";
                        break;
                    default:

                        continue;
                }
                signatureParams.Add(property.Name, value);
            }

            var signatureRawData = string.Join(
                "&",
                signatureParams.Select(kvp => $"{kvp.Key}={kvp.Value}")
            );

            var expectedSignature = PayOSHelper.CreateSignature(signatureRawData, checksumKey);

            Console.WriteLine("--- PAYOS WEBHOOK: SIGNATURE VERIFICATION ---");
            Console.WriteLine($"[DATA TO SIGN]: {signatureRawData}");
            Console.WriteLine($"[CHECKSUM KEY]: {checksumKey}");
            Console.WriteLine($"[EXPECTED SIG]: {expectedSignature}");
            Console.WriteLine($"[RECEIVED SIG]: {webhookResponse.signature}");


            if (expectedSignature != webhookResponse.signature)
            {
                Console.WriteLine("--- !!! PAYOS WEBHOOK: SIGNATURE MISMATCH. EXITING. !!! ---");
                return Ok();
            }


            Console.WriteLine("--- PAYOS WEBHOOK: SIGNATURE IS VALID. PROCEEDING TO UPDATE DB. ---");

            var payosOrderCode = webhookResponse.data.orderCode;
            var order = _context.Orders.FirstOrDefault(x => x.PayOSOrderCode == payosOrderCode);

            if (order == null)
            {
                Console.WriteLine($"--- PAYOS WEBHOOK: ORDER NOT FOUND IN DB WITH PayOSOrderCode = {payosOrderCode}. EXITING. ---");
                return Ok();
            }

            Console.WriteLine($"--- PAYOS WEBHOOK: Found Order {order.OrderCode} with current status {order.Status}. ---");

            // Chỉ cập nhật khi giao dịch thành công ("00" là mã thành công)
            if (webhookResponse.success && webhookResponse.data.code == "00")
            {
                if (order.Status != 2)
                {
                    order.Status = 2;
                    _context.SaveChanges();
                    Console.WriteLine($"--- PAYOS WEBHOOK: SUCCESS. Order {order.OrderCode} (PayOS: {payosOrderCode}) status updated to 2 (PAID). ---");
                }
                else
                {
                    Console.WriteLine($"--- PAYOS WEBHOOK: INFO. Order {order.OrderCode} (PayOS: {payosOrderCode}) was already marked as PAID. ---");
                }
            }
            // Xử lý trường hợp thanh toán bị hủy hoặc thất bại từ webhook
            else if (webhookResponse.data.code != "00")
            {
                if (order.Status != 2 && order.Status != 4) // Chỉ cập nhật nếu chưa thanh toán hoặc chưa hủy
                {
                    order.Status = 4; // 4: Đã hủy
                    _context.SaveChanges();
                    Console.WriteLine($"--- PAYOS WEBHOOK: CANCELLED/FAILED. Order {order.OrderCode} (PayOS: {payosOrderCode}) status updated to 4 (CANCELLED). ---");
                }
                else
                {
                    Console.WriteLine($"--- PAYOS WEBHOOK: INFO. Order {order.OrderCode} was already PAID or CANCELLED. No status change needed. ---");
                }
            }
            else
            {
                Console.WriteLine($"--- PAYOS WEBHOOK: PAYMENT NOT SUCCESSFUL (success={webhookResponse.success}, code={webhookResponse.data.code}). NO DB UPDATE. ---");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("--- !!! PAYOS WEBHOOK: CRITICAL EXCEPTION !!! ---");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=================================================");
            return Ok(); // ⚠️ TUYỆT ĐỐI KHÔNG TRẢ 500
        }
    }

    [HttpPost("CancelOrder")]
    public async Task<IActionResult> CancelOrder(long payosOrderCode)
    {
        var order = _context.Orders.FirstOrDefault(o => o.PayOSOrderCode == payosOrderCode);

        if (order == null)
        {
            return NotFound($"Không tìm thấy đơn hàng với PayOS Order Code: {payosOrderCode}");
        }

        // Only allow cancellation if the order is not already paid or cancelled
        if (order.Status == 2 || order.Status == 4)
        {
            return RedirectToAction("Index", "Cart");
        }

        try
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);


            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"https://api-merchant.payos.vn/v2/payment-requests/{payosOrderCode}/cancel",
                content
            );

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(jsonResponse).RootElement;

            // "00" is success code for cancellation as well
            if (root.GetProperty("code").GetString() == "00")
            {
                // Update order status in the database to "Cancelled"
                order.Status = 4; // 4: Đã hủy
                await _context.SaveChangesAsync();
                Console.WriteLine($"SUCCESS (Manual Cancel): Order for PayOS code {payosOrderCode} cancelled successfully via API.");
            }
            else
            {
                var errorDesc = root.GetProperty("desc").GetString();
                Console.WriteLine($"ERROR (Manual Cancel): Failed to cancel order for PayOS code {payosOrderCode}. Reason: {errorDesc}");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION (Manual Cancel): An error occurred while cancelling order for PayOS code {payosOrderCode}. Exception: {ex.Message}");

        }

        return RedirectToAction("Index", "Cart", new { payment = "cancel" });
    }

    [HttpGet("CheckStatus")]
    public IActionResult CheckStatus(long payosOrderCode)
    {
        Console.WriteLine($"INFO (CheckStatus): Received status check request for PayOS order code: {payosOrderCode}");
        var order = _context.Orders.FirstOrDefault(o => o.PayOSOrderCode == payosOrderCode);

        if (order == null)
        {
            Console.WriteLine($"WARN (CheckStatus): PayOS order code not found: {payosOrderCode}");
            return NotFound(new { status = "notfound" });
        }

        string status;
        switch (order.Status)
        {
            case 2:
                status = "paid";
                break;
            case 4:
                status = "cancelled";
                break;
            default:
                status = "pending";
                break;
        }

        Console.WriteLine($"INFO (CheckStatus): Returning status '{status}' for PayOS order code: {payosOrderCode}");
        return Json(new { status = status });
    }



    [HttpGet("Webhook")]
    public IActionResult WebhookTest()
    {
        return Ok("Webhook OK");
    }

    [HttpPost("Success")]
    public async Task<IActionResult> Success([FromBody] PaymentInfo payment)
    {
        var productsInCart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart")?.ToList() ?? new List<CartItemModel>();
        var productsOrdered = new List<ProductInfo>();

        foreach(var product in productsInCart)
        {
            var productInfo = new ProductInfo
            {
                Name = product.ProductName,
                Quantity = product.Quantity,
                Price = product.Price,
                Url = $"{Request.Scheme}://{Request.Host}/Product/Detail/{product.ProductId}",
                DocumentId = await _context.Products.FindAsync(product.ProductId) is ProductModel p ? p.DocumentId : ""
            };
            productsOrdered.Add(productInfo);
        }

        if (payment.Status == "success")
        {
            TempData["success"] = "Thanh toán thành công";
            // Clear the cart only upon a successful payment.
            HttpContext.Session.Remove("Cart");
        }

        if (TempData["success"] != null)
        {
            ViewBag.SuccessMessage = TempData["success"];
        }


        var result = await _mailSender.SendMailAsync(new MailContent
        {
            To = payment.Email,
            Subject = "Đơn hàng của bạn đã được thanh toán thành công!",
            Body = HtmlHelper.GenerateHTMLContent(productsOrdered, payment.Name, payment.Phone, payment.Email)
        });

        if (result)
        {
            Console.WriteLine("Gửi mail thành công");
        }
        else
        {
            Console.WriteLine("Gửi mail thất bại");
        }

         return Ok(new { returnUrl = $"/VietQR/Payment-Success?email={payment.Email}" });
    }

    [HttpGet("Payment-Success")]
    public IActionResult SuccessGet([FromQuery]string email)
    {
        return View("Success", email);
    }
}
