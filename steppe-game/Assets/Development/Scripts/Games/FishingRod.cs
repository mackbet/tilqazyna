using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FishingRod : MonoBehaviour, IPointerDownHandler
{
    public bool IsEnabled { get; set; } = false;
    [SerializeField] private RectTransform hook;
    [SerializeField] private Image lineImage;
    [SerializeField] private float descendSpeed = 500f;
    [SerializeField] private float ascendSpeed = 500f;
    [SerializeField] private float hookRadius = 50f;
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip caughtSound;

    private RectTransform canvasRect;
    private Vector2 startPosition;
    private bool isActive = false;
    private Coroutine castCoroutine;
    private Camera mainCamera;

    public bool IsActive => isActive;
    public event Action<Fish> OnFishCaught;
    public event Action OnCast;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        mainCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActive && IsEnabled)
        {
            Vector2 clickPosition = GetClickPosition(eventData.position);
            CastToPosition(clickPosition);
        }
    }

    private Vector2 GetClickPosition(Vector2 screenPosition)
    {
        Vector2 localPoint;
        Canvas canvas = GetComponentInParent<Canvas>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out localPoint
        );

        return localPoint;
    }

    private Vector2 GetTopPosition(float xPosition)
    {
        return new Vector2(xPosition, canvasRect.sizeDelta.y / 2f + 100f);
    }

    public void CastToPosition(Vector2 targetPosition)
    {
        if (isActive) return;

        if (castCoroutine != null)
        {
            StopCoroutine(castCoroutine);
        }

        if (castSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(castSound);
        }

        OnCast?.Invoke();

        castCoroutine = StartCoroutine(CastCoroutine(targetPosition));
    }

    private IEnumerator CastCoroutine(Vector2 targetPosition)
    {
        isActive = true;

        startPosition = GetTopPosition(targetPosition.x);
        hook.anchoredPosition = startPosition;
        UpdateLine();

        Fish caughtFish = null;
        float targetY = targetPosition.y;

        while (hook.anchoredPosition.y > targetY && caughtFish == null)
        {
            Vector2 currentPos = hook.anchoredPosition;
            currentPos.y -= descendSpeed * Time.deltaTime;
            currentPos.y = Mathf.Max(currentPos.y, targetY);
            hook.anchoredPosition = currentPos;

            UpdateLine();
            caughtFish = CheckFishCollision();

            yield return null;
        }

        if (caughtFish != null)
        {
            AudioManager.Instance.PlaySound(caughtSound);
            OnFishCaught?.Invoke(caughtFish);
            caughtFish.Catch();
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        while (hook.anchoredPosition.y < startPosition.y)
        {
            Vector2 currentPos = hook.anchoredPosition;
            currentPos.y += ascendSpeed * Time.deltaTime;
            currentPos.y = Mathf.Min(currentPos.y, startPosition.y);
            hook.anchoredPosition = currentPos;

            UpdateLine();

            yield return null;
        }

        hook.anchoredPosition = startPosition;
        UpdateLine();

        isActive = false;
    }

    private Fish CheckFishCollision()
    {
        Fish[] allFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);

        foreach (Fish fish in allFish)
        {
            if (fish == null) continue;

            RectTransform fishRect = fish.GetComponent<RectTransform>();
            if (fishRect == null) continue;

            float distance = Vector2.Distance(hook.anchoredPosition, fishRect.anchoredPosition);

            if (distance < hookRadius)
            {
                return fish;
            }
        }

        return null;
    }

    private void UpdateLine()
    {
        if (lineImage == null) return;

        RectTransform lineRect = lineImage.GetComponent<RectTransform>();
        Vector2 topPosition = GetTopPosition(hook.anchoredPosition.x);

        float lineHeight = topPosition.y - hook.anchoredPosition.y;
        lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, lineHeight);

        Vector2 linePos = new Vector2(
            hook.anchoredPosition.x,
            topPosition.y - lineHeight / 2f
        );
        lineRect.anchoredPosition = linePos;
    }
}