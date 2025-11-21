using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomImageButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private Image image;

#if UNITY_EDITOR
    private void OnValidate()
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
        // Вызывается при нажатии
    }

    protected virtual void OnReleased()
    {
        // Вызывается при отпускании
    }

    protected virtual void Clicked()
    {
        // Вызывается при клике (после release)
    }
}