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

    private bool isAttacking = false;
    private Tween moveTween = null;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        if (!movementRoot)
            movementRoot = transform;
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
    }

    public void Attack(in CombatData combatData)
    {
        if (isAttacking || !playerAnimationController.CanPlayCombatAnimation)
            return;

        AttackSequence(combatData).Forget();
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
        await UniTask.Delay(TimeSpan.FromSeconds(data.GetChargeDuration()), cancellationToken: token);
        StopFeedback(chargeFeedback);

        PlayFeedback(dashFeedback);
        await MoveTo(attackPosition.x, data.GetDashDuration(), dashEase, token);

        PlayFeedback(attackFeedback);
        targetHealth.TakeDamage(data.GetDamageAmount());

        await playerAnimationController.PlayCombatAnimationAsync(data);
        token.ThrowIfCancellationRequested();

        playerAnimationController.PlayIdle();
        PlayFeedback(returnFeedback);
        await MoveTo(startPosition.x, data.GetReturnDuration(), returnEase, token);
        isAttacking = false;
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