using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [Range(0, 1)]
    [SerializeField] private float volume;

    private void OnEnable()
    {
        SoundManager.Instance.PlayMusic(music, volume);
    }
}
