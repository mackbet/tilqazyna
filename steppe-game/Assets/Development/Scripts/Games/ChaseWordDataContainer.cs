using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

public class WordContainer : CustomButton
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float speed = 300f;
    [SerializeField] private Color clickedColor;
    [SerializeField] private AudioClip clickedSound;
    [SerializeField] private ParticleSystem clickedEffect;
    
    [Header("Animated Object")]
    [SerializeField] private RectTransform animatedObject; // Иконка пальца
    [SerializeField] private float scaleMin = 0.9f;
    [SerializeField] private float scaleMax = 1.1f;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float disappearDuration = 0.3f;
    
    private bool isClicked = false;
    private WordData wordData;
    private bool isMoving = false;
    private Action<WordContainer> onOutOfBounds;
    private Sequence scaleSequence;
    
    public event Action<WordContainer> OnWordClicked;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(WordData data, Vector2 startPosition, float moveSpeed, System.Action<WordContainer> onOutOfBoundsCallback = null)
    {
        wordData = data;
        speed = moveSpeed;
        onOutOfBounds = onOutOfBoundsCallback;

        if (textComponent != null && wordData != null)
            textComponent.text = wordData.Word;

        if (rectTransform != null)
            rectTransform.anchoredPosition = startPosition;

        isMoving = true;
        isClicked = false;

        // Показываем и запускаем анимацию иконки пальца
        if (animatedObject != null)
        {
            animatedObject.gameObject.SetActive(true);
            animatedObject.localScale = Vector3.one;
            StartScaleAnimation();
        }

        // Воспроизводим звук, если есть
        if (wordData != null && wordData.AudioClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(wordData.AudioClip);
    }

    private void StartScaleAnimation()
    {
        if (animatedObject == null) return;
        
        // Останавливаем предыдущую анимацию, если она есть
        scaleSequence?.Kill();
        
        // Создаем бесконечную анимацию пульсации для иконки пальца
        scaleSequence = DOTween.Sequence();
        scaleSequence.Append(animatedObject.DOScale(scaleMax, scaleDuration).SetEase(Ease.InOutSine));
        scaleSequence.Append(animatedObject.DOScale(scaleMin, scaleDuration).SetEase(Ease.InOutSine));
        scaleSequence.SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        if (!isMoving || rectTransform == null) return;

        // Двигаем слово влево
        Vector2 pos = rectTransform.anchoredPosition;
        pos.x -= speed * Time.deltaTime;
        rectTransform.anchoredPosition = pos;

        // Проверяем выход за границы
        float screenLeft = -Screen.width / 2f;
        if (pos.x < screenLeft - 200f)
        {
            isMoving = false;
            onOutOfBounds?.Invoke(this);
        }
    }

    public void Stop()
    {
        isMoving = false;
        scaleSequence?.Kill();
    }

    protected override void Clicked()
    {
        if (isClicked) return;
        
        base.Clicked();
        isClicked = true;
        // НЕ останавливаем движение слова - оно продолжает двигаться

        // Останавливаем анимацию пульсации иконки пальца
        scaleSequence?.Kill();

        // Меняем цвет текста
        if (textComponent != null)
            textComponent.color = clickedColor;

        // Воспроизводим звук
        if (AudioManager.Instance != null)
        {
            AudioClip soundToPlay = wordData?.AudioClip != null ? wordData.AudioClip : clickedSound;
            if (soundToPlay != null)
                AudioManager.Instance.PlaySound(soundToPlay);
        }

        // Воспроизводим эффект частиц
        if (clickedEffect != null)
            clickedEffect.Play();

        // Уведомляем подписчиков
        OnWordClicked?.Invoke(this);

        // Запускаем анимацию исчезновения только иконки пальца
        DisappearAnimatedObject();
    }

    private void DisappearAnimatedObject()
    {
        if (animatedObject == null) return;
        
        // Анимация уменьшения и исчезновения иконки пальца
        animatedObject.DOScale(Vector3.zero, disappearDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                animatedObject.gameObject.SetActive(false);
            });
    }

    private void OnDestroy()
    {
        // Очищаем все tweens при уничтожении объекта
        scaleSequence?.Kill();
        animatedObject?.DOKill();
    }

    public WordData GetWordData() => wordData;
    public Vector2 GetPosition() => rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
}