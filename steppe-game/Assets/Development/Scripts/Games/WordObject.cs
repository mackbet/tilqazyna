using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WordObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private RectTransform wordContainer;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private WordData wordData;
    private Vector2 velocity;
    private float gravity = -980f; // Гравитация
    private bool isAlive = true;

    public WordData Data => wordData;
    public bool IsAlive => isAlive;
    public event Action<WordObject> OnWordMissed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(WordData data, Vector2 startPosition, Vector2 initialVelocity)
    {
        wordData = data;
        velocity = initialVelocity;
        rectTransform.anchoredPosition = startPosition;

        if (wordText != null)
        {
            wordText.text = data.Word;
        }

        isAlive = true;
    }

    private void Update()
    {
        if (!isAlive) return;

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;

        // Двигаем слово
        rectTransform.anchoredPosition += velocity * Time.deltaTime;

        // Проверяем выход за границы экрана (упало вниз)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            float bottomBound = -canvasRect.sizeDelta.y / 2f - 200f;

            if (rectTransform.anchoredPosition.y < bottomBound)
            {
                isAlive = false;
                OnWordMissed?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }

    public void Slice(bool isCorrect)
    {
        if (!isAlive) return;

        isAlive = false;

        // Анимация разрезания
        AnimateSlice(isCorrect);

        // Воспроизводим звук слова если есть
        if (isCorrect && wordData.AudioClip != null)
        {
            AudioManager.Instance.PlaySound(wordData.AudioClip);
        }
    }

    private void AnimateSlice(bool isCorrect)
    {
        // Создаем две половинки
        GameObject leftHalf = CreateHalf(true);
        GameObject rightHalf = CreateHalf(false);

        // Анимируем разлет половинок
        float force = isCorrect ? 300f : 150f;

        if (leftHalf != null)
        {
            RectTransform leftRect = leftHalf.GetComponent<RectTransform>();
            leftRect.DOAnchorPos(leftRect.anchoredPosition + new Vector2(-force, 0), 0.5f);
            leftRect.DORotate(new Vector3(0, 0, -180f), 0.5f);
            leftHalf.GetComponent<CanvasGroup>()?.DOFade(0, 0.5f).OnComplete(() => Destroy(leftHalf));
        }

        if (rightHalf != null)
        {
            RectTransform rightRect = rightHalf.GetComponent<RectTransform>();
            rightRect.DOAnchorPos(rightRect.anchoredPosition + new Vector2(force, 0), 0.5f);
            rightRect.DORotate(new Vector3(0, 0, 180f), 0.5f);
            rightHalf.GetComponent<CanvasGroup>()?.DOFade(0, 0.5f).OnComplete(() => Destroy(rightHalf));
        }

        // Скрываем оригинал
        canvasGroup.alpha = 0;
        Destroy(gameObject, 0.6f);
    }

    private GameObject CreateHalf(bool isLeft)
    {
        GameObject half = new GameObject(isLeft ? "LeftHalf" : "RightHalf");
        half.transform.SetParent(transform.parent, false);

        RectTransform halfRect = half.AddComponent<RectTransform>();
        halfRect.anchoredPosition = rectTransform.anchoredPosition;
        halfRect.sizeDelta = new Vector2(rectTransform.sizeDelta.x / 2f, rectTransform.sizeDelta.y);

        CanvasGroup halfGroup = half.AddComponent<CanvasGroup>();

        TextMeshProUGUI halfText = half.AddComponent<TextMeshProUGUI>();
        halfText.text = wordText.text;
        halfText.font = wordText.font;
        halfText.fontSize = wordText.fontSize;
        halfText.color = wordText.color;
        halfText.alignment = wordText.alignment;

        // Обрезаем текст маской
        UnityEngine.UI.RectMask2D mask = half.AddComponent<UnityEngine.UI.RectMask2D>();

        // Сдвигаем половинку
        if (isLeft)
        {
            halfRect.pivot = new Vector2(1, 0.5f);
            halfRect.anchorMin = new Vector2(0.5f, 0.5f);
            halfRect.anchorMax = new Vector2(0.5f, 0.5f);
        }
        else
        {
            halfRect.pivot = new Vector2(0, 0.5f);
            halfRect.anchorMin = new Vector2(0.5f, 0.5f);
            halfRect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        return half;
    }

    public bool IsPointInside(Vector2 point)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, point, GetComponentInParent<Canvas>().worldCamera);
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
