using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private DialoguePhrase[] dialoguePhrases;
    [SerializeField] private bool playEveryTime = false;
    [SerializeField] private Button nextButton;

    private bool isPlayed = false;
    private CancellationTokenSource cancellationTokenSource;
    private DialoguePhrase currentPhrase;
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

        currentPhrase?.Stop();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
        buttonClickedTCS = null;
    }

    private async Task PlayDialogueAsync(CancellationToken token)
    {
        foreach (var phrase in dialoguePhrases)
        {
            currentPhrase = phrase;
            display.text = phrase.Phrase.LocalizedString.GetLocalizedString();

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }

            buttonClickedTCS = new TaskCompletionSource<bool>();
            await phrase.Play(token, buttonClickedTCS);

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }

            currentPhrase = null;
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }
    }

    private void OnNextButtonClicked()
    {
        buttonClickedTCS?.TrySetResult(true);
    }

    [Serializable]
    public class DialoguePhrase
    {
        public Phrase Phrase => phrase;
        [SerializeField] private Phrase phrase;
        [SerializeField] private float delayAfterPhrase = 0.5f;
        [SerializeField] private ActivateableObject[] onStartedActivateables;
        [SerializeField] private ActivateableObject[] onFinishedActivateables;
        public UnityEvent OnStartedEvent;
        public UnityEvent OnEndedEvent;

        private AudioSource audioSource;

        public async Task Play(CancellationToken token, TaskCompletionSource<bool> buttonClickedTCS)
        {
            ActivateObjects(onStartedActivateables);
            OnStartedEvent?.Invoke();

            // Проверяем LocalizedAudio
            if (phrase.LocalizedAudio == null || phrase.LocalizedAudio.IsEmpty)
            {
                Debug.LogWarning($"LocalizedAudio не настроен для фразы. Пропускаем воспроизведение.");
            }
            else
            {
                var loadOp = phrase.LocalizedAudio.LoadAssetAsync();

                while (!loadOp.IsDone)
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }

                if (loadOp.Result != null)
                {
                    audioSource = AudioManager.Instance.PlaySound(loadOp.Result, 1f);
                }
            }

            // Ждем нажатия кнопки
            try
            {
                await buttonClickedTCS.Task;
            }
            catch (OperationCanceledException)
            {
                Stop();
                throw;
            }

            Stop();

            ActivateObjects(onFinishedActivateables);
            OnEndedEvent?.Invoke();
        }

        public void Stop()
        {
            if (audioSource != null && audioSource.gameObject != null)
            {
                audioSource.Stop();
                Destroy(audioSource.gameObject);
                audioSource = null;
            }
        }

        private void ActivateObjects(ActivateableObject[] objects)
        {
            if (objects == null) return;
            foreach (var obj in objects) obj.Call();
        }
    }
}