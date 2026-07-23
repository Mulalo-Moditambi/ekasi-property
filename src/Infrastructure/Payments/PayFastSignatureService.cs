using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Payments;

internal sealed class PayFastSignatureService
{
    public string Sign(IEnumerable<KeyValuePair<string, string>> orderedFields, string? passphrase)
    {
        var builder = new StringBuilder();

        foreach ((string key, string value) in orderedFields)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(key).Append('=').Append(WebUtility.UrlEncode(value.Trim()));
        }

        if (!string.IsNullOrEmpty(passphrase))
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append("passphrase=").Append(WebUtility.UrlEncode(passphrase.Trim()));
        }

        // PayFast's ITN signature protocol mandates MD5 - not our choice, dictated by their API.
#pragma warning disable CA5351
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
#pragma warning restore CA5351

        return Convert.ToHexStringLower(hash);
    }
}
