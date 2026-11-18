using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator = null;

    private const int BaseLayer = 0;

    private static readonly int IDLE_HASH = Animator.StringToHash("Idle");
    private static readonly int TAKE_HIT_HASH = Animator.StringToHash("TakeHit");
    private static readonly int DIE_HASH = Animator.StringToHash("Die");

    private bool isDead = false;

    private void Awake()
    {
        PlayIdle();
    }

    public void HandleOnHealthTakeDamage(OnTakeDamagePackage damagePackage)
    {
        if (isDead)
            return;

        animator.Play(TAKE_HIT_HASH, BaseLayer, 0.00f);
    }

    public void HandleOnDead()
    {
        isDead = true;
        animator.Play(DIE_HASH, BaseLayer, 0.00f);
    }

    public void PlayIdle()
    {
        animator.Play(IDLE_HASH, BaseLayer, 0.00f);
    }
}
