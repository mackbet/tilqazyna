using UnityEngine;

public class HitTargets : GameController
{
    [Header("UI")]
    [SerializeField] private Stopwatch stopwatch;
    
    [Header("Spawners")]
    [SerializeField] private HitSpawner[] spawners;
    
    [Header("Game Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnDuration = 2f;
    [SerializeField] private float gameDuration = 15f;
    
    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Transform effectParent;
    [SerializeField] private float effectLifetime = 1f;

    private HitSpawner lastSpawner = null;
    private float currentTime = 0f;
    private float nextSpawnTime = 0f;
    private bool isGameActive = false;
    private int score = 0;
    private int maxScore = 0;

    private void Start()
    {
        stopwatch.SetTime(gameDuration);
        stopwatch.StopStopwatch();
    }

    protected override void InitializeGame()
    {
        base.InitializeGame();

        currentTime = 0f;
        score = 0;
        maxScore = 0;
        isGameActive = true;

        foreach (var spawner in spawners)
        {
            spawner.OnTargetHit += OnTargetHit;
        }

        SpawnNextTarget();
        nextSpawnTime = spawnInterval;
        stopwatch.StartStopwatch();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

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
            selectedSpawner.SpawnTargetFor(spawnDuration);
            maxScore++;
            lastSpawner = selectedSpawner;
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

    private void OnTargetHit(Vector2 screenPosition)
    {
        score++;
        Debug.Log($"Попадание! Счет: {score}/{maxScore}");
        
        SpawnHitEffect(screenPosition);
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

        Debug.Log($"Игра окончена! Финальный счет: {score}/{maxScore}");
        FinishGame();
    }

    public float GetProgress() => gameDuration > 0 ? currentTime / gameDuration : 0f;
    public int GetScore() => score;
}