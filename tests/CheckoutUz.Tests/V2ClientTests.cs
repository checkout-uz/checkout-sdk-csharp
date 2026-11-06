using System.Text.Json.Nodes;
using CheckoutUz;
using Xunit;

namespace CheckoutUz.Tests;

public class V2ClientTests
{
    [Fact]
    public async Task CreatePaymentWithIdempotencyKeySetsHeader()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\"}");
        var client = new V2Client("test_key_123", transport);

        await client.CreatePaymentAsync(50000, expireTime: 7200, idempotencyKey: "order-42");

        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v2/create_payment", call.Url);
        var sentBody = JsonNode.Parse(call.JsonBody)!.AsObject();
        Assert.Equal(50000, sentBody["amount"]!.GetValue<double>());
        Assert.Equal(7200, sentBody["expire_time"]!.GetValue<int>());
        Assert.Equal("order-42", call.ExtraHeaders["Idempotency-Key"]);
    }

    [Fact]
    public async Task CreatePaymentWithoutIdempotencyKeyOmitsHeader()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\"}");
        var client = new V2Client("test_key_123", transport);

        await client.CreatePaymentAsync(1000);

        Assert.Empty(transport.Calls[0].ExtraHeaders);
    }

    [Fact]
    public async Task GetHistorySendsFilters()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\",\"data\":[]}");
        var client = new V2Client("test_key_123", transport);

        await client.GetHistoryAsync(limit: 20, offset: 10, status: "paid", dateFrom: "2026-01-01");

        var sentBody = JsonNode.Parse(transport.Calls[0].JsonBody)!.AsObject();
        Assert.Equal(20, sentBody["limit"]!.GetValue<int>());
        Assert.Equal(10, sentBody["offset"]!.GetValue<int>());
        Assert.Equal("paid", sentBody["status"]!.GetValue<string>());
        Assert.Equal("2026-01-01", sentBody["date_from"]!.GetValue<string>());
    }

    [Fact]
    public async Task CancelPayment()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\",\"order_status\":\"canceled\"}");
        var client = new V2Client("test_key_123", transport);

        var result = await client.CancelPaymentAsync(123);

        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v2/cancel_payment", call.Url);
        Assert.Equal(123, JsonNode.Parse(call.JsonBody)!["order_id"]!.GetValue<int>());
        Assert.Equal("canceled", result["order_status"]!.GetValue<string>());
    }

    [Fact]
    public async Task RefundPaymentErrorThrows()
    {
        var transport = new FakeTransport(409, "{\"error\":\"More than 22 hours have passed\"}");
        var client = new V2Client("test_key_123", transport);

        var ex = await Assert.ThrowsAsync<CheckoutApiException>(() => client.RefundPaymentAsync(123));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task GetFiscal()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\",\"fiscal\":{}}");
        var client = new V2Client("test_key_123", transport);

        await client.GetFiscalAsync(123);

        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v2/get_fiscal", call.Url);
        Assert.Equal(123, JsonNode.Parse(call.JsonBody)!["order_id"]!.GetValue<int>());
    }

    [Fact]
    public async Task ProfileClientUsesDifferentCredentialAgainstSameBaseUrl()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\",\"shops\":[]}");
        var profile = new ProfileClient("profile_key_xyz", transport);

        await profile.GetShopsAsync();

        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v2/get_shops", call.Url);
        Assert.Equal("profile_key_xyz", call.Token);
    }

    [Fact]
    public void ProfileClientRequiresKey()
    {
        Assert.Throws<ArgumentException>(() => new ProfileClient(""));
    }
}
