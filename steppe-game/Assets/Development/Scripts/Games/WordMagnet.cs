using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordMagnet : GameController
{
    [Header("Game Objects")]
    [SerializeField] private RectTransform magnet; // Магнит игрока
    [SerializeField] private RectTransform magnetAttractionPoint; // Точка притяжения магнита
    [SerializeField] private RectTransform gameArea; // Игровая область
    [SerializeField] private RectTransform boxContainer; // Контейнер для коробок
    [SerializeField] private WordMagnetBox correctWordBoxPrefab; // Префаб для правильных слов
    [SerializeField] private WordMagnetBox wrongWordBoxPrefab; // Префаб для неправильных слов
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private ParallaxController parallaxController;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI categoryText; // Текст категории на магните
    [SerializeField] private TextMeshProUGUI scoreText; // Счет

    [Header("Word Data")]
    [SerializeField] private WordData[] allWords; // Все доступные слова
    [SerializeField] private WordCategory[] availableCategories; // Доступные категории

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f; // Длительность игры
    [SerializeField] private int startLives = 3; // Начальное количество жизней
    [SerializeField] private float spawnInterval = 2f; // Интервал спавна коробок
    [SerializeField] private float boxSpeed = 150f; // Скорость движения коробок
    [SerializeField] private float magnetSpeed = 500f; // Скорость движения магнита
    [SerializeField] private float magnetRotationSpeed = 360f; // Скорость поворота магнита (градусов в секунду)
    [SerializeField] private float magnetCaptureRadius = 150f; // Радиус захвата магнита
    [SerializeField][Range(0f, 1f)] private float correctWordProbability = 0.5f; // Вероятность спавна правильного слова
    [SerializeField] private float minRotationAngle = -15f; // Минимальный угол наклона слова
    [SerializeField] private float maxRotationAngle = 15f; // Максимальный угол наклона слова

    [Header("Sounds")]
    [SerializeField] private AudioClip correctCaptureSound;
    [SerializeField] private AudioClip wrongCaptureSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private WordCategory targetCategory; // Целевая категория магнита
    private List<WordData> targetCategoryWords = new List<WordData>(); // Слова целевой категории
    private List<WordData> otherCategoryWords = new List<WordData>(); // Слова других категорий
    private List<WordMagnetBox> activeBoxes = new List<WordMagnetBox>();
    private Coroutine spawnCoroutine;
    private bool isGameActive = false;
    private float gameTimer;
    private int score = 0;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        SetLives(startLives);
        score = 0;
        gameTimer = gameDuration;
        isGameActive = true;

        // Выбираем случайную категорию
        SelectRandomCategory();

        // Инициализируем UI
        UpdateCategoryUI();
        UpdateScoreUI();

        // Инициализируем параллакс
        if (parallaxController != null)
        {
            parallaxController.InitializeLayers();
            parallaxController.ResumeParallax();
        }

        // Очищаем активные коробки
        ClearBoxes();

        // Запускаем спавн коробок
        spawnCoroutine = StartCoroutine(SpawnBoxes());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        isGameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Останавливаем параллакс
        if (parallaxController != null)
        {
            parallaxController.StopParallax();
        }

        ClearBoxes();
    }

    private void Update()
    {
        if (!isGameActive) return;

        UpdateMagnetMovement();
        UpdateGameTimer();
        UpdateBoxes();
        CheckMagnetCapture();
    }

    private void UpdateMagnetMovement()
    {
        if (magnet == null || joystick == null) return;

        // Получаем направление от джойстика
        Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (direction.magnitude > 0.01f)
        {
            // Двигаем магнит
            Vector2 movement = direction * magnetSpeed * Time.deltaTime;
            Vector2 newPos = magnet.anchoredPosition + movement;

            // Ограничиваем движение магнита границами экрана
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float margin = 50f;

            newPos.x = Mathf.Clamp(newPos.x, -screenWidth / 2f + margin, screenWidth / 2f - margin);
            newPos.y = Mathf.Clamp(newPos.y, -screenHeight / 2f + margin, screenHeight / 2f - margin);

            magnet.anchoredPosition = newPos;

            // Плавно поворачиваем магнит в сторону движения
            // Локальная ось X должна быть направлена в сторону движения
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            magnet.localRotation = Quaternion.RotateTowards(
                magnet.localRotation,
                targetRotation,
                magnetRotationSpeed * Time.deltaTime
            );
        }

        // Компенсируем поворот для текста категории, чтобы он всегда был читабельным
        if (categoryText != null)
        {
            categoryText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -magnet.localEulerAngles.z);
        }
    }

    private void SelectRandomCategory()
    {
        if (availableCategories == null || availableCategories.Length == 0)
        {
            Debug.LogError("No available categories!");
            return;
        }

        // Выбираем случайную категорию
        targetCategory = availableCategories[Random.Range(0, availableCategories.Length)];

        // Заполняем списки слов
        targetCategoryWords.Clear();
        otherCategoryWords.Clear();

        foreach (var word in allWords)
        {
            if (word.HasCategory(targetCategory))
            {
                targetCategoryWords.Add(word);
            }
            else
            {
                otherCategoryWords.Add(word);
            }
        }
    }

    private void UpdateCategoryUI()
    {
        if (categoryText != null && targetCategory != null)
        {
            categoryText.text = targetCategory.CategoryName;
        }

        // Устанавливаем цвет магнита (если есть Image компонент)
        if (magnet != null && targetCategory != null)
        {
            var image = magnet.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = targetCategory.CategoryColor;
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateGameTimer()
    {
        gameTimer -= Time.deltaTime;

        if (gameTimer <= 0)
        {
            WinGame();
        }
    }

    private IEnumerator SpawnBoxes()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        while (isGameActive)
        {
            yield return wait;
            SpawnBox();
        }
    }

    private void SpawnBox()
    {
        if (boxContainer == null || gameArea == null)
            return;

        // Определяем, будет ли это правильное или неправильное слово
        bool isCorrectWord = Random.value < correctWordProbability;

        WordData selectedWord = null;
        WordMagnetBox prefabToUse = null;

        if (isCorrectWord && targetCategoryWords.Count > 0)
        {
            selectedWord = targetCategoryWords[Random.Range(0, targetCategoryWords.Count)];
            prefabToUse = correctWordBoxPrefab;
        }
        else if (otherCategoryWords.Count > 0)
        {
            selectedWord = otherCategoryWords[Random.Range(0, otherCategoryWords.Count)];
            prefabToUse = wrongWordBoxPrefab;
        }

        if (selectedWord == null || prefabToUse == null)
            return;

        // Создаем коробку
        WordMagnetBox box = Instantiate(prefabToUse, boxContainer);
        RectTransform boxRect = box.GetComponent<RectTransform>();

        // Определяем сторону спавна (true = слева, false = справа)
        bool spawnFromLeft = Random.value > 0.5f;

        // Определяем позицию спавна и угол наклона
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float yPos = Random.Range(-screenHeight / 2f + 100f, screenHeight / 2f - 100f);

        Vector2 spawnPos;
        float rotationAngle = Random.Range(minRotationAngle, maxRotationAngle);
        float moveSpeed;

        if (spawnFromLeft)
        {
            // Спавн слева - слово движется вправо
            spawnPos = new Vector2(-screenWidth / 2f - 100f, yPos);
            moveSpeed = boxSpeed; // Положительная скорость - движение вправо по локальной оси X
        }
        else
        {
            // Спавн справа - слово движется влево
            spawnPos = new Vector2(screenWidth / 2f + 100f, yPos);
            moveSpeed = -boxSpeed; // Отрицательная скорость - движение влево по локальной оси X
        }

        boxRect.anchoredPosition = spawnPos;
        boxRect.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

        // Инициализируем коробку
        box.Initialize(selectedWord, moveSpeed, this);

        activeBoxes.Add(box);
    }


    private void UpdateBoxes()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        for (int i = activeBoxes.Count - 1; i >= 0; i--)
        {
            WordMagnetBox box = activeBoxes[i];
            if (box == null)
            {
                activeBoxes.RemoveAt(i);
                continue;
            }

            // Удаляем коробки, вышедшие за границы
            if (box.IsOutOfBounds(screenWidth, screenHeight))
            {
                Destroy(box.gameObject);
                activeBoxes.RemoveAt(i);
            }
        }
    }

    private void CheckMagnetCapture()
    {
        if (magnetAttractionPoint == null) return;

        for (int i = activeBoxes.Count - 1; i >= 0; i--)
        {
            WordMagnetBox box = activeBoxes[i];
            if (box == null || box.IsCaptured)
                continue;

            RectTransform boxRect = box.GetComponent<RectTransform>();

            // Простая проверка расстояния от точки притяжения
            float distance = Vector2.Distance(magnetAttractionPoint.anchoredPosition, boxRect.anchoredPosition);

            if (distance <= magnetCaptureRadius)
            {
                // Захватываем коробку
                box.Capture(magnetAttractionPoint);
            }
        }
    }

    public void OnBoxCaptured(WordMagnetBox box)
    {
        if (box == null || box.WordData == null)
            return;

        bool isCorrect = box.WordData.HasCategory(targetCategory);

        if (isCorrect)
        {
            // Правильное слово - добавляем очки
            score++;
            UpdateScoreUI();

            if (box.WordData.AudioClip)
                AudioManager.Instance.PlaySound(box.WordData.AudioClip);
            else
            {
                if (correctCaptureSound != null)
                    AudioManager.Instance.PlaySound(correctCaptureSound);
            }
        }
        else
        {
            // Неправильное слово - отнимаем жизнь
            SetLives(Lives - 1);

            if (wrongCaptureSound != null)
            {
                AudioManager.Instance.PlaySound(wrongCaptureSound);
            }

            if (Lives <= 0)
            {
                LoseGame();
            }

            CameraShake.Instance.Shake(gameArea, 80f, 0.3f, 1.05f);
        }

        // Удаляем коробку
        activeBoxes.Remove(box);
        Destroy(box.gameObject, 0.5f); // Удаляем с небольшой задержкой для анимации
    }


    private void ClearBoxes()
    {
        foreach (var box in activeBoxes)
        {
            if (box != null)
            {
                Destroy(box.gameObject);
            }
        }
        activeBoxes.Clear();
    }

    private void WinGame()
    {
        isGameActive = false;

        if (winSound != null)
        {
            AudioManager.Instance.PlaySound(winSound);
        }

        finishPanel.SetReward(7 + lives * 5, 80, 80);
        FinishGame();
    }

    private void LoseGame()
    {
        isGameActive = false;

        if (loseSound != null)
        {
            AudioManager.Instance.PlaySound(loseSound);
        }

        finishPanel.SetReward(2, 20, 30);
        FinishGame();
    }
}
