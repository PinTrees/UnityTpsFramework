using UnityEngine;

public struct vDamage
{
    public int damage;
    public string damageType;
    public CharacterActorBase source; 
}

public class HealthController : MonoBehaviour
{
    public CharacterActorBase ownerCharacter;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int maxHealth;
    private float dampedHealth;

    public event System.Action<int> OnHealthChanged;

    
    public void Init(CharacterActorBase owner, int maxHealth)
    {
        this.ownerCharacter = owner;
        dampedHealth = currentHealth = this.maxHealth = maxHealth; 
    }

    public void TakeDamage(vDamage damage)
    {
        currentHealth -= damage.damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        OnHealthChanged?.Invoke(currentHealth); 

        if (currentHealth <= 0)
        {
            ownerCharacter.OnDeath();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        OnHealthChanged?.Invoke(currentHealth);
    }

    public float GetHealthAmount()
    {
        return (float)currentHealth / maxHealth; 
    }
    public float GetHealthAmountDamp()
    {
        return (float)dampedHealth / maxHealth;
    }

    public void Revive(int healthAmount)
    {
        currentHealth = Mathf.Clamp(healthAmount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void UpdateHealth()
    {
        dampedHealth = Mathf.Lerp(dampedHealth, currentHealth, Time.deltaTime * 1);
    }
}
