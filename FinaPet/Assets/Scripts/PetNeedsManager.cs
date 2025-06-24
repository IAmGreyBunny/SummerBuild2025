using UnityEngine;
using UnityEngine.UI;

public class PetNeedsManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider hungerSlider;

    [Header("Hunger Settings")]
    public float decreaseIntervalInSeconds = 30f; // Decrease every 30 seconds for debugging
    public float decreaseAmount = 0.1f;
    public float feedAmount = 0.2f;

    private float nextDecreaseTime;

    void Start()
    {
        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        hungerSlider.value = 100f; // Full hunger at start
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
        hungerSlider.value = Mathf.Max(0f, hungerSlider.value - decreaseAmount);
        Debug.Log("Hunger decreased to: " + hungerSlider.value);
    }

    public void FeedPet()
    {
        hungerSlider.value = Mathf.Min(100f, hungerSlider.value + feedAmount);
        Debug.Log("Feeding the pet. Hunger is now: " + hungerSlider.value);
    }
}
