using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Custom/Card")]
public class Card : ScriptableObject
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

    [Header("Special")]
    public string cardName;
}
