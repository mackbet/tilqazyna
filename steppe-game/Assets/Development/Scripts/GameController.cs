using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float finishDelay = 1f;
    [SerializeField] private bool startWithDelay = true;

    public event Action OnGameStarted;
    public event Action OnGameFinished;
    public event Action OnGameFailed;

    private CancellationTokenSource cancellationTokenSource;

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

    protected async void FinishGame()
    {
        await FinishGameWithDelay(cancellationTokenSource.Token);
    }

    private async Task FinishGameWithDelay(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(finishDelay), token);
            OnGameFinished?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // Игра была завершена до окончания задержки
        }
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