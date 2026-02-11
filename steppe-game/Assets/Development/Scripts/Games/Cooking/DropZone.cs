using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class DropZone : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Material _highlightMaterial;

    private Draggable _hovering;

    public event Action<DropZone, Draggable> ItemDropped;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var draggable = other.GetComponent<Draggable>();
        if (draggable == null || !draggable.IsDragging) return;

        _hovering = draggable;
        draggable.DragEnded += OnItemDropped;
        SetHighlight(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var draggable = other.GetComponent<Draggable>();
        if (draggable == null || draggable != _hovering) return;

        draggable.DragEnded -= OnItemDropped;
        _hovering = null;
        SetHighlight(false);
    }

    private void OnItemDropped(Draggable draggable, PointerEventData eventData)
    {
        draggable.DragEnded -= OnItemDropped;
        _hovering = null;
        SetHighlight(false);

        draggable.Accept();
        ItemDropped?.Invoke(this, draggable);
    }

    private void SetHighlight(bool active)
    {
        if (_image != null)
            _image.material = active ? _highlightMaterial : null;
    }

    private void OnDisable()
    {
        if (_hovering != null)
        {
            _hovering.DragEnded -= OnItemDropped;
            _hovering = null;
        }

        SetHighlight(false);
    }
}
