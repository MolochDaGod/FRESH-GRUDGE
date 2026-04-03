using UnityEngine;
using Mirror;
using Mirror.SimpleWeb;

[DisallowMultipleComponent]
public class ProductionReadinessGuard : MonoBehaviour
{
    [Tooltip("If true, automatically replace placeholder networkAddress with first serverList entry.")]
    public bool autoFixNetworkAddress = true;

    [Tooltip("If true, force WSS for WebGL clients when using SimpleWebTransport.")]
    public bool autoForceWssOnWebGl = true;

    [Tooltip("If true, print summary output at startup.")]
    public bool logSummary = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInstall()
    {
        NetworkManagerMMO manager = NetworkManager.singleton as NetworkManagerMMO;
        if (manager == null)
            manager = FindObjectOfType<NetworkManagerMMO>();

        if (manager == null)
            return;

        if (manager.GetComponent<ProductionReadinessGuard>() == null)
            manager.gameObject.AddComponent<ProductionReadinessGuard>();
    }

    void Start()
    {
        ValidateAndFix();
    }

    public void ValidateAndFix()
    {
        NetworkManagerMMO manager = NetworkManager.singleton as NetworkManagerMMO;
        if (manager == null)
            return;

        int warnings = 0;
        int fixes = 0;

        if (string.IsNullOrWhiteSpace(manager.networkAddress) || manager.networkAddress.Contains("Server List"))
        {
            warnings++;
            Debug.LogWarning("[ProductionReadinessGuard] Network address is empty/placeholder.");

            if (autoFixNetworkAddress && manager.serverList != null && manager.serverList.Count > 0)
            {
                string fallbackIp = manager.serverList[0].ip;
                if (!string.IsNullOrWhiteSpace(fallbackIp))
                {
                    manager.networkAddress = fallbackIp.Trim();
                    fixes++;
                    Debug.Log($"[ProductionReadinessGuard] Applied fallback network address: {manager.networkAddress}");
                }
            }
        }

        if (manager.serverList == null || manager.serverList.Count == 0)
        {
            warnings++;
            Debug.LogWarning("[ProductionReadinessGuard] Server list is empty. WebGL clients need a production endpoint or URL override.");
        }
        else if (manager.serverList.Count == 1)
        {
            string onlyServer = manager.serverList[0].ip;
            if (string.Equals(onlyServer, "localhost", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(onlyServer, "127.0.0.1", System.StringComparison.OrdinalIgnoreCase))
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] Server list still points to localhost. Replace it for staging/production or pass ?server=... in the WebGL URL.");
            }
        }

        if (manager.transport is SimpleWebTransport simpleWeb)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (autoForceWssOnWebGl && !simpleWeb.clientUseWss)
            {
                simpleWeb.clientUseWss = true;
                fixes++;
                Debug.Log("[ProductionReadinessGuard] Enabled clientUseWss for WebGL production safety.");
            }

            if (simpleWeb.port == 0)
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] SimpleWebTransport port is 0. Set a valid WSS port (typically 443 via proxy). ");
            }
            else if (simpleWeb.port != 443 && portLooksExternal(manager.networkAddress))
            {
                Debug.LogWarning($"[ProductionReadinessGuard] WebGL client is using port {simpleWeb.port}. Ensure your reverse proxy/CDN forwards that WSS port correctly.");
            }

            if (Application.absoluteURL.StartsWith("https://") && !simpleWeb.clientUseWss)
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] HTTPS page with non-WSS client transport is likely to fail in browsers.");
            }
#endif
        }
        else
        {
            warnings++;
            Debug.LogWarning("[ProductionReadinessGuard] Active transport is not SimpleWebTransport. Verify your WebGL transport supports WSS.");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(ApiEndpointRuntime.BaseUrl))
        {
            warnings++;
            Debug.LogWarning("[ProductionReadinessGuard] API endpoint was not resolved. Use ?api=https://api.yourdomain.com or host /api on same domain.");
        }
#endif

#if _MYSQL
        if (Database.singleton != null)
        {
            if (string.IsNullOrWhiteSpace(Database.singleton.dbHost) ||
                string.Equals(Database.singleton.dbHost, "localhost", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Database.singleton.dbHost, "127.0.0.1", System.StringComparison.OrdinalIgnoreCase))
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] MySQL host is still localhost/empty. Use your actual database host for deployment.");
            }

            if (string.IsNullOrWhiteSpace(Database.singleton.dbName) || string.Equals(Database.singleton.dbName, "dbName", System.StringComparison.OrdinalIgnoreCase))
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] MySQL database name is still the default placeholder.");
            }

            if (string.IsNullOrWhiteSpace(Database.singleton.dbUser) || string.Equals(Database.singleton.dbUser, "dbUser", System.StringComparison.OrdinalIgnoreCase))
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] MySQL username is still the default placeholder.");
            }

            if (string.IsNullOrWhiteSpace(Database.singleton.dbPassword) || string.Equals(Database.singleton.dbPassword, "dbPassword", System.StringComparison.OrdinalIgnoreCase))
            {
                warnings++;
                Debug.LogWarning("[ProductionReadinessGuard] MySQL password is still the default placeholder.");
            }
        }
#endif

        if (logSummary)
        {
            Debug.Log($"[ProductionReadinessGuard] Startup checks complete. fixes={fixes}, warnings={warnings}, address={manager.networkAddress}");
        }
    }

    bool portLooksExternal(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        return !string.Equals(host, "localhost", System.StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(host, "127.0.0.1", System.StringComparison.OrdinalIgnoreCase);
    }
}
