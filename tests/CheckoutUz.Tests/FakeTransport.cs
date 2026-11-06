using CheckoutUz;

namespace CheckoutUz.Tests;

/// <summary>Records every call it receives and returns a canned response - no real network calls.</summary>
internal sealed class FakeTransport : IHttpTransport
{
    public sealed record Call(string Url, string Token, string JsonBody, IReadOnlyDictionary<string, string> ExtraHeaders);

    public readonly List<Call> Calls = new();
    private readonly int _statusCode;
    private readonly string? _responseBody;
    private readonly Exception? _toThrow;

    public FakeTransport(int statusCode, string? responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    private FakeTransport(Exception toThrow)
    {
        _toThrow = toThrow;
    }

    public static FakeTransport Throwing(Exception ex) => new(ex);

    public Task<HttpTransportResponse> SendAsync(
        string url,
        string token,
        string jsonBody,
        IReadOnlyDictionary<string, string> extraHeaders,
        CancellationToken cancellationToken)
    {
        Calls.Add(new Call(url, token, jsonBody, extraHeaders));
        if (_toThrow is not null) throw _toThrow;
        return Task.FromResult(new HttpTransportResponse(_statusCode, _responseBody ?? ""));
    }
}
