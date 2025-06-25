using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for Sum() and other LINQ operations
using System.Text;
using TMPro;

/// <summary>
/// Manages the display of financial data on the Diary/Summary page.
/// It fetches tracker settings and all spending records, then calculates
/// and displays various budget and saving metrics.
/// </summary>
public class DiaryDisplayManager : MonoBehaviour
{
    [Header("UI Text References")]
    [Tooltip("Text to display the calculated daily budget.")]
    public TextMeshProUGUI dailyBudgetText;
    [Tooltip("Text to display the calculated monthly budget.")]
    public TextMeshProUGUI monthlyBudgetText;
    [Tooltip("Text to display the calculated daily savings.")]
    public TextMeshProUGUI dailySavingsText;
    [Tooltip("Text to display the calculated monthly savings.")]
    public TextMeshProUGUI monthlySavingsText;
    [Tooltip("Text to display the total spending for the current month.")]
    public TextMeshProUGUI totalSpendingText;
    [Tooltip("Text to display the total savings for the current month.")]
    public TextMeshProUGUI totalSavingsText;

    [Header("Debug Settings")]
    [Tooltip("The player ID to use for testing when not logged in.")]
    public int debugPlayerId = 1;

    private int _playerId;
    private float _monthlyIncome = 0f;
    private List<SpendingRecord> _spendingRecords = new List<SpendingRecord>();

    void Start()
    {
        // Determine the active player ID
        _playerId = PlayerAuthSession.IsLoggedIn ? PlayerAuthSession.PlayerId : debugPlayerId;

        // Start the master coroutine to fetch all necessary data.
        StartCoroutine(FetchAllDiaryData());
    }

    /// <summary>
    /// Master coroutine that orchestrates the fetching of all data needed for the diary page.
    /// </summary>
    private IEnumerator FetchAllDiaryData()
    {
        Debug.Log("--- Starting Diary Data Fetch ---");
        // First, fetch the tracker settings to get the player's income.
        yield return StartCoroutine(FetchTrackerSettings());

        // Second, fetch all the player's spending records.
        yield return StartCoroutine(FetchSpendingRecords());

        // Finally, after all data is fetched, perform the calculations and update the UI.
        Debug.Log("--- All data fetched. Performing calculations. ---");
        CalculateAndDisplayData();
    }

    /// <summary>
    /// Fetches the player's income from the tracker settings.
    /// </summary>
    private IEnumerator FetchTrackerSettings()
    {
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("DiaryDisplayManager: API path is null or empty. Cannot fetch tracker settings.");
            yield break;
        }

        string url = apiPath + "/get_player_tracker_setting.php";

