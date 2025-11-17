using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button targetButton;
    [SerializeField] private RectTransform targetTransform; // Target (Image)
    [SerializeField] private Image targetImage;

    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.3f;
    [SerializeField] private float despawnDuration = 0.3f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    [Header("Collision Settings")]
    [SerializeField] private float collisionDelay = 0.2f;

    public event Action OnTargetHit;

    private Vector3 originalScale;
    private Tween currentTween;
    private Tween despawnTween;
    private bool isTargetActive = false;
    private bool canBeHit = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!targetButton)
            targetButton = GetComponent<Button>();

        if (!targetTransform && transform.childCount > 0)
        {
            // Ищем дочерний объект с именем Target
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Target"))
                {
                    targetTransform = child.GetComponent<RectTransform>();
                    break;
                }
            }
        }

        if (targetTransform && !targetImage)
            targetImage = targetTransform.GetComponent<Image>();
    }
#endif

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetTransform == null && transform.childCount > 0)
        {
            targetTransform = transform.GetChild(0).GetComponent<RectTransform>();
        }

        if (targetImage == null && targetTransform != null)
            targetImage = targetTransform.GetComponent<Image>();

        if (targetTransform != null)
            originalScale = targetTransform.localScale;

        // Подписываемся на событие нажатия кнопки
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }

        // Скрываем цель изначально
        Deactivate();
    }

    private void OnDestroy()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClicked);
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

        // Поднимаем Target из родителя
        if (targetTransform != null)
        {
            targetTransform.SetAsLastSibling();
        }

        // Делаем Image видимым но пока не кликабельным
        if (targetImage != null)
        {
            targetImage.raycastTarget = false;
        }

        // Анимация появления
        currentTween = targetTransform.DOScale(originalScale, spawnDuration)
            .SetEase(spawnEase)
            .OnComplete(() =>
            {
                // Включаем возможность попадания после задержки
                DOVirtual.DelayedCall(collisionDelay, () =>
                {
                    if (isTargetActive)
                    {
                        canBeHit = true;
                        if (targetImage != null)
                        {
                            targetImage.raycastTarget = true;
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

    private void OnButtonClicked()
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

        if (targetImage != null)
        {
            targetImage.raycastTarget = false;
        }

        // Останавливаем текущую анимацию
        currentTween?.Kill();

        // Анимация исчезновения
        despawnTween = targetTransform.DOScale(Vector3.zero, despawnDuration)
            .SetEase(despawnEase);
    }

    public void Deactivate()
    {
        isTargetActive = false;
        canBeHit = false;

        if (targetImage != null)
        {
            targetImage.raycastTarget = false;
        }

        currentTween?.Kill();
        despawnTween?.Kill();

        if (targetTransform != null)
        {
            targetTransform.localScale = Vector3.zero;
        }
    }

    private void OnDisable()
    {
        Deactivate();
    }

    public bool IsActive() => isTargetActive;
    public bool CanBeHit() => canBeHit;
}