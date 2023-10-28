using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] public float currenthealth;
    [SerializeField] private float maxHealth;

    [HideInInspector] public HealthBar healthbar;
    [SerializeField] private bool canBeDestroyed = false;

    void Start()
    {
        currenthealth = maxHealth;

        if(this.gameObject.CompareTag("Player")) { healthbar.SetSliderMaxHP(maxHealth); }
    }

    void Update() 
    {
        Mathf.Clamp(currenthealth, 0, maxHealth);
        if (canBeDestroyed == true && currenthealth <= 0) { Destroy(gameObject); }
    }

    public void TakeDamage(float amount)
    {
        //Take damage only when the health is above 0 so that the value doesn't go below 0.
        if(currenthealth >= 0) { currenthealth -= amount; }
        
        if (this.gameObject.CompareTag("Player")) { healthbar.SetSliderHP(currenthealth); }
            
    }

    public void Heal(float amount)
    {
        currenthealth += amount;
        if (this.gameObject.CompareTag("Player")) { healthbar.SetSliderHP(currenthealth); }
    }

}
