using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Воспроизводит аудио клип и возвращает созданный объект с AudioSource
    /// </summary>
    /// <param name="clip">Аудио клип для воспроизведения</param>
    /// <param name="volume">Громкость (0-1)</param>
    /// <param name="loop">Зациклить звук?</param>
    /// <returns>GameObject с AudioSource компонентом</returns>
    public AudioSource PlaySound(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Попытка воспроизвести null AudioClip!");
            return null;
        }

        GameObject soundObject = new GameObject($"Sound_{clip.name}");
        soundObject.transform.SetParent(transform);

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.Play();

        // Если звук не зациклен, удаляем объект после завершения
        if (!loop)
        {
            Destroy(soundObject, clip.length);
        }

        return audioSource;
    }

    /// <summary>
    /// Воспроизводит звук в определенной точке пространства
    /// </summary>
    public AudioSource PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource audioSource = PlaySound(clip, volume, false);
        if (audioSource != null)
        {
            audioSource.transform.position = position;
            audioSource.spatialBlend = 1f; // 3D звук
        }
        return audioSource;
    }

    /// <summary>
    /// Останавливает все звуки
    /// </summary>
    public void StopAllSounds()
    {
        AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
}