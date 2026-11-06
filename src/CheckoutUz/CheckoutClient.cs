namespace CheckoutUz;

/// <summary>
/// Top-level entry point for the CHECKOUT.UZ merchant API. Holds a single
/// shop API key and exposes both API versions:
/// <code>
/// var client = new CheckoutClient("shop_api_key");
/// await client.V1.CreatePaymentAsync(50000);
/// await client.V2.CreatePaymentAsync(50000, expireTime: 7200);
/// </code>
///
/// For the account-level (not shop-level) v2 endpoints
/// (GetProfile/GetShops/GetProfileStats), use <see cref="ProfileClient"/>
/// instead - they use a different credential.
/// </summary>
public class CheckoutClient
{
    public V1Client V1 { get; }
    public V2Client V2 { get; }

    public CheckoutClient(string apiKey)
    {
        V1 = new V1Client(apiKey);
        V2 = new V2Client(apiKey);
    }
}
