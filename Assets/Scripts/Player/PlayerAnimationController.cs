using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator = null;

    private const string IdleAnimationKey = "Idle";
    private static readonly int IDLE_ANIMATION_HASH = Animator.StringToHash(IdleAnimationKey);

    private const string UnarmedToArmedAnimationKey = "UnarmedToArmed";
    private static readonly int UNARMED_TO_ARMED_ANIMATION_HASH = Animator.StringToHash(UnarmedToArmedAnimationKey);

    private void Awake()
    {
        PlayStartAnimation();
    }


    private async UniTaskVoid PlayStartAnimation()
    {
        animator.Play(UNARMED_TO_ARMED_ANIMATION_HASH);
        await UniTask.Yield();
        
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        await UniTask.WaitUntil(
            () => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.00f,
            cancellationToken: token);

        animator.Play(IDLE_ANIMATION_HASH);
    }


    public void PlayCombatAnimation(in CombatData combatData)
    {
        animator.Play(combatData.GetAttackAnimationClipHash());
    }
}