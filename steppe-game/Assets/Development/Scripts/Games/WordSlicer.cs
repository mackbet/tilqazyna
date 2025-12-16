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
    [SerializeField][Range(0f, 1f)] private float correctWordsProbability = 0.4f; // Вероятность спавна правильного слова (0-1)
    [SerializeField][Range(0f, 1f)] private float spawnXMin = 0.1f; // Минимальная позиция спавна по X (0 = левый край, 1 = правый край)
    [SerializeField][Range(0f, 1f)] private float spawnXMax = 0.9f; // Максимальная позиция спавна по X
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

    [Header("Spawn Effects")]
    [SerializeField] private RectTransform animalContainer; // Контейнер для животных (рыбы и птицы)
    [SerializeField] private GameObject fishPrefab; // Префаб рыбы
    [SerializeField] private GameObject birdPrefab; // Префаб птицы
    [SerializeField] private AudioClip fishSound; // Звук появления рыбы
    [SerializeField] private AudioClip birdSound; // Звук появления птиц
    [SerializeField][Range(0f, 1f)] private float fishSpawnZoneMin = 0.3f; // Минимальная высота зоны спавна рыбы (0 = низ, 1 = верх)
    [SerializeField][Range(0f, 1f)] private float fishSpawnZoneMax = 0.7f; // Максимальная высота зоны спавна рыбы
    [SerializeField][Range(0f, 1f)] private float birdSpawnZoneMin = 0.5f; // Минимальная высота зоны спавна птиц
    [SerializeField][Range(0f, 1f)] private float birdSpawnZoneMax = 0.9f; // Максимальная высота зоны спавна птиц
    [SerializeField] private float fishMinScale = 0.5f; // Минимальный размер рыбы
    [SerializeField] private float fishMaxScale = 1.5f; // Максимальный размер рыбы
    [SerializeField] private float birdScale = 1.0f; // Размер птиц
    [SerializeField] private int minBirdCount = 3; // Минимальное количество птиц
    [SerializeField] private int maxBirdCount = 4; // Максимальное количество птиц

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

            // Спавним рыбу
            SpawnFish(slicePoint);

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

            // Спавним птиц
            SpawnBirds(slicePoint);

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

    private void SpawnFish(Vector2 slicePosition)
    {
        if (fishPrefab == null || animalContainer == null) return;

        // Воспроизводим звук появления рыбы
        if (fishSound != null)
        {
            AudioManager.Instance.PlaySound(fishSound);
        }

        // Получаем размеры канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Вычисляем случайную вертикальную позицию в зоне спавна
        // 0 = низ экрана, 1 = верх экрана
        float normalizedY = Random.Range(fishSpawnZoneMin, fishSpawnZoneMax);

        // Конвертируем в координаты канваса
        float yPosition = Mathf.Lerp(-canvasSize.y / 2f, canvasSize.y / 2f, normalizedY);

        // Случайная X позиция в пределах экрана
        float xPosition = Random.Range(-canvasSize.x / 2f, canvasSize.x / 2f);

        // Создаем рыбу в контейнере для животных
        GameObject fish = Instantiate(fishPrefab, animalContainer);
        RectTransform fishRect = fish.GetComponent<RectTransform>();

        if (fishRect != null)
        {
            fishRect.anchoredPosition = new Vector2(xPosition, yPosition);

            // Размер рыбы зависит от высоты зоны: чем выше, тем больше
            // normalizedY уже в диапазоне от fishSpawnZoneMin до fishSpawnZoneMax
            // Нормализуем это в диапазон 0-1 относительно зоны спавна
            float zoneNormalizedY = (normalizedY - fishSpawnZoneMin) / (fishSpawnZoneMax - fishSpawnZoneMin);
            float fishScale = Mathf.Lerp(fishMinScale, fishMaxScale, 1 - zoneNormalizedY);
            fishRect.localScale = Vector3.one * fishScale;
        }

        // Настраиваем анимацию
        ImageAnimator animator = fish.GetComponent<ImageAnimator>();
        if (animator != null)
        {
            // Подписываемся на завершение анимации
            animator.OnAnimationComplete += () =>
            {
                if (fish != null)
                {
                    Destroy(fish);
                }
            };
        }
    }

    private void SpawnBirds(Vector2 slicePosition)
    {
        if (birdPrefab == null || animalContainer == null) return;

        // Воспроизводим звук появления птиц
        if (birdSound != null)
        {
            AudioManager.Instance.PlaySound(birdSound);
        }

        // Получаем размеры канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Случайное количество птиц
        int birdCount = Random.Range(minBirdCount, maxBirdCount + 1);

        for (int i = 0; i < birdCount; i++)
        {
            // Вычисляем случайную вертикальную позицию в зоне спавна
            float normalizedY = Random.Range(birdSpawnZoneMin, birdSpawnZoneMax);
            float yPosition = Mathf.Lerp(-canvasSize.y / 2f, canvasSize.y / 2f, normalizedY);

            // Случайная X позиция в пределах экрана
            float xPosition = Random.Range(-canvasSize.x / 2f, canvasSize.x / 2f);

            // Создаем птицу в контейнере для животных
            GameObject bird = Instantiate(birdPrefab, animalContainer);
            RectTransform birdRect = bird.GetComponent<RectTransform>();

            if (birdRect != null)
            {
                birdRect.anchoredPosition = new Vector2(xPosition, yPosition);
                birdRect.localScale = Vector3.one * birdScale;

                // Случайное направление движения (влево или вправо)
                float horizontalDirection = Random.value > 0.5f ? 1f : -1f;

                // Если птица летит влево, отражаем спрайт
                if (horizontalDirection < 0)
                {
                    birdRect.localScale = new Vector3(-birdScale, birdScale, birdScale);
                }

                // Запускаем движение птицы
                StartCoroutine(MoveBird(bird, birdRect, horizontalDirection));
            }
        }
    }

    private IEnumerator MoveBird(GameObject bird, RectTransform birdRect, float horizontalDirection)
    {
        // Скорости движения
        float horizontalSpeed = Random.Range(300f, 500f) * horizontalDirection;
        float verticalSpeed = Random.Range(400f, 600f);

        // Получаем размеры канваса
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Границы экрана (с запасом)
        float topBound = canvasSize.y / 2f + 200f;
        float leftBound = -canvasSize.x / 2f - 200f;
        float rightBound = canvasSize.x / 2f + 200f;

        while (bird != null && birdRect != null)
        {
            // Двигаем птицу
            birdRect.anchoredPosition += new Vector2(horizontalSpeed, verticalSpeed) * Time.deltaTime;

            // Проверяем выход за границы
            Vector2 pos = birdRect.anchoredPosition;
            if (pos.y > topBound || pos.x < leftBound || pos.x > rightBound)
            {
                // Птица вылетела за экран
                if (bird != null)
                {
                    Destroy(bird);
                }
                yield break;
            }

            yield return null;
        }
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
