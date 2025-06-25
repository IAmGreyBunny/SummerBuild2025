using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using UnityEngine.UI; // NEW! This was missing and is needed for the 'Button' type.
using TMPro; // This is needed for TextMeshPro elements
using UnityEngine.SceneManagement; // This is needed for loading scenes

public class ExpenseManager : MonoBehaviour
{
    [Header("Page References")]
    [Tooltip("The parent object holding the expense input field and main submit button.")]
    [SerializeField] private GameObject addExpensePage;
    [Tooltip("The parent object holding the status text and the go-back button.")]
    [SerializeField] private GameObject statusPanel;

    [Header("UI Element References")]
    [SerializeField] private TMP_InputField expenseInputField;
    [SerializeField] private Button submitButton; // This requires UnityEngine.UI
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private Button yesButton; // This requires UnityEngine.UI
    [SerializeField] private Button noButton; // This requires UnityEngine.UI
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button goBackButton; // This requires UnityEngine.UI

    [Header("Player and Server Config")]
    public int playerId = 1;
    private const string LAST_SUBMISSION_DATE_KEY = "LastExpenseSubmissionDate";

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

        // Session and Listeners
        if (PlayerAuthSession.IsLoggedIn) { playerId = PlayerAuthSession.PlayerId; }
        else { Debug.LogWarning("Player not logged in, using default ID: " + playerId); }

        submitButton.onClick.AddListener(OnSubmitClicked);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        goBackButton.onClick.AddListener(GoToPreviousScene);
    }

    /// <summary>
    /// Controls which UI view is active.
    /// </summary>
    /// <param name="isInputMode">True to show the expense input, False to show the status panel.</param>
    /// <param name="message">The message to display in the status panel.</param>
    private void SetUIMode(bool isInputMode, string message = "")
    {
        if (isInputMode)
        {
            addExpensePage.SetActive(true);
            statusPanel.SetActive(false);
        }
        else
        {
            addExpensePage.SetActive(false);
            statusPanel.SetActive(true);
            statusText.text = message;
        }
    }

    private IEnumerator Co_SendExpenseData()
    {
        float dailySpending;
        if (!float.TryParse(expenseInputField.text, out dailySpending))
        {
            Debug.LogError("Failed to parse expense amount. Aborting.");
            yield break;
        }

        var requestData = new DailyExpenseRequestData { player_id = this.playerId, daily_spending = dailySpending };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/insert_player_daily_tracker.php";

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
            }
            else
            {
                var response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                if (response.status_code == "200" || response.status_code == "0")
                {
                    Debug.Log("Expense submitted successfully!");
                    RewardPlayer();

                    PlayerPrefs.SetString(LAST_SUBMISSION_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
                    PlayerPrefs.Save();

                    SetUIMode(false, "Submission successful!");
                }
                else
                {
                    Debug.LogError("Server returned an error: " + response.message);
                }
            }
        }
    }

    public void GoToPreviousScene() { SceneManager.LoadScene("MainMenu"); }
    private void OnSubmitClicked() { if (string.IsNullOrEmpty(expenseInputField.text) || !float.TryParse(expenseInputField.text, out _)) { Debug.LogError("Invalid input."); return; } confirmationPopup.SetActive(true); }
    private void OnNoClicked() { confirmationPopup.SetActive(false); }
    private void OnYesClicked() { confirmationPopup.SetActive(false); StartCoroutine(Co_SendExpenseData()); }
    private void RewardPlayer() { int creditsToAward = 100; Debug.Log($"Player has been awarded {creditsToAward} in-game credits!"); }
    #region Data Transfer Classes
    [System.Serializable] private class DailyExpenseRequestData { public int player_id; public float daily_spending; }
    [System.Serializable] private class ServerResponse { public string status_code; public string message; }
    #endregion
}