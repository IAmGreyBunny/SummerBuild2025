using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles the UI for the budgeting page.
/// Fetches data on start, submits updates, and shows a confirmation pop-up.
/// </summary>
public class BudgetCalculator : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField monthlyAllowanceInputField;
    public TMP_Text monthlyBudgetText;
    public Button submitButton;

    [Header("UI Feedback")]
    public GameObject loadingIndicator;
    [Tooltip("The panel that pops up to confirm submission.")]
    public GameObject confirmationPanel; // Reference to your new panel

    [Header("Calculation Settings")]
    [SerializeField] private float budgetPercentage = 0.70f;

    [Header("Testing")]
    public int debugID = 1;

    private int playerID;
    private bool isSubmitting = false;

    void Start()
    {
        // --- Initial UI State ---
        monthlyAllowanceInputField.interactable = false;
        submitButton.interactable = false;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);
        // Make sure the confirmation panel is hidden at the start
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // --- Dependency and Session Check ---
        if (AllowanceDataManager.Instance == null)
        {
            Debug.LogError("FATAL ERROR: AllowanceDataManager is not present.");
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            return;
        }

        if (PlayerAuthSession.IsLoggedIn) { playerID = PlayerAuthSession.PlayerId; }
        else { playerID = debugID; Debug.LogWarning($"Player not logged in. Using Debug ID: {playerID}"); }

        StartCoroutine(LoadInitialData());
        monthlyAllowanceInputField.onValueChanged.AddListener(CalculateAndDisplayBudget);
        submitButton.onClick.AddListener(OnSubmitButtonPressed);
    }

    private IEnumerator LoadInitialData()
    {
        float loadedIncome = -1f;
        yield return StartCoroutine(AllowanceDataManager.Instance.Co_GetMonthlyIncome(playerID, result => { loadedIncome = result; }));
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        monthlyAllowanceInputField.interactable = true;
        submitButton.interactable = true;
        if (loadedIncome > 0)
        {
            string incomeText = loadedIncome.ToString("F2");
            monthlyAllowanceInputField.text = incomeText;
            CalculateAndDisplayBudget(incomeText);
        }
        else
        {
            monthlyAllowanceInputField.text = "";
            monthlyBudgetText.text = "0.00";
        }
    }

    public void CalculateAndDisplayBudget(string inputValue)
    {
        if (float.TryParse(inputValue, out float allowance)) { monthlyBudgetText.text = (allowance * budgetPercentage).ToString("F2"); }
        else { monthlyBudgetText.text = "0.00"; }
    }

    public void OnSubmitButtonPressed()
    {
        if (isSubmitting) return;
        if (float.TryParse(monthlyAllowanceInputField.text, out float allowanceValue)) { StartCoroutine(SubmitData(allowanceValue)); }
        else { Debug.LogError("Invalid monthly allowance input. Cannot submit."); }
    }

    /// <summary>
    /// Handles the submission process and shows the confirmation panel.
    /// </summary>
    private IEnumerator SubmitData(float allowance)
    {
        isSubmitting = true;
        submitButton.interactable = false; // Disable submit button during process
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        // Wait for the data to be sent
        yield return StartCoroutine(AllowanceDataManager.Instance.Co_UpdateMonthlyIncome(playerID, allowance));

        // Hide loading indicator and show the confirmation
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the confirmation pop-up and re-enables the submit button.
    /// This should be called by the 'OK' or 'Close' button on your pop-up panel.
    /// </summary>
    public void CloseConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        // Re-enable the submit button now that the user has acknowledged the popup
        submitButton.interactable = true;
        isSubmitting = false;
    }

    void OnDestroy()
    {
        if (monthlyAllowanceInputField != null) monthlyAllowanceInputField.onValueChanged.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
    }
}
