using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private DialoguePhrase[] dialoguePhrases;
    [SerializeField] private float delayBetweenPhrases = 0.5f;
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
        try
        {
            foreach (var phrase in dialoguePhrases)
            {
                currentPhrase = phrase;
                display.text = phrase.Phrase.LocalizedString.GetLocalizedString();
                
                await phrase.Play(token);
                
                if (delayBetweenPhrases > 0)
                    await Task.Delay(TimeSpan.FromSeconds(delayBetweenPhrases), token);
                
                currentPhrase = null;
            }
        }
        catch (OperationCanceledException) { }
    }

    [Serializable]
    public class DialoguePhrase
    {
        public Phrase Phrase => phrase;
        [SerializeField] private Phrase phrase;
        [SerializeField] private ActivateableObject[] onStartedActivateables;
        [SerializeField] private ActivateableObject[] onFinishedActivateables;

        private AudioSource audioSource;

        public async Task Play(CancellationToken token)
        {
            ActivateObjects(onStartedActivateables);

            var loadOp = phrase.LocalizedAudio.LoadAssetAsync();
            while (!loadOp.IsDone)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
            }

            if (loadOp.Result == null) return;

            audioSource = AudioManager.Instance.PlaySound(loadOp.Result, 1f);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(loadOp.Result.length), token);
            }
            catch (OperationCanceledException)
            {
                Stop();
                throw;
            }

            ActivateObjects(onFinishedActivateables);
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