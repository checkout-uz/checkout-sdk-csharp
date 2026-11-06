namespace CheckoutUz;

/// <summary>
/// Low-level transport abstraction used by every client. The default
/// implementation (<see cref="DefaultHttpTransport"/>) uses
/// <see cref="System.Net.Http.HttpClient"/>; tests inject a fake
/// implementation instead of making real network calls.
/// </summary>
public interface IHttpTransport
{
    Task<HttpTransportResponse> SendAsync(
        string url,
        string token,
        string jsonBody,
        IReadOnlyDictionary<string, string> extraHeaders,
        CancellationToken cancellationToken);
}

public readonly struct HttpTransportResponse
{
    public int StatusCode { get; }
    public string Body { get; }

    public HttpTransportResponse(int statusCode, string body)
    {
        StatusCode = statusCode;
        Body = body;
    }
}
