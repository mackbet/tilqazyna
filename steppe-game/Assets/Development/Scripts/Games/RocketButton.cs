using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RocketButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action<bool> OnStateChanged;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnStateChanged?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnStateChanged?.Invoke(false);
    }
}
