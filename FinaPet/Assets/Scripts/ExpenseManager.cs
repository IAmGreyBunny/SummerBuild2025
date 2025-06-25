using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

using TMPro; // Make sure this is using TMPro if your text is TextMeshPro
using UnityEngine.SceneManagement; // Required for loading scenes
using System.Collections.Generic; // Required for List

public class ExpenseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField expenseInputField; // Using TMP_InputField
    [SerializeField] private Button submitButton;

    [Header("Confirmation Popup UI")]
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private TextMeshProUGUI confirmSubmitText; // Assign this TextMeshProUGUI in the Inspector
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Status Panel UI")]
    [SerializeField] private GameObject statusPanel; // The parent panel
    [SerializeField] private TextMeshProUGUI statusText; // The text inside the panel
    [SerializeField] private Button goBackButton; // The new button

    [Header("Player and Server Config")]
    public int playerId = 1;
    private const string LAST_SUBMISSION_DATE_KEY = "LastExpenseSubmissionDate";

    // Internal state for calculations
    private float _fetchedMonthlyIncome = 0f;
    private int _potentialCoinsEarned = 0;
    private string _previousSubmissionDate = ""; // Stores the date from PlayerPrefs before this session's submission

    void Start()
    {
        // --- This is the core logic that runs every time the scene loads ---
        string lastSubmissionDate = PlayerPrefs.GetString(LAST_SUBMISSION_DATE_KEY, "");
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastSubmissionDate == currentDate)
        {
            // If we already submitted today, go straight to Status Mode.
            SetUIMode(false, "Return to Diary");
        }
        else
        {
            // Otherwise, it's a new day. Go to Input Mode.
            SetUIMode(true);
        }
        // --- End of core logic ---

        // Hide the confirmation popup on start
        confirmationPopup.SetActive(false);

        statusPanel.SetActive(false);

        // Determine player ID based on login status
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

        // Check if a submission was already made today
        CheckForDailySubmission();
    }

    /// <summary>
    /// Checks if a daily spending submission has already been made today and disables the page if so.
    /// </summary>
    private void CheckForDailySubmission()
    {
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        string lastSubmissionDate = PlayerPrefs.GetString(LAST_SUBMISSION_DATE_KEY, "");

        if (lastSubmissionDate == currentDate)
        {
            DisableExpensePage("You have already logged your spending for today.");
        }
    }

    /// <summary>
    /// Called when the submit button is clicked. Validates input, fetches data, and shows confirmation popup.
    /// </summary>
    private void OnSubmitClicked()
    {
        if (string.IsNullOrEmpty(expenseInputField.text) || !float.TryParse(expenseInputField.text, out _))
        {
            Debug.LogError("Invalid input. Please enter a numerical value.");
            SetStatusText("Please enter a valid spending amount.", false);
            return;
        }

        // Store the previous submission date from PlayerPrefs right before potentially logging a new one.
        _previousSubmissionDate = PlayerPrefs.GetString(LAST_SUBMISSION_DATE_KEY, "");

        // Start coroutine to pre-calculate rewards and show confirmation
        StartCoroutine(Co_PreCalculateRewardsAndShowConfirmation());
    }

    /// <summary>
    /// Coroutine to fetch necessary data, pre-calculate rewards, and then display the confirmation popup.
    /// </summary>
    private IEnumerator Co_PreCalculateRewardsAndShowConfirmation()
    {
        // Temporarily disable buttons to prevent double-clicks while fetching data
        submitButton.interactable = false;

        float dailySpending;
        if (!float.TryParse(expenseInputField.text, out dailySpending))
        {
            SetStatusText("Invalid spending amount entered.", false);
            submitButton.interactable = true; // Re-enable if parsing fails
            yield break;
        }

        SetStatusText("Loading...", true); // Indicate loading

        // Fetch monthly income before calculating potential rewards
        yield return Co_FetchMonthlyIncome();

        // Calculate potential coins
        _potentialCoinsEarned = CalculateCoinsEarned(dailySpending, _fetchedMonthlyIncome, _previousSubmissionDate);

        // Update the confirmation text
        if (confirmSubmitText != null)
        {
            confirmSubmitText.text = $"Are you sure you want to log spending of ${dailySpending:F2}? You will earn {_potentialCoinsEarned} coins.";
        }

        // Show the confirmation popup
        confirmationPopup.SetActive(true);
        statusPanel.SetActive(false); // Hide status panel if it was showing
        submitButton.interactable = true; // Re-enable submit button
    }

    /// <summary>
    /// Coroutine to fetch the player's monthly income from the server.
    /// Stores the result in _fetchedMonthlyIncome.
    /// </summary>
    private IEnumerator Co_FetchMonthlyIncome()
    {
        _fetchedMonthlyIncome = 0f; // Reset before fetching

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
                        Debug.Log($"Fetched monthly income for pre-calculation: {_fetchedMonthlyIncome}");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to get monthly income for budget calculation during pre-check. Defaulting to 0.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse monthly income response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch monthly income for pre-calculation: {request.error}");
            }
        }
    }

    /// <summary>
    /// Pure function to calculate the number of coins earned based on rules.
    /// </summary>
    /// <param name="dailySpending">The amount spent today.</param>
    /// <param name="monthlyIncome">The player's monthly income.</param>
    /// <param name="previousSubmissionDateStr">The date string of the last successful submission.</param>
    /// <returns>Total coins to be earned, capped at 15.</returns>
    private int CalculateCoinsEarned(float dailySpending, float monthlyIncome, string previousSubmissionDateStr)
    {
        int coins = 0;
        // Rule 1: 5 coins for logging daily spending
        coins += 5;
        Debug.Log("Rule 1: +5 coins for logging spending.");

        // Rule 3: 5 coins if you stayed within budget (70% of allowance/30 days)
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

        // Rule 2: 5 coins if you logged in your spending the previous day
        DateTime yesterday = DateTime.Now.AddDays(-1).Date;
        if (DateTime.TryParse(previousSubmissionDateStr, out DateTime parsedPrevDate) && parsedPrevDate.Date == yesterday)
        {
            coins += 5;
            Debug.Log("Rule 2: +5 coins for logging spending yesterday.");
        }
        else
        {
            Debug.Log("Rule 2: No coins for not logging yesterday.");
        }

        // Cap rewards at 15 coins
        return Mathf.Min(coins, 15);
    }

    /// <summary>
    /// Called when the 'No' button in the confirmation popup is clicked. Hides the popup.
    /// </summary>
    private void OnNoClicked()
    {
        confirmationPopup.SetActive(false);
    }

    /// <summary>
    /// Called when the 'Yes' button in the confirmation popup is clicked. Starts the data submission and reward process.
    /// </summary>
    private void OnYesClicked()
    {
        confirmationPopup.SetActive(false);
        StartCoroutine(Co_SendExpenseDataAndApplyRewards());
    }

    /// <summary>
    /// Coroutine to send expense data to the server and then apply rewards based on pre-calculated value.
    /// </summary>
    private IEnumerator Co_SendExpenseDataAndApplyRewards()
    {
        float dailySpending;
        if (!float.TryParse(expenseInputField.text, out dailySpending))
        {
            SetStatusText("Invalid spending amount.", false);
            yield break;
        }

        // First, send the expense data to the server
        yield return Co_SendExpenseData(dailySpending);

        // After sending expense data, proceed to apply rewards if submission was successful.
        // We check the statusText message or a boolean flag from Co_SendExpenseData if it was available.
        // For simplicity here, we assume if Co_SendExpenseData completes without setting a hard error, it was OK.
        // A more robust system would have Co_SendExpenseData return a success bool.
        // For now, we rely on the internal state after Co_SendExpenseData has run.

        // If the status is not already an error from Co_SendExpenseData, proceed to apply coins.
        if (statusText.text != "Submission failed!") // Check if a failure message was set
        {
            // The _potentialCoinsEarned should already be calculated from Co_PreCalculateRewardsAndShowConfirmation
            // We re-use this value to ensure consistency with what was shown to the user.
            int coinsToAward = _potentialCoinsEarned;

            // Update player's total coins using PlayerDataManager
            // Ensure PlayerDataManager has current player data before attempting to update.
            if (!PlayerDataManager.IsDataLoaded)
            {
                Debug.Log("PlayerDataManager not loaded, fetching player data first for reward application.");
                yield return PlayerDataManager.FetchPlayerData(playerId);
            }

            if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
            {
                int currentCoins = PlayerDataManager.CurrentPlayerMainData.coin;
                int newTotalCoins = currentCoins + coinsToAward;
                int avatarSpriteId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;

                Debug.Log($"Updating player coins: {currentCoins} -> {newTotalCoins} (awarding {coinsToAward})");
                yield return PlayerDataManager.UpdatePlayerDataOnServer(playerId, newTotalCoins, avatarSpriteId);

                if (PlayerDataManager.IsUpdateSuccessful)
                {
                    // Update PlayerPrefs for the *next* day's check after all operations are successful.
                    PlayerPrefs.SetString(LAST_SUBMISSION_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
                    PlayerPrefs.Save();

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
            // If Co_SendExpenseData already failed, its error message is already shown.
            // Just disable the page.
            DisableExpensePage("Submission failed (see above).");
        }
    }

    /// <summary>
    /// Coroutine to send the daily expense data to the server.
    /// This method is now focused solely on sending the expense data.
    /// </summary>
    private IEnumerator Co_SendExpenseData(float dailySpending)
    {
        var requestData = new DailyExpenseRequestData { player_id = this.playerId, daily_spending = dailySpending };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/insert_player_daily_tracker.php";

        SetStatusText("Submitting spending data...", true); // Inform user

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
                        // Successfully submitted spending, but don't set final message yet, rewards are next.
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

    /// <summary>
    /// Disables the expense input page and displays a status message.
    /// </summary>
    /// <param name="message">The message to display to the user.</param>
    private void DisableExpensePage(string message)
    {
        expenseInputField.text = "";
        expenseInputField.interactable = false;
        submitButton.interactable = false;

        if (statusPanel != null && statusText != null)
        {
            addExpensePage.SetActive(false);
            statusPanel.SetActive(true);
            statusText.text = message;
        }
    }


    /// <summary>
    /// Helper method to set the status text and its color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="isSuccess">If true, sets text color to green; otherwise, sets to red.</param>
    private void SetStatusText(string message, bool isSuccess)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isSuccess ? Color.green : Color.red;
        }
    }

    /// <summary>
    /// Navigates back to the previous scene (DiaryPage in this case).
    /// </summary>
    public void GoToPreviousScene()
    {
        SceneManager.LoadScene("DiaryPage");
    }

    void OnDestroy()
    {
        // Remove all listeners to prevent memory leaks
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
        if (yesButton != null) yesButton.onClick.RemoveAllListeners();
        if (noButton != null) noButton.onClick.RemoveAllListeners();
        if (goBackButton != null) goBackButton.onClick.RemoveAllListeners();
    }

    #region Data Transfer Classes
    // Data Transfer Objects for communication with the server
    [System.Serializable]
    private class DailyExpenseRequestData { public int player_id; public float daily_spending; }
    [System.Serializable]
    private class ServerResponse { public string status_code; public string message; }

    // DTOs for fetching tracker settings (adapted from DiaryDisplayManager)
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
    #endregion
}