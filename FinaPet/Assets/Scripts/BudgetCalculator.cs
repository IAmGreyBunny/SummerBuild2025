using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI logic for the budgeting page, including calculation
/// and triggering the data submission process.
/// </summary>
public class BudgetCalculator : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The input field for the user's monthly allowance.")]
    public TMP_InputField monthlyAllowanceInputField;
    [Tooltip("The text element to display the calculated 70% budget.")]
    public TMP_Text monthlyBudgetText;
    [Tooltip("The button to submit the data to the server.")]
    public Button submitButton;
    public int debugId;

    [Header("Calculation Settings")]
    [SerializeField] private float budgetPercentage = 0.70f;

    // We will get the player ID from the authentication session.
    private int playerID;

    /// <summary>
    /// This method is called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // --- Dependency and Session Check ---
        // Ensure the manager for sending data exists in the scene.
        if (AllowanceDataManager.Instance == null)
        {
            Debug.LogError("FATAL ERROR: PlayerTrackerManager is not present in the scene.");
            // Disable interaction if the manager is missing.
            submitButton.interactable = false;
            monthlyAllowanceInputField.interactable = false;
            return;
        }

        // Get Player ID from your session manager, just like in PetManager.
        if (PlayerAuthSession.IsLoggedIn)
        {
            playerID = PlayerAuthSession.PlayerId;
        }
        else
        {
            playerID = debugId;
            // If the player is not logged in, disable the functionality.
            //Debug.LogError("Player not logged in. Budget submission will be disabled.");
            //submitButton.interactable = false;
            //monthlyAllowanceInputField.interactable = false;
            //return;
        }

        // --- UI Setup ---
        // Add listeners for real-time calculation and button click
        monthlyAllowanceInputField.onValueChanged.AddListener(CalculateAndDisplayBudget);
        submitButton.onClick.AddListener(OnSubmitButtonPressed);

        // Clear the budget text at the start
        monthlyBudgetText.text = "0.00";
    }

    /// <summary>
    /// Calculates the budget based on the input value and updates the display text.
    /// </summary>
    public void CalculateAndDisplayBudget(string inputValue)
    {
        if (float.TryParse(inputValue, out float allowance))
        {
            float calculatedBudget = allowance * budgetPercentage;
            monthlyBudgetText.text = calculatedBudget.ToString("F2");
        }
        else
        {
            monthlyBudgetText.text = "0.00";
        }
    }

    /// <summary>
    /// Called when the submit button is pressed.
    /// </summary>
    public void OnSubmitButtonPressed()
    {
        // Validate the input before sending.
        if (float.TryParse(monthlyAllowanceInputField.text, out float allowanceValue))
        {
            Debug.Log($"Submitting allowance: {allowanceValue} for player ID: {playerID}");
            // Start the coroutine on the PlayerTrackerManager to send the data.
            StartCoroutine(AllowanceDataManager.Instance.Co_UpdateMonthlyIncome(playerID, allowanceValue));

            // Optional: Provide user feedback, e.g., disable the button while submitting.
            submitButton.interactable = false;
            // You can re-enable it after the coroutine finishes.
        }
        else
        {
            Debug.LogError("Invalid monthly allowance input. Cannot submit.");
            // Optionally, show a UI error message to the user here.
        }
    }

    void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks.
        if (monthlyAllowanceInputField != null) monthlyAllowanceInputField.onValueChanged.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
    }
}
