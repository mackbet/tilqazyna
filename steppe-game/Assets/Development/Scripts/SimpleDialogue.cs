using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleDialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private DialogueEntry[] dialogueEntries;
    [SerializeField] private bool playEveryTime = false;

    private bool isPlayed = false;
    private CancellationTokenSource cancellationTokenSource;
    private AudioSource currentAudioSource;

    private void OnEnable()
    {
        if (isPlayed && !playEveryTime) return;

        cancellationTokenSource = new CancellationTokenSource();
        _ = PlayDialogueAsync(cancellationTokenSource.Token);
        isPlayed = true;
    }

    private void OnDisable()
    {
        StopCurrentAudio();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    private async Task PlayDialogueAsync(CancellationToken token)
    {
        foreach (var entry in dialogueEntries)
        {
            ActivateObjects(entry.onStartedActivateables);
            entry.onStartedEvent?.Invoke();

            display.text = entry.text;

            if (entry.audioClip != null)
            {
                currentAudioSource = AudioManager.Instance.PlaySound(entry.audioClip, 1f);
                float audioLength = entry.audioClip.length;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(audioLength + entry.delayAfter), token);
                }
                catch (OperationCanceledException)
                {
                    StopCurrentAudio();
                    throw;
                }

                currentAudioSource = null;
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(entry.delayAfter), token);
            }

            ActivateObjects(entry.onFinishedActivateables);
            entry.onFinishedEvent?.Invoke();
        }
    }

    private void StopCurrentAudio()
    {
        if (currentAudioSource != null && currentAudioSource.gameObject != null)
        {
            currentAudioSource.Stop();
            Destroy(currentAudioSource.gameObject);
            currentAudioSource = null;
        }
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
        public float delayAfter = 0.5f;
        public ActivateableObject[] onStartedActivateables;
        public ActivateableObject[] onFinishedActivateables;
        public UnityEvent onStartedEvent;
        public UnityEvent onFinishedEvent;
    }
}