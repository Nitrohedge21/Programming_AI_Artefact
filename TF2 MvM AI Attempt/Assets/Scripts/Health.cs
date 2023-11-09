using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] public int currentHealth;
    [SerializeField] public int maxHealth;

    [HideInInspector] public HealthBar healthbar;
    [SerializeField] private bool canBeDestroyed = false;

    void Start()
    {
        currentHealth = maxHealth;

        if(this.gameObject.CompareTag("Player")) { healthbar.SetSliderMaxHP(maxHealth); }
    }

    void Update() 
    {
        Mathf.Clamp(currentHealth, 0, maxHealth);
        //The line above is for overheal which is supposed to decay over time but since health is an integer, idk what to do.
        if(currentHealth > maxHealth) { currentHealth -= 1; }
        if (canBeDestroyed == true && currentHealth <= 0) { Destroy(gameObject); }
    }

    public void TakeDamage(int amount)
    {
        //Take damage only when the health is above 0 so that the value doesn't go below 0.
        if(currentHealth >= 0) { currentHealth -= amount; }
        
        if (this.gameObject.CompareTag("Player")) { healthbar.SetSliderHP(currentHealth); }
            
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (this.gameObject.CompareTag("Player")) { healthbar.SetSliderHP(currentHealth); }
    }

}
