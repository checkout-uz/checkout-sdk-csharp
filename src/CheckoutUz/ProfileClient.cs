using System.Text.Json.Nodes;

namespace CheckoutUz;

/// <summary>
/// Client for the account-level (profile) v2 endpoints. Authenticated with
/// a <strong>profile API key</strong> (from account settings), sent as
/// <c>Authorization: Bearer &lt;profileApiKey&gt;</c>. This is a different
/// credential from a shop's API key used by <see cref="CheckoutClient"/> -
/// it is not tied to one shop and can read data across every shop the
/// account owns.
/// <code>
/// var profile = new ProfileClient("profile_api_key");
/// await profile.GetShopsAsync();
/// </code>
/// </summary>
public class ProfileClient : BaseClient
{
    public ProfileClient(string profileApiKey) : base(V2Client.BaseUrlConstant, profileApiKey)
    {
    }

    internal ProfileClient(string profileApiKey, IHttpTransport transport)
        : base(V2Client.BaseUrlConstant, profileApiKey, transport)
    {
    }

    /// <summary>Return the authenticated account's profile information.</summary>
    public Task<JsonObject> GetProfileAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_profile", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>List every (non-deleted) shop owned by the authenticated account.</summary>
    public Task<JsonObject> GetShopsAsync(CancellationToken cancellationToken = default)
        => PostAsync("get_shops", new Dictionary<string, object?>(), new Dictionary<string, string>(), cancellationToken);

    /// <summary>Return aggregate stats across all of the account's shops, broken down by shop and by provider.</summary>
    public Task<JsonObject> GetProfileStatsAsync(string? dateFrom = null, string? dateTo = null, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?> { ["date_from"] = dateFrom, ["date_to"] = dateTo };
        return PostAsync("get_profile_stats", parameters, new Dictionary<string, string>(), cancellationToken);
    }
}
