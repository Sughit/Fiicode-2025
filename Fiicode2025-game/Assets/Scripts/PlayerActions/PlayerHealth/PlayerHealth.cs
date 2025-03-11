using UnityEngine;

public class PlayerHealth : Health
{
    void Start()
    {
        base.Start();   
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        Debug.Log("Player took " + damage + " damage. Current health: " + currentHealth);
    }

    public override void Heal(float amount)
    {
        base.Heal(amount);
        Debug.Log("Player healed for " + amount + ". Current health: " + currentHealth);
    }

    public override void Die()
    {
        base.Die();
        Debug.Log("Player died!");
    }
}
