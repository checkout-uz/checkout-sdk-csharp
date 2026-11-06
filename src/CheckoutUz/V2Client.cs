using System.Text.Json.Nodes;

namespace CheckoutUz;

/// <summary>
/// Client for the CHECKOUT.UZ merchant API v2 (<c>/api/v2</c>).
/// Authenticated with the same shop API key as v1. v2 adds idempotent
/// payment creation, pagination, cancellation/refunds and fiscal receipts
/// on top of everything v1 offers.
///
/// <para>Note: <see cref="ProfileClient"/>'s GetProfile/GetShops/GetProfileStats
/// are account-level (not shop-level) and use a <em>different</em> credential.</para>
/// </summary>
public class V2Client : BaseClient
{
    public const string BaseUrlConstant = "https://checkout.uz/api/v2";

    public V2Client(string apiKey) : base(BaseUrlConstant, apiKey)
    {
    }

    internal V2Client(string apiKey, IHttpTransport transport) : base(BaseUrlConstant, apiKey, transport)
    {
    }

    /// <summary>
    /// Create a new payment invoice. <paramref name="amount"/> is in UZS (1000-10,000,000).
    /// <paramref name="expireTime"/> is in seconds (60-432000, default 3600). If
    /// <paramref name="idempotencyKey"/> is given and the same key was used for this shop
    /// within the last 24 hours, the original invoice is returned instead of creating a
    /// new one (<c>idempotent_replay</c> in the response tells you which happened).
    /// </summary>
    public Task<JsonObject> CreatePaymentAsync(
        double amount,
        string? description = null,
        int? expireTime = null,
        string? webhookUrl = null,
        string? returnUrl = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["amount"] = amount,
            ["description"] = description,
            ["expire_time"] = expireTime,
            ["webhook_url"] = webhookUrl,
            ["return_url"] = returnUrl,
        };
        var headers = idempotencyKey is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { ["Idempotency-Key"] = idempotencyKey };
        return PostAsync("create_payment", parameters, headers, cancellationToken);
    }

    /// <summary>Look up an invoice by its numeric id.</summary>
    public Task<JsonObject> StatusPaymentAsync(long id, CancellationToken cancellationToken = default)
        => PostAsync("status_payment", new Dictionary<string, object?> { ["id"] = id },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Look up an invoice by its uuid (pay_token).</summary>
    public Task<JsonObject> StatusPaymentAsync(string uuid, CancellationToken cancellationToken = default)
        => PostAsync("status_payment", new Dictionary<string, object?> { ["uuid"] = uuid },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Return the shop's current balance per currency (uppercase currency codes).</summary>
    public Task<JsonObject> GetBalanceAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_balance", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>
    /// Return the shop's invoices, newest first. <paramref name="limit"/> is 1-200.
    /// <paramref name="status"/> filters by one of pending/paid/canceled/reversed.
    /// <paramref name="dateFrom"/>/<paramref name="dateTo"/> accept any format PHP's
    /// <c>strtotime()</c> understands (e.g. "2026-01-01"). The response's <c>has_more</c>
    /// tells you whether another page exists.
    /// </summary>
    public Task<JsonObject> GetHistoryAsync(
        int limit = 10,
        int offset = 0,
        string? status = null,
        string? dateFrom = null,
        string? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["limit"] = limit,
            ["offset"] = offset,
            ["status"] = status,
            ["date_from"] = dateFrom,
            ["date_to"] = dateTo,
        };
        return PostAsync("get_history", parameters, new Dictionary<string, string>(), cancellationToken);
    }

    /// <summary>Return aggregate order count / income for the shop, broken down by payment provider.</summary>
    public Task<JsonObject> GetStatsAsync(string? dateFrom = null, string? dateTo = null, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?> { ["date_from"] = dateFrom, ["date_to"] = dateTo };
        return PostAsync("get_stats", parameters, new Dictionary<string, string>(), cancellationToken);
    }

    /// <summary>List the payment methods enabled for this shop, including min/max amount and fiscal support.</summary>
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

    /// <summary>Cancel a pending invoice. Fails if the invoice is not in <c>pending</c> status.</summary>
    public Task<JsonObject> CancelPaymentAsync(long orderId, CancellationToken cancellationToken = default)
        => PostAsync("cancel_payment", new Dictionary<string, object?> { ["order_id"] = orderId },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>
    /// Reverse a successful card/Payme/Click payment and deduct it back from the shop
    /// balance. Only available within a limited window after the original payment
    /// (currently 22 hours); the API returns an error past that point.
    /// </summary>
    public Task<JsonObject> RefundPaymentAsync(long orderId, CancellationToken cancellationToken = default)
        => PostAsync("refund_payment", new Dictionary<string, object?> { ["order_id"] = orderId },
            new Dictionary<string, string>(), cancellationToken);

    /// <summary>Fetch the fiscal (tax) receipt for a paid order. Supported for Payme, Click and Plum payments only.</summary>
    public Task<JsonObject> GetFiscalAsync(long orderId, CancellationToken cancellationToken = default)
        => PostAsync("get_fiscal", new Dictionary<string, object?> { ["order_id"] = orderId },
            new Dictionary<string, string>(), cancellationToken);
}
