using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ItemContainerSound : MonoBehaviour
{
    [SerializeField] private ItemContainer _container;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _transferLoop;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_container)
            _container = GetComponent<ItemContainer>();

        if (!_audioSource)
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
        }
    }
#endif

    private void Awake()
    {
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        _container.TransferStarted += OnTransferStarted;
        _container.TransferFinished += OnTransferFinished;
        StateManager.StateChanged += SyncVolume;
        SyncVolume();
    }

    private void OnDisable()
    {
        _container.TransferStarted -= OnTransferStarted;
        _container.TransferFinished -= OnTransferFinished;
        StateManager.StateChanged -= SyncVolume;
        _audioSource.Stop();
    }

    private void SyncVolume()
    {
        if (StateManager.Instance != null)
            _audioSource.volume = Mathf.Clamp(StateManager.Instance.SoundVolume / 10f, 0f, 1f);
    }

    private void OnTransferStarted(IItemReceiver receiver)
    {
        if (_transferLoop == null) return;
        _audioSource.clip = _transferLoop;
        _audioSource.Play();
    }

    private void OnTransferFinished()
    {
        _audioSource.Stop();
    }
}
