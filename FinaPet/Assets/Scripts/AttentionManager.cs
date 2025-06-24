using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the pet's attention level, including the UI slider and interactions.
/// </summary>
public class AttentionManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider attentionSlider;

    [Header("Attention Settings")]
    public float decreaseIntervalInSeconds = 20f; // Decrease attention more frequently than hunger
    public float decreaseAmount = 5f;
    public float petAmount = 15f; // How much attention increases when petted

    private float nextDecreaseTime;

    void Start()
    {
        // --- This logic gets the initial value from the data transfer ---
        // 1. Check if the GameDataManager and its selected pet data exist.
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            // 2. Get the affection value from the transferred data.
            int initialAffection = GameDataManager.Instance.selectedPet.affection;

            // 3. Set the slider's properties.
            // Assuming your affection value from the server is 0-100.
            attentionSlider.minValue = 0;
            attentionSlider.maxValue = 100;
            attentionSlider.value = initialAffection;

            Debug.Log($"Attention slider initialized with value: {initialAffection}");
        }
        else
        {
            // Fallback for when you test the scene directly.
            Debug.LogWarning("No pet data found. Initializing attention to a default value of 100.");
            attentionSlider.minValue = 0;
            attentionSlider.maxValue = 100;
            attentionSlider.value = 100; // Full attention at start
        }
        // --- End of initialization logic ---

        // Set the timer for the first decrease.
        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
    }

    void Update()
    {
        // Check if it's time to decrease the attention level.
        if (Time.time >= nextDecreaseTime)
        {
            DecreaseAttention();
            nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        }
    }

    /// <summary>
    /// Reduces the attention slider's value over time.
    /// </summary>
    void DecreaseAttention()
    {
        attentionSlider.value = Mathf.Max(attentionSlider.minValue, attentionSlider.value - decreaseAmount);
        Debug.Log("Attention decreased to: " + attentionSlider.value);
    }

    /// <summary>
    /// Public method to be called by a "Pet" button to increase attention.
    /// </summary>
    public void PetTheAnimal()
    {
        attentionSlider.value = Mathf.Min(attentionSlider.maxValue, attentionSlider.value + petAmount);
        Debug.Log("Petting the animal. Attention is now: " + attentionSlider.value);
    }
}
