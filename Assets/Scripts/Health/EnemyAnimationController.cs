using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator = null;

    private const int BaseLayer = 0;

    private static readonly int IDLE_HASH = Animator.StringToHash("Idle");
    private static readonly int TAKE_HIT_HASH = Animator.StringToHash("TakeHit");
    private static readonly int DIE_HASH = Animator.StringToHash("Die");

    private CancellationTokenSource animationCancellationTokenSource = null;
    private bool isDead = false;

    private void Awake()
    {
        PlayIdle();
    }

    private void OnDestroy()
    {
        CancelCurrentAnimation();
    }

    public void HandleOnHealthTakeDamage(OnTakeDamagePackage damagePackage)
    {
        if (isDead)
            return;

        PlayHitThenIdle().Forget();
    }

    public void HandleOnDead()
    {
        isDead = true;
        CancelCurrentAnimation();
        animator.Play(DIE_HASH, BaseLayer, 0.00f);
    }

    public void PlayIdle()
    {
        animator.Play(IDLE_HASH, BaseLayer, 0.00f);
    }

    private async UniTaskVoid PlayHitThenIdle()
    {
        CancelCurrentAnimation();
        animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        CancellationToken token = animationCancellationTokenSource.Token;

        animator.Play(TAKE_HIT_HASH, BaseLayer, 0.00f);

        bool cancelled = await WaitUntilAnimationEnd(token).SuppressCancellationThrow();
        if (cancelled || isDead)
            return;

        PlayIdle();
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
