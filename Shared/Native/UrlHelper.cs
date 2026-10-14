using Serilog;

namespace WorkLifeBalance.Shared.Native
{
    public static class UrlHelper
    {
        public static bool TryGetHost(string? url, out string? host)
        {
            host = null;
            if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                return false;

            try
            {
                host = new Uri(url).Authority;
                return true;
            }
            catch (UriFormatException)
            {
                Log.Warning("Failed to get authority from url: {Url}. Trying to resolve by schema/host", url);
                if (ValidateWithUriBuilder(url, out host))
                    return true;
            }

            Log.Warning("Failed to get host from url: {Url}", url);
            return false;
        }

        private static bool ValidateWithUriBuilder(string url, out string? host)
        {
            host = null;
            var trimmed = url.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return false;

            try
            {
                var builder = new UriBuilder(trimmed);
                if (GetSchema(trimmed) is { } schema)
                {
                    builder.Scheme = schema;
                    host = builder.Uri.Authority;
                    return true;
                }

                host = builder.Host;
                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        private static string? GetSchema(string url)
        {
            if (url.StartsWith("https://", StringComparison.Ordinal))
                return "https";

            if (url.StartsWith("http://", StringComparison.Ordinal))
                return "http";

            return null;
        }
    }
}
