using UnityEngine;
using UnityEngine.EventSystems;

public class VesselPourer : MonoBehaviour
{
    [SerializeField] private Vessel _vessel;
    [SerializeField] private Draggable _draggable;

    private Vessel _targetVessel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_draggable.IsDragging || _vessel.Items.Count == 0) return;

        var vessel = other.GetComponent<Vessel>();
        if (vessel == null || vessel == _vessel) return;

        _targetVessel = vessel;
        _draggable.DragEnded += OnDragEnded;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var vessel = other.GetComponent<Vessel>();
        if (vessel == null || vessel != _targetVessel) return;

        ClearTarget();
    }

    private void OnDragEnded(Draggable draggable, PointerEventData eventData)
    {
        if (_targetVessel == null)
        {
            ClearTarget();
            return;
        }

        var target = _targetVessel;
        ClearTarget();

        for (int i = _vessel.Items.Count - 1; i >= 0; i--)
        {
            var item = _vessel.Items[i];
            if (target.CanAccept(item))
            {
                _vessel.RemoveItem(item);
                target.AddItem(item);
            }
        }
    }

    private void ClearTarget()
    {
        _draggable.DragEnded -= OnDragEnded;
        _targetVessel = null;
    }

    private void OnDisable()
    {
        ClearTarget();
    }
}
