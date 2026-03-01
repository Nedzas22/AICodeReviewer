using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CodeLens.Web.Helpers;

/// <summary>Client-side JWT utilities — parses claims and checks expiry without a full JWT library.</summary>
public static class JwtHelper
{
    /// <summary>Extracts claims from the JWT payload without signature validation.</summary>
    public static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3) return [];

        var payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        // Re-pad to a valid Base64 length
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .EnumerateObject()
            .Select(p => new Claim(
                p.Name,
                p.Value.ValueKind == JsonValueKind.String
                    ? p.Value.GetString()!
                    : p.Value.ToString()))
            .ToList();
    }

    /// <summary>Returns <c>true</c> when the JWT <c>exp</c> claim is in the future.</summary>
    public static bool IsTokenValid(string jwt)
    {
        var expClaim = ParseClaims(jwt).FirstOrDefault(c => c.Type == "exp");
        if (expClaim is null) return false;
        if (!long.TryParse(expClaim.Value, out var exp)) return false;
        return DateTimeOffset.FromUnixTimeSeconds(exp) > DateTimeOffset.UtcNow;
    }
}
