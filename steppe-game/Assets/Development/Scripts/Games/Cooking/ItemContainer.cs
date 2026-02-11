using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemContainer : MonoBehaviour
{
    [SerializeField] private ItemData _item;
    [SerializeField] private bool _isResource;

    private Draggable _draggable;
    private Vessel _currentVessel;

    public ItemData Item => _item;
    public bool IsResource => _isResource;
    public bool IsEmpty => _item == null;

    public event Action<ItemData> ItemChanged;

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

        var vessel = other.GetComponent<Vessel>();
        if (vessel == null || IsEmpty || !vessel.CanAccept(_item)) return;

        _currentVessel = vessel;
        _draggable.DragEnded += OnDragEnded;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var vessel = other.GetComponent<Vessel>();
        if (vessel == null || vessel != _currentVessel) return;

        ClearVessel();
    }

    private void OnDragEnded(Draggable draggable, PointerEventData eventData)
    {
        if (_currentVessel == null || IsEmpty || !_currentVessel.CanAccept(_item))
        {
            ClearVessel();
            return;
        }

        var vessel = _currentVessel;
        var item = Take();
        ClearVessel();
        vessel.AddItem(item);
    }

    private void ClearVessel()
    {
        if (_draggable != null)
            _draggable.DragEnded -= OnDragEnded;

        _currentVessel = null;
    }

    private void OnDisable()
    {
        ClearVessel();
    }
}
