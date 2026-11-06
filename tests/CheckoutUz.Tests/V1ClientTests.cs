using System.Text.Json.Nodes;
using CheckoutUz;
using Xunit;

namespace CheckoutUz.Tests;

public class V1ClientTests
{
    [Fact]
    public void RequiresApiKey()
    {
        Assert.Throws<ArgumentException>(() => new V1Client(""));
    }

    [Fact]
    public async Task CreatePaymentSendsCorrectRequest()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\",\"payment\":{\"_id\":1}}");
        var client = new V1Client("test_key_123", transport);

        var result = await client.CreatePaymentAsync(50000, description: "test order");

        Assert.Equal("success", result["status"]!.GetValue<string>());
        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v1/create_payment", call.Url);
        Assert.Equal("test_key_123", call.Token);
        var sentBody = JsonNode.Parse(call.JsonBody)!.AsObject();
        Assert.Equal(50000, sentBody["amount"]!.GetValue<double>());
        Assert.Equal("test order", sentBody["description"]!.GetValue<string>());
        Assert.False(sentBody.ContainsKey("webhook_url"));
    }

    [Fact]
    public async Task OptionalNullParamsAreOmitted()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\"}");
        var client = new V1Client("test_key_123", transport);

        await client.CreatePaymentAsync(1000);

        var sentBody = JsonNode.Parse(transport.Calls[0].JsonBody)!.AsObject();
        Assert.Single(sentBody);
        Assert.Equal(1000, sentBody["amount"]!.GetValue<double>());
    }

    [Fact]
    public async Task ErrorResponseThrowsCheckoutApiException()
    {
        var transport = new FakeTransport(401, "{\"error\":\"Unauthorized: Invalid or missing Bearer token\"}");
        var client = new V1Client("test_key_123", transport);

        var ex = await Assert.ThrowsAsync<CheckoutApiException>(() => client.GetBalanceAsync());
        Assert.Equal(401, ex.StatusCode);
        Assert.Contains("Unauthorized", ex.Message);
    }

    [Fact]
    public async Task NetworkErrorThrowsCheckoutApiException()
    {
        var transport = FakeTransport.Throwing(new HttpRequestException("boom"));
        var client = new V1Client("test_key_123", transport);

        await Assert.ThrowsAsync<CheckoutApiException>(() => client.GetBalanceAsync());
    }

    [Fact]
    public async Task PayViaCardSendsCorrectBody()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\"}");
        var client = new V1Client("test_key_123", transport);

        await client.PayViaCardAsync(5, "8600123412341234", "1230");

        var call = transport.Calls[0];
        Assert.Equal("https://checkout.uz/api/v1/pay_via_card", call.Url);
        var sentBody = JsonNode.Parse(call.JsonBody)!.AsObject();
        Assert.Equal(5, sentBody["order_id"]!.GetValue<int>());
        Assert.Equal("8600123412341234", sentBody["card_number"]!.GetValue<string>());
    }

    [Fact]
    public async Task StatusPaymentByUuid()
    {
        var transport = new FakeTransport(200, "{\"status\":\"success\"}");
        var client = new V1Client("test_key_123", transport);

        await client.StatusPaymentAsync("abc-123");

        var sentBody = JsonNode.Parse(transport.Calls[0].JsonBody)!.AsObject();
        Assert.Equal("abc-123", sentBody["uuid"]!.GetValue<string>());
    }
}
