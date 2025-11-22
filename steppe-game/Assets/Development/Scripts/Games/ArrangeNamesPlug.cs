using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrangeNamesPlug : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector2 initialPosition;
    private Vector2 dragOffset;
    private bool isLocked = false;
    private ArrangeNamesSocket currentSocket = null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        if (!image)
            image = GetComponent<Image>();

        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    private void Start()
    {
        initialPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );

        dragOffset = localPointerPosition - rectTransform.anchoredPosition;

        canvasGroup.alpha = 0.6f;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint - dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.alpha = 1f;

        ArrangeNamesSocket targetSocket = FindSocketUnderPointer(eventData);

        if (targetSocket != null && targetSocket.CanAcceptPlug(this))
        {
            LockToSocket(targetSocket);
        }
        else
        {
            ReturnToInitialPosition();
        }
    }

    private ArrangeNamesSocket FindSocketUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            ArrangeNamesSocket socket = result.gameObject.GetComponent<ArrangeNamesSocket>();
            if (socket != null)
            {
                return socket;
            }
        }

        return null;
    }

    private void LockToSocket(ArrangeNamesSocket socket)
    {
        isLocked = true;
        currentSocket = socket;
        canvasGroup.blocksRaycasts = false;

        rectTransform.anchoredPosition = socket.GetComponent<RectTransform>().anchoredPosition;

        socket.AttachPlug(this);
    }

    private void ReturnToInitialPosition()
    {
        rectTransform.anchoredPosition = initialPosition;
    }

    public void Reset()
    {
        isLocked = false;
        currentSocket = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        ReturnToInitialPosition();
    }
}
