using System;
using UnityEngine;

public static class ApiEndpointRuntime
{
    // Resolved once per startup. Can be overridden with ?api=https://api.example.com
    public static string BaseUrl { get; private set; } = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Resolve()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        BaseUrl = ResolveWebGlApiBaseUrl();
#else
        BaseUrl = "";
#endif

        if (!string.IsNullOrWhiteSpace(BaseUrl))
            Debug.Log($"[ApiEndpointRuntime] API endpoint resolved to {BaseUrl}");
    }

    static string ResolveWebGlApiBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(Application.absoluteURL))
            return "";

        if (!Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out Uri pageUri))
            return "";

        string fromQuery = GetQueryParam(pageUri, "api");
        if (!string.IsNullOrWhiteSpace(fromQuery))
            return fromQuery.Trim();

        // Default convention: same host, /api path, HTTPS.
        string host = pageUri.Host;
        if (string.IsNullOrWhiteSpace(host))
            return "";

        return $"https://{host}/api";
    }

    static string GetQueryParam(Uri uri, string key)
    {
        if (uri == null || string.IsNullOrWhiteSpace(key))
            return "";

        string query = uri.Query;
        if (string.IsNullOrWhiteSpace(query))
            return "";

        if (query[0] == '?')
            query = query.Substring(1);

        string[] pairs = query.Split('&');
        for (int i = 0; i < pairs.Length; ++i)
        {
            string[] kv = pairs[i].Split(new[] { '=' }, 2);
            if (kv.Length == 2 && string.Equals(Uri.UnescapeDataString(kv[0]), key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }

        return "";
    }
}
