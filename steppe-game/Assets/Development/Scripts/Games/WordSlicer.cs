using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class WordSlicer : GameController, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [Header("Game Objects")]
    [SerializeField] private RectTransform wordContainer; // Контейнер для слов
    [SerializeField] private RectTransform gameArea; // Игровая область
    [SerializeField] private GameObject wordPrefab; // Префаб WordObject
    [SerializeField] private GameObject trailObject; // Объект следа за касанием

    [Header("Word Data")]
    [SerializeField] private WordData[] allWords; // Все доступные слова
    [SerializeField] private WordCategory[] availableCategories; // Доступные категории для игры

    [Header("Game Settings")]
    [SerializeField] private int wordsToSlice = 10; // Сколько нужно разрезать правильных слов
    [SerializeField] private float spawnInterval = 1.5f; // Интервал спавна слов
    [SerializeField] private int maxWordsOnScreen = 5; // Максимум слов на экране
    [SerializeField] [Range(0f, 1f)] private float correctWordsProbability = 0.4f; // Вероятность спавна правильного слова (0-1)
    [SerializeField] [Range(0f, 1f)] private float spawnXMin = 0.1f; // Минимальная позиция спавна по X (0 = левый край, 1 = правый край)
    [SerializeField] [Range(0f, 1f)] private float spawnXMax = 0.9f; // Максимальная позиция спавна по X
    [SerializeField] private float launchForceMin = 800f; // Минимальная сила запуска
    [SerializeField] private float launchForceMax = 1200f; // Максимальная сила запуска
    [SerializeField] private float launchAngleMin = 60f; // Минимальный угол запуска
    [SerializeField] private float launchAngleMax = 120f; // Максимальный угол запуска

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText; // "Разрезано: 5/10"
    [SerializeField] private TextMeshProUGUI categoryText; // "Категория: Животные"

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSliceSound;
    [SerializeField] private AudioClip wrongSliceSound;
    [SerializeField] private AudioClip missSound;

    private WordCategory targetCategory; // Целевая категория
    private List<WordData> targetCategoryWords = new List<WordData>(); // Слова целевой категории
    private int slicedCount = 0; // Количество разрезанных правильных слов
    private List<WordObject> activeWords = new List<WordObject>();
    private Coroutine spawnCoroutine;
    private bool isGameActive = false;

    // Свайп
    private Vector2 swipeStart;
    private Vector2 lastSwipePosition;
    private bool isSwiping = false;
    private Camera mainCamera;
    private Canvas canvas;
    private GameObject activeTrailObject;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        slicedCount = 0;
        SetLives(3);
        activeWords.Clear();
        isGameActive = true;

        // Выбираем случайную категорию
        SelectRandomCategory();

        UpdateUI();

        // Запускаем спавн слов
        spawnCoroutine = StartCoroutine(SpawnWordRoutine());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isGameActive) return;

        swipeStart = eventData.position;
        lastSwipePosition = eventData.position;
        isSwiping = true;

        // Создаем объект следа
        if (trailObject != null)
        {
            activeTrailObject = Instantiate(trailObject, transform);
            UpdateTrailPosition(eventData.position);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isGameActive || !isSwiping) return;

        // Проверяем пересечение с словами от предыдущей позиции до текущей
        CheckSwipe(lastSwipePosition, eventData.position);
        lastSwipePosition = eventData.position;

        // Обновляем позицию объекта следа
        if (activeTrailObject != null)
        {
            UpdateTrailPosition(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isGameActive || !isSwiping) return;

        // Проверяем последний сегмент (от последней позиции до конца)
        CheckSwipe(lastSwipePosition, eventData.position);
        isSwiping = false;

        // Удаляем след через 1 секунду (чтобы отработал Color over Lifetime)
        if (activeTrailObject != null)
        {
            StartCoroutine(DestroyTrailAfterDelay(activeTrailObject, 1f));
            activeTrailObject = null;
        }
    }

    private void UpdateTrailPosition(Vector2 screenPosition)
    {
        if (activeTrailObject == null) return;

        // Конвертируем экранные координаты в локальные координаты канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out localPoint
        );

        // Устанавливаем позицию объекта следа
        RectTransform trailRect = activeTrailObject.GetComponent<RectTransform>();
        if (trailRect != null)
        {
            trailRect.anchoredPosition = localPoint;
        }
    }

    private IEnumerator DestroyTrailAfterDelay(GameObject trail, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (trail != null)
        {
            Destroy(trail);
        }
    }

    private void SelectRandomCategory()
    {
        if (availableCategories == null || availableCategories.Length == 0) return;

        // Выбираем случайную категорию
        targetCategory = availableCategories[Random.Range(0, availableCategories.Length)];

        // Выписываем все слова этой категории (только одиночные слова, без пробелов)
        targetCategoryWords.Clear();
        if (allWords != null && targetCategory != null)
        {
            foreach (WordData word in allWords)
            {
                if (word.HasCategory(targetCategory) && !word.Word.Contains(" "))
                {
                    targetCategoryWords.Add(word);
                }
            }
        }

        if (categoryText != null && targetCategory != null)
        {
            categoryText.text = $"Категория: {targetCategory.CategoryName}";
        }
    }

    private void CheckSwipe(Vector2 start, Vector2 end)
    {
        // Проверяем все активные слова на пересечение с линией свайпа
        List<(WordObject word, Vector2 slicePoint)> wordsToSlice = new List<(WordObject, Vector2)>();

        foreach (var word in activeWords)
        {
            if (!word.IsAlive) continue;

            // Проверяем пересечение линии с объектом
            Vector2? intersectionPoint = IsSwipeIntersectingWord(start, end, word);
            if (intersectionPoint.HasValue)
            {
                wordsToSlice.Add((word, intersectionPoint.Value));
            }
        }

        // Разрезаем все найденные слова
        foreach (var (word, slicePoint) in wordsToSlice)
        {
            SliceWord(word, slicePoint);
        }
    }

    private Vector2? IsSwipeIntersectingWord(Vector2 start, Vector2 end, WordObject word)
    {
        // Вычисляем количество шагов в зависимости от длины свайпа
        // Чем длиннее свайп, тем больше точек проверяем
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.Max(5, Mathf.CeilToInt(distance / 5f)); // Минимум 5 шагов, по 1 шагу на каждые 5 пикселей

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            if (word.IsPointInside(point))
            {
                return point; // Возвращаем точку пересечения
            }
        }

        return null; // Пересечения нет
    }

    private void SliceWord(WordObject word, Vector2 slicePoint)
    {
        if (!word.IsAlive) return;

        bool isCorrect = word.Data.HasCategory(targetCategory);

        if (isCorrect)
        {
            // Правильное слово
            slicedCount++;

            if (correctSliceSound != null)
            {
                AudioManager.Instance.PlaySound(correctSliceSound);
            }

            UpdateUI();

            // Проверяем победу
            if (slicedCount >= wordsToSlice)
            {
                StartCoroutine(WinGame());
            }
        }
        else
        {
            // Неправильное слово - теряем жизнь
            SetLives(lives - 1);

            if (wrongSliceSound != null)
            {
                AudioManager.Instance.PlaySound(wrongSliceSound);
            }

            // Тряска экрана при ошибке
            CameraShake.Instance.Shake(gameArea, 80f, 0.3f, 1.05f);

            UpdateUI();

            // Проверяем проигрыш
            if (lives <= 0)
            {
                StartCoroutine(LoseGame());
            }
        }

        word.Slice(isCorrect, slicePoint);
        activeWords.Remove(word);
    }

    private IEnumerator SpawnWordRoutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (isGameActive && activeWords.Count < maxWordsOnScreen)
            {
                SpawnWord();
            }
        }
    }

    private void SpawnWord()
    {
        if (allWords == null || allWords.Length == 0 || wordPrefab == null)
            return;

        // Выбираем слово по процентному соотношению
        WordData wordData = null;
        float randomValue = Random.value; // 0.0 - 1.0

        if (randomValue < correctWordsProbability && targetCategoryWords.Count > 0)
        {
            // Спавним правильное слово из списка целевой категории
            wordData = targetCategoryWords[Random.Range(0, targetCategoryWords.Count)];
        }
        else
        {
            // Спавним случайное слово из общего списка (без пробелов)
            int attempts = 0;
            while (wordData == null && attempts < 50)
            {
                WordData candidate = allWords[Random.Range(0, allWords.Length)];
                if (!candidate.Word.Contains(" "))
                {
                    wordData = candidate;
                }
                attempts++;
            }

            // Если не нашли подходящее слово, берем первое без пробела
            if (wordData == null)
            {
                foreach (WordData w in allWords)
                {
                    if (!w.Word.Contains(" "))
                    {
                        wordData = w;
                        break;
                    }
                }
            }
        }

        // Если так и не нашли подходящее слово, выходим
        if (wordData == null)
            return;

        // Получаем размеры канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Случайная X позиция (от 0 до 1, где 0 = левый край, 1 = правый край)
        float normalizedX = Random.Range(spawnXMin, spawnXMax);
        // Конвертируем в координаты канваса (-width/2 до +width/2)
        float randomX = Mathf.Lerp(-canvasSize.x / 2f, canvasSize.x / 2f, normalizedX);
        Vector2 spawnPosition = new Vector2(randomX, -canvasSize.y / 2f - 100f);

        // Случайный угол и сила запуска
        float angle = Random.Range(launchAngleMin, launchAngleMax);
        float force = Random.Range(launchForceMin, launchForceMax);

        Vector2 velocity = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad) * force,
            Mathf.Sin(angle * Mathf.Deg2Rad) * force
        );

        // Создаем слово
        GameObject wordObj = Instantiate(wordPrefab, wordContainer);
        WordObject word = wordObj.GetComponent<WordObject>();

        if (word != null)
        {
            word.Initialize(wordData, spawnPosition, velocity);
            word.OnWordMissed += OnWordMissed;
            activeWords.Add(word);
        }
    }

    private void OnWordMissed(WordObject word)
    {
        // Слово улетело за пределы экрана
        // Можно воспроизвести звук если нужно
        if (word.Data.HasCategory(targetCategory) && missSound != null)
        {
            AudioManager.Instance.PlaySound(missSound);
        }

        activeWords.Remove(word);
    }

    private IEnumerator WinGame()
    {
        isGameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        yield return new WaitForSeconds(1f);

        FinishGame();
    }

    private IEnumerator LoseGame()
    {
        isGameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        yield return new WaitForSeconds(1f);

        // Можно вызвать метод проигрыша или перезапуска
        FinishGame();
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"{slicedCount}/{wordsToSlice}";
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        isGameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Очищаем активные слова
        foreach (var word in activeWords)
        {
            if (word != null)
            {
                Destroy(word.gameObject);
            }
        }

        activeWords.Clear();
    }
}
