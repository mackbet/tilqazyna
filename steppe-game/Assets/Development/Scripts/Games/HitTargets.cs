using UnityEngine;

public class HitTargets : GameController
{
    [SerializeField] private HitSpawner[] spawners;
    [SerializeField] private float targetCount = 15;
    [SerializeField] private float gameDuration = 15;
    
    private HitSpawner lastSpawner = null;
    private float spawnInterval;
    private float currentTime = 0f;
    private float nextSpawnTime = 0f;
    private int spawnedCount = 0;
    private int hitCount = 0;
    private bool isGameActive = false;

    protected override void InitializeGame()
    {
        base.InitializeGame();
        
        spawnInterval = gameDuration / targetCount;
        currentTime = 0f;
        nextSpawnTime = 0f;
        spawnedCount = 0;
        hitCount = 0;
        isGameActive = true;
        
        // Подписываемся на события попадания
        foreach (var spawner in spawners)
        {
            spawner.OnTargetHit += HandleTargetHit;
        }
        
        // Спавним первую цель сразу
        SpawnNextTarget();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        // Отписываемся от событий
        if (spawners != null)
        {
            foreach (var spawner in spawners)
            {
                spawner.OnTargetHit -= HandleTargetHit;
            }
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentTime += Time.deltaTime;

        // Проверяем, пора ли спавнить следующую цель
        if (currentTime >= nextSpawnTime && spawnedCount < targetCount)
        {
            SpawnNextTarget();
        }

        // Проверяем, закончилось ли время игры
        if (currentTime >= gameDuration)
        {
            EndGame();
        }
    }

    private void SpawnNextTarget()
    {
        if (spawners == null || spawners.Length == 0) return;

        // Выбираем случайный спавнер, отличный от предыдущего
        HitSpawner selectedSpawner = GetRandomSpawner();
        
        // Спавним цель на оставшееся время до следующего спавна
        float targetDuration = spawnInterval;
        selectedSpawner.SpawnTargetFor(targetDuration);
        
        lastSpawner = selectedSpawner;
        spawnedCount++;
        nextSpawnTime = currentTime + spawnInterval;
    }

    private HitSpawner GetRandomSpawner()
    {
        if (spawners.Length == 1) return spawners[0];

        HitSpawner selectedSpawner;
        do
        {
            selectedSpawner = spawners[Random.Range(0, spawners.Length)];
        }
        while (selectedSpawner == lastSpawner && spawners.Length > 1);

        return selectedSpawner;
    }

    private void HandleTargetHit()
    {
        hitCount++;
        Debug.Log($"Попадание! Счёт: {hitCount}/{spawnedCount}");
    }

    private void EndGame()
    {
        isGameActive = false;
        
        // Деактивируем все спавнеры
        foreach (var spawner in spawners)
        {
            spawner.Deactivate();
        }
        
        FinishGame();
    }

    public float GetProgress()
    {
        return gameDuration > 0 ? currentTime / gameDuration : 0f;
    }

    public int GetSpawnedCount() => spawnedCount;
    public int GetHitCount() => hitCount;
    public int GetTotalTargets() => (int)targetCount;
}