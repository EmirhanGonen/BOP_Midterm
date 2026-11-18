using System;
using DG.Tweening;
using UnityEngine;
using System.Threading;
using MoreMountains.Feedbacks;
using Cysharp.Threading.Tasks;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private CombatData[] combatData = Array.Empty<CombatData>();
    [SerializeField] private PlayerAnimationController playerAnimationController = null;
    [SerializeField] private Health targetHealth = null;

    [SerializeField] private Transform movementRoot = null;
    [SerializeField] private Ease dashEase = Ease.InQuad;
    [SerializeField] private Ease returnEase = Ease.OutQuad;

    [SerializeField] private MMF_Player chargeFeedback = null;
    [SerializeField] private bool matchChargeFeedbackDuration = true;
    [SerializeField] private MMF_Player dashFeedback = null;
    [SerializeField] private MMF_Player attackFeedback = null;
    [SerializeField] private MMF_Player returnFeedback = null;

    [SerializeField] private VfxSpawnData chargeVfx = default;
    [SerializeField] private VfxSpawnData dashVfx = default;
    [SerializeField] private VfxSpawnData hitVfx = default;
    [SerializeField] private VfxSpawnData returnVfx = default;

    [SerializeField, Min(0.00f)] private float hitFreezeDuration = 0.08f;
    [SerializeField, Range(0.00f, 1.00f)] private float hitFreezeTimeScale = 0.00f;

    private bool isAttacking = false;
    private bool isHitFreezing = false;
    private Tween moveTween = null;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        movementRoot = transform;
    }

    private void OnDestroy()
    {
        if (isHitFreezing)
            Time.timeScale = 1.00f;

        moveTween?.Kill();
    }

    public void Attack(in CombatData combatData)
    {
        TryAttack(combatData);
    }

    public bool TryAttack(CombatData combatData)
    {
        if (isAttacking || !playerAnimationController.CanPlayCombatAnimation)
            return false;

        AttackSequence(combatData).Forget();
        return true;
    }

    public void Attack(in int index)
    {
        Attack(combatData[index]);
    }

    private async UniTaskVoid AttackSequence(CombatData data)
    {
        isAttacking = true;
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        Vector3 startPosition = movementRoot.position;
        Vector3 attackPosition = GetAttackPosition(startPosition, data.GetAttackDistance());

        SetFeedbackDurations(chargeFeedback, data.GetChargeDuration());
        PlayFeedback(chargeFeedback);
        GameObject chargeInstance = chargeVfx.Spawn(startPosition, true, movementRoot);
        await UniTask.Delay(TimeSpan.FromSeconds(data.GetChargeDuration()), cancellationToken: token);
        StopFeedback(chargeFeedback);
        if (chargeInstance)
            Destroy(chargeInstance);

        PlayFeedback(dashFeedback);
        dashVfx.Spawn(startPosition, startPosition.x < attackPosition.x);
        await MoveTo(attackPosition.x, data.GetDashDuration(), dashEase, token);

        PlayFeedback(attackFeedback);
        hitVfx.Spawn(targetHealth.transform.position, startPosition.x < attackPosition.x);
        targetHealth.TakeDamage(data.GetDamageAmount());
        HitFreeze().Forget();

        await playerAnimationController.PlayCombatAnimationAsync(data);
        token.ThrowIfCancellationRequested();

        playerAnimationController.PlayIdle();
        PlayFeedback(returnFeedback);
        returnVfx.Spawn(movementRoot.position, startPosition.x < attackPosition.x);
        await MoveTo(startPosition.x, data.GetReturnDuration(), returnEase, token);
        isAttacking = false;
    }

    private async UniTaskVoid HitFreeze()
    {
        if (isHitFreezing || hitFreezeDuration <= 0.00f)
            return;

        isHitFreezing = true;
        float previousTimeScale = Time.timeScale;
        Time.timeScale = hitFreezeTimeScale;

        await UniTask.Delay(TimeSpan.FromSeconds(hitFreezeDuration), DelayType.UnscaledDeltaTime,
            cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

        Time.timeScale = previousTimeScale;
        isHitFreezing = false;
    }

    private Vector3 GetAttackPosition(Vector3 startPosition, float attackDistance)
    {
        float targetX = targetHealth.transform.position.x;
        float side = Mathf.Sign(startPosition.x - targetX);
        if (side == 0.00f)
            side = -1.00f;

        return new Vector3(targetX + side * attackDistance, startPosition.y, startPosition.z);
    }

    private async UniTask MoveTo(float x, float duration, Ease ease, CancellationToken token)
    {
        moveTween?.Kill();
        moveTween = movementRoot.DOMoveX(x, duration).SetEase(ease).SetLink(gameObject);

        Tween tween = moveTween;
        await UniTask.WaitWhile(() => tween.IsActive() && !tween.IsComplete(), cancellationToken: token);
    }

    private static void PlayFeedback(MMF_Player feedback)
    {
        feedback.PlayFeedbacks();
    }

    private static void SetFeedbackDurations(MMF_Player feedback, float duration)
    {
        foreach (MMF_Feedback item in feedback.FeedbacksList)
            item.FeedbackDuration = duration;
    }

    private static void StopFeedback(MMF_Player feedback)
    {
        feedback.StopFeedbacks();
    }
}

[Serializable]
public struct VfxSpawnData
{
    [SerializeField] private GameObject prefab;

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private Vector3 offset;
    [SerializeField] private int sortingOrder;
    [SerializeField] private float lifetime;

    public GameObject Spawn(Vector3 position, bool facingRight, Transform parent = null)
    {
        if (spawnPoint)
            position = spawnPoint.position;

        Vector3 o = offset;
        if (!facingRight)
            o.x = -o.x;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, position + o, Quaternion.identity,
            spawnPoint ? spawnPoint : parent);

        foreach (ParticleSystemRenderer r in instance.GetComponentsInChildren<ParticleSystemRenderer>())
            r.sortingOrder += sortingOrder;

        UnityEngine.Object.Destroy(instance, lifetime > 0.00f ? lifetime : 3.00f);

        return instance;
    }
}