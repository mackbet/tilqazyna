using UnityEngine;

[RequireComponent(typeof(CustomImageButton))]
public class CustomImageButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private CustomImageButton button;
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float pressVolume = 1f;
    [SerializeField] private float releaseVolume = 1f;
    [SerializeField] private float clickVolume = 1f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!button)
            button = GetComponent<CustomImageButton>();
    }
#endif

    private void Awake()
    {
        if (!button)
            button = GetComponent<CustomImageButton>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.OnButtonPressed += OnButtonPressed;
            button.OnButtonReleased += OnButtonReleased;
            button.OnButtonClicked += OnButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.OnButtonPressed -= OnButtonPressed;
            button.OnButtonReleased -= OnButtonReleased;
            button.OnButtonClicked -= OnButtonClicked;
        }
    }

    private void OnButtonPressed()
    {
        if (pressSound != null)
        {
            PlaySound(pressSound, pressVolume);
        }
    }

    private void OnButtonReleased()
    {
        if (releaseSound != null)
        {
            PlaySound(releaseSound, releaseVolume);
        }
    }

    private void OnButtonClicked()
    {
        if (clickSound != null)
        {
            PlaySound(clickSound, clickVolume);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        AudioManager.Instance.PlaySound(clip, volume);
    }
}

