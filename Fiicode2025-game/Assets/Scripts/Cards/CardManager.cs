using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Stats")]
    public int projNum;
    public float projSpeed;
    public float projDamage;
    public float projLifeTime;
    public float projSize;
    public float maxHealth;
    public float healthRegen;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;

    public static CardManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void DiscoverCard(Card card)
    {

    }

    public void OnProjStart()
    {
        Debug.Log("Attack start");
    }

    public void OnProjEnd()
    {
        Debug.Log("Attack end");
    }

    public void OnAbility()
    {
        Debug.Log("Ability");
    }
}
