using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordSlicer : GameController, IPointerDownHandler, IPointerUpHandler
{
    [Header("Game Objects")]
    [SerializeField] private RectTransform wordContainer; // Контейнер для слов
    [SerializeField] private RectTransform gameArea; // Игровая область
    [SerializeField] private GameObject wordPrefab; // Префаб WordObject

    [Header("Word Data")]
    [SerializeField] private WordData[] allWords; // Все доступные слова
    [SerializeField] private WordCategory[] availableCategories; // Доступные категории для игры

    [Header("Game Settings")]
    [SerializeField] private int wordsToSlice = 10; // Сколько нужно разрезать правильных слов
    [SerializeField] private float spawnInterval = 1.5f; // Интервал спавна слов
    [SerializeField] private int maxWordsOnScreen = 5; // Максимум слов на экране
    [SerializeField] private float launchForceMin = 800f; // Минимальная сила запуска
    [SerializeField] private float launchForceMax = 1200f; // Максимальная сила запуска
    [SerializeField] private float launchAngleMin = 60f; // Минимальный угол запуска
    [SerializeField] private float launchAngleMax = 120f; // Максимальный угол запуска

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText; // "Разрезано: 5/10"
    [SerializeField] private TextMeshProUGUI categoryText; // "Категория: Животные"
    [SerializeField] private TextMeshProUGUI livesText; // "Жизни: 3"

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSliceSound;
    [SerializeField] private AudioClip wrongSliceSound;
    [SerializeField] private AudioClip missSound;

    private WordCategory targetCategory; // Целевая категория
    private int slicedCount = 0; // Количество разрезанных правильных слов
    private int lives = 3; // Количество жизней
    private List<WordObject> activeWords = new List<WordObject>();
    private Coroutine spawnCoroutine;
    private bool isGameActive = false;

    // Свайп
    private Vector2 swipeStart;
    private bool isSwiping = false;
    private Camera mainCamera;
    private Canvas canvas;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        slicedCount = 0;
        lives = 3;
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
        isSwiping = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isGameActive || !isSwiping) return;

        Vector2 swipeEnd = eventData.position;
        CheckSwipe(swipeStart, swipeEnd);
        isSwiping = false;
    }

    private void SelectRandomCategory()
    {
        if (availableCategories == null || availableCategories.Length == 0) return;

        // Выбираем случайную категорию
        targetCategory = availableCategories[Random.Range(0, availableCategories.Length)];

        if (categoryText != null && targetCategory != null)
        {
            categoryText.text = $"Категория: {targetCategory.CategoryName}";
        }
    }

    private void CheckSwipe(Vector2 start, Vector2 end)
    {
        // Проверяем все активные слова на пересечение с линией свайпа
        List<WordObject> wordsToSlice = new List<WordObject>();

        foreach (var word in activeWords)
        {
            if (!word.IsAlive) continue;

            // Проверяем пересечение линии с объектом
            if (IsSwipeIntersectingWord(start, end, word))
            {
                wordsToSlice.Add(word);
            }
        }

        // Разрезаем все найденные слова
        foreach (var word in wordsToSlice)
        {
            SliceWord(word);
        }
    }

    private bool IsSwipeIntersectingWord(Vector2 start, Vector2 end, WordObject word)
    {
        // Проверяем несколько точек вдоль линии свайпа
        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            if (word.IsPointInside(point))
            {
                return true;
            }
        }

        return false;
    }

    private void SliceWord(WordObject word)
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
            lives--;

            if (wrongSliceSound != null)
            {
                AudioManager.Instance.PlaySound(wrongSliceSound);
            }

            UpdateUI();

            // Проверяем проигрыш
            if (lives <= 0)
            {
                StartCoroutine(LoseGame());
            }
        }

        word.Slice(isCorrect);
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

        // Выбираем случайное слово
        WordData wordData = allWords[Random.Range(0, allWords.Length)];

        // Получаем размеры канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Случайная X позиция внизу экрана
        float randomX = Random.Range(-canvasSize.x / 2f + 100f, canvasSize.x / 2f - 100f);
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
        // Если пропустили слово правильной категории - теряем жизнь
        if (word.Data.HasCategory(targetCategory))
        {
            lives--;

            if (missSound != null)
            {
                AudioManager.Instance.PlaySound(missSound);
            }

            UpdateUI();

            // Проверяем проигрыш
            if (lives <= 0)
            {
                StartCoroutine(LoseGame());
            }
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

        if (livesText != null)
        {
            livesText.text = $"Жизни: {lives}";
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
