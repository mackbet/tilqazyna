using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class FishingLevel
{
    public LocalizedString levelName;
    public Sprite levelIcon; // Иконка для кнопки уровня

    [Header("Fish Settings")]
    public Fish[] fishPrefabs; // Префабы рыб для этого уровня
    public string[] targetFishTypes; // Типы рыб которые нужно поймать
    public int fishToCatch = 5; // Сколько нужно поймать целевых рыб

    [Header("Difficulty")]
    public float spawnInterval = 2f; // Интервал спавна рыб
    public int maxFishOnScreen = 5; // Максимум рыб на экране
}

public class Fishing : GameController
{
    [Header("Game Objects")]
    [SerializeField] private FishingRod fishingRod;
    [SerializeField] private RectTransform fishContainer; // Контейнер для рыб
    [SerializeField] private RectTransform gameArea; // Игровая область

    [Header("Levels")]
    [SerializeField] private FishingLevel[] levels; // Массив уровней

    [Header("UI Panels")]
    [SerializeField] private GameObject levelSelectionPanel; // Панель выбора уровня
    [SerializeField] private GameObject gamePanel; // Панель игры

    [Header("Level Selection")]
    [SerializeField] private Transform levelButtonsContainer; // Контейнер для кнопок уровней
    [SerializeField] private CustomButton levelButtonPrefab; // Префаб кнопки уровня

    [Header("Game Settings")]
    [SerializeField][Range(0f, 1f)] private float minSpawnHeight = 0.2f; // Минимальная высота спавна (0 = низ, 1 = верх)
    [SerializeField][Range(0f, 1f)] private float maxSpawnHeight = 0.8f; // Максимальная высота спавна (0 = низ, 1 = верх)

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText; // "Поймано: 3/5"
    [SerializeField] private Image targetFishImage; // Изображение целевой рыбы

    [Header("Sounds")]
    [SerializeField] private AudioClip noiseSound;
    [SerializeField] private AudioClip catchSound;
    [SerializeField] private AudioClip wrongFishSound;

    private int caughtCount = 0;
    private string currentTargetFishType; // Текущий целевой тип рыбы
    private List<Fish> activeFish = new List<Fish>();
    private Coroutine spawnCoroutine;
    private bool isGameActive = false;
    private AudioSource noise;

    private FishingLevel currentLevel; // Текущий выбранный уровень
    private List<CustomButton> levelButtons = new List<CustomButton>();
    private Dictionary<int, Action> levelButtonActions = new Dictionary<int, Action>(); // Для корректной отписки

    protected override void InitializeGame()
    {
        base.InitializeGame();

        // Показываем панель выбора уровня
        ShowLevelSelection();
    }

    private void ShowLevelSelection()
    {
        // Показываем панель выбора, скрываем игру
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(true);

        if (gamePanel != null)
            gamePanel.SetActive(false);

        // Создаем кнопки для каждого уровня
        CreateLevelButtons();
    }

