using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VesselCookingSound : MonoBehaviour
{
    [SerializeField] private TimedVessel _vessel;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _cookingLoop;
    [SerializeField] private AudioClip _completedClip;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_audioSource)
            _audioSource = GetComponent<AudioSource>();

        if (!_vessel)
            _vessel = GetComponent<TimedVessel>();
    }
#endif

    private void OnEnable()
    {
        _vessel.CookingStarted += OnCookingStarted;
        _vessel.CookingFinished += OnCookingFinished;
        StateManager.StateChanged += SyncVolume;
        SyncVolume();
    }

    private void OnDisable()
    {
        _vessel.CookingStarted -= OnCookingStarted;
        _vessel.CookingFinished -= OnCookingFinished;
        StateManager.StateChanged -= SyncVolume;
        _audioSource.Stop();
    }

    private void SyncVolume()
    {
        if (StateManager.Instance != null)
            _audioSource.volume = Mathf.Clamp(StateManager.Instance.SoundVolume / 10f, 0f, 1f);
    }

    private void OnCookingStarted()
    {
        if (_cookingLoop == null) return;
        _audioSource.clip = _cookingLoop;
        _audioSource.Play();
    }

    private void OnCookingFinished()
    {
        _audioSource.Stop();
        SoundManager.Instance?.PlaySound(_completedClip);
    }
}
