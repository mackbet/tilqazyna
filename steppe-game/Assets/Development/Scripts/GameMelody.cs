using UnityEngine;

public class GameMelody : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float volume = 1;

    private AudioClip lastMusic;
    void OnEnable()
    {
        lastMusic = SoundManager.Instance.GetMusicAudioSource().clip;
        SoundManager.Instance.PlayMusic(audioClip);
    }

    void OnDisable()
    {
        SoundManager.Instance.PlayMusic(lastMusic, volume);
    }
}
