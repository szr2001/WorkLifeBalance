using System;
using Serilog;

namespace WorkLifeBalance.Helpers;

public static class UrlHelper
{
    public static bool TryGetHost(string url, out string? host)
    {
        if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            host = null;
            return false;
        }
        
        try
        {
            var uri = new Uri(url);

            host = uri.Authority;
            return true;
        } catch (UriFormatException e)
        {
            Log.Warning("Failed to get authority from url: " + url + " with error: " + e);

            if (ValidateWithUriBuilder(url, out string? newUrl))
            {
                host = newUrl;
                return true;
            }
        }
        
        Log.Warning("Failed to get host from url: " + url);
        host = null;
        return false;
    }

    private static bool ValidateWithUriBuilder(string url, out string? newUrl)
    {
        string urlToValidate = url.TrimStart().TrimEnd();
        
        if (string.IsNullOrEmpty(urlToValidate))
        {
            newUrl = null;
            return false;
        }

        try
        {
            UriBuilder uriBuilder = new UriBuilder(urlToValidate);

            string? schema = GetSchema(urlToValidate);
            if (schema != null)
            {
                uriBuilder.Scheme = schema;

                newUrl = uriBuilder.Uri.Authority;
                return true;
            }
            
            newUrl = uriBuilder.Host;
            return true;
        }
        catch (UriFormatException e)
        {
            newUrl = null;
            return false;
        }
    }

    private static string? GetSchema(string urlToValidate)
    {
        try
        {

            if (urlToValidate.AsSpan(0, 8).ToString() == "https://")
            {
                return "https";
            }

            if (urlToValidate.AsSpan(0, 7).ToString() == "http://")
            {
                return "http";
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }

        return null;
    }
}