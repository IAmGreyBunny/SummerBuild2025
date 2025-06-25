using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetNeedsManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider hungerSlider;
    [Header("UI Popups")]
    public GameObject petRanAwayPopup;
    [Header("Hunger Settings")]
    public float decreaseIntervalInSeconds = 30f;
    public float decreaseAmount = 10f;
    public float feedAmount = 20f;

    private float nextDecreaseTime;
    private bool petHasRunAway = false;

    void Start()
    {
        if (petRanAwayPopup != null) { petRanAwayPopup.SetActive(false); }

        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialHunger = GameDataManager.Instance.selectedPet.hunger;
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = initialHunger;
        }
        else
        {
            hungerSlider.value = 100;
        }
        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        CheckHungerStatus();
    }

    void Update()
    {
        if (petHasRunAway) { return; }
        if (Time.time >= nextDecreaseTime)
        {
            DecreaseHunger();
            nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        }
    }

    void DecreaseHunger()
    {
        hungerSlider.value = Mathf.Max(hungerSlider.minValue, hungerSlider.value - decreaseAmount);
        CheckHungerStatus();
    }

    private void CheckHungerStatus()
    {
        if (petHasRunAway) return;
        if (hungerSlider.value <= 0) { HandlePetRunAway(); }
    }

    public void FeedPet()
    {
        if (petHasRunAway) return;
        hungerSlider.value = Mathf.Min(hungerSlider.maxValue, hungerSlider.value + feedAmount);
    }

    private void HandlePetRunAway()
    {
        petHasRunAway = true;
        PetDetails petObject = FindObjectOfType<PetDetails>();
        if (petObject != null) { petObject.gameObject.SetActive(false); }
        if (petRanAwayPopup != null) { petRanAwayPopup.SetActive(true); }
    }

    public void GoToPreviousScene()
    {
        SceneManager.LoadScene("My Pet");
    }
}