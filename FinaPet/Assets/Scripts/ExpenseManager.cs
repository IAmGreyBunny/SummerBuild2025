using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TMPro; // Make sure this is using TMPro if your text is TextMeshPro
using UnityEngine.SceneManagement; // NEW! Required for loading scenes

public class ExpenseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField expenseInputField; // Using TMP_InputField
    [SerializeField] private Button submitButton;
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Status Panel UI")] // NEW! Section for the new panel
    [SerializeField] private GameObject statusPanel; // The parent panel
    [SerializeField] private TextMeshProUGUI statusText; // The text inside the panel
    [SerializeField] private Button goBackButton; // The new button

    [Header("Player and Server Config")]
    public int playerId = 1; // Default player ID
    private const string LAST_SUBMISSION_DATE_KEY = "LastExpenseSubmissionDate";

    void Start()
    {
        // Hide popups on start
        confirmationPopup.SetActive(false);
        statusPanel.SetActive(false); // NEW! Hide the status panel on start

        if (PlayerAuthSession.IsLoggedIn) { playerId = PlayerAuthSession.PlayerId; }
        else { Debug.LogWarning("Player not logged in, using default ID: " + playerId); }

        // Add listeners to the buttons
        submitButton.onClick.AddListener(OnSubmitClicked);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);

        // NEW! Add listener for the new Go Back button
        goBackButton.onClick.AddListener(GoToPreviousScene);
    }

    private void CheckForDailySubmission()
    {
        string lastSubmissionDate = PlayerPrefs.GetString(LAST_SUBMISSION_DATE_KEY, "");
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastSubmissionDate == currentDate)
        {
            DisableExpensePage("Submitted for today");
        }
    }

    // ... (OnSubmitClicked, OnNoClicked, OnYesClicked, Co_SendExpenseData, RewardPlayer methods are unchanged) ...
    // ... (You can leave them as they were in the previous script) ...

    private void DisableExpensePage(string message)
    {
        // Disable the main page controls
        expenseInputField.text = "";
        expenseInputField.interactable = false;
        submitButton.interactable = false;

        // NEW! Update the text and show the entire status panel
        if (statusPanel != null && statusText != null)
        {
            statusText.text = message;
            statusPanel.SetActive(true);
        }
    }

    /// <summary>
    /// NEW! This function will be called by the GoBackButton.
    /// It loads the scene you specify.
    /// </summary>
    public void GoToPreviousScene()
    {
        // !!! IMPORTANT !!!
        // Replace "MainMenu" with the actual name of your previous scene.
        SceneManager.LoadScene("DiaryPage");
    }

    #region Data Transfer Classes
    [System.Serializable]
    private class DailyExpenseRequestData { public int player_id; public float daily_spending; }
    [System.Serializable]
    private class ServerResponse { public string status_code; public string message; }
    #endregion

    // --- PASTE THE UNCHANGED METHODS HERE ---
    private void OnSubmitClicked() { if (string.IsNullOrEmpty(expenseInputField.text) || !float.TryParse(expenseInputField.text, out _)) { Debug.LogError("Invalid input."); return; } confirmationPopup.SetActive(true); }
    private void OnNoClicked() { confirmationPopup.SetActive(false); }
    private void OnYesClicked() { confirmationPopup.SetActive(false); StartCoroutine(Co_SendExpenseData()); }
    private void RewardPlayer() { int creditsToAward = 100; Debug.Log($"Player has been awarded {creditsToAward} in-game credits!"); }
    private IEnumerator Co_SendExpenseData() { float dailySpending; if (!float.TryParse(expenseInputField.text, out dailySpending)) { yield break; } var requestData = new DailyExpenseRequestData { player_id = this.playerId, daily_spending = dailySpending }; string jsonRequestBody = JsonUtility.ToJson(requestData); string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/insert_player_daily_tracker.php"; using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST")) { byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody); request.uploadHandler = new UploadHandlerRaw(bodyRaw); request.downloadHandler = new DownloadHandlerBuffer(); request.SetRequestHeader("Content-Type", "application/json"); yield return request.SendWebRequest(); if (request.result != UnityWebRequest.Result.Success) { Debug.LogError($"API Call Failed! Error: {request.error}"); } else { try { var response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text); if (response.status_code == "200" || response.status_code == "0") { RewardPlayer(); PlayerPrefs.SetString(LAST_SUBMISSION_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd")); PlayerPrefs.Save(); DisableExpensePage("Submission successful!"); } else { Debug.LogError("Server returned an error: " + response.message); } } catch (Exception e) { Debug.LogError($"Failed to parse JSON response. Error: {e.Message}"); } } } }

}