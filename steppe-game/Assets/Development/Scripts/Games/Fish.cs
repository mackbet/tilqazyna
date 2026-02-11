using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class Fish : MonoBehaviour
{
    [SerializeField] private Image fishImage;
    [SerializeField] private TextMeshProUGUI nameText; // Название рыбы
    [SerializeField] private string fishType; // Тип рыбы (для идентификации)
    [SerializeField] private float moveSpeed = 2f; // Скорость движения
    [SerializeField] private AudioClip fishSound; // Звук, который издает рыба
    [SerializeField] private float soundDelay = 0.5f; // Задержка перед воспроизведением звука
    [SerializeField] private bool invertDirection; // Включить если спрайт смотрит влево

    private RectTransform rectTransform;
    private Vector2 direction; // Направление движения
    private bool isMoving = false;

    public string FishType => fishType;
    public Sprite FishSprite => fishImage != null ? fishImage.sprite : null;
    public event Action<Fish> OnFishCaught;
    public event Action<Fish> OnFishEscaped; // Когда рыба ушла за пределы экрана

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Vector2 startPosition, Vector2 moveDirection)
    {
        direction = moveDirection.normalized;

        // Устанавливаем правильное направление спрайта на основе движения
        // Зеркалим весь объект рыбы
        Vector3 scale = rectTransform.localScale;

        bool flipSprite = invertDirection ? direction.x > 0 : direction.x < 0;
        scale.x = flipSprite ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);

        rectTransform.localScale = scale;

        // Сохраняем текст читаемым (отзеркаливаем обратно, чтобы компенсировать зеркалирование родителя)
        if (nameText != null)
        {
            Vector3 textScale = nameText.transform.localScale;
            textScale.x = scale.x; // Тот же знак что и у родителя - компенсирует зеркалирование
            nameText.transform.localScale = textScale;
        }

        rectTransform.anchoredPosition = startPosition;
        isMoving = true;

        // Запускаем воспроизведение звука с задержкой
        if (fishSound != null)
        {
            StartCoroutine(PlaySoundAfterDelay());
        }
    }

    private IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(soundDelay);

        if (fishSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(fishSound);
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        // Двигаем рыбу в заданном направлении
        rectTransform.anchoredPosition += direction * moveSpeed * Time.deltaTime * 100f;

        // Проверяем выход за границы экрана
        CheckBounds();
    }

    private void CheckBounds()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        Vector2 pos = rectTransform.anchoredPosition;

        // Если рыба вышла за пределы экрана
        if (Mathf.Abs(pos.x) > canvasSize.x / 2 + 500f ||
            Mathf.Abs(pos.y) > canvasSize.y / 2 + 500f)
        {
            isMoving = false;
            OnFishEscaped?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void Catch()
    {
        isMoving = false;
        OnFishCaught?.Invoke(this);

        // Анимация пойманной рыбы
        AnimateCatch();
    }

    private void AnimateCatch()
    {
        Sequence catchSequence = DOTween.Sequence();

        // Сохраняем знак X scale чтобы рыба не переворачивалась
        Vector3 currentScale = rectTransform.localScale;
        float scaleXSign = Mathf.Sign(currentScale.x);

        catchSequence.Append(rectTransform.DOScale(new Vector3(1.2f * scaleXSign, 1.2f, 1.2f), 0.2f));
        catchSequence.Append(rectTransform.DOScale(Vector3.zero, 0.3f));

        catchSequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public Vector2 GetPosition()
    {
        return rectTransform.anchoredPosition;
    }

    private void OnDestroy()
    {
        DOTween.Kill(rectTransform);
    }
}
