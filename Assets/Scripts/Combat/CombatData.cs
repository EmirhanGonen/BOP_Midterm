using UnityEngine;

[CreateAssetMenu(fileName = "New " + nameof(CombatData), menuName = "ScriptableObjects/Combat/" + nameof(CombatData))]
public class CombatData : ScriptableObject
{
    [SerializeField] private int damage = 0;
    [SerializeField] private string attackAnimationKey = string.Empty;

    [SerializeField, Min(0.00f)] private float attackDistance = 2.00f;
    [SerializeField, Min(0.00f)] private float chargeDuration = 0.60f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float returnDuration = 0.25f;

    public int GetDamageAmount() => damage;
    public int GetAttackAnimationClipHash() => Animator.StringToHash(attackAnimationKey);

    public float GetAttackDistance() => attackDistance;
    public float GetChargeDuration() => chargeDuration;
    public float GetDashDuration() => dashDuration;
    public float GetReturnDuration() => returnDuration;
}
