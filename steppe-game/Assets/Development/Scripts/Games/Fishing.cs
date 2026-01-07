using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fishing : GameController
{
    [Header("Game Objects")]
    [SerializeField] private FishingRod fishingRod;
    [SerializeField] private RectTransform fishContainer; // Контейнер для рыб
    [SerializeField] private RectTransform gameArea; // Игровая область

    [Header("Fish Prefabs")]
    [SerializeField] private Fish[] fishPrefabs; // Префабы всех видов рыб
    [SerializeField] private string[] targetFishTypes; // Типы рыб которые нужно поймать (Fish.fishType)

    [Header("Game Settings")]
    [SerializeField] private int fishToCatch = 5; // Сколько нужно поймать целевых рыб
    [SerializeField] private float spawnInterval = 2f; // Интервал спавна рыб
    [SerializeField] private int maxFishOnScreen = 5; // Максимум рыб на экране
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

    protected override void InitializeGame()
    {
        base.InitializeGame();

        caughtCount = 0;
        activeFish.Clear();
        isGameActive = true;

        // Выбираем случайный целевой тип рыбы
        if (targetFishTypes != null && targetFishTypes.Length > 0)
        {
            currentTargetFishType = targetFishTypes[Random.Range(0, targetFishTypes.Length)];
            ShowTargetFish();
        }

        UpdateUI();

        // Подписываемся на события удочки
        if (fishingRod != null)
        {
            fishingRod.OnFishCaught += OnFishCaught;
        }

        // Запускаем спавн рыб
        spawnCoroutine = StartCoroutine(SpawnFishRoutine());
    }

    private IEnumerator SpawnFishRoutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (isGameActive && activeFish.Count < maxFishOnScreen)
            {
                SpawnRandomFish();
            }
        }
    }

    private void SpawnRandomFish()
    {
        if (fishPrefabs == null || fishPrefabs.Length == 0)
            return;

        // Выбираем случайный префаб рыбы
        Fish fishPrefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];

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
            if (caughtCount >= fishToCatch)
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
            progressText.text = $"{caughtCount}/{fishToCatch}";
        }
    }

    private void ShowTargetFish()
    {
        if (targetFishImage == null || fishPrefabs == null) return;

        // Находим префаб рыбы с нужным типом
        Fish targetFishPrefab = System.Array.Find(fishPrefabs, fish => fish.FishType == currentTargetFishType);

        if (targetFishPrefab != null && targetFishPrefab.FishSprite != null)
        {
            targetFishImage.sprite = targetFishPrefab.FishSprite;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        noise = AudioManager.Instance.PlaySound(noiseSound, 1, true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

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
