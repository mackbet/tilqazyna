using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Development.Objects.Cooking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private List<DraggableObject> draggableObjectList;
    [SerializeField] private List<Order> _orders;
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private Kumis _kumis;

    [Header("Animations")] 
    [SerializeField] private SpriteAnimator _waterAnimator;
    [SerializeField] private SpriteAnimator _yeastAnimator;
    [SerializeField] private SpriteAnimator _saltAnimator;
    [SerializeField] private SpriteAnimator _sugarAnimator;
    [SerializeField] private SpriteAnimator _flourAnimator;
    [SerializeField] private SpriteAnimator _oilAnimator;
    [SerializeField] private SpriteAnimator _milkAnimator;
    [SerializeField] private SpriteAnimator _kumisAnimator;

    [Header("Anchors")] 
    [SerializeField] private RectTransform _kumisCup;
    [SerializeField] private RectTransform _pot;
    [SerializeField] private RectTransform _plate;

    private Dictionary<RectTransform, ObjectState> _objectStates;
    private GameObject _currentDraggedObject;
    private RectTransform _currentDraggedRect;
    private Vector2 _dragOffset;
    private Vector2 _currentVelocity;
    private Vector2 _targetDragPosition;

    private readonly HashSet<RectTransform> _lockedObjects = new();

    public event Action<IngredientType, IngredientType> OnIngredientAdded;
    public event Action<IngredientType,int, Order[]> OnOrderCompleted;
    public event Action OnIngredientGrabbed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        StopAllAnimations();

        _objectStates = new Dictionary<RectTransform, ObjectState>();
        foreach (var draggableObject in draggableObjectList)
        {
            var rectTransform = draggableObject.RectTransform;
            _objectStates[rectTransform] = new ObjectState
            {
                OriginalParent = rectTransform.parent,
                OriginalPosition = rectTransform.anchoredPosition,
                IngredientType = draggableObject.IngredientType
            };
        }
    }

    private bool _isDragging;

    public void StartDragging(GameObject draggableObject, RectTransform rectTransform, PointerEventData eventData)
    {
        if (_lockedObjects.Contains(rectTransform)) return;

        OnIngredientGrabbed?.Invoke();
        _currentDraggedObject = draggableObject;
        _currentDraggedRect = rectTransform;

        _isDragging = true;
        _currentVelocity = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            eventData.position,
            _canvas.worldCamera,
            out var localMousePosition
        );
        _dragOffset = rectTransform.anchoredPosition - localMousePosition;
        _targetDragPosition = rectTransform.anchoredPosition;
    }

    public void DragObject(RectTransform rectTransform, PointerEventData eventData)
    {
        if (_currentDraggedObject == null || _lockedObjects.Contains(rectTransform)) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            eventData.position,
            _canvas.worldCamera,
            out var localMousePosition
        );

        _targetDragPosition = localMousePosition + _dragOffset;
    }

    public void StopDragging(RectTransform rectTransform, PointerEventData eventData, IngredientType ingredientType)
    {
        if (_currentDraggedObject == null || _lockedObjects.Contains(rectTransform)) return;

        _isDragging = false;
        bool isWrongTry = true;

        foreach (var interactable in _objectStates
                     .Where(pair => pair.Value.IngredientType != ingredientType))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(interactable.Key, eventData.position,
                    _canvas.worldCamera))
            {
                var targetType = interactable.Value.IngredientType;

                if (isTypeMatchCorrectly(ingredientType, targetType, interactable))
                    continue;

                PlayIngredientAnimation(rectTransform, targetType, ingredientType,
                    () => { ResetObjectPosition(rectTransform); });
                isWrongTry = false;
                return;
            }
        }

        foreach (var order in _orders)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(order.RectTransform, eventData.position,
                    _canvas.worldCamera) && !order.IsCompleted)
            {
                OnOrderCompleted?.Invoke(ingredientType, _orders.IndexOf(order), _orders.ToArray());

                ResetObjectPosition(rectTransform);
                return;
            }
        }

        if (isWrongTry)
        {
            _soundManager.PlayWrongAttemptSound();
        }

        ResetObjectPosition(rectTransform);
    }
    
    private static bool isTypeMatchCorrectly(IngredientType ingredientType, IngredientType targetType, 
                                             KeyValuePair<RectTransform, ObjectState> interactable)
    {
        return (targetType is not (IngredientType.DoughBowl or IngredientType.Board or IngredientType.Bowl1
                    or IngredientType.Bowl2
                    or IngredientType.Pot or IngredientType.Pot2 or IngredientType.Cauldron
                    or IngredientType.TrashBin) ||
                !interactable.Key.gameObject.activeInHierarchy)
               ||
               (targetType is not IngredientType.Board &&
                (ingredientType == IngredientType.DoughBaursak ||
                 ingredientType == IngredientType.DoughBershmak ||
                 ingredientType == IngredientType.DoughBowl))
               ||
               (targetType is not (IngredientType.Pot
                    or IngredientType.Pot2) &&
                (ingredientType == IngredientType.Baursak))
               ||
               (targetType is not (IngredientType.Bowl1
                    or IngredientType.Bowl2) &&
                (ingredientType == IngredientType.BaursakCooked ||
                 ingredientType == IngredientType.BershmakCooked ||
                 ingredientType == IngredientType.KurutCooked ||
                 ingredientType == IngredientType.BershmakMeatCutted))
               ||
               (targetType is not IngredientType.TrashBin &&
                (ingredientType == IngredientType.BaursakBurned ||
                 ingredientType == IngredientType.MilkBurned))
               ||
               (targetType is not IngredientType.Cauldron &&
                (ingredientType == IngredientType.Meat ||
                 ingredientType == IngredientType.Sausage ||
                 ingredientType == IngredientType.OnionPeeled ||
                 ingredientType == IngredientType.DoughBershmakCutted))
               ||
               (targetType is not IngredientType.DoughBowl &&
                (ingredientType == IngredientType.Flour  ||
                 ingredientType == IngredientType.Sugar ||
                 ingredientType == IngredientType.Water ||
                 ingredientType == IngredientType.Egg ||
                 ingredientType == IngredientType.Yest))
               ||
               (targetType is not (IngredientType.DoughBowl 
                       or IngredientType.Pot 
                       or IngredientType.Pot2) &&
                (ingredientType == IngredientType.Salt ||
                 ingredientType == IngredientType.Milk ||
                 ingredientType == IngredientType.Oil ||
                 ingredientType == IngredientType.Baursak));
    }


    private void Update()
    {
        bool isTouchActive = Touchscreen.current != null && Touchscreen.current.press.isPressed ||
                             Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (!isTouchActive)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ResetAllObjects();
            }
        }
        else if (_isDragging && _currentDraggedRect != null && !_lockedObjects.Contains(_currentDraggedRect))
        {
            _currentDraggedRect.anchoredPosition = Vector2.SmoothDamp(
                _currentDraggedRect.anchoredPosition,
                _targetDragPosition,
                ref _currentVelocity,
                _smoothTime
            );
        }
    }

    private void FixedUpdate()
    {
        if (!_isDragging)
        {
            foreach (var rectTransform in _objectStates.Keys)
            {
                if (!_lockedObjects.Contains(rectTransform) &&
                    Vector2.Distance(rectTransform.anchoredPosition, _objectStates[rectTransform].OriginalPosition) >
                    0.01f)
                {
                    ResetObjectPosition(rectTransform);
                }
            }
        }
    }

    private void ResetAllObjects()
    {
        foreach (var rectTransform in _objectStates.Keys)
        {
            if (!_lockedObjects.Contains(rectTransform))
            {
                ResetObjectPosition(rectTransform);
            }
        }
    }


    private void ResetObjectPosition(RectTransform rectTransform)
    {
        if (_objectStates.TryGetValue(rectTransform, out var state))
        {
            StartCoroutine(SmoothReturn(rectTransform, state.OriginalPosition));
        }
        else
        {
            _currentDraggedObject = null;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    private IEnumerator SmoothReturn(RectTransform rectTransform, Vector2 targetPosition)
    {
        _lockedObjects.Add(rectTransform);

        var startPosition = rectTransform.anchoredPosition;
        var timeElapsed = 0f;
        var duration = 1f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, timeElapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;

        _lockedObjects.Remove(rectTransform);
        _currentDraggedObject = null;
    }

    private void StopAllAnimations()
    {
        _waterAnimator.StopAnimation();
        _yeastAnimator.StopAnimation();
        _saltAnimator.StopAnimation();
        _sugarAnimator.StopAnimation();
        _flourAnimator.StopAnimation();
        _oilAnimator.StopAnimation();
        _milkAnimator.StopAnimation();
        _kumisAnimator.StopAnimation();
    }


    public void PlayIngredientAnimation(RectTransform rectTransform, IngredientType targetType,
        IngredientType ingredientType) => PlayIngredientAnimation(rectTransform, targetType, ingredientType,
        () => {});
    
    private void PlayIngredientAnimation(RectTransform rectTransform, IngredientType targetType,
        IngredientType ingredientType, Action onComplete)
    {
        Vector3 targetRotation;
        var duration = 1.5f;

        var targetPosition = rectTransform.position;

        var targetRectTransform =
            (from interactable in _objectStates
                where interactable.Value.IngredientType == ingredientType
                select interactable.Key).FirstOrDefault();

        if (targetRectTransform != null)
        {
            Vector3 potOffset = new Vector3(-targetRectTransform.rect.width * 1.5f,
                                  targetRectTransform.rect.height + 150, 0);
            Vector3 plateOffset = new Vector3(-targetRectTransform.rect.width * 1.5f,
                                  targetRectTransform.rect.height + 50, 0);
            switch (targetType)
            {
                case IngredientType.Pot:
                    targetPosition = _pot.position + potOffset;
                    break;
                case IngredientType.DoughBowl:
                    targetPosition = _plate.position + plateOffset;
                    break;
                default:
                    targetPosition = targetRectTransform.position +
                                  new Vector3(-targetRectTransform.rect.width * 1.5f,
                                      targetRectTransform.rect.height / 2 + 50, 0);
                    break;
            }
        }

        SpriteAnimator animator = null;

        switch (ingredientType)
        {
            case IngredientType.Salt:
                targetRotation = new Vector3(0, 0, -60f);
                animator = _saltAnimator;
                break;
            case IngredientType.Milk:
                targetRotation = new Vector3(0, 0, -90f);
                animator = _milkAnimator;
                break;
            case IngredientType.Sugar:
                targetRotation = new Vector3(0, 0, -45f);
                animator = _sugarAnimator;
                break;
            case IngredientType.Flour:
                targetRotation = new Vector3(0, 0, -45f);
                animator = _flourAnimator;
                break;
            case IngredientType.Yest:
                targetRotation = new Vector3(0, 0, -45f);
                animator = _yeastAnimator;
                break;
            case IngredientType.Oil:
                targetRotation = new Vector3(0, 0, -90f);
                animator = _oilAnimator;
                break;
            case IngredientType.Water:
                targetRotation = new Vector3(0, 0, -90f);
                animator = _waterAnimator;
                break;
            case IngredientType.Egg:
                targetRotation = new Vector3(0, 0, -90f);
                break;
            case IngredientType.Kumis:
                targetRotation = new Vector3(0, 0, 90f);
                animator = _kumisAnimator;
                break;
            default:
                OnIngredientAdded?.Invoke(targetType, ingredientType);
                onComplete?.Invoke();
                return;
        }

        StartCoroutine(AnimateIngredient(rectTransform, targetType, ingredientType, targetPosition, 
            targetRotation, duration, onComplete, animator));
    }

    private IEnumerator AnimateIngredient(RectTransform rectTransform, IngredientType targetType,
        IngredientType ingredientType, Vector3 targetPosition, Vector3 targetRotation,
        float duration, Action onComplete, SpriteAnimator animator)
    {
        _lockedObjects.Add(rectTransform);

        var initialPosition = rectTransform.position;
        var initialRotation = rectTransform.rotation;
        var targetQuaternion = Quaternion.Euler(targetRotation);
        float time = 0;

        if (!rectTransform.gameObject.name.Contains("Kumis"))
        {
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                rectTransform.position = Vector3.Lerp(initialPosition, targetPosition, time / (duration / 2));
                yield return null;
            }
        }
        
        bool isAdded = false;
        time = 0;

        while (time < duration / 2)
        {
            time += Time.deltaTime;
            rectTransform.rotation = Quaternion.Lerp(initialRotation, targetQuaternion, time / (duration / 2));

            if (!isAdded)
            {
                OnIngredientAdded?.Invoke(targetType, ingredientType);
                isAdded = true;
            }

            yield return null;
        }

        rectTransform.position = targetPosition;
        animator?.StartAnimation();
        yield return new WaitForSeconds(1);
        rectTransform.rotation = Quaternion.identity;
        if (rectTransform.gameObject.name.Contains("Kumis"))
        {
            _kumis.SetLidOn();
        }
        animator?.StopAnimation();
        _lockedObjects.Remove(rectTransform);
        onComplete?.Invoke();
    }

    [Serializable]
    private class ObjectState
    {
        public Transform OriginalParent { get; set; }
        public Vector2 OriginalPosition { get; set; }
        public IngredientType IngredientType { get; set; }
    }
}