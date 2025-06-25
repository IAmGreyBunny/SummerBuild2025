using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the pet's hunger UI slider and handles the "run away" logic.
/// It initializes the slider's value from GameDataManager and provides methods
/// for other scripts to interact with the hunger value. It no longer
/// decreases hunger over time on its own.
/// </summary>
public class PetNeedsManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Slider that displays the pet's current hunger level.")]
    public Slider hungerSlider;
    [Tooltip("The popup panel that appears when the pet runs away.")]
    public GameObject petRanAwayPopup;

    private bool petHasRunAway = false;

    void Start()
    {
        // Ensure the runaway popup is hidden on start.
        if (petRanAwayPopup != null)
        {
            petRanAwayPopup.SetActive(false);
        }

        // Initialize the hunger slider's value from the persistent data manager.
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialHunger = GameDataManager.Instance.selectedPet.hunger;

            // Set slider properties.
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = initialHunger;

            Debug.Log($"PetNeedsManager: Hunger slider initialized with value: {initialHunger}");

            // Immediately check if the pet should have already run away based on initial hunger.
            CheckHungerStatus();
        }
        else
        {
            // Fallback for testing or if data is missing.
            Debug.LogWarning("PetNeedsManager: No pet data found in GameDataManager. Initializing hunger to a default of 100.");
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = 100;
        }
    }

    /// <summary>
    /// Checks the current hunger value and triggers the runaway sequence if it's zero.
    /// </summary>
    private void CheckHungerStatus()
    {
        if (petHasRunAway) return;

        if (hungerSlider.value <= 0)
        {
            HandlePetRunAway();
        }
    }

    /// <summary>
    /// The sequence of events when a pet's hunger reaches zero.
    /// </summary>
    private void HandlePetRunAway()
    {
        petHasRunAway = true;
        Debug.Log("Pet has run away due to hunger!");

        // Find and disable the pet's GameObject.
        PetDetails petObject = FindObjectOfType<PetDetails>();
        if (petObject != null)
        {
            petObject.gameObject.SetActive(false);
        }

        // Show the "Pet Ran Away" popup.
        if (petRanAwayPopup != null)
        {
            petRanAwayPopup.SetActive(true);
        }
    }

    /// <summary>
    /// Public method that can be linked to a UI button on the runaway popup
    /// to return the player to the previous scene.
    /// </summary>
    public void GoToPreviousScene()
    {
        // Consider using your SceneController singleton if you have one for history management
        // For now, this will load the "My Pets" scene directly.
        SceneManager.LoadScene("My Pets");
    }

    // --- Public Methods for other scripts (like FeedManager) ---

    /// <summary>
    /// Gets the current integer value of the hunger slider.
    /// </summary>
    public int GetCurrentHunger()
    {
        return (int)hungerSlider.value;
    }

    /// <summary>
    /// Sets the hunger slider to a new value, clamped between its min and max.
    /// After setting, it re-checks the hunger status.
    /// </summary>
    /// <param name="newHunger">The new hunger value.</param>
    public void SetHunger(int newHunger)
    {
        if (petHasRunAway) return;

        hungerSlider.value = Mathf.Clamp(newHunger, hungerSlider.minValue, hungerSlider.maxValue);
        CheckHungerStatus(); // Check if this change caused the pet to run away.
    }

    /// <summary>
    /// Checks if the hunger slider is at its maximum value.
    /// </summary>
    /// <returns>True if hunger is full, false otherwise.</returns>
    public bool IsHungerFull()
    {
        return hungerSlider.value >= hungerSlider.maxValue;
    }
}
