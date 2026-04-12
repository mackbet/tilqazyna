using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CityProgressChecker : MonoBehaviour
{
    private const string KEY_COMPLETED_GAMES = "completedGames";

    [SerializeField] private CityConfig _config;
    [SerializeField] private TrophyPanel _trophyPanelPrefab;
    [SerializeField] private bool _debugAlwaysShow;

    // Состояние общее для всех городов — загружается один раз за сессию
    private static readonly HashSet<string> s_completedGames = new();
    private static Task s_loadTask;

    private void OnEnable()
    {
        GameController.GameCompleted += OnGameCompleted;
        _ = CheckOnOpen();
    }

    private void OnDisable()
    {
        GameController.GameCompleted -= OnGameCompleted;
    }

    private async void OnGameCompleted(string gameId)
    {
        if (s_completedGames.Contains(gameId)) return;
        s_completedGames.Add(gameId);

        await RealtimeManager.Instance.SaveData(new Dictionary<string, object>
        {
            { KEY_COMPLETED_GAMES, string.Join(",", s_completedGames) }
        });

        TryShowTrophy();
    }

    private async Task CheckOnOpen()
    {
        await EnsureLoaded();
        TryShowTrophy();
    }

    private static async Task EnsureLoaded()
    {
        if (s_loadTask != null)
        {
            await s_loadTask;
            return;
        }
        s_loadTask = LoadProgress();
        await s_loadTask;
    }

    private static async Task LoadProgress()
    {
        if (AndroidLoginManager.Instance != null && !AndroidLoginManager.Instance.IsAuthenticated)
        {
            var tcs = new TaskCompletionSource<bool>();
            void OnLogin(string _)
            {
                AndroidLoginManager.Instance.OnLoginSuccess -= OnLogin;
                tcs.TrySetResult(true);
            }
            AndroidLoginManager.Instance.OnLoginSuccess += OnLogin;
            await tcs.Task;
        }

        var result = await RealtimeManager.Instance.LoadData(
            new HashSet<string> { KEY_COMPLETED_GAMES });

        if (result != null && result.TryGetValue(KEY_COMPLETED_GAMES, out var raw))
        {
            var csv = raw?.ToString() ?? "";
            foreach (var id in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                s_completedGames.Add(id);
        }
    }

    private void TryShowTrophy()
    {
        if (_config == null) return;
        if (_config.requiredGames == null || _config.requiredGames.Length == 0) return;

        foreach (var game in _config.requiredGames)
        {
            if (!s_completedGames.Contains(game.GameId)) return;
        }

        if (!_debugAlwaysShow)
        {
            if (IsCityAwarded()) return;
            MarkCityAwarded();
        }
        var panel = CanvasViewManager.Instance.Load(_trophyPanelPrefab, false);
        panel.Show(_config.trophyPrefab);
    }

    private bool IsCityAwarded() =>
        PlayerPrefs.GetInt($"trophy_{_config.city}", 0) == 1;

    private void MarkCityAwarded() =>
        PlayerPrefs.SetInt($"trophy_{_config.city}", 1);
}
