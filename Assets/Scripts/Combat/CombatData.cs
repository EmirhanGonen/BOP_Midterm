using UnityEngine;

[CreateAssetMenu(fileName = "New " + nameof(CombatData), menuName = "ScriptableObjects/Combat/" + nameof(CombatData))]
public class CombatData : ScriptableObject
{
    [SerializeField] private int damage = 0;
    [SerializeField] private string attackAnimationKey = string.Empty;


    public int GetDamage() => damage;
    public int GetAttackAnimationClipHash() => Animator.StringToHash(attackAnimationKey);
}