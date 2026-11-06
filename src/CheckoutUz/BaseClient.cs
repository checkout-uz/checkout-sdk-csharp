using System.Text.Json;
using System.Text.Json.Nodes;

namespace CheckoutUz;

/// <summary>
/// Shared request/response handling for all CHECKOUT.UZ API clients. Every
/// endpoint accepts a JSON body over POST and replies with a JSON body -
/// any non-2xx HTTP status is treated as an error.
/// </summary>
public abstract class BaseClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);
    private static readonly IReadOnlyDictionary<string, string> NoHeaders =
        new Dictionary<string, string>();

    protected string BaseUrl { get; }
    private readonly string _token;
    private readonly IHttpTransport _transport;

    protected BaseClient(string baseUrl, string token)
        : this(baseUrl, token, new DefaultHttpTransport(DefaultTimeout))
    {
    }

    /// <summary>Used by tests to inject a fake <see cref="IHttpTransport"/>.</summary>
    internal BaseClient(string baseUrl, string token, IHttpTransport transport)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("An API token is required", nameof(token));
        }
        BaseUrl = baseUrl.TrimEnd('/');
        _token = token;
        _transport = transport;
    }

    protected Task<JsonObject> PostAsync(string methodName, IDictionary<string, object?> parameters)
        => PostAsync(methodName, parameters, NoHeaders, CancellationToken.None);

    protected Task<JsonObject> PostAsync(
        string methodName,
        IDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, string> extraHeaders)
        => PostAsync(methodName, parameters, extraHeaders, CancellationToken.None);

    protected async Task<JsonObject> PostAsync(
        string methodName,
        IDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, string> extraHeaders,
        CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/{methodName}";

        var body = new JsonObject();
        foreach (var (key, value) in parameters)
        {
            if (value is null) continue;
            body[key] = JsonValue.Create(value);
        }
        var jsonBody = body.ToJsonString();

        HttpTransportResponse response;
        try
        {
            response = await _transport.SendAsync(url, _token, jsonBody, extraHeaders, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not CheckoutApiException)
        {
            throw new CheckoutApiException($"Network error while calling {methodName}: {ex.Message}", ex);
        }

        JsonObject? data = null;
        if (!string.IsNullOrEmpty(response.Body))
        {
            try
            {
                data = JsonNode.Parse(response.Body) as JsonObject;
            }
            catch (JsonException)
            {
                // Non-JSON body - leave data null, handled below.
            }
        }

        if (response.StatusCode < 200 || response.StatusCode >= 300)
        {
            var message = data?["error"]?.GetValue<string>()
                ?? data?["message"]?.GetValue<string>()
                ?? "Unknown error";
            throw new CheckoutApiException(message, response.StatusCode, response.Body);
        }

        return data ?? new JsonObject();
    }
}
