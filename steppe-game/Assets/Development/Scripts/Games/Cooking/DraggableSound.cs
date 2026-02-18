using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSound : MonoBehaviour
{
    [SerializeField] private Draggable _draggable;
    [SerializeField] private AudioClip _grabClip;
    [SerializeField] private AudioClip _returnClip;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_draggable)
            _draggable = GetComponent<Draggable>();
    }
#endif

    private void OnEnable()
    {
        _draggable.DragStarted += OnDragStarted;
        _draggable.Returned += OnReturned;
    }

    private void OnDisable()
    {
        _draggable.DragStarted -= OnDragStarted;
        _draggable.Returned -= OnReturned;
    }

    private void OnDragStarted(Draggable draggable, PointerEventData eventData)
    {
        SoundManager.Instance?.PlaySound(_grabClip);
    }

    private void OnReturned(Draggable draggable)
    {
        SoundManager.Instance?.PlaySound(_returnClip);
    }
}
