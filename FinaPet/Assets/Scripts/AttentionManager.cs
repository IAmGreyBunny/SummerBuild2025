using UnityEngine;
using UnityEngine.UI;

public class AttentionManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider attentionSlider;
    [Header("Attention Settings")]
    public float decreaseIntervalInSeconds = 20f;
    public float decreaseAmount = 5f;
    public float petAmount = 15f;

    private float nextDecreaseTime;

    void Start()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialAffection = GameDataManager.Instance.selectedPet.affection;
            attentionSlider.minValue = 0;
            attentionSlider.maxValue = 100;
            attentionSlider.value = initialAffection;
        }
        else
        {
            attentionSlider.value = 100;
        }
        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
    }

    void Update()
    {
        if (Time.time >= nextDecreaseTime)
        {
            DecreaseAttention();
            nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        }
    }

    void DecreaseAttention()
    {
        attentionSlider.value = Mathf.Max(attentionSlider.minValue, attentionSlider.value - decreaseAmount);
    }

    public void PetTheAnimal()
    {
        attentionSlider.value = Mathf.Min(attentionSlider.maxValue, attentionSlider.value + petAmount);
    }
}