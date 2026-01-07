using UnityEngine;
using UnityEngine.EventSystems;

public class HotkeyObserver : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int requiredTaps = 5; // Количество необходимых касаний
    [SerializeField] private float timeWindow = 1f; // Время в секундах для всех касаний
    [SerializeField] private ActivateableObject[] activateables;

    private int tapCount = 0;
    private float lastTapTime = 0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        float currentTime = Time.time;

        // Проверяем, не прошло ли слишком много времени с последнего касания
        if (currentTime - lastTapTime > timeWindow)
        {
            // Слишком долго - сбрасываем счётчик
            tapCount = 1;
        }
        else
        {
            // Увеличиваем счётчик
            tapCount++;

            // Проверяем, достигли ли нужного количества
            if (tapCount >= requiredTaps)
            {
                OnHotkeyActivated();
                tapCount = 0; // Сбрасываем после активации
            }
        }

        lastTapTime = currentTime;
    }

    private void OnHotkeyActivated()
    {
        Debug.Log("Hotkey активирован! Было " + requiredTaps + " быстрых касаний.");
        foreach (ActivateableObject ao in activateables)
            ao.Call();
    }
}