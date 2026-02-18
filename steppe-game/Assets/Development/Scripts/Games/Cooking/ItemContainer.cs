using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemContainer : MonoBehaviour
{
    [SerializeField] private ItemData _item;
    [SerializeField] private bool _isResource;
    [SerializeField] private float _transferDelay;

    private Draggable _draggable;
    private IItemReceiver _currentReceiver;
    private Coroutine _transferCoroutine;

    public ItemData Item => _item;
    public bool IsResource => _isResource;
    public bool IsEmpty => _item == null;

    public event Action<ItemData> ItemChanged;
    public event Action<IItemReceiver> TransferStarted;
    public event Action TransferFinished;

    private void Awake()
    {
        _draggable = GetComponent<Draggable>();
    }

    public ItemData Take()
    {
        if (_item == null) return null;

        var taken = _item;

        if (!_isResource)
        {
            _item = null;
            ItemChanged?.Invoke(null);
        }

        return taken;
    }

    public void Put(ItemData item)
    {
        _item = item;
        ItemChanged?.Invoke(_item);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_draggable == null || !_draggable.IsDragging) return;

        var receiver = other.GetComponent<IItemReceiver>();
        if (receiver == null || IsEmpty || !receiver.CanAccept(_item)) return;

        _currentReceiver = receiver;
        _draggable.DragEnded += OnDragEnded;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var vessel = other.GetComponent<IItemReceiver>();
        if (vessel == null || vessel != _currentReceiver) return;

        ClearReceiver();
    }

    private void OnDragEnded(Draggable draggable, PointerEventData eventData)
    {
        if (_currentReceiver == null || IsEmpty || !_currentReceiver.CanAccept(_item))
        {
            ClearReceiver();
            return;
        }

        var vessel = _currentReceiver;
        ClearReceiver();

        if (_transferDelay > 0f)
            _transferCoroutine = StartCoroutine(TransferWithDelay(vessel));
        else
            Transfer(vessel);
    }

    private IEnumerator TransferWithDelay(IItemReceiver vessel)
    {
        _draggable.Accept();
        _draggable.CanDrag = false;
        TransferStarted?.Invoke(vessel);

        yield return new WaitForSeconds(_transferDelay);

        _transferCoroutine = null;
        Transfer(vessel);

        TransferFinished?.Invoke();
        _draggable.ReturnToOrigin();
    }

    private void Transfer(IItemReceiver vessel)
    {
        var item = Take();
        vessel.AddItem(item);
    }

    private void ClearReceiver()
    {
        if (_draggable != null)
            _draggable.DragEnded -= OnDragEnded;

        _currentReceiver = null;
    }

    private void OnDisable()
    {
        ClearReceiver();

        if (_transferCoroutine != null)
        {
            StopCoroutine(_transferCoroutine);
            _transferCoroutine = null;
            _draggable.ReturnToOrigin();
        }
    }
}
