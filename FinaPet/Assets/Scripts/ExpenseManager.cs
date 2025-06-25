using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TMPro;
using UnityEngine.SceneManagement; // This line is crucial for SceneManager to be recognized
using System.Collections.Generic;

public class ExpenseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField expenseInputField;
    [SerializeField] private Button submitButton;

    [Header("Confirmation Popup UI")]
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private TextMeshProUGUI confirmSubmitText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Status Panel UI")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button goBackButton;

    [Header("Player and Server Config")]
    public int playerId = 1;
    private const string LAST_SUBMISSION_DATE_KEY = "LastExpenseSubmissionDate";

    private float _fetchedMonthlyIncome = 0f;
    private int _potentialCoinsEarned = 0;
    private bool _hasLoggedPreviousDayFromDB = false;

    void Start()
    {
        confirmationPopup.SetActive(false);
        statusPanel.SetActive(false);

        if (PlayerAuthSession.IsLoggedIn)
        {
            playerId = PlayerAuthSession.PlayerId;
        }
        else
        {
            Debug.LogWarning("Player not logged in, using default ID: " + playerId);
        }

        submitButton.onClick.AddListener(OnSubmitClicked);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        goBackButton.onClick.AddListener(GoToPreviousScene);

        CheckForDailySubmission();
    }

    private void CheckForDailySubmission()
    {
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        string lastSubmissionDate = PlayerPrefs.GetString(LAST_SUBMISSION_DATE_KEY, "");

        if (lastSubmissionDate == currentDate)
        {
            DisableExpensePage("You have already logged your spending for today.");
        }
    }

    private void OnSubmitClicked()
    {
        if (string.IsNullOrEmpty(expenseInputField.text) || !float.TryParse(expenseInputField.text, out _))
        {
            Debug.LogError("Invalid input. Please enter a numerical value.");
            SetStatusText("Please enter a valid spending amount.", false);
            return;
        }

        StartCoroutine(Co_PreCalculateRewardsAndShowConfirmation());
    }

    private IEnumerator Co_PreCalculateRewardsAndShowConfirmation()
    {
        submitButton.interactable = false;

        float dailySpending;
        if (!float.TryParse(expenseInputField.text, out dailySpending))
        {
            SetStatusText("Invalid spending amount entered.", false);
            submitButton.interactable = true;
            yield break;
        }

        SetStatusText("Loading...", true);

        yield return Co_FetchMonthlyIncome();
        yield return Co_CheckPreviousDaySpendingFromDB();

        _potentialCoinsEarned = CalculateCoinsEarned(dailySpending, _fetchedMonthlyIncome, _hasLoggedPreviousDayFromDB);

        if (confirmSubmitText != null)
        {
            confirmSubmitText.text = $"Are you sure you want to log spending of ${dailySpending:F2}? You will earn {_potentialCoinsEarned} coins.";
        }

        confirmationPopup.SetActive(true);
        statusPanel.SetActive(false);
        submitButton.interactable = true;
    }

    private IEnumerator Co_FetchMonthlyIncome()
    {
        _fetchedMonthlyIncome = 0f;

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("ExpenseManager: API path is null or empty. Cannot fetch tracker settings for budget.");
            yield break;
        }

        string getTrackerUrl = apiPath + "/get_player_tracker_setting.php";
        var trackerRequestData = new PlayerIdRequest { player_id = playerId };
        string trackerJson = JsonUtility.ToJson(trackerRequestData);

        using (UnityWebRequest request = new UnityWebRequest(getTrackerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(trackerJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GetPlayerTrackerSettingResponse>(request.downloadHandler.text);
                    if (response.status_code == 0 && response.player_tracker_setting != null && response.player_tracker_setting.Count > 0)
                    {
                        _fetchedMonthlyIncome = response.player_tracker_setting[0].monthly_income;
                        Debug.Log($"[Co_FetchMonthlyIncome] Fetched monthly income for pre-calculation: {_fetchedMonthlyIncome}");
                    }
                    else
                    {
                        Debug.LogWarning("[Co_FetchMonthlyIncome] Failed to get monthly income for budget calculation during pre-check. Defaulting to 0.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Co_FetchMonthlyIncome] Failed to parse monthly income response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[Co_FetchMonthlyIncome] Failed to fetch monthly income for pre-calculation: {request.error}");
            }
        }
    }

    private IEnumerator Co_CheckPreviousDaySpendingFromDB()
    {
        _hasLoggedPreviousDayFromDB = false;

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("ExpenseManager: API path is null or empty. Cannot fetch spending records from DB.");
            yield break;
        }

        string url = apiPath + "/get_player_spending.php";
        var requestData = new PlayerIdRequest { player_id = playerId };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"[Co_CheckPreviousDaySpendingFromDB] Sending request to {url} with body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Co_CheckPreviousDaySpendingFromDB] Request failed: {request.error}");
            }
            else
            {
                Debug.Log($"[Co_CheckPreviousDaySpendingFromDB] Response received: {request.downloadHandler.text}");
                try
                {
                    var response = JsonUtility.FromJson<GetPlayerSpendingResponse>(request.downloadHandler.text);
                    if (response.status_code == 0 && response.spending_records != null)
                    {
                        DateTime yesterday = DateTime.Now.AddDays(-1).Date;
                        Debug.Log($"[Co_CheckPreviousDaySpendingFromDB] Checking for records on: {yesterday.ToString("yyyy-MM-dd")}");
                        foreach (var record in response.spending_records)
                        {
                            if (DateTime.TryParse(record.last_updated, out DateTime parsedRecordDate) && parsedRecordDate.Date == yesterday)
                            {
                                _hasLoggedPreviousDayFromDB = true;
                                Debug.Log($"[Co_CheckPreviousDaySpendingFromDB] Found spending record for yesterday: {yesterday.ToString("yyyy-MM-dd")} (Record: {record.last_updated})");
                                break;
                            }
                        }
                        if (!_hasLoggedPreviousDayFromDB)
                        {
                            Debug.Log($"[Co_CheckPreviousDaySpendingFromDB] No spending record found for yesterday: {yesterday.ToString("yyyy-MM-dd")} in the database.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Co_CheckPreviousDaySpendingFromDB] Failed to get valid spending data. Status: {response.status_code}, Message: {response.error_message}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Co_CheckPreviousDaySpendingFromDB] Failed to parse JSON response. Error: {e.Message}");
                }
            }
        }
    }

    private int CalculateCoinsEarned(float dailySpending, float monthlyIncome, bool hasLoggedPreviousDay)
    {
        int coins = 0;
        coins += 5;
        Debug.Log("Rule 1: +5 coins for logging spending.");

        if (monthlyIncome > 0)
        {
            float dailyBudget = (monthlyIncome * 0.7f) / 30f;
            Debug.Log($"Budget check: Daily Budget={dailyBudget:F2}, Daily Spending={dailySpending:F2}");
            if (dailySpending <= dailyBudget)
            {
                coins += 5;
                Debug.Log("Rule 3: +5 coins for staying within budget.");
            }
            else
            {
                Debug.Log("Rule 3: No coins for exceeding budget.");
            }
        }
        else
        {
            Debug.LogWarning("Monthly income is zero or not fetched, cannot check budget adherence.");
        }

        if (hasLoggedPreviousDay)
        {
            coins += 5;
            Debug.Log("Rule 2: +5 coins for logging spending yesterday (from DB check).");
        }
        else
        {
            Debug.Log("Rule 2: No coins for not logging yesterday (no record found in DB).");
        }

        return Mathf.Min(coins, 15);
    }

    private void OnNoClicked()
    {
        confirmationPopup.SetActive(false);
    }

    private void OnYesClicked()
    {
        confirmationPopup.SetActive(false);
        StartCoroutine(Co_SendExpenseDataAndApplyRewards());
    }

    private IEnumerator Co_SendExpenseDataAndApplyRewards()
    {
        float dailySpending;
        if (!float.TryParse(expenseInputField.text, out dailySpending))
        {
            SetStatusText("Invalid spending amount.", false);
            yield break;
        }

        yield return Co_SendExpenseData(dailySpending);

        if (statusText.text != "Submission failed!" && !statusText.text.Contains("error"))
        {
            PlayerPrefs.SetString(LAST_SUBMISSION_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            Debug.Log($"PlayerPrefs updated: {LAST_SUBMISSION_DATE_KEY} set to {DateTime.Now.ToString("yyyy-MM-dd")}");

            int coinsToAward = _potentialCoinsEarned;

            if (!PlayerDataManager.IsDataLoaded)
            {
                Debug.Log("PlayerDataManager not loaded, fetching player data first for reward application.");
                yield return PlayerDataManager.FetchPlayerData(playerId);
            }

            if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
            {
                int currentCoins = PlayerDataManager.CurrentPlayerMainData.coin;
                int avatarSpriteId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;
                int newTotalCoins = currentCoins + coinsToAward;

                Debug.Log($"Updating player coins: {currentCoins} -> {newTotalCoins} (awarding {coinsToAward})");
                yield return PlayerDataManager.UpdatePlayerDataOnServer(playerId, newTotalCoins, avatarSpriteId);

                if (PlayerDataManager.IsUpdateSuccessful)
                {
                    SetStatusText($"Submission successful! Earned {coinsToAward} coins. Total coins: {newTotalCoins}.", true);
                    DisableExpensePage("Submission successful! Rewards applied.");
                }
                else
                {
                    SetStatusText($"Submission successful, but failed to update coins: {PlayerDataManager.LastUpdateMessage}", false);
                    Debug.LogError($"Failed to update player coins: {PlayerDataManager.LastUpdateMessage}");
                    DisableExpensePage("Submission successful, but failed to apply rewards.");
                }
            }
            else
            {
                SetStatusText("Could not update coins: Player data not available.", false);
                Debug.LogError("PlayerDataManager was not loaded or CurrentPlayerMainData is null after fetch attempt.");
                DisableExpensePage("Submission successful, but player data for rewards could not be loaded.");
            }
        }
        else
        {
            DisableExpensePage("Submission failed (see above).");
        }
    }

    private IEnumerator Co_SendExpenseData(float dailySpending)
    {
        var requestData = new DailyExpenseRequestData { player_id = this.playerId, daily_spending = dailySpending };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath() + "/insert_player_daily_tracker.php";

        SetStatusText("Submitting spending data...", true);

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Call Failed! Error: {request.error}");
                SetStatusText($"Submission failed! Network error: {request.error}", false);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                    if (response.status_code == "200" || response.status_code == "0")
                    {
                        Debug.Log("Daily spending submitted successfully to server.");
                    }
                    else
                    {
                        Debug.LogError("Server returned an error for spending submission: " + response.message);
                        SetStatusText($"Submission failed: {response.message}", false);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response for spending submission. Error: {e.Message}");
                    SetStatusText($"Submission failed: Data parse error. {e.Message}", false);
                }
            }
        }
    }

    private void DisableExpensePage(string message)
    {
        expenseInputField.text = "";
        expenseInputField.interactable = false;
        submitButton.interactable = false;

        if (statusPanel != null && statusText != null)
        {
            statusText.text = message;
            statusPanel.SetActive(true);
        }
    }

    private void SetStatusText(string message, bool isSuccess)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isSuccess ? Color.green : Color.red;
        }
    }

    public void GoToPreviousScene()
    {
        SceneManager.LoadScene("DiaryPage");
    }

    void OnDestroy()
    {
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
        if (yesButton != null) yesButton.onClick.RemoveAllListeners();
        if (noButton != null) noButton.onClick.RemoveAllListeners();
        if (goBackButton != null) goBackButton.onClick.RemoveAllListeners();
    }

    #region Data Transfer Classes
    [System.Serializable]
    private class PlayerIdRequest
    {
        public int player_id;
    }

    [System.Serializable]
    private class GetPlayerTrackerSettingResponse
    {
        public int status_code;
        public string error_message;
        public List<PlayerTrackerSetting> player_tracker_setting;
    }
    [System.Serializable]
    private class PlayerTrackerSetting
    {
        public float monthly_income;
    }

    [System.Serializable]
    private class DailyExpenseRequestData { public int player_id; public float daily_spending; }
    [System.Serializable]
    private class ServerResponse { public string status_code; public string message; }

    [System.Serializable]
    private class GetPlayerSpendingResponse
    {
        public int status_code;
        public string error_message;
        public List<SpendingRecord> spending_records;
    }
    [System.Serializable]
    private class SpendingRecord
    {
        public float daily_spending;
        public string last_updated;
    }
    #endregion
}