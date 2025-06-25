using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;

/// <summary>
/// Fetches player tracker settings from the database, calculates a spending
/// suggestion (70% of monthly income), and displays it in a UI text element.
/// </summary>
public class PlayerTrackerDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshProUGUI element where the spending suggestion will be displayed.")]
    public TextMeshProUGUI spendingSuggestionText;

    [Header("Debug Settings")]
    [Tooltip("The player ID to use for testing when not logged in.")]
    public int debugPlayerId = 1;

    private int _playerId;

    void Start()
    {
        // 1. Initial Sanity Check
        if (spendingSuggestionText == null)
        {
            Debug.LogError("PlayerTrackerDisplay: The 'spendingSuggestionText' field is not assigned in the Inspector!");
            return;
        }

        // 2. Determine Player ID
        // Use the logged-in player's ID if available, otherwise use the debug ID.
        if (PlayerAuthSession.IsLoggedIn)
        {
            _playerId = PlayerAuthSession.PlayerId;
            Debug.Log($"PlayerTrackerDisplay: Player is logged in. Using Player ID: {_playerId}");
        }
        else
        {
            _playerId = debugPlayerId;
            Debug.LogWarning($"PlayerTrackerDisplay: Player not logged in. Using Debug ID: {_playerId}");
        }

        // 3. Start the Data Fetching Process
        StartCoroutine(FetchTrackerSettings());
    }

    /// <summary>
    /// Coroutine to fetch the player tracker settings from the server.
    /// </summary>
    private IEnumerator FetchTrackerSettings()
    {
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("PlayerTrackerDisplay: Could not load server configuration.");
            spendingSuggestionText.text = "Error";
            yield break;
        }

        string url = apiPath + "/get_player_tracker_setting.php";
        var requestData = new GetPlayerTrackerSettingRequest { player_id = _playerId };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"PlayerTrackerDisplay: Sending request to {url} with body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PlayerTrackerDisplay: Failed to fetch tracker settings. Error: {request.error}");
                spendingSuggestionText.text = "Error";
            }
            else
            {
                Debug.Log($"PlayerTrackerDisplay: Received response: {request.downloadHandler.text}");
                var response = JsonUtility.FromJson<GetPlayerTrackerSettingResponse>(request.downloadHandler.text);

                // Check if the request was successful and if any settings were returned.
                if (response.status_code == 0 && response.player_tracker_setting != null && response.player_tracker_setting.Count > 0)
                {
                    // Use the first record returned.
                    PlayerTrackerSetting settings = response.player_tracker_setting[0];

                    // Calculate 70% of the monthly income.
                    float spendingSuggestion = settings.monthly_income * 0.7f;

                    // Format the result as currency and update the UI text.
                    // "C2" formats the number as a currency string with 2 decimal places.
                    spendingSuggestionText.text = $"${spendingSuggestion.ToString("F2")}";
                }
                else
                {
                    Debug.LogWarning("PlayerTrackerDisplay: No tracker settings found for this player or an error occurred.");
                    spendingSuggestionText.text = "No income data found.";
                }
            }
        }
    }

    #region Data Transfer Classes
    // --- Helper classes for JSON serialization ---

    [Serializable]
    private class GetPlayerTrackerSettingRequest
    {
        public int player_id;
    }

    [Serializable]
    private class GetPlayerTrackerSettingResponse
    {
        public int status_code;
        public string error_message;
        public List<PlayerTrackerSetting> player_tracker_setting;
    }

    [Serializable]
    private class PlayerTrackerSetting
    {
        public int tracker_id;
        public int player_id;
        // The income is a number but can be parsed from a string if needed.
        // For robustness, float is used.
        public float monthly_income;
    }
    #endregion
}
