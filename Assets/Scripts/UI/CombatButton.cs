using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;

public class CombatButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PlayerCombat playerCombat = null;
    [SerializeField] private CombatData combatData = null;

    [SerializeField] private Transform scaleTarget = null;
    [SerializeField, Min(1.00f)] private float hoverScale = 1.10f;
    [SerializeField, Min(0.00f)] private float hoverDuration = 0.15f;
    [SerializeField] private TextMeshProUGUI skillNameText = null;
    [SerializeField] private Image skillIconImage = null;

    [SerializeField] private Image durationFill = null;

    private Button combatButton = null;
    private Vector3 baseScale = Vector3.one;
    private Tween scaleTween = null;
    private Tween fillTween = null;

    private void Awake()
    {
        combatButton = GetComponent<Button>();
        combatButton.onClick.AddListener(CombatAction);
        
        skillNameText.SetText(combatData.GetSkillName());
        skillIconImage.sprite = combatData.GetSkillIcon();

        if (!scaleTarget)
            scaleTarget = transform.parent ? transform.parent : transform;
        baseScale = scaleTarget.localScale;

        if (durationFill)
            durationFill.fillAmount = 0.00f;
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
        fillTween?.Kill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(baseScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(baseScale);
    }

    private void CombatAction()
    {
        if (!combatButton.interactable || !playerCombat.TryAttack(combatData))
            return;

        RunCooldown().Forget();
    }

    private async UniTaskVoid RunCooldown()
    {
        combatButton.interactable = false;
        fillTween?.Kill();

        if (durationFill)
            durationFill.fillAmount = 1.00f;

        bool cancelled = await UniTask.WaitWhile(() => playerCombat.IsAttacking, cancellationToken: this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();
        
        if (cancelled)
            return;

        if (!durationFill)
        {
            combatButton.interactable = true;
            return;
        }

        fillTween = durationFill.DOFillAmount(0.00f, combatData.GetCooldown())
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() => combatButton.interactable = true);
    }

    private void ScaleTo(Vector3 target)
    {
        scaleTween?.Kill();
        scaleTween = scaleTarget.DOScale(target, hoverDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}