        // FIX: Use a serializable class instead of an anonymous type for the request.
        var requestData = new PlayerIdRequest { player_id = _playerId };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"[FetchTrackerSettings] Sending request to {url} with body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FetchTrackerSettings] Request failed: {request.error}");
            }
            else
            {
                Debug.Log($"[FetchTrackerSettings] Response received: {request.downloadHandler.text}");
                var response = JsonUtility.FromJson<GetPlayerTrackerSettingResponse>(request.downloadHandler.text);
                if (response.status_code == 0 && response.player_tracker_setting.Count > 0)
                {
                    _monthlyIncome = response.player_tracker_setting[0].monthly_income;
                    Debug.Log($"[FetchTrackerSettings] Monthly income parsed successfully: {_monthlyIncome}");
                }
                else
                {
                    Debug.LogWarning($"[FetchTrackerSettings] Failed to get valid tracker data. Status: {response.status_code}, Message: {response.error_message}");
                }
            }
        }
    }

    /// <summary>
    /// Fetches all spending records for the player from the new PHP script.
    /// </summary>
    private IEnumerator FetchSpendingRecords()
    {
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("DiaryDisplayManager: API path is null or empty. Cannot fetch spending records.");
            yield break;
        }

        string url = apiPath + "/get_player_spending.php";

        // FIX: Use the same serializable class for the request.
        var requestData = new PlayerIdRequest { player_id = _playerId };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"[FetchSpendingRecords] Sending request to {url} with body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FetchSpendingRecords] Request failed: {request.error}");
            }
            else
            {
                Debug.Log($"[FetchSpendingRecords] Response received: {request.downloadHandler.text}");
                var response = JsonUtility.FromJson<GetPlayerSpendingResponse>(request.downloadHandler.text);
                if (response.status_code == 0 && response.spending_records != null)
                {
                    _spendingRecords = response.spending_records;
                    Debug.Log($"[FetchSpendingRecords] Parsed {_spendingRecords.Count} spending records.");
                }
                else
                {
                    Debug.LogWarning($"[FetchSpendingRecords] Failed to get valid spending data. Status: {response.status_code}, Message: {response.error_message}");
                }
            }
        }
    }

    /// <summary>
    /// Performs all calculations and updates the six UI text elements.
    /// </summary>
    private void CalculateAndDisplayData()
    {
        // --- Budget Calculations ---
        float monthlyBudget = _monthlyIncome * 0.7f;
        float dailyBudget = monthlyBudget / 30f;

        Debug.Log($"Calculating with Monthly Income: {_monthlyIncome}. Monthly Budget: {monthlyBudget}, Daily Budget: {dailyBudget}");

        monthlyBudgetText.text = $"${monthlyBudget:F2}";
        dailyBudgetText.text = $"${dailyBudget:F2}";

        // --- Spending & Savings Calculations ---
        DateTime today = DateTime.Today;

        SpendingRecord todaySpendingRecord = _spendingRecords.Find(r => DateTime.Parse(r.record_date).Date == today);

        List<SpendingRecord> thisMonthRecords = _spendingRecords.Where(r => {
            DateTime recordDate = DateTime.Parse(r.record_date);
            return recordDate.Year == today.Year && recordDate.Month == today.Month;
        }).ToList();

        float totalMonthlySpending = thisMonthRecords.Sum(r => r.daily_spending);
        Debug.Log($"Found {(todaySpendingRecord != null ? "a" : "no")} spending record for today. Found {thisMonthRecords.Count} records for this month, totaling: {totalMonthlySpending}");

        // --- Update UI Text ---
        // Daily Savings
        if (todaySpendingRecord != null)
        {
            float dailyAllowance = _monthlyIncome / 30f;
            float dailySavings = dailyAllowance - todaySpendingRecord.daily_spending;
            dailySavingsText.text = $"${dailySavings:F2}";
            Debug.Log($"Daily Savings calculated: {dailyAllowance} - {todaySpendingRecord.daily_spending} = {dailySavings}");
        }
        else
        {
            dailySavingsText.text = "N/A";
        }

        // Monthly Savings
        float monthlySavings = _monthlyIncome - totalMonthlySpending;
        monthlySavingsText.text = $"${monthlySavings:F2}";

        // Total Spending (This Month)
        totalSpendingText.text = $"${totalMonthlySpending:F2}";

        // Total Savings (This Month)
        totalSavingsText.text = $"${monthlySavings:F2}";

        Debug.Log("--- UI Update Complete ---");
    }

    #region Data Transfer Classes
    // --- DTO for sending a player_id request ---
    [Serializable]
    private class PlayerIdRequest
    {
        public int player_id;
    }

    // --- DTOs for get_player_tracker_setting.php ---
    [Serializable]
    private class GetPlayerTrackerSettingResponse
    {
        public int status_code;
        public string error_message; // Added for better error logging
        public List<PlayerTrackerSetting> player_tracker_setting;
    }
    [Serializable]
    private class PlayerTrackerSetting
    {
        public float monthly_income;
    }

    // --- DTOs for the new get_player_spending.php ---
    [Serializable]
    private class GetPlayerSpendingResponse
    {
        public int status_code;
        public string error_message; // Added for better error logging
        public List<SpendingRecord> spending_records;
    }
    [Serializable]
    private class SpendingRecord
    {
        public float daily_spending;
        public string record_date; // Keep as string for parsing
    }
    #endregion
}
