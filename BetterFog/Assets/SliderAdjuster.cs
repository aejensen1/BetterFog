using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderAdjuster : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Slider slider;
    public float changeAmount = 1f;
    public float adjustmentInterval = 0.05f; // Interval for adjustments

    private bool isAdjusting = false;
    private float lastAdjustmentTime;

    private void Update()
    {
        if (isAdjusting && Time.time - lastAdjustmentTime >= adjustmentInterval)
        {
            slider.value = Mathf.Clamp(slider.value + changeAmount, slider.minValue, slider.maxValue);
            lastAdjustmentTime = Time.time;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isAdjusting = true;
        lastAdjustmentTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isAdjusting = false;
    }
}
