using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image backFillImage = null, frontFillImage = null;
    [SerializeField] private float backFillDelay = 0.30f;
    [SerializeField] private float backFillDuration = 0.50f;
    [SerializeField] private MMF_Player damageFeedback = null;
    [SerializeField] private TMP_Text healthText = null;
    [SerializeField] private Health health = null;
    [SerializeField, Min(0.00f)] private float textCountDuration = 0.50f;

    private float displayedHealth = 0.00f;
    private Tween textTween = null;

    private void Awake()
    {
        frontFillImage.fillAmount = 1.00f;
        backFillImage.fillAmount = 1.00f;

        if (!health)
            health = GetComponentInParent<Health>();
    }

    private void Start()
    {
        displayedHealth = health.CurrentHealth;
        SetText(health.CurrentHealth, health.MaxHealth);
    }

    private void SetText(int currentHealth, int maxHealth)
    {
        healthText.text = $"{currentHealth}/{maxHealth}";
    }

    private void AnimateText(int targetHealth, int maxHealth)
    {
        textTween?.Kill();
        textTween = DOTween.To(() => displayedHealth, value =>
            {
                displayedHealth = value;
                SetText(Mathf.RoundToInt(displayedHealth), maxHealth);
            }, targetHealth, textCountDuration)
            .SetDelay(backFillDelay)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() => SetText(targetHealth, maxHealth));
    }

    private void OnDestroy()
    {
        backFillImage.DOKill();
        textTween?.Kill();
    }

    public void HandleOnHealthTakeDamage(OnTakeDamagePackage damagePackage)
    {
        float fillPercentage = damagePackage.GetMaxHealth() > 0
            ? Mathf.Clamp01((float)damagePackage.GetCurrentHealth() / damagePackage.GetMaxHealth())
            : 0.00f;

        frontFillImage.fillAmount = fillPercentage;
        AnimateText(damagePackage.GetCurrentHealth(), damagePackage.GetMaxHealth());

        if (damageFeedback)
            damageFeedback.PlayFeedbacks();

        backFillImage.DOKill();
        backFillImage.DOFillAmount(fillPercentage, backFillDuration)
            .SetDelay(backFillDelay)
            .SetEase(Ease.OutQuad);
    }
}