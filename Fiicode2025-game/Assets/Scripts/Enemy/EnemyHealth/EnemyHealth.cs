using UnityEngine;

public class EnemyHealth : Health
{
    void Start()
    {
        base.Start();   
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        Debug.Log("Enemy took " + damage + " damage. Current health: " + currentHealth);
    }

    public override void Heal(float amount)
    {
        base.Heal(amount);
        Debug.Log("Enemy healed for " + amount + ". Current health: " + currentHealth);
    }

    public override void Die()
    {
        GameObject manager = GameObject.FindWithTag("GameManager");
        if(manager != null)
            manager.GetComponent<GameManagerHostile>().EnemyKilled();
        base.Die();
        Debug.Log("Enemy died!");
    }
}
