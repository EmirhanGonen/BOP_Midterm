using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image backFillImage = null, frontFillImage = null;
    [SerializeField] private float backFillDelay = 0.30f;
    [SerializeField] private float backFillDuration = 0.50f;

    private void Awake()
    {
        frontFillImage.fillAmount = 1.00f;
        backFillImage.fillAmount = 1.00f;
    }

    private void OnDestroy()
    {
        backFillImage.DOKill();
    }

    public void HandleOnHealthTakeDamage(OnTakeDamagePackage damagePackage)
    {
        float fillPercentage = damagePackage.GetMaxHealth() > 0
            ? Mathf.Clamp01((float)damagePackage.GetCurrentHealth() / damagePackage.GetMaxHealth())
            : 0.00f;

        frontFillImage.fillAmount = fillPercentage;

        backFillImage.DOKill();
        backFillImage.DOFillAmount(fillPercentage, backFillDuration)
            .SetDelay(backFillDelay)
            .SetEase(Ease.OutQuad);
    }
}
