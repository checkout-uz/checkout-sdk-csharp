namespace CheckoutUz;

/// <summary>
/// Thrown whenever the CHECKOUT.UZ API returns an HTTP error status or a
/// network error occurs. <see cref="StatusCode"/> is 0 if the request
/// never reached the server.
/// </summary>
public class CheckoutApiException : Exception
{
    /// <summary>The HTTP status code returned by the API (0 if the request never reached the server).</summary>
    public int StatusCode { get; }

    /// <summary>The raw JSON response body, if any was returned.</summary>
    public string? RawResponse { get; }

    public CheckoutApiException(string message, int statusCode = 0, string? rawResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        RawResponse = rawResponse;
    }

    public CheckoutApiException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = 0;
        RawResponse = null;
    }
}
