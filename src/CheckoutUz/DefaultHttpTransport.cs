using System.Net.Http.Headers;
using System.Text;

namespace CheckoutUz;

/// <summary>Real HTTP transport, built on <see cref="HttpClient"/>, no extra dependency.</summary>
internal sealed class DefaultHttpTransport : IHttpTransport
{
    private static readonly HttpClient SharedClient = new();
    private readonly TimeSpan _timeout;

    public DefaultHttpTransport(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public async Task<HttpTransportResponse> SendAsync(
        string url,
        string token,
        string jsonBody,
        IReadOnlyDictionary<string, string> extraHeaders,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        foreach (var (key, value) in extraHeaders)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_timeout);

        using var response = await SharedClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new HttpTransportResponse((int)response.StatusCode, body);
    }
}
