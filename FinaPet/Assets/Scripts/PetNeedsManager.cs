using UnityEngine;
using UnityEngine.UI;

public class PetNeedsManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider hungerSlider;

    [Header("Hunger Settings")]
    public float decreaseIntervalInSeconds = 30f;
    public float decreaseAmount = 10f; // Changed to whole numbers for clarity
    public float feedAmount = 20f;     // Changed to whole numbers for clarity

    private float nextDecreaseTime;

    void Start()
    {
        // --- THIS IS THE FIX ---
        // 1. Check if the GameDataManager and its selected pet data exist.
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            // 2. Get the hunger value from the transferred data.
            int initialHunger = GameDataManager.Instance.selectedPet.hunger;

            // 3. Set the slider's properties.
            // Assuming your hunger value from the server is 0-100.
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = initialHunger;

            Debug.Log($"Hunger slider initialized with value: {initialHunger}");
        }
        else
        {
            // Fallback for when you test the scene directly without coming from MyPet scene.
            Debug.LogWarning("No pet data found. Initializing hunger to a default value of 100.");
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = 100; // Full hunger at start
        }
        // --- END OF FIX ---

        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
    }

    void Update()
    {
        if (Time.time >= nextDecreaseTime)
        {
            DecreaseHunger();
            nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        }
    }

    void DecreaseHunger()
    {
        hungerSlider.value = Mathf.Max(hungerSlider.minValue, hungerSlider.value - decreaseAmount);
        Debug.Log("Hunger decreased to: " + hungerSlider.value);
    }

    public void FeedPet()
    {
        hungerSlider.value = Mathf.Min(hungerSlider.maxValue, hungerSlider.value + feedAmount);
        Debug.Log("Feeding the pet. Hunger is now: " + hungerSlider.value);
    }
}