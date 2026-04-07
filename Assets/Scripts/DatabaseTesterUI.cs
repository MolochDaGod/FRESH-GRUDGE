using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// DatabaseTesterUI
//
// In-game panel for testing all DatabaseConnector endpoints.
// Attach to a Canvas UI panel. Wire up the buttons and text fields
// in the Inspector.
//
// PREFAB SETUP (all fields optional — leave null to skip that UI element):
//   statusText       — shows connection state
//   logText          — scrollable result log
//   playerIdField    — InputField for player ID
//   limitField       — InputField for leaderboard limit
//   accountNameField — InputField for account name
//   passwordField    — InputField for password
//   displayNameField — InputField for display name
//   killsField       — InputField for kills
//   deathsField      — InputField for deaths
//   scoreField       — InputField for score
//   [Buttons]        — wire each to the matching On___ method
// ─────────────────────────────────────────────────────────────────────────────
public class DatabaseTesterUI : MonoBehaviour
{
    [Header("Status")]
    public Text statusText;
    public Text logText;
    public Image statusIndicator;  // green = connected, red = not

    [Header("Player Profile")]
    public InputField playerIdField;

    [Header("Leaderboard")]
    public InputField limitField;

    [Header("Create Account")]
    public InputField accountNameField;
    public InputField passwordField;
    public InputField displayNameField;

    [Header("Update Stats")]
    public InputField killsField;
    public InputField deathsField;
    public InputField scoreField;

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Start()
    {
        SetStatus("Checking connection...", Color.yellow);
        DatabaseConnector.Instance.CheckHealth(connected =>
        {
            if (connected)
                SetStatus("Connected ✓", Color.green);
            else
                SetStatus("Not connected ✗", Color.red);
        });
    }

    // ── Button callbacks (wire in Inspector) ──────────────────────────────────

    public void OnCheckHealth()
    {
        Log("Checking API health...");
        DatabaseConnector.Instance.CheckHealth(ok =>
        {
            if (ok) { SetStatus("Connected ✓", Color.green); Log("✓ API is reachable."); }
            else    { SetStatus("Unreachable ✗", Color.red); Log("✗ API not reachable."); }
        });
    }

    public void OnGetPlayerProfile()
    {
        if (!TryGetInt(playerIdField, "Player ID", out int id)) return;

        Log($"Fetching profile for player {id}...");
        DatabaseConnector.Instance.GetPlayerProfile(id,
            profile => Log(
                $"✓ Profile:\n" +
                $"  Name:    {profile.display_name}\n" +
                $"  Class:   {profile.class_name}\n" +
                $"  Level:   {profile.level}\n" +
                $"  Rating:  {profile.rating}\n" +
                $"  K/D:     {profile.kills}/{profile.deaths}\n" +
                $"  Gold:    {profile.gold}"),
            error => Log($"✗ {error}"));
    }

    public void OnGetLeaderboard()
    {
        if (!TryGetInt(limitField, "Limit", out int limit)) limit = 10;

        Log($"Fetching top {limit} leaderboard entries...");
        DatabaseConnector.Instance.GetLeaderboard(limit,
            entries =>
            {
                var sb = new System.Text.StringBuilder("✓ Leaderboard:\n");
                int rank = 1;
                foreach (var e in entries)
                    sb.AppendLine($"  {rank++}. {e.display_name}  Rating:{e.rating}  Lv:{e.level}");
                Log(sb.ToString());
            },
            error => Log($"✗ {error}"));
    }

    public void OnCreateAccount()
    {
        string account = accountNameField != null ? accountNameField.text : "";
        string pass    = passwordField    != null ? passwordField.text    : "";
        string display = displayNameField != null ? displayNameField.text : "";

        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(pass))
        {
            Log("✗ Account name and password are required."); return;
        }

        Log($"Creating account '{account}'...");
        DatabaseConnector.Instance.CreateAccount(account, pass, display,
            profile => Log($"✓ Account created! ID: {profile.player_id}, Name: {profile.display_name}"),
            error => Log($"✗ {error}"));
    }

    public void OnLogin()
    {
        string account = accountNameField != null ? accountNameField.text : "";
        string pass    = passwordField    != null ? passwordField.text    : "";

        if (string.IsNullOrWhiteSpace(account)) { Log("✗ Account name required."); return; }

        Log($"Logging in as '{account}'...");
        DatabaseConnector.Instance.Login(account, pass,
            profile => Log($"✓ Logged in! Welcome {profile.display_name} (Level {profile.level})"),
            error => Log($"✗ {error}"));
    }

    public void OnUpdateStats()
    {
        if (!TryGetInt(playerIdField, "Player ID", out int id)) return;
        TryGetInt(killsField,  "Kills",  out int kills);
        TryGetInt(deathsField, "Deaths", out int deaths);
        TryGetInt(scoreField,  "Score",  out int score);

        Log($"Updating stats for player {id}: K{kills} D{deaths} S{score}...");
        DatabaseConnector.Instance.UpdatePlayerStats(id, kills, deaths, score,
            res => Log($"✓ Stats updated: {res.message}"),
            error => Log($"✗ {error}"));
    }

    public void OnClearLog()
    {
        if (logText != null) logText.text = "";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void Log(string msg)
    {
        Debug.Log($"[DBTester] {msg}");
        if (logText == null) return;

        // Keep at most 40 lines
        var lines = logText.text.Split('\n');
        if (lines.Length > 40)
            logText.text = string.Join("\n", lines, lines.Length - 40, 40);

        logText.text += (logText.text.Length > 0 ? "\n" : "") + msg;
    }

    void SetStatus(string msg, Color color)
    {
        if (statusText    != null) statusText.text = msg;
        if (statusIndicator != null) statusIndicator.color = color;
        Debug.Log($"[DBTester] Status: {msg}");
    }

    bool TryGetInt(InputField field, string label, out int value)
    {
        value = 0;
        if (field == null || string.IsNullOrWhiteSpace(field.text))
        {
            Log($"✗ {label} field is empty."); return false;
        }
        if (!int.TryParse(field.text, out value))
        {
            Log($"✗ {label} must be a number."); return false;
        }
        return true;
    }
}
