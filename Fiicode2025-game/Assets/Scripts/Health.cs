using UnityEngine;

public abstract class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
