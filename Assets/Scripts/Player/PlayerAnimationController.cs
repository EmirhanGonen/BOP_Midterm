using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private float toIdleAnimationDelay = 0.20f;

    private const int BaseLayer = 0;

    private const string IdleAnimationKey = "Idle";
    private static readonly int IDLE_ANIMATION_HASH = Animator.StringToHash(IdleAnimationKey);

    private const string UnarmedToArmedAnimationKey = "UnarmedToArmed";
    private static readonly int UNARMED_TO_ARMED_ANIMATION_HASH = Animator.StringToHash(UnarmedToArmedAnimationKey);

    private CancellationTokenSource animationCancellationTokenSource = null;

    private bool isLocked = false;

    public bool CanPlayCombatAnimation => !isLocked;

    private void Awake()
    {
        PlayStartAnimation().Forget();
    }

    private void OnDestroy()
    {
        CancelCurrentAnimation();
    }

    private async UniTaskVoid PlayStartAnimation()
    {
        bool completed = await PlayAndWaitAsync(UNARMED_TO_ARMED_ANIMATION_HASH, uninterruptible: true);
        if (completed)
            PlayIdle();
    }

    public async UniTask<bool> PlayCombatAnimationAsync(CombatData combatData)
    {
        if (isLocked)
            return false;

        bool completed = await PlayAndWaitAsync(combatData.GetAttackAnimationClipHash());
        if (!completed)
            return false;

        bool cancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(toIdleAnimationDelay),
            cancellationToken: animationCancellationTokenSource.Token).SuppressCancellationThrow();
        return !cancelled;
    }

    public void PlayIdle()
    {
        animator.Play(IDLE_ANIMATION_HASH, BaseLayer, 0.00f);
    }

    private async UniTask<bool> PlayAndWaitAsync(int stateHash, bool uninterruptible = false)
    {
        CancelCurrentAnimation();
        animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        CancellationToken token = animationCancellationTokenSource.Token;

        isLocked = uninterruptible;
        animator.Play(stateHash, BaseLayer, 0.00f);

        bool cancelled = await WaitUntilAnimationEnd(token).SuppressCancellationThrow();
        if (uninterruptible)
            isLocked = false;

        return !cancelled;
    }

    private async UniTask WaitUntilAnimationEnd(CancellationToken token)
    {
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(BaseLayer);

        float duration = info.length / Mathf.Max(animator.speed, 0.0001f);
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);
    }

    private void CancelCurrentAnimation()
    {
        if (animationCancellationTokenSource == null)
            return;

        animationCancellationTokenSource.Cancel();
        animationCancellationTokenSource.Dispose();
        animationCancellationTokenSource = null;
    }
}
