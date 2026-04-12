using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Leaderboard Settings")]
    [SerializeField] private string leaderboardId = "main_leaderboard";

    [Header("Content Object")]
    [SerializeField] private GameObject content;

    [Header("Leader card prefabs")]
    [SerializeField] private GameObject leaderFirstPrefab;
    [SerializeField] private GameObject leaderSecondPrefab;
    [SerializeField] private GameObject leaderThirdPrefab;

    [Header("Loading items")]
    [SerializeField] private Slider loader;
    [SerializeField] private GameObject loaderObject;

    private RealtimeManager _realtimeManager;
    private StateManager _stateManager;

    private List<GameObject> leaderCards = new();

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(RealtimeManager realtimeManager, StateManager stateManager)
    {
        _realtimeManager = realtimeManager;
        _stateManager = stateManager;
    }

    /// <summary>
    /// Отправить очки в лидерборд с метаданными (имя, пол, уровень)
    /// </summary>
    public async Task SubmitScore(int score, string playerName, CharacterSex sex, int level)
    {
        if (!AndroidLoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[Leaderboard] Пользователь не авторизован");
            return;
        }

        try
        {
            var metadata = new LeaderboardMetadata
            {
                name = playerName,
                sex = (int)sex,
                level = level
            };

            var options = new AddPlayerScoreOptions { Metadata = metadata };
            var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
            Debug.Log($"[Leaderboard] Очки отправлены: {score}, Место: {result.Rank + 1}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Leaderboard] Ошибка отправки очков: {e.Message}");
        }
    }

    /// <summary>
    /// Отправить очки (упрощённый метод - берёт данные из StateManager)
    /// </summary>
    public async Task SubmitScore(int score)
    {
        if (_stateManager != null)
        {
            await SubmitScore(
                score,
                _stateManager.PlayerName,
                _stateManager.CharacterSex,
                _stateManager.ExperienceAmount / 80
            );
        }
        else if (StateManager.Instance != null)
        {
            await SubmitScore(
                score,
                StateManager.Instance.PlayerName,
                StateManager.Instance.CharacterSex,
                StateManager.Instance.ExperienceAmount / 80
            );
        }
        else
        {
            Debug.LogWarning("[Leaderboard] StateManager не найден");
        }
    }

    public async Task ReadData()
    {
        loaderObject.SetActive(true);
        loader.value = 0;

        const float totalSteps = 10;
        for (var i = 0; i < totalSteps / 2; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            loader.value = (i + 1) / totalSteps;
        }

        try
        {
            // Отправляем текущие очки игрока, чтобы он появился в списке
            var sm = _stateManager != null ? _stateManager : StateManager.Instance;
            if (sm != null && (AndroidLoginManager.Instance?.IsAuthenticated ?? false))
            {
                await SubmitScore(sm.PointsAmount);
            }
            // Получаем топ игроков из Unity Leaderboards с метаданными
            var options = new GetScoresOptions
            {
                Limit = 30,
                IncludeMetadata = true
            };
            var leaderboardResult = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);

            int place = 1;
            foreach (var entry in leaderboardResult.Results)
            {
                GameObject prefab;
                if (place == 1)
                    prefab = leaderFirstPrefab;
                else if (place <= 3)
                    prefab = leaderSecondPrefab;
                else
                    prefab = leaderThirdPrefab;

                var cardObj = InstantPrefab(prefab);
                var cardManager = cardObj.GetComponent<LeaderCardManager>();

                // Парсим метаданные
                string name = entry.PlayerName ?? $"Игрок {entry.PlayerId.Substring(0, 6)}";
                CharacterSex sex = CharacterSex.Boy;
                int level = 0;

                if (!string.IsNullOrEmpty(entry.Metadata))
                {
                    try
                    {
                        var metadata = JsonUtility.FromJson<LeaderboardMetadata>(entry.Metadata);
                        if (metadata != null)
                        {
                            if (!string.IsNullOrEmpty(metadata.name))
                                name = metadata.name;
                            sex = (CharacterSex)metadata.sex;
                            level = metadata.level;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Leaderboard] Ошибка парсинга метаданных: {e.Message}");
                    }
                }

                var userModel = new UserModel(
                    name: name,
                    sex: sex,
                    experience: level * 80,  // В лидерборде хранится level, конвертируем в experience
                    points: (int)entry.Score
                );

                cardManager.SetCardInfo(userModel: userModel, userPlace: place);
                leaderCards.Add(cardObj);

                place++;
            }

            Debug.Log($"[Leaderboard] Загружено {leaderboardResult.Results.Count} записей");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Leaderboard] Ошибка загрузки: {e.Message}");
        }

        for (var i = 4; i < totalSteps; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            loader.value = (i + 1) / totalSteps;
        }

        loaderObject.SetActive(false);
    }

    public void ClearCards()
    {
        foreach (var obj in leaderCards)
        {
            Destroy(obj);
        }

        leaderCards = new();
    }

    GameObject InstantPrefab(GameObject prefab)
    {
        var go = Instantiate(prefab);
        go.transform.parent = content.transform;
        go.transform.localScale = Vector3.one;

        return go;
    }
}

/// <summary>
/// Класс для парсинга метаданных лидерборда
/// </summary>
[Serializable]
public class LeaderboardMetadata
{
    public string name;
    public int sex;
    public int level;
}
