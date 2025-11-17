using System;
using UnityEngine;
using DG.Tweening;

public class HitSpawner : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.3f;
    [SerializeField] private float despawnDuration = 0.3f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    [Header("Collision Settings")]
    [SerializeField] private float collisionDelay = 0.2f;

    [Header("Visual Settings")]
    [SerializeField] private Transform visualTransform; // Визуальная часть цели

    public event Action OnTargetHit;

    private Vector3 originalScale;
    private Tween currentTween;
    private Tween despawnTween;
    private bool isTargetActive = false;
    private bool canBeHit = false;
    private Collider2D targetCollider;

    private void Awake()
    {
        if (visualTransform == null)
            visualTransform = transform;

        originalScale = visualTransform.localScale;
        targetCollider = GetComponent<Collider2D>();
        
        // Скрываем цель изначально
        visualTransform.localScale = Vector3.zero;
        
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }
    }

    public void SpawnTargetFor(float duration)
    {
        // Если уже есть активная цель, удаляем её
        if (isTargetActive)
        {
            DespawnTarget();
        }

        isTargetActive = true;
        canBeHit = false;

        // Останавливаем предыдущие анимации
        currentTween?.Kill();
        despawnTween?.Kill();

        // Анимация появления
        currentTween = visualTransform.DOScale(originalScale, spawnDuration)
            .SetEase(spawnEase)
            .OnComplete(() =>
            {
                // Включаем возможность попадания после задержки
                DOVirtual.DelayedCall(collisionDelay, () =>
                {
                    if (isTargetActive)
                    {
                        canBeHit = true;
                        if (targetCollider != null)
                        {
                            targetCollider.enabled = true;
                        }
                    }
                });

                // Автоматически удаляем цель через заданное время
                DOVirtual.DelayedCall(duration, () =>
                {
                    if (isTargetActive)
                    {
                        DespawnTarget();
                    }
                });
            });
    }

    private void OnMouseDown()
    {
        if (canBeHit && isTargetActive)
        {
            Hit();
        }
    }

    // Для мобильных устройств
    private void OnTouchDown()
    {
        if (canBeHit && isTargetActive)
        {
            Hit();
        }
    }

    private void Hit()
    {
        OnTargetHit?.Invoke();
        DespawnTarget();
    }

    private void DespawnTarget()
    {
        if (!isTargetActive) return;

        isTargetActive = false;
        canBeHit = false;

        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        // Останавливаем текущую анимацию
        currentTween?.Kill();

        // Анимация исчезновения
        despawnTween = visualTransform.DOScale(Vector3.zero, despawnDuration)
            .SetEase(despawnEase);
    }

    public void Deactivate()
    {
        isTargetActive = false;
        canBeHit = false;
        
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        currentTween?.Kill();
        despawnTween?.Kill();
        
        visualTransform.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        Deactivate();
    }
}