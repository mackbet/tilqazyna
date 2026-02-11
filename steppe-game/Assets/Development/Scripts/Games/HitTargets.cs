using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HitTargets : GameController
{
    [Header("UI")]
    [SerializeField] private Stopwatch stopwatch;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private LocalizedString localizedScore;

    [Header("Spawners")]
    [SerializeField] private HitSpawner[] spawners;

    [Header("Word Data")]
    [SerializeField] private WordData[] allWords;
    [SerializeField] private WordCategory[] availableCategories;

    [Header("Game Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnDuration = 2f;
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int startLives = 3;
    [SerializeField][Range(0f, 1f)] private float correctWordProbability = 0.5f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Transform effectParent;
    [SerializeField] private float effectLifetime = 1f;

    [Header("Sounds")]
    [SerializeField] private AudioClip correctHitSound;
    [SerializeField] private AudioClip wrongHitSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private WordCategory targetCategory;
    private List<WordData> targetCategoryWords = new List<WordData>();
    private List<WordData> otherCategoryWords = new List<WordData>();
    private HitSpawner lastSpawner = null;
    private float currentTime = 0f;
    private float nextSpawnTime = 0f;
    private bool isGameActive = false;
    private int score = 0;

    private void Start()
    {
        stopwatch.SetTime(gameDuration);
        stopwatch.StopStopwatch();
    }

    protected override void InitializeGame()
    {
        base.InitializeGame();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        SetLives(startLives);
        currentTime = 0f;
        score = 0;
        isGameActive = true;

        // Выбираем случайную категорию
        SelectRandomCategory();

        // Инициализируем UI
        UpdateCategoryUI();
        UpdateScoreUI();

        foreach (var spawner in spawners)
        {
            spawner.OnTargetHit += OnTargetHit;
        }

        SpawnNextTarget();
        nextSpawnTime = spawnInterval;
        stopwatch.StartStopwatch();
    }

    private void SelectRandomCategory()
    {
        if (availableCategories == null || availableCategories.Length == 0)
        {
            Debug.LogError("No available categories!");
            return;
        }

        targetCategory = availableCategories[Random.Range(0, availableCategories.Length)];

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
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = localizedScore.GetLocalizedString() + score.ToString();
        }
    }

    private void OnLocaleChanged(Locale locale) => UpdateScoreUI();

    protected override void OnDisable()
    {
        base.OnDisable();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        if (spawners != null)
        {
            foreach (var spawner in spawners)
            {
                spawner.OnTargetHit -= OnTargetHit;
                spawner.Deactivate();
            }
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentTime += Time.deltaTime;

        if (currentTime >= nextSpawnTime && currentTime < gameDuration - 2f)
        {
            SpawnNextTarget();
            nextSpawnTime = currentTime + spawnInterval;
        }

        if (currentTime >= gameDuration)
        {
            EndGame();
        }
    }

    private void SpawnNextTarget()
    {
        if (!isGameActive || spawners == null || spawners.Length == 0) return;

        HitSpawner selectedSpawner = GetRandomSpawner();

        if (selectedSpawner != null)
        {
            // Выбираем слово (правильное или неправильное)
            bool isCorrectWord = Random.value < correctWordProbability;
            WordData selectedWord = null;

            if (isCorrectWord && targetCategoryWords.Count > 0)
            {
                selectedWord = targetCategoryWords[Random.Range(0, targetCategoryWords.Count)];
            }
            else if (otherCategoryWords.Count > 0)
            {
                selectedWord = otherCategoryWords[Random.Range(0, otherCategoryWords.Count)];
            }

            if (selectedWord != null)
            {
                selectedSpawner.SpawnTargetFor(spawnDuration, selectedWord);
                lastSpawner = selectedSpawner;
            }
        }
    }

    private HitSpawner GetRandomSpawner()
    {
        if (spawners.Length == 1)
        {
            return spawners[0].IsActive() ? null : spawners[0];
        }

        // Собираем список доступных спавнеров (неактивных)
        var availableSpawners = new System.Collections.Generic.List<HitSpawner>();

        foreach (var spawner in spawners)
        {
            if (!spawner.IsActive() && spawner != lastSpawner)
            {
                availableSpawners.Add(spawner);
            }
        }

        // Если нет доступных без учета lastSpawner, берем любой неактивный
        if (availableSpawners.Count == 0)
        {
            foreach (var spawner in spawners)
            {
                if (!spawner.IsActive())
                {
                    availableSpawners.Add(spawner);
                }
            }
        }

        // Если все спавнеры заняты, возвращаем null
        if (availableSpawners.Count == 0)
        {
            Debug.LogWarning("Все спавнеры заняты! Увеличьте spawnInterval или уменьшите spawnDuration.");
            return null;
        }

        return availableSpawners[Random.Range(0, availableSpawners.Count)];
    }

    private void OnTargetHit(HitSpawner spawner, Vector2 screenPosition)
    {
        if (spawner == null) return;

        WordData word = spawner.GetCurrentWord();
        if (word == null) return;

        bool isCorrect = word.HasCategory(targetCategory);

        if (isCorrect)
        {
            // Правильное слово - добавляем очки
            score++;
            UpdateScoreUI();

            if (word.AudioClip)
                AudioManager.Instance.PlaySound(word.AudioClip);
            else if (correctHitSound != null)
                AudioManager.Instance.PlaySound(correctHitSound);

            SpawnHitEffect(screenPosition);
        }
        else
        {
            // Неправильное слово - отнимаем жизнь
            SetLives(Lives - 1);

            if (wrongHitSound != null)
            {
                AudioManager.Instance.PlaySound(wrongHitSound);
            }

            if (Lives <= 0)
            {
                LoseGame();
            }
        }
    }

    private void SpawnHitEffect(Vector2 screenPosition)
    {
        if (hitEffectPrefab == null || effectParent == null) return;

        GameObject effect = Instantiate(hitEffectPrefab, effectParent);
        RectTransform effectRect = effect.GetComponent<RectTransform>();

        if (effectRect != null)
        {
            RectTransform canvasRect = effectParent.GetComponent<RectTransform>();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out localPoint
            );

            effectRect.localPosition = localPoint;
        }

        Destroy(effect, effectLifetime);
    }

    private void EndGame()
    {
        isGameActive = false;

        foreach (var spawner in spawners)
        {
            spawner.Deactivate();
        }

        if (winSound != null)
        {
            AudioManager.Instance.PlaySound(winSound);
        }

        finishPanel.SetReward(7 + lives * 5, score, 80);
        finishPanel.SetState(true);
        finishPanel.SetStars(lives);
        FinishGame();
    }

    private void LoseGame()
    {
        isGameActive = false;

        if (stopwatch != null)
        {
            stopwatch.StopStopwatch();
        }

        foreach (var spawner in spawners)
        {
            spawner.Deactivate();
        }

        if (loseSound != null)
        {
            AudioManager.Instance.PlaySound(loseSound);
        }

        finishPanel.SetReward(2, score, 30);
        finishPanel.SetState(false);
        finishPanel.SetStars(lives);
        FinishGame();
    }
}