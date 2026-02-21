using DG.Tweening;
using UnityEngine;

public class ItemContainerTransferAnimator : MonoBehaviour
{
    [SerializeField] private ItemContainer _container;
    [SerializeField] private Draggable _draggable;
    [SerializeField] private ImageAnimator _animator;
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _tiltAngle;

    private Sequence _sequence;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_container)
            _container = GetComponent<ItemContainer>();
        if (!_draggable)
            _draggable = GetComponent<Draggable>();
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
        KillSequence();
    }

    private void OnTransferStarted(IItemReceiver receiver)
    {
        var vessel = receiver as Vessel;
        if (vessel == null || vessel.TransferTarget == null) return;

        KillSequence();
        var rt = _draggable.RectTransform;

        _sequence = DOTween.Sequence()
            .Join(rt.DOMove(vessel.TransferTarget.position, _moveDuration).SetEase(Ease.OutQuad))
            .Join(rt.DOLocalRotate(new Vector3(0f, 0f, _tiltAngle), _moveDuration).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                _sequence = null;
                if (_animator)
                {
                    _animator.gameObject.SetActive(true);
                    _animator.StartAnimation();
                }
            });
    }

    private void OnTransferFinished()
    {
        KillSequence();
        _draggable.RectTransform.localRotation = Quaternion.identity;

        if (_animator)
        {
            _animator.StopAnimation();
            _animator.gameObject.SetActive(false);
        }
    }

    private void KillSequence()
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}
