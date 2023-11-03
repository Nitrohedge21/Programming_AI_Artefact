using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;

    public void SetSliderHP(int amount)
    {
        healthSlider.value = amount;
    }

    public void SetSliderMaxHP(int amount)
    {
        healthSlider.maxValue = amount;
        SetSliderHP(amount);
    }
}
