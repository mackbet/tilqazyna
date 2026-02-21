using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VesselPourer : MonoBehaviour
{
    [SerializeField] private Vessel _vessel;
    [SerializeField] private Draggable _draggable;

    private readonly List<IItemReceiver> _targets = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_draggable.IsDragging || _vessel.Items.Count == 0) return;

        var receiver = other.GetComponent<IItemReceiver>();
        if (receiver == null || receiver as Component == _vessel) return;
        if (_targets.Contains(receiver)) return;

        if (_targets.Count == 0)
            _draggable.DragEnded += OnDragEnded;

        _targets.Add(receiver);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var receiver = other.GetComponent<IItemReceiver>();
        if (receiver == null) return;

        _targets.Remove(receiver);

        if (_targets.Count == 0)
            _draggable.DragEnded -= OnDragEnded;
    }

    private void OnDragEnded(Draggable draggable, PointerEventData eventData)
    {
        _draggable.DragEnded -= OnDragEnded;

        for (int i = _vessel.Items.Count - 1; i >= 0; i--)
        {
            var item = _vessel.Items[i];

            foreach (var target in _targets)
            {
                if (target.CanAccept(item))
                {
                    _vessel.RemoveItem(item);
                    target.AddItem(item);
                    break;
                }
            }
        }

        _targets.Clear();
    }

    private void OnDisable()
    {
        _draggable.DragEnded -= OnDragEnded;
        _targets.Clear();
    }
}
