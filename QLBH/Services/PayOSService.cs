using PayOS;

public class PayOSService
{
    public PayOSClient Client { get; }

    public PayOSService(IConfiguration config)
    {
        Client = new PayOSClient(
            config["PayOS:ClientId"],
            config["PayOS:ApiKey"],
            config["PayOS:ChecksumKey"]
        );
    }
}
