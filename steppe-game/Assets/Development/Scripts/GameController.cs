using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] protected UIPanelFinish finishPanel;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float finishDelay = 1f;
    [SerializeField] private bool startWithDelay = true;
    [SerializeField] private string _gameId;
    public string GameId => _gameId;

    public event Action OnGameStarted;
    public event Action OnGameFinished;
    public event Action OnGameFailed;
    public event Action<int> OnLivesChanged;

    public static event Action<string> GameCompleted;

    protected int lives = 0;
    public int Lives => lives;

    private CancellationTokenSource cancellationTokenSource;
    private bool _isFinished;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_gameId == "")
            _gameId = name;
    }
#endif

    protected virtual void OnEnable()
    {
        cancellationTokenSource = new CancellationTokenSource();
        if (startWithDelay)
            StartGameWithDelay();
    }

    protected virtual void OnDisable()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        _isFinished = false;
    }

    public void InitializeGameManualy()
    {
        InitializeGame();
    }

    private async void StartGameWithDelay()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(startDelay), cancellationTokenSource.Token);
            InitializeGame();
            OnGameStarted?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // Игра была отменена до начала
        }
    }

    protected virtual void InitializeGame()
    {

    }

    protected void SetLives(int value)
    {
        lives = value;
        OnLivesChanged?.Invoke(lives);
    }

    protected async void FinishGame()
    {
        await FinishGameWithDelay(cancellationTokenSource.Token);
    }

    private async Task FinishGameWithDelay(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(finishDelay), token);
            _isFinished = true;
            OnGameFinished?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // Игра была завершена до окончания задержки
        }
    }

    public void NotifyCompleted()
    {
        if (_isFinished && !string.IsNullOrEmpty(_gameId))
            GameCompleted?.Invoke(_gameId);
    }

    protected async void FailGame()
    {
        await FailGameWithDelay(cancellationTokenSource.Token);
    }

    private async Task FailGameWithDelay(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(finishDelay), token);
            OnGameFailed?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // Игра была завершена до окончания задержки
        }
    }
}