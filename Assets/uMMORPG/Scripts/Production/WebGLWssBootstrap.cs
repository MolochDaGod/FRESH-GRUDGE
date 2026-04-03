using System;
using UnityEngine;
using Mirror;
using Mirror.SimpleWeb;

[DisallowMultipleComponent]
public class WebGLWssBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInstall()
    {
        NetworkManager manager = NetworkManager.singleton ?? FindObjectOfType<NetworkManager>();
        if (manager == null)
            return;

        if (manager.GetComponent<WebGLWssBootstrap>() == null)
            manager.gameObject.AddComponent<WebGLWssBootstrap>();
    }

    [Header("Endpoint Overrides")]
    [Tooltip("Optional fixed host override. Leave empty to derive host from browser URL.")]
    public string hostOverride = "";

    [Tooltip("Optional fixed port override. Use 0 to keep current transport port.")]
    public ushort portOverride = 0;

    [Header("Runtime Query Parameters")]
    [Tooltip("URL query parameter key for host. Example: ?server=play.example.com")]
    public string hostQueryKey = "server";

    [Tooltip("URL query parameter key for port. Example: ?port=443")]
    public string portQueryKey = "port";

    [Header("WSS Policy")]
    [Tooltip("Force WebSocket Secure mode for browser clients.")]
    public bool forceWss = true;

    [Tooltip("When behind reverse proxy/CDN TLS, keep this disabled and use clientUseWss only.")]
    public bool enableTransportSsl = false;

    [Tooltip("Write resolved endpoint info to console on startup.")]
    public bool logResolvedEndpoint = true;

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ApplyWebGlSettings();
#endif
    }

    void ApplyWebGlSettings()
    {
        if (NetworkManager.singleton == null)
            return;

        NetworkManager manager = NetworkManager.singleton;

        string resolvedHost = hostOverride;
        if (string.IsNullOrWhiteSpace(resolvedHost))
            resolvedHost = ResolveRuntimeHost(hostQueryKey);

        ushort resolvedPort = portOverride;
        if (resolvedPort == 0)
            resolvedPort = ResolveRuntimePort(portQueryKey);

        ApplyOverrides(manager, resolvedHost, resolvedPort, forceWss, enableTransportSsl, logResolvedEndpoint);
    }

    public static void ApplyRuntimeOverrides(NetworkManager manager)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string resolvedHost = ResolveRuntimeHost("server");
        ushort resolvedPort = ResolveRuntimePort("port");
        ApplyOverrides(manager, resolvedHost, resolvedPort, true, false, false);
#endif
    }

    static void ApplyOverrides(NetworkManager manager, string resolvedHost, ushort resolvedPort, bool forceWss, bool enableTransportSsl, bool logResolvedEndpoint)
    {
        if (manager == null)
            return;

        if (!string.IsNullOrWhiteSpace(resolvedHost))
            manager.networkAddress = resolvedHost.Trim();

        if (manager.transport is SimpleWebTransport simpleWeb)
        {
            if (forceWss)
                simpleWeb.clientUseWss = true;

            simpleWeb.sslEnabled = enableTransportSsl;

            if (resolvedPort != 0)
                simpleWeb.port = resolvedPort;

            if (logResolvedEndpoint)
            {
                string scheme = (simpleWeb.sslEnabled || simpleWeb.clientUseWss) ? "wss" : "ws";
                Debug.Log($"[WebGLWssBootstrap] Endpoint resolved to {scheme}://{manager.networkAddress}:{simpleWeb.port}");
            }
        }
        else if (logResolvedEndpoint)
        {
            Debug.LogWarning("[WebGLWssBootstrap] Active transport is not SimpleWebTransport. WSS policy was not applied.");
        }
    }

    static string ResolveRuntimeHost(string key)
    {
        string resolvedHost = GetQueryParam(key);

        if (string.IsNullOrWhiteSpace(resolvedHost) && Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out Uri pageUri))
            resolvedHost = pageUri.Host;

        return resolvedHost;
    }

    static ushort ResolveRuntimePort(string key)
    {
        string queryPort = GetQueryParam(key);
        if (!string.IsNullOrWhiteSpace(queryPort) && ushort.TryParse(queryPort, out ushort parsedPort))
            return parsedPort;

        return 0;
    }

    static string GetQueryParam(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(Application.absoluteURL))
            return "";

        if (!Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out Uri uri))
            return "";

        string query = uri.Query;
        if (string.IsNullOrWhiteSpace(query))
            return "";

        if (query[0] == '?')
            query = query.Substring(1);

        string[] parts = query.Split('&');
        for (int i = 0; i < parts.Length; ++i)
        {
            string[] kv = parts[i].Split(new[] { '=' }, 2);
            if (kv.Length == 2 && string.Equals(Uri.UnescapeDataString(kv[0]), key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }

        return "";
    }
}
