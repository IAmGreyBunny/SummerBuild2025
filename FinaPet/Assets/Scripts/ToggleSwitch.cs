using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Toggle Switch Settings")]
    [SerializeField, Range(0, 1f)] private float toggleValue = 0f;

    public bool CurrentValue { get; private set; }

    [Header("Animation")]
    [SerializeField, Range(0, 2f)] private float animationDuration = 0.25f;

    [Header("Events")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    private Slider slider;
    private Coroutine currentAnimation;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.interactable = false; // optional: makes it non-draggable
        slider.value = toggleValue;
        CurrentValue = slider.value > 0.5f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        SetStateAndAnimate(!CurrentValue);
    }

    private void SetStateAndAnimate(bool newState)
    {
        CurrentValue = newState;
        float targetValue = newState ? 1f : 0f;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateToggle(targetValue));

        if (CurrentValue)
            onToggleOn?.Invoke();
        else
            onToggleOff?.Invoke();
    }

    private IEnumerator AnimateToggle(float targetValue)
    {
        float startValue = slider.value;
        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, time / animationDuration);
            yield return null;
        }

        slider.value = targetValue;
    }
}