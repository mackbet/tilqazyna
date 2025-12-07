using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Random = UnityEngine.Random;

public class WordObject : MonoBehaviour
{
    [SerializeField] private GameObject letterBoxPrefab; // Префаб квадратика с буквой
    [SerializeField] private RectTransform lettersContainer; // Контейнер для букв
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AudioClip sound;
    [SerializeField] private GameObject debrisPrefab; // Префаб частиц разреза

    private RectTransform rectTransform;
    private WordData wordData;
    private Vector2 velocity;
    private float gravity = -980f; // Гравитация
    private bool isAlive = true;
    private List<GameObject> letterBoxes = new List<GameObject>();
    private Vector2? slicePoint = null; // Точка разреза в локальных координатах

    public WordData Data => wordData;
    public bool IsAlive => isAlive;
    public event Action<WordObject> OnWordMissed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (lettersContainer == null)
            lettersContainer = rectTransform;
    }

    public void Initialize(WordData data, Vector2 startPosition, Vector2 initialVelocity)
    {
        wordData = data;
        velocity = initialVelocity;
        rectTransform.anchoredPosition = startPosition;

        // Создаем квадратики для каждой буквы
        CreateLetterBoxes(data.Word);

        isAlive = true;
    }

    private void CreateLetterBoxes(string word)
    {
        // Очищаем предыдущие буквы
        foreach (var box in letterBoxes)
        {
            if (box != null)
                Destroy(box);
        }
        letterBoxes.Clear();

        if (letterBoxPrefab == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(word))
            return;

        // Создаем квадратик для каждой буквы
        // Horizontal Layout Group сам расположит их в ряд
        for (int i = 0; i < word.Length; i++)
        {
            GameObject letterBox = Instantiate(letterBoxPrefab, lettersContainer);
            letterBox.name = $"Letter_{word[i]}_{i}";

            LetterBox box = letterBox.GetComponent<LetterBox>();
            if (box == null)
            {
                box = letterBox.AddComponent<LetterBox>();
            }

            box.SetLetter(word[i]);

            letterBoxes.Add(letterBox);
        }
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

    public void Slice(bool isCorrect, Vector2 screenSlicePoint)
    {
        if (!isAlive) return;

        isAlive = false;

        // Сохраняем экранную точку разреза
        slicePoint = screenSlicePoint;

        // Создаем эффект частиц в месте разреза
        if (debrisPrefab != null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            // Конвертируем экранную точку в локальные координаты канваса
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenSlicePoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            GameObject debris = Instantiate(debrisPrefab, canvasRect);
            Destroy(debris, 5);
            RectTransform debrisRect = debris.GetComponent<RectTransform>();
            if (debrisRect != null)
            {
                debrisRect.anchoredPosition = localPoint;
            }
        }

        // Анимация разрезания
        AnimateSlice(isCorrect);

        // Воспроизводим звук слова если есть
        if (isCorrect && wordData.AudioClip != null)
        {
            AudioManager.Instance.PlaySound(wordData.AudioClip);
        }

        // Воспроизводим звук разреза с случайным pitch
        float randomPitch = Random.Range(0.6f, 1.4f);
        AudioManager.Instance.PlaySound(sound, 0.15f, false, randomPitch);
    }

    private void AnimateSlice(bool isCorrect)
    {
        if (letterBoxes.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // Определяем индекс разреза
        int sliceIndex = FindSliceIndex();

        // Создаем контейнеры для двух половинок
        GameObject leftHalf = CreateHalfContainer(true);
        GameObject rightHalf = CreateHalfContainer(false);

        // Распределяем буквы по половинкам
        for (int i = 0; i < letterBoxes.Count; i++)
        {
            if (letterBoxes[i] == null) continue;

            GameObject targetParent = i < sliceIndex ? leftHalf : rightHalf;
            letterBoxes[i].transform.SetParent(targetParent.transform, true);
        }

        // Добавляем физику падения к половинкам
        float horizontalForce = isCorrect ? 300f : 150f;
        float upwardForce = isCorrect ? 400f : 200f; // Начальная скорость вверх

        if (leftHalf != null)
        {
            FallingWordPiece fallingPiece = leftHalf.AddComponent<FallingWordPiece>();
            Vector2 initialVelocity = new Vector2(-horizontalForce, upwardForce);
            float rotationSpeed = -180f; // Вращение против часовой
            fallingPiece.Initialize(initialVelocity, rotationSpeed);
        }

        if (rightHalf != null)
        {
            FallingWordPiece fallingPiece = rightHalf.AddComponent<FallingWordPiece>();
            Vector2 initialVelocity = new Vector2(horizontalForce, upwardForce);
            float rotationSpeed = 180f; // Вращение по часовой
            fallingPiece.Initialize(initialVelocity, rotationSpeed);
        }

        // Скрываем оригинал и удаляем сразу
        canvasGroup.alpha = 0;
        Destroy(gameObject);
    }

    private int FindSliceIndex()
    {
        // Если меньше 2 букв, не можем разрезать
        if (letterBoxes.Count < 2)
        {
            return 1;
        }

        // Если точка разреза не задана, режем посередине
        if (!slicePoint.HasValue)
        {
            return letterBoxes.Count / 2;
        }

        Vector2 screenSlicePoint = slicePoint.Value;
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        int sliceIndex = letterBoxes.Count / 2; // По умолчанию - середина
        float minDistance = float.MaxValue;

        // Находим ближайшую букву к точке разреза (в экранных координатах)
        for (int i = 0; i < letterBoxes.Count; i++)
        {
            if (letterBoxes[i] == null) continue;

            RectTransform boxRect = letterBoxes[i].GetComponent<RectTransform>();
            if (boxRect == null) continue;

            // Получаем экранную позицию центра буквы
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, boxRect.position);

            // Вычисляем расстояние от точки разреза до центра буквы по X
            float distance = Mathf.Abs(screenPos.x - screenSlicePoint.x);

            if (distance < minDistance)
            {
                minDistance = distance;

                // Проверяем с какой стороны от центра находится точка
                if (screenSlicePoint.x < screenPos.x)
                    sliceIndex = i; // Режем ДО этой буквы
                else
                    sliceIndex = i + 1; // Режем ПОСЛЕ этой буквы
            }
        }

        // ВАЖНО: Проверяем что каждая часть содержит минимум одну букву
        sliceIndex = Mathf.Clamp(sliceIndex, 1, letterBoxes.Count - 1);

        return sliceIndex;
    }

    private GameObject CreateHalfContainer(bool isLeft)
    {
        GameObject half = new GameObject(isLeft ? "LeftHalf" : "RightHalf");
        half.transform.SetParent(transform.parent, false);

        RectTransform halfRect = half.AddComponent<RectTransform>();
        halfRect.anchoredPosition = rectTransform.anchoredPosition;
        halfRect.sizeDelta = rectTransform.sizeDelta;

        CanvasGroup halfGroup = half.AddComponent<CanvasGroup>();

        return half;
    }

    public bool IsPointInside(Vector2 point)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(lettersContainer, point, GetComponentInParent<Canvas>().worldCamera);
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
