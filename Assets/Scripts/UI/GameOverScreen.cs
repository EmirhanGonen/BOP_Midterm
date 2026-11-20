using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private RectTransform panel = null;
    [SerializeField, Min(0.00f)] private float showDelay = 0.80f;
    [SerializeField, Min(0.00f)] private float fadeDuration = 0.40f;

    private CanvasGroup canvasGroup = null;
    private Sequence showSequence = null;
    private bool isShown = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    private void OnDestroy()
    {
        showSequence?.Kill();
    }

    public void Show()
    {
        if (isShown)
            return;

        isShown = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        showSequence?.Kill();
        showSequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .AppendInterval(showDelay)
            .Append(canvasGroup.DOFade(1.00f, fadeDuration));

        if (panel)
        {
            panel.localScale = Vector3.one * 0.70f;
            showSequence.Join(panel.DOScale(1.00f, fadeDuration * 1.50f).SetEase(Ease.OutBack));
        }
    }

    public void Hide()
    {
        showSequence?.Kill();
        isShown = false;
        canvasGroup.alpha = 0.00f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
