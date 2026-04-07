using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ─────────────────────────────────────────────────────────────────────────────
// DatabaseConnector
//
// Singleton MonoBehaviour that handles all communication with the Grudge
// Node.js REST API. WebGL builds cannot open raw TCP sockets, so every
// MySQL operation goes through HTTP requests to the backend API.
//
// SETUP:
//   1. Create a GameObject → name it "DatabaseManager"
//   2. Add this component
//   3. Drag a MySQLConnection asset into the "Config" field
//      (or fill in the API URL directly in the inspector)
//
// USAGE:
//   DatabaseConnector.Instance.GetPlayerProfile(1,
//       onSuccess: p => Debug.Log(p.display_name),
//       onError:   e => Debug.LogError(e));
// ─────────────────────────────────────────────────────────────────────────────
public class DatabaseConnector : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static DatabaseConnector Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Configuration")]
    [Tooltip("Drag a MySQLConnection ScriptableObject here, OR fill the fields below")]
    public MySQLConnection config;

    [Header("Override (used if no Config asset is assigned)")]
    public string apiBaseUrl    = "http://localhost:3000";
    public float  requestTimeout = 10f;
    public string apiKey         = "";

    // ── State ─────────────────────────────────────────────────────────────────
    bool _isConnected;
    public bool IsConnected => _isConnected;

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(CheckConnection());
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Check if the backend API is reachable.</summary>
    public void CheckHealth(Action<bool> callback = null)
    {
        StartCoroutine(CheckConnection(callback));
    }

    /// <summary>Fetch a player's full profile by ID.</summary>
    public void GetPlayerProfile(int playerId,
        Action<PlayerProfile> onSuccess, Action<string> onError)
    {
        StartCoroutine(Get<PlayerProfileResponse>(
            $"api/players/{playerId}",
            res => {
                if (res.success && res.data != null) onSuccess?.Invoke(res.data);
                else onError?.Invoke(res.error ?? "No data returned");
            },
            onError));
    }

    /// <summary>Fetch the top N players from the leaderboard.</summary>
    public void GetLeaderboard(int limit,
        Action<ApiLeaderboardEntry[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(Get<LeaderboardResponse>(
            $"api/leaderboard?limit={limit}",
            res => {
                if (res.success && res.data != null) onSuccess?.Invoke(res.data);
                else onError?.Invoke(res.error ?? "No leaderboard data");
            },
            onError));
    }

    /// <summary>Post updated combat stats for a player after a match.</summary>
    public void UpdatePlayerStats(int playerId, int kills, int deaths, int score,
        Action<ApiResponse> onSuccess, Action<string> onError)
    {
        var body = new UpdateStatsRequest { kills = kills, deaths = deaths, score = score };
        StartCoroutine(Post<ApiResponse>(
            $"api/players/{playerId}/stats",
            body,
            res => {
                if (res.success) onSuccess?.Invoke(res);
                else onError?.Invoke(res.error ?? "Stats update failed");
            },
            onError));
    }

    /// <summary>Register a new account. Returns the created profile.</summary>
    public void CreateAccount(string accountName, string password, string displayName,
        Action<PlayerProfile> onSuccess, Action<string> onError)
    {
        var body = new { account_name = accountName, password, display_name = displayName };
        StartCoroutine(Post<PlayerProfileResponse>(
            "api/players",
            body,
            res => {
                if (res.success && res.data != null) onSuccess?.Invoke(res.data);
                else onError?.Invoke(res.error ?? "Account creation failed");
            },
            onError));
    }

    /// <summary>Authenticate an account. Returns the profile on success.</summary>
    public void Login(string accountName, string password,
        Action<PlayerProfile> onSuccess, Action<string> onError)
    {
        var body = new { account_name = accountName, password };
        StartCoroutine(Post<PlayerProfileResponse>(
            "api/auth/login",
            body,
            res => {
                if (res.success && res.data != null) onSuccess?.Invoke(res.data);
                else onError?.Invoke(res.error ?? "Login failed");
            },
            onError));
    }

    // ── Internal coroutines ───────────────────────────────────────────────────

    IEnumerator CheckConnection(Action<bool> callback = null)
    {
        using var req = UnityWebRequest.Get(Endpoint("health"));
        SetTimeout(req);
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        _isConnected = req.result == UnityWebRequest.Result.Success;
        if (_isConnected)
            Debug.Log($"[DB] Connected to API at {BaseUrl()}");
        else
            Debug.LogWarning($"[DB] API not reachable: {req.error}");

        callback?.Invoke(_isConnected);
    }

    IEnumerator Get<T>(string path, Action<T> onSuccess, Action<string> onError)
    {
        using var req = UnityWebRequest.Get(Endpoint(path));
        SetTimeout(req);
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
            yield break;
        }

        try
        {
            var result = JsonUtility.FromJson<T>(req.downloadHandler.text);
            onSuccess?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"JSON parse error: {ex.Message}\n{req.downloadHandler.text}");
        }
    }

    IEnumerator Post<T>(string path, object body, Action<T> onSuccess, Action<string> onError)
    {
        string json    = JsonUtility.ToJson(body);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(Endpoint(path), "POST");
        req.uploadHandler   = new UploadHandlerRaw(payload);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        SetTimeout(req);
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
            yield break;
        }

        try
        {
            var result = JsonUtility.FromJson<T>(req.downloadHandler.text);
            onSuccess?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"JSON parse error: {ex.Message}\n{req.downloadHandler.text}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    string BaseUrl() => config != null ? config.apiBaseUrl : apiBaseUrl;

    string Endpoint(string path)
    {
        if (config != null) return config.Endpoint(path);
        return $"{apiBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    float Timeout() => config != null ? config.requestTimeout : requestTimeout;

    string ApiKey() => config != null ? config.apiKey : apiKey;

    void SetTimeout(UnityWebRequest req) => req.timeout = Mathf.CeilToInt(Timeout());

    void AddAuthHeader(UnityWebRequest req)
    {
        string key = ApiKey();
        if (!string.IsNullOrEmpty(key))
            req.SetRequestHeader("X-API-Key", key);
    }
}
