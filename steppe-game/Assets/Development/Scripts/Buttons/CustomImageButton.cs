using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomImageButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private Image image;

    public event Action OnButtonPressed;
    public event Action OnButtonReleased;
    public event Action OnButtonClicked;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (!image)
            image = GetComponent<Image>();
    }
#endif

    private void Awake()
    {
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnReleased();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked();
    }

    protected virtual void OnPressed()
    {
        OnButtonPressed?.Invoke();
    }

    protected virtual void OnReleased()
    {
        OnButtonReleased?.Invoke();
    }

    protected virtual void Clicked()
    {
        OnButtonClicked?.Invoke();
    }
}