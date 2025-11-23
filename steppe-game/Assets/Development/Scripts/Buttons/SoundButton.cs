using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public class SoundButton : CustomImageButton
{
    [SerializeField] private LocalizedAudioClip localizedAudio;
    [SerializeField] private float volume = 1f;

    private AudioSource currentAudioSource;
    private CancellationTokenSource cancellationTokenSource;

    private void OnDisable()
    {
        StopCurrentAudio();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    protected override void Clicked()
    {
        base.Clicked();
        PlayAudio();
    }

    public void SetAudio(LocalizedAudioClip audio)
    {
        localizedAudio = audio;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
    }

    public async void PlayAudio()
    {
        if (localizedAudio == null || localizedAudio.IsEmpty)
        {
            Debug.LogWarning("SoundButton: LocalizedAudioClip не установлен!");
            return;
        }

        StopCurrentAudio();

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        await PlayAudioAsync(cancellationTokenSource.Token);
    }

    private async Task PlayAudioAsync(CancellationToken token)
    {
        var loadOp = localizedAudio.LoadAssetAsync();

        while (!loadOp.IsDone)
        {
            await Task.Yield();

            if (token.IsCancellationRequested)
                return;
        }

        if (loadOp.Result != null)
        {
            currentAudioSource = AudioManager.Instance.PlaySound(loadOp.Result, volume);
        }
        else
        {
            Debug.LogWarning("SoundButton: Не удалось загрузить аудио!");
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

    public bool IsPlaying()
    {
        return currentAudioSource != null && currentAudioSource.isPlaying;
    }

    public void Stop()
    {
        StopCurrentAudio();
        cancellationTokenSource?.Cancel();
    }
}
