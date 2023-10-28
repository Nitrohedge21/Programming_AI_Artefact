using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;

    public void SetSliderHP(float amount)
    {
        healthSlider.value = amount;
    }

    public void SetSliderMaxHP(float amount)
    {
        healthSlider.maxValue = amount;
        SetSliderHP(amount);
    }
}
