using UnityEngine;
using TMPro;
using System;

public class WordContainer : CustomButton
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float speed = 300f;
    [SerializeField] private Color clickedColor;
    [SerializeField] private AudioClip clickedSound;
    [SerializeField] private ParticleSystem clickedEffect;
    private bool isClicked = false;

    private WordData wordData;
    private bool isMoving = false;
    private Action<WordContainer> onOutOfBounds;
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

        // Воспроизводим звук, если есть
        if (wordData != null && wordData.AudioClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(wordData.AudioClip);
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
    }

    protected override void Clicked()
    {
        base.Clicked();

        AudioManager.Instance.PlaySound(wordData.AudioClip);

        if (!isClicked)
        {
            isClicked = true;

            textComponent.color = clickedColor;

            AudioManager.Instance.PlaySound(wordData.AudioClip ? wordData.AudioClip : clickedSound);
            clickedEffect.Play();
            OnWordClicked?.Invoke(this);
        }
    }
    public WordData GetWordData() => wordData;
    public Vector2 GetPosition() => rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
}