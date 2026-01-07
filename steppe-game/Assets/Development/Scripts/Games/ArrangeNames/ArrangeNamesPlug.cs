using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrangeNamesPlug : CustomImageButton, IDragHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private Transform initialParent;
    private int initialSiblingIndex;
    private Vector2 initialPivot;
    private Vector2 dragOffset;
    private bool isLocked = false;
    private bool isDragging = false;
    private ArrangeNamesSocket currentSocket = null;
    private PointerEventData lastPointerEventData;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    private void Start()
    {
        initialParent = transform.parent;
        initialSiblingIndex = transform.GetSiblingIndex();
        initialPivot = rectTransform.pivot;
        Debug.Log($"[{gameObject.name}] Start: initialParent = {initialParent.name}, siblingIndex = {initialSiblingIndex}, pivot = {initialPivot}");
    }

    protected override void OnPressed()
    {
        base.OnPressed();

        Debug.Log($"[{gameObject.name}] OnPressed: isLocked = {isLocked}");

        if (isLocked) return;

        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        lastPointerEventData = eventData;

        if (!isDragging)
        {
            isDragging = true;
            Debug.Log($"[{gameObject.name}] OnDrag: Started dragging, moving to canvas");

            // Сохраняем текущую позицию в мировых координатах
            Vector3 worldPosition = rectTransform.position;

            // Вытаскиваем кнопку из родителя в canvas
            transform.SetParent(canvas.transform);
            rectTransform.SetAsLastSibling();

            // Устанавливаем pivot в центр (0.5, 0.5)
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Восстанавливаем мировую позицию после смены pivot
            rectTransform.position = worldPosition;

            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition
            );

            dragOffset = localPointerPosition - rectTransform.anchoredPosition;
            Debug.Log($"[{gameObject.name}] OnDrag: dragOffset = {dragOffset}, pivot set to center");
        }

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint - dragOffset;
    }

    protected override void OnReleased()
    {
        base.OnReleased();

        Debug.Log($"[{gameObject.name}] OnReleased: isLocked = {isLocked}, isDragging = {isDragging}");

        if (isLocked) return;

        isDragging = false;
        dragOffset = Vector2.zero;

        ArrangeNamesSocket targetSocket = FindSocketUnderPointer(lastPointerEventData);

        Debug.Log($"[{gameObject.name}] OnReleased: targetSocket = {(targetSocket != null ? targetSocket.gameObject.name : "NULL")}");

        if (targetSocket != null)
        {
            bool canAccept = targetSocket.CanAcceptPlug(this);
            Debug.Log($"[{gameObject.name}] OnReleased: targetSocket.CanAcceptPlug = {canAccept}");

            if (canAccept)
            {
                Debug.Log($"[{gameObject.name}] OnReleased: Locking to socket");
                LockToSocket(targetSocket);
            }
            else
            {
                Debug.Log($"[{gameObject.name}] OnReleased: Socket cannot accept, returning to parent");
                ReturnToInitialParent();
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] OnReleased: No socket found, returning to parent");
            ReturnToInitialParent();
        }

        lastPointerEventData = null;
    }

    private ArrangeNamesSocket FindSocketUnderPointer(PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning($"[{gameObject.name}] FindSocketUnderPointer: EventSystem.current is NULL");
            return null;
        }

        if (eventData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] FindSocketUnderPointer: eventData is NULL");
            return null;
        }

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        Debug.Log($"[{gameObject.name}] FindSocketUnderPointer: Found {results.Count} raycast results at position {eventData.position}");

        foreach (var result in results)
        {
            Debug.Log($"[{gameObject.name}] FindSocketUnderPointer: Checking {result.gameObject.name}");
            ArrangeNamesSocket socket = result.gameObject.GetComponent<ArrangeNamesSocket>();
            if (socket != null)
            {
                Debug.Log($"[{gameObject.name}] FindSocketUnderPointer: Found socket on {result.gameObject.name}");
                return socket;
            }
        }

        return null;
    }

    private void LockToSocket(ArrangeNamesSocket socket)
    {
        Debug.Log($"[{gameObject.name}] LockToSocket: Locking to {socket.gameObject.name}");
        
        isLocked = true;
        currentSocket = socket;
        canvasGroup.blocksRaycasts = false;

        // Делаем plug дочерним объектом сокета
        transform.SetParent(socket.AttachPoint);
        rectTransform.anchoredPosition = Vector2.zero;

        socket.AttachPlug(this);
        
        Debug.Log($"[{gameObject.name}] LockToSocket: Complete");
    }

    private void ReturnToInitialParent()
    {
        Debug.Log($"[{gameObject.name}] ReturnToInitialParent: Returning to {initialParent.name}");
        
        // Восстанавливаем исходный pivot
        rectTransform.pivot = initialPivot;
        
        transform.SetParent(initialParent);
        transform.SetSiblingIndex(initialSiblingIndex);
        LayoutRebuilder.ForceRebuildLayoutImmediate(initialParent.GetComponent<RectTransform>());
        
        Debug.Log($"[{gameObject.name}] ReturnToInitialParent: Complete, pivot restored to {initialPivot}");
    }

    public void Reset()
    {
        Debug.Log($"[{gameObject.name}] Reset called");
        
        isLocked = false;
        currentSocket = null;
        canvasGroup.blocksRaycasts = true;
        isDragging = false;
        dragOffset = Vector2.zero;
        lastPointerEventData = null;
        ReturnToInitialParent();
    }
}