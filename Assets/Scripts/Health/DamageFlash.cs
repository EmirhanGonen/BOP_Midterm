using DG.Tweening;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer = null;
    [SerializeField] private Shader flashShader = null;

    [SerializeField, ColorUsage(false, true)]
    private Color flashColor = Color.white;

    [SerializeField, Min(1.00f)] private float emissionIntensity = 2.00f;
    [SerializeField] private float flashInDuration = 0.03f;
    [SerializeField] private float flashOutDuration = 0.15f;

    private static readonly int FLASH_AMOUNT_ID = Shader.PropertyToID("_FlashAmount");
    private static readonly int FLASH_COLOR_ID = Shader.PropertyToID("_FlashColor");
    private static readonly int FLASH_INTENSITY_ID = Shader.PropertyToID("_FlashIntensity");

    private Material originalMaterial = null;
    private Material flashMaterial = null;
    private Sequence flashSequence = null;

    private void Awake()
    {
        originalMaterial = spriteRenderer.sharedMaterial;
        flashMaterial = new Material(flashShader);
        flashMaterial.SetColor(FLASH_COLOR_ID, flashColor);
        flashMaterial.SetFloat(FLASH_INTENSITY_ID, emissionIntensity);
        flashMaterial.SetFloat(FLASH_AMOUNT_ID, 0.00f);
    }

    private void OnDestroy()
    {
        flashSequence?.Kill();
        if (flashMaterial)
            Destroy(flashMaterial);
    }

    public void HandleOnHealthTakeDamage(OnTakeDamagePackage damagePackage)
    {
        PlayFlash();
    }

    public void PlayFlash()
    {
        flashSequence?.Kill();

        spriteRenderer.sharedMaterial = flashMaterial;
        flashMaterial.SetColor(FLASH_COLOR_ID, flashColor);
        flashMaterial.SetFloat(FLASH_INTENSITY_ID, emissionIntensity);

        flashSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true)
            .Append(DOTween.To(() => flashMaterial.GetFloat(FLASH_AMOUNT_ID),
                v => flashMaterial.SetFloat(FLASH_AMOUNT_ID, v), 1.00f, flashInDuration))
            .Append(DOTween.To(() => flashMaterial.GetFloat(FLASH_AMOUNT_ID),
                v => flashMaterial.SetFloat(FLASH_AMOUNT_ID, v), 0.00f, flashOutDuration).SetEase(Ease.OutQuad))
            .OnComplete(RestoreMaterial)
            .OnKill(RestoreMaterial);
    }

    private void RestoreMaterial()
    {
        if (spriteRenderer)
            spriteRenderer.sharedMaterial = originalMaterial;
    }
}