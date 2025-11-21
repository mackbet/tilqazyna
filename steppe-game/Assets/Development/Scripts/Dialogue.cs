using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private DialoguePhrase[] dialoguePhrases;
    [SerializeField] private bool playEveryTime = false;

    private bool isPlayed = false;
    private CancellationTokenSource cancellationTokenSource;
    private DialoguePhrase currentPhrase;

    private void OnEnable()
    {
        if (isPlayed && !playEveryTime) return;

        cancellationTokenSource = new CancellationTokenSource();
        _ = PlayDialogueAsync(cancellationTokenSource.Token);
        isPlayed = true;
    }

    private void OnDisable()
    {
        currentPhrase?.Stop();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    private async Task PlayDialogueAsync(CancellationToken token)
    {
        foreach (var phrase in dialoguePhrases)
        {
            currentPhrase = phrase;
            display.text = phrase.Phrase.LocalizedString.GetLocalizedString();

            await phrase.Play(token);

            currentPhrase = null;
        }
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

        public async Task Play(CancellationToken token)
        {
            ActivateObjects(onStartedActivateables);
            OnStartedEvent?.Invoke();

            // Проверяем LocalizedAudio
            if (phrase.LocalizedAudio == null || phrase.LocalizedAudio.IsEmpty)
            {
                Debug.LogWarning($"LocalizedAudio не настроен для фразы. Пропускаем воспроизведение.");
                await Task.Delay(TimeSpan.FromSeconds(delayAfterPhrase), token);
                ActivateObjects(onFinishedActivateables);
                return;
            }

            var loadOp = phrase.LocalizedAudio.LoadAssetAsync();

            while (!loadOp.IsDone)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
            }

            float audioLength = 0;

            if (loadOp.Result != null)
            {
                audioSource = AudioManager.Instance.PlaySound(loadOp.Result, 1f);
                audioLength = loadOp.Result.length;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(audioLength + delayAfterPhrase), token);
            }
            catch (OperationCanceledException)
            {
                Stop();
                throw;
            }

            ActivateObjects(onFinishedActivateables);
            OnEndedEvent?.Invoke();
            audioSource = null;
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