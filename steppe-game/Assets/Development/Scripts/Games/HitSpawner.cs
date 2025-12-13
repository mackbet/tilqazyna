using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class HitSpawner : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button targetButton;
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private Image targetImage;
    [SerializeField] private TextMeshProUGUI wordText;

    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.3f;
    [SerializeField] private float despawnDuration = 0.3f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    public event Action<HitSpawner, Vector2> OnTargetHit; // Передаем спавнер и позицию клика

    private Vector3 originalScale;
    private Vector2 hiddenPosition;
    private Sequence currentSequence;
    private bool isTargetActive = false;
    private WordData currentWord;

    private void Awake()
    {
        if (targetTransform != null)
        {
            originalScale = targetTransform.localScale;
            hiddenPosition = targetTransform.anchoredPosition;
        }

        Deactivate();
    }

    public void SpawnTargetFor(float duration, WordData word)
    {
        if (isTargetActive)
            DespawnTarget();

        currentWord = word;
        isTargetActive = true;
        currentSequence?.Kill();

        // Устанавливаем текст слова
        if (wordText != null && word != null)
        {
            wordText.text = word.Word;
        }

        if (targetImage != null)
            targetImage.raycastTarget = true;

        currentSequence = DOTween.Sequence()
            .Append(targetTransform.DOScale(originalScale, spawnDuration).SetEase(spawnEase))
            .Join(targetTransform.DOAnchorPosY(0, spawnDuration).SetEase(spawnEase))
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(duration, () =>
                {
                    if (isTargetActive)
                        DespawnTarget();
                });
            });
    }

    public WordData GetCurrentWord() => currentWord;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isTargetActive)
        {
            OnTargetHit?.Invoke(this, eventData.position);
            DespawnTarget();
        }
    }

    private void DespawnTarget()
    {
        if (!isTargetActive) return;

        isTargetActive = false;

        if (targetImage != null)
            targetImage.raycastTarget = false;

        currentSequence?.Kill();

        currentSequence = DOTween.Sequence()
            .Append(targetTransform.DOScale(Vector3.zero, despawnDuration).SetEase(despawnEase))
            .Join(targetTransform.DOAnchorPosY(hiddenPosition.y, despawnDuration).SetEase(despawnEase));
    }

    public void Deactivate()
    {
        isTargetActive = false;

        if (targetImage != null)
            targetImage.raycastTarget = false;

        currentSequence?.Kill();

        if (targetTransform != null)
        {
            targetTransform.localScale = Vector3.zero;
            targetTransform.anchoredPosition = hiddenPosition;
        }
    }

    private void OnDisable()
    {
        Deactivate();
    }
    
    public bool IsActive() => isTargetActive;
}