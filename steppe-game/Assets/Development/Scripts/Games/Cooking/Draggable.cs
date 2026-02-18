using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Canvas _canvas;
    [SerializeField, Range(5f, 30f)] private float _followSpeed = 15f;
    [SerializeField] private float _returnDuration = 0.5f;

    private Transform _originalParent;
    private int _originalSiblingIndex;
    private Vector2 _originalPosition;
    private Vector2 _dragOffset;
    private Vector2 _targetPosition;
    private bool _isDragging;
    private bool _canDrag = true;
    private bool _accepted;
    private Tweener _returnTween;

    public RectTransform RectTransform => _rectTransform;
    public bool CanDrag { get => _canDrag; set => _canDrag = value; }
    public bool IsDragging => _isDragging;

    public event Action<Draggable, PointerEventData> DragStarted;
    public event Action<Draggable, PointerEventData> Dragging;
    public event Action<Draggable, PointerEventData> DragEnded;
    public event Action<Draggable> Returned;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (!_rectTransform)
            _rectTransform = GetComponent<RectTransform>();
    }

#endif

    private void Awake()
    {
        _originalParent = _rectTransform.parent;
        _originalSiblingIndex = _rectTransform.GetSiblingIndex();
        _originalPosition = _rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (!_isDragging) return;

        float t = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
        _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, _targetPosition, t);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_canDrag) return;

        KillReturnTween();

        _isDragging = true;
        _accepted = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out var localPoint
        );

        _rectTransform.SetParent(_canvas.transform, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out localPoint
        );

        _dragOffset = _rectTransform.anchoredPosition - localPoint;
        _targetPosition = _rectTransform.anchoredPosition;

        DragStarted?.Invoke(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out var localPoint
        );

        _targetPosition = localPoint + _dragOffset;

        Dragging?.Invoke(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        DragEnded?.Invoke(this, eventData);

        if (!_accepted)
            ReturnToOrigin();
    }

    public void Accept() => _accepted = true;

    public void ReturnToOrigin()
    {
        KillReturnTween();
        _canDrag = false;

        _rectTransform.SetParent(_originalParent, true);
        _rectTransform.SetSiblingIndex(_originalSiblingIndex);

        _returnTween = _rectTransform
            .DOAnchorPos(_originalPosition, _returnDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _canDrag = true;
                _returnTween = null;
                Returned?.Invoke(this);
            });
    }

    private void KillReturnTween()
    {
        if (_returnTween != null && _returnTween.IsActive())
        {
            _returnTween.Kill();
            _returnTween = null;
            _canDrag = true;
        }
    }

    private void OnDisable()
    {
        KillReturnTween();
        _isDragging = false;
    }
}
