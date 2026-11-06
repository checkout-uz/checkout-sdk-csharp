using System.Text.Json.Nodes;

namespace CheckoutUz;

/// <summary>
/// Client for the original CHECKOUT.UZ merchant API (<c>/api/v1</c>).
/// Authenticated with a shop's API key (found in the shop's settings on
/// checkout.uz), sent as <c>Authorization: Bearer &lt;apiKey&gt;</c>.
/// </summary>
public class V1Client : BaseClient
{
    public const string BaseUrlConstant = "https://checkout.uz/api/v1";

    public V1Client(string apiKey) : base(BaseUrlConstant, apiKey)
    {
    }

    internal V1Client(string apiKey, IHttpTransport transport) : base(BaseUrlConstant, apiKey, transport)
    {
    }

    /// <summary>Create a new payment invoice. <paramref name="amount"/> is in UZS (1000-10,000,000).</summary>
    public Task<JsonObject> CreatePaymentAsync(
        double amount,
        string? description = null,
        string? webhookUrl = null,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["amount"] = amount,
            ["description"] = description,
            ["webhook_url"] = webhookUrl,
            ["return_url"] = returnUrl,
        };
        return PostAsync("create_payment", parameters, new Dictionary<string, string>(), cancellationToken);
    }

    /// <summary>Look up an invoice by its numeric id. Use <see cref="StatusPaymentAsync(string, CancellationToken)"/> for the uuid overload.</summary>
    public Task<JsonObject> StatusPaymentAsync(long id, CancellationToken cancellationToken = default)
        => PostAsync("status_payment", new Dictionary<string, object?> { ["id"] = id },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Look up an invoice by its uuid (pay_token).</summary>
    public Task<JsonObject> StatusPaymentAsync(string uuid, CancellationToken cancellationToken = default)
        => PostAsync("status_payment", new Dictionary<string, object?> { ["uuid"] = uuid },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Return the shop's current balance per currency.</summary>
    public Task<JsonObject> GetBalanceAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_balance", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>Return the shop's most recent invoices (newest first).</summary>
    public Task<JsonObject> GetHistoryAsync(int limit = 10, CancellationToken cancellationToken = default)
        => PostAsync("get_history", new Dictionary<string, object?> { ["limit"] = limit },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Return aggregate order count / total income for the shop.</summary>
    public Task<JsonObject> GetStatsAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_stats", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>List the payment methods enabled for this shop.</summary>
    public Task<JsonObject> GetPaymentMethodsAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_payment_methods", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>Start a direct card payment (Uzcard/Humo) for a pending invoice. Sends an SMS code to the cardholder.</summary>
    public Task<JsonObject> PayViaCardAsync(long orderId, string cardNumber, string cardExpiry, CancellationToken cancellationToken = default)
        => PostAsync("pay_via_card", new Dictionary<string, object?>
        {
            ["order_id"] = orderId,
            ["card_number"] = cardNumber,
            ["card_expiry"] = cardExpiry,
        }, new Dictionary<string, string>(), cancellationToken);

    /// <summary>Confirm a card payment started with <see cref="PayViaCardAsync"/> using the SMS code the cardholder received.</summary>
    public Task<JsonObject> ConfirmCardPaymentAsync(string paymentToken, string smsCode, CancellationToken cancellationToken = default)
        => PostAsync("confirm_card_payment", new Dictionary<string, object?>
        {
            ["payment_token"] = paymentToken,
            ["sms_code"] = smsCode,
        }, new Dictionary<string, string>(), cancellationToken);
}
