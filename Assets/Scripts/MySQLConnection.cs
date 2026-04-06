using System;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// MySQLConnection
//
// Holds all connection configuration for the Grudge backend API.
// WebGL cannot open raw TCP sockets, so all MySQL access goes through
// the Node.js REST API (localhost:3000 or your VPS URL).
//
// Create via:  Assets → Create → Grudge / MySQL Connection Config
// ─────────────────────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "MySQLConnection", menuName = "Grudge/MySQL Connection Config")]
public class MySQLConnection : ScriptableObject
{
    [Header("API Gateway (Node.js backend)")]
    [Tooltip("Base URL of your Node.js REST API — no trailing slash.\nExamples:\n  http://localhost:3000\n  https://api.grudge-studio.com")]
    public string apiBaseUrl = "http://localhost:3000";

    [Header("Request Settings")]
    [Tooltip("Seconds before a request times out")]
    public float requestTimeout = 10f;

    [Tooltip("Shared secret sent in the X-API-Key header for server-to-server calls")]
    public string apiKey = "";

    [Header("MySQL (server-side reference only)")]
    [Tooltip("Only used for documentation / server setup. Unity never opens a raw MySQL socket.")]
    public string mysqlHost = "localhost";
    public int    mysqlPort = 3306;
    public string mysqlDatabase = "c0gamemanager";
    public string mysqlUser     = "0gamerDBsql";
    [Tooltip("Never store real passwords in assets — use env vars on the server.")]
    public string mysqlPassword = "";

    // ── Convenience ───────────────────────────────────────────────────────────
    public string Endpoint(string path) =>
        $"{apiBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}

// ─────────────────────────────────────────────────────────────────────────────
// Data transfer objects shared across the database layer
// ─────────────────────────────────────────────────────────────────────────────
[Serializable]
public class PlayerProfile
{
    public int    player_id;
    public string account_name;
    public string display_name;
    public int    level;
    public long   experience;
    public long   gold;
    public long   coins;
    public int    rating;
    public int    kills;
    public int    deaths;
    public int    score;
    public string class_name;
    public string last_login;
    public string created_at;
}

[Serializable]
public class ApiLeaderboardEntry
{
    public int    rank;
    public int    player_id;
    public string display_name;
    public int    rating;
    public int    level;
    public int    kills;
    public int    deaths;
}

[Serializable]
public class UpdateStatsRequest
{
    public int kills;
    public int deaths;
    public int score;
}

[Serializable]
public class ApiResponse
{
    public bool   success;
    public string message;
    public string error;
}

[Serializable]
public class PlayerProfileResponse
{
    public bool          success;
    public PlayerProfile data;
    public string        error;
}

[Serializable]
public class LeaderboardResponse
{
    public bool               success;
    public ApiLeaderboardEntry[] data;
    public string             error;
}

[Serializable]
public class LeaderboardResponseWrapper
{
    public ApiLeaderboardEntry[] entries;
}
