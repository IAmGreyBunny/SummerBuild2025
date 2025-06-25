using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the pet's affection UI slider. It initializes the slider's value
/// based on the selected pet's data and provides methods for other scripts
/// to interact with the affection value.
/// </summary>
public class AttentionManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider attentionSlider;

    void Start()
    {
        // Initialize the affection slider's value from the persistent data manager.
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialAffection = GameDataManager.Instance.selectedPet.affection;

            // Set slider properties. Assuming affection is always 0-100.
            attentionSlider.minValue = 0;
            attentionSlider.maxValue = 100;
            attentionSlider.value = initialAffection;

            Debug.Log($"AttentionManager: Affection slider initialized with value: {initialAffection}");
        }
        else
        {
            // Fallback for testing or if data is missing.
            Debug.LogWarning("AttentionManager: No pet data found in GameDataManager. Initializing affection to a default of 0.");
            attentionSlider.minValue = 0;
            attentionSlider.maxValue = 100;
            attentionSlider.value = 0;
        }
    }

    /// <summary>
    /// Gets the current integer value of the affection slider.
    /// </summary>
    public int GetCurrentAffection()
    {
        return (int)attentionSlider.value;
    }

    /// <summary>
    /// Sets the affection slider to a new value, clamped between its min and max.
    /// </summary>
    /// <param name="newAffection">The new affection value.</param>
    public void SetAffection(int newAffection)
    {
        attentionSlider.value = Mathf.Clamp(newAffection, attentionSlider.minValue, attentionSlider.maxValue);
    }

    /// <summary>
    /// Checks if the affection slider is at its maximum value.
    /// </summary>
    /// <returns>True if affection is full, false otherwise.</returns>
    public bool IsAffectionFull()
    {
        return attentionSlider.value >= attentionSlider.maxValue;
    }
}
