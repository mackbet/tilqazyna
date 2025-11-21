using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HitSpawner : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button targetButton;
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private Image targetImage;

    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.3f;
    [SerializeField] private float despawnDuration = 0.3f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    public event Action<Vector2> OnTargetHit; // Теперь передаем позицию клика

    private Vector3 originalScale;
    private Vector2 hiddenPosition;
    private Sequence currentSequence;
    private bool isTargetActive = false;

    private void Awake()
    {
        if (targetTransform != null)
        {
            originalScale = targetTransform.localScale;
            hiddenPosition = targetTransform.anchoredPosition;
        }

        Deactivate();
    }

    public void SpawnTargetFor(float duration)
    {
        if (isTargetActive)
            DespawnTarget();

        isTargetActive = true;
        currentSequence?.Kill();

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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isTargetActive)
        {
            OnTargetHit?.Invoke(eventData.position);
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