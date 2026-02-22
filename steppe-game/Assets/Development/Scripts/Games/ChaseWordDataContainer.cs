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
    private string wordText;
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

    public void Initialize(WordData data, Vector2 startPosition, float moveSpeed, Action<WordContainer> onOutOfBoundsCallback = null)
    {
        wordData = data;
        wordText = data != null ? data.Word : "";
        speed = moveSpeed;
        onOutOfBounds = onOutOfBoundsCallback;

        if (textComponent != null)
            textComponent.text = wordText;

        if (rectTransform != null)
            rectTransform.anchoredPosition = startPosition;

        isMoving = true;
        isClicked = false;

        if (animatedObject != null)
        {
            animatedObject.gameObject.SetActive(true);
            animatedObject.localScale = Vector3.one;
            StartScaleAnimation();
        }

        if (wordData != null && wordData.AudioClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(wordData.AudioClip);
    }

    public void Initialize(string word, Vector2 startPosition, float moveSpeed, Action<WordContainer> onOutOfBoundsCallback = null)
    {
        wordData = null;
        wordText = word;
        speed = moveSpeed;
        onOutOfBounds = onOutOfBoundsCallback;

        if (textComponent != null)
            textComponent.text = wordText;

        if (rectTransform != null)
            rectTransform.anchoredPosition = startPosition;

        isMoving = true;
        isClicked = false;

        if (animatedObject != null)
        {
            animatedObject.gameObject.SetActive(true);
            animatedObject.localScale = Vector3.one;
            StartScaleAnimation();
        }
    }

    private void StartScaleAnimation()
    {
        if (animatedObject == null) return;

        scaleSequence?.Kill();

        scaleSequence = DOTween.Sequence();
        scaleSequence.Append(animatedObject.DOScale(scaleMax, scaleDuration).SetEase(Ease.InOutSine));
        scaleSequence.Append(animatedObject.DOScale(scaleMin, scaleDuration).SetEase(Ease.InOutSine));
        scaleSequence.SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        if (!isMoving || rectTransform == null) return;

        Vector2 pos = rectTransform.anchoredPosition;
        pos.x -= speed * Time.deltaTime;
        rectTransform.anchoredPosition = pos;

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

        scaleSequence?.Kill();

        if (textComponent != null)
            textComponent.color = clickedColor;

        if (AudioManager.Instance != null)
        {
            AudioClip soundToPlay = wordData?.AudioClip != null ? wordData.AudioClip : clickedSound;
            if (soundToPlay != null)
                AudioManager.Instance.PlaySound(soundToPlay);
        }

        if (clickedEffect != null)
            clickedEffect.Play();

        OnWordClicked?.Invoke(this);

        DisappearAnimatedObject();
    }

    private void DisappearAnimatedObject()
    {
        if (animatedObject == null) return;

        animatedObject.DOScale(Vector3.zero, disappearDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                animatedObject.gameObject.SetActive(false);
            });
    }

    private void OnDestroy()
    {
        scaleSequence?.Kill();
        animatedObject?.DOKill();
    }

    public string GetWordText() => wordText;
    public WordData GetWordData() => wordData;
    public bool IsClicked => isClicked;
    public Vector2 GetPosition() => rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
}
