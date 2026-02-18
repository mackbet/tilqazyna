using UnityEngine;
using UnityEngine.Events;

public class ItemContainerEvents : MonoBehaviour
{
    [SerializeField] private ItemContainer _container;
    [SerializeField] private UnityEvent _onTransferStarted;
    [SerializeField] private UnityEvent _onTransferFinished;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_container)
            _container = GetComponent<ItemContainer>();
    }
#endif

    private void OnEnable()
    {
        _container.TransferStarted += OnTransferStarted;
        _container.TransferFinished += OnTransferFinished;
    }

    private void OnDisable()
    {
        _container.TransferStarted -= OnTransferStarted;
        _container.TransferFinished -= OnTransferFinished;
    }

    private void OnTransferStarted(IItemReceiver receiver) => _onTransferStarted.Invoke();
    private void OnTransferFinished() => _onTransferFinished.Invoke();
}