    private void CreateLevelButtons()
    {
        // Отписываемся от старых кнопок
        foreach (var kvp in levelButtonActions)
        {
            if (levelButtons.Count > kvp.Key && levelButtons[kvp.Key] != null)
            {
                levelButtons[kvp.Key].OnButtonClicked -= kvp.Value;
            }
        }
        levelButtonActions.Clear();

        // Очищаем предыдущие кнопки
        foreach (var button in levelButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        levelButtons.Clear();

        // Создаем кнопку для каждого уровня
        if (levels != null && levelButtonPrefab != null && levelButtonsContainer != null)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                int levelIndex = i; // Захватываем индекс для замыкания
                FishingLevel level = levels[i];

                // Создаем кнопку
                CustomButton levelButton = Instantiate(levelButtonPrefab, levelButtonsContainer);

                levelButton.Image.sprite = level.levelIcon;
                levelButton.Label.text = level.levelName.GetLocalizedString();

                // Создаем действие и сохраняем его
                Action buttonAction = () => OnLevelSelected(levelIndex);
                levelButtonActions[i] = buttonAction;

                // Подписываемся на событие нажатия
                levelButton.OnButtonClicked += buttonAction;

                levelButtons.Add(levelButton);
            }
        }
    }

    private void OnLevelSelected(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
            return;

        currentLevel = levels[levelIndex];
        StartLevel();
    }

    private void StartLevel()
    {
        // Скрываем панель выбора, показываем игру
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Инициализируем игру с параметрами уровня
        caughtCount = 0;
        activeFish.Clear();
        isGameActive = true;

        // Выбираем случайный целевой тип рыбы из уровня
        if (currentLevel.targetFishTypes != null && currentLevel.targetFishTypes.Length > 0)
        {
            currentTargetFishType = currentLevel.targetFishTypes[Random.Range(0, currentLevel.targetFishTypes.Length)];
            ShowTargetFish();
        }

        UpdateUI();

        // Подписываемся на события удочки
        if (fishingRod != null)
        {
            fishingRod.OnFishCaught += OnFishCaught;
        }

        // Запускаем спавн рыб с интервалом из уровня
        spawnCoroutine = StartCoroutine(SpawnFishRoutine());
    }

    private IEnumerator SpawnFishRoutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(currentLevel.spawnInterval);

            if (isGameActive && activeFish.Count < currentLevel.maxFishOnScreen)
            {
                SpawnRandomFish();
            }
        }
    }

    private void SpawnRandomFish()
    {
        // Используем рыб из текущего уровня
        if (currentLevel.fishPrefabs == null || currentLevel.fishPrefabs.Length == 0)
            return;

        // Выбираем случайную рыбу из списка рыб текущего уровня
        Fish fishPrefab = currentLevel.fishPrefabs[Random.Range(0, currentLevel.fishPrefabs.Length)];

        // Определяем сторону спавна (слева или справа)
        bool spawnFromLeft = Random.value > 0.5f;

        // Получаем размеры канваса (экрана)
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Вычисляем случайную Y позицию на основе нормализованных значений (0-1)
        float minY = -canvasSize.y / 2f + minSpawnHeight * canvasSize.y;
        float maxY = -canvasSize.y / 2f + maxSpawnHeight * canvasSize.y;
        float randomY = Random.Range(minY, maxY);

        Vector2 spawnPosition;
        Vector2 moveDirection;

        if (spawnFromLeft)
        {
            // Спавн на левом краю канваса, движение вправо
            spawnPosition = new Vector2(-canvasSize.x / 2f - 450, randomY);
            moveDirection = Vector2.right;
        }
        else
        {
            // Спавн на правом краю канваса, движение влево
            spawnPosition = new Vector2(canvasSize.x / 2f + 450, randomY);
            moveDirection = Vector2.left;
        }

        // Создаем рыбу
        Fish newFish = Instantiate(fishPrefab, fishContainer);
        newFish.Initialize(spawnPosition, moveDirection);

        // Подписываемся на события
        newFish.OnFishEscaped += OnFishEscaped;

        activeFish.Add(newFish);
    }

    private void OnFishCaught(Fish fish)
    {
        if (fish == null) return;

        // Проверяем, правильная ли рыба поймана
        bool isTargetFish = fish.FishType == currentTargetFishType;

        if (isTargetFish)
        {
            caughtCount++;

            if (catchSound != null)
            {
                AudioManager.Instance.PlaySound(catchSound);
            }

            UpdateUI();

            // Проверяем победу
            if (caughtCount >= currentLevel.fishToCatch)
            {
                StartCoroutine(WinGame());
            }
        }
        else
        {
            // Поймали не ту рыбу
            if (wrongFishSound != null)
            {
                AudioManager.Instance.PlaySound(wrongFishSound);
            }
        }

        // Удаляем из списка активных рыб
        activeFish.Remove(fish);
    }

    private void OnFishEscaped(Fish fish)
    {
        activeFish.Remove(fish);
    }

    private IEnumerator WinGame()
    {
        // Останавливаем спавн рыб
        isGameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        yield return new WaitForSeconds(1f);

        finishPanel.SetReward(15, 30, 200);
        finishPanel.SetState(true);
        finishPanel.SetStars(3);
        FinishGame();
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"{caughtCount}/{currentLevel.fishToCatch}";
        }
    }

    private void ShowTargetFish()
    {
        // Используем рыб из текущего уровня
        if (targetFishImage == null || currentLevel.fishPrefabs == null) return;

        // Находим префаб рыбы с нужным типом в списке рыб текущего уровня
        Fish targetFishPrefab = Array.Find(currentLevel.fishPrefabs, fish => fish.FishType == currentTargetFishType);

        if (targetFishPrefab != null && targetFishPrefab.FishSprite != null)
        {
            targetFishImage.sprite = targetFishPrefab.FishSprite;

            // Получаем AspectRatioFitter и устанавливаем соотношение сторон
            AspectRatioFitter aspectRatioFitter = targetFishImage.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter != null)
            {
                Sprite sprite = targetFishPrefab.FishSprite;
                aspectRatioFitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (noiseSound != null)
            noise = AudioManager.Instance.PlaySound(noiseSound, 1, true);

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (i < levels.Length && levelButtons[i] != null)
                levelButtons[i].Label.text = levels[i].levelName.GetLocalizedString();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        // Останавливаем игру
        isGameActive = false;

        // Останавливаем спавн
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Отписываемся от событий
        if (fishingRod != null)
        {
            fishingRod.OnFishCaught -= OnFishCaught;
        }

        // Отписываемся от кнопок уровней
        foreach (var kvp in levelButtonActions)
        {
            if (levelButtons.Count > kvp.Key && levelButtons[kvp.Key] != null)
            {
                levelButtons[kvp.Key].OnButtonClicked -= kvp.Value;
            }
        }

        // Очищаем активных рыб
        foreach (var fish in activeFish)
        {
            if (fish != null)
            {
                Destroy(fish.gameObject);
            }
        }

        activeFish.Clear();

        if (noise)
            Destroy(noise.gameObject);
    }
}