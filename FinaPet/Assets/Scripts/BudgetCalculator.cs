using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BudgetCalculator : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField monthlyAllowanceInputField;
    public TMP_Text monthlyBudgetText;
    public Button submitButton;
    public GameObject loadingIndicator;

    [Header("Calculation Settings")]
    [SerializeField] private float budgetPercentage = 0.70f;

    [Header("Testing")]
    [Tooltip("A fallback ID to use when not logged in.")]
    public int debugID = 1;

    private int playerID;
    private bool isSubmitting = false;

    void Start()
    {
        monthlyAllowanceInputField.interactable = false;
        submitButton.interactable = false;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        if (AllowanceDataManager.Instance == null)
        {
            Debug.LogError("FATAL ERROR: AllowanceDataManager is not present.");
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            return;
        }

        if (PlayerAuthSession.IsLoggedIn)
        {
            playerID = PlayerAuthSession.PlayerId;
        }
        else
        {
            playerID = debugID;
            Debug.LogWarning($"Player not logged in. Using Debug ID: {playerID}");
        }

        StartCoroutine(LoadInitialData());
        monthlyAllowanceInputField.onValueChanged.AddListener(CalculateAndDisplayBudget);
        submitButton.onClick.AddListener(OnSubmitButtonPressed);
    }

    private IEnumerator LoadInitialData()
    {
        float loadedIncome = -1f;
        yield return StartCoroutine(AllowanceDataManager.Instance.Co_GetMonthlyIncome(
            playerID,
            result => { loadedIncome = result; }
        ));

        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        monthlyAllowanceInputField.interactable = true;
        submitButton.interactable = true;

        if (loadedIncome > 0)
        {
            // Set the text for the input field
            string incomeText = loadedIncome.ToString("F2");
            monthlyAllowanceInputField.text = incomeText;

            // --- THIS IS THE FIX ---
            // Manually call the calculation function to update the second box
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
        if (float.TryParse(inputValue, out float allowance))
        {
            monthlyBudgetText.text = (allowance * budgetPercentage).ToString("F2");
        }
        else
        {
            monthlyBudgetText.text = "0.00";
        }
    }

    public void OnSubmitButtonPressed()
    {
        if (isSubmitting) return;
        if (float.TryParse(monthlyAllowanceInputField.text, out float allowanceValue))
        {
            StartCoroutine(SubmitData(allowanceValue));
        }
        else
        {
            Debug.LogError("Invalid monthly allowance input. Cannot submit.");
        }
    }

    private IEnumerator SubmitData(float allowance)
    {
        isSubmitting = true;
        submitButton.interactable = false;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);
        yield return StartCoroutine(AllowanceDataManager.Instance.Co_UpdateMonthlyIncome(playerID, allowance));
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        submitButton.interactable = true;
        isSubmitting = false;
    }

    void OnDestroy()
    {
        if (monthlyAllowanceInputField != null) monthlyAllowanceInputField.onValueChanged.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
    }
}
