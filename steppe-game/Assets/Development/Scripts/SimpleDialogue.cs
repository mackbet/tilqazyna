using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleDialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private DialogueEntry[] dialogueEntries;
    [SerializeField] private bool playEveryTime = false;
    [SerializeField] private Button nextButton;

    private bool isPlayed = false;
    private CancellationTokenSource cancellationTokenSource;
    private TaskCompletionSource<bool> buttonClickedTCS;

    private void OnEnable()
    {
        if (isPlayed && !playEveryTime) return;

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        cancellationTokenSource = new CancellationTokenSource();
        _ = PlayDialogueAsync(cancellationTokenSource.Token);
        isPlayed = true;
    }

    private void OnDisable()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        StopCurrentAudio();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
        buttonClickedTCS = null;
    }

    private async Task PlayDialogueAsync(CancellationToken token)
    {
        foreach (var entry in dialogueEntries)
        {
            ActivateObjects(entry.onStartedActivateables);
            entry.onStartedEvent?.Invoke();

            display.text = entry.text;

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }

            float duration = entry.delayAfter;

            if (entry.audioClip != null)
            {
                SoundManager.Instance.PlayVoice(entry.audioClip);
                duration = entry.audioClip.length + entry.delayAfter;
            }

            // Ждем либо нажатия кнопки, либо окончания времени (с учетом игрового времени)
            buttonClickedTCS = new TaskCompletionSource<bool>();
            
            try
            {
                await WaitForButtonOrTime(duration, token);
            }
            catch (OperationCanceledException)
            {
                StopCurrentAudio();
                throw;
            }

            StopCurrentAudio();

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }

            ActivateObjects(entry.onFinishedActivateables);
            entry.onFinishedEvent?.Invoke();
            
            buttonClickedTCS = null;
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }
    }

    private async Task WaitForButtonOrTime(float duration, CancellationToken token)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Проверяем, была ли нажата кнопка
            if (buttonClickedTCS.Task.IsCompleted)
            {
                return;
            }
            
            // Проверяем отмену
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            
            // Ждем один кадр
            await Task.Yield();
            
            // Увеличиваем время на deltaTime (игровое время)
            elapsed += Time.deltaTime;
        }
    }

    private void OnNextButtonClicked()
    {
        buttonClickedTCS?.TrySetResult(true);
    }

    private void StopCurrentAudio()
    {
        SoundManager.Instance?.StopVoice();
    }

    private void ActivateObjects(ActivateableObject[] objects)
    {
        if (objects == null) return;
        foreach (var obj in objects) obj.Call();
    }

    [Serializable]
    public class DialogueEntry
    {
        [TextArea(2, 5)]
        public string text;
        public AudioClip audioClip;
        [Tooltip("Задержка после окончания аудио (или базовая длительность, если аудио нет)")]
        public float delayAfter = 0.5f;
        public ActivateableObject[] onStartedActivateables;
        public ActivateableObject[] onFinishedActivateables;
        public UnityEvent onStartedEvent;
        public UnityEvent onFinishedEvent;
    }
}