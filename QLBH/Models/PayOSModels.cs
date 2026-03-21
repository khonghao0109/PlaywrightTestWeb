public class PayOSCreateRequest
{
    public long orderCode { get; set; }
    public int amount { get; set; }
    public string description { get; set; }
    public string cancelUrl { get; set; }
    public string returnUrl { get; set; }
}

public class PayOSCreateResponse
{
    public string code { get; set; }
    public bool success { get; set; }
    public PayOSData data { get; set; }
}

public class PayOSData
{
    public string qrCode { get; set; }
    public string accountNumber { get; set; }
    public string accountName { get; set; }
    public string status { get; set; }
}

public class PayOSWebhookData
{
    public string accountNumber { get; set; }
    public int amount { get; set; }
    public string description { get; set; }
    public string reference { get; set; }
    public string transactionDateTime { get; set; }
    public string virtualAccountNumber { get; set; }
    public string counterAccountBankId { get; set; }
    public string counterAccountBankName { get; set; }
    public object counterAccountName { get; set; }
    public string counterAccountNumber { get; set; }
    public string virtualAccountName { get; set; }
    public string currency { get; set; }
    public long orderCode { get; set; }
    public string paymentLinkId { get; set; }
    public string code { get; set; }
    public string desc { get; set; }
}

public class PayOSWebhookResponse
{
    public string code { get; set; }
    public string desc { get; set; }
    public bool success { get; set; }
    public PayOSWebhookData data { get; set; }
    public string signature { get; set; }
}